using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Pysar.Avalonia.Sample.ViewModels;

namespace Pysar.Avalonia.Sample.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        ExitCommand = new RelayCommand(Close);
        InitializeComponent();
    }

    public ICommand ExitCommand { get; }

    /// <summary>Surfaces a viewer failure in the same label the load path uses.</summary>
    private void OnRenderFailed(object? sender, Exception exception)
    {
        if (DataContext is ReportViewerViewModel viewModel)
            viewModel.ErrorMessage = exception.Message;

        Console.WriteLine($"Pysar: render failed - {exception}");
    }
}
