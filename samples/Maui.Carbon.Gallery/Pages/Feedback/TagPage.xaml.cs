using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Tag", GalleryCategory.Feedback, Order = 50,
    Summary = "A read-only pill that labels or categorizes an item. Use the same color for the same category everywhere; the label carries the meaning.")]
public partial class TagPage : GalleryPage
{
    public TagPage()
    {
        InitializeComponent();
    }
}
