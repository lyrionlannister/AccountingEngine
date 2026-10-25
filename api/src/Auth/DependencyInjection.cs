using Auth.Persistence;
using Auth.Repositories;
using Auth.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing connection string 'Default'.");

        services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));

        // Registro temporal directo al respaldo de Postgres — se reemplaza por
        // FailoverSessionStore (Redis primario + este respaldo) en Phase 2 del plan.
        services.AddScoped<ISessionStore, PostgresSessionFallbackStore>();

        return services;
    }
}
