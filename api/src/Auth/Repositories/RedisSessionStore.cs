using Auth.Domain;
using Auth.Services;
using StackExchange.Redis;

namespace Auth.Repositories;

public sealed class RedisSessionStore : ISessionStore
{
    private static readonly TimeSpan SlidingWindow = UserSession.SlidingWindow;

    private const string CreateSessionScript = @"
local idxKey = KEYS[1]
local sessionKey = KEYS[2]
local userId = ARGV[1]
local ip = ARGV[2]
local ttlSeconds = ARGV[3]
local epochSeconds = ARGV[4]
local sessionId = ARGV[5]

local entries = redis.call('HGETALL', idxKey)
for i = 1, #entries, 2 do
    local sid = entries[i]
    local existingIp = entries[i + 1]
    local candidateKey = 'session:' .. userId .. ':' .. sid
    if redis.call('EXISTS', candidateKey) == 1 then
        if existingIp ~= ip then
            return { 'conflict', existingIp }
        end
    else
        redis.call('HDEL', idxKey, sid)
    end
end

redis.call('HSET', sessionKey, 'ip', ip, 'createdAt', epochSeconds, 'lastSeenAt', epochSeconds)
redis.call('EXPIRE', sessionKey, ttlSeconds)
redis.call('HSET', idxKey, sessionId, ip)
return { 'ok' }
";

    private readonly IConnectionMultiplexer _connection;

    public RedisSessionStore(IConnectionMultiplexer connection) => _connection = connection;

    public async Task<UserSession> CreateAsync(long userId, string ip, CancellationToken cancellationToken)
    {
        var db = _connection.GetDatabase();
        var now = DateTimeOffset.UtcNow;
        var session = UserSession.Create(userId, ip, now);

        var result = (RedisResult[])await db.ScriptEvaluateAsync(
            CreateSessionScript,
            keys: [UserSessionsIndexKey(userId), SessionKey(userId, session.SessionId)],
            values:
            [
                userId,
                ip,
                (long)SlidingWindow.TotalSeconds,
                now.ToUnixTimeSeconds(),
                session.SessionId.ToString()
            ]);

        if ((string)result[0]! == "conflict")
            throw new SessionConflictException(userId, ip);

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

    private static string SessionKey(long userId, Guid sessionId) => $"session:{userId}:{sessionId}";

    private static string UserSessionsIndexKey(long userId) => $"user_sessions:{userId}";
}
