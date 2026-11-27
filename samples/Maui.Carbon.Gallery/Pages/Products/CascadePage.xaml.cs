using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Cascade", GalleryCategory.Products, Order = 20, Spec = "Cascade",
    Summary = "Staggered entrance of cards, tiles and similar blocks in the main area of a page.")]
public partial class CascadePage : GalleryPage
{
    public CascadePage()
    {
        InitializeComponent();
    }

    void OnReplayClicked(object? sender, EventArgs e)
    {
        StackCascade.Replay();
        GridCascade.Replay();
        Status.Text = $"Replayed {StackCascade.Items.Count} items";
    }

    void OnAddClicked(object? sender, EventArgs e)
    {
        var tile = new CdsTile
        {
            Body = new VerticalStackLayout
            {
                Spacing = CdsSpacing.Spacing03,
                Children =
                {
                    new CdsText("Review access", CdsTypeStyle.Heading02),
                    new CdsText("Check who can read each topic.", CdsTypeStyle.Body01, CdsTextTone.Secondary),
                },
            },
        };
        StackCascade.Items.Add(tile);
        Status.Text = $"{StackCascade.Items.Count} items";
    }
}
