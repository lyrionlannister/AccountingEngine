using System;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AccountingEngine.Desktop.Accounting.ViewModels;
using AccountingEngine.Desktop.Auth.Services;
using AccountingEngine.Desktop.Auth.ViewModels;
using AccountingEngine.Desktop.Shared;
using AccountingEngine.Desktop.Shell.Services;
using AccountingEngine.Desktop.Shell.ViewModels;
using AccountingEngine.Desktop.Users.Services;
using AccountingEngine.Desktop.Users.ViewModels;
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
            var root = Services.GetRequiredService<AppRootViewModel>();
            root.Start();
            desktop.MainWindow = new Views.MainWindow { DataContext = root };
        }

        base.OnFrameworkInitializationCompleted();
    }

    // La lógica de cada módulo vive en Services inyectados (ver <Modulo>/Services), nunca en el code-behind de la View.
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // apunta al nginx del compose (HTTP_PORT), no al contenedor api directo: api no publica puerto propio.
        var apiBaseUrl = Environment.GetEnvironmentVariable("ACCOUNTINGENGINE_API_URL") ?? "http://localhost:8080";
        services.AddSingleton(_ => new HttpClient { BaseAddress = new Uri($"{apiBaseUrl}/api/") });
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IUsersService, UsersService>();

        services.AddSingleton<ISidebarState, SidebarState>();
        services.AddSingleton<ShellViewModel>();
        services.AddTransient<PanelViewModel>();
        services.AddTransient<PolizasViewModel>();
        services.AddTransient<ComponentesViewModel>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddSingleton<AppRootViewModel>();
        services.AddSingleton<IAppNavigator>(sp => sp.GetRequiredService<AppRootViewModel>());

        return services.BuildServiceProvider();
    }
}
