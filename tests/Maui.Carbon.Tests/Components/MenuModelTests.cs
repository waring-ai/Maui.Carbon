namespace Maui.Carbon.Tests.Components;

public class MenuModelTests
{
    static CdsMenuItem Item(string text, bool enabled = true, params CdsMenuItemBase[] children)
    {
        var item = new CdsMenuItem(text) { IsEnabled = enabled };
        foreach (var c in children) item.Items.Add(c);
        return item;
    }

    // Rows: 0 Open, 1 Save (disabled), 2 divider, 3 Share, 4 Rename (disabled), 5 Delete
    static MenuModel Sample() => new(new CdsMenuItemBase[]
    {
        Item("Open"), Item("Save", enabled: false), new CdsMenuItemDivider(), Item("Share"), Item("Rename", enabled: false),
        new CdsMenuItem("Delete") { Kind = CdsMenuItemKind.Danger },
    });

    [Fact]
    public void No_row_is_highlighted_at_first()
    {
        var model = Sample();
        Assert.Equal(-1, model.Highlighted);
        Assert.Null(model.HighlightedRow);
    }

    [Fact]
    public void Down_from_nothing_lands_on_the_first_enabled_row()
    {
        var model = Sample();
        model.MoveNext();
        Assert.Equal(0, model.Highlighted);
    }

    [Fact]
    public void Up_from_nothing_lands_on_the_last_enabled_row()
    {
        var model = Sample();
        model.MovePrevious();
        Assert.Equal(5, model.Highlighted);
    }

    [Fact]
    public void Down_skips_disabled_rows_and_dividers()
    {
        var model = Sample();
        model.MoveFirst();
        model.MoveNext();
        Assert.Equal(3, model.Highlighted);
        model.MoveNext();
        Assert.Equal(5, model.Highlighted);
    }

    [Fact]
    public void Down_wraps_from_the_end_to_the_start()
    {
        var model = Sample();
        model.MoveLast();
        model.MoveNext();
        Assert.Equal(0, model.Highlighted);
    }

    [Fact]
    public void Up_wraps_from_the_start_to_the_end_and_skips_disabled_rows()
    {
        var model = Sample();
        model.MoveFirst();
        model.MovePrevious();
        Assert.Equal(5, model.Highlighted);
        model.MovePrevious();
        Assert.Equal(3, model.Highlighted);
        model.MovePrevious();
        Assert.Equal(0, model.Highlighted);
    }

    [Fact]
    public void Home_and_End_jump_to_the_first_and_last_focusable_rows()
    {
        var model = new MenuModel(new CdsMenuItemBase[] { Item("a", false), new CdsMenuItemDivider(), Item("b"), Item("c"), Item("d", false) });
        model.HandleKey(CdsKey.End, true);
        Assert.Equal(3, model.Highlighted);
        model.HandleKey(CdsKey.Home, true);
        Assert.Equal(2, model.Highlighted);
    }

    [Fact]
    public void Navigation_keys_are_handled_even_when_nothing_can_be_focused()
    {
        var model = new MenuModel(new CdsMenuItemBase[] { Item("a", false), new CdsMenuItemDivider() });
        foreach (var key in new[] { CdsKey.Up, CdsKey.Down, CdsKey.Home, CdsKey.End })
        {
            var result = model.HandleKey(key, true);
            Assert.True(result.Handled);
            Assert.Equal(MenuAction.None, result.Action);
        }
        Assert.Equal(-1, model.Highlighted);
        Assert.False(model.HasFocusableRows);
    }

    [Fact]
    public void An_empty_menu_does_not_loop_or_throw()
    {
        var model = new MenuModel(Array.Empty<CdsMenuItemBase>());
        Assert.False(model.MoveNext());
        Assert.False(model.MovePrevious());
        Assert.False(model.MoveFirst());
        Assert.False(model.MoveLast());
        Assert.Equal(-1, model.Highlighted);
    }

    [Fact]
    public void A_single_focusable_row_stays_highlighted_when_moving()
    {
        var model = new MenuModel(new CdsMenuItemBase[] { Item("only") });
        model.MoveNext();
        Assert.False(model.MoveNext());
        Assert.False(model.MovePrevious());
        Assert.Equal(0, model.Highlighted);
    }

