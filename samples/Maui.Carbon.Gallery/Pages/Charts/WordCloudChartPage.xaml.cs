using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Word cloud chart", GalleryCategory.Charts, Order = 250, Spec = "WordCloudChart",
    Summary = "Words sized by a value and colored by group, packed without overlap, for a quick impression of which terms dominate.")]
public partial class WordCloudChartPage : GalleryPage
{
    public WordCloudChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;
}
