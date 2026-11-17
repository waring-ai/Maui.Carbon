namespace Maui.Carbon.Tests.Components;

public class InteractiveTagTokensTests
{
    [Theory]
    [InlineData(CdsTagType.Red, CdsColorKeys.TagBorderRed)]
    [InlineData(CdsTagType.Blue, CdsColorKeys.TagBorderBlue)]
    [InlineData(CdsTagType.CoolGray, CdsColorKeys.TagBorderCoolGray)]
    [InlineData(CdsTagType.WarmGray, CdsColorKeys.TagBorderWarmGray)]
    [InlineData(CdsTagType.Gray, CdsColorKeys.TagBorderGray)]
    [InlineData(CdsTagType.HighContrast, CdsColorKeys.TagBorderGray)]
    [InlineData(CdsTagType.Outline, CdsColorKeys.TagBorderGray)]
    public void Tag_border_per_type(CdsTagType type, string key) => Assert.Equal(key, CdsInteractiveTagTokens.TagBorder(type));

    [Fact]
    public void Operational_rest_and_hover()
    {
        var rest = CdsInteractiveTagTokens.Operational(CdsTagType.Blue, CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.TagBackgroundBlue, rest.Background);
        Assert.Equal(CdsColorKeys.TagBorderBlue, rest.Border);
        Assert.Equal(CdsColorKeys.TagColorBlue, rest.Foreground);

        var hover = CdsInteractiveTagTokens.Operational(CdsTagType.Blue, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.TagHoverBlue, hover.Background);
        Assert.Equal(CdsColorKeys.TagBorderBlue, hover.Border);
    }

    [Fact]
    public void Operational_disabled_has_no_hover()
    {
        foreach (var state in new[] { CdsInteractionState.Disabled, CdsInteractionState.Disabled | CdsInteractionState.Hover })
        {
            var v = CdsInteractiveTagTokens.Operational(CdsTagType.Green, state);
            Assert.Equal(CdsColorKeys.Layer01, v.Background);
            Assert.Equal(CdsColorKeys.BorderDisabled, v.Border);
            Assert.Equal(CdsColorKeys.TextDisabled, v.Foreground);
        }
    }

    [Fact]
    public void Selectable_unselected_and_hover()
    {
        var rest = CdsInteractiveTagTokens.Selectable(false, CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.Layer01, rest.Background);
        Assert.Equal(CdsColorKeys.BorderInverse, rest.Border);
        Assert.Equal(CdsColorKeys.TextPrimary, rest.Foreground);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsInteractiveTagTokens.Selectable(false, CdsInteractionState.Hover).Background);
    }

    [Fact]
    public void Selectable_selected_keeps_its_fill_on_hover()
    {
        var selected = CdsInteractiveTagTokens.Selectable(true, CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.LayerSelectedInverse, selected.Background);
        Assert.Equal(CdsColorKeys.TextInverse, selected.Foreground);
        Assert.Equal(selected, CdsInteractiveTagTokens.Selectable(true, CdsInteractionState.Hover));
        Assert.NotEqual(selected, CdsInteractiveTagTokens.Selectable(false, CdsInteractionState.None));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Selectable_disabled_looks_the_same_selected_or_not(bool selected)
    {
        var v = CdsInteractiveTagTokens.Selectable(selected, CdsInteractionState.Disabled);
        Assert.Equal(CdsColorKeys.Layer01, v.Background);
        Assert.Equal(CdsColorKeys.BorderDisabled, v.Border);
        Assert.Equal(CdsColorKeys.TextDisabled, v.Foreground);
    }

    [Fact]
    public void Close_button_colors()
    {
        var rest = CdsInteractiveTagTokens.Close(CdsTagType.Blue, hovering: false, disabled: false);
        Assert.Null(rest.Hover);
        Assert.Equal(CdsColorKeys.TagColorBlue, rest.Icon);
        Assert.Equal(CdsColorKeys.Focus, rest.Focus);

        Assert.Equal(CdsColorKeys.TagHoverBlue, CdsInteractiveTagTokens.Close(CdsTagType.Blue, true, false).Hover);
        Assert.Equal(CdsColorKeys.FocusInverse, CdsInteractiveTagTokens.Close(CdsTagType.HighContrast, false, false).Focus);

        var disabled = CdsInteractiveTagTokens.Close(CdsTagType.Blue, true, true);
        Assert.Null(disabled.Hover);
        Assert.Equal(CdsColorKeys.IconDisabled, disabled.Icon);
    }
}

public class SelectableTagTests
{
    [Fact]
    public void Toggling_raises_SelectedChanged()
    {
        var tag = new CdsSelectableTag("us-south");
        var values = new List<bool>();
        tag.SelectedChanged += (_, e) => values.Add(e.Value);

        tag.IsSelected = true;
        tag.IsSelected = true;
        tag.IsSelected = false;

        Assert.Equal([true, false], values);
        Assert.False(tag.IsSelected);
    }

    [Fact]
    public void Selected_state_flag_follows_the_property()
    {
        var tag = new CdsSelectableTag("us-south", selected: true);
        Assert.True(tag.IsSelected);
        Assert.True(tag.Is(CdsInteractionState.Selected));
        tag.IsSelected = false;
        Assert.False(tag.Is(CdsInteractionState.Selected));
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var tag = new CdsSelectableTag
        {
            Text = "Edge nodes",
            Size = CdsTagSize.Lg,
            Icon = CdsIconName.Add,
            IsEnabled = false,
        };
        tag.Size = CdsTagSize.Sm;
    }
}
