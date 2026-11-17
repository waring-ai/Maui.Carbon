using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Password input", GalleryCategory.Forms, Order = 40, Spec = "PasswordInput",
    Summary = "A single-line text field that masks its value, with a button to show or hide the characters.")]
public partial class PasswordInputPage : GalleryPage
{
    public PasswordInputPage()
    {
        InitializeComponent();
    }

    void OnToggled(object? sender, EventArgs e) =>
        Status.Text = ((CdsPasswordInput)sender!).IsPasswordVisible ? "Password is visible" : "Password is hidden";
}
