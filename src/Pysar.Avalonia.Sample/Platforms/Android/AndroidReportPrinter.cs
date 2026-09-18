using Android.Content;
using Android.OS;
using Android.Print;
using Pysar.Avalonia;
using Pysar.Elements;
using Pysar.Export;

namespace Pysar.Avalonia.Sample;

/// <summary>
///     Android printer: renders the report to a vector PDF and hands it to the system print UI,
///     which is what the desktop printer does through the OS print panel. The default
///     <c>AvaloniaReportPrinter</c> shells out to a desktop shell and reports "Printing is not
///     supported on Android", so the Android bootstrap installs this one on
///     <see cref="SampleServices.Printer" />.
/// </summary>
internal sealed class AndroidReportPrinter : IReportPrinter
{
    public async Task PrintAsync(Report report, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);

        // The renderer is read per call rather than captured in the constructor: UsePysar installs
        // it while the AppBuilder chain runs, which is after this printer is created.
        var pdfBytes = await PysarAvalonia.Renderer
            .RenderToPdfBytesAsync(report, cancellationToken)
            .ConfigureAwait(true);

        cancellationToken.ThrowIfCancellationRequested();

        // PrintManager.Print shows a system dialog, so it needs the activity and the UI thread.
        var activity = MainActivity.Current
            ?? throw new InvalidOperationException("No Android activity is available to print from.");

        var jobName = string.IsNullOrWhiteSpace(report.Metadata.Title) ? "Report" : report.Metadata.Title;

        activity.RunOnUiThread(() =>
        {
            var printManager = (PrintManager?)activity.GetSystemService(Context.PrintService);

            printManager?.Print(jobName, new PdfPrintDocumentAdapter(jobName, pdfBytes), null);
        });
    }

    /// <summary>
    ///     Feeds an already rendered PDF to the print framework. Nothing is laid out here - the
    ///     document exists in full before printing starts, so the adapter only has to copy it to
    ///     the descriptor the framework hands over.
    /// </summary>
    private sealed class PdfPrintDocumentAdapter(string jobName, byte[] pdfBytes) : PrintDocumentAdapter
    {
        public override void OnLayout(
            PrintAttributes? oldAttributes,
            PrintAttributes? newAttributes,
            CancellationSignal? cancellationSignal,
            LayoutResultCallback? callback,
            Bundle? extras)
        {
            if (cancellationSignal?.IsCanceled == true)
            {
                callback?.OnLayoutCancelled();
                return;
            }

            var info = new PrintDocumentInfo.Builder($"{jobName}.pdf")
                .SetContentType(PrintContentType.Document)!
                .SetPageCount(PrintDocumentInfo.PageCountUnknown)!
                .Build();

            callback?.OnLayoutFinished(info, true);
        }

        public override void OnWrite(
            PageRange[]? pages,
            ParcelFileDescriptor? destination,
            CancellationSignal? cancellationSignal,
            WriteResultCallback? callback)
        {
            if (destination is null)
            {
                callback?.OnWriteFailed("No destination to write the document to.");
                return;
            }

            try
            {
                using var output = new Java.IO.FileOutputStream(destination.FileDescriptor);

                output.Write(pdfBytes);
                output.Flush();

                callback?.OnWriteFinished([PageRange.AllPages!]);
            }
            catch (Exception exception)
            {
                callback?.OnWriteFailed(exception.Message);
            }
        }
    }
}
