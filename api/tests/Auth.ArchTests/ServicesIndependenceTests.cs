using System.Reflection;
using NetArchTest.Rules;

namespace Auth.ArchTests;

public class ServicesIndependenceTests
{
    private static readonly Assembly ModuleAssembly = Assembly.Load("Auth");

    [Fact]
    public void Services_Should_Not_Depend_On_Repositories_Or_Persistence()
    {
        var result = Types.InAssembly(ModuleAssembly)
            .That().ResideInNamespace("Auth.Services")
            .Should().NotHaveDependencyOnAny("Auth.Repositories", "Auth.Persistence", "Auth.Configurations")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }

    [Fact]
    public void Services_Should_Not_Depend_On_EntityFrameworkCore_Or_Npgsql_Or_Redis_Directly()
    {
        var result = Types.InAssembly(ModuleAssembly)
            .That().ResideInNamespace("Auth.Services")
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql", "StackExchange.Redis")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }
}
