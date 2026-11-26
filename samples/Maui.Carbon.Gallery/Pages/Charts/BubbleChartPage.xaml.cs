using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Bubble chart", GalleryCategory.Charts, Order = 90, Spec = "BubbleChart",
    Summary = "Dots placed by two measures and sized by a third, to compare items on three dimensions at once.")]
public partial class BubbleChartPage : GalleryPage
{
    public BubbleChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnRulerClicked(object? sender, EventArgs e)
    {
        Interactive.IsRulerEnabled = !Interactive.IsRulerEnabled;
        Status.Text = Interactive.IsRulerEnabled ? "Ruler on" : "Ruler off: hover a bubble";
    }

}
