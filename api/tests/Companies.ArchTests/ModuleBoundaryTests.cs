using System.Reflection;

namespace Companies.ArchTests;

public class ModuleBoundaryTests
{
    private static readonly Assembly CompaniesAssembly = Assembly.Load("Companies");

    [Theory]
    [InlineData("Users.Repositories")]
    [InlineData("Users.Persistence")]
    [InlineData("Users.Configurations")]
    public void Companies_Should_Not_Depend_On_Users_Internals(string usersInternalNamespacePrefix)
    {
        var result = ModuleBoundaryRules.AssertDoesNotDependOnInternalsOf(CompaniesAssembly, usersInternalNamespacePrefix);

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }
}
