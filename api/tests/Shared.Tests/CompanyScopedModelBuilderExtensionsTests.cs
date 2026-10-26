using Microsoft.EntityFrameworkCore;
using Shared;

namespace Shared.Tests;

public sealed class ScopedThing
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class ExcludedThing
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class UnscopedThing
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class FakeCurrentCompanyContext(long companyId) : ICurrentCompanyContext
{
    public long CompanyId => companyId;
    public string Role => "Admin";
}

public sealed class TestDbContext(DbContextOptions<TestDbContext> options, ICurrentCompanyContext currentCompany)
    : DbContext(options)
{
    public long CurrentCompanyId => currentCompany.CompanyId;

    public DbSet<ScopedThing> ScopedThings => Set<ScopedThing>();
    public DbSet<ExcludedThing> ExcludedThings => Set<ExcludedThing>();
    public DbSet<UnscopedThing> UnscopedThings => Set<UnscopedThing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyCompanyScopeFilters(this, ctx => ctx.CurrentCompanyId, typeof(ExcludedThing));
    }
}

public class CompanyScopedModelBuilderExtensionsTests
{
    private static TestDbContext CreateContext(string dbName, long companyId)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new TestDbContext(options, new FakeCurrentCompanyContext(companyId));
    }

    [Fact]
    public void QueryFilter_AppliesOnlyToEntitiesWithCompanyId()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var seedContext = CreateContext(dbName, companyId: 1))
        {
            seedContext.ScopedThings.Add(new ScopedThing { Id = 1, CompanyId = 1, Name = "mine" });
            seedContext.ScopedThings.Add(new ScopedThing { Id = 2, CompanyId = 2, Name = "theirs" });
            seedContext.UnscopedThings.Add(new UnscopedThing { Id = 1, Name = "global" });
            seedContext.SaveChanges();
        }

        using var context = CreateContext(dbName, companyId: 1);

        Assert.Equal(["mine"], context.ScopedThings.Select(e => e.Name).ToList());
        Assert.Equal(["global"], context.UnscopedThings.Select(e => e.Name).ToList());
    }

    [Fact]
    public void QueryFilter_SkipsExcludedTypes()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var seedContext = CreateContext(dbName, companyId: 1))
        {
            seedContext.ExcludedThings.Add(new ExcludedThing { Id = 1, CompanyId = 1, Name = "mine" });
            seedContext.ExcludedThings.Add(new ExcludedThing { Id = 2, CompanyId = 2, Name = "theirs" });
            seedContext.SaveChanges();
        }

        using var context = CreateContext(dbName, companyId: 1);

        Assert.Equal(2, context.ExcludedThings.Count());
    }

    [Fact]
    public void ApplyCompanyIdOnAdd_FillsDefaultCompanyIdOnAddedEntities()
    {
        using var context = CreateContext(Guid.NewGuid().ToString(), companyId: 7);
        var entity = new ScopedThing { Id = 1, Name = "no company set" };
        context.ScopedThings.Add(entity);

        context.ChangeTracker.ApplyCompanyIdOnAdd(new FakeCurrentCompanyContext(7));

        Assert.Equal(7, entity.CompanyId);
    }

    [Fact]
    public void QueryFilter_UsesEachInstancesOwnCompanyId_DespiteSharedCompiledModel()
    {
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(dbName, companyId: 1))
        {
            seedContext.ScopedThings.Add(new ScopedThing { Id = 1, CompanyId = 1, Name = "company-1" });
            seedContext.ScopedThings.Add(new ScopedThing { Id = 2, CompanyId = 2, Name = "company-2" });
            seedContext.SaveChanges();
        }

        using var warmUpContext = CreateContext(dbName, companyId: 1);
        _ = warmUpContext.ScopedThings.ToList();

        using var contextForCompanyTwo = CreateContext(dbName, companyId: 2);

        Assert.Equal(["company-2"], contextForCompanyTwo.ScopedThings.Select(e => e.Name).ToList());
    }
}
