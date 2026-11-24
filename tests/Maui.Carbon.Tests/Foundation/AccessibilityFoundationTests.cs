namespace Maui.Carbon.Tests.Foundation;

public class SemanticsGroupTests
{
    [Fact]
    public void Group_properties_are_stored_without_touching_semantic_properties()
    {
        var grid = new Grid();
        CdsSemantics.SetGroup(grid, "Theme", CdsSemanticGroupKind.Navigation, "Pick one");
        Assert.Equal("Theme", CdsSemantics.GetGroupName(grid));
        Assert.Equal("Pick one", CdsSemantics.GetGroupHint(grid));
        Assert.Equal(CdsSemanticGroupKind.Navigation, CdsSemantics.GetGroupKind(grid));
        Assert.Null(SemanticProperties.GetDescription(grid));
        Assert.Null(SemanticProperties.GetHint(grid));
        Assert.True(CdsSemantics.HasGroup(grid));
    }

    [Fact]
    public void Clearing_the_name_and_hint_removes_the_group()
    {
        var grid = new Grid();
        CdsSemantics.SetGroupName(grid, "Theme");
        CdsSemantics.SetGroupName(grid, null);
        Assert.False(CdsSemantics.HasGroup(grid));
        CdsSemantics.SetGroupHint(grid, "   ");
        Assert.False(CdsSemantics.HasGroup(grid));
    }

    [Fact]
    public void Group_kind_defaults_to_group()
    {
        Assert.Equal(CdsSemanticGroupKind.Group, CdsSemantics.GetGroupKind(new Grid()));
    }

    [Fact]
    public void Group_naming_works_on_every_container_kind_without_a_platform()
    {
        View[] views = [new Grid(), new VerticalStackLayout(), new Border(), new ScrollView(), new ContentView()];
        foreach (var view in views)
        {
            CdsSemantics.SetGroupName(view, "Group");
            Assert.Equal("Group", CdsSemantics.GetGroupName(view));
        }
    }
}

public class AnnouncerTests
{
    [Fact]
    public void Announcements_are_captured_with_their_priority()
    {
        using var heard = CdsAnnouncer.Capture();
        CdsAnnouncer.Announce("3 items selected");
        CdsAnnouncer.Announce("Upload failed", CdsAnnouncePriority.Assertive);
        Assert.Equal(
            [new CdsAnnouncement("3 items selected", CdsAnnouncePriority.Polite), new CdsAnnouncement("Upload failed", CdsAnnouncePriority.Assertive)],
            heard.Announcements);
    }

    [Fact]
    public void Empty_text_is_not_announced()
    {
        using var heard = CdsAnnouncer.Capture();
        CdsAnnouncer.Announce(null);
        CdsAnnouncer.Announce("");
        CdsAnnouncer.Announce("  ");
        Assert.Empty(heard.Announcements);
    }

    [Fact]
    public void Nested_captures_restore_the_outer_one()
    {
        using var outer = CdsAnnouncer.Capture();
        using (var inner = CdsAnnouncer.Capture())
        {
            CdsAnnouncer.Announce("inner");
            Assert.Single(inner.Announcements);
        }
        CdsAnnouncer.Announce("outer");
        Assert.Equal("outer", Assert.Single(outer.Announcements).Text);
    }

    [Fact]
    public void Controls_announce_through_the_announcer()
    {
        var tree = new CdsTreeView { Label = "Files" };
        var node = new CdsTreeNode { Label = "Reports" };
        tree.Nodes.Add(node);
        using var heard = CdsAnnouncer.Capture();
        tree.Select(node);
        Assert.Equal(new CdsAnnouncement("Reports", CdsAnnouncePriority.Polite), Assert.Single(heard.Announcements));
    }
}

public class HitTargetStateTests
{
    [Fact]
    public void New_states_default_off_and_can_be_set()
    {
        var hit = new CdsHitTarget();
        Assert.False(hit.IsReadOnly);
        Assert.Equal(CdsCurrentKind.None, hit.CurrentKind);
        hit.IsReadOnly = true;
        hit.CurrentKind = CdsCurrentKind.Page;
        Assert.True(hit.IsReadOnly);
        Assert.Equal(CdsCurrentKind.Page, hit.CurrentKind);
    }

