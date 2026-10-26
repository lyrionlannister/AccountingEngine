using System.Net;
using Accounting.Domain.Enums;
using Accounting.Persistence;
using Companies.Domain.Enums;
using Companies.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Users.Persistence;

namespace Accounting.ApiTests;

public sealed class AccountingApiTestEnvironment : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .WithCommand("redis-server", "--notify-keyspace-events", "Ex")
        .Build();

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string RedisConnectionString => _redis.GetConnectionString();

    public async Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("ACCOUNTING_APP_DB_PASSWORD", "test-app-role-password");

        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(PostgresConnectionString);
        dataSourceBuilder.MapEnum<CompanyMembershipRole>("rol_membresia");
        dataSourceBuilder.MapEnum<AccountNature>("naturaleza_cuenta");
        dataSourceBuilder.MapEnum<AccountType>("tipo_cuenta");
        dataSourceBuilder.MapEnum<PeriodStatus>("estado_periodo");
        dataSourceBuilder.MapEnum<JournalEntryStatus>("estado_asiento");
        var dataSource = dataSourceBuilder.Build();

        await using var companiesContext = new CompaniesDbContext(new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseNpgsql(dataSource, b => b.MapEnum<CompanyMembershipRole>("rol_membresia"))
            .Options);
        await companiesContext.Database.MigrateAsync();

        await using var usersContext = new UsersDbContext(
            new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(PostgresConnectionString).Options);
        await usersContext.Database.MigrateAsync();

        await using var authContext = new Auth.Persistence.AuthDbContext(
            new DbContextOptionsBuilder<Auth.Persistence.AuthDbContext>().UseNpgsql(PostgresConnectionString).Options);
        await authContext.Database.MigrateAsync();

        await using var accountingContext = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dataSource, b =>
            {
                b.MapEnum<AccountNature>("naturaleza_cuenta");
                b.MapEnum<AccountType>("tipo_cuenta");
                b.MapEnum<PeriodStatus>("estado_periodo");
                b.MapEnum<JournalEntryStatus>("estado_asiento");
            })
            .Options, new MigrationOnlyCompanyContext());
        await accountingContext.Database.MigrateAsync();
    }

    private sealed class MigrationOnlyCompanyContext : Shared.ICurrentCompanyContext
    {
        public long CompanyId => throw new InvalidOperationException("Not resolved; migration-only context.");
        public string Role => throw new InvalidOperationException("Not resolved; migration-only context.");
    }

    public async Task DisposeAsync() =>
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
}

[CollectionDefinition(Name)]
public sealed class AccountingApiTestCollection : ICollectionFixture<AccountingApiTestEnvironment>
{
    public const string Name = "Accounting Api test environment";
}

public sealed class AccountingApiFactory : WebApplicationFactory<Program>
{
    public const string TestRemoteIpHeader = "X-Test-RemoteIp";

    public AccountingApiFactory(AccountingApiTestEnvironment environment)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", environment.PostgresConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", environment.RedisConnectionString);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", Convert.ToBase64String(new byte[32]));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureServices(services =>
            services.AddTransient<IStartupFilter, FakeRemoteIpStartupFilter>());

    private sealed class FakeRemoteIpStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((context, nextMiddleware) =>
            {
                var testIp = context.Request.Headers[TestRemoteIpHeader].FirstOrDefault();
                context.Connection.RemoteIpAddress = testIp is not null ? IPAddress.Parse(testIp) : IPAddress.Loopback;
                return nextMiddleware();
            });
            next(app);
        };
    }
}
