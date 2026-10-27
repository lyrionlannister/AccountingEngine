namespace AccountingEngine.Desktop.Shell.Services;

public interface ISidebarState
{
    bool IsExpanded { get; }

    void Toggle();
}
