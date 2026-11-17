using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Navigation;

[GalleryPage("Header panel", GalleryCategory.Navigation, Order = 60, Spec = "HeaderPanel",
    Summary = "A 256px panel that slides in from the right edge under the 48px header, opened by a header action; it holds a switcher of product links or your own content.")]
public partial class HeaderPanelPage : GalleryPage
{
    public HeaderPanelPage()
    {
        InitializeComponent();
        foreach (var frame in new[] { Frame1, Frame2, Frame3 })
        {
            frame.StrokeThickness = 1;
            CdsTokens.SetBrush(frame, Microsoft.Maui.Controls.Border.StrokeProperty, CdsColorKeys.BorderSubtle01);
        }

        // Standalone: a tap on the page behind a switcher panel closes it (inside CdsUIShell the shell does this).
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => SetOpen(false);
        Catcher.GestureRecognizers.Add(tap);
        Panel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(CdsHeaderPanel.IsExpanded)) return;
            SwitcherAction.IsActive = Panel.IsExpanded;
            Catcher.IsVisible = Panel.IsExpanded;
            Status.Text = Panel.IsExpanded ? "Panel open" : "Panel closed";
        };
    }

    void SetOpen(bool open) => Panel.IsExpanded = open;

    void OnSwitcherClicked(object? sender, EventArgs e) => SetOpen(!Panel.IsExpanded);

    void OnItemClicked(object? sender, EventArgs e)
    {
        if (sender is not CdsSwitcherItem item) return;
        Status.Text = $"Switched to {item.Text}";
        SetOpen(false);
    }
}
