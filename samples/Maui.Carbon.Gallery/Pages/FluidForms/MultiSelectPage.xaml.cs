using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid multi-select", GalleryCategory.FluidForms, Order = 90, Spec = "FluidMultiSelect",
    Summary = "A multi-select with the label inside a 64px field and a count tag.")]
public partial class MultiSelectPage : GalleryPage
{
    public MultiSelectPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"{Input.SelectedItems?.Count ?? 0} selected";
}
