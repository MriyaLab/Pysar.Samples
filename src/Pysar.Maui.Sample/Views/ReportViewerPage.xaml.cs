using Pysar.Maui.Sample.ViewModels;

namespace Pysar.Maui.Sample.Views;

public partial class ReportViewerPage : ContentPage
{
    private readonly ReportViewerViewModel _viewModel;

    public ReportViewerPage(ReportViewerViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private void OnOpenFlyoutClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }

    /// <summary>Surfaces a viewer failure in the same label the export uses.</summary>
    private void OnRenderFailed(object? sender, Exception exception)
    {
        _viewModel.ErrorMessage = exception.Message;

        Console.WriteLine($"Pysar: render failed - {exception}");
    }
}
