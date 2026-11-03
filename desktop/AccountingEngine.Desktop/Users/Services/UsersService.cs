using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AccountingEngine.Desktop.Shared.Http;
using AccountingEngine.Desktop.Users.Models;

namespace AccountingEngine.Desktop.Users.Services;

public sealed class UsersService : IUsersService
{
    private readonly HttpClient _http;

    public UsersService(HttpClient http) => _http = http;

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _http.PostAsJsonAsync("users/register", request, JsonDefaults.Options, cancellationToken);
        await response.EnsureApiSuccessAsync(cancellationToken);

        return await response.Content.ReadFromJsonAsync<RegisterResult>(JsonDefaults.Options, cancellationToken)
            ?? throw new ApiException((int)response.StatusCode, "Respuesta vacía del servidor.");
    }
}
