using NetArchTest.Rules;

namespace Accounting.ArchTests;

internal static class ArchTestResult
{
    public static string Describe(TestResult result) =>
        result.FailingTypes is null
            ? string.Empty
            : string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
