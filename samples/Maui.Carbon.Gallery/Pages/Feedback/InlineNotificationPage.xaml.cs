using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Inline notification", GalleryCategory.Feedback, Order = 10, Spec = "InlineNotification",
    Summary = "An in-context message about the state of the page or of a task: 3px colored start edge, status icon, title, subtitle and an optional close button.")]
public partial class InlineNotificationPage : GalleryPage
{
    public InlineNotificationPage()
    {
        InitializeComponent();
    }

    void OnCloseRequested(object? sender, CdsNotificationCloseEventArgs e) => Status.Text = "Closing...";

    void OnClosed(object? sender, EventArgs e) => Status.Text = "Closed";

    void OnShowAgain(object? sender, EventArgs e)
    {
        Demo.IsVisible = true;
        Status.Text = "Open";
    }
}
