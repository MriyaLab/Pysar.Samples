using Pysar.Export;

namespace Pysar.Avalonia.Sample;

/// <summary>
///     The few services a head has to supply because the shared library cannot build them itself.
/// </summary>
/// <remarks>
///     Avalonia's AppBuilder has no service collection, so a head assigns these before the app
///     starts and <c>App</c> reads them when it creates the view model.
/// </remarks>
public static class SampleServices
{
    /// <summary>
    ///     The printer the viewer uses. A head sets this when the default Avalonia desktop printer
    ///     cannot work: the browser has no OS print UI, so the browser head supplies one that goes
    ///     through the page instead. Left null, the view model falls back to
    ///     <c>AvaloniaReportPrinter</c>, which is correct for desktop.
    /// </summary>
    public static IReportPrinter? Printer { get; set; }
}
