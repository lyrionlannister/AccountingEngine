using System.Threading;
using System.Threading.Tasks;
using AccountingEngine.Desktop.Users.Models;

namespace AccountingEngine.Desktop.Users.Services;

public interface IUsersService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
}
