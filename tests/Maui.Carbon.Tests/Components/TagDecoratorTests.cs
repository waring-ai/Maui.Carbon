namespace Maui.Carbon.Tests.Components;

/// <summary>The AI label decorator of the tags (Carbon's <c>decorator</c> slot).</summary>
public class TagDecoratorTests
{
    static HorizontalStackLayout RowOf(CdsTag tag) => Private.Field<HorizontalStackLayout>(tag, "Row");

    static Grid GridOf(CdsTag tag) => (Grid)Private.Field<Border>(tag, "Pill").Content!;

    [Fact]
    public void An_ai_label_on_a_tag_is_forced_to_sm_inline_and_sits_after_the_label()
    {
        var label = new CdsAILabel { Size = CdsAILabelSize.Xl };
        var tag = new CdsTag("Summarized", CdsTagType.Blue) { Decorator = label };
        Assert.Equal(CdsAILabelSize.Sm, label.Size);
        Assert.Equal(CdsAILabelKind.Inline, label.Kind);
        var row = RowOf(tag).Children;
        Assert.Equal(3, row.Count);
        Assert.Same(label, row[2]);
        Assert.Equal(new Thickness(1, 0, 0, 0), label.Margin);
    }

    [Fact]
    public void A_read_only_tag_drops_a_decorator_that_is_not_an_ai_label_as_carbon_does()
    {
        var tag = new CdsTag("Summarized") { Decorator = new BoxView() };
        Assert.Equal(2, RowOf(tag).Children.Count);
    }

    [Fact]
    public void A_dismissible_tag_shows_the_label_between_the_text_and_the_close_button()
    {
        var label = new CdsAILabel();
        var tag = new CdsDismissibleTag("Region: us-east") { Decorator = label };
        var grid = GridOf(tag);
        Assert.Equal(4, grid.ColumnDefinitions.Count);
        Assert.Equal(2, Grid.GetColumn(label));
        Assert.Equal(CdsAILabelSize.Sm, label.Size);
        Assert.Equal(CdsAILabelKind.Inline, label.Kind);
        Assert.Equal(32.14, label.MinimumWidthRequest);
        var close = grid.Children.OfType<Grid>().Single();
        Assert.Equal(3, Grid.GetColumn(close));
    }

    [Fact]
    public void A_dismissible_tag_shows_any_view_as_it_is()
    {
        var plain = new BoxView { WidthRequest = 8, HeightRequest = 8 };
        var tag = new CdsDismissibleTag("Region: us-east") { Decorator = plain };
        Assert.Contains(plain, GridOf(tag).Children);
        Assert.Equal(2, Grid.GetColumn(plain));
        Assert.Equal(-1, plain.MinimumWidthRequest);
    }

    [Fact]
    public void Operational_and_selectable_tags_ignore_the_decorator_and_keep_their_layout()
    {
        var label = new CdsAILabel();
        var operational = new CdsOperationalTag("+3") { Decorator = label };
        Assert.Equal(3, GridOf(operational).ColumnDefinitions.Count);
        Assert.DoesNotContain(label, GridOf(operational).Children);

        var other = new CdsAILabel();
        var selectable = new CdsSelectableTag { Text = "Network", Decorator = other };
        Assert.Equal(3, GridOf(selectable).ColumnDefinitions.Count);
        Assert.DoesNotContain(other, GridOf(selectable).Children);
        Assert.Null(other.ForegroundKey);
    }

    [Fact]
    public void Replacing_or_removing_the_decorator_leaves_the_row_clean()
    {
        var tag = new CdsTag("Summarized");
        tag.Decorator = new CdsAILabel();
        tag.Decorator = new CdsAILabel();
        Assert.Equal(3, RowOf(tag).Children.Count);
        tag.Decorator = null;
        Assert.Equal(2, RowOf(tag).Children.Count);

        var dismissible = new CdsDismissibleTag("Region");
        var before = GridOf(dismissible).Children.Count;
        dismissible.Decorator = new CdsAILabel();
        dismissible.Decorator = null;
        Assert.Equal(before, GridOf(dismissible).Children.Count);
    }

    [Theory]
    [InlineData(CdsTagType.Blue)]
    [InlineData(CdsTagType.Green)]
    [InlineData(CdsTagType.HighContrast)]
    [InlineData(CdsTagType.Outline)]
    public void The_label_takes_the_tags_text_color(CdsTagType type)
    {
        var label = new CdsAILabel();
        var tag = new CdsTag("Summarized", type) { Decorator = label };
        Assert.Equal(CdsTagTokens.Resolve(type, false).Foreground, label.ForegroundKey);
        var dismissible = new CdsDismissibleTag("Region", type) { Decorator = new CdsAILabel() };
        Assert.Equal(CdsTagTokens.Resolve(type, false).Foreground, ((CdsAILabel)dismissible.Decorator!).ForegroundKey);
    }

    [Fact]
    public void A_disabled_tag_gives_the_label_the_disabled_text_color()
    {
        var label = new CdsAILabel();
        var tag = new CdsTag("Summarized", CdsTagType.Blue) { Decorator = label };
        tag.IsEnabled = false;
        Assert.Equal(CdsColorKeys.TextDisabled, label.ForegroundKey);
    }

    [Fact]
    public void Dismissing_works_and_the_label_has_its_own_hit_target()
    {
        var tag = new CdsDismissibleTag("Region", CdsTagType.Blue) { Decorator = new CdsAILabel() };
        var dismissed = 0;
        tag.Dismissed += (_, _) => dismissed++;
        Assert.Equal(0, dismissed);
        Assert.Null(Private.Field<Border>(tag, "Pill").GetValue(SemanticProperties.DescriptionProperty) as string);
    }

    // ---- the label's foreground option

    [Fact]
    public void An_inline_label_with_a_foreground_uses_it_in_every_state_and_for_the_hover_outline()
    {
        const string key = CdsColorKeys.TagColorBlue;
        var rest = CdsAILabelTokens.Resolve(CdsAILabelKind.Inline, false, CdsInteractionState.None, key);
        Assert.Equal(key, rest.Text);
        Assert.Equal(key, rest.Dot);
        Assert.Null(rest.Border);

        var hover = CdsAILabelTokens.Resolve(CdsAILabelKind.Inline, false, CdsInteractionState.Hover, key);
        Assert.Equal(key, hover.Text);
        Assert.Equal(key, hover.Dot);
        Assert.Equal(key, hover.Border);

        var focus = CdsAILabelTokens.Resolve(CdsAILabelKind.Inline, false, CdsInteractionState.FocusVisible, key);
        Assert.Equal(CdsColorKeys.Focus, focus.Border);

        var withText = CdsAILabelTokens.Resolve(CdsAILabelKind.Inline, true, CdsInteractionState.None, key);
        Assert.Equal(CdsColorKeys.BorderInverse, withText.Border);
        Assert.Equal(key, withText.SecondaryText);
    }

    [Fact]
    public void The_default_kind_ignores_the_foreground()
    {
        var plain = CdsAILabelTokens.Resolve(CdsAILabelKind.Default, false, CdsInteractionState.None);
        var withKey = CdsAILabelTokens.Resolve(CdsAILabelKind.Default, false, CdsInteractionState.None, CdsColorKeys.TagColorBlue);
        Assert.Equal(plain, withKey);
    }
}
