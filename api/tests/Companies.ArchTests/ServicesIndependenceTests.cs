using System.Reflection;
using NetArchTest.Rules;

namespace Companies.ArchTests;

public class ServicesIndependenceTests
{
    private static readonly Assembly ModuleAssembly = Assembly.Load("Companies");

    [Fact]
    public void Services_Should_Not_Depend_On_Repositories_Or_Persistence()
    {
        var result = Types.InAssembly(ModuleAssembly)
            .That().ResideInNamespace("Companies.Services")
            .Should().NotHaveDependencyOnAny("Companies.Repositories", "Companies.Persistence", "Companies.Configurations")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }

    [Fact]
    public void Services_Should_Not_Depend_On_EntityFrameworkCore_Or_Npgsql_Directly()
    {
        var result = Types.InAssembly(ModuleAssembly)
            .That().ResideInNamespace("Companies.Services")
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }
}
