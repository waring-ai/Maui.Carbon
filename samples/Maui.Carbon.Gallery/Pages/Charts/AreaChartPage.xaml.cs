using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Area chart", GalleryCategory.Charts, Order = 50, Spec = "AreaChart",
    Summary = "Filled areas under one or more series that show how a quantity's volume changes across an ordered domain, with a single-series gradient and a bounded band for forecasts and ranges.")]
public partial class AreaChartPage : GalleryPage
{
    public AreaChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnRulerClicked(object? sender, EventArgs e)
    {
        Interactive.IsRulerEnabled = !Interactive.IsRulerEnabled;
        Status.Text = Interactive.IsRulerEnabled ? "Ruler on: hover reads every region" : "Ruler off: hover a point for its own tooltip";
    }

}
