using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid text area", GalleryCategory.FluidForms, Order = 20, Spec = "FluidTextArea",
    Summary = "A multi-line text field whose label sits inside the field above the editor.")]
public partial class TextAreaPage : GalleryPage
{
    public TextAreaPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"{Input.Text?.Length ?? 0} characters";
}
