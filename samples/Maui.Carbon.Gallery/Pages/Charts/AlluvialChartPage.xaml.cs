using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Alluvial chart", GalleryCategory.Charts, Order = 240, Spec = "AlluvialChart",
    Summary = "Flows between columns of nodes, each band as wide as its value, to show how quantities move between categories or stages (a Sankey diagram).")]
public partial class AlluvialChartPage : GalleryPage
{
    public AlluvialChartPage()
    {
        InitializeComponent();
    }

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;
}
