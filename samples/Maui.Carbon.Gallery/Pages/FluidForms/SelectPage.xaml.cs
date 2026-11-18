using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid select", GalleryCategory.FluidForms, Order = 50, Spec = "FluidSelect",
    Summary = "A native select with the label inside a 64px field.")]
public partial class SelectPage : GalleryPage
{
    public SelectPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"Region: {Input.SelectedItem}";
}
