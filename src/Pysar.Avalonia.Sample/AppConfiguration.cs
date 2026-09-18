using Avalonia;
using Pysar.Avalonia;
using Pysar.Sample.Shared;
using Pysar.Sample.Shared.QRCode;

namespace Pysar.Avalonia.Sample;

/// <summary>
///     The host-independent half of the Avalonia bootstrap. Every head - desktop, browser, mobile -
///     configures its own platform (<c>UsePlatformDetect</c>, <c>UseBrowser</c>, ...) and its own
///     fonts (<c>WithInterFont</c>), then calls this so Pysar is registered the same way everywhere.
/// </summary>
/// <remarks>
///     <c>WithInterFont</c> is deliberately left to each head rather than folded in here: the font
///     registration has to be rooted in the head's own entry assembly, or the browser (WASM)
///     trimmer drops Avalonia.Fonts.Inter from the boot manifest and the app fails to start with
///     "Could not load file or assembly 'Avalonia.Fonts.Inter'".
/// </remarks>
public static class AppConfiguration
{
    /// <summary>
    ///     Registers Pysar with the sample's fonts and custom QR-code drawer.
    /// </summary>
    /// <remarks>
    ///     The report assets (fonts, images) are compiled into this shared library, not into the
    ///     head's entry assembly, so <c>UsePysar</c> is told the assembly to resolve
    ///     <c>avares://</c> URIs against explicitly - the entry-assembly default would point at the
    ///     head, where the assets are not.
    /// </remarks>
    public static AppBuilder ConfigurePysarSample(this AppBuilder builder)
        => builder
            .UsePysar(
                pysar => pysar
                    .AddFonts(ReportBootstrap.RegisterFonts)
                    .AddDrawer<QRCode>(new QRCodeDrawer()),
                assemblyName: "Pysar.Avalonia.Sample");
}
