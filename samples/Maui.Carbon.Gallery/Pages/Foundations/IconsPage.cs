using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Foundations;

[GalleryPage("Icons", GalleryCategory.Foundations, Order = 40,
    Summary = "The bundled Carbon glyphs at 16, 20, 24 and 32. Icons inherit icon-primary; status glyphs always sit next to a label.")]
public sealed class IconsPage : GalleryPage
{
    public IconsPage()
    {
        var flex = new WrapLayout();
        foreach (var icon in Enum.GetValues<CdsIconName>())
        {
            var sizes = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing04, HeightRequest = 32 };
            foreach (var s in new[] { 16d, 20, 24, 32 })
                sizes.Add(new CdsIcon(icon, s) { VerticalOptions = LayoutOptions.Center });
            var tile = new VerticalStackLayout { WidthRequest = 200, Spacing = CdsSpacing.Spacing03, Margin = new Thickness(0, 0, CdsSpacing.Spacing05, CdsSpacing.Spacing06) };
            tile.Add(sizes);
            tile.Add(new CdsText(CdsIcons.GetStem(icon), CdsTypeStyle.Code01, CdsTextTone.Secondary));
            flex.Add(tile);
        }
        Body = flex;
    }
}
