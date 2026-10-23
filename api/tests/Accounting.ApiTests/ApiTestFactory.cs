using Microsoft.AspNetCore.Mvc.Testing;

namespace Accounting.ApiTests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    public ApiTestFactory()
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Default",
            "Host=localhost;Database=accounting_engine_apitests;Username=postgres;Password=postgres");
    }
}
