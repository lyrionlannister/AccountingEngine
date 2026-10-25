using Auth.Domain;
using Auth.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Auth.Repositories;

// ISessionStore real que registra DependencyInjection.cs: Redis es la fuente de verdad del
// camino caliente; Postgres solo entra si Redis no responde (nunca un doble chequeo en cada
// operación exitosa). Ver tasks/plan-auth.md, Architecture Decisions.
public sealed class FailoverSessionStore : ISessionStore
{
    private readonly RedisSessionStore _primary;
    private readonly PostgresSessionFallbackStore _fallback;
    private readonly ILogger<FailoverSessionStore> _logger;

    public FailoverSessionStore(RedisSessionStore primary, PostgresSessionFallbackStore fallback, ILogger<FailoverSessionStore> logger)
    {
        _primary = primary;
        _fallback = fallback;
        _logger = logger;
    }

    public async Task<UserSession> CreateAsync(long userId, string ip, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _primary.CreateAsync(userId, ip, cancellationToken);
            await MirrorBestEffortAsync(() => _fallback.CreateMirrorAsync(session, cancellationToken));
            return session;
        }
        catch (RedisException ex)
        {
            LogRedisDown(ex);
            return await _fallback.CreateAsync(userId, ip, cancellationToken);
        }
    }

    public async Task<UserSession?> FindAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            return await _primary.FindAsync(userId, sessionId, cancellationToken);
        }
        catch (RedisException ex)
        {
            LogRedisDown(ex);
            return await _fallback.FindAsync(userId, sessionId, cancellationToken);
        }
    }

    public async Task TouchAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            await _primary.TouchAsync(userId, sessionId, cancellationToken);
            await MirrorBestEffortAsync(() => _fallback.TouchAsync(userId, sessionId, cancellationToken));
        }
        catch (RedisException ex)
        {
            LogRedisDown(ex);
            await _fallback.TouchAsync(userId, sessionId, cancellationToken);
        }
    }

    public async Task RevokeAsync(long userId, Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            await _primary.RevokeAsync(userId, sessionId, cancellationToken);
            await MirrorBestEffortAsync(() => _fallback.RevokeAsync(userId, sessionId, cancellationToken));
        }
        catch (RedisException ex)
        {
            LogRedisDown(ex);
            await _fallback.RevokeAsync(userId, sessionId, cancellationToken);
        }
    }

    private async Task MirrorBestEffortAsync(Func<Task> mirror)
    {
        try
        {
            await mirror();
        }
        catch (Exception ex)
        {
            // El respaldo puede quedar unos segundos desincronizado — aceptable, solo se lee
            // cuando Redis ya no responde (ver tasks/plan-auth.md).
            _logger.LogWarning(ex, "Failed to mirror session write to the Postgres fallback store.");
        }
    }

    private void LogRedisDown(RedisException ex) =>
        _logger.LogError(ex, "Redis unreachable, failing over to the Postgres session store.");
}
