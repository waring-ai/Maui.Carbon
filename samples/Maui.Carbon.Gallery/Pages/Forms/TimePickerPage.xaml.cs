using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Time picker", GalleryCategory.Forms, Order = 80, Spec = "TimePicker",
    Summary = "A text input for a time (hh:mm) with optional selects beside it for AM/PM and a time zone.")]
public partial class TimePickerPage : GalleryPage
{
    public TimePickerPage()
    {
        InitializeComponent();
    }

    void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var picker = (CdsTimePicker)sender!;
        Status.Text = picker.Time is { } time ? $"Time of day: {time:hh\\:mm}" : "Not a complete time yet";
    }
}
