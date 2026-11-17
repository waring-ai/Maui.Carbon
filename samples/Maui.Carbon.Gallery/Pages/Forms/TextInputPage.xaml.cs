using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Text input", GalleryCategory.Forms, Order = 20, Spec = "TextInput",
    Summary = "A single-line field for short, unique values such as names, IDs and addresses.")]
public partial class TextInputPage : GalleryPage
{
    public TextInputPage()
    {
        InitializeComponent();
    }

    void OnNameChanged(object? sender, TextChangedEventArgs e) => Status.Text = $"{e.NewTextValue?.Length ?? 0} characters";

    void OnNameCompleted(object? sender, EventArgs e)
    {
        var text = NameInput.Text ?? "";
        NameInput.IsInvalid = text.Contains(' ') || text.Any(char.IsUpper);
        NameInput.InvalidText = "Use lowercase letters, numbers and dashes.";
        Status.Text = NameInput.IsInvalid ? "Invalid name" : $"Valid: {text}";
    }
}
