using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Controls;
using Pysar.Elements;
using Pysar.Sample.Shared;
using Pysar.Sample.Shared.Data;
using Pysar.Sample.Shared.Reports.Invoice;
using Pysar.Viewer.Zoom;

namespace Pysar.Uno.Sample;

/// <summary>One entry of the report picker: a display name and how to build the report.</summary>
public sealed record ReportDescriptor(string Title, Func<Report> Create)
{
    public static IReadOnlyList<ReportDescriptor> All { get; } =
    [
        new("Invoice", () => new InvoiceReport(InvoiceData.CreateDesignInstance())),
        new("Annual", () => new AnnualReport(AnnualLedger.CreateDesignInstance())),
        new("Revenue By Customer", () => new RevenueByCustomerReport(RevenueReportData.CreateDesignInstance()))
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

    private void UpdateStatus()
        => StatusText.Text = Viewer.PageCount == 0
            ? "-"
            : $"Page {Viewer.CurrentPage} of {Viewer.PageCount}  -  {Viewer.EffectiveZoom:P0}";
}
