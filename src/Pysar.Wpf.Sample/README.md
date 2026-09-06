# Pysar.Wpf.Sample

The Pysar viewer on WPF. Same toolbar, same view model and the same `ReportView` API as the
[Avalonia sample](../Pysar.Avalonia.Sample) — only the XAML dialect and the startup hook differ.

```bash
dotnet run --project src/Pysar.Wpf.Sample
```

**Windows only.** On any other host the project deliberately degrades to an empty `net10.0` library
so that `dotnet build Pysar.Samples.sln` still succeeds:

```xml
<PropertyGroup Condition="$([MSBuild]::IsOSPlatform('windows'))">
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
</PropertyGroup>

<PropertyGroup Condition="!$([MSBuild]::IsOSPlatform('windows'))">
  <OutputType>Library</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
</PropertyGroup>

<ItemGroup Condition="!$([MSBuild]::IsOSPlatform('windows'))">
  <Compile Include="NonWindowsStub.cs" />
</ItemGroup>
```

The screenshots in this repository were captured on macOS, so this sample has none. The rendered
output is identical to the other hosts — see the
[shared report library](../Pysar.Sample.Reports/README.md).

## Registering Pysar

WPF has no builder chain, so the registration goes in `OnStartup`:

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        this.UsePysar(pysar => pysar
            .RegisterFonts(ReportBootstrap.RegisterFonts)
            .AddDrawer<QRCode>(new QRCodeDrawer()));

        var window = new MainWindow { DataContext = new ReportViewerViewModel() };
        window.Show();
    }
}
```

## Assets as embedded resources

The other hosts copy fonts, images and style dictionaries to disk or fetch them over HTTP. This
project embeds them instead, giving a single-file-friendly executable, and maps each one back to the
path the reports ask for:

```xml
<ItemGroup Condition="$([MSBuild]::IsOSPlatform('windows'))">
  <EmbeddedResource Include="..\Pysar.Sample.Reports\Fonts\**"
                    LogicalName="Fonts/%(Filename)%(Extension)" />
  <EmbeddedResource Include="..\Pysar.Sample.Reports\Images\**"
                    LogicalName="Images/%(Filename)%(Extension)" />
  <EmbeddedResource Include="..\Pysar.Sample.Reports\Styles\**"
                    LogicalName="Styles/%(Filename)%(Extension)" />
</ItemGroup>
```

`LogicalName` is the important part: `Fonts/Ubuntu-Regular.ttf` stays `Fonts/Ubuntu-Regular.ttf`, so
`ReportBootstrap.RegisterFonts` needs no per-host variant.

## The viewer

```xml
<pysar:ReportView x:Name="Viewer"
                  Grid.Row="2"
                  Background="#292929"
                  Report="{Binding Report}"
                  CurrentPage="{Binding CurrentPage}"
                  PageCount="{Binding PageCount, Mode=OneWayToSource}"
                  EffectiveZoom="{Binding EffectiveZoom, Mode=OneWayToSource}"
                  Zoom="{Binding Zoom}"
                  ZoomMode="{Binding ZoomMode}"
                  RenderFailed="OnRenderFailed"
                  DocumentPadding="16"
                  PageSpacing="16"
                  PageBorderColor="#3E4351"
                  PageBorderThickness="2"
                  RenderBudget="256" />
```

Two WPF-specific details in the surrounding markup:

- The page-number `TextBox` needs `UpdateSourceTrigger=PropertyChanged`; WPF's default only commits
  on focus loss.
- Hiding the error label takes a `Style` with `DataTrigger`s for `null` and `""`, since WPF has no
  equivalent of Avalonia's `StringConverters.IsNotNullOrEmpty`.
