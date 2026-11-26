using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Simple bar chart", GalleryCategory.Charts, Order = 10, Spec = "SimpleBarChart",
    Summary = "Bars that compare one value per category or time point, vertical or horizontal, from a zero baseline or floating between two values.")]
public partial class SimpleBarChartPage : GalleryPage
{
    public SimpleBarChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;
}
