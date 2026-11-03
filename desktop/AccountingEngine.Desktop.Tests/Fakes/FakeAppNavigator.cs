using AccountingEngine.Desktop.Shared;

namespace AccountingEngine.Desktop.Tests.Fakes;

public sealed class FakeAppNavigator : IAppNavigator
{
    public string? LoginSuccessMessage { get; private set; }
    public int ShowLoginCallCount { get; private set; }
    public int ShowRegisterCallCount { get; private set; }
    public int ShowShellCallCount { get; private set; }

    public void ShowLogin(string? successMessage = null)
    {
        ShowLoginCallCount++;
        LoginSuccessMessage = successMessage;
    }

    public void ShowRegister() => ShowRegisterCallCount++;

    public void ShowShell() => ShowShellCallCount++;
}
