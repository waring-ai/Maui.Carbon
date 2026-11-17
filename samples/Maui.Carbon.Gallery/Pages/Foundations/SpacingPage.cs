using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Foundations;

[GalleryPage("Spacing and size", GalleryCategory.Foundations, Order = 30,
    Summary = "The 8px mini-unit spacing scale and the control size scale. Never use off-scale values.")]
public sealed class SpacingPage : GalleryPage
{
    public SpacingPage()
    {
        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing03 };
        (string, double)[] spacing =
        [
            ("spacing-01", CdsSpacing.Spacing01), ("spacing-02", CdsSpacing.Spacing02), ("spacing-03", CdsSpacing.Spacing03),
            ("spacing-04", CdsSpacing.Spacing04), ("spacing-05", CdsSpacing.Spacing05), ("spacing-06", CdsSpacing.Spacing06),
            ("spacing-07", CdsSpacing.Spacing07), ("spacing-08", CdsSpacing.Spacing08), ("spacing-09", CdsSpacing.Spacing09),
            ("spacing-10", CdsSpacing.Spacing10), ("spacing-11", CdsSpacing.Spacing11), ("spacing-12", CdsSpacing.Spacing12),
            ("spacing-13", CdsSpacing.Spacing13),
        ];
        foreach (var (name, value) in spacing)
        {
            var bar = new BoxView { WidthRequest = value, HeightRequest = 16, HorizontalOptions = LayoutOptions.Start };
            bar.SetDynamicResource(BoxView.ColorProperty, CdsColorKeys.Interactive);
            var row = new Grid { ColumnSpacing = CdsSpacing.Spacing05 };
            row.ColumnDefinitions.Add(new ColumnDefinition(120));
            row.ColumnDefinitions.Add(new ColumnDefinition(60));
            row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            row.Add(new CdsText(name, CdsTypeStyle.Code02), 0);
            row.Add(new CdsText($"{value}", CdsTypeStyle.Code02, CdsTextTone.Secondary), 1);
            row.Add(bar, 2);
            stack.Add(row);
        }

        stack.Add(new CdsText("Control sizes", CdsTypeStyle.Heading03) { Margin = new Thickness(0, CdsSpacing.Spacing07, 0, 0) });
        var sizes = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing05 };
        foreach (var size in Enum.GetValues<CdsSize>())
            sizes.Add(new CdsButton { Text = size.ToString().ToLowerInvariant(), Size = size, Kind = CdsButtonKind.Tertiary, VerticalOptions = LayoutOptions.End });
        stack.Add(sizes);
        Body = stack;
    }
}
