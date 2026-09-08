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

The picker is a plain list of factories — the same shape the MAUI and Avalonia samples use:

```csharp
private sealed record ReportOption(string Title, Func<Report> Create);

private static readonly ReportOption[] ReportOptions =
[
    new("Invoice", () => new InvoiceReport(InvoiceData.CreateDesignInstance())),
    new("Annual", () => new AnnualReport(AnnualLedger.CreateDesignInstance())),
    new("Revenue By Customer", () => new RevenueByCustomerReport(RevenueReportData.CreateDesignInstance()))
];

private void LoadReport()
{
    var report = ReportOptions.First(option => option.Title == _selectedReport).Create();
    report.Build();

    _report = report;
    _currentPage = 1;
}
```

Note that the first render after a cold load takes a few seconds: the .NET runtime, the report
assemblies and the assets all have to arrive before Skia can paint anything.
