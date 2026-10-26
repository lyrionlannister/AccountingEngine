using Auth.Domain;
using Auth.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Auth.Repositories;

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
            var session = await _primary.FindAsync(userId, sessionId, cancellationToken);
            if (session is not null)
                return session;

            return await _fallback.FindAsync(userId, sessionId, cancellationToken);
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
            await MirrorWithRetryAsync(() => _fallback.RevokeAsync(userId, sessionId, cancellationToken));
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
            _logger.LogWarning(ex, "Failed to mirror session write to the Postgres fallback store.");
        }
    }

    private async Task MirrorWithRetryAsync(Func<Task> mirror)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await mirror();
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "Failed to mirror session revocation to Postgres (attempt {Attempt}/{MaxAttempts}).", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to mirror session revocation to Postgres after {MaxAttempts} attempts; the fallback row may remain stale.", maxAttempts);
            }
        }
    }

    private void LogRedisDown(RedisException ex) =>
        _logger.LogError(ex, "Redis unreachable, failing over to the Postgres session store.");
}
