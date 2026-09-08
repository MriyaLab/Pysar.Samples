# Pysar.Avalonia.Sample

The Pysar viewer on the Avalonia desktop — Windows, macOS and Linux from one `net10.0` project.

```bash
dotnet run --project src/Pysar.Avalonia.Sample
```

A 1000×700 window with a report drawer behind the toolbar's hamburger, an in-window `Menu`
(File / View), a toolbar of rounded pills (zoom controls, fit toggle, print, export), and a
`ReportView` filling the rest. Shortcuts: Ctrl+P print, Ctrl+E export, Ctrl++ / Ctrl+- zoom,
Ctrl+0 actual size, Ctrl+W exit.
The reports come from
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

## The report panel

```csharp
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
```

The panel is an ordinary `ListBox` bound to that list with `SelectedItem="{Binding SelectedReport}"`,
so selection needs no command of its own. `Glyph` is a Font Awesome code point, rendered by the
`nav-item-icon` style.

It is a drawer at every window size, as the MAUI flyout is: hidden until the toolbar's hamburger
opens it, then drawn over a scrim above the content, and closed again by the scrim, the hamburger or
picking a report. Rather than a property on the window, the state is a class the styles react to:

```csharp
private void OnNavToggleClick(object? sender, RoutedEventArgs e)
    => Classes.Set("nav-open", !Classes.Contains("nav-open"));
```

```xml
<Style Selector="DockPanel.nav-panel">
    <Setter Property="IsVisible" Value="False" />
</Style>

<Style Selector="Window.nav-open DockPanel.nav-panel">
    <Setter Property="IsVisible" Value="True" />
</Style>
```

The view model is a port of the MAUI sample's. Printing goes through the same `IReportPrinter`
abstraction; the PDF export lives in the window rather than the view model, because
`StorageProvider.SaveFilePickerAsync` needs a top level:

```csharp
private readonly IReportExportService _exporter = SkiaReportExport.CreateExportService(PysarAvalonia.Renderer);
```

`SkiaReportExport.CreateExportService` is what a host without a DI container uses; `AddPysar` calls
the same registration for the hosts that have one.

## Chrome

The dark theme is the MAUI sample's, translated to Avalonia: `Themes/Colors.axaml` holds the
`Chrome*` entries of `Resources/Styles/Colors.xaml`, and `Themes/Styles.axaml` the toolbar pills and
panel rows. The translation is not a copy — where MAUI keys a style and applies it per control,
Avalonia matches on a class:

```xml
<Style Selector="Border.toolbar-pill">
    <Setter Property="Background" Value="{StaticResource ChromePillBrush}" />
    <Setter Property="CornerRadius" Value="24" />
    <Setter Property="Height" Value="48" />
</Style>
```

`RequestedThemeVariant="Dark"` is fixed rather than following the system, so Fluent's scrollbars and
menus match the chrome instead of turning white on a light desktop.
