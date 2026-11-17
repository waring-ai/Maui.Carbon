using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Containers;

[GalleryPage("Expandable tile", GalleryCategory.Containers, Order = 11, Spec = "ExpandableTile",
    Summary = "A tile that shows a summary and reveals more content when expanded.")]
public partial class ExpandableTilePage : GalleryPage
{
    public ExpandableTilePage()
    {
        InitializeComponent();
    }

    void OnExpandedChanged(object? sender, CdsChoiceChangedEventArgs<bool> e) => Status.Text = e.NewValue ? "Expanded" : "Collapsed";
}
