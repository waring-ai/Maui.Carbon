using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Stacked bar chart", GalleryCategory.Charts, Order = 30, Spec = "StackedBarChart",
    Summary = "Bars that stack each group's value into one bar per category or time point, showing totals and their composition.")]
public partial class StackedBarChartPage : GalleryPage
{
    public StackedBarChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnRulerClicked(object? sender, EventArgs e)
    {
        Interactive.IsRulerEnabled = !Interactive.IsRulerEnabled;
        Status.Text = Interactive.IsRulerEnabled ? "Ruler on" : "Ruler off";
    }

    void OnLoadingClicked(object? sender, EventArgs e)
    {
        Interactive.IsLoading = !Interactive.IsLoading;
        Status.Text = Interactive.IsLoading ? "Loading" : "Loaded";
    }
}
