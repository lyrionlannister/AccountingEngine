using System.Threading;
using System.Threading.Tasks;
using AccountingEngine.Desktop.Shared;
using AccountingEngine.Desktop.Shared.Http;
using AccountingEngine.Desktop.Users.Models;
using AccountingEngine.Desktop.Users.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AccountingEngine.Desktop.Users.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly IUsersService _users;
    private readonly IAppNavigator _navigator;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TenantName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CompanyName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CountryCode { get; set; } = "MX";

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public RegisterViewModel(IUsersService users, IAppNavigator navigator)
    {
        _users = users;
        _navigator = navigator;
    }

    [RelayCommand]
    private async Task Register()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var request = new RegisterRequest(Email, Password, TenantName, CompanyName, CountryCode);
            await _users.RegisterAsync(request, CancellationToken.None);
            _navigator.ShowLogin("Tu cuenta quedó registrada. Inicia sesión para continuar.");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void GoToLogin() => _navigator.ShowLogin();
}
