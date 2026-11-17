using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Skeleton icon", GalleryCategory.Feedback, Order = 82, Spec = "SkeletonIcon",
    Summary = "A 16x16px pulsing placeholder shown where an icon will appear once content loads.")]
public partial class SkeletonIconPage : GalleryPage
{
    public SkeletonIconPage()
    {
        InitializeComponent();
    }
}
