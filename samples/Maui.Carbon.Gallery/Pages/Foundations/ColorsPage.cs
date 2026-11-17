using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Foundations;

[GalleryPage("Color tokens", GalleryCategory.Foundations, Order = 10,
    Summary = "All 197 Carbon color tokens of the current theme. Switch themes in the flyout; every swatch is a dynamic resource.")]
public sealed class ColorsPage : GalleryPage
{
    public ColorsPage()
    {
        var flex = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, JustifyContent = Microsoft.Maui.Layouts.FlexJustify.Start };
        for (var i = 0; i < CdsColorKeys.All.Count; i++)
        {
            var key = CdsColorKeys.All[i];
            var swatch = new BoxView { HeightRequest = 40 };
            swatch.SetDynamicResource(BoxView.ColorProperty, key);
            var tile = new VerticalStackLayout { WidthRequest = 176, Margin = new Thickness(0, 0, CdsSpacing.Spacing05, CdsSpacing.Spacing05), Spacing = CdsSpacing.Spacing02 };
            var frame = new Border { StrokeThickness = 1, Content = swatch, Padding = 0 };
            CdsTokens.SetBrush(frame, Border.StrokeProperty, CdsColorKeys.BorderSubtle01);
            tile.Add(frame);
            tile.Add(new CdsText(CdsColorKeys.TokenNames[i], CdsTypeStyle.Code01));
            flex.Add(tile);
        }
        Body = flex;
    }
}
