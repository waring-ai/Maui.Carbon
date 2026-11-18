using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid time picker", GalleryCategory.FluidForms, Order = 60, Spec = "FluidTimePicker",
    Summary = "A time field with its AM/PM and time zone selects as 64px parts of one row.")]
public partial class TimePickerPage : GalleryPage
{
    public TimePickerPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"Time: {Input.Text} {Input.Period}";
}
