using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.AI;

[GalleryPage("AI label", GalleryCategory.AI, Order = 10, Spec = "AILabel",
    Summary = "Marks content or a field as AI-generated: a small \"AI\" button that opens an explainability callout.")]
public partial class AILabelPage : GalleryPage
{
    public AILabelPage()
    {
        InitializeComponent();

        foreach (var row in new[] { SizesRow, InlineRow })
            foreach (var label in row.Children.OfType<CdsAILabel>())
                label.Callout = CreateContent(withActions: false);

        TileLabel.Callout = CreateContent(withActions: false);

        var preview = new CdsAILabel { Size = CdsAILabelSize.Xs, Label = "Why bx2.8x32", Callout = CreateContent(withActions: true) };
        CalloutHost.Content = preview.CreateCalloutPreview();
    }

    static CdsAILabelContent CreateContent(bool withActions)
    {
        var body = new VerticalStackLayout { Spacing = CdsSpacing.Spacing05 };
        body.Add(new CdsText("AI explained", CdsTypeStyle.Label01, CdsTextTone.Secondary));
        body.Add(new CdsText("84%", CdsTypeStyle.Heading05));
        body.Add(new CdsText("Confidence score", CdsTypeStyle.Label01, CdsTextTone.Secondary));
        body.Add(new CdsText("Suggested because two nodes ran out of memory last week. The recommendation compares 30 days of usage against the three closest profiles.", CdsTypeStyle.Body01) { LineBreakMode = LineBreakMode.WordWrap });
        body.Add(new CdsText("Model type", CdsTypeStyle.Label01, CdsTextTone.Secondary));
        body.Add(new CdsText("Capacity forecaster v2", CdsTypeStyle.Body01));

        var content = new CdsAILabelContent { Body = body };
        if (withActions)
        {
            content.Actions = new CdsAILabelActions
            {
                new CdsIconButton(CdsIconName.ThumbsUp, "Helpful", CdsIconButtonKind.Ghost),
                new CdsIconButton(CdsIconName.ThumbsDown, "Not helpful", CdsIconButtonKind.Ghost),
                new CdsButton("View details"),
            };
        }
        return content;
    }

    void OnClosed(object? sender, EventArgs e) => Status.Text = "Callout closed";

    void OnOpenClicked(object? sender, EventArgs e)
    {
        Live.IsOpen = true;
        Status.Text = "Callout open";
    }

    void OnEditClicked(object? sender, EventArgs e)
    {
        CdsAiTreatment.SetIsActive(AiTile, false);
        TileLabel.IsRevertActive = true;
        TileStatus.Text = "Edited: AI styling removed, revert shown";
    }

    void OnRevertClicked(object? sender, EventArgs e)
    {
        TileLabel.IsRevertActive = false;
        CdsAiTreatment.SetIsActive(AiTile, true);
        TileStatus.Text = "AI suggestion restored";
    }
}
