namespace Maui.Carbon.Tests.Components;

public class AILabelTokenTests
{
    [Theory]
    [InlineData(CdsAILabelSize.Mini, 16)]
    [InlineData(CdsAILabelSize.TwoXs, 20)]
    [InlineData(CdsAILabelSize.Xs, 24)]
    [InlineData(CdsAILabelSize.Sm, 32)]
    [InlineData(CdsAILabelSize.Md, 40)]
    [InlineData(CdsAILabelSize.Lg, 48)]
    [InlineData(CdsAILabelSize.Xl, 64)]
    public void Button_side_by_size(CdsAILabelSize size, double side) => Assert.Equal(side, CdsAILabelTokens.Side(size));

    [Theory]
    [InlineData(CdsAILabelSize.Mini, 9)]
    [InlineData(CdsAILabelSize.TwoXs, 12)]
    [InlineData(CdsAILabelSize.Xs, 12)]
    [InlineData(CdsAILabelSize.Sm, 16)]
    [InlineData(CdsAILabelSize.Lg, 16)]
    [InlineData(CdsAILabelSize.Xl, 20)]
    public void Font_size_by_size(CdsAILabelSize size, double font) => Assert.Equal(font, CdsAILabelTokens.FontSize(size));

    [Theory]
    [InlineData(CdsAILabelSize.Sm, 12)]
    [InlineData(CdsAILabelSize.Md, 14)]
    [InlineData(CdsAILabelSize.Lg, 16)]
    public void Inline_font_size(CdsAILabelSize size, double font) => Assert.Equal(font, CdsAILabelTokens.InlineFontSize(size));

    [Fact]
    public void Inline_square_grows_at_lg_and_xl()
    {
        Assert.Equal(4, CdsAILabelTokens.InlineDot(CdsAILabelSize.Md));
        Assert.Equal(8, CdsAILabelTokens.InlineDot(CdsAILabelSize.Lg));
        Assert.Equal(8, CdsAILabelTokens.InlineGap(CdsAILabelSize.Md));
        Assert.Equal(12, CdsAILabelTokens.InlineGap(CdsAILabelSize.Xl));
    }

    [Fact]
    public void Mini_and_2xs_get_a_24_hit_area()
    {
        Assert.Equal(24, CdsAILabelTokens.HitArea(CdsAILabelSize.Mini));
        Assert.Equal(24, CdsAILabelTokens.HitArea(CdsAILabelSize.TwoXs));
        Assert.Equal(32, CdsAILabelTokens.HitArea(CdsAILabelSize.Sm));
    }

    [Fact]
    public void Default_button_is_an_inverse_outline_that_fills_on_hover()
    {
        var rest = CdsAILabelTokens.Resolve(CdsAILabelKind.Default, false, CdsInteractionState.None);
        Assert.Null(rest.Background);
        Assert.Equal(CdsColorKeys.BorderInverse, rest.Border);
        Assert.Equal(CdsColorKeys.TextPrimary, rest.Text);
        Assert.Null(rest.InnerRing);
        var hover = CdsAILabelTokens.Resolve(CdsAILabelKind.Default, false, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.BorderInverse, hover.Background);
        Assert.Equal(CdsColorKeys.TextInverse, hover.Text);
    }

    [Fact]
    public void Default_button_focus_is_a_focus_border_plus_an_inset_ring()
    {
        var focus = CdsAILabelTokens.Resolve(CdsAILabelKind.Default, false, CdsInteractionState.FocusVisible);
        Assert.Equal(CdsColorKeys.Focus, focus.Border);
        Assert.Equal(CdsColorKeys.Focus, focus.InnerRing);
    }

