using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Search", GalleryCategory.Forms, Order = 60, Spec = "Search",
    Summary = "A text field for searching, with a magnifier icon, a clear button that appears once there is text, and an expandable form.")]
public partial class SearchPage : GalleryPage
{
    public SearchPage()
    {
        InitializeComponent();
    }

    void OnTextChanged(object? sender, TextChangedEventArgs e) =>
        Status.Text = string.IsNullOrEmpty(e.NewTextValue) ? "Waiting for a query" : $"Typing: {e.NewTextValue}";

    void OnCompleted(object? sender, EventArgs e) => Status.Text = $"Searched for \"{Query.Text}\"";

    void OnCleared(object? sender, EventArgs e) => Status.Text = "Cleared";
}
