using Companies.Domain.Enums;
using Companies.Persistence;
using Companies.Repositories;
using Companies.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shared;

namespace Companies;

public static class DependencyInjection
{
    public static IServiceCollection AddCompaniesModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing connection string 'Default'.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<CompanyMembershipRole>("rol_membresia");
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<CompaniesDbContext>(options => options.UseNpgsql(dataSource, npgsqlOptions =>
        {
            npgsqlOptions.MapEnum<CompanyMembershipRole>("rol_membresia");
        }));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IPlatformAdministratorRepository, PlatformAdministratorRepository>();

        services.AddScoped<CurrentCompanyContext>();
        services.AddScoped<ICurrentCompanyContext>(sp => sp.GetRequiredService<CurrentCompanyContext>());

        services.AddScoped<RegisterUserService>();
        services.AddScoped<CreateTenantWithFirstCompanyService>();
        services.AddScoped<AddMembershipService>();
        services.AddScoped<GrantPlatformAdministratorService>();

        return services;
    }
}
