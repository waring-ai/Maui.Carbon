using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Productive card", GalleryCategory.Products, Order = 1, Spec = "ProductiveCard",
    Summary = "A card for task-focused content in dashboards and resource lists, with small ghost actions.")]
public partial class ProductiveCardPage : GalleryPage
{
    public ProductiveCardPage()
    {
        InitializeComponent();
    }

    void OnCardClicked(object? sender, EventArgs e) =>
        Status.Text = sender is CdsProductiveCard card ? $"Opened {card.Title}" : "Clicked";

    void OnReadMore(object? sender, EventArgs e) => Status.Text = "Read more";
}
