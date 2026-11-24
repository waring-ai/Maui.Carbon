namespace Maui.Carbon.Tests.Components;

public class RadioTileTests
{
    static CdsTileGroup Group(out CdsRadioTile dev, out CdsRadioTile standard, out CdsRadioTile premium)
    {
        dev = new CdsRadioTile { Value = "dev", Body = new CdsText("Development") };
        standard = new CdsRadioTile { Value = "std", Body = new CdsText("Standard") };
        premium = new CdsRadioTile { Value = "pro", Body = new CdsText("Premium") };
        // Tiles nested in a layout, like a grid of cards.
        var grid = new Grid();
        grid.Add(dev, 0, 0);
        grid.Add(standard, 1, 0);
        grid.Add(new VerticalStackLayout { premium }, 2, 0);
        return new CdsTileGroup { Legend = "Plan", Body = grid };
    }

    [Fact]
    public void Group_finds_radio_tiles_nested_in_layouts_and_nothing_is_chosen_initially()
    {
        var group = Group(out var dev, out var standard, out var premium);
        Assert.Null(group.SelectedValue);
        Assert.False(dev.IsChecked || standard.IsChecked || premium.IsChecked);
    }

    [Fact]
    public void Setting_SelectedValue_chooses_exactly_one_tile()
    {
        var group = Group(out var dev, out var standard, out var premium);
        group.SelectedValue = "std";
        Assert.True(standard.IsChecked);
        Assert.False(dev.IsChecked || premium.IsChecked);
        group.SelectedValue = "pro";
        Assert.False(standard.IsChecked);
        Assert.True(premium.IsChecked);
    }

    [Fact]
    public void Checking_a_tile_updates_the_group()
    {
        var group = Group(out var dev, out var standard, out _);
        dev.IsChecked = true;
        Assert.Equal("dev", group.SelectedValue);
        standard.IsChecked = true;
        Assert.Equal("std", group.SelectedValue);
        Assert.False(dev.IsChecked);
        standard.IsChecked = false;
        Assert.Null(group.SelectedValue);
    }

    [Fact]
    public void A_read_only_group_reverts_programmatic_changes_to_tiles()
    {
        var group = Group(out var dev, out var standard, out _);
        group.SelectedValue = "dev";
        group.IsReadOnly = true;
        standard.IsChecked = true;
        Assert.Equal("dev", group.SelectedValue);
        Assert.False(standard.IsChecked);
        Assert.True(dev.IsChecked);
    }

    [Fact]
    public void Arrow_keys_move_the_choice_in_document_order_and_wrap()
    {
        var group = Group(out _, out _, out _);
        group.SelectedValue = "std";
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
        Assert.Equal("pro", group.SelectedValue);
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal("dev", group.SelectedValue);
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Up)));
        Assert.Equal("pro", group.SelectedValue);
        Assert.False(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Enter)));
        Assert.True(group.WantsNavigationKeys);
    }

    [Fact]
    public void Arrow_keys_do_not_choose_in_a_read_only_group()
    {
        var group = Group(out _, out _, out _);
        group.SelectedValue = "dev";
        group.IsReadOnly = true;
        group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right));
        Assert.Equal("dev", group.SelectedValue);
    }

    [Fact]
    public void Tiles_added_later_are_picked_up()
    {
        var stack = new VerticalStackLayout();
        var group = new CdsTileGroup { Body = stack, SelectedValue = "late" };
        var tile = new CdsRadioTile { Value = "late" };
        stack.Add(tile);
        Assert.True(tile.IsChecked);
    }

    [Fact]
    public void SelectionChanged_reports_old_and_new_values()
    {
        var group = Group(out _, out _, out _);
        CdsChoiceChangedEventArgs<object>? seen = null;
        group.SelectionChanged += (_, e) => seen = e;
        group.SelectedValue = "dev";
        group.SelectedValue = "pro";
        Assert.Equal("dev", seen!.OldValue);
        Assert.Equal("pro", seen.NewValue);
    }

    [Fact]
    public void Disabling_the_group_disables_every_tile()
    {
        var group = Group(out var dev, out var standard, out _);
        group.IsEnabled = false;
        Assert.True(dev.IsEffectivelyDisabled);
        Assert.True(standard.IsEffectivelyDisabled);
        group.IsEnabled = true;
        Assert.False(dev.IsEffectivelyDisabled);
    }

    [Fact]
    public void Radio_tile_alone_can_be_constructed_and_checked()
    {
        var tile = new CdsRadioTile { Body = new CdsText("Standard"), AccessibleName = "Standard plan", Value = 3 };
        bool? seen = null;
        tile.CheckedChanged += (_, e) => seen = e.Value;
        tile.IsChecked = true;
        Assert.True(seen);
    }

    // RadioTile/README.md: the group is one tab stop; with nothing selected Tab lands on the first tile (CF-02).
    [Fact]
    public void The_group_is_one_tab_stop_on_the_first_tile_until_one_is_chosen()
    {
        var group = Group(out var dev, out var standard, out var premium);
        Assert.Equal([true, false, false], new[] { dev, standard, premium }.Select(t => t.Hit.IsTabStop));
        group.SelectedValue = "pro";
        Assert.Equal([false, false, true], new[] { dev, standard, premium }.Select(t => t.Hit.IsTabStop));
        Assert.Equal(2, group.TabStopIndex);
    }

    [Fact]
    public void The_tab_stop_skips_disabled_and_hidden_tiles()
    {
        var group = Group(out var dev, out var standard, out var premium);
        dev.IsEnabled = false;
        Assert.True(standard.Hit.IsTabStop);
        Assert.False(dev.Hit.IsTabStop);
        standard.IsVisible = false;
        Assert.True(premium.Hit.IsTabStop);
        Assert.Single(new[] { dev, standard, premium }, t => t.Hit.IsTabStop);

        group.SelectedValue = "dev";
        Assert.False(dev.Hit.IsTabStop);
        Assert.True(premium.Hit.IsTabStop);
    }

    [Fact]
    public void Arrow_keys_skip_disabled_tiles_and_move_the_tab_stop_with_the_choice()
    {
        var group = Group(out var dev, out var standard, out var premium);
        group.SelectedValue = "dev";
        standard.IsEnabled = false;
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
        Assert.Equal("pro", group.SelectedValue);
        Assert.True(premium.Hit.IsTabStop);
        Assert.False(dev.Hit.IsTabStop);
        Assert.False(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Home)));
    }

    [Fact]
    public void A_disabled_group_has_no_tab_stop_and_tiles_added_later_join_the_rotation()
    {
        var stack = new VerticalStackLayout();
        var group = new CdsTileGroup { Body = stack };
        var first = new CdsRadioTile { Value = "a" };
        var second = new CdsRadioTile { Value = "b" };
        stack.Add(first);
        stack.Add(second);
        Assert.True(first.Hit.IsTabStop);
        Assert.False(second.Hit.IsTabStop);

        group.IsEnabled = false;
        Assert.Equal(-1, group.TabStopIndex);
        group.IsEnabled = true;

        stack.Remove(first);
        Assert.True(first.Hit.IsTabStop); // a tile that leaves the group is a lone tab stop again
        Assert.True(second.Hit.IsTabStop);
    }
}
