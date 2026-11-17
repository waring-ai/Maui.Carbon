using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Toast notification", GalleryCategory.Feedback, Order = 30, Spec = "ToastNotification",
    Summary = "A transient message that floats above the page to report the result of an action; CdsToast.Show puts it in the top-right toast region.")]
public partial class ToastNotificationPage : GalleryPage
{
    public ToastNotificationPage()
    {
        InitializeComponent();
    }
    int _count;

    void OnSuccess(object? sender, EventArgs e)
    {
        _count++;
        CdsToast.Show(this, new CdsToastOptions
        {
            Kind = CdsNotificationKind.Success,
            Title = "Deployment complete",
            Subtitle = $"checkout-api v2.4.{_count} is live in prod-us-east.",
            Caption = "Just now",
            Timeout = 5000,
            Closed = () => Status.Text = "Toast closed",
        });
        Status.Text = "Success toast shown (closes after 5 s)";
    }

    void OnError(object? sender, EventArgs e)
    {
        CdsToast.Show(this, new CdsToastOptions
        {
            Kind = CdsNotificationKind.Error,
            Title = "Deployment failed",
            Subtitle = "Image checkout-api:2.4 was not found. Check the tag and retry.",
            Caption = "Just now",
            Closed = () => Status.Text = "Toast closed",
        });
        Status.Text = "Error toast shown (stays until closed)";
    }
}
