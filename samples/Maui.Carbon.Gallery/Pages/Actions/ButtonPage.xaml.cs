using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Actions;

[GalleryPage("Button", GalleryCategory.Actions, Order = 10,
    Summary = "Triggers an action. Use one primary button per view, secondary or tertiary for supporting actions, ghost for low emphasis and danger for destructive steps.")]
public partial class ButtonPage : GalleryPage
{
    int _count;

    public ButtonPage()
    {
        InitializeComponent();
    }

    void OnCountClicked(object? sender, EventArgs e) => Status.Text = $"{++_count} nodes";

    void OnResetClicked(object? sender, EventArgs e)
    {
        _count = 0;
        Status.Text = "0 nodes";
    }
}
