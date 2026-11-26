using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Charts.Data;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Proportional symbol map", GalleryCategory.Charts, Order = 290, Spec = "ProportionalSymbolMap",
    Summary = "Circles placed at longitude and latitude over a muted base map, each circle's area proportional to its value, colored by group, with a size key.")]
public partial class ProportionalSymbolMapPage : GalleryPage
{
    public ProportionalSymbolMapPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await MapAssets.Shared.LoadAsync();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);
}
