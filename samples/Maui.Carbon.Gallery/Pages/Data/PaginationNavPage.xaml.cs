using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Pagination nav", GalleryCategory.Data, Order = 80, Spec = "PaginationNav",
    Summary = "Numbered page buttons with previous / next arrows, for navigating pages of content rather than table rows.")]
public partial class PaginationNavPage : GalleryPage
{
    public PaginationNavPage() => InitializeComponent();

    void OnChanged(object? sender, CdsPageChangedEventArgs e) => Status.Text = $"Page {e.Page + 1} of {Interactive.TotalItems}";
}
