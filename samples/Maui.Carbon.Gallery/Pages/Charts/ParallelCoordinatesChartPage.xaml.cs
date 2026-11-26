using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Parallel coordinates chart", GalleryCategory.Charts, Order = 270, Spec = "ParallelCoordinatesChart",
    Summary = "One vertical axis per variable and one line per record across them, colored by group, for comparing records over several measures with different units.")]
public partial class ParallelCoordinatesChartPage : GalleryPage
{
    string _groups = "all groups shown";

    public ParallelCoordinatesChartPage()
    {
        InitializeComponent();
        Filtered.Filters.Add(new CdsParallelFilter("latency", 80, 180));
    }

    void OnLatencyClicked(object? sender, EventArgs e) => SetFilter(new CdsParallelFilter("latency", 80, 180));

    void OnCpuClicked(object? sender, EventArgs e) => SetFilter(new CdsParallelFilter("cpu", 50, 90));

    void OnClearClicked(object? sender, EventArgs e)
    {
        Interactive.Filters.Clear();
        UpdateStatus();
    }

    void SetFilter(CdsParallelFilter filter)
    {
        var existing = Interactive.Filters.FirstOrDefault(f => f.Key == filter.Key);
        if (existing is not null) Interactive.Filters.Remove(existing);
        else Interactive.Filters.Add(filter);
        UpdateStatus();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e)
    {
        _groups = e.SelectedGroups.Count == 0 ? "all groups shown" : "showing " + string.Join(", ", e.SelectedGroups);
        UpdateStatus();
    }

    void UpdateStatus()
    {
        var filters = Interactive.Filters.Count == 0 ? "No filters" : "Filtered by " + string.Join(" and ", Interactive.Filters.Select(f => f.Key == "cpu" ? "CPU" : f.Key));
        Status.Text = filters + ", " + _groups;
    }
}
