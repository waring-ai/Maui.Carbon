namespace Maui.Carbon.Tests.Components;

public class TabsModelTests
{
    static readonly bool[] AllEnabled = [true, true, true, true];
    static readonly bool[] MiddleDisabled = [true, false, false, true];

    [Fact]
    public void Next_moves_and_wraps()
    {
        Assert.Equal(2, TabsModel.Next(AllEnabled, 1, 1));
        Assert.Equal(0, TabsModel.Next(AllEnabled, 3, 1));
        Assert.Equal(3, TabsModel.Next(AllEnabled, 0, -1));
        Assert.Equal(0, TabsModel.Next(AllEnabled, 1, -1));
    }

    [Fact]
    public void Next_skips_disabled_items()
    {
        Assert.Equal(3, TabsModel.Next(MiddleDisabled, 0, 1));
        Assert.Equal(0, TabsModel.Next(MiddleDisabled, 3, 1));
        Assert.Equal(0, TabsModel.Next(MiddleDisabled, 3, -1));
    }

    [Fact]
    public void Next_without_wrap_stops_at_the_ends()
    {
        Assert.Equal(-1, TabsModel.Next(AllEnabled, 3, 1, wrap: false));
        Assert.Equal(-1, TabsModel.Next(AllEnabled, 0, -1, wrap: false));
        Assert.Equal(1, TabsModel.Next(AllEnabled, 0, 1, wrap: false));
    }

    [Fact]
    public void Next_with_one_enabled_item_returns_it_and_with_none_returns_minus_one()
    {
        Assert.Equal(2, TabsModel.Next([false, false, true], 2, 1));
        Assert.Equal(-1, TabsModel.Next([false, false], 0, 1));
        Assert.Equal(-1, TabsModel.Next([], 0, 1));
    }

    [Fact]
    public void First_and_last_enabled()
    {
        Assert.Equal(1, TabsModel.First([false, true, true, false]));
        Assert.Equal(2, TabsModel.Last([false, true, true, false]));
        Assert.Equal(-1, TabsModel.First([false]));
        Assert.Equal(-1, TabsModel.Last([]));
    }

    [Fact]
    public void Coerce_keeps_enabled_selections_and_moves_off_disabled_ones()
    {
        Assert.Equal(0, TabsModel.Coerce(AllEnabled, 0));
        Assert.Equal(3, TabsModel.Coerce(MiddleDisabled, 1));
        Assert.Equal(3, TabsModel.Coerce(MiddleDisabled, 2));
        Assert.Equal(3, TabsModel.Coerce(AllEnabled, 99));
        Assert.Equal(0, TabsModel.Coerce(AllEnabled, -4));
        Assert.Equal(0, TabsModel.Coerce([true, false, false], 2));
        Assert.Equal(-1, TabsModel.Coerce([false, false], 0));
        Assert.Equal(-1, TabsModel.Coerce([], 0));
    }

    [Fact]
    public void Keys_map_to_targets()
    {
        Assert.Equal(2, TabsModel.Target(CdsKey.Right, AllEnabled, 1));
        Assert.Equal(0, TabsModel.Target(CdsKey.Left, AllEnabled, 1));
        Assert.Equal(3, TabsModel.Target(CdsKey.Left, AllEnabled, 0));
        Assert.Equal(0, TabsModel.Target(CdsKey.Home, MiddleDisabled, 3));
        Assert.Equal(3, TabsModel.Target(CdsKey.End, MiddleDisabled, 0));
        Assert.Equal(-1, TabsModel.Target(CdsKey.Enter, AllEnabled, 0));
        Assert.Equal(-1, TabsModel.Target(CdsKey.Up, AllEnabled, 0));
        Assert.Equal(1, TabsModel.Target(CdsKey.Down, AllEnabled, 0, vertical: true));
        Assert.Equal(-1, TabsModel.Target(CdsKey.Right, AllEnabled, 0, vertical: true));
    }

    [Fact]
    public void Removing_a_tab_adjusts_the_selection()
    {
        Assert.Equal(1, TabsModel.AfterRemoval([true, true, true], selected: 2, removed: 0));
        Assert.Equal(1, TabsModel.AfterRemoval([true, true, true], selected: 1, removed: 2));
        Assert.Equal(1, TabsModel.AfterRemoval([true, true], selected: 1, removed: 1));
        Assert.Equal(0, TabsModel.AfterRemoval([true, true], selected: 0, removed: 0));
        Assert.Equal(-1, TabsModel.AfterRemoval([], selected: 0, removed: 0));
    }

