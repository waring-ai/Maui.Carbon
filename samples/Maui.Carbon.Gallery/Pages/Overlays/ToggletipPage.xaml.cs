using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Overlays;

[GalleryPage("Toggletip", GalleryCategory.Overlays, Order = 50,
    Summary = "An information bubble that opens on click and can hold links and buttons.")]
public partial class ToggletipPage : GalleryPage
{
    public ToggletipPage()
    {
        InitializeComponent();
    }

    void OnEditClicked(object? sender, EventArgs e)
    {
        ActionsTip.IsOpen = false;
        Status.Text = "Edit window clicked";
    }
}
