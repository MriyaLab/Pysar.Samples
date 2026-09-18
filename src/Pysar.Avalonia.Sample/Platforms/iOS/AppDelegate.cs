using Avalonia;
using Avalonia.iOS;
using Foundation;

namespace Pysar.Avalonia.Sample;

/// <summary>
///     The iOS entry point. iOS has no Main of our own either: UIApplicationMain creates this
///     delegate, Avalonia hosts the shared <see cref="App" />, and App shows MainView through the
///     single-view lifetime - the same path the browser and Android take.
/// </summary>
[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<App>
{
    // The platform-independent half of the bootstrap is shared with every other platform; iOS needs
    // no UsePlatformDetect, the delegate has already installed the iOS platform.
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        // The desktop printer shells out to an OS shell iOS does not have, so the viewer is given
        // one that goes through UIKit's own print controller instead.
        SampleServices.Printer = new IosReportPrinter();

        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .ConfigurePysarSample();
    }
}
