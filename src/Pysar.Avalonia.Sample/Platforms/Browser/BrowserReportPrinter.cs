using System.Runtime.InteropServices.JavaScript;
using Pysar.Avalonia;
using Pysar.Elements;
using Pysar.Export;

namespace Pysar.Avalonia.Sample;

/// <summary>
///     Browser printer: renders the report to a vector PDF and prints that PDF through the page,
///     which is what the desktop printer does through the OS print UI. The default
///     <c>AvaloniaReportPrinter</c> cannot work here - it shells out to the OS - so the browser
///     bootstrap installs this one on <see cref="SampleServices.Printer" />.
/// </summary>
internal sealed partial class BrowserReportPrinter : IReportPrinter
{
    private const string ModuleName = "pysarPrint";

    private Task? _moduleLoad;

    public async Task PrintAsync(Report report, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);

        // The renderer is read per call rather than captured in the constructor: UsePysar installs
        // it while the AppBuilder chain runs, which can be after this printer is created.
        var pdfBytes = await PysarAvalonia.Renderer
            .RenderToPdfBytesAsync(report, cancellationToken)
            .ConfigureAwait(true);

        cancellationToken.ThrowIfCancellationRequested();

        // Imported once and reused; the module lives beside index.html, one level up from _framework.
        _moduleLoad ??= JSHost.ImportAsync(ModuleName, "../print.js");
        await _moduleLoad.ConfigureAwait(true);

        // Base64 keeps the interop boundary to a single string, with no pinned buffer to outlive
        // the call - the JS side holds on to the bytes for as long as the print frame needs them.
        PrintPdf(Convert.ToBase64String(pdfBytes));
    }

    [JSImport("printPdf", ModuleName)]
    private static partial void PrintPdf(string base64Pdf);
}
