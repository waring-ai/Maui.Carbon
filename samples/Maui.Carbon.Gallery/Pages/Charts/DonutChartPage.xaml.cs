using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Donut chart", GalleryCategory.Charts, Order = 160, Spec = "DonutChart",
    Summary = "A pie with a hole showing the total (or any figure) and a label in its center, for parts of a whole whose sum matters.")]
public partial class DonutChartPage : GalleryPage
{
    public DonutChartPage()
    {
        InitializeComponent();
        Budget.CenterNumberFormatter = n => "$" + n.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "M";
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;
}
