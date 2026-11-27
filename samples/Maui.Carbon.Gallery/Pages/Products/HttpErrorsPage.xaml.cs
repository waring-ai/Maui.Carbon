using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("HTTP errors", GalleryCategory.Products, Order = 62, Spec = "HTTPErrors",
    Summary = "Full-screen 403, 404 and other HTTP error pages with an error code, a title, a description and links over an illustration.")]
public partial class HttpErrorsPage : GalleryPage
{
    public HttpErrorsPage()
    {
        InitializeComponent();
    }

    void OnLinkClicked(object? sender, EventArgs e)
    {
        if (sender is CdsLink link) Status.Text = $"Opened: {link.Text}";
    }
}
