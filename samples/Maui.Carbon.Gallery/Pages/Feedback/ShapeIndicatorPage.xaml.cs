using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Shape indicator", GalleryCategory.Feedback, Order = 62, Spec = "ShapeIndicator",
    Summary = "A small colored shape plus a label, used for severity and lifecycle where the shape distinguishes levels without relying on hue.")]
public partial class ShapeIndicatorPage : GalleryPage
{
    public ShapeIndicatorPage()
    {
        InitializeComponent();
    }
}