    [Fact]
    public void SetHighlight_ignores_disabled_rows_dividers_and_bad_indices()
    {
        var model = Sample();
        Assert.False(model.SetHighlight(1));
        Assert.False(model.SetHighlight(2));
        Assert.False(model.SetHighlight(-1));
        Assert.False(model.SetHighlight(99));
        Assert.Equal(-1, model.Highlighted);
        Assert.True(model.SetHighlight(3));
        Assert.False(model.SetHighlight(3));
        model.Clear();
        Assert.Equal(-1, model.Highlighted);
    }

    [Fact]
    public void Enter_and_Space_activate_the_highlighted_row()
    {
        var model = Sample();
        model.SetHighlight(3);
        foreach (var key in new[] { CdsKey.Enter, CdsKey.Space })
            Assert.Equal(new MenuKeyResult(true, MenuAction.Activate), model.HandleKey(key, true));
    }

    [Fact]
    public void Enter_without_a_highlight_is_swallowed_without_action()
    {
        var model = Sample();
        Assert.Equal(new MenuKeyResult(true, MenuAction.None), model.HandleKey(CdsKey.Enter, true));
    }

    [Fact]
    public void Enter_on_a_submenu_row_opens_the_submenu()
    {
        var model = new MenuModel(new CdsMenuItemBase[] { Item("Share", true, Item("Copy link")) });
        model.MoveFirst();
        Assert.Equal(MenuAction.OpenSubmenu, model.HandleKey(CdsKey.Enter, true).Action);
        Assert.Equal(MenuAction.OpenSubmenu, model.HandleKey(CdsKey.Space, true).Action);
    }

    [Fact]
    public void Right_opens_a_submenu_only_on_a_submenu_row()
    {
        var model = new MenuModel(new CdsMenuItemBase[] { Item("Plain"), Item("Share", true, Item("Copy link")) });
        model.SetHighlight(0);
        Assert.False(model.HandleKey(CdsKey.Right, true).Handled);
        model.SetHighlight(1);
        Assert.Equal(new MenuKeyResult(true, MenuAction.OpenSubmenu), model.HandleKey(CdsKey.Right, true));
    }

    [Fact]
    public void Left_closes_a_submenu_but_is_ignored_on_the_root()
    {
        var model = Sample();
        Assert.False(model.HandleKey(CdsKey.Left, true).Handled);
        Assert.Equal(new MenuKeyResult(true, MenuAction.CloseSubmenu), model.HandleKey(CdsKey.Left, false));
    }

    [Fact]
    public void Escape_closes_the_submenu_first_then_the_menu()
    {
        var model = Sample();
        Assert.Equal(new MenuKeyResult(true, MenuAction.CloseSubmenu), model.HandleKey(CdsKey.Escape, false));
        Assert.Equal(new MenuKeyResult(true, MenuAction.Close), model.HandleKey(CdsKey.Escape, true));
    }

    [Fact]
    public void Tab_is_not_a_menu_key_so_it_moves_focus_and_the_trigger_closes_the_menu()
    {
        Assert.Equal(MenuKeyResult.Ignored, Sample().HandleKey(CdsKey.Tab, true));
        Assert.Equal(MenuKeyResult.Ignored, Sample().HandleKey(CdsKey.Tab, false));
    }

    [Fact]
    public void Describe_gives_the_label_state_and_position_among_the_items()
    {
        var model = Sample();
        Assert.Equal("Open, 1 of 5", model.Describe(0));
        Assert.Equal("Save, unavailable, 2 of 5", model.Describe(1));
        Assert.Equal("Share, 3 of 5", model.Describe(3));
        Assert.Equal("Delete, 5 of 5", model.Describe(5));
    }

    [Fact]
    public void Describe_is_empty_for_dividers_and_out_of_range_rows()
    {
        var model = Sample();
        Assert.Equal(string.Empty, model.Describe(2));
        Assert.Equal(string.Empty, model.Describe(-1));
        Assert.Equal(string.Empty, model.Describe(6));
    }

    [Fact]
    public void Describe_reports_submenus_and_checked_state()
    {
        var model = new MenuModel(new CdsMenuItemBase[]
        {
            Item("Share", true, Item("Copy link")),
            new CdsMenuItemSelectable { Text = "Show hidden", IsSelected = true },
            new CdsMenuItemSelectable { Text = "Wrap lines" },
        });
        Assert.Equal("Share, submenu, 1 of 3", model.Describe(0));
        Assert.Equal("Show hidden, checked, 2 of 3", model.Describe(1));
        Assert.Equal("Wrap lines, not checked, 3 of 3", model.Describe(2));
    }

