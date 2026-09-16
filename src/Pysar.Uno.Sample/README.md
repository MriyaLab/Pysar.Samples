# Pysar.Uno.Sample

The Pysar viewer on Uno Platform — desktop, WebAssembly, Android and iOS from one project. Chrome matches
[Pysar.Blazor.Wasm.Sample](../Pysar.Blazor.Wasm.Sample): dark sidebar, rounded toolbar pills,
print and PDF export. The reports come from [Pysar.Sample.Shared](../Pysar.Sample.Shared).

```bash
dotnet run --project src/Pysar.Uno.Sample -f net10.0-desktop
```

```bash
dotnet run --project src/Pysar.Uno.Sample -f net10.0-browserwasm
```

```bash
dotnet run --project src/Pysar.Uno.Sample -f net10.0-android
```

```bash
dotnet run --project src/Pysar.Uno.Sample -f net10.0-ios
```

WASM listens on <http://localhost:5080>.
Android and iOS need the corresponding .NET workloads.

## Registering Pysar

`UsePysar` runs in `App.OnLaunched`. The assembly is this one because report fonts and images
from Shared are linked under `Assets/` as embedded resources (`LogicalName` remains `Fonts/...`
and `Images/...`).

```csharp
this.UsePysar(typeof(App).Assembly, builder =>
{
    ReportBootstrap.RegisterFonts(builder.Fonts);
    ReportBootstrap.RegisterDrawers(PysarUno.Renderer);
});
```

## Chrome

Sidebar (250px) lists Invoice, Annual and Revenue By Customer. Below 641px it overlays and a
hamburger appears. Toolbar pills: zoom steps, fit width/page, print, PDF.

Print uses `UnoReportPrinter` on desktop. On WebAssembly, Android and iOS the package throws
`PlatformNotSupportedException`, so the Print button is disabled; export PDF still works
(`FileSavePicker` first, then `PysarUno.ExportService`).
