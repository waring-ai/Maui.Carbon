using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Overlays;

[GalleryPage("Tooltip", GalleryCategory.Overlays, Order = 40,
    Summary = "Names an icon-only control or briefly describes a trigger. Opens on hover and keyboard focus.")]
public partial class TooltipPage : GalleryPage
{
    public TooltipPage()
    {
        InitializeComponent();
    }
}
