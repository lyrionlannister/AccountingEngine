using System.Reflection;
using NetArchTest.Rules;

namespace Accounting.ArchTests;

public class ApiBoundaryTests
{
    private static readonly Assembly ApiAssembly = Assembly.Load("Api");

    [Fact]
    public void Api_Should_Not_Depend_On_Persistence_Repositories_Or_Configurations_Directly()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace("Api")
            .Should().NotHaveDependencyOnAny("Accounting.Persistence", "Accounting.Repositories", "Accounting.Configurations")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }

    [Fact]
    public void Api_Should_Not_Depend_On_EntityFrameworkCore_Query_Types_Directly()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace("Api")
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }
}