    [Fact]
    public void Inline_button_has_no_box_until_it_has_text_or_is_hovered()
    {
        var rest = CdsAILabelTokens.Resolve(CdsAILabelKind.Inline, false, CdsInteractionState.None);
        Assert.Null(rest.Border);
        Assert.Equal(CdsColorKeys.IconPrimary, rest.Dot);
        var withText = CdsAILabelTokens.Resolve(CdsAILabelKind.Inline, true, CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.BorderInverse, withText.Border);
        var hover = CdsAILabelTokens.Resolve(CdsAILabelKind.Inline, false, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.IconSecondary, hover.Border);
        Assert.Equal(CdsColorKeys.TextSecondary, hover.Text);
        Assert.Equal(CdsColorKeys.IconSecondary, hover.Dot);
        var focus = CdsAILabelTokens.Resolve(CdsAILabelKind.Inline, false, CdsInteractionState.FocusVisible | CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.Focus, focus.Border);
    }

    [Fact]
    public void Accessible_name_is_the_ai_text_plus_the_label()
    {
        Assert.Equal("AI Show information", CdsAILabelTokens.AccessibleName("AI", "Show information", null, CdsAILabelKind.Default));
        Assert.Equal("AI Why bx2.8x32", CdsAILabelTokens.AccessibleName("AI", "Why bx2.8x32", "Generated", CdsAILabelKind.Default));
        Assert.Equal("AI Generated", CdsAILabelTokens.AccessibleName("AI", "Why", "Generated", CdsAILabelKind.Inline));
    }

    [Fact]
    public void Revert_button_size_follows_the_label_size()
    {
        Assert.Equal(CdsSize.Xs, CdsAILabelTokens.RevertSize(CdsAILabelSize.Mini));
        Assert.Equal(CdsSize.Xs, CdsAILabelTokens.RevertSize(CdsAILabelSize.Xs));
        Assert.Equal(CdsSize.Sm, CdsAILabelTokens.RevertSize(CdsAILabelSize.Sm));
        Assert.Equal(CdsSize.Md, CdsAILabelTokens.RevertSize(CdsAILabelSize.Md));
        Assert.Equal(CdsSize.Lg, CdsAILabelTokens.RevertSize(CdsAILabelSize.Xl));
    }
}

public class AiTreatmentTests
{
    [Fact]
    public void Tokens_are_carbons_ai_theme_tokens()
    {
        Assert.Equal(CdsColorKeys.AiAuraStart, CdsAiTokens.Aura);
        Assert.Equal(CdsColorKeys.AiAuraStartSm, CdsAiTokens.AuraSmall);
        Assert.Equal(CdsColorKeys.AiBorderStart, CdsAiTokens.BorderStart);
        Assert.Equal(CdsColorKeys.AiBorderEnd, CdsAiTokens.BorderEnd);
        Assert.Equal(CdsColorKeys.AiBorderStrong, CdsAiTokens.BorderStrong);
        Assert.Equal(CdsColorKeys.AiPopoverBackground, CdsAiTokens.CalloutBackground);
        Assert.Equal(CdsColorKeys.AiSkeletonBackground, CdsAiTokens.SkeletonBackground);
        Assert.Equal(CdsColorKeys.AiSkeletonElementBackground, CdsAiTokens.SkeletonElement);
        Assert.Equal(0.5, CdsAiTokens.SkeletonElementOpacity);
    }

    [Theory]
    [InlineData(CdsThemeName.White, "ai-aura-start", 69, 137, 255, 0.1)]
    [InlineData(CdsThemeName.G100, "ai-border-strong", 120, 169, 255, 1.0)]
    [InlineData(CdsThemeName.White, "ai-overlay", 0, 17, 65, 0.5)]
    public void Ai_token_values_match_carbon(CdsThemeName theme, string token, int r, int g, int b, double a)
    {
        var c = CdsPalettes.GetTokenColor(theme, token);
        Assert.Equal((r, g, b), ((int)Math.Round(c.Red * 255), (int)Math.Round(c.Green * 255), (int)Math.Round(c.Blue * 255)));
        Assert.Equal(a, c.Alpha, 2);
    }

    [Fact]
    public void A_translucent_aura_is_composited_over_the_surface()
    {
        var aura = new Color(69 / 255f, 137 / 255f, 1f, 0.1f);
        var fill = CdsAiTreatment.CreateFill(aura, Colors.White);
        var start = fill.GradientStops[0].Color;
        Assert.Equal(1f, start.Alpha, 3);
        Assert.Equal((0.9f * 1f) + (0.1f * 69 / 255f), start.Red, 3);
    }

