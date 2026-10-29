using System;
using System.Threading;
using System.Threading.Tasks;
using AccountingEngine.Desktop.Auth.Models;

namespace AccountingEngine.Desktop.Auth.Services;

public interface IAuthService
{
    Session? CurrentSession { get; }
    event EventHandler? SessionChanged;
    Task LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task LogoutAsync(CancellationToken cancellationToken);
}
