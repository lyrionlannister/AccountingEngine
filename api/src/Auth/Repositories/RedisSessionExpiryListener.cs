using Auth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Auth.Repositories;

public sealed class RedisSessionExpiryListener : BackgroundService
{
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
