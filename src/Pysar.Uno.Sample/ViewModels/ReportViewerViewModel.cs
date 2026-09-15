using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Pysar.Elements;
using Pysar.Uno;
using Pysar.Viewer.Zoom;

namespace Pysar.Uno.Sample.ViewModels;

/// <summary>
///     Drives the report panel and the viewer's toolbar. Ported from the Avalonia sample's view
///     model of the same name; PDF export stays on the page because the picker must run in the
///     originating click on WebAssembly.
/// </summary>
public sealed partial class ReportViewerViewModel : ObservableObject
{
    private static readonly double[] ZoomSteps =
        [0.25, 0.33, 0.5, 0.67, 0.75, 0.8, 0.9, 1, 1.1, 1.25, 1.5, 1.75, 2, 2.5, 3, 4, 5];

    private readonly UnoReportPrinter _printer;
    private bool _isBusy;

    [ObservableProperty]
    private ReportDescriptor _selectedReport = ReportDescriptor.All[0];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PrintCommand))]
    private Report? _report;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FitButtonGlyph))]
    [NotifyPropertyChangedFor(nameof(FitButtonTitle))]
    private ReportZoomMode _zoomMode = ReportZoomMode.FitWidth;

    [ObservableProperty]
    private double _zoom = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ZoomText))]
    private double _effectiveZoom = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageIndicatorText))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageIndicatorText))]
    private int _pageCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    [NotifyPropertyChangedFor(nameof(ErrorVisibility))]
    private string? _errorMessage;

    public ReportViewerViewModel(UnoReportPrinter? printer = null)
    {
        _printer = printer ?? new UnoReportPrinter(PysarUno.Renderer);
        IsPrintSupported = !OperatingSystem.IsBrowser()
            && !OperatingSystem.IsAndroid()
            && !OperatingSystem.IsIOS();
        LoadReport();
    }

    public IReadOnlyList<ReportDescriptor> Reports { get; } = ReportDescriptor.All;

    public bool IsPrintSupported { get; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public Visibility ErrorVisibility => HasError ? Visibility.Visible : Visibility.Collapsed;

    public string ZoomText => $"{Math.Round(EffectiveZoom * 100)}%";

    public string FitButtonGlyph => ZoomMode == ReportZoomMode.FitWidth ? "\uf065" : "\uf337";

    public string FitButtonTitle => ZoomMode == ReportZoomMode.FitWidth ? "Fit page" : "Fit width";

    public string PageIndicatorText => $"{CurrentPage} of {PageCount}";

    public bool IsBusy => _isBusy;

    partial void OnSelectedReportChanged(ReportDescriptor value) => LoadReport();

    private void LoadReport()
    {
        try
        {
            var report = SelectedReport.Create();
            report.Build();
            Report = report;
            CurrentPage = 1;
            ErrorMessage = null;
        }
        catch (Exception exception)
        {
            Report = null;
            ErrorMessage = exception.ToString();
        }
    }

    [RelayCommand]
    private void ZoomIn()
    {
        var next = ZoomSteps.FirstOrDefault(step => step > EffectiveZoom + 0.001);
        if (next > 0)
            SetZoom(next);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        var previous = ZoomSteps.LastOrDefault(step => step < EffectiveZoom - 0.001);
        if (previous > 0)
            SetZoom(previous);
    }

    [RelayCommand]
    private void ActualSize() => SetZoom(1);

    [RelayCommand]
    private void ToggleFit()
        => ZoomMode = ZoomMode == ReportZoomMode.FitWidth ? ReportZoomMode.FitPage : ReportZoomMode.FitWidth;

    [RelayCommand(CanExecute = nameof(CanPrint))]
    private async Task PrintAsync()
    {
        if (Report is null || _isBusy || !IsPrintSupported)
            return;

        SetBusy(true);
        ErrorMessage = null;

        try
        {
            await _printer.PrintAsync(Report);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.ToString();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool CanPrint() => !_isBusy && Report is not null && IsPrintSupported;

    private void SetZoom(double zoom)
    {
        Zoom = zoom;
        ZoomMode = ReportZoomMode.Custom;
    }

    private void SetBusy(bool value)
    {
        if (_isBusy == value)
            return;

        _isBusy = value;
        OnPropertyChanged(nameof(IsBusy));
        PrintCommand.NotifyCanExecuteChanged();
    }
}
