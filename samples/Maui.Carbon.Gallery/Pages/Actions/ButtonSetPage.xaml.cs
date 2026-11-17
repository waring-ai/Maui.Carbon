using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Actions;

[GalleryPage("Button set", GalleryCategory.Actions, Order = 30, Spec = "ButtonSet",
    Summary = "Groups two or three related buttons in a row or column with a hairline separator between them.")]
public partial class ButtonSetPage : GalleryPage
{
    public ButtonSetPage()
    {
        InitializeComponent();
    }

    void OnCancelClicked(object? sender, EventArgs e) => Status.Text = "Cancelled";

    void OnCreateClicked(object? sender, EventArgs e) => Status.Text = "Cluster created";
}
