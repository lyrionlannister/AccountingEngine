using Auth.Domain;
using Auth.Services;
using StackExchange.Redis;

namespace Auth.Repositories;

// Camino caliente: fuente de verdad mientras Redis responda (ver FailoverSessionStore y
// tasks/plan-auth.md, Phase 2). Esquema de keys privado de este archivo:
//   session:{userId}:{sessionId}   -> hash { ip, createdAt, lastSeenAt }, TTL deslizante
//   user_sessions:{userId}         -> hash { sessionId -> ip }, usado para el chequeo de 1 IP
// Ningún otro módulo (ni Auth.Services) conoce este esquema.
public sealed class RedisSessionStore : ISessionStore
{
    private static readonly TimeSpan SlidingWindow = TimeSpan.FromMinutes(30);

    private readonly IConnectionMultiplexer _connection;

    public RedisSessionStore(IConnectionMultiplexer connection) => _connection = connection;

    public async Task<UserSession> CreateAsync(long userId, string ip, CancellationToken cancellationToken)
    {
        var db = _connection.GetDatabase();
        var indexKey = UserSessionsIndexKey(userId);

        var activeIps = await ActiveIpsAsync(db, userId, indexKey);
        if (activeIps.Count > 0 && activeIps.Any(existingIp => existingIp != ip))
            throw new SessionConflictException(userId, ip);

        var now = DateTimeOffset.UtcNow;
        var session = UserSession.Create(userId, ip, now);

        await WriteSessionAsync(db, session);
        await db.HashSetAsync(indexKey, session.SessionId.ToString(), ip);

        return session;
    }

    public async Task<UserSession?> FindAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var db = _connection.GetDatabase();
        var entries = await db.HashGetAllAsync(SessionKey(userId, sessionId));

        return ToSession(userId, sessionId, entries);
    }

    public async Task TouchAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var db = _connection.GetDatabase();
        var sessionKey = SessionKey(userId, sessionId);

        var entries = await db.HashGetAllAsync(sessionKey);
        var session = ToSession(userId, sessionId, entries);
        if (session is null)
            return;

        session.Touch(DateTimeOffset.UtcNow);
        await WriteSessionAsync(db, session);
    }

    public async Task RevokeAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var db = _connection.GetDatabase();

        await db.KeyDeleteAsync(SessionKey(userId, sessionId));
        await db.HashDeleteAsync(UserSessionsIndexKey(userId), sessionId.ToString());
    }

    private static async Task WriteSessionAsync(IDatabase db, UserSession session)
    {
        var sessionKey = SessionKey(session.UserId, session.SessionId);

        await db.HashSetAsync(sessionKey, new HashEntry[]
        {
            new("ip", session.Ip),
            new("createdAt", session.CreatedAt.ToUnixTimeSeconds()),
            new("lastSeenAt", session.LastSeenAt.ToUnixTimeSeconds())
        });
        await db.KeyExpireAsync(sessionKey, SlidingWindow);
    }

    private static UserSession? ToSession(long userId, Guid sessionId, HashEntry[] entries)
    {
        if (entries.Length == 0)
            return null;

        var map = entries.ToDictionary(e => e.Name.ToString(), e => e.Value);
        var createdAt = DateTimeOffset.FromUnixTimeSeconds((long)map["createdAt"]);
        var lastSeenAt = DateTimeOffset.FromUnixTimeSeconds((long)map["lastSeenAt"]);

        return UserSession.Reconstitute(sessionId, userId, map["ip"].ToString()!, createdAt, lastSeenAt);
    }

    // ActiveIpsAsync descarta por lectura (no HEXPIRE, no disponible en StackExchange.Redis
    // 2.8) cualquier entrada del índice cuya key companion ya no exista — ver Architecture
    // Decisions en tasks/plan-auth.md.
    private static async Task<List<string>> ActiveIpsAsync(IDatabase db, long userId, RedisKey indexKey)
    {
        var entries = await db.HashGetAllAsync(indexKey);
        var ips = new List<string>();

        foreach (var entry in entries)
        {
            if (!Guid.TryParse(entry.Name.ToString(), out var sessionId))
                continue;

            if (await db.KeyExistsAsync(SessionKey(userId, sessionId)))
                ips.Add(entry.Value.ToString());
            else
                await db.HashDeleteAsync(indexKey, entry.Name);
        }

        return ips;
    }

    private static string SessionKey(long userId, Guid sessionId) => $"session:{userId}:{sessionId}";

    private static string UserSessionsIndexKey(long userId) => $"user_sessions:{userId}";
}
