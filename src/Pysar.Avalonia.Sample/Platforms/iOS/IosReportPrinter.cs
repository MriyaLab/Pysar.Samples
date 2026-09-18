using CoreGraphics;
using Foundation;
using Pysar.Avalonia;
using Pysar.Elements;
using Pysar.Export;
using UIKit;

namespace Pysar.Avalonia.Sample;

/// <summary>
///     iOS printer: renders the report to a vector PDF and hands it to UIKit's print controller,
///     which is what the desktop printer does through the OS print panel. The default
///     <c>AvaloniaReportPrinter</c> shells out to a desktop shell iOS does not have, so the iOS
///     bootstrap installs this one on <see cref="SampleServices.Printer" />.
/// </summary>
internal sealed class IosReportPrinter : IReportPrinter
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

        var jobName = string.IsNullOrWhiteSpace(report.Metadata.Title) ? "Report" : report.Metadata.Title;

        // UIKit only: the controller presents a sheet, so it has to run on the main thread.
        UIApplication.SharedApplication.BeginInvokeOnMainThread(() => Present(jobName, pdfBytes));
    }

    private static void Present(string jobName, byte[] pdfBytes)
    {
        var printInfo = UIPrintInfo.PrintInfo;
        printInfo.JobName = jobName;
        printInfo.OutputType = UIPrintInfoOutputType.General;

        var controller = UIPrintInteractionController.SharedPrintController;
        controller.PrintInfo = printInfo;
        controller.PrintingItem = NSData.FromArray(pdfBytes);

        // iPad refuses the plain sheet presentation - it needs something to anchor a popover to.
        var view = UIApplication.SharedApplication.Windows
            .FirstOrDefault(window => window.IsKeyWindow)?
            .RootViewController?
            .View;

        if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad && view is not null)
        {
            var anchor = new CGRect(view.Bounds.GetMidX(), view.Bounds.GetMidY(), 1, 1);

            controller.PresentFromRectInView(anchor, view, true, null);

            return;
        }

        controller.Present(true, null);
    }
}
