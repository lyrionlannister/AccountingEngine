using System.Threading;
using System.Threading.Tasks;
using AccountingEngine.Desktop.Auth.Services;
using AccountingEngine.Desktop.Shared;
using AccountingEngine.Desktop.Shared.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AccountingEngine.Desktop.Auth.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly IAppNavigator _navigator;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? SuccessMessage { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public LoginViewModel(IAuthService auth, IAppNavigator navigator)
    {
        _auth = auth;
        _navigator = navigator;
    }

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            await _auth.LoginAsync(Email, Password, CancellationToken.None);
            _navigator.ShowShell();
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
    private void GoToRegister() => _navigator.ShowRegister();
}
