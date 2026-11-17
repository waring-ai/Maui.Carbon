using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Layout.Data;

namespace Maui.Carbon.Gallery.Pages.Layout;

[GalleryPage("Aspect ratio", GalleryCategory.Layout, Order = 30, Spec = "AspectRatio",
    Summary = "A box that keeps a fixed width-to-height ratio at any width.")]
public partial class AspectRatioPage : GalleryPage
{
    public AspectRatioPage()
    {
        InitializeComponent();

        foreach (var (ratio, name) in new[]
        {
            (CdsRatio.Ratio1x1, "1:1"), (CdsRatio.Ratio2x1, "2:1"), (CdsRatio.Ratio2x3, "2:3"), (CdsRatio.Ratio3x2, "3:2"),
            (CdsRatio.Ratio4x3, "4:3"), (CdsRatio.Ratio16x9, "16:9"),
        })
        {
            var box = new CdsAspectRatio { Ratio = ratio, WidthRequest = 160, Margin = new Thickness(0, 0, 24, 24) };
            box.Add(LayoutDemos.Cell(name));
            RatioFlex.Add(box);
        }

        for (var i = 1; i <= 4; i++)
        {
            var box = new CdsAspectRatio { Ratio = CdsRatio.Ratio16x9 };
            box.Add(LayoutDemos.Cell($"Thumbnail {i}"));
            ThumbGrid.Add(new CdsColumn { Content = box, SpanLg = 4, SpanMd = 4, SpanSm = 4 });
        }
    }
}
