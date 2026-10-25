using Auth.Domain;
using Auth.Persistence;
using Auth.Services;
using Microsoft.EntityFrameworkCore;

namespace Auth.Repositories;

// Respaldo: solo se usa directamente cuando Redis no responde (ver FailoverSessionStore,
// tasks/plan-auth.md Phase 2). Mismo contrato ISessionStore que el camino caliente.
public sealed class PostgresSessionFallbackStore : ISessionStore
{
    private static readonly TimeSpan SlidingWindow = TimeSpan.FromMinutes(30);

    private readonly AuthDbContext _context;

    public PostgresSessionFallbackStore(AuthDbContext context) => _context = context;

    public async Task<UserSession> CreateAsync(long userId, string ip, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var existing = await _context.Sessions.Where(s => s.UserId == userId).ToListAsync(cancellationToken);
        var active = existing.Where(s => !s.IsExpiredAt(now, SlidingWindow)).ToList();

        if (active.Count > 0 && active.Any(s => s.Ip != ip))
            throw new SessionConflictException(userId, ip);

        var session = UserSession.Create(userId, ip, now);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return session;
    }

    public Task<UserSession?> FindAsync(Guid sessionId, CancellationToken cancellationToken) =>
        _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);

    public async Task TouchAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
        if (session is null)
            return;

        session.Touch(DateTimeOffset.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
        if (session is null)
            return;

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
