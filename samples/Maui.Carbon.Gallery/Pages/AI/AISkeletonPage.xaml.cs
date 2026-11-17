using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.AI;

[GalleryPage("AI skeleton", GalleryCategory.AI, Order = 20, Spec = "AISkeleton",
    Summary = "Loading placeholders for content an AI is generating: the regular skeletons with a blue tint, rounded corners and a fast sliding shimmer.")]
public partial class AISkeletonPage : GalleryPage
{
    public AISkeletonPage()
    {
        InitializeComponent();
    }
}
