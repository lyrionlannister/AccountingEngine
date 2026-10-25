using Microsoft.AspNetCore.Mvc.Testing;

namespace Accounting.ApiTests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    public ApiTestFactory()
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Default",
            "Host=localhost;Database=accounting_engine_apitests;Username=postgres;Password=postgres");
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", "localhost:6380");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", Convert.ToBase64String(new byte[32]));
    }
}
