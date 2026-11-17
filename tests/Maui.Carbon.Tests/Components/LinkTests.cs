namespace Maui.Carbon.Tests.Components;

public class LinkTokenTests
{
    [Fact]
    public void Default_link_is_link_primary_without_underline()
    {
        var v = CdsLinkTokens.Resolve(CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.LinkPrimary, v.Foreground);
        Assert.False(v.Underline);
    }

    [Fact]
    public void Hover_uses_the_hover_color_and_underlines()
    {
        var v = CdsLinkTokens.Resolve(CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.LinkPrimaryHover, v.Foreground);
        Assert.True(v.Underline);
    }

    [Fact]
    public void Pressed_is_text_primary_and_underlined()
    {
        var v = CdsLinkTokens.Resolve(CdsInteractionState.Pressed | CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.TextPrimary, v.Foreground);
        Assert.True(v.Underline);
    }

    [Fact]
    public void Visited_applies_only_when_opted_in_and_hover_wins()
    {
        Assert.Equal(CdsColorKeys.LinkPrimary, CdsLinkTokens.Resolve(CdsInteractionState.None).Foreground);
        Assert.Equal(CdsColorKeys.LinkVisited, CdsLinkTokens.Resolve(CdsInteractionState.None, isVisited: true).Foreground);
        Assert.Equal(CdsColorKeys.LinkPrimaryHover, CdsLinkTokens.Resolve(CdsInteractionState.Hover, isVisited: true).Foreground);
    }

    [Fact]
    public void Inline_links_are_always_underlined()
    {
        Assert.True(CdsLinkTokens.Resolve(CdsInteractionState.None, isInline: true).Underline);
        Assert.True(CdsLinkTokens.Resolve(CdsInteractionState.None, isVisited: true, isInline: true).Underline);
    }

    [Fact]
    public void Disabled_is_text_disabled_and_plain()
    {
        var v = CdsLinkTokens.Resolve(CdsInteractionState.Disabled | CdsInteractionState.Hover, isInline: true);
        Assert.Equal(CdsColorKeys.TextDisabled, v.Foreground);
        Assert.False(v.Underline);
    }

    [Fact]
    public void Inverse_links_use_the_inverse_family()
    {
        Assert.Equal(CdsColorKeys.LinkInverse, CdsLinkTokens.Resolve(CdsInteractionState.None, isInverse: true).Foreground);
        Assert.Equal(CdsColorKeys.LinkInverseHover, CdsLinkTokens.Resolve(CdsInteractionState.Hover, isInverse: true).Foreground);
        Assert.Equal(CdsColorKeys.LinkInverseVisited, CdsLinkTokens.Resolve(CdsInteractionState.None, isVisited: true, isInverse: true).Foreground);
    }

    [Theory]
    [InlineData(CdsLinkSize.Sm, CdsTypeStyle.Label01)]
    [InlineData(CdsLinkSize.Md, CdsTypeStyle.BodyCompact01)]
    [InlineData(CdsLinkSize.Lg, CdsTypeStyle.BodyCompact02)]
    public void Sizes_map_to_type_styles(CdsLinkSize size, CdsTypeStyle expected) => Assert.Equal(expected, CdsLinkTokens.TypeStyle(size));

    [Fact]
    public void Controls_construct_and_toggle_the_span_state()
    {
        var link = new CdsLink("Service status page", "https://status.example.com") { Icon = CdsIconName.Launch, Size = CdsLinkSize.Lg, IsVisited = true };
        _ = new CdsLink { Text = "volume retention", IsInline = true, IsEnabled = false };
        var span = new CdsLinkSpan { Href = "https://example.com", Text = "volume retention" };
        Assert.Equal(TextDecorations.Underline, span.TextDecorations);
        span.IsEnabled = false;
        Assert.Equal(TextDecorations.None, span.TextDecorations);
        Assert.Equal("Service status page", link.Text);
    }
}
