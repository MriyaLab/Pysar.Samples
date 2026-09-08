using System.ComponentModel;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Pysar.Avalonia;
using Pysar.Avalonia.Sample.ViewModels;
using Pysar.Export;
using Pysar.Skia;

namespace Pysar.Avalonia.Sample.Views;

public partial class MainWindow : Window
{
    /// <summary>How long the page overlay stays up after the page changes, as in the MAUI sample.</summary>
    private static readonly TimeSpan PageIndicatorLifetime = TimeSpan.FromSeconds(3);

    // The sample has no container, so the export service is built from the renderer directly;
    // this is what AddPysar does for the hosts that do have one.
    private readonly IReportExportService _exporter = SkiaReportExport.CreateExportService(PysarAvalonia.Renderer);

    private readonly DispatcherTimer _pageIndicatorTimer;

    private ReportViewerViewModel? _viewModel;

    public MainWindow()
    {
        ExitCommand = new RelayCommand(Close);
        ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);

        InitializeComponent();

        _pageIndicatorTimer = new DispatcherTimer { Interval = PageIndicatorLifetime };
        _pageIndicatorTimer.Tick += OnPageIndicatorElapsed;

        DataContextChanged += OnDataContextChanged;
    }

    public ICommand ExitCommand { get; }

    public ICommand ExportPdfCommand { get; }

    /// <summary>Surfaces a viewer failure in the same label the load path uses.</summary>
    private void OnRenderFailed(object? sender, Exception exception)
    {
        if (DataContext is ReportViewerViewModel viewModel)
            viewModel.ErrorMessage = exception.Message;

        Console.WriteLine($"Pysar: render failed - {exception}");
    }

    /// <summary>
    ///     Keeps the drawer state on the window itself: the styles read the class, so opening and
    ///     closing needs no property of its own.
    /// </summary>
    private void OnNavToggleClick(object? sender, RoutedEventArgs e)
        => Classes.Set("nav-open", !Classes.Contains("nav-open"));

    private void OnNavScrimPressed(object? sender, PointerPressedEventArgs e)
        => Classes.Set("nav-open", false);

    /// <summary>Picking a report is what the drawer is for, so it closes behind the choice.</summary>
    private void OnReportSelected(object? sender, SelectionChangedEventArgs e)
        => Classes.Set("nav-open", false);

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = DataContext as ReportViewerViewModel;

        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ReportViewerViewModel.CurrentPage)
            or nameof(ReportViewerViewModel.PageCount))
        {
            ShowPageIndicator();
        }
    }

    private void ShowPageIndicator()
    {
        if (_viewModel is null || _viewModel.PageCount < 1)
            return;

        PageIndicator.IsVisible = true;

        _pageIndicatorTimer.Stop();
        _pageIndicatorTimer.Start();
    }

    private void OnPageIndicatorElapsed(object? sender, EventArgs e)
    {
        _pageIndicatorTimer.Stop();
        PageIndicator.IsVisible = false;
    }

    private async Task ExportPdfAsync()
    {
        if (_viewModel?.Report is null)
            return;

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export report",
            SuggestedFileName = _viewModel.SelectedReport.FileName,
            DefaultExtension = "pdf",
            FileTypeChoices = [FilePickerFileTypes.Pdf]
        });

        if (file is null)
            return;

        try
        {
            var bytes = await _exporter.ExportAsync(_viewModel.Report, ExportFormat.Pdf);

            await using var stream = await file.OpenWriteAsync();
            await stream.WriteAsync(bytes);

            _viewModel.ErrorMessage = null;
        }
        catch (Exception exception)
        {
            _viewModel.ErrorMessage = exception.Message;

            Console.WriteLine($"Pysar: export failed - {exception}");
        }
    }
}
