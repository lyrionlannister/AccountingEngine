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

        services.AddHostedService<RedisSessionExpiryListener>();

        return services;
    }
}
