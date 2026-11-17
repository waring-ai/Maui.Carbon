using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Actions;

[GalleryPage("Overflow menu", GalleryCategory.Actions, Order = 60, Spec = "OverflowMenu",
    Summary = "An icon-only button (three dots) that opens a menu of secondary actions for a row, card or toolbar.")]
public partial class OverflowMenuPage : GalleryPage
{
    public OverflowMenuPage()
    {
        InitializeComponent();
    }

    void OnItemClicked(object? sender, EventArgs e) => Status.Text = $"Chose \"{(sender as CdsMenuItem)?.Text}\"";

    void OnOpened(object? sender, EventArgs e) => Status.Text = "Menu opened";

    void OnClosed(object? sender, EventArgs e) => Status.Text = "Menu closed";
}
