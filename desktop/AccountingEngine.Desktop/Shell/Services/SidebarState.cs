using CommunityToolkit.Mvvm.ComponentModel;

namespace AccountingEngine.Desktop.Shell.Services;

public partial class SidebarState : ObservableObject, ISidebarState
{
    [ObservableProperty]
    public partial bool IsExpanded { get; set; } = true;

    public void Toggle() => IsExpanded = !IsExpanded;
}
