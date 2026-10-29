namespace AccountingEngine.Desktop.Shared;

public interface IAppNavigator
{
    void ShowLogin(string? successMessage = null);
    void ShowRegister();
    void ShowShell();
}
