using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Overlays;

[GalleryPage("Definition tooltip", GalleryCategory.Overlays, Order = 55, Spec = "DefinitionTooltip",
    Summary = "A term in running text with a dotted underline that shows a short definition.")]
public partial class DefinitionTooltipPage : GalleryPage
{
    public DefinitionTooltipPage()
    {
        InitializeComponent();
    }
}
