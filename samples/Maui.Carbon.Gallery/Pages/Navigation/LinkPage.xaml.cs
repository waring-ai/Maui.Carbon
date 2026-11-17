using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Navigation;

[GalleryPage("Link", GalleryCategory.Navigation, Order = 10,
    Summary = "Navigates to another page, site, section or address; underlined on hover, always underlined when inline.")]
public partial class LinkPage : GalleryPage
{
    int _clicks;

    public LinkPage()
    {
        InitializeComponent();
    }

    void OnLinkClicked(object? sender, EventArgs e) => Status.Text = $"Clicked {++_clicks} time(s)";
}
