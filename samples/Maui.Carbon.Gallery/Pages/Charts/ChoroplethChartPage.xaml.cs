using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Charts.Data;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Choropleth chart", GalleryCategory.Charts, Order = 280, Spec = "ChoroplethChart",
    Summary = "A map whose regions are shaded by value on a sequential or diverging scale, with a color legend and a hatch for regions without data.")]
public partial class ChoroplethChartPage : GalleryPage
{
    public ChoroplethChartPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await MapAssets.Shared.LoadAsync();
    }

    void OnProjectionClicked(object? sender, EventArgs e)
    {
        var all = Enum.GetValues<CdsGeoProjection>();
        Interactive.Projection = all[(Array.IndexOf(all, Interactive.Projection) + 1) % all.Length];
        Status.Text = Interactive.Projection switch
        {
            CdsGeoProjection.NaturalEarth1 => "Natural Earth 1",
            CdsGeoProjection.EqualEarth => "Equal Earth",
            CdsGeoProjection.ConicEqualArea => "Conic equal-area",
            CdsGeoProjection.ConicEquidistant => "Conic equidistant",
            var p => p.ToString(),
        } + ", whole map";
    }
}
