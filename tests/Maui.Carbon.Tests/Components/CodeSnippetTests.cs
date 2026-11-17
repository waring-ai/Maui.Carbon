namespace Maui.Carbon.Tests.Components;

public class CodeSnippetTests
{
    [Fact]
    public void Lines_are_split_on_every_line_ending()
    {
        Assert.Empty(CdsCodeSnippetModel.SplitLines(null));
        Assert.Empty(CdsCodeSnippetModel.SplitLines(""));
        Assert.Equal(["a"], CdsCodeSnippetModel.SplitLines("a"));
        Assert.Equal(["a", "b", "c"], CdsCodeSnippetModel.SplitLines("a\nb\r\nc"));
        Assert.Equal(["a", "b"], CdsCodeSnippetModel.SplitLines("a\rb"));
        Assert.Equal(3, CdsCodeSnippetModel.LineCount("a\nb\n"));
    }

    [Theory]
    [InlineData(16, false, 48)]    // fewer than the minimum rows: padded to three rows
    [InlineData(100, false, 100)]
    [InlineData(400, false, 240)]  // capped at fifteen rows
    [InlineData(400, true, 400)]   // expanded: unlimited
    [InlineData(100, true, 256)]   // expanded: at least sixteen rows
    public void Viewport_height_is_clamped_to_the_row_limits(double content, bool expanded, double expectedHeight) =>
        Assert.Equal(expectedHeight, CdsCodeSnippetModel.ViewportHeight(content, expanded, 3, 15, 16, 0));

    [Fact]
    public void Expanded_height_respects_a_maximum()
    {
        Assert.Equal(20 * 16, CdsCodeSnippetModel.ViewportHeight(1000, true, 3, 15, 16, 20));
        Assert.Equal(16 * 16, CdsCodeSnippetModel.ViewportHeight(1000, true, 3, 15, 16, 10));
    }

    [Fact]
    public void Expand_button_only_shows_for_code_taller_than_the_collapsed_maximum()
    {
        Assert.False(CdsCodeSnippetModel.ShowsExpandButton(240, 15));
        Assert.True(CdsCodeSnippetModel.ShowsExpandButton(241, 15));
        Assert.False(CdsCodeSnippetModel.ShowsExpandButton(40, 15));
    }

    [Fact]
    public void Expand_text_and_chevron_follow_state()
    {
        Assert.Equal("Show more", CdsCodeSnippetModel.ExpandText(false, "Show more", "Show less"));
        Assert.Equal("Show less", CdsCodeSnippetModel.ExpandText(true, "Show more", "Show less"));
        Assert.Equal(0, CdsCodeSnippetModel.ChevronRotation(false));
        Assert.Equal(180, CdsCodeSnippetModel.ChevronRotation(true));
    }

    [Fact]
    public void Copy_text_replaces_the_code()
    {
        Assert.Equal("code", CdsCodeSnippetModel.TextToCopy("code", null));
        Assert.Equal("other", CdsCodeSnippetModel.TextToCopy("code", "other"));
        Assert.Equal("", CdsCodeSnippetModel.TextToCopy(null, null));
    }

    [Fact]
    public void Fill_follows_state_and_disabled_stays_flat()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsCodeSnippetTokens.Fill(CdsInteractionState.None));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsCodeSnippetTokens.Fill(CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.LayerActive01, CdsCodeSnippetTokens.Fill(CdsInteractionState.Hover | CdsInteractionState.Pressed));
        Assert.Equal(CdsColorKeys.Layer01, CdsCodeSnippetTokens.Fill(CdsInteractionState.Disabled | CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.Layer01, CdsCodeSnippetTokens.PlainFill);
    }

    [Fact]
    public void Text_icon_and_border_tokens()
    {
        Assert.Equal(CdsColorKeys.TextPrimary, CdsCodeSnippetTokens.Text(false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsCodeSnippetTokens.Text(true));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsCodeSnippetTokens.Icon(false));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsCodeSnippetTokens.Icon(true));
        Assert.Equal(CdsColorKeys.Focus, CdsCodeSnippetTokens.InlineBorder(CdsInteractionState.FocusVisible));
        Assert.Null(CdsCodeSnippetTokens.InlineBorder(CdsInteractionState.Hover));
    }

    [Theory]
    [InlineData(CdsCodeSnippetType.Single)]
    [InlineData(CdsCodeSnippetType.Inline)]
    [InlineData(CdsCodeSnippetType.Multi)]
    public void Every_type_can_be_created_and_reconfigured(CdsCodeSnippetType type)
    {
        var snippet = new CdsCodeSnippet { Code = "cloud login --sso", Type = type };
        snippet.HideCopyButton = true;
        snippet.IsWrapText = true;
        snippet.HideCopyButton = false;
        snippet.IsExpanded = true;
        snippet.MaxCollapsedNumberOfRows = 5;
        snippet.Feedback = "Copied to clipboard";
        snippet.IsEnabled = false;
        Assert.Equal(type, snippet.Type);
        Assert.Equal(2000, snippet.FeedbackTimeout);
        Assert.Equal("Copy to clipboard", snippet.CopyButtonDescription);
        Assert.Equal(CdsPlacement.BottomEnd, snippet.FeedbackPlacement);
    }
}
