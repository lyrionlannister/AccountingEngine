using AccountingEngine.Desktop.Accounting.ViewModels;
using AccountingEngine.Desktop.Shell.Services;
using AccountingEngine.Desktop.Shell.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingEngine.Desktop.Tests;

public class ShellViewModelTests
{
    private static ShellViewModel CreateShell()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISidebarState, SidebarState>();
        services.AddTransient<PanelViewModel>();
        services.AddTransient<PolizasViewModel>();
        services.AddTransient<ComponentesViewModel>();
        var provider = services.BuildServiceProvider();

        return new ShellViewModel(provider.GetRequiredService<ISidebarState>(), provider);
    }

    [Fact]
    public void SelectedKey_DefaultsToPanel()
    {
        var shell = CreateShell();

        Assert.Equal("panel", shell.SelectedKey);
        Assert.IsType<PanelViewModel>(shell.CurrentPage);
    }

    [Fact]
    public void NavigateCommand_SwitchesCurrentPage()
    {
        var shell = CreateShell();

        shell.NavigateCommand.Execute("polizas");

        Assert.Equal("polizas", shell.SelectedKey);
        Assert.IsType<PolizasViewModel>(shell.CurrentPage);
    }

    [Fact]
    public void ToggleSidebarCommand_FlipsExpandedState()
    {
        var shell = CreateShell();
        var wasExpanded = shell.Sidebar.IsExpanded;

        shell.ToggleSidebarCommand.Execute(null);

        Assert.Equal(!wasExpanded, shell.Sidebar.IsExpanded);
    }
}
