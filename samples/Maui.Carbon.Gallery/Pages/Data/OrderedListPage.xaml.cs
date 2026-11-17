using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Ordered list", GalleryCategory.Data, Order = 10, Spec = "OrderedList",
    Summary = "Numbered and bulleted lists, plain or nested, in the productive (body-01) or expressive (body-02) type size.")]
public partial class OrderedListPage : GalleryPage
{
    public OrderedListPage()
    {
        InitializeComponent();
    }
}
