using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Layout;

[GalleryPage("Stack", GalleryCategory.Layout, Order = 20,
    Summary = "Lays children out in one direction with a consistent gap from the spacing scale.")]
public partial class StackPage : GalleryPage
{
    public StackPage()
    {
        InitializeComponent();
    }
}
