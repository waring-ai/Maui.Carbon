using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Data table skeleton", GalleryCategory.Data, Order = 60, Spec = "DataTableSkeleton",
    Summary = "A loading placeholder in the shape of a data table: optional title block, toolbar, header row and n body rows of pulsing bars.")]
public partial class DataTableSkeletonPage : GalleryPage
{
    public DataTableSkeletonPage() => InitializeComponent();
}
