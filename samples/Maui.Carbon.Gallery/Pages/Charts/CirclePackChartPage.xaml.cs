using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Circle pack chart", GalleryCategory.Charts, Order = 220, Spec = "CirclePackChart",
    Summary = "Nested circles that show a hierarchy of up to three levels, sized by value, with top-level groups in palette colors and optional click-to-zoom into the third level.")]
public partial class CirclePackChartPage : GalleryPage
{
    public CirclePackChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;

    void OnCircleClicked(object? sender, CdsChartNodeEventArgs e) =>
        CircleStatus.Text = "Clicked " + e.Node.Name;
}
