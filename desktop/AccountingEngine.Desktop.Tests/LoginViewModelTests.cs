using AccountingEngine.Desktop.Auth.ViewModels;
using AccountingEngine.Desktop.Shared.Http;
using AccountingEngine.Desktop.Tests.Fakes;

namespace AccountingEngine.Desktop.Tests;

public class LoginViewModelTests
{
    [Fact]
    public async Task LoginCommand_OnSuccess_NavigatesToShell()
    {
        var auth = new FakeAuthService();
        var navigator = new FakeAppNavigator();
        var vm = new LoginViewModel(auth, navigator) { Email = "owner@acme.test", Password = "s3cret" };

        await vm.LoginCommand.ExecuteAsync(null);

        Assert.Equal(1, auth.LoginCallCount);
        Assert.Equal(1, navigator.ShowShellCallCount);
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public async Task LoginCommand_OnApiException_SetsErrorMessageAndDoesNotNavigate()
    {
        var auth = new FakeAuthService { ExceptionToThrow = new ApiException(401, "Credenciales inválidas") };
        var navigator = new FakeAppNavigator();
        var vm = new LoginViewModel(auth, navigator) { Email = "owner@acme.test", Password = "wrong" };

        await vm.LoginCommand.ExecuteAsync(null);

        Assert.Equal("Credenciales inválidas", vm.ErrorMessage);
        Assert.Equal(0, navigator.ShowShellCallCount);
    }

    [Fact]
    public void GoToRegisterCommand_NavigatesToRegister()
    {
        var navigator = new FakeAppNavigator();
        var vm = new LoginViewModel(new FakeAuthService(), navigator);

        vm.GoToRegisterCommand.Execute(null);

        Assert.Equal(1, navigator.ShowRegisterCallCount);
    }
}
