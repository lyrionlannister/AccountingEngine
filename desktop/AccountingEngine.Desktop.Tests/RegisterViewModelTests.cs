using AccountingEngine.Desktop.Shared.Http;
using AccountingEngine.Desktop.Tests.Fakes;
using AccountingEngine.Desktop.Users.ViewModels;

namespace AccountingEngine.Desktop.Tests;

public class RegisterViewModelTests
{
    private static RegisterViewModel CreateVm(FakeUsersService users, FakeAppNavigator navigator) =>
        new(users, navigator)
        {
            Email = "owner@acme.test",
            Password = "s3cret-password",
            TenantName = "Acme",
            CompanyName = "Acme MX",
            CountryCode = "MX",
        };

    [Fact]
    public async Task RegisterCommand_OnSuccess_NavigatesToLoginWithSuccessMessage()
    {
        var users = new FakeUsersService();
        var navigator = new FakeAppNavigator();
        var vm = CreateVm(users, navigator);

        await vm.RegisterCommand.ExecuteAsync(null);

        Assert.Equal(1, navigator.ShowLoginCallCount);
        Assert.NotNull(navigator.LoginSuccessMessage);
        Assert.Null(vm.ErrorMessage);
        Assert.Equal("owner@acme.test", users.LastRequest?.Email);
    }

    [Fact]
    public async Task RegisterCommand_OnDuplicateEmail_SetsErrorMessageAndDoesNotNavigate()
    {
        var users = new FakeUsersService { ExceptionToThrow = new ApiException(409, "Ya existe una cuenta registrada con ese correo.") };
        var navigator = new FakeAppNavigator();
        var vm = CreateVm(users, navigator);

        await vm.RegisterCommand.ExecuteAsync(null);

        Assert.Equal("Ya existe una cuenta registrada con ese correo.", vm.ErrorMessage);
        Assert.Equal(0, navigator.ShowLoginCallCount);
    }

    [Fact]
    public void GoToLoginCommand_NavigatesToLogin()
    {
        var navigator = new FakeAppNavigator();
        var vm = CreateVm(new FakeUsersService(), navigator);

        vm.GoToLoginCommand.Execute(null);

        Assert.Equal(1, navigator.ShowLoginCallCount);
        Assert.Null(navigator.LoginSuccessMessage);
    }
}
