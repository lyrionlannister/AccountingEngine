using System;
using AccountingEngine.Desktop.Accounting.ViewModels;
using AccountingEngine.Desktop.Shared;
using AccountingEngine.Desktop.Shell.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingEngine.Desktop.Shell.ViewModels;

public partial class ShellViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;

    public ISidebarState Sidebar { get; }

    [ObservableProperty]
    public partial string SelectedKey { get; set; } = "panel";

    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    public ShellViewModel(ISidebarState sidebar, IServiceProvider services)
    {
        Sidebar = sidebar;
        _services = services;
        CurrentPage = services.GetRequiredService<PanelViewModel>();
    }

    [RelayCommand]
    private void ToggleSidebar() => Sidebar.Toggle();

    [RelayCommand]
    private void Navigate(string key)
    {
        SelectedKey = key;
        CurrentPage = key switch
        {
            "panel" => _services.GetRequiredService<PanelViewModel>(),
            "polizas" => _services.GetRequiredService<PolizasViewModel>(),
            "componentes" => _services.GetRequiredService<ComponentesViewModel>(),
            _ => CurrentPage,
        };
    }
}
