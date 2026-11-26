using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Combo chart", GalleryCategory.Charts, Order = 110, Spec = "ComboChart",
    Summary = "Several graph types on one set of axes, such as bars for a volume and a line for a rate, each type drawing the datasets you assign to it.")]
public partial class ComboChartPage : GalleryPage
{
    public ComboChartPage()
    {
        InitializeComponent();
    }
}
