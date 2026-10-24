using System.Reflection;

namespace Accounting.ArchTests;

public class ModuleBoundaryTests
{
    private static readonly Assembly AccountingAssembly = Assembly.Load("Accounting");

    [Theory]
    [InlineData("Companies.Repositories")]
    [InlineData("Companies.Persistence")]
    [InlineData("Companies.Configurations")]
    public void Accounting_Should_Not_Depend_On_Companies_Internals(string companiesInternalNamespacePrefix)
    {
        var result = ModuleBoundaryRules.AssertDoesNotDependOnInternalsOf(AccountingAssembly, companiesInternalNamespacePrefix);

        Assert.True(result.IsSuccessful, ArchTestResult.Describe(result));
    }
}
