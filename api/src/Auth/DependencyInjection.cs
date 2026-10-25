using Auth.Persistence;
using Auth.Repositories;
using Auth.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Caching;

namespace Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing connection string 'Default'.");

        services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));
        services.AddRedisConnection(configuration);

        services.AddScoped<RedisSessionStore>();
        services.AddScoped<PostgresSessionFallbackStore>();
        services.AddScoped<ISessionStore, FailoverSessionStore>();

        services.AddScoped<LoginService>();
        services.AddScoped<LogoutService>();

        // Requiere que el composition root (Api) registre ISessionExpiryNotifier antes de
        // construir el host — lo implementa vía SignalR (ver tasks/plan-auth.md, Phase 3).
        services.AddHostedService<RedisSessionExpiryListener>();

        return services;
    }
}
