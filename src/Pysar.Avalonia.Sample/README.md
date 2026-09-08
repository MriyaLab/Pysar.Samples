# Pysar.Avalonia.Sample

The Pysar viewer on the Avalonia desktop — Windows, macOS and Linux from one `net10.0` project.

```bash
dotnet run --project src/Pysar.Avalonia.Sample
```

A 1000×700 window with an in-window `Menu` (File / View / Reports), a toolbar (report picker, page
box, zoom controls, fit toggle, print), and a `ReportView` filling the rest. Shortcuts: Ctrl+P print,
Ctrl++ / Ctrl+- zoom, Ctrl+0 actual size, Ctrl+W exit. The reports come from
[Pysar.Sample.Shared](../Pysar.Sample.Shared); this is what they look like rendered by the shared
viewer control:

![Invoice report in the Pysar viewer](../../docs/screenshots/blazor/blazor-viewer.png)

## Registering Pysar

`UsePysar` slots into the `AppBuilder` chain, next to the platform and font setup:

```csharp
public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .UsePysar(pysar => pysar
            .RegisterFonts(ReportBootstrap.RegisterFonts)
            .AddDrawer<QRCode>(new QRCodeDrawer()))
        .LogToTrace();
```

That is the whole host-specific part. Everything after it is ordinary MVVM.

## The viewer

```xml
<pysar:ReportView x:Name="Viewer"
                  Grid.Row="2"
                  Background="#292929"
                  Report="{Binding Report}"
                  CurrentPage="{Binding CurrentPage}"
                  PageCount="{Binding PageCount, Mode=OneWayToSource}"
                  Zoom="{Binding Zoom}"
                  ZoomMode="{Binding ZoomMode}"
                  EffectiveZoom="{Binding EffectiveZoom}"
                  RenderFailed="OnRenderFailed"
                  DocumentPadding="16"
                  PageSpacing="16"
                  PageBorderColor="#3E4351"
                  PageBorderThickness="2"
                  RenderBudget="256" />
```

- `Report` is a **built** report — `Build()` has already run.
- `PageCount` and `EffectiveZoom` flow out of the control, so they are bound `OneWayToSource`.
- `RenderFailed` is an event rather than an exception on the UI thread: rendering happens off the
  dispatcher, and the sample surfaces the message in a red `TextBlock` above the viewer.

## Zoom modes

`ZoomMode` is `FitWidth`, `FitPage` or `Custom`. The fit modes resolve to a factor only the control
can compute, which is why the toolbar shows `EffectiveZoom` and not `Zoom`:

```csharp
private static readonly double[] ZoomSteps =
    [0.25, 0.33, 0.5, 0.67, 0.75, 0.8, 0.9, 1, 1.1, 1.25, 1.5, 1.75, 2, 2.5, 3, 4, 5];

private void ZoomIn()
{
    var next = ZoomSteps.FirstOrDefault(step => step > EffectiveZoom + 0.001);
    if (next > 0)
        SetZoom(next);
}

private void SetZoom(double zoom)
{
    Zoom = zoom;
    ZoomMode = ReportZoomMode.Custom;   // an explicit zoom leaves fit mode
}
```

Stepping from the *effective* zoom is what makes `+` behave like a browser's zoom after a fit mode:
the next step is relative to what you can actually see.

## The report picker

```csharp
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
```

The view model is a port of the MAUI sample's, minus PDF export — sharing is a platform feature and
this sample is about the viewer. Printing goes through the same `IReportPrinter` abstraction.
