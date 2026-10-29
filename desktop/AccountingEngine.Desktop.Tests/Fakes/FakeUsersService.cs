using AccountingEngine.Desktop.Shared.Http;
using AccountingEngine.Desktop.Users.Models;
using AccountingEngine.Desktop.Users.Services;

namespace AccountingEngine.Desktop.Tests.Fakes;

public sealed class FakeUsersService : IUsersService
{
    public RegisterResult ResultToReturn { get; set; } = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
    public ApiException? ExceptionToThrow { get; set; }
    public RegisterRequest? LastRequest { get; private set; }

    public Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
        return Task.FromResult(ResultToReturn);
    }
}
