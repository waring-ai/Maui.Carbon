using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Line chart", GalleryCategory.Charts, Order = 40, Spec = "LineChart",
    Summary = "Lines that show how one or more measures change across an ordered domain, usually time, with a ruler that reads every series at the hovered point.")]
public partial class LineChartPage : GalleryPage
{
    public LineChartPage()
    {
        InitializeComponent();
    }
}
