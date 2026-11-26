using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Bullet chart", GalleryCategory.Charts, Order = 140, Spec = "BulletChart",
    Summary = "Bars that measure progress toward a target against qualitative ranges (poor, satisfactory, great), one row per measure.")]
public partial class BulletChartPage : GalleryPage
{
    public BulletChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);
}
