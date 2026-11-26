using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Stacked area chart", GalleryCategory.Charts, Order = 60, Spec = "StackedAreaChart",
    Summary = "Areas stacked on each other so each band shows one group's share and the top edge shows the total, across an ordered domain; with a percentage axis every point sums to 100%.")]
public partial class StackedAreaChartPage : GalleryPage
{
    public StackedAreaChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;

}
