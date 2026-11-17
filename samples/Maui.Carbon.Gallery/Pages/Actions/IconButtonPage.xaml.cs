using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Actions;

[GalleryPage("Icon button", GalleryCategory.Actions, Order = 20, Spec = "IconButton",
    Summary = "A square, icon-only button that always carries a tooltip label.")]
public partial class IconButtonPage : GalleryPage
{
    int _refreshes;

    public IconButtonPage()
    {
        InitializeComponent();
    }

    void OnRefreshClicked(object? sender, EventArgs e) => Status.Text = $"Logs refreshed {++_refreshes} time(s)";

    void OnPinClicked(object? sender, EventArgs e) => Status.Text = PinButton.IsSelected == true ? "Cluster pinned" : "Cluster unpinned";
}
