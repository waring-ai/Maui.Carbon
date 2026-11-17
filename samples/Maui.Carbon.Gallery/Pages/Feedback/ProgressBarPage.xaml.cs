using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Progress bar", GalleryCategory.Feedback, Order = 72, Spec = "ProgressBar",
    Summary = "A horizontal bar showing how far a task has progressed, or that it is running when the amount is unknown.")]
public partial class ProgressBarPage : GalleryPage
{
    public ProgressBarPage()
    {
        InitializeComponent();
    }

    bool _running;

    async void OnRun(object? sender, EventArgs e)
    {
        if (_running) return;
        _running = true;
        Demo.Status = CdsProgressStatus.Active;
        for (var rows = 0; rows <= 100; rows += 10)
        {
            Demo.Value = rows;
            Demo.HelperText = $"{rows} of 100 rows";
            await Task.Delay(300);
        }
        Demo.Status = CdsProgressStatus.Finished;
        Demo.HelperText = "Report exported";
        _running = false;
    }
}
