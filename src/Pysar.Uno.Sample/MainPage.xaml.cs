using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pysar.Elements;
using Pysar.Export;
using Pysar.Sample.Shared;
using Pysar.Sample.Shared.Data;
using Pysar.Sample.Shared.Reports.Invoice;
using Pysar.Viewer.Zoom;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Pysar.Uno.Sample;

/// <summary>
/// One entry of the report picker: a display name, the name its PDF export is offered under, and
/// how to build the report.
/// </summary>
public sealed record ReportDescriptor(string Title, string FileName, Func<Report> Create)
{
    public static IReadOnlyList<ReportDescriptor> All { get; } =
    [
        new("Invoice", "InvoiceReport.pdf", () => new InvoiceReport(InvoiceData.CreateDesignInstance())),
        new("Annual", "AnnualReport.pdf", () => new AnnualReport(AnnualLedger.CreateDesignInstance())),
        new("Revenue By Customer", "RevenueByCustomer.pdf", () => new RevenueByCustomerReport(RevenueReportData.CreateDesignInstance()))
    ];

    public override string ToString() => Title;
}

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        ReportPicker.ItemsSource = ReportDescriptor.All;
        ZoomPicker.ItemsSource = new[] { ReportZoomMode.FitWidth, ReportZoomMode.FitPage, ReportZoomMode.Custom };

        // The view reports where the scroll landed and what the current mode resolved to; showing
        // them is what makes a wheel zoom or a pinch visibly do something beyond the pixels.
        Viewer.RegisterPropertyChangedCallback(ReportView.CurrentPageProperty, (_, _) => UpdateStatus());
        Viewer.RegisterPropertyChangedCallback(ReportView.PageCountProperty, (_, _) => UpdateStatus());
        Viewer.RegisterPropertyChangedCallback(ReportView.EffectiveZoomProperty, (_, _) => UpdateStatus());

        Viewer.RenderFailed += (_, exception) => StatusText.Text = $"Render failed: {exception.Message}";

        ReportPicker.SelectedIndex = 0;
        ZoomPicker.SelectedIndex = 0;
    }

    private void OnReportChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ReportPicker.SelectedItem is not ReportDescriptor descriptor)
            return;

        try
        {
            var report = descriptor.Create();

            // The view takes a report that has already been built - it measures and paginates, it
            // does not build.
            report.Build();

            Viewer.Report = report;
            StatusText.Text = "Loading...";
        }
        catch (Exception exception)
        {
            Viewer.Report = null;
            StatusText.Text = $"Build failed: {exception.Message}";
        }
    }

    private void OnZoomModeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ZoomPicker.SelectedItem is ReportZoomMode mode)
            Viewer.ZoomMode = mode;
    }

    /// <summary>
    /// Renders the current report to PDF and offers it for saving. Rendering exercises a different
    /// slice of Skia's C API than on-screen drawing does, which is why it is the first thing to
    /// break when the managed SkiaSharp and the native binary linked beside it come from different
    /// versions. The full exception goes into the status line deliberately: on a browser head that
    /// is the only place it is readable.
    /// </summary>
    private async void OnExportPdfClick(object sender, RoutedEventArgs e)
    {
        if (Viewer.Report is not { } report || ReportPicker.SelectedItem is not ReportDescriptor descriptor)
            return;

        ExportPdfButton.IsEnabled = false;

        try
        {
            // The picker comes first, before the report is rendered, and the order is not a
            // preference. On WebAssembly this reaches the browser's File System Access API, which
            // only opens while the click that started it still counts as a user gesture - a window
            // of a few seconds. Rendering a multi-page report to PDF outruns it, and the picker
            // then refuses to open at all, which arrives here as an ordinary "user cancelled".
            //
            // FileSavePicker is also what makes one code path serve both heads: a save dialog on
            // the desktop host, a download in the browser.
            var picker = new FileSavePicker
            {
                SuggestedFileName = Path.GetFileNameWithoutExtension(descriptor.FileName),
                DefaultFileExtension = ".pdf"
            };
            picker.FileTypeChoices.Add("PDF document", new List<string> { ".pdf" });

            if (await picker.PickSaveFileAsync() is not { } file)
            {
                StatusText.Text = "Saving cancelled";
                return;
            }

            StatusText.Text = "Exporting...";

            var bytes = await PysarUno.ExportService.ExportAsync(report, ExportFormat.Pdf);

            await FileIO.WriteBytesAsync(file, bytes);

            StatusText.Text = $"Saved {file.Name}: {bytes.Length:N0} bytes";
        }
        catch (Exception exception)
        {
            StatusText.Text = $"PDF export failed: {exception}";
        }
        finally
        {
            ExportPdfButton.IsEnabled = true;
        }
    }

    private void UpdateStatus()
        => StatusText.Text = Viewer.PageCount == 0
            ? "-"
            : $"Page {Viewer.CurrentPage} of {Viewer.PageCount}  -  {Viewer.EffectiveZoom:P0}";
}
