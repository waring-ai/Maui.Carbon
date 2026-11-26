using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Lollipop chart", GalleryCategory.Charts, Order = 100, Spec = "LollipopChart",
    Summary = "Dots on thin stems that compare one value per category, a lighter alternative to bars when there are many categories.")]
public partial class LollipopChartPage : GalleryPage
{
    public LollipopChartPage()
    {
        InitializeComponent();
    }

    void OnSelectedGroupsChanged(object? sender, CdsChartSelectedGroupsChangedEventArgs e) =>
        Status.Text = e.SelectedGroups.Count == 0 ? "All groups shown" : "Showing " + string.Join(", ", e.SelectedGroups);

    void OnRulerClicked(object? sender, EventArgs e)
    {
        Interactive.IsRulerEnabled = !Interactive.IsRulerEnabled;
        Status.Text = Interactive.IsRulerEnabled ? "Ruler on" : "Ruler off";
    }

    void OnFilledClicked(object? sender, EventArgs e)
    {
        Interactive.ArePointsFilled = !Interactive.ArePointsFilled;
        Status.Text = Interactive.ArePointsFilled ? "Filled dots" : "Hollow dots";
    }
}
