using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Contained list", GalleryCategory.Data, Order = 30, Spec = "ContainedList",
    Summary = "A list with a header and optional actions for groups of similar items in a panel, card or popover.")]
public partial class ContainedListPage : GalleryPage
{
    public ContainedListPage()
    {
        InitializeComponent();
    }

    void OnItemClicked(object? sender, EventArgs e)
    {
        if (sender is CdsContainedListItem item) Status.Text = $"Opened {item.Text}";
    }
}
