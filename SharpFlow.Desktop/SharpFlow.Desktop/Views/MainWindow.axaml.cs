using Avalonia.Controls;
using Avalonia.Input;
using SharpFlow.Desktop.ViewModels;
using SharpFlow.Desktop.Models;

namespace SharpFlow.Desktop.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Node_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is WorkflowNode node)
        {
            ViewModel.SelectNode(node);
        }
    }
}