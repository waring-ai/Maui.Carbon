using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Boxplot chart", GalleryCategory.Charts, Order = 130, Spec = "BoxplotChart",
    Summary = "Boxes that summarize each group's distribution: quartiles, median, whiskers to the last values within 1.5 x IQR, and outliers.")]
public partial class BoxplotChartPage : GalleryPage
{
    public BoxplotChartPage()
    {
        InitializeComponent();
    }

    void OnColorClicked(object? sender, EventArgs e)
    {
        Interactive.ColorPairingOption = Interactive.ColorPairingOption % 4 + 1;
        Status.Text = $"Color option {Interactive.ColorPairingOption}";
    }

    void OnLoadingClicked(object? sender, EventArgs e)
    {
        Interactive.IsLoading = !Interactive.IsLoading;
        Status.Text = Interactive.IsLoading ? "Loading" : "Loaded";
    }
}
