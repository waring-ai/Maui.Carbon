using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Pagination", GalleryCategory.Data, Order = 70, Spec = "Pagination",
    Summary = "Lets people move through a long list: items-per-page select, x–y of z items, a page select with 'of N pages', and previous / next buttons.")]
public partial class PaginationPage : GalleryPage
{
    public PaginationPage()
    {
        InitializeComponent();
        Custom.ItemRangeText = (min, max, total) => $"{min}–{max} of {total} buckets";
    }

    void OnChanged(object? sender, CdsPaginationChangedEventArgs e)
    {
        var first = (e.Page - 1) * e.PageSize + 1;
        var last = Math.Min(e.Page * e.PageSize, Interactive.TotalItems);
        Status.Text = $"Showing items {first} to {last} (page {e.Page}, {e.PageSize} per page).";
    }
}
