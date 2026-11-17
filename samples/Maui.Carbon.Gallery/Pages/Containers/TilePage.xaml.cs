using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Containers;

[GalleryPage("Tile", GalleryCategory.Containers, Order = 10, Spec = "Tile",
    Summary = "A flat container that groups related content; the clickable tile makes the whole surface one action.")]
public partial class TilePage : GalleryPage
{
    int _clicks;

    public TilePage()
    {
        InitializeComponent();
    }

    void OnTileClicked(object? sender, EventArgs e) => Status.Text = $"Clicked {++_clicks} time{(_clicks == 1 ? "" : "s")}";
}
