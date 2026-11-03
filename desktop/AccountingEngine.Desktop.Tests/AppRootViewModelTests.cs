using AccountingEngine.Desktop.Accounting.ViewModels;
using AccountingEngine.Desktop.Auth.Models;
using AccountingEngine.Desktop.Auth.Services;
using AccountingEngine.Desktop.Auth.ViewModels;
using AccountingEngine.Desktop.Shared;
using AccountingEngine.Desktop.Shell.Services;
using AccountingEngine.Desktop.Shell.ViewModels;
using AccountingEngine.Desktop.Tests.Fakes;
using AccountingEngine.Desktop.Users.Services;
using AccountingEngine.Desktop.Users.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingEngine.Desktop.Tests;

public class AppRootViewModelTests
{
    private static (AppRootViewModel Root, FakeAuthService Auth) CreateRoot(bool authenticated = false)
    {
        var auth = new FakeAuthService();
        var services = new ServiceCollection();

        services.AddSingleton<IAuthService>(auth);
        services.AddSingleton<IUsersService, FakeUsersService>();
        services.AddSingleton<ISidebarState, SidebarState>();
        services.AddTransient<PanelViewModel>();
        services.AddTransient<PolizasViewModel>();
        services.AddTransient<ComponentesViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<AppRootViewModel>();
        services.AddSingleton<IAppNavigator>(sp => sp.GetRequiredService<AppRootViewModel>());

        var provider = services.BuildServiceProvider();

        if (authenticated)
        {
            auth.LoginAsync("owner@acme.test", "s3cret", CancellationToken.None).GetAwaiter().GetResult();
        }

        var root = provider.GetRequiredService<AppRootViewModel>();
        root.Start();

        return (root, auth);
    }

    [Fact]
    public void WithoutSession_StartsOnLogin()
    {
        var (root, _) = CreateRoot(authenticated: false);

        Assert.IsType<LoginViewModel>(root.CurrentContent);
    }

    [Fact]
    public void WithExistingSession_StartsOnShell()
    {
        var (root, _) = CreateRoot(authenticated: true);

        Assert.IsType<ShellViewModel>(root.CurrentContent);
    }

    [Fact]
    public void ShowRegister_SwitchesToRegister()
    {
        var (root, _) = CreateRoot();

        root.ShowRegister();

        Assert.IsType<RegisterViewModel>(root.CurrentContent);
    }

    [Fact]
    public void ShowLogin_CarriesSuccessMessageIntoNewLoginViewModel()
    {
        var (root, _) = CreateRoot();

        root.ShowLogin("Tu cuenta quedó registrada. Inicia sesión para continuar.");

        var login = Assert.IsType<LoginViewModel>(root.CurrentContent);
        Assert.Equal("Tu cuenta quedó registrada. Inicia sesión para continuar.", login.SuccessMessage);
    }

    [Fact]
    public void ShowShell_SwitchesToShell()
    {
        var (root, _) = CreateRoot();

        root.ShowShell();

        Assert.IsType<ShellViewModel>(root.CurrentContent);
    }
}
