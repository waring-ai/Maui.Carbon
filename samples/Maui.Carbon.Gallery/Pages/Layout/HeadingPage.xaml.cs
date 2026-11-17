using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Layout;

[GalleryPage("Heading", GalleryCategory.Layout, Order = 40,
    Summary = "A heading that picks its level from the enclosing sections, so content can be moved or reused without hand-picking heading levels.")]
public partial class HeadingPage : GalleryPage
{
    public HeadingPage()
    {
        InitializeComponent();

        // The same card at level 2 and, inside one more section, at level 3.
        CardRow.Add(new CdsSection { Body = Card("default", "3 nodes per zone") });
        CardRow.Add(new CdsSection { Body = new CdsSection { Body = Card("gpu-pool", "2 nodes in eu-de-1") } });
    }

    static View Card(string name, string description)
    {
        var card = new VerticalStackLayout { Spacing = 4, Padding = 16, WidthRequest = 240 };
        CdsTokens.SetColor(card, VisualElement.BackgroundColorProperty, CdsColorKeys.Layer01);
        card.Add(new CdsHeading(name));
        card.Add(new CdsText(description, CdsTypeStyle.Body01, CdsTextTone.Secondary));
        return card;
    }
}
