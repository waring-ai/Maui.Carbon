using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Actionable notification", GalleryCategory.Feedback, Order = 20, Spec = "ActionableNotification",
    Summary = "A notification with one action button, in a toast layout (stacked, 288 wide) or an inline layout (full width, ghost action).")]
public partial class ActionableNotificationPage : GalleryPage
{
    public ActionableNotificationPage()
    {
        InitializeComponent();
    }

    void OnAction(object? sender, EventArgs e) => Status.Text = "Update started";
}
