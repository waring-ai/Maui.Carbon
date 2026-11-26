using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Network diagram", GalleryCategory.Charts, Order = 260, Spec = "NetworkDiagram",
    Summary = "Building blocks for node-and-edge diagrams: card and shape nodes, edges with variants and markers, drawn from nodes you have already positioned.")]
public partial class NetworkDiagramPage : GalleryPage
{
    public NetworkDiagramPage()
    {
        InitializeComponent();
    }

    void OnNodeClicked(object? sender, CdsNetworkNodeEventArgs e) => Status.Text = "Clicked " + e.Node.Title;
}
