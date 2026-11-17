using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Actions;

[GalleryPage("Menu button", GalleryCategory.Actions, Order = 70, Spec = "MenuButton",
    Summary = "A button with a chevron that opens a menu of related actions beneath it.")]
public partial class MenuButtonPage : GalleryPage
{
    public MenuButtonPage()
    {
        InitializeComponent();
    }

    void OnItemClicked(object? sender, EventArgs e) => Status.Text = $"Chose \"{(sender as CdsMenuItem)?.Text}\"";
}
