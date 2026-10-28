using System.Reflection;
using NetArchTest.Rules;

namespace Companies.ArchTests;

public static class ModuleBoundaryRules
{
    public static TestResult AssertDoesNotDependOnInternalsOf(
        Assembly consumerAssembly,
        string otherModuleInternalNamespacePrefix)
    {
        return Types.InAssembly(consumerAssembly)
            .Should().NotHaveDependencyOnAny(otherModuleInternalNamespacePrefix)
            .GetResult();
    }
}
