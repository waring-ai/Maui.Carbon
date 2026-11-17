namespace Maui.Carbon.Tests.Components;

public class RadioButtonGroupTests
{
    static readonly bool[] AllEnabled = [true, true, true];

    [Theory]
    [InlineData(CdsKey.Down, 1)]
    [InlineData(CdsKey.Right, 1)]
    [InlineData(CdsKey.Up, -1)]
    [InlineData(CdsKey.Left, -1)]
    [InlineData(CdsKey.Home, 0)]
    [InlineData(CdsKey.Tab, 0)]
    [InlineData(CdsKey.Space, 0)]
    public void Direction_maps_arrow_keys(CdsKey key, int expected) => Assert.Equal(expected, RadioGroupModel.Direction(key));

    [Fact]
    public void NextEnabled_moves_forward_and_back()
    {
        Assert.Equal(1, RadioGroupModel.NextEnabled(AllEnabled, 0, 1));
        Assert.Equal(0, RadioGroupModel.NextEnabled(AllEnabled, 1, -1));
    }

    [Fact]
    public void NextEnabled_wraps_at_both_ends()
    {
        Assert.Equal(0, RadioGroupModel.NextEnabled(AllEnabled, 2, 1));
        Assert.Equal(2, RadioGroupModel.NextEnabled(AllEnabled, 0, -1));
    }

    [Fact]
    public void NextEnabled_skips_disabled_items_including_across_the_wrap()
    {
        bool[] enabled = [true, false, true, false];
        Assert.Equal(2, RadioGroupModel.NextEnabled(enabled, 0, 1));
        Assert.Equal(0, RadioGroupModel.NextEnabled(enabled, 2, 1));
        Assert.Equal(2, RadioGroupModel.NextEnabled(enabled, 0, -1));
        Assert.Equal(0, RadioGroupModel.NextEnabled(enabled, 2, -1));
    }

    [Fact]
    public void NextEnabled_with_one_enabled_item_stays_on_it()
    {
        bool[] enabled = [false, true, false];
        Assert.Equal(1, RadioGroupModel.NextEnabled(enabled, 1, 1));
        Assert.Equal(1, RadioGroupModel.NextEnabled(enabled, 1, -1));
    }

    [Fact]
    public void NextEnabled_with_nothing_current_starts_at_the_first_or_last_enabled()
    {
        bool[] enabled = [false, true, true, false];
        Assert.Equal(1, RadioGroupModel.NextEnabled(enabled, -1, 1));
        Assert.Equal(2, RadioGroupModel.NextEnabled(enabled, -1, -1));
    }

    [Fact]
    public void NextEnabled_returns_minus_one_when_nothing_is_enabled_or_empty()
    {
        Assert.Equal(-1, RadioGroupModel.NextEnabled([false, false], 0, 1));
        Assert.Equal(-1, RadioGroupModel.NextEnabled([], 0, 1));
        Assert.Equal(-1, RadioGroupModel.NextEnabled(AllEnabled, 0, 0));
    }

    [Fact]
    public void TabStop_is_the_selected_item_or_the_first_enabled()
    {
        Assert.Equal(1, RadioGroupModel.TabStop(AllEnabled, 1));
        Assert.Equal(0, RadioGroupModel.TabStop(AllEnabled, -1));
        Assert.Equal(1, RadioGroupModel.TabStop([false, true, true], -1));
        Assert.Equal(1, RadioGroupModel.TabStop([false, true, true], 0));
        Assert.Equal(-1, RadioGroupModel.TabStop([false, false], 0));
    }

    [Fact]
    public void IndexOfValue_matches_by_equality_and_ignores_null()
    {
        object?[] values = ["a", 2, "c"];
        Assert.Equal(0, RadioGroupModel.IndexOfValue(values, "a"));
        Assert.Equal(1, RadioGroupModel.IndexOfValue(values, 2));
        Assert.Equal(-1, RadioGroupModel.IndexOfValue(values, "z"));
        Assert.Equal(-1, RadioGroupModel.IndexOfValue(values, null));
    }

    [Fact]
    public void Tokens_default_ring_and_dot_are_icon_primary()
    {
        var v = CdsRadioTokens.Resolve(CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.IconPrimary, v.Ring);
        Assert.Equal(CdsColorKeys.IconPrimary, v.Dot);
        Assert.Equal(CdsColorKeys.TextPrimary, v.Label);
    }

    [Fact]
    public void Tokens_invalid_ring_is_error_and_disabled_wins_over_invalid()
    {
        Assert.Equal(CdsColorKeys.SupportError, CdsRadioTokens.Resolve(CdsInteractionState.Invalid).Ring);
        var disabled = CdsRadioTokens.Resolve(CdsInteractionState.Invalid | CdsInteractionState.Disabled);
        Assert.Equal(CdsColorKeys.IconDisabled, disabled.Ring);
        Assert.Equal(CdsColorKeys.TextDisabled, disabled.Dot);
        Assert.Equal(CdsColorKeys.TextDisabled, disabled.Label);
    }

    [Fact]
    public void Tokens_read_only_ring_is_icon_disabled()
    {
        var v = CdsRadioTokens.Resolve(CdsInteractionState.ReadOnly);
        Assert.Equal(CdsColorKeys.IconDisabled, v.Ring);
        Assert.Equal(CdsColorKeys.IconPrimary, v.Dot);
    }

    static CdsRadioButtonGroup Group(out CdsRadioButton a, out CdsRadioButton b, out CdsRadioButton c)
    {
        a = new CdsRadioButton("Single zone", "single");
        b = new CdsRadioButton("Multizone", "multi");
        c = new CdsRadioButton("Edge", "edge");
        var group = new CdsRadioButtonGroup { Legend = "Cluster type", Orientation = CdsChoiceOrientation.Vertical };
        group.Items.Add(a);
        group.Items.Add(b);
        group.Items.Add(c);
        return group;
    }

    [Fact]
    public void Nothing_is_selected_by_default()
    {
        var group = Group(out var a, out var b, out var c);
        Assert.Null(group.SelectedValue);
        Assert.Null(group.SelectedItem);
        Assert.False(a.IsChecked || b.IsChecked || c.IsChecked);
    }

    [Fact]
    public void Setting_SelectedValue_checks_exactly_that_radio_and_sets_SelectedItem()
    {
        var group = Group(out var a, out var b, out var c);
        group.SelectedValue = "multi";
        Assert.True(b.IsChecked);
        Assert.False(a.IsChecked);
        Assert.False(c.IsChecked);
        Assert.Same(b, group.SelectedItem);
        group.SelectedValue = "edge";
        Assert.False(b.IsChecked);
        Assert.True(c.IsChecked);
        Assert.Same(c, group.SelectedItem);
    }

    [Fact]
    public void Checking_a_radio_selects_its_value_and_unchecks_the_others()
    {
        var group = Group(out var a, out var b, out _);
        a.IsChecked = true;
        Assert.Equal("single", group.SelectedValue);
        b.IsChecked = true;
        Assert.Equal("multi", group.SelectedValue);
        Assert.False(a.IsChecked);
        Assert.Same(b, group.SelectedItem);
    }

    [Fact]
    public void Unchecking_the_selected_radio_clears_the_selection()
    {
        var group = Group(out var a, out _, out _);
        a.IsChecked = true;
        a.IsChecked = false;
        Assert.Null(group.SelectedValue);
    }

    [Fact]
    public void Setting_SelectedItem_selects_that_radio()
    {
        var group = Group(out _, out _, out var c);
        group.SelectedItem = c;
        Assert.Equal("edge", group.SelectedValue);
        Assert.True(c.IsChecked);
    }

    [Fact]
    public void An_unknown_value_leaves_every_radio_unchecked()
    {
        var group = Group(out var a, out var b, out var c);
        group.SelectedValue = "nope";
        Assert.False(a.IsChecked || b.IsChecked || c.IsChecked);
        Assert.Null(group.SelectedItem);
    }

    [Fact]
    public void Selecting_before_the_radios_are_added_applies_when_they_arrive()
    {
        var group = new CdsRadioButtonGroup { SelectedValue = "b" };
        var radio = new CdsRadioButton("B", "b");
        group.Items.Add(radio);
        Assert.True(radio.IsChecked);
    }

    [Fact]
    public void SelectionChanged_reports_old_and_new_values()
    {
        var group = Group(out _, out _, out _);
        CdsChoiceChangedEventArgs<object>? seen = null;
        group.SelectionChanged += (_, e) => seen = e;
        group.SelectedValue = "single";
        group.SelectedValue = "multi";
        Assert.Equal("single", seen!.OldValue);
        Assert.Equal("multi", seen.NewValue);
    }

    [Fact]
    public void Arrow_keys_select_the_next_radio_and_wrap()
    {
        var group = Group(out _, out _, out _);
        group.SelectedValue = "multi";
        var down = new CdsKeyEventArgs(CdsKey.Down);
        Assert.True(group.OnKeyDown(down));
        Assert.True(down.Handled);
        Assert.Equal("edge", group.SelectedValue);
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
        Assert.Equal("single", group.SelectedValue);
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Up)));
        Assert.Equal("edge", group.SelectedValue);
    }

    [Fact]
    public void Arrow_keys_do_not_change_a_read_only_selection_and_other_keys_are_ignored()
    {
        var group = Group(out _, out _, out _);
        group.SelectedValue = "single";
        group.IsReadOnly = true;
        group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down));
        Assert.Equal("single", group.SelectedValue);
        Assert.False(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Home)));
        Assert.True(group.WantsNavigationKeys);
    }

    [Fact]
    public void Group_can_be_constructed_and_every_property_set()
    {
        var group = Group(out var a, out _, out _);
        group.LabelPosition = CdsRadioLabelPosition.Left;
        group.Orientation = CdsChoiceOrientation.Horizontal;
        group.HelperText = "help";
        group.IsInvalid = true;
        group.InvalidText = "bad";
        group.IsWarning = true;
        group.WarnText = "careful";
        group.IsEnabled = false;
        a.HideLabel = true;
        a.IsReadOnly = true;
        Assert.Equal(3, group.Items.Count);
    }
}
