using Pysar.Core.Enums;
using Pysar.Core.Structs;
using Pysar.Elements;
using PysarColor = Pysar.Core.Structs.Colors;
using PysarStackPanel = Pysar.Elements.StackPanel;
using PysarStackOrientation = Pysar.Core.Enums.StackOrientation;
using PysarThickness = Pysar.Core.Structs.Thickness;

namespace Pysar.Uno.Sample.Reports;

/// <summary>
///     Faces come from <c>UnoFont</c>, not from <c>AddFont</c>. The family string is the name
///     inside the file (<c>Cookie</c>), not the XAML resource key.
/// </summary>
public static class UnoFontsReport
{
    public static Report Create()
        => ReportBuilder.Create("Uno fonts")
            .WithPageFormat(new PageFormat
            {
                Size = PageSize.A4,
                Margin = new PysarThickness(40)
            })
            .WithDetail(detail => detail.AddElement(new PysarStackPanel()
                .WithOrientation(PysarStackOrientation.Vertical)
                .WithSpacing(16)
                .WithSize(SizeLength.Fill, SizeLength.Fill)
                .AddElements(
                [
                    new Text { Content = "Cookie from UnoFont" }
                        .WithFont("Cookie", 28f, PysarColor.Black),
                    new Text { Content = "\uf031  \uf15c  \uf02f" }
                        .WithFont("Font Awesome 6 Free", 18f, PysarColor.Black, FontStyle.Bold)
                ])))
            .Build();
}
