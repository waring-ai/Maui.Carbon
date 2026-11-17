using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("Combo box", GalleryCategory.Forms, Order = 101, Spec = "ComboBox",
    Summary = "A text field with a list of options that filters as you type, for choosing one value from a long list.")]
public partial class ComboBoxPage : GalleryPage
{
    public ComboBoxPage()
    {
        InitializeComponent();
    }

    void OnImageChanged(object? sender, CdsListBoxSelectionChangedEventArgs e) =>
        Status.Text = e.SelectedItem is { } item ? $"Chose {item}" : $"Custom image: {e.InputText}";
}