    [Fact]
    public void Scrolling_reveals_a_hidden_item()
    {
        Assert.Equal(100, TabsModel.ScrollToReveal(100, 180, 150, 300));
        Assert.Equal(180, TabsModel.ScrollToReveal(400, 480, 0, 300));
        Assert.Equal(50, TabsModel.ScrollToReveal(100, 180, 50, 300));
    }
}

public class TabsTokenTests
{
    [Fact]
    public void Line_selected_underlines_with_border_interactive_and_goes_semibold()
    {
        var v = CdsTabsTokens.Resolve(CdsTabsType.Line, CdsInteractionState.None, selected: true);
        Assert.Equal(CdsColorKeys.TextPrimary, v.Foreground);
        Assert.Equal(CdsColorKeys.BorderInteractive, v.BottomRule);
        Assert.True(v.IsSemiBold);
        Assert.Null(v.Background);
    }

    [Fact]
    public void Line_unselected_is_secondary_on_the_subtle_rule_and_hover_strengthens_it()
    {
        var rest = CdsTabsTokens.Resolve(CdsTabsType.Line, CdsInteractionState.None, selected: false);
        Assert.Equal(CdsColorKeys.TextSecondary, rest.Foreground);
        Assert.Equal(CdsColorKeys.BorderSubtle00, rest.BottomRule);
        var hover = CdsTabsTokens.Resolve(CdsTabsType.Line, CdsInteractionState.Hover, selected: false);
        Assert.Equal(CdsColorKeys.BorderStrong01, hover.BottomRule);
        Assert.Equal(CdsColorKeys.TextPrimary, hover.Foreground);
    }

    [Fact]
    public void Line_disabled_uses_disabled_tokens()
    {
        var v = CdsTabsTokens.Resolve(CdsTabsType.Line, CdsInteractionState.Disabled | CdsInteractionState.Hover, selected: false);
        Assert.Equal(CdsColorKeys.TextDisabled, v.Foreground);
        Assert.Equal(CdsColorKeys.BorderDisabled, v.BottomRule);
    }

    [Fact]
    public void Contained_states()
    {
        var rest = CdsTabsTokens.Resolve(CdsTabsType.Contained, CdsInteractionState.None, selected: false);
        Assert.Equal(CdsColorKeys.LayerAccent01, rest.Background);
        Assert.Null(rest.BottomRule);
        Assert.Equal(CdsColorKeys.LayerAccentHover01, CdsTabsTokens.Resolve(CdsTabsType.Contained, CdsInteractionState.Hover, false).Background);
        var selected = CdsTabsTokens.Resolve(CdsTabsType.Contained, CdsInteractionState.None, selected: true);
        Assert.Equal(CdsColorKeys.Layer01, selected.Background);
        Assert.Equal(CdsColorKeys.BorderInteractive, selected.TopRule);
        var disabled = CdsTabsTokens.Resolve(CdsTabsType.Contained, CdsInteractionState.Disabled, selected: false);
        Assert.Equal(CdsColorKeys.ButtonDisabled, disabled.Background);
        Assert.Equal(CdsColorKeys.TextOnColorDisabled, disabled.Foreground);
    }

    [Fact]
    public void Icon_only_tabs_use_icon_tokens()
    {
        Assert.Equal(CdsColorKeys.IconSecondary, CdsTabsTokens.Resolve(CdsTabsType.Line, CdsInteractionState.None, false, iconOnly: true).Icon);
        Assert.Equal(CdsColorKeys.IconPrimary, CdsTabsTokens.Resolve(CdsTabsType.Line, CdsInteractionState.None, true, iconOnly: true).Icon);
        Assert.Equal(CdsColorKeys.IconDisabled, CdsTabsTokens.Resolve(CdsTabsType.Line, CdsInteractionState.Disabled, false, iconOnly: true).Icon);
        Assert.Equal(CdsColorKeys.TextSecondary, CdsTabsTokens.Resolve(CdsTabsType.Line, CdsInteractionState.None, false).Icon);
    }

    [Fact]
    public void Heights()
    {
        Assert.Equal(40, CdsTabsTokens.Height(CdsTabsType.Line, false, CdsSize.Md, false));
        Assert.Equal(48, CdsTabsTokens.Height(CdsTabsType.Contained, false, CdsSize.Md, false));
        Assert.Equal(64, CdsTabsTokens.Height(CdsTabsType.Contained, false, CdsSize.Md, true));
        Assert.Equal(40, CdsTabsTokens.Height(CdsTabsType.Line, true, CdsSize.Md, false));
        Assert.Equal(48, CdsTabsTokens.Height(CdsTabsType.Line, true, CdsSize.Lg, false));
    }
}

