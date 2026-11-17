using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Toggle", GalleryCategory.Forms, Order = 33, Spec = "Toggle",
    Summary = "Immediately switches a single setting on or off.")]
public partial class TogglePage : GalleryPage
{
    public TogglePage()
    {
        InitializeComponent();
    }

    void OnToggled(object? sender, ToggledEventArgs e) => Status.Text = e.Value ? "Auto-scaling is on" : "Auto-scaling is off";
}
