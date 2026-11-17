using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Actions;

[GalleryPage("Combo button", GalleryCategory.Actions, Order = 80, Spec = "ComboButton",
    Summary = "A split button: a primary action beside a chevron button that opens a menu of related actions.")]
public partial class ComboButtonPage : GalleryPage
{
    public ComboButtonPage()
    {
        InitializeComponent();
    }

    void OnDeployClicked(object? sender, EventArgs e) => Status.Text = "Deploying to production";

    void OnItemClicked(object? sender, EventArgs e) => Status.Text = $"Chose \"{(sender as CdsMenuItem)?.Text}\"";
}