public class TabsControlTests
{
    static CdsTabs Make() => new()
    {
        Items =
        {
            new CdsTab { Text = "Overview", Body = new Label { Text = "Overview body" } },
            new CdsTab { Text = "Nodes", Body = new Label { Text = "Nodes body" } },
            new CdsTab { Text = "Billing", IsEnabled = false },
            new CdsTab { Text = "Access", Icon = CdsIconName.User, SecondaryLabel = "3 keys" },
        },
    };

    [Fact]
    public void First_tab_is_selected_and_its_body_is_shown()
    {
        var tabs = Make();
        Assert.Equal(0, tabs.SelectedIndex);
        Assert.Same(tabs.Items[0], tabs.SelectedTab);
    }

    [Fact]
    public void Setting_the_index_swaps_the_body_and_raises_the_event()
    {
        var tabs = Make();
        CdsTabsSelectionChangedEventArgs? args = null;
        tabs.SelectionChanged += (_, e) => args = e;
        tabs.SelectedIndex = 1;
        Assert.Same(tabs.Items[1], tabs.SelectedTab);
        Assert.Equal((0, 1), (args?.OldIndex, args?.NewIndex));
    }

    [Fact]
    public void Selecting_a_disabled_tab_falls_back_to_the_nearest_enabled_one()
    {
        var tabs = Make();
        tabs.SelectedIndex = 2;
        Assert.Equal(3, tabs.SelectedIndex);
    }

    [Fact]
    public void Removing_the_selected_tab_selects_its_neighbour()
    {
        var tabs = Make();
        tabs.SelectedIndex = 1;
        tabs.Items.RemoveAt(1);
        Assert.Equal(2, tabs.SelectedIndex);
        Assert.Equal("Access", tabs.SelectedTab?.Text);
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var tabs = Make();
        tabs.Type = CdsTabsType.Contained;
        tabs.IsFullWidth = true;
        tabs.Activation = CdsActivation.Manual;
        tabs.IsDismissible = true;
        tabs.IsIconOnly = true;
        tabs.IconSize = CdsSize.Lg;
        tabs.Label = "Cluster sections";
        tabs.PanelPadding = new Thickness(0);
        Assert.Equal(4, tabs.Items.Count);
    }

    [Fact]
    public void Arrow_keys_move_and_select_in_automatic_mode_and_skip_disabled_tabs()
    {
        var tabs = Make();
        var right = new CdsKeyEventArgs(CdsKey.Right);
        Assert.True(tabs.OnKeyDown(right));
        Assert.Equal(1, tabs.SelectedIndex);
        tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right));
        Assert.Equal(3, tabs.SelectedIndex);
        tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right));
        Assert.Equal(0, tabs.SelectedIndex);
        tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.End));
        Assert.Equal(3, tabs.SelectedIndex);
        tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Home));
        Assert.Equal(0, tabs.SelectedIndex);
        Assert.False(tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Enter)));
    }

    [Fact]
    public void Arrow_keys_only_move_focus_in_manual_mode()
    {
        var tabs = Make();
        tabs.Activation = CdsActivation.Manual;
        tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right));
        Assert.Equal(0, tabs.SelectedIndex);
    }

    [Fact]
    public void Tabs_want_navigation_keys() => Assert.True(Make().WantsNavigationKeys);
}

public class TabsRovingFocusTests
{
    static CdsTabs Make(int selected = 0)
    {
        var tabs = new CdsTabs
        {
            Items =
            {
                new CdsTab { Text = "Overview" },
                new CdsTab { Text = "Nodes" },
                new CdsTab { Text = "Billing", IsEnabled = false },
                new CdsTab { Text = "Access" },
            },
        };
        tabs.SelectedIndex = selected;
        return tabs;
    }

    static IEnumerable<Element> Walk(Element root)
    {
        yield return root;
        if (root is IVisualTreeElement tree)
            foreach (var child in tree.GetVisualChildren().OfType<Element>())
                foreach (var e in Walk(child)) yield return e;
    }

    static int[] TabStops(CdsTabs tabs) =>
        [.. Walk(tabs).OfType<CdsHitTarget>().Where(h => h.Role == CdsAccessibilityRole.Tab).Select((h, i) => (h, i)).Where(x => x.h.IsTabStop).Select(x => x.i)];

    [Fact]
    public void Only_the_selected_tab_is_a_tab_stop()
    {
        var tabs = Make(selected: 1);
        Assert.Equal([1], TabStops(tabs));
        tabs.SelectedIndex = 3;
        Assert.Equal([3], TabStops(tabs));
        Assert.Equal(3, tabs.TabStopIndex);
    }

