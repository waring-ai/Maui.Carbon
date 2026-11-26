using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Radar chart", GalleryCategory.Charts, Order = 190, Spec = "RadarChart",
    Summary = "Several measures per group plotted on spokes around a circle, each group drawn as a translucent shape.")]
public partial class RadarChartPage : GalleryPage
{
    public RadarChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;
}
