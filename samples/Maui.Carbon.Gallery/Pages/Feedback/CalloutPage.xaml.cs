using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Callout", GalleryCategory.Feedback, Order = 40, Spec = "Callout",
    Summary = "A persistent, non-dismissible message placed in page content to highlight information the user should know before acting.")]
public partial class CalloutPage : GalleryPage
{
    public CalloutPage()
    {
        InitializeComponent();
    }

    void OnAction(object? sender, EventArgs e) => Status.Text = "Action used";
}
