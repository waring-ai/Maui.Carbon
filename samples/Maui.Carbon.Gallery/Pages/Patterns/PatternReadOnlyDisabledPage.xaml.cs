using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Read-only and disabled pattern", GalleryCategory.Patterns, Order = 72, Spec = "PatternReadOnlyDisabled",
    Summary = "The difference between read-only and disabled, shown for every main input side by side, and in a page where a viewer sees settings read-only and a temporarily unavailable action is disabled.")]
public partial class PatternReadOnlyDisabledPage : GalleryPage
{
    public PatternReadOnlyDisabledPage()
    {
        InitializeComponent();
    }
}