    [Theory]
    [InlineData(CdsAccessibilityRole.Checkbox, true, "checked")]
    [InlineData(CdsAccessibilityRole.Checkbox, false, "not checked")]
    [InlineData(CdsAccessibilityRole.Checkbox, null, "mixed")]
    [InlineData(CdsAccessibilityRole.Radio, true, "checked")]
    [InlineData(CdsAccessibilityRole.Switch, true, "on")]
    [InlineData(CdsAccessibilityRole.Switch, null, "off")]
    [InlineData(CdsAccessibilityRole.Button, true, null)]
    [InlineData(CdsAccessibilityRole.Link, null, null)]
    public void Checked_words(CdsAccessibilityRole role, bool? isChecked, string? expected) =>
        Assert.Equal(expected, CdsHitTargetStates.CheckedWord(role, isChecked));

    [Fact]
    public void Apple_value_reports_expanded_for_every_role_before_ios_18()
    {
        // CN-08: a header action or menu button that opens a panel is a disclosure even with the Button role.
        Assert.Equal("collapsed", CdsHitTargetStates.AppleValue(CdsAccessibilityRole.Button, false, false, false, CdsCurrentKind.None, nativeExpandedStatus: false));
        Assert.Equal("expanded", CdsHitTargetStates.AppleValue(CdsAccessibilityRole.Link, false, true, false, CdsCurrentKind.None, nativeExpandedStatus: false));
        Assert.Null(CdsHitTargetStates.AppleValue(CdsAccessibilityRole.Button, false, true, false, CdsCurrentKind.None, nativeExpandedStatus: true));
        Assert.Null(CdsHitTargetStates.AppleValue(CdsAccessibilityRole.Button, false, null, false, CdsCurrentKind.None, nativeExpandedStatus: false));
    }

    [Fact]
    public void Apple_value_combines_checked_current_and_read_only()
    {
        Assert.Equal("checked, read only", CdsHitTargetStates.AppleValue(CdsAccessibilityRole.Checkbox, true, null, true, CdsCurrentKind.None, false));
        Assert.Equal("on, read only", CdsHitTargetStates.AppleValue(CdsAccessibilityRole.Switch, true, null, true, CdsCurrentKind.None, false));
        Assert.Equal("current page", CdsHitTargetStates.AppleValue(CdsAccessibilityRole.Link, false, null, false, CdsCurrentKind.Page, false));
        Assert.Equal("collapsed, read only", CdsHitTargetStates.AppleValue(CdsAccessibilityRole.ComboBox, false, false, true, CdsCurrentKind.None, false));
    }

    [Fact]
    public void Android_state_description_only_when_there_is_more_than_checked()
    {
        Assert.Null(CdsHitTargetStates.AndroidStateDescription(CdsAccessibilityRole.Checkbox, true, null, false, CdsCurrentKind.None));
        Assert.Null(CdsHitTargetStates.AndroidStateDescription(CdsAccessibilityRole.Button, false, null, false, CdsCurrentKind.None));
        Assert.Equal("mixed", CdsHitTargetStates.AndroidStateDescription(CdsAccessibilityRole.Checkbox, null, null, false, CdsCurrentKind.None));
        Assert.Equal("expanded", CdsHitTargetStates.AndroidStateDescription(CdsAccessibilityRole.Expander, false, true, false, CdsCurrentKind.None));
        Assert.Equal("collapsed", CdsHitTargetStates.AndroidStateDescription(CdsAccessibilityRole.Button, false, false, false, CdsCurrentKind.None));
        Assert.Equal("not checked, read only", CdsHitTargetStates.AndroidStateDescription(CdsAccessibilityRole.Checkbox, false, null, true, CdsCurrentKind.None));
        Assert.Equal("current page", CdsHitTargetStates.AndroidStateDescription(CdsAccessibilityRole.Link, false, null, false, CdsCurrentKind.Page));
    }

    [Theory]
    [InlineData(CdsAccessibilityRole.Link, "link")]
    [InlineData(CdsAccessibilityRole.Tab, "tab")]
    [InlineData(CdsAccessibilityRole.MenuItem, "menu item")]
    [InlineData(CdsAccessibilityRole.Option, "option")]
    [InlineData(CdsAccessibilityRole.Button, null)]
    [InlineData(CdsAccessibilityRole.Checkbox, null)]
    public void Android_role_descriptions(CdsAccessibilityRole role, string? expected) =>
        Assert.Equal(expected, CdsHitTargetStates.AndroidRoleDescription(role));

    [Fact]
    public void Android_does_not_call_links_tabs_menu_items_or_options_buttons()
    {
        // CF-06.
        foreach (var role in new[] { CdsAccessibilityRole.Link, CdsAccessibilityRole.Tab, CdsAccessibilityRole.MenuItem, CdsAccessibilityRole.Option })
            Assert.NotEqual("android.widget.Button", CdsHitTargetStates.AndroidClassName(role));
        Assert.Equal("android.widget.CheckBox", CdsHitTargetStates.AndroidClassName(CdsAccessibilityRole.Checkbox));
        Assert.Equal("android.widget.Button", CdsHitTargetStates.AndroidClassName(CdsAccessibilityRole.Expander));
    }

