using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AccountingEngine.Desktop.Auth.Models;
using AccountingEngine.Desktop.Shared.Http;

namespace AccountingEngine.Desktop.Auth.Services;

public sealed class AuthService : IAuthService
{
    private readonly HttpClient _http;

    public Session? CurrentSession { get; private set; }
    public event EventHandler? SessionChanged;

    public AuthService(HttpClient http) => _http = http;

    public async Task LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new { email, password }, JsonDefaults.Options, cancellationToken);
        await response.EnsureApiSuccessAsync(cancellationToken);

        var session = await response.Content.ReadFromJsonAsync<Session>(JsonDefaults.Options, cancellationToken)
            ?? throw new ApiException((int)response.StatusCode, "Respuesta vacía del servidor.");

        CurrentSession = session;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        if (CurrentSession is { } session)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "auth/logout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

            try
            {
                var response = await _http.SendAsync(request, cancellationToken);
                await response.EnsureApiSuccessAsync(cancellationToken);
            }
            catch (ApiException)
            {
                // la sesión local se limpia de todas formas
            }
        }

        CurrentSession = null;
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }
}
