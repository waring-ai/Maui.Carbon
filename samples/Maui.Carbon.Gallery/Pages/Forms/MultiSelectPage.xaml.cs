using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Multi-select", GalleryCategory.Forms, Order = 102, Spec = "MultiSelect",
    Summary = "A field that opens a checkbox list and holds several selected items, summarised by a count tag.")]
public partial class MultiSelectPage : GalleryPage
{
    public MultiSelectPage()
    {
        InitializeComponent();
    }

    void OnStatusChanged(object? sender, CdsListBoxSelectionChangedEventArgs e) =>
        Status.Text = e.SelectedItems.Count == 0 ? "Showing every status" : $"Showing {string.Join(", ", e.SelectedItems)}";
}
