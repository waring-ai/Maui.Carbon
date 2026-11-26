using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Charts.Data;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Connection map", GalleryCategory.Charts, Order = 300, Spec = "ConnectionMap",
    Summary = "Lines between pairs of locations over a muted base map, drawn as great circles, straight lines or arcs, with line width by value and location markers.")]
public partial class ConnectionMapPage : GalleryPage
{
    string _groups = "all groups shown";

    public ConnectionMapPage()
    {
        InitializeComponent();
        foreach (var node in PackageESamples.DataCenters) Links.Nodes.Add(node);
        Loaded += async (_, _) => await MapAssets.Shared.LoadAsync();
    }

    void OnLineTypeClicked(object? sender, EventArgs e)
    {
        Interactive.LineType = Interactive.LineType switch
        {
            CdsConnectionLineType.GreatCircle => CdsConnectionLineType.Straight,
            CdsConnectionLineType.Straight => CdsConnectionLineType.Arc,
            _ => CdsConnectionLineType.GreatCircle,
        };
        UpdateStatus();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e)
    {
        _groups = e.SelectedGroups.Count == 0 ? "all groups shown" : "showing " + string.Join(", ", e.SelectedGroups);
        UpdateStatus();
    }

    void UpdateStatus() => Status.Text = Interactive.LineType switch
    {
        CdsConnectionLineType.Straight => "Straight lines",
        CdsConnectionLineType.Arc => "Arcs",
        _ => "Great circles",
    } + ", " + _groups;
}
