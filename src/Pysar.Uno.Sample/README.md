# Pysar.Uno.Sample

> The "Uno fonts" report requires the platform font integration added in Pysar commit
> `fc4a988` or a later package containing it. This repository currently pins Pysar packages to
> `0.1.63`, which does not include that integration. Update the package references when that
> release is available; a build against `0.1.63` does not verify automatic font resolution.


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

Sidebar (250px) lists Uno fonts, Invoice, Annual and Revenue By Customer. Uno fonts uses the
`Cookie` and `Font Awesome 6 Free` family names from `UnoFont`, with no `AddFont`. The Font
Awesome icon row requests `FontStyle.Bold`, matching the Solid face's weight. Below
641px the sidebar overlays and a
hamburger appears. Toolbar pills: zoom steps, fit width/page, print, PDF.

Print uses `UnoReportPrinter` on desktop. On WebAssembly, Android and iOS the package throws
`PlatformNotSupportedException`, so the Print button is disabled; export PDF still works
(`FileSavePicker` first, then `PysarUno.ExportService`).
