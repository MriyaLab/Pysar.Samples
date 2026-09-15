using System.ComponentModel;
using Pysar.Export;
using Pysar.Uno.Sample.ViewModels;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;

namespace Pysar.Uno.Sample;

public sealed partial class MainPage : Page
{
    private bool _isNarrow;
    private DispatcherTimer? _pageIndicatorTimer;

    public MainPage()
    {
        ViewModel = new ReportViewerViewModel();
        InitializeComponent();

        _pageIndicatorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _pageIndicatorTimer.Tick += OnPageIndicatorTick;

        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        Viewer.RenderFailed += (_, exception) => ViewModel.ErrorMessage = exception.ToString();

        Loaded += OnMainPageLoaded;
    }

    public ReportViewerViewModel ViewModel { get; }

    private void OnMainPageLoaded(object sender, RoutedEventArgs e)
    {
        ApplySafeArea();
        SyncPaneForWidth(ActualWidth);
        SizeChanged += OnMainPageSizeChanged;
        ApplicationView.GetForCurrentView().VisibleBoundsChanged += (_, _) => ApplySafeArea();
    }

    private void OnMainPageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplySafeArea();
        SyncPaneForWidth(e.NewSize.Width);
    }

    private void SyncPaneForWidth(double width)
    {
        var narrow = width > 0 && width < 641;
        if (narrow == _isNarrow)
            return;

        _isNarrow = narrow;
        Shell.IsPaneOpen = !narrow;
    }

    private void ApplySafeArea()
    {
        RootLayout.Margin = GetSafeAreaMargin();
    }

    private Thickness GetSafeAreaMargin()
    {
#if __IOS__
        double top = 0, left = 0, right = 0, bottom = 0;
        foreach (var scene in UIKit.UIApplication.SharedApplication.ConnectedScenes.OfType<UIKit.UIWindowScene>())
        {
            foreach (var window in scene.Windows)
            {
                var windowInsets = window.SafeAreaInsets;
                var viewInsets = window.RootViewController?.View?.SafeAreaInsets ?? default;
                top = Math.Max(top, Math.Max(windowInsets.Top, viewInsets.Top));
                left = Math.Max(left, Math.Max(windowInsets.Left, viewInsets.Left));
                right = Math.Max(right, Math.Max(windowInsets.Right, viewInsets.Right));
                bottom = Math.Max(bottom, Math.Max(windowInsets.Bottom, viewInsets.Bottom));
            }
        }

        // Uno's Skia UIWindow reports 0 insets; Dynamic Island / notch is ~59pt.
        if (top < 1)
            top = 59;

        return new Thickness(left, top, right, bottom);
#else
        if (XamlRoot is null)
            return new Thickness(0);

        var visible = ApplicationView.GetForCurrentView().VisibleBounds;
        var width = XamlRoot.Size.Width;
        var height = XamlRoot.Size.Height;
        if (width <= 0 || height <= 0)
            return new Thickness(0);

        return new Thickness(
            Math.Max(0, visible.X),
            Math.Max(0, visible.Y),
            Math.Max(0, width - visible.Right),
            Math.Max(0, height - visible.Bottom));
#endif
    }

    private void OnNavToggleClick(object sender, RoutedEventArgs e)
        => Shell.IsPaneOpen = !Shell.IsPaneOpen;

    private void OnReportListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Shell.DisplayMode == SplitViewDisplayMode.Overlay)
            Shell.IsPaneOpen = false;
    }

    private async void OnExportPdfClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Report is not { } report || ViewModel.IsBusy)
            return;

        try
        {
            var picker = new FileSavePicker
            {
                SuggestedFileName = Path.GetFileNameWithoutExtension(ViewModel.SelectedReport.FileName),
                DefaultFileExtension = ".pdf"
            };
            picker.FileTypeChoices.Add("PDF document", new List<string> { ".pdf" });

            if (await picker.PickSaveFileAsync() is not { } file)
                return;

            var bytes = await global::Pysar.Uno.PysarUno.ExportService.ExportAsync(report, ExportFormat.Pdf);
            await FileIO.WriteBytesAsync(file, bytes);
        }
        catch (Exception exception)
        {
            ViewModel.ErrorMessage = exception.ToString();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ReportViewerViewModel.CurrentPage)
            or nameof(ReportViewerViewModel.PageCount))
            ShowPageIndicator();
    }

    private void ShowPageIndicator()
    {
        if (ViewModel.PageCount < 1)
            return;

        PageIndicator.Visibility = Visibility.Visible;
        _pageIndicatorTimer!.Stop();
        _pageIndicatorTimer.Start();
    }

    private void OnPageIndicatorTick(object? sender, object e)
    {
        _pageIndicatorTimer?.Stop();
        PageIndicator.Visibility = Visibility.Collapsed;
    }
}
