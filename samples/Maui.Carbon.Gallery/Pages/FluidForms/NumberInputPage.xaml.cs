using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid number input", GalleryCategory.FluidForms, Order = 30, Spec = "FluidNumberInput",
    Summary = "A numeric field with the label inside a 64px field and the steppers at the bottom end.")]
public partial class NumberInputPage : GalleryPage
{
    public NumberInputPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"Workers: {Input.Value}";
}
