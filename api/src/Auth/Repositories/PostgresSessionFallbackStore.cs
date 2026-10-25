using Auth.Domain;
using Auth.Persistence;
using Auth.Services;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories;

// Respaldo: solo se usa directamente cuando Redis no responde (ver FailoverSessionStore,
// tasks/plan-auth.md Phase 2). Mismo contrato ISessionStore que el camino caliente.
public sealed class PostgresSessionFallbackStore : ISessionStore
{
    private static readonly TimeSpan SlidingWindow = UserSession.SlidingWindow;

    private readonly AuthDbContext _context;

    public PostgresSessionFallbackStore(AuthDbContext context) => _context = context;

    public async Task<UserSession> CreateAsync(long userId, string ip, CancellationToken cancellationToken)
    {
        // pg_advisory_xact_lock serializa por usuario: sin esto, dos creaciones concurrentes
        // podían leer ambas "sin conflicto" antes de que cualquiera insertara (mismo problema
        // que RedisSessionStore resuelve con un script Lua atómico).
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({userId})", cancellationToken);

        var now = DateTimeOffset.UtcNow;

        var existing = await _context.Sessions.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
        var active = existing.Where(s => !s.IsExpiredAt(now, SlidingWindow)).ToList();

        if (active.Count > 0 && active.Any(s => s.Ip != ip))
            throw new SessionConflictException(userId, ip);

        var session = UserSession.Create(userId, ip, now);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return session;
    }

    // Escribe la sesión exacta que Redis ya aceptó (mismo SessionId) — nunca re-valida la
    // regla de 1 IP, eso ya lo decidió el camino caliente. Llamado best-effort por
    // FailoverSessionStore; si la fila ya existe (reintento), actualiza LastSeenAt.
    public async Task CreateMirrorAsync(UserSession session, CancellationToken cancellationToken)
    {
        var existing = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == session.SessionId, cancellationToken);
        if (existing is null)
            _context.Sessions.Add(session);
        else
            existing.Touch(session.LastSeenAt);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserSession?> FindAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId, cancellationToken);

        // Redis expira la sesión vía TTL; este respaldo no tiene un mecanismo equivalente, así
        // que el TTL deslizante hay que aplicarlo explícitamente en la lectura — de lo
        // contrario una sesión vencida queda "válida" para siempre mientras Redis no responda.
        if (session is not null && session.IsExpiredAt(DateTimeOffset.UtcNow, SlidingWindow))
            return null;

        return session;
    }

    public async Task TouchAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await FindAsync(userId, sessionId, cancellationToken);
        if (session is null)
            return;

        session.Touch(DateTimeOffset.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await FindAsync(userId, sessionId, cancellationToken);
        if (session is null)
            return;

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
