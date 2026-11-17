using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Progress indicator", GalleryCategory.Feedback, Order = 73, Spec = "ProgressIndicator",
    Summary = "Shows the steps of a multi-step flow and where the user is in it, horizontally or vertically.")]
public partial class ProgressIndicatorPage : GalleryPage
{
    public ProgressIndicatorPage()
    {
        InitializeComponent();
    }

    void OnStepRequested(object? sender, CdsProgressStepRequestedEventArgs e) => e.Cancel = e.Index > Demo.CurrentIndex;

    void OnIndexChanged(object? sender, ValueChangedEventArgs e) => Status.Text = $"Step {Demo.CurrentIndex + 1} of {Demo.Steps.Count}";

    void OnBack(object? sender, EventArgs e) => Demo.CurrentIndex = Math.Max(0, Demo.CurrentIndex - 1);

    void OnNext(object? sender, EventArgs e) => Demo.CurrentIndex = Math.Min(Demo.Steps.Count - 1, Demo.CurrentIndex + 1);
}
