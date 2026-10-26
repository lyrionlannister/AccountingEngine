using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Shared.Caching;

public static class RedisConnectionExtensions
{
    public static IServiceCollection AddRedisConnection(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Missing connection string 'Redis'.");

        var options = ConfigurationOptions.Parse(connectionString);
        options.AbortOnConnectFail = false;

        services.TryAddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));

        return services;
    }
}
