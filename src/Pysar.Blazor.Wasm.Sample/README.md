# Pysar.Blazor.Wasm.Sample

The Pysar viewer running entirely in the browser: the reports from
[Pysar.Sample.Shared](../Pysar.Sample.Shared) are built and rasterised inside WebAssembly, with no
server-side rendering and no PDF round-trip.

```bash
dotnet run --project src/Pysar.Blazor.Wasm.Sample
```

Then open <http://localhost:5093>.

![Blazor viewer](../../docs/screenshots/blazor/blazor-viewer.png)

![Annual report, five pages](../../docs/screenshots/blazor/blazor-annual.png)

## Registering Pysar

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddPysar(ReportBootstrap.RegisterDrawers);

await builder.Build().RunAsync();
```

`AddPysar` also registers `IReportPrinter`, which the toolbar's Print button injects.

## Assets over HTTP

A browser has no file system, so the fonts and images the reports ask for by path have to be fetched
first and handed to a platform handler that serves them from memory. Everything the reports touch is
listed once and preloaded before the first build:

```csharp
private static readonly string[] ReportAssets =
[
    "Fonts/Ubuntu-Regular.ttf",
    "Fonts/Ubuntu-Bold.ttf",
    "Fonts/LibreBarcode128-Regular.ttf",
    "Images/logo.svg",
    "Images/student.png",
    "Styles/ReportStyles.rxaml",
    "Styles/ReportColors.rxaml"
];

protected override async Task OnInitializedAsync()
{
    WasmPlatformHandler.Install(await PreloadedFileSystem.FetchAsync(Http, ReportAssets));

    // Not ReportBootstrap.Initialize: that installs a disk-reading file system. These
    // registrations are host-agnostic by design and are what the MAUI host calls too.
    ReportBootstrap.RegisterFonts(ReportPlatformHandler.FontCollection);

    LoadReport();
}
```

This is the only meaningful difference from the desktop hosts. Miss it and the report still renders
— just with fallback fonts and empty image boxes.

## The viewer

`ReportView` takes a built `Report` and owns paging and zoom; the page count and the zoom the
control actually settled on come back through change callbacks.

```razor
<ReportView Report="_report"
            CurrentPage="_currentPage"
            CurrentPageChanged="page => _currentPage = page"
            PageCountChanged="count => _pageCount = count"
            Zoom="_zoom"
            ZoomChanged="zoom => _zoom = zoom"
            ZoomMode="_zoomMode"
            ZoomModeChanged="mode => _zoomMode = mode"
            EffectiveZoomChanged="zoom => _effectiveZoom = zoom"
            RenderFailed="exception => _error = exception.ToString()"
            DocumentPadding="16"
            PageSpacing="16"
            PageBorderColor="#3E4351"
            PageBorderThickness="2"
            RenderBudget="256" />
```

Two properties are worth calling out:

- **`ZoomMode` vs `Zoom`.** `FitWidth` and `FitPage` resolve to a factor only the control can know,
  so the toolbar's percentage reads `EffectiveZoom` rather than `Zoom`. Setting `Zoom` implies
  `ZoomMode.Custom`.
- **`RenderBudget`.** Caps how much work one render pass may do, which keeps the UI thread
  responsive on a long document.

## Switching reports

The picker is a plain list of factories — the same shape the MAUI and Avalonia samples use, with a
route slug and a Font Awesome code point so the side panel can render a row per report:

```csharp
public sealed record ReportDescriptor(string Slug, string Title, string FileName, string Glyph, Func<Report> Create)
{
    public static IReadOnlyList<ReportDescriptor> All { get; } =
    [
        new("invoice", "Invoice", "InvoiceReport.pdf", "\uf571", () => new InvoiceReport(InvoiceData.CreateDesignInstance())),
        new("annual", "Annual", "AnnualReport.pdf", "\uf201", () => new AnnualReport(AnnualLedger.CreateDesignInstance())),
        new("revenue", "Revenue By Customer", "RevenueByCustomer.pdf", "\uf0c0", () => new RevenueByCustomerReport(RevenueReportData.CreateDesignInstance()))
    ];
}
```

The viewer page is routed on that slug (`@page "/report/{Slug}"`), so picking a report in the panel
is an ordinary navigation and the selected row comes from `NavLink` rather than from page state.

## Chrome

The dark theme is the MAUI sample's, ported to CSS: the colours in `wwwroot/css/theme.css` are the
`Chrome*` entries of `Resources/Styles/Colors.xaml`, and the rounded toolbar pills are its
`ToolbarPill` styles. Where MAUI opens the report list as a Shell flyout, the browser keeps it as a
permanent 250px panel, collapsing into a drawer below 641px — the width at which MAUI's phone
layout takes over.

Note that the first render after a cold load takes a few seconds: the .NET runtime, the report
assemblies and the assets all have to arrive before Skia can paint anything.