    [Fact]
    public void Describe_names_the_group_only_when_the_highlight_enters_it()
    {
        var sort = new CdsMenuItemRadioGroup { Label = "Sort by", SelectedItem = "Name" };
        sort.Items.Add("Name");
        sort.Items.Add("Date");
        var model = new MenuModel(new CdsMenuItemBase[] { Item("Refresh"), new CdsMenuItemDivider(), sort });
        Assert.Equal("Sort by, Name, checked, 2 of 3", model.Describe(2, previous: 0));
        Assert.Equal("Sort by, Name, checked, 2 of 3", model.Describe(2));
        Assert.Equal("Date, not checked, 3 of 3", model.Describe(3, previous: 2));
        Assert.Equal("Refresh, 1 of 3", model.Describe(0, previous: 3));
    }

    [Fact]
    public void A_disabled_submenu_row_cannot_be_highlighted()
    {
        var model = new MenuModel(new CdsMenuItemBase[] { Item("Share", false, Item("Copy link")), Item("Open") });
        model.MoveFirst();
        Assert.Equal(1, model.Highlighted);
    }

    [Fact]
    public void Groups_are_flattened_in_order_with_their_label()
    {
        var group = new CdsMenuItemGroup { Label = "Sharing" };
        group.Items.Add(Item("Share link"));
        group.Items.Add(Item("Invite"));
        var model = new MenuModel(new CdsMenuItemBase[] { Item("Open"), group });
        Assert.Equal(["Open", "Share link", "Invite"], model.Rows.Select(r => r.Text));
        Assert.Null(model.Rows[0].GroupLabel);
        Assert.Equal("Sharing", model.Rows[2].GroupLabel);
    }

    [Fact]
    public void Radio_groups_become_one_row_per_choice_and_mark_the_selected_one()
    {
        var radio = new CdsMenuItemRadioGroup { Label = "Sort by", SelectedItem = "Name" };
        radio.Items.Add("Name");
        radio.Items.Add("Date");
        var model = new MenuModel(new CdsMenuItemBase[] { radio });
        Assert.Equal(2, model.Rows.Count);
        Assert.All(model.Rows, r => Assert.Equal(MenuRowKind.Radio, r.Kind));
        Assert.True(model.Rows[0].IsSelected);
        Assert.False(model.Rows[1].IsSelected);
        Assert.Equal("Date", model.Rows[1].RadioValue);
        Assert.Equal("Sort by", model.Rows[1].GroupLabel);
        Assert.True(model.HasSelection);
    }

    [Fact]
    public void Radio_choices_use_ItemToString()
    {
        var radio = new CdsMenuItemRadioGroup { ItemToString = o => $"#{o}" };
        radio.Items.Add(1);
        radio.Items.Add(2);
        Assert.Equal(["#1", "#2"], new MenuModel(new CdsMenuItemBase[] { radio }).Rows.Select(r => r.Text));
    }

    [Fact]
    public void Selectable_rows_report_their_state_and_reserve_the_checkmark_column()
    {
        var model = new MenuModel(new CdsMenuItemBase[] { new CdsMenuItemSelectable { Text = "Show hidden", IsSelected = true }, Item("Open") });
        Assert.True(model.HasSelection);
        Assert.Equal(MenuRowKind.Selectable, model.Rows[0].Kind);
        Assert.True(model.Rows[0].IsSelected);
    }

    [Fact]
    public void Icons_are_detected_across_rows()
    {
        Assert.False(Sample().HasIcons);
        var model = new MenuModel(new CdsMenuItemBase[] { Item("Open"), new CdsMenuItem("Edit", CdsIconName.Edit) });
        Assert.True(model.HasIcons);
        Assert.False(model.HasSelection);
    }

    [Fact]
    public void A_command_that_cannot_execute_disables_the_row()
    {
        var item = new CdsMenuItem("Run") { Command = new Command(() => { }, () => false) };
        var model = new MenuModel(new CdsMenuItemBase[] { item });
        Assert.False(model.Rows[0].IsEnabled);
        Assert.False(model.HasFocusableRows);
    }

    [Fact]
    public void Submenu_children_are_exposed()
    {
        var model = new MenuModel(new CdsMenuItemBase[] { Item("Share", true, Item("Copy link"), Item("Email")) });
        Assert.True(model.Rows[0].HasSubmenu);
        Assert.Equal(2, model.Rows[0].Children.Count);
    }

    [Fact]
    public void Danger_items_are_marked()
    {
        Assert.True(Sample().Rows[5].IsDanger);
        Assert.False(Sample().Rows[0].IsDanger);
    }
}
