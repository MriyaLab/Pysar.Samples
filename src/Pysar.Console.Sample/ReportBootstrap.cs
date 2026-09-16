using Pysar.Console.Sample.Reports.CustomControls;
using Pysar.Core;
using Pysar.Core.Enums;
using Pysar.Skia;

namespace Pysar.Console.Sample;

/// <summary>
///     Shared by <c>Program</c> and the design-time .rxaml preview host (discovered by reflection).
///     Without this, the preview has no platform handler: custom fonts and images silently vanish.
/// </summary>
public sealed class ReportBootstrap : IReportBootstrap
{
    public static void Initialize(SkiaReportRenderer renderer)
    {
        // Reads the deployment directory first, then the report assets embedded in this assembly and
        // in any referenced library - which is where ReportAsset puts them.
        ReportPlatformHandler.Create(new DefaultReportPlatformHandler());
        var fonts = ReportPlatformHandler.FontCollection;
        fonts.AddFont("Fonts/Kanit-Bold.ttf", "Kanit", FontStyle.Bold);
        fonts.AddFont("Fonts/Kanit-Regular.ttf", "Kanit");
        fonts.AddFont("Fonts/Ubuntu-Bold.ttf", "Ubuntu", FontStyle.Bold);
        fonts.AddFont("Fonts/Ubuntu-Regular.ttf", "Ubuntu");
        fonts.AddFont("Fonts/LibreBarcode128-Regular.ttf", "LibreBarcode128");
        
        renderer.WithDrawer<QRCode>(new QRCodeDrawer());
    }
}
