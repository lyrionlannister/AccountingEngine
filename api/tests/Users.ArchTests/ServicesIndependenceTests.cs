using System.Reflection;
using NetArchTest.Rules;

namespace Users.ArchTests;

public class ServicesIndependenceTests
{
    private static readonly Assembly ModuleAssembly = Assembly.Load("Users");

    [Fact]
    public void Services_Should_Not_Depend_On_Repositories_Or_Persistence()
    {
        var result = Types.InAssembly(ModuleAssembly)
            .That().ResideInNamespace("Users.Services")
            .Should().NotHaveDependencyOnAny("Users.Repositories", "Users.Persistence", "Users.Configurations")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }

    [Fact]
    public void Services_Should_Not_Depend_On_EntityFrameworkCore_Or_Npgsql_Directly()
    {
        var result = Types.InAssembly(ModuleAssembly)
            .That().ResideInNamespace("Users.Services")
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Npgsql")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }
}
