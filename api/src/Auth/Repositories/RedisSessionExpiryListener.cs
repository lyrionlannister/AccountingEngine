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
    private readonly IConnectionMultiplexer _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RedisSessionExpiryListener> _logger;

    public RedisSessionExpiryListener(
        IConnectionMultiplexer connection, IServiceScopeFactory scopeFactory, ILogger<RedisSessionExpiryListener> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _connection.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal("__keyevent@0__:expired"), (channel, value) =>
            HandleExpiredKeyAsync(value.ToString(), stoppingToken));

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Apagado normal del host.
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
