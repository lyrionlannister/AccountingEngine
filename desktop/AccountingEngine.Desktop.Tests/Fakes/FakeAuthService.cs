using AccountingEngine.Desktop.Auth.Models;
using AccountingEngine.Desktop.Auth.Services;
using AccountingEngine.Desktop.Shared.Http;

namespace AccountingEngine.Desktop.Tests.Fakes;

public sealed class FakeAuthService : IAuthService
{
    public Session? CurrentSession { get; private set; }
    public event EventHandler? SessionChanged;

    public Session? SessionToReturn { get; set; }
    public ApiException? ExceptionToThrow { get; set; }
    public int LoginCallCount { get; private set; }
    public int LogoutCallCount { get; private set; }

    public Task LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        LoginCallCount++;
        if (ExceptionToThrow is not null) throw ExceptionToThrow;

        CurrentSession = SessionToReturn ?? new Session("token", []);
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task LogoutAsync(CancellationToken cancellationToken)
    {
        LogoutCallCount++;
        CurrentSession = null;
        SessionChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
