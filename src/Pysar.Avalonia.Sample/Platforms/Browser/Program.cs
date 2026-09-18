using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;

[assembly: SupportedOSPlatform("browser")]

namespace Pysar.Avalonia.Sample;

internal sealed partial class Program
{
    // The WebAssembly host has no window, so the app starts against the DOM element "out" (see
    // wwwroot/index.html) and App shows MainView through the single-view lifetime.
    private static Task Main(string[] args)
    {
        // The OS print UI the desktop printer shells out to does not exist here, so the viewer is
        // given one that prints the rendered PDF through the page instead.
        SampleServices.Printer = new BrowserReportPrinter();

        return BuildAvaloniaApp().StartBrowserAppAsync("out");
    }

    // The Pysar registration (fonts, custom drawer) is shared with every other platform. Browser
    // adds no UsePlatformDetect - StartBrowserAppAsync installs the browser platform itself.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .WithInterFont()
            .ConfigurePysarSample();
}
