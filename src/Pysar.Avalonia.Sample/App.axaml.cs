using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Pysar.Avalonia.Sample.ViewModels;
using Pysar.Avalonia.Sample.Views;

namespace Pysar.Avalonia.Sample;

// Fully qualified: on Android "Application" is ambiguous with Android.App.Application, and a bare
// "Avalonia.Application" would bind to this project's own Pysar.Avalonia namespace.
public partial class App : global::Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        switch (ApplicationLifetime)
        {
            // Desktop (Windows, macOS, Linux): the view lives inside a window.
            case IClassicDesktopStyleApplicationLifetime desktop:
                MacDockIcon.Apply();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new ReportViewerViewModel(SampleServices.Printer),
                };
                break;

            // Single-view hosts (browser, mobile): there is no window, so the view is the root.
            case ISingleViewApplicationLifetime singleView:
                singleView.MainView = new MainView
                {
                    DataContext = new ReportViewerViewModel(SampleServices.Printer),
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
