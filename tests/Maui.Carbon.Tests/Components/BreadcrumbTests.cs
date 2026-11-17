namespace Maui.Carbon.Tests.Components;

public class BreadcrumbModelTests
{
    [Theory]
    [InlineData(3, 0, 0, 0)]
    [InlineData(3, 4, 0, 0)]
    [InlineData(4, 4, 0, 0)]
    [InlineData(5, 4, 1, 2)]
    [InlineData(6, 4, 1, 3)]
    [InlineData(7, 5, 1, 3)]
    [InlineData(5, 3, 1, 3)]
    [InlineData(4, 2, 1, 2)]
    public void Keeps_the_first_level_and_the_last_two_and_collapses_the_middle(int count, int max, int start, int collapsed) =>
        Assert.Equal((start, collapsed), BreadcrumbModel.Collapse(count, max));

    [Fact]
    public void Collapses_nothing_when_a_single_level_would_be_replaced_by_the_overflow_crumb_only()
    {
        Assert.Equal((0, 0), BreadcrumbModel.Collapse(2, 1));
        Assert.Equal((0, 0), BreadcrumbModel.Collapse(0, 3));
    }
}

public class BreadcrumbTokenTests
{
    [Fact]
    public void Links_are_link_primary_hover_underlines_pressed_is_text_primary()
    {
        Assert.Equal(new CdsBreadcrumbVisuals(CdsColorKeys.LinkPrimary, false), CdsBreadcrumbTokens.Resolve(CdsInteractionState.None, true));
        Assert.Equal(new CdsBreadcrumbVisuals(CdsColorKeys.LinkPrimaryHover, true), CdsBreadcrumbTokens.Resolve(CdsInteractionState.Hover, true));
        Assert.Equal(new CdsBreadcrumbVisuals(CdsColorKeys.TextPrimary, true), CdsBreadcrumbTokens.Resolve(CdsInteractionState.Pressed, true));
    }

    [Fact]
    public void Current_page_and_plain_text_are_text_primary_and_never_underline()
    {
        Assert.Equal(new CdsBreadcrumbVisuals(CdsColorKeys.TextPrimary, false), CdsBreadcrumbTokens.Resolve(CdsInteractionState.Hover, false));
    }

    [Fact]
    public void Sizes()
    {
        Assert.Equal(CdsTypeStyle.BodyCompact01, CdsBreadcrumbTokens.TypeStyle(CdsBreadcrumbSize.Md));
        Assert.Equal(CdsTypeStyle.Label01, CdsBreadcrumbTokens.TypeStyle(CdsBreadcrumbSize.Sm));
        Assert.Equal(8, CdsBreadcrumbTokens.SeparatorGap(CdsBreadcrumbSize.Md));
        Assert.Equal(4, CdsBreadcrumbTokens.SeparatorGap(CdsBreadcrumbSize.Sm));
    }
}

public class BreadcrumbControlTests
{
    [Fact]
    public void Builds_with_items_overflow_and_current_page()
    {
        var overflow = new CdsBreadcrumbItem();
        overflow.OverflowItems.Add(new CdsBreadcrumbItem("Kubernetes", "https://example.com/k"));
        overflow.OverflowItems.Add(new CdsBreadcrumbItem("Clusters"));
        Assert.True(overflow.IsOverflow);
        var trail = new CdsBreadcrumb
        {
            Size = CdsBreadcrumbSize.Sm,
            NoTrailingSlash = true,
            Items =
            {
                new CdsBreadcrumbItem("Dashboard", "https://example.com"),
                overflow,
                new CdsBreadcrumbItem { Text = "prod-us-south", IsCurrentPage = true },
            },
        };
        Assert.Equal(3, trail.Items.Count);
    }

    [Fact]
    public void Max_visible_items_collapses_the_middle_without_losing_items()
    {
        var trail = new CdsBreadcrumb { MaxVisibleItems = 4 };
        for (var i = 1; i <= 6; i++) trail.Items.Add(new CdsBreadcrumbItem($"Level {i}", $"https://example.com/{i}"));
        Assert.Equal(6, trail.Items.Count);
        trail.MaxVisibleItems = 0;
        trail.MaxVisibleItems = -3;
        Assert.Equal(0, trail.MaxVisibleItems);
    }

    [Fact]
    public void Activation_runs_click_and_command()
    {
        var clicks = 0;
        var commands = 0;
        var item = new CdsBreadcrumbItem { Text = "Clusters", Command = new Command(() => commands++) };
        item.Clicked += (_, _) => clicks++;
        item.Activate();
        Assert.Equal((1, 1), (clicks, commands));
    }
}
