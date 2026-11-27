using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Empty state", GalleryCategory.Products, Order = 60, Spec = "EmptyState",
    Summary = "Tells people why an area has no content and what to do next, with an optional illustration, a title, a subtitle, an action and a link.")]
public partial class EmptyStatePage : GalleryPage
{
    int _count;

    public EmptyStatePage()
    {
        InitializeComponent();
    }

    void OnActionClicked(object? sender, EventArgs e) => Status.Text = $"{++_count} {(_count == 1 ? "key" : "keys")} created";

    void OnLinkClicked(object? sender, EventArgs e) => Status.Text = "Opened: How API keys work";
}
