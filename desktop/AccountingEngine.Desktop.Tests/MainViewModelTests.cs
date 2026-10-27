using AccountingEngine.Desktop.ViewModels;

namespace AccountingEngine.Desktop.Tests;

public class MainViewModelTests
{
    [Fact]
    public void Greeting_HasDefaultValue()
    {
        var viewModel = new MainViewModel();

        Assert.Equal("Welcome to Avalonia!", viewModel.Greeting);
    }
}
