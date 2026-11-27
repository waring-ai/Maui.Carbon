using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("About modal", GalleryCategory.Products, Order = 40, Spec = "AboutModal",
    Summary = "Tells people about the product: logo, name, version, links, legal text and copyright.")]
public partial class AboutModalPage : GalleryPage
{
    public AboutModalPage()
    {
        InitializeComponent();
    }

    void OnAboutClicked(object? sender, EventArgs e) => AboutModal.IsOpen = true;

    void OnRequestClose(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "About closed";
}
