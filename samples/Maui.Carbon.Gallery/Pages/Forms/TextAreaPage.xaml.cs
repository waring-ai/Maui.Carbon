using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Text area", GalleryCategory.Forms, Order = 30, Spec = "TextArea",
    Summary = "A multi-line text field for longer free-form input such as descriptions and comments, with an optional character or word counter.")]
public partial class TextAreaPage : GalleryPage
{
    public TextAreaPage()
    {
        InitializeComponent();
    }

    void OnTextChanged(object? sender, TextChangedEventArgs e) => Status.Text = $"{e.NewTextValue?.Length ?? 0} of 40 characters";
}
