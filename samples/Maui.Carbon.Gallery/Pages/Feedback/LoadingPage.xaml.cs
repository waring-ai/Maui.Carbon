using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Loading", GalleryCategory.Feedback, Order = 70, Spec = "Loading",
    Summary = "A spinner that shows the app or a region is busy, optionally covering it with a dimmed overlay.")]
public partial class LoadingPage : GalleryPage
{
    public LoadingPage()
    {
        InitializeComponent();
    }

    async void OnRecalculate(object? sender, EventArgs e)
    {
        Status.Text = "Recalculating...";
        Overlay.IsActive = true;
        await Task.Delay(3000);
        Overlay.IsActive = false;
        Status.Text = "Done";
    }
}
