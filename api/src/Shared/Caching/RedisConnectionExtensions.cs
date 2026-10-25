using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Shared.Caching;

// Fábrica de conexión cruda a Redis, compartida entre módulos (hoy Auth, luego Reportes —
// ver tasks/plan-auth.md). Shared nunca usa la conexión, solo la entrega: cada módulo conoce
// su propio esquema de keys y lo mantiene privado en su Repositories/.
public static class RedisConnectionExtensions
{
    public static IServiceCollection AddRedisConnection(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Missing connection string 'Redis'.");

        // TryAdd: si más de un módulo llama esto, la conexión se crea una sola vez.
        services.TryAddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));

        return services;
    }
}
