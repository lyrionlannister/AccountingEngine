using System;
using System.Threading;
using System.Threading.Tasks;
using AccountingEngine.Desktop.Accounting.ViewModels;
using AccountingEngine.Desktop.Auth.Services;
using AccountingEngine.Desktop.Shared;
using AccountingEngine.Desktop.Shell.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingEngine.Desktop.Shell.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly IAuthService _auth;
    private readonly IAppNavigator _navigator;

    public ISidebarState Sidebar { get; }

    [ObservableProperty]
    public partial string SelectedKey { get; set; } = "panel";

    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    public ShellViewModel(ISidebarState sidebar, IServiceProvider services, IAuthService auth, IAppNavigator navigator)
    {
        Sidebar = sidebar;
        _services = services;
        _auth = auth;
        _navigator = navigator;
        CurrentPage = services.GetRequiredService<PanelViewModel>();
    }

    [RelayCommand]
    private void ToggleSidebar() => Sidebar.Toggle();

    [RelayCommand]
    private async Task Logout()
    {
        await _auth.LogoutAsync(CancellationToken.None);
        _navigator.ShowLogin();
    }

    [RelayCommand]
    private void Navigate(string key)
    {
        SelectedKey = key;
        CurrentPage = key switch
        {
            "panel" => _services.GetRequiredService<PanelViewModel>(),
            "polizas" => _services.GetRequiredService<PolizasViewModel>(),
            _ => CurrentPage,
        };
    }
}
