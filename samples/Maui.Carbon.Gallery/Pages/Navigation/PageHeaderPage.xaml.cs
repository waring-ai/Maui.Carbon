using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Navigation;

[GalleryPage("Page header", GalleryCategory.Navigation, Order = 70, Spec = "PageHeader",
    Summary = "The top-of-page header: a breadcrumb bar, the page title with optional icon and actions, subtitle and body text, and a tab bar with tags, all on layer-01.")]
public partial class PageHeaderPage : GalleryPage
{
    static readonly string[] s_panels = ["Members", "Policies", "Audit"];

    public PageHeaderPage()
    {
        InitializeComponent();
        // A placeholder that fills the 2 x 1 box, as in Carbon's preview: a layer-accent-01 field with a centred icon.
        var fill = new BoxView();
        CdsTokens.SetColor(fill, BoxView.ColorProperty, CdsColorKeys.LayerAccent01);
        var icon = new CdsIcon(CdsIconName.CloudFoundry1, 32) { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        CdsTokens.SetColor(icon, CdsIcon.ColorProperty, CdsColorKeys.IconSecondary);
        Hero.Image = new Grid { Children = { fill, icon } };
    }

    void OnTabsChanged(object? sender, CdsTabsSelectionChangedEventArgs e) =>
        Status.Text = $"Panel: {s_panels[Math.Clamp(e.NewIndex, 0, s_panels.Length - 1)]}";
}
