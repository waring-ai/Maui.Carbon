namespace Maui.Carbon.Tests.Components;

public class CopyButtonTests
{
    [Fact]
    public void Fill_is_layer_01_with_hover_and_active_steps()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsCopyButtonTokens.Resolve(CdsInteractionState.None).Background);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsCopyButtonTokens.Resolve(CdsInteractionState.Hover).Background);
        Assert.Equal(CdsColorKeys.LayerActive01, CdsCopyButtonTokens.Resolve(CdsInteractionState.Pressed | CdsInteractionState.Hover).Background);
    }

    [Fact]
    public void The_glyph_is_icon_primary_and_icon_disabled_when_disabled_and_there_is_no_border()
    {
        var rest = CdsCopyButtonTokens.Resolve(CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.IconPrimary, rest.Icon);
        Assert.Null(rest.Border);
        var disabled = CdsCopyButtonTokens.Resolve(CdsInteractionState.Disabled | CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.IconDisabled, disabled.Icon);
        Assert.Equal(CdsColorKeys.Layer01, disabled.Background);
    }

    [Theory]
    [InlineData(CdsSize.Xs, 32)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    public void Side_clamps_to_sm_through_lg(CdsSize size, double side) => Assert.Equal(side, CdsCopyButtonTokens.Side(size));

    [Fact]
    public void Feedback_starts_on_copy_and_expires_with_its_own_token()
    {
        var model = new CopyFeedbackModel();
        Assert.False(model.IsCopied);
        var first = model.Start();
        Assert.True(model.IsCopied);
        Assert.True(model.Expire(first));
        Assert.False(model.IsCopied);
        Assert.False(model.Expire(first));
    }

    [Fact]
    public void A_second_copy_restarts_the_window_and_the_first_expiry_is_ignored()
    {
        var model = new CopyFeedbackModel();
        var first = model.Start();
        var second = model.Start();
        Assert.False(model.Expire(first));
        Assert.True(model.IsCopied);
        Assert.True(model.Expire(second));
        Assert.False(model.IsCopied);
    }

    [Fact]
    public void Reset_ends_the_feedback_and_invalidates_pending_expiries()
    {
        var model = new CopyFeedbackModel();
        var token = model.Start();
        model.Reset();
        Assert.False(model.IsCopied);
        Assert.False(model.Expire(token));
    }

    [Theory]
    [InlineData(false, null, null, "Copy to clipboard")]
    [InlineData(false, "", "x", "Copy to clipboard")]
    [InlineData(false, "Copy key ID", "Done", "Copy key ID")]
    [InlineData(true, "Copy key ID", "Done", "Done")]
    [InlineData(true, "Copy key ID", null, "Copied!")]
    [InlineData(true, null, "", "Copied!")]
    public void Tooltip_text_switches_between_description_and_feedback(bool copied, string? description, string? feedback, string expected) =>
        Assert.Equal(expected, CopyFeedbackModel.TooltipText(copied, description, feedback));

    [Fact]
    public void Defaults_match_the_readme_and_setting_properties_does_not_throw()
    {
        var button = new CdsCopyButton();
        Assert.Equal("Copy to clipboard", button.IconDescription);
        Assert.Equal("Copied!", button.Feedback);
        Assert.Equal(2000, button.FeedbackTimeout);
        Assert.Equal(CdsSize.Md, button.Size);
        Assert.Equal(CdsPlacement.Bottom, button.TooltipPlacement);
        button.Text = "key-123";
        button.Size = CdsSize.Xs;
        Assert.Equal(CdsSize.Sm, button.Size);
        button.FeedbackTimeout = -5;
        Assert.Equal(0, button.FeedbackTimeout);
        button.IconDescription = "Copy key ID";
        button.IsEnabled = false;
    }
}
