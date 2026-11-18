using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid form", GalleryCategory.FluidForms, Order = 1, Spec = "FluidForm",
    Summary = "A wrapper for fluid inputs: fields with the label inside a 64px box, stacked flush so the form reads as one block.")]
public partial class FluidFormPage : GalleryPage
{
    public FluidFormPage()
    {
        InitializeComponent();
    }

    void OnCreate(object? sender, EventArgs e) => Form.Submit();

    void OnSubmitted(object? sender, EventArgs e) => Status.Text = "Deployment submitted";
}
