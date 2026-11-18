using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid combo box", GalleryCategory.FluidForms, Order = 80, Spec = "FluidComboBox",
    Summary = "A combo box with the label inside a 64px field above the text entry that filters the list.")]
public partial class ComboBoxPage : GalleryPage
{
    public ComboBoxPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"Zone: {Input.SelectedItem}";
}
