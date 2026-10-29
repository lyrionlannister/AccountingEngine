using System;
using AccountingEngine.Desktop.Auth.Services;
using AccountingEngine.Desktop.Auth.ViewModels;
using AccountingEngine.Desktop.Shell.ViewModels;
using AccountingEngine.Desktop.Users.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace AccountingEngine.Desktop.Shared;

public partial class AppRootViewModel : ViewModelBase, IAppNavigator
{
    private readonly IServiceProvider _services;
    private readonly IAuthService _auth;

    [ObservableProperty]
    public partial ViewModelBase? CurrentContent { get; set; }

    public AppRootViewModel(IServiceProvider services, IAuthService auth)
    {
        _services = services;
        _auth = auth;
    }

    // Resuelve el contenido inicial fuera del constructor: LoginViewModel/ShellViewModel dependen de
    // IAppNavigator, cuyo factory vuelve a pedir AppRootViewModel al contenedor. Si eso ocurriera dentro
    // del propio constructor, el singleton todavía no estaría cacheado y el contenedor lo reconstruiría
    // en bucle (stack overflow). Por eso Start() se llama una vez el singleton ya quedó resuelto.
    public void Start()
    {
        CurrentContent = _auth.CurrentSession is not null
            ? _services.GetRequiredService<ShellViewModel>()
            : _services.GetRequiredService<LoginViewModel>();
    }

    public void ShowLogin(string? successMessage = null)
    {
        var login = _services.GetRequiredService<LoginViewModel>();
        login.SuccessMessage = successMessage;
        CurrentContent = login;
    }

    public void ShowRegister() => CurrentContent = _services.GetRequiredService<RegisterViewModel>();

    public void ShowShell() => CurrentContent = _services.GetRequiredService<ShellViewModel>();
}
