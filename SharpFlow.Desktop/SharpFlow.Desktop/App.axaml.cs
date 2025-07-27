using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SharpFlow.Desktop.ViewModels;
using SharpFlow.Desktop.Views;

namespace SharpFlow.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindowViewModel mainViewModel = new MainWindowViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };

            if (this.DataContext is MainWindowViewModel viewModel)
            {
                // If the App's DataContext is already set, use it
                desktop.MainWindow.DataContext = viewModel;
            }
            else
            {
                // Otherwise, set the DataContext to a new instance of MainWindowViewModel
                desktop.MainWindow.DataContext = mainViewModel;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}