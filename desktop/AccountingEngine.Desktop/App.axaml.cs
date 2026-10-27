using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AccountingEngine.Desktop.Accounting.ViewModels;
using AccountingEngine.Desktop.Shell.Services;
using AccountingEngine.Desktop.Shell.ViewModels;
using AccountingEngine.Desktop.Shell.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingEngine.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var shell = Services.GetRequiredService<ShellViewModel>();
            desktop.MainWindow = new Views.MainWindow
            {
                Content = new ShellView { DataContext = shell },
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    // La lógica de cada módulo vive en Services inyectados (ver <Modulo>/Services), nunca en el code-behind de la View.
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ISidebarState, SidebarState>();
        services.AddSingleton<ShellViewModel>();
        services.AddTransient<PanelViewModel>();
        services.AddTransient<PolizasViewModel>();
        services.AddTransient<ComponentesViewModel>();

        return services.BuildServiceProvider();
    }
}
