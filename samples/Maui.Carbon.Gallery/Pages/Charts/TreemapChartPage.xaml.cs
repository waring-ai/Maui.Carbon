using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Treemap chart", GalleryCategory.Charts, Order = 210, Spec = "TreemapChart",
    Summary = "Nested rectangles that show part-to-whole within a two-level hierarchy, each top-level group in its own palette color and each leaf sized by its value.")]
public partial class TreemapChartPage : GalleryPage
{
    public TreemapChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;
}
