using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Foundations;

[GalleryPage("Typography", GalleryCategory.Foundations, Order = 20,
    Summary = "The IBM Plex type scale. Productive styles for UI, body styles for paragraphs, headings for page structure.")]
public sealed class TypographyPage : GalleryPage
{
    public TypographyPage()
    {
        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing06 };
        foreach (var style in Enum.GetValues<CdsTypeStyle>())
        {
            var spec = CdsTypeSpec.Of(style);
            var row = new VerticalStackLayout { Spacing = CdsSpacing.Spacing02 };
            row.Add(new CdsText($"{spec.TokenName} · {spec.FontSize}/{spec.LineBoxHeight} · {spec.FontWeight}", CdsTypeStyle.Code01, CdsTextTone.Secondary));
            row.Add(new CdsText("The quick brown fox deploys a cluster", style));
            stack.Add(row);
        }
        Body = stack;
    }
}