    [Theory]
    [InlineData(CdsCurrentKind.None, null)]
    [InlineData(CdsCurrentKind.Page, "current page")]
    [InlineData(CdsCurrentKind.Step, "current step")]
    [InlineData(CdsCurrentKind.Location, "current location")]
    [InlineData(CdsCurrentKind.Date, "current date")]
    [InlineData(CdsCurrentKind.Item, "current")]
    public void Current_words(CdsCurrentKind kind, string? expected) => Assert.Equal(expected, CdsHitTargetStates.CurrentWord(kind));
}

public class RovingFocusTests
{
    static Func<int, bool> All(int count) => i => i >= 0 && i < count;

    [Theory]
    [InlineData(0, CdsKey.Down, 1)]
    [InlineData(1, CdsKey.Up, 0)]
    [InlineData(3, CdsKey.Down, -1)] // end, no wrap
    [InlineData(0, CdsKey.Up, -1)]
    [InlineData(2, CdsKey.Home, 0)]
    [InlineData(0, CdsKey.End, 3)]
    [InlineData(-1, CdsKey.Down, 0)]
    [InlineData(-1, CdsKey.Up, 3)]
    public void Vertical_navigation(int current, CdsKey key, int expected) =>
        Assert.Equal(expected, CdsRovingFocus.Navigate(4, All(4), current, key, CdsRovingOrientation.Vertical, wraps: false, homeEnd: true));

    [Fact]
    public void Keys_of_the_other_orientation_are_not_handled()
    {
        Assert.Equal(CdsRovingFocus.NotHandled, CdsRovingFocus.Navigate(4, All(4), 0, CdsKey.Right, CdsRovingOrientation.Vertical, false, true));
        Assert.Equal(CdsRovingFocus.NotHandled, CdsRovingFocus.Navigate(4, All(4), 0, CdsKey.Down, CdsRovingOrientation.Horizontal, false, true));
        Assert.Equal(1, CdsRovingFocus.Navigate(4, All(4), 0, CdsKey.Right, CdsRovingOrientation.Both, false, true));
        Assert.Equal(1, CdsRovingFocus.Navigate(4, All(4), 0, CdsKey.Down, CdsRovingOrientation.Both, false, true));
        Assert.Equal(CdsRovingFocus.NotHandled, CdsRovingFocus.Navigate(4, All(4), 0, CdsKey.Home, CdsRovingOrientation.Both, false, homeEnd: false));
        Assert.Equal(CdsRovingFocus.NotHandled, CdsRovingFocus.Navigate(4, All(4), 0, CdsKey.Enter, CdsRovingOrientation.Both, false, true));
    }

    [Fact]
    public void Wrapping_and_skipping_disabled_items()
    {
        Func<int, bool> canFocus = i => i is 0 or 2; // 1 and 3 disabled
        Assert.Equal(2, CdsRovingFocus.Navigate(4, canFocus, 0, CdsKey.Right, CdsRovingOrientation.Horizontal, false, true));
        Assert.Equal(-1, CdsRovingFocus.Navigate(4, canFocus, 2, CdsKey.Right, CdsRovingOrientation.Horizontal, false, true));
        Assert.Equal(0, CdsRovingFocus.Navigate(4, canFocus, 2, CdsKey.Right, CdsRovingOrientation.Horizontal, wraps: true, true));
        Assert.Equal(2, CdsRovingFocus.Navigate(4, canFocus, 0, CdsKey.Left, CdsRovingOrientation.Horizontal, wraps: true, true));
        Assert.Equal(2, CdsRovingFocus.Navigate(4, canFocus, 0, CdsKey.End, CdsRovingOrientation.Horizontal, false, true));
    }

    [Fact]
    public void Tab_stop_precedence()
    {
        Assert.Equal(2, CdsRovingFocus.TabStop(4, All(4), current: 2, focused: 1, preferred: 3));
        Assert.Equal(1, CdsRovingFocus.TabStop(4, All(4), current: -1, focused: 1, preferred: 3));
        Assert.Equal(3, CdsRovingFocus.TabStop(4, All(4), current: -1, focused: -1, preferred: 3));
        Assert.Equal(0, CdsRovingFocus.TabStop(4, All(4), current: -1, focused: -1, preferred: -1));
        Assert.Equal(1, CdsRovingFocus.TabStop(4, i => i != 0 && i != 3, current: -1, focused: -1, preferred: 3));
        Assert.Equal(-1, CdsRovingFocus.TabStop(0, All(0), -1, -1, -1));
    }

