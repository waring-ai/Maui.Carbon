using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Overlays;

[GalleryPage("Popover", GalleryCategory.Overlays, Order = 30,
    Summary = "A small surface of controls anchored to a trigger, such as filters or settings.")]
public partial class PopoverPage : GalleryPage
{
    public PopoverPage()
    {
        InitializeComponent();
    }

    void OnFilterClicked(object? sender, EventArgs e) => FilterPopover.IsOpen = !FilterPopover.IsOpen;

    void OnContrastClicked(object? sender, EventArgs e) => ContrastPopover.IsOpen = !ContrastPopover.IsOpen;

    void OnZoneClicked(object? sender, EventArgs e) => ZonePopover.IsOpen = !ZonePopover.IsOpen;

    void OnApplyClicked(object? sender, EventArgs e)
    {
        FilterPopover.IsOpen = false;
        Status.Text = "Filters applied";
    }
}
