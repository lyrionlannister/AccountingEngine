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
            var session = await _primary.FindAsync(userId, sessionId, cancellationToken);
            if (session is not null)
                return session;

            // Miss limpio en Redis (no excepción): puede ser que la sesión nunca haya existido,
            // o que se haya creado/tocado durante una caída anterior de Redis y solo viva en el
            // respaldo. Sin este chequeo, un usuario pierde su sesión en cuanto Redis vuelve a
            // responder aunque la sesión siga vigente en Postgres.
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
            // La revocación nunca es best-effort: una fila "revocada" que queda viva en el
            // respaldo puede resucitar una sesión cerrada si Redis cae después (ver
            // code-reviewer/security-auditor). Create/Touch sí siguen best-effort — FindAsync
            // ahora consulta el respaldo en cualquier miss limpio de Redis, así que un mirror
            // de creación perdido ya no implica perder la sesión mientras Redis siga arriba.
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
            // El respaldo puede quedar unos segundos desincronizado — aceptable para
            // Create/Touch: FindAsync ya cae al respaldo en cualquier miss limpio de Redis.
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
                // Se agotaron los reintentos: la sesión ya está revocada en Redis (la fuente de
                // verdad mientras responda), pero el respaldo puede seguir marcándola activa.
                // Error, no Warning: si Redis cae después, esa fila revocada puede aceptar un
                // JWT que el usuario ya cerró.
                _logger.LogError(ex, "Failed to mirror session revocation to Postgres after {MaxAttempts} attempts; the fallback row may remain stale.", maxAttempts);
            }
        }
    }

    private void LogRedisDown(RedisException ex) =>
        _logger.LogError(ex, "Redis unreachable, failing over to the Postgres session store.");
}
