using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Operational tag", GalleryCategory.Feedback, Order = 52, Spec = "OperationalTag",
    Summary = "A clickable tag that triggers an action, such as opening a popover with more detail or the full list behind a \"+3\" tag.")]
public partial class OperationalTagPage : GalleryPage
{
    public OperationalTagPage()
    {
        InitializeComponent();
    }

    void OnMoreClicked(object? sender, EventArgs e)
    {
        More.IsOpen = !More.IsOpen;
        MoreTag.IsExpanded = More.IsOpen;
        Status.Text = More.IsOpen ? "Open" : "Closed";
    }
}
