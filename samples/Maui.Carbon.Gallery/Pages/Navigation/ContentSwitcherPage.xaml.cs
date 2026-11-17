using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Navigation;

[GalleryPage("Content switcher", GalleryCategory.Navigation, Order = 40, Spec = "ContentSwitcher",
    Summary = "A segmented control for switching between two or more alternate views of the same content; one switch is always selected.")]
public partial class ContentSwitcherPage : GalleryPage
{
    public ContentSwitcherPage()
    {
        InitializeComponent();

        // A selected, disabled segment fills with button-disabled.
        DisabledSelected.Items[1].IsEnabled = false;
        DisabledSelected.SelectedIndex = 1;
    }

    void OnSelectionChanged(object? sender, CdsSwitchSelectionChangedEventArgs e) =>
        Status.Text = $"Selected {e.Text} (name {e.Name}, index {e.Index})";
}
