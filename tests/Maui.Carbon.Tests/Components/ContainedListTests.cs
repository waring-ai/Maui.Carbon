namespace Maui.Carbon.Tests.Components;

public class ContainedListTests
{
    [Theory]
    [InlineData(CdsSize.Sm, 32, 32)]
    [InlineData(CdsSize.Md, 40, 40)]
    [InlineData(CdsSize.Lg, 48, 48)]
    [InlineData(CdsSize.Xl, 64, 48)]
    public void Height_and_action_height_follow_size(CdsSize size, double height, double action)
    {
        Assert.Equal(height, CdsContainedListMetrics.Height(size));
        Assert.Equal(action, CdsContainedListMetrics.ActionHeight(size));
    }

    [Fact]
    public void Disclosed_headers_are_always_32()
    {
        Assert.Equal(32, CdsContainedListMetrics.HeaderHeight(CdsContainedListKind.Disclosed, CdsSize.Xl));
        Assert.Equal(64, CdsContainedListMetrics.HeaderHeight(CdsContainedListKind.OnPage, CdsSize.Xl));
        Assert.Equal(32, CdsContainedListMetrics.HeaderActionHeight(CdsContainedListKind.Disclosed, CdsSize.Lg));
        Assert.Equal(64, CdsContainedListMetrics.HeaderActionHeight(CdsContainedListKind.OnPage, CdsSize.Xl));
    }

    [Theory]
    [InlineData(CdsSize.Sm, 6)]
    [InlineData(CdsSize.Md, 10)]
    [InlineData(CdsSize.Lg, 14)]
    [InlineData(CdsSize.Xl, 22)]
    public void Item_padding_centers_a_20px_line(CdsSize size, double expected) =>
        Assert.Equal(expected, CdsContainedListMetrics.ItemPadding(size));

    [Fact]
    public void Header_tokens_differ_by_kind()
    {
        Assert.Equal(CdsColorKeys.Background, CdsContainedListTokens.HeaderFill(CdsContainedListKind.OnPage));
        Assert.Equal(CdsColorKeys.Layer01, CdsContainedListTokens.HeaderFill(CdsContainedListKind.Disclosed));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsContainedListTokens.HeaderText(CdsContainedListKind.OnPage));
        Assert.Equal(CdsColorKeys.TextSecondary, CdsContainedListTokens.HeaderText(CdsContainedListKind.Disclosed));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsContainedListTokens.HeaderRule(CdsContainedListKind.OnPage));
        Assert.Null(CdsContainedListTokens.HeaderRule(CdsContainedListKind.Disclosed));
    }

    [Fact]
    public void Clickable_items_fill_on_hover_and_press_only()
    {
        Assert.Null(CdsContainedListTokens.ItemFill(CdsInteractionState.Hover, false));
        Assert.Null(CdsContainedListTokens.ItemFill(CdsInteractionState.None, true));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsContainedListTokens.ItemFill(CdsInteractionState.Hover, true));
        Assert.Equal(CdsColorKeys.LayerActive01, CdsContainedListTokens.ItemFill(CdsInteractionState.Hover | CdsInteractionState.Pressed, true));
        Assert.Null(CdsContainedListTokens.ItemFill(CdsInteractionState.Hover | CdsInteractionState.Disabled, true));
    }

    [Fact]
    public void Item_text_icon_and_divider_tokens()
    {
        Assert.Equal(CdsColorKeys.TextPrimary, CdsContainedListTokens.ItemText(false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsContainedListTokens.ItemText(true));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsContainedListTokens.ItemIcon(false));
        // Icons are decorative: a disabled item keeps icon-primary (only its text turns text-disabled).
        Assert.Equal(CdsColorKeys.IconPrimary, CdsContainedListTokens.ItemIcon(true));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsContainedListTokens.Divider);
    }

    [Fact]
    public void List_with_items_header_action_and_search_can_be_built()
    {
        var list = new CdsContainedList { Label = "API keys", Kind = CdsContainedListKind.Disclosed, Size = CdsSize.Md, IsInset = true };
        list.Action = new Label { Text = "+" };
        list.Search = new Entry();
        var clicked = 0;
        var item = new CdsContainedListItem { Text = "ci-deploy", Icon = CdsIconName.Document, Action = new Label { Text = "x" } };
        item.Clicked += (_, _) => clicked++;
        item.IsClickable = true;
        list.Items.Add(item);
        list.Items.Add(new CdsContainedListItem { Text = "ops-readonly", IsEnabled = false, IsClickable = true });
        list.Items.Add(new CdsContainedListItem { Body = new Label { Text = "custom" } });
        Assert.Equal(3, list.Items.Count);
        list.Items.RemoveAt(1);
        list.Kind = CdsContainedListKind.OnPage;
        list.Action = null;
        list.Search = null;
        Assert.Equal(0, clicked);
        Assert.Equal(2, list.Items.Count);
    }

    [Fact]
    public void A_clickable_item_is_named_by_its_label_then_text_then_body_text()
    {
        var item = new CdsContainedListItem { IsClickable = true, Body = new VerticalStackLayout { new Label { Text = "usage-march.csv" }, new Label { Text = "2 MB" } } };
        var hit = Maui.Carbon.Tests.Foundation.ContainerSemanticsAuditTests.Walk(item).OfType<CdsHitTarget>().Single();
        Assert.Equal("usage-march.csv 2 MB", SemanticProperties.GetDescription(hit));
        item.Text = "Usage for March";
        Assert.Equal("Usage for March", SemanticProperties.GetDescription(hit));
        item.Label = "Open usage report for March";
        Assert.Equal("Open usage report for March", SemanticProperties.GetDescription(hit));
    }

    [Fact]
    public void Body_text_ignores_hidden_labels()
    {
        Assert.Equal("a", CdsContainedListModel.TextOf(new HorizontalStackLayout { new Label { Text = "a" }, new Label { Text = "b", IsVisible = false } }));
        Assert.Null(CdsContainedListModel.TextOf(new BoxView()));
        Assert.Null(CdsContainedListModel.TextOf(null));
    }
}
