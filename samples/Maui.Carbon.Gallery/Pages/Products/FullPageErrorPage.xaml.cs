using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Full page error", GalleryCategory.Products, Order = 61, Spec = "FullPageError",
    Summary = "A whole-page error for a page people cannot open: a 403, a 404 or another error, with a description, links back and a line drawing.")]
public partial class FullPageErrorPage : GalleryPage
{
    public FullPageErrorPage()
    {
        InitializeComponent();
    }

    void OnLinkClicked(object? sender, EventArgs e)
    {
        if (sender is CdsLink link) Status.Text = $"Opened: {link.Text}";
    }
}
