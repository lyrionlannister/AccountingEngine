using System.Reflection;
using NetArchTest.Rules;

namespace Companies.ArchTests;

public class DomainIndependenceTests
{
    private static readonly Assembly DomainAssembly = Assembly.Load("Companies.Domain");

    [Fact]
    public void Domain_Should_Not_Depend_On_EntityFrameworkCore()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should().NotHaveDependencyOnAny("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_AspNetCore()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should().NotHaveDependencyOnAny("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Npgsql()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should().NotHaveDependencyOnAny("Npgsql")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Shared()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should().NotHaveDependencyOnAny("Shared")
            .GetResult();

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }
}