    static List<CdsHitTarget> Targets(int n) => [.. Enumerable.Range(0, n).Select(_ => new CdsHitTarget())];

    static int[] Stops(IEnumerable<CdsHitTarget> items) => [.. items.Select((t, i) => (t, i)).Where(x => x.t.IsTabStop).Select(x => x.i)];

    [Fact]
    public void Exactly_one_item_is_a_tab_stop()
    {
        var items = Targets(3);
        var roving = new CdsRovingFocus(CdsRovingOrientation.Horizontal);
        roving.SetItems(items);
        Assert.Equal([0], Stops(items));
        roving.PreferredIndex = () => 2;
        Assert.Equal([2], Stops(items));
        Assert.Equal(2, roving.TabStopIndex);
    }

    [Fact]
    public void Disabled_and_hidden_items_never_take_the_stop()
    {
        var items = Targets(3);
        items[0].IsEnabled = false;
        var roving = new CdsRovingFocus();
        roving.SetItems(items);
        Assert.Equal([1], Stops(items));
        items[1].IsVisible = false; // tracked through PropertyChanged
        Assert.Equal([2], Stops(items));
        items[0].IsEnabled = true;
        Assert.Equal([0], Stops(items));
    }

    [Fact]
    public void The_stop_follows_added_and_removed_items()
    {
        var items = Targets(2);
        var roving = new CdsRovingFocus { PreferredIndex = () => 0 };
        roving.SetItems(items);
        var first = new CdsHitTarget { IsEnabled = false };
        roving.Insert(0, first);
        Assert.Equal(1, roving.TabStopIndex);
        Assert.False(first.IsTabStop);
        roving.Remove(items[0]);
        Assert.True(items[0].IsTabStop); // back to a lone hit target's default
        Assert.Equal(1, roving.TabStopIndex);
        Assert.True(items[1].IsTabStop);
        roving.Clear();
        Assert.Equal(-1, roving.TabStopIndex);
        Assert.Empty(roving.Items);
    }

    [Fact]
    public void Arrow_keys_move_the_stop_and_report_the_move()
    {
        var items = Targets(3);
        var roving = new CdsRovingFocus(CdsRovingOrientation.Horizontal, wraps: true);
        roving.SetItems(items);
        var moves = new List<int>();
        roving.Navigated += (_, i) => moves.Add(i);
        var e = new CdsKeyEventArgs(CdsKey.Right);
        Assert.True(roving.HandleKey(e));
        Assert.True(e.Handled);
        Assert.Equal([1], Stops(items));
        Assert.True(roving.HandleKey(CdsKey.End));
        Assert.True(roving.HandleKey(CdsKey.Right)); // wraps
        Assert.Equal([1, 2, 0], moves);
        Assert.False(roving.HandleKey(CdsKey.Down));
        Assert.False(roving.HandleKey(CdsKey.Enter));
    }

    [Fact]
    public void Focus_moves_the_stop_and_is_remembered_by_default()
    {
        var items = Targets(3);
        var roving = new CdsRovingFocus { PreferredIndex = () => 0 };
        roving.SetItems(items);
        items[2].ReportFocus(true, fromKeyboard: true);
        Assert.Equal([2], Stops(items));
        items[2].ReportFocus(false, fromKeyboard: true);
        Assert.Equal([2], Stops(items));
        roving.ForgetFocus();
        Assert.Equal([0], Stops(items));
    }

    [Fact]
    public void Without_remembering_the_stop_returns_to_the_preferred_item_when_focus_leaves()
    {
        var items = Targets(3);
        var roving = new CdsRovingFocus { PreferredIndex = () => 1, RemembersFocus = false };
        roving.SetItems(items);
        Assert.Equal([1], Stops(items));
        items[2].ReportFocus(true, fromKeyboard: true);
        Assert.Equal([2], Stops(items));
        items[2].ReportFocus(false, fromKeyboard: true);
        Assert.Equal([1], Stops(items));
    }

    [Fact]
    public void Moving_away_from_the_focused_item_gives_the_stop_to_the_target_first()
    {
        // Focus can only land on a tab stop (Apple: CanBecomeFocused), so the target takes the stop before focus moves.
        var items = Targets(3);
        var roving = new CdsRovingFocus { RemembersFocus = false };
        roving.SetItems(items);
        items[0].ReportFocus(true, fromKeyboard: true);
        Assert.True(roving.HandleKey(CdsKey.Down));
        Assert.Equal([1], Stops(items));
        items[0].ReportFocus(false, fromKeyboard: true); // the old item loses focus after the move
        Assert.Equal([1], Stops(items));
    }
}
