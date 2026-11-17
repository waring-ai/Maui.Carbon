using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Icon indicator", GalleryCategory.Feedback, Order = 61, Spec = "IconIndicator",
    Summary = "A status glyph followed by a text label. The glyph shape carries the status, so it stays readable without color.")]
public partial class IconIndicatorPage : GalleryPage
{
    public IconIndicatorPage()
    {
        InitializeComponent();
    }
}
