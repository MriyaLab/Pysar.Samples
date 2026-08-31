using System.ComponentModel;
using Pysar.Maui.Sample.ViewModels;

namespace Pysar.Maui.Sample.Views;

public partial class ReportViewerPage : ContentPage
{
    private readonly ReportViewerViewModel _viewModel;
    private bool _nativeHandlerWasDetached;
    private bool _isOverflowOpen;
    private CancellationTokenSource? _pageIndicatorHideCts;

    public ReportViewerPage(ReportViewerViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;

        Viewer.HandlerChanged += OnViewerHandlerChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // A newly created flyout page can finish loading the report before the native scroll
        // view exists; reloading once the page is on screen is what actually paints it.
        if (Viewer.Handler is not null)
            _viewModel.Reload();

        ShowPageIndicator();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SetOverflowOpen(false);
        CancelPageIndicatorHide();
        PageIndicator.IsVisible = false;
    }

    private void OnOpenFlyoutClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }

    private void SetOverflowOpen(bool isOpen)
    {
        _isOverflowOpen = isOpen;
        OverflowDismiss.IsVisible = isOpen;
        OverflowMenu.IsVisible = isOpen;
    }

    private void OnOverflowClicked(object? sender, EventArgs e)
        => SetOverflowOpen(!_isOverflowOpen);

    private void OnOverflowDismissed(object? sender, TappedEventArgs e)
        => SetOverflowOpen(false);

    private void OnOverflowPrintClicked(object? sender, TappedEventArgs e)
        => SetOverflowOpen(false);

    private void OnOverflowExportClicked(object? sender, TappedEventArgs e)
        => SetOverflowOpen(false);

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
        if (_viewModel.PageCount < 1)
            return;

        PageIndicator.IsVisible = true;
        CancelPageIndicatorHide();
        _pageIndicatorHideCts = new CancellationTokenSource();
        var token = _pageIndicatorHideCts.Token;
        _ = HidePageIndicatorAsync(token);
    }

    private async Task HidePageIndicatorAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), token);
            if (token.IsCancellationRequested)
                return;

            await MainThread.InvokeOnMainThreadAsync(() => PageIndicator.IsVisible = false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void CancelPageIndicatorHide()
    {
        _pageIndicatorHideCts?.Cancel();
        _pageIndicatorHideCts?.Dispose();
        _pageIndicatorHideCts = null;
    }

    private void OnViewerHandlerChanged(object? sender, EventArgs e)
    {
        if (Viewer.Handler is null)
        {
            _nativeHandlerWasDetached = true;
            return;
        }

        if (!_nativeHandlerWasDetached)
            return;

        _nativeHandlerWasDetached = false;
        _viewModel.Reload();
    }

    /// <summary>Surfaces a viewer failure in the same label the export uses.</summary>
    private void OnRenderFailed(object? sender, Exception exception)
    {
        _viewModel.ErrorMessage = exception.Message;

        Console.WriteLine($"Pysar: render failed - {exception}");
    }
}
