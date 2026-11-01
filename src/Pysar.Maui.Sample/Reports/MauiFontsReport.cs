using Pysar.Core.Enums;
using Pysar.Core.Structs;
using Pysar.Elements;
using PysarFileImageSource = Pysar.Elements.FileImageSource;
using PysarStackOrientation = Pysar.Core.Enums.StackOrientation;
using PysarColor = Pysar.Core.Structs.Colors;
using PysarImage = Pysar.Elements.Image;
using PysarThickness = Pysar.Core.Structs.Thickness;

namespace Pysar.Maui.Sample.Reports;

/// <summary>
///     A report whose faces come from <c>ConfigureFonts</c>, not from <c>AddFont</c>.
///     <c>OpenSansRegular</c> and <c>CookieRegular</c> are aliases registered only with MAUI.
/// </summary>
public static class MauiFontsReport
{
    public static Report Create()
        => ReportBuilder.Create("MAUI fonts")
            .WithPageFormat(new PageFormat
            {
                Size = PageSize.A4,
                Margin = new PysarThickness(40)
            })
            .WithDetail(detail => detail.AddElement(new StackPanel()
                .WithOrientation(PysarStackOrientation.Vertical)
                .WithSpacing(16)
                .WithSize(SizeLength.Fill, SizeLength.Fill)
                .AddElements(
                [
                    new Text { Content = "Open Sans from ConfigureFonts" }
                        .WithFont("OpenSansRegular", 22f, PysarColor.Black),
                    new Text { Content = "Cookie from ConfigureFonts" }
                        .WithFont("CookieRegular", 28f, PysarColor.Black),
                    new PysarImage { Source = new PysarFileImageSource("Images/world.svg") }
                        .WithSize(160, 100)
                        .WithHorizontalAlignment(Alignment.Start)
                ])))
            .Build();
}
