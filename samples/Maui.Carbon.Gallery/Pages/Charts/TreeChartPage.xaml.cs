using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Tree chart", GalleryCategory.Charts, Order = 230, Spec = "TreeChart",
    Summary = "A left-to-right node-link diagram of a hierarchy, as a tidy tree or a dendrogram, whose parent nodes collapse and expand on click.")]
public partial class TreeChartPage : GalleryPage
{
    public TreeChartPage()
    {
        InitializeComponent();
    }

    void OnNodeClicked(object? sender, CdsChartNodeEventArgs e) => Status.Text = "Toggled " + e.Node.Name;

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;
}
