using Pysar.Elements;
using Pysar.Sample.Shared.Data;
using Pysar.Sample.Shared.Reports;
using Pysar.Sample.Shared.Reports.Invoice;

namespace Pysar.Uno.Sample;

/// <summary>One entry of the report panel: a display name, export file name, icon glyph and factory.</summary>
/// <param name="Glyph">
///     A Font Awesome solid code point; it only renders where <c>FontAwesomeSolid</c> is applied.
/// </param>
public sealed record ReportDescriptor(string Title, string FileName, string Glyph, Func<Report> Create)
{
    public static IReadOnlyList<ReportDescriptor> All { get; } =
    [
        new("Invoice", "InvoiceReport.pdf", "\uf571", () => new InvoiceReport(InvoiceData.CreateDesignInstance())),
        new("Annual", "AnnualReport.pdf", "\uf201", () => new AnnualReport(AnnualLedger.CreateDesignInstance())),
        new("Revenue By Customer", "RevenueByCustomer.pdf", "\uf0c0", () => new RevenueByCustomerReport(RevenueReportData.CreateDesignInstance()))
    ];

    public override string ToString() => Title;
}