    [Fact]
    public void Fill_rises_from_the_aura_at_the_bottom_into_the_surface_color()
    {
        var aura = Color.FromRgb(200, 220, 255);
        var surface = Color.FromRgb(255, 255, 255);
        var brush = CdsAiTreatment.CreateFill(aura, surface);
        Assert.Equal(new Point(0, 1), brush.StartPoint);
        Assert.Equal(new Point(0, 0), brush.EndPoint);
        Assert.Equal(3, brush.GradientStops.Count);
        Assert.Equal(aura, brush.GradientStops[0].Color);
        Assert.Equal(0f, brush.GradientStops[0].Offset);
        Assert.Equal(surface, brush.GradientStops[1].Color);
        Assert.Equal(surface, brush.GradientStops[2].Color);
    }

    [Fact]
    public void Border_runs_from_the_start_color_at_the_top_to_the_end_color()
    {
        var start = Color.FromRgb(208, 226, 255);
        var end = Color.FromRgb(15, 98, 254);
        var brush = CdsAiTreatment.CreateBorder(start, end);
        Assert.Equal(new Point(0, 0), brush.StartPoint);
        Assert.Equal(new Point(0, 1), brush.EndPoint);
        Assert.Equal(start, brush.GradientStops[0].Color);
        Assert.Equal(end, brush.GradientStops[1].Color);
    }

    [Fact]
    public void Treatment_can_be_applied_and_removed_without_a_platform()
    {
        var tile = new Border { StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle() };
        CdsAiTreatment.Apply(tile);
        Assert.True(tile.StrokeThickness >= 1);
        CdsAiTreatment.Remove(tile);
        CdsAiTreatment.SetIsActive(tile, true);
        Assert.True(CdsAiTreatment.GetIsActive(tile));
        CdsAiTreatment.SetIsActive(tile, false);
        Assert.False(CdsAiTreatment.GetIsActive(tile));
    }
}

public class AILabelControlTests
{
    [Fact]
    public void Defaults_match_the_spec()
    {
        var label = new CdsAILabel();
        Assert.Equal(CdsAILabelSize.Xs, label.Size);
        Assert.Equal(CdsAILabelKind.Default, label.Kind);
        Assert.Equal("AI", label.AiText);
        Assert.Equal("Show information", label.Label);
        Assert.Equal("Revert to AI input", label.RevertLabel);
        Assert.Equal(CdsPlacement.Bottom, label.Placement);
        Assert.False(label.IsOpen);
        Assert.False(label.IsRevertActive);
    }

    [Fact]
    public void Every_size_kind_and_option_can_be_set()
    {
        foreach (var size in Enum.GetValues<CdsAILabelSize>())
            foreach (var kind in Enum.GetValues<CdsAILabelKind>())
            {
                var label = new CdsAILabel { Size = size, Kind = kind, TextLabel = "Generated", AiText = "AI", Label = "Why" };
                label.IsRevertActive = true;
                label.IsRevertActive = false;
            }
    }

    [Fact]
    public void Open_before_load_is_remembered_and_can_be_closed()
    {
        var label = new CdsAILabel { Callout = new CdsAILabelContent { Body = new Label { Text = "Because." } } };
        label.IsOpen = true;
        Assert.True(label.IsOpen);
        label.IsOpen = false;
        Assert.False(label.IsOpen);
    }

    [Fact]
    public void Revert_event_is_exposed()
    {
        var label = new CdsAILabel { IsRevertActive = true };
        var raised = 0;
        label.RevertClicked += (_, _) => raised++;
        Assert.Equal(0, raised);
    }

    [Fact]
    public void Callout_preview_wraps_the_content()
    {
        var content = new CdsAILabelContent
        {
            Body = new Label { Text = "Explanation" },
            Actions = new CdsAILabelActions { new CdsButton("View details") },
        };
        var label = new CdsAILabel { Callout = content };
        var preview = label.CreateCalloutPreview();
        Assert.NotNull(preview);
        Assert.NotNull(content.Parent);
    }
}
