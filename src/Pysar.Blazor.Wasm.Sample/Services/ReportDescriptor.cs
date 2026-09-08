using Pysar.Elements;
using Pysar.Sample.Shared;
using Pysar.Sample.Shared.Data;
using Pysar.Sample.Shared.Reports.Invoice;

namespace Pysar.Blazor.Wasm.Sample.Services;

/// <summary>One entry of the report panel: a route slug, a display name, how to build the report and its icon.</summary>
/// <param name="Glyph">
///     A Font Awesome solid code point; it only renders inside an element carrying the
///     <c>fa</c> class, which is what binds the font declared in theme.css.
/// </param>
public sealed record ReportDescriptor(string Slug, string Title, string FileName, string Glyph, Func<Report> Create)
{
    public static IReadOnlyList<ReportDescriptor> All { get; } =
    [
        new("invoice", "Invoice", "InvoiceReport.pdf", "\uf571", () => new InvoiceReport(InvoiceData.CreateDesignInstance())),
        new("annual", "Annual", "AnnualReport.pdf", "\uf201", () => new AnnualReport(AnnualLedger.CreateDesignInstance())),
        new("revenue", "Revenue By Customer", "RevenueByCustomer.pdf", "\uf0c0", () => new RevenueByCustomerReport(RevenueReportData.CreateDesignInstance()))
    ];

    /// <summary>The descriptor for a route slug; an unknown slug falls back to the first report.</summary>
    public static ReportDescriptor Find(string? slug)
        => All.FirstOrDefault(descriptor => descriptor.Slug == slug) ?? All[0];
}
