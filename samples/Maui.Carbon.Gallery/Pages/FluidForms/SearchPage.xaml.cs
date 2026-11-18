using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid search", GalleryCategory.FluidForms, Order = 40, Spec = "FluidSearch",
    Summary = "A search field with the label inside a 64px field, the magnifier at the bottom end and a clear button.")]
public partial class SearchPage : GalleryPage
{
    public SearchPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"Query: {Input.Text}";
}
