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
        var image = new BoxView { HeightRequest = 100 };
        CdsTokens.SetColor(image, BoxView.ColorProperty, CdsColorKeys.TagBackgroundBlue);
        Hero.Image = image;
    }

    void OnTabsChanged(object? sender, CdsTabsSelectionChangedEventArgs e) =>
        Status.Text = $"Panel: {s_panels[Math.Clamp(e.NewIndex, 0, s_panels.Length - 1)]}";
}
