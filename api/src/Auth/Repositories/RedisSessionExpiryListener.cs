using Auth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Auth.Repositories;

// Escucha __keyevent@0__:expired de Redis y notifica vía ISessionExpiryNotifier (lo implementa
// Api con SignalR — ver tasks/plan-auth.md, Phase 3). Requiere "notify-keyspace-events Ex" en
// el servidor (ya configurado en compose.yaml). Ignora cualquier key expirada sin el prefijo
// "session:" — prepara el terreno para que Reportes (Fase 5) use la misma instancia de Redis.
public sealed class RedisSessionExpiryListener : BackgroundService
{
    // Cota de handlers concurrentes: el callback de SubscribeAsync no espera a que termine
    // cada HandleExpiredKeyAsync, así que un evento de expiración masiva (p. ej. un restart de
    // Redis que expira miles de keys casi simultáneas) podía abrir un scope de DI y una
    // notificación por cada una sin ningún límite.
    private const int MaxConcurrentNotifications = 50;

    private readonly IConnectionMultiplexer _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RedisSessionExpiryListener> _logger;
    private readonly SemaphoreSlim _concurrencyLimiter = new(MaxConcurrentNotifications);

    public RedisSessionExpiryListener(
        IConnectionMultiplexer connection, IServiceScopeFactory scopeFactory, ILogger<RedisSessionExpiryListener> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Un BackgroundService sin manejar sus propias excepciones tira todo el host abajo
        // (.NET 8+, BackgroundServiceExceptionBehavior.StopHost por defecto) — si Redis está
        // caído al arrancar o se cae después, SubscribeAsync lanza y esto anulaba el mismo
        // failover que FailoverSessionStore ya implementa para el resto del módulo. Reintenta
        // con backoff en vez de dejar que la excepción escape de ExecuteAsync.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var subscriber = _connection.GetSubscriber();
                await subscriber.SubscribeAsync(RedisChannel.Literal("__keyevent@0__:expired"), (channel, value) =>
                    HandleExpiredKeyBoundedAsync(value.ToString(), stoppingToken));

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Apagado normal del host.
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(ex, "No se pudo suscribir a la expiración de sesiones en Redis; reintentando.");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
    }

    private async Task HandleExpiredKeyBoundedAsync(string key, CancellationToken cancellationToken)
    {
        await _concurrencyLimiter.WaitAsync(cancellationToken);
        try
        {
            await HandleExpiredKeyAsync(key, cancellationToken);
        }
        finally
        {
            _concurrencyLimiter.Release();
        }
    }

    private async Task HandleExpiredKeyAsync(string key, CancellationToken cancellationToken)
    {
        if (!TryParseSessionKey(key, out var userId, out var sessionId))
            return;

        using var scope = _scopeFactory.CreateScope();
        var notifier = scope.ServiceProvider.GetRequiredService<ISessionExpiryNotifier>();

        try
        {
            await notifier.NotifyExpiredAsync(userId, sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify session expiry for key {Key}.", key);
        }
    }

    internal static bool TryParseSessionKey(string key, out long userId, out Guid sessionId)
    {
        userId = default;
        sessionId = default;

        if (!key.StartsWith("session:", StringComparison.Ordinal))
            return false;

        var parts = key.Split(':');
        return parts.Length == 3
            && long.TryParse(parts[1], out userId)
            && Guid.TryParse(parts[2], out sessionId);
    }
}
