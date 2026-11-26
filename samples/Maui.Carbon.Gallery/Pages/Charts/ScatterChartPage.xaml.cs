using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Scatter chart", GalleryCategory.Charts, Order = 80, Spec = "ScatterChart",
    Summary = "Dots that plot each observation by two measures, or along a time or category axis, to show correlation, clusters, distribution and outliers.")]
public partial class ScatterChartPage : GalleryPage
{
    public ScatterChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnRulerClicked(object? sender, EventArgs e)
    {
        Interactive.IsRulerEnabled = !Interactive.IsRulerEnabled;
        Status.Text = Interactive.IsRulerEnabled ? "Ruler on" : "Ruler off: hover a dot";
    }

    void OnFillClicked(object? sender, EventArgs e)
    {
        Interactive.ArePointsFilled = !Interactive.ArePointsFilled;
        Status.Text = Interactive.ArePointsFilled ? "Filled dots" : "Unfilled dots";
    }

}