    [Fact]
    public void The_stop_follows_the_arrow_keys_and_skips_disabled_tabs()
    {
        var tabs = Make(selected: 1);
        Assert.True(tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
        Assert.Equal(3, tabs.SelectedIndex);
        Assert.Equal([3], TabStops(tabs));
    }

    [Fact]
    public void Manual_mode_moves_the_stop_without_selecting()
    {
        var tabs = Make();
        tabs.Activation = CdsActivation.Manual;
        tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right));
        Assert.Equal(0, tabs.SelectedIndex);
        Assert.Equal([1], TabStops(tabs));
    }

    [Fact]
    public void Rebuilding_keeps_a_single_stop_on_the_selected_tab()
    {
        var tabs = Make(selected: 3);
        tabs.Type = CdsTabsType.Contained;
        tabs.Items.Add(new CdsTab { Text = "Logs" });
        Assert.Equal([3], TabStops(tabs));
    }

    [Fact]
    public void Scroll_buttons_are_not_tab_stops()
    {
        var tabs = Make();
        var others = Walk(tabs).OfType<CdsHitTarget>().Where(h => h.Role != CdsAccessibilityRole.Tab).ToList();
        Assert.NotEmpty(others);
        Assert.All(others, h => Assert.False(h.IsTabStop));
    }
}

public class VerticalTabsTests
{
    static CdsTabs Make(int selected = 0)
    {
        var tabs = new CdsTabs { Orientation = CdsTabsOrientation.Vertical };
        tabs.Items.Add(new CdsTab { Text = "Overview" });
        tabs.Items.Add(new CdsTab { Text = "Access policies" });
        tabs.Items.Add(new CdsTab { Text = "Billing", IsEnabled = false });
        tabs.Items.Add(new CdsTab { Text = "Notifications" });
        tabs.SelectedIndex = selected;
        return tabs;
    }

    [Fact]
    public void Vertical_tabs_stack_in_a_column_and_use_up_and_down()
    {
        var tabs = Make();
        Assert.Equal(4, tabs.VerticalList!.Children.Count);
        Assert.Equal(CdsRovingOrientation.Vertical, tabs.KeyOrientation);
        Assert.False(tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
        Assert.True(tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal(1, tabs.SelectedIndex);
        // Skips the disabled tab.
        tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down));
        Assert.Equal(3, tabs.SelectedIndex);
        tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Up));
        Assert.Equal(1, tabs.SelectedIndex);
    }

    [Fact]
    public void Vertical_tabs_are_64_high_with_the_selected_edge()
    {
        var tabs = Make(selected: 1);
        var views = tabs.TabViews;
        Assert.All(views, v => Assert.Equal(CdsTabsTokens.VerticalHeight, v.MinimumHeightRequest));
        Assert.Equal(CdsColorKeys.BorderInteractive, views[1].VerticalVisuals.StartEdge);
        Assert.Null(views[1].VerticalVisuals.EndRule);
        Assert.True(views[1].VerticalVisuals.IsSemiBold);
        Assert.Equal(CdsColorKeys.BorderSubtle01, views[0].VerticalVisuals.StartEdge);
        Assert.Equal(CdsColorKeys.TextDisabled, views[2].VerticalVisuals.Foreground);
    }

    [Fact]
    public void Switching_back_to_horizontal_restores_the_row()
    {
        var tabs = Make(selected: 3);
        tabs.Orientation = CdsTabsOrientation.Horizontal;
        Assert.Empty(tabs.VerticalList!.Children);
        Assert.Equal(CdsRovingOrientation.Horizontal, tabs.KeyOrientation);
        Assert.Equal(3, tabs.SelectedIndex);
        Assert.True(tabs.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
        Assert.Equal(0, tabs.SelectedIndex);
    }

    [Theory]
    [InlineData(false, false, false, CdsColorKeys.Layer01, CdsColorKeys.TextSecondary, CdsColorKeys.BorderSubtle01)]
    [InlineData(false, true, false, CdsColorKeys.LayerHover01, CdsColorKeys.TextPrimary, CdsColorKeys.BorderStrong01)]
    [InlineData(true, false, false, CdsColorKeys.Layer01, CdsColorKeys.TextPrimary, CdsColorKeys.BorderInteractive)]
    [InlineData(false, false, true, CdsColorKeys.Layer01, CdsColorKeys.TextDisabled, CdsColorKeys.BorderSubtle01)]
    public void Vertical_tokens_follow_carbon(bool selected, bool hover, bool disabled, string fill, string text, string edge)
    {
        var state = (hover ? CdsInteractionState.Hover : 0) | (disabled ? CdsInteractionState.Disabled : 0);
        var v = CdsTabsTokens.ResolveVertical(state, selected);
        Assert.Equal(fill, v.Background);
        Assert.Equal(text, v.Foreground);
        Assert.Equal(edge, v.StartEdge);
        Assert.Equal(CdsColorKeys.BorderSubtle01, v.BottomRule);
    }
}
