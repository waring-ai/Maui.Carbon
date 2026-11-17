using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Skeleton text", GalleryCategory.Feedback, Order = 80, Spec = "SkeletonText",
    Summary = "An animated grey bar standing in for a line, heading or paragraph of text while it loads.")]
public partial class SkeletonTextPage : GalleryPage
{
    public SkeletonTextPage()
    {
        InitializeComponent();
    }
}
