using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Expressive card", GalleryCategory.Products, Order = 2, Spec = "ExpressiveCard",
    Summary = "A card for browsable, marketing-like content such as catalog entries and getting-started links.")]
public partial class ExpressiveCardPage : GalleryPage
{
    int _clicks;

    public ExpressiveCardPage()
    {
        InitializeComponent();
    }

    void OnCardClicked(object? sender, EventArgs e) => Status.Text = $"Opened {++_clicks} time{(_clicks == 1 ? "" : "s")}";
}
