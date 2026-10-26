using Companies.Domain.Enums;
using Companies.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Users.Domain;
using Users.Persistence;

namespace Users.IntegrationTests;

[Collection(PostgresCollection.Name)]
public sealed class UserOwnershipTransferTests : IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private string _postgres = null!;
    private NpgsqlDataSource _dataSource = null!;

    public UserOwnershipTransferTests(PostgresContainerFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        _postgres = await _fixture.CreateDatabaseAsync();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_postgres);
        dataSourceBuilder.MapEnum<CompanyMembershipRole>("rol_membresia");
        _dataSource = dataSourceBuilder.Build();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task OwnershipTransfer_PreservesPreExistingRows()
    {
        var companiesOptions = new DbContextOptionsBuilder<CompaniesDbContext>()
            .UseNpgsql(_dataSource, b => b.MapEnum<CompanyMembershipRole>("rol_membresia"))
            .Options;

        await using (var pristineContext = new CompaniesDbContext(companiesOptions))
        {
            await pristineContext.Database.MigrateAsync("20261024093015_InitialCompaniesSchema");
        }

        await using (var connection = new NpgsqlConnection(_postgres))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText =
                "INSERT INTO usuarios (correo, contrasena_hash, activo) VALUES ('legacy@acme.test', 'legacy-hash', true);";
            await command.ExecuteNonQueryAsync();
        }

        await using (var companiesContext = new CompaniesDbContext(companiesOptions))
        {
            await companiesContext.Database.MigrateAsync();
        }

        var usersOptions = new DbContextOptionsBuilder<UsersDbContext>().UseNpgsql(_postgres).Options;
        await using var usersContext = new UsersDbContext(usersOptions);
        await usersContext.Database.MigrateAsync();

        var user = await usersContext.Users.FirstOrDefaultAsync(u => u.Email == "legacy@acme.test");
        Assert.NotNull(user);
        Assert.Equal("legacy-hash", user!.PasswordHash);
        Assert.True(user.IsActive);

        usersContext.Users.Add(User.Create("LEGACY@ACME.TEST", "another-hash"));
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => usersContext.SaveChangesAsync());
    }
}
