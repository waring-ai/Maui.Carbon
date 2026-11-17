using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Containers;

[GalleryPage("Radio tile", GalleryCategory.Containers, Order = 13, Spec = "RadioTile",
    Summary = "A tile that acts as a radio button, grouped so exactly one can be chosen.")]
public partial class RadioTilePage : GalleryPage
{
    public RadioTilePage()
    {
        InitializeComponent();
    }

    void OnSizeChanged(object? sender, CdsChoiceChangedEventArgs<object> e) =>
        Status.Text = e.NewValue is null ? "Nothing chosen" : $"Chosen: {e.NewValue}";
}
