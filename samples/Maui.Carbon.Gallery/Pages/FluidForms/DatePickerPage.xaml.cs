using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid date picker", GalleryCategory.FluidForms, Order = 100, Spec = "FluidDatePicker",
    Summary = "A date picker whose fields are 64px with the label inside, and a calendar under the first field.")]
public partial class DatePickerPage : GalleryPage
{
    public DatePickerPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"Date: {Input.Value:d}";
}
