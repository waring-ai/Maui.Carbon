using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Feedback;

[GalleryPage("Badge indicator", GalleryCategory.Feedback, Order = 60, Spec = "BadgeIndicator",
    Summary = "A red dot or count pill pinned to the top-right corner of its parent, used on icon buttons for unread or pending items.")]
public partial class BadgeIndicatorPage : GalleryPage
{
    public BadgeIndicatorPage()
    {
        InitializeComponent();
    }

    int _count = 3;

    void OnNewClicked(object? sender, EventArgs e) => SetCount(_count + 1);

    void OnReadClicked(object? sender, EventArgs e) => SetCount(0);

    void SetCount(int count)
    {
        _count = count;
        Badge.Count = count;
        Status.Text = count == 0 ? "All read" : $"{count} unread";
    }
}
