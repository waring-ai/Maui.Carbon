using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Grouped bar chart", GalleryCategory.Charts, Order = 20, Spec = "GroupedBarChart",
    Summary = "Bars for several groups side by side within each category or time point, for comparing the groups at every point.")]
public partial class GroupedBarChartPage : GalleryPage
{
    public GroupedBarChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e)
    {
        Interactive.IsLoading = !Interactive.IsLoading;
        Status.Text = Interactive.IsLoading ? "Loading" : "Loaded";
    }
}
