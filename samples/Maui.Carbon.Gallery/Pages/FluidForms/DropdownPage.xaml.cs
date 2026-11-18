using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.FluidForms;

[GalleryPage("Fluid dropdown", GalleryCategory.FluidForms, Order = 70, Spec = "FluidDropdown",
    Summary = "A dropdown whose label sits inside the top of a 64px field above the chosen item.")]
public partial class DropdownPage : GalleryPage
{
    public DropdownPage()
    {
        InitializeComponent();
    }

    void OnChanged(object? sender, EventArgs e) => Status.Text = $"Region: {Input.SelectedItem}";
}
