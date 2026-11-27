using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Options tile", GalleryCategory.Products, Order = 3, Spec = "OptionsTile",
    Summary = "An expandable tile that groups related settings and summarizes them while closed.")]
public partial class OptionsTilePage : GalleryPage
{
    public OptionsTilePage()
    {
        InitializeComponent();
    }

    void OnOpenChanged(object? sender, CdsChoiceChangedEventArgs<bool> e) => UpdateStatus();

    void OnToggled(object? sender, ToggledEventArgs e) => UpdateStatus();

    void UpdateStatus() =>
        Status.Text = $"{(Interactive.IsOpen ? "Open" : "Closed")}, {(Interactive.IsToggled == true ? "enabled" : "disabled")}";
}
