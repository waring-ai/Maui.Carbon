using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Skeleton placeholder", GalleryCategory.Feedback, Order = 81, Spec = "SkeletonPlaceholder",
    Summary = "An animated grey block standing in for a chart, image, tile or any non-text content while it loads.")]
public partial class SkeletonPlaceholderPage : GalleryPage
{
    public SkeletonPlaceholderPage()
    {
        InitializeComponent();
    }
}
