using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Dropdown", GalleryCategory.Forms, Order = 100, Spec = "Dropdown",
    Summary = "A select-like control: a field that opens a styled list of options and holds one selected item.")]
public partial class DropdownPage : GalleryPage
{
    public DropdownPage()
    {
        InitializeComponent();
        FlavorDropdown.SelectedItem = Data.GListData.Flavors[1];
    }

    void OnRegionChanged(object? sender, CdsListBoxSelectionChangedEventArgs e) =>
        Status.Text = e.SelectedItem is string region ? $"Chose {region}" : "Nothing chosen";
}
