using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Containers;

[GalleryPage("Selectable tile", GalleryCategory.Containers, Order = 12, Spec = "SelectableTile",
    Summary = "A tile that toggles a selected state, for picking any number of cards from a set.")]
public partial class SelectableTilePage : GalleryPage
{
    public SelectableTilePage()
    {
        InitializeComponent();
    }

    void OnSelectionChanged(object? sender, CdsChoiceChangedEventArgs<bool> e)
    {
        var total = (LogsTile.IsSelected ? 20 : 0) + (BackupsTile.IsSelected ? 35 : 0);
        var count = (LogsTile.IsSelected ? 1 : 0) + (BackupsTile.IsSelected ? 1 : 0);
        Status.Text = count == 0 ? "No add-ons selected" : $"{count} add-on{(count == 1 ? "" : "s")} selected: ${total} per month";
    }
}
