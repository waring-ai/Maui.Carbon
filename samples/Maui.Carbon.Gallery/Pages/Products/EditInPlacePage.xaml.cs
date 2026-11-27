using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Edit in place", GalleryCategory.Products, Order = 10, Spec = "EditInPlace",
    Summary = "A value that reads as text and turns into an input in place, with save and cancel buttons.")]
public partial class EditInPlacePage : GalleryPage
{
    public EditInPlacePage()
    {
        InitializeComponent();
    }

    void OnSaved(object? sender, CdsEditInPlaceEventArgs e) => Status.Text = $"Saved {e.Value} (was {e.InitialValue})";

    void OnCanceled(object? sender, CdsEditInPlaceEventArgs e) => Status.Text = $"Cancelled, back to {e.InitialValue}";

    void OnBlurred(object? sender, CdsEditInPlaceEventArgs e) => Status.Text = $"Left the field with {e.Value}; it started as {e.InitialValue}";
}
