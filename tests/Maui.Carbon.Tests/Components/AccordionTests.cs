namespace Maui.Carbon.Tests.Components;

public class AccordionTests
{
    [Theory]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xs, 32)]
    [InlineData(CdsSize.Xl, 48)]
    public void Heading_height_follows_size(CdsSize size, double expected) =>
        Assert.Equal(expected, CdsAccordionMetrics.HeadingHeight(size));

    [Theory]
    [InlineData(300, 16)]
    [InlineData(479, 16)]
    [InlineData(480, 48)]
    [InlineData(639, 48)]
    [InlineData(640, 160)]
    [InlineData(800, 200)]
    public void Content_end_padding_steps_with_the_width(double width, double expected) =>
        Assert.Equal(expected, CdsAccordionMetrics.ContentEndPadding(width));

    [Fact]
    public void Start_aligned_content_is_inset_past_the_chevron()
    {
        Assert.Equal(16, CdsAccordionMetrics.ContentStartPadding(CdsAccordionAlign.End));
        Assert.Equal(48, CdsAccordionMetrics.ContentStartPadding(CdsAccordionAlign.Start));
    }

    [Fact]
    public void Chevron_points_down_closed_and_up_open()
    {
        Assert.Equal(0, CdsAccordionMetrics.ChevronRotation(false));
        Assert.Equal(180, CdsAccordionMetrics.ChevronRotation(true));
    }

    [Fact]
    public void Tokens_cover_hover_and_disabled()
    {
        Assert.Equal(CdsColorKeys.LayerHover01, CdsAccordionTokens.Heading(true, false));
        Assert.Null(CdsAccordionTokens.Heading(true, true));
        Assert.Null(CdsAccordionTokens.Heading(false, false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsAccordionTokens.Title(true));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsAccordionTokens.Title(false));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsAccordionTokens.Chevron(true));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsAccordionTokens.Chevron(false));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsAccordionTokens.Divider(false));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsAccordionTokens.Divider(true));
    }

    [Fact]
    public void Accordion_holds_items_and_survives_reconfiguration()
    {
        var accordion = new CdsAccordion { Size = CdsSize.Lg, Align = CdsAccordionAlign.Start, IsFlush = true };
        var first = new CdsAccordionItem { Title = "How is node usage billed?", IsOpen = true, Body = new Label { Text = "Per hour." } };
        var second = new CdsAccordionItem { Title = "Can I change a region?", IsEnabled = false };
        accordion.Items.Add(first);
        accordion.Items.Add(second);
        Assert.Equal(2, accordion.Items.Count);
        Assert.True(first.IsOpen);
        accordion.IsEnabled = false;
        accordion.IsEnabled = true;
        accordion.Items.Remove(second);
        Assert.Single(accordion.Items);
    }

    [Fact]
    public void Several_items_can_be_open_at_once()
    {
        var accordion = new CdsAccordion();
        var a = new CdsAccordionItem { Title = "A" };
        var b = new CdsAccordionItem { Title = "B" };
        accordion.Items.Add(a);
        accordion.Items.Add(b);
        a.IsOpen = true;
        b.IsOpen = true;
        Assert.True(a.IsOpen && b.IsOpen);
    }
}
