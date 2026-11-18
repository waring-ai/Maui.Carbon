using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid text input", GalleryCategory.FluidForms, Order = 10, Spec = "FluidTextInput",
    Summary = "A single-line text field whose label sits inside the 64px field, for dense edge-to-edge fluid forms.")]
public partial class TextInputPage : GalleryPage
{
    public TextInputPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"{Input.Text?.Length ?? 0} characters";
}
