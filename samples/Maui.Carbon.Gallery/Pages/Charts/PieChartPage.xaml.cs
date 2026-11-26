using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Pie chart", GalleryCategory.Charts, Order = 150, Spec = "PieChart",
    Summary = "Slices of one whole, one row per part, with percentage labels outside the pie and callouts for the smallest slices.")]
public partial class PieChartPage : GalleryPage
{
    public PieChartPage()
    {
        InitializeComponent();
        Sorted.SortComparison = (a, b) => string.Compare(a.Group, b.Group, StringComparison.CurrentCulture);
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;
}
