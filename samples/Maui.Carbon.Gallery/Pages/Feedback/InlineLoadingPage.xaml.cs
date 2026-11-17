using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Inline loading", GalleryCategory.Feedback, Order = 71, Spec = "InlineLoading",
    Summary = "A small spinner with status text for a short action in progress, usually in place of the button that started it, which settles to a success or error icon.")]
public partial class InlineLoadingPage : GalleryPage
{
    public InlineLoadingPage()
    {
        InitializeComponent();
    }

    async void OnSave(object? sender, EventArgs e)
    {
        SaveButton.IsVisible = false;
        Saving.IsVisible = true;
        Saving.Description = "Saving...";
        Saving.Status = CdsInlineLoadingStatus.Active;
        Status.Text = "Saving";
        await Task.Delay(1500);
        Saving.Description = "Saved";
        Saving.Status = CdsInlineLoadingStatus.Finished;
    }

    void OnSuccess(object? sender, EventArgs e)
    {
        Saving.IsVisible = false;
        SaveButton.IsVisible = true;
        Status.Text = "Saved";
    }
}
