using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Date picker", GalleryCategory.Forms, Order = 103, Spec = "DatePicker",
    Summary = "A date field with a calendar popup: a single date, a range with two fields, or a typed date with no calendar.")]
public partial class DatePickerPage : GalleryPage
{
    public DatePickerPage()
    {
        InitializeComponent();
    }

    void OnDateChanged(object? sender, CdsDateChangedEventArgs e) =>
        Status.Text = e.StartDate is { } date ? $"Deploying on {date:dddd, MMMM d}" : "No date chosen";
}
