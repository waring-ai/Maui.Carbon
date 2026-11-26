using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Histogram chart", GalleryCategory.Charts, Order = 120, Spec = "HistogramChart",
    Summary = "Bars that count how many rows fall into each value range (bin), with groups stacked inside each bin.")]
public partial class HistogramChartPage : GalleryPage
{
    public HistogramChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnMoreBinsClicked(object? sender, EventArgs e) => SetBins(Interactive.Bins + 5);

    void OnFewerBinsClicked(object? sender, EventArgs e) => SetBins(Math.Max(5, Interactive.Bins - 5));

    void SetBins(int bins)
    {
        Interactive.Bins = bins;
        Status.Text = $"About {bins} bins";
    }
}
