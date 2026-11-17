namespace Maui.Carbon.Tests.Components;

public class DropdownListBoxModelTests
{
    // 0 Apple, 1 Banana (disabled), 2 Cherry, 3 Cranberry, 4 Date (disabled), 5 Elderberry
    static ListBoxModel Sample() => new(
    [
        new("a", "Apple"),
        new("b", "Banana", true),
        new("c", "Cherry"),
        new("cr", "Cranberry"),
        new("d", "Date", true),
        new("e", "Elderberry"),
    ]);

    [Fact]
    public void Nothing_is_highlighted_at_first()
    {
        var model = Sample();
        Assert.Equal(-1, model.Highlighted);
        Assert.Null(model.HighlightedRow);
    }

    [Fact]
    public void Down_from_nothing_lands_on_the_first_enabled_row_and_up_on_the_last()
    {
        var down = Sample();
        down.MoveNext();
        Assert.Equal(0, down.Highlighted);

        var up = Sample();
        up.MovePrevious();
        Assert.Equal(5, up.Highlighted);
    }

    [Fact]
    public void Movement_skips_disabled_rows_and_wraps_at_both_ends()
    {
        var model = Sample();
        model.MoveFirst();
        model.MoveNext();
        Assert.Equal(2, model.Highlighted); // Banana skipped
        model.MoveNext();
        model.MoveNext();
        Assert.Equal(5, model.Highlighted); // Date skipped
        model.MoveNext();
        Assert.Equal(0, model.Highlighted); // wrapped
        model.MovePrevious();
        Assert.Equal(5, model.Highlighted); // wrapped back
    }

    [Fact]
    public void Home_and_end_pick_the_first_and_last_enabled_rows()
    {
        var rows = new ListBoxModel([new("x", "X", true), new("y", "Y"), new("z", "Z", true)]);
        Assert.True(rows.MoveFirst());
        Assert.Equal(1, rows.Highlighted);
        Assert.True(rows.MoveLast());
        Assert.Equal(1, rows.Highlighted);
    }

    [Fact]
    public void A_list_of_disabled_rows_cannot_be_highlighted()
    {
        var model = new ListBoxModel([new("x", "X", true), new("y", "Y", true)]);
        Assert.False(model.MoveFirst());
        Assert.False(model.MoveNext());
        Assert.False(model.MovePrevious());
        Assert.Equal(-1, model.Highlighted);
    }

    [Fact]
    public void An_empty_list_cannot_be_highlighted()
    {
        var model = new ListBoxModel();
        Assert.False(model.MoveNext());
        Assert.False(model.MoveLast());
    }

    [Fact]
    public void Set_highlight_rejects_disabled_and_out_of_range_rows()
    {
        var model = Sample();
        Assert.False(model.SetHighlight(1));
        Assert.False(model.SetHighlight(-1));
        Assert.False(model.SetHighlight(6));
        Assert.True(model.SetHighlight(3));
        Assert.Equal(3, model.Highlighted);
        Assert.False(model.SetHighlight(4));
        Assert.Equal(3, model.Highlighted);
    }

    [Fact]
    public void Highlight_value_finds_enabled_rows_only()
    {
        var model = Sample();
        Assert.True(model.HighlightValue("cr"));
        Assert.Equal(3, model.Highlighted);
        Assert.False(model.HighlightValue("b"));
        Assert.False(model.HighlightValue(null));
        Assert.Equal(3, model.Highlighted);
        Assert.True(model.HighlightValueOrFirst("zzz"));
        Assert.Equal(0, model.Highlighted);
    }

    [Fact]
    public void Replacing_the_rows_clears_the_highlight_unless_it_is_kept()
    {
        var model = Sample();
        model.SetHighlight(2);
        model.SetRows([new("cr", "Cranberry"), new("c", "Cherry")], keepHighlight: true);
        Assert.Equal(1, model.Highlighted);

        model.SetRows([new("a", "Apple")], keepHighlight: true);
        Assert.Equal(-1, model.Highlighted);

        model.SetHighlight(0);
        model.SetRows([new("a", "Apple")]);
        Assert.Equal(-1, model.Highlighted);
    }

    [Theory]
    [InlineData(CdsKey.Down, true, "None")]
    [InlineData(CdsKey.Up, true, "None")]
    [InlineData(CdsKey.Home, true, "None")]
    [InlineData(CdsKey.End, true, "None")]
    [InlineData(CdsKey.Escape, true, "Close")]
    [InlineData(CdsKey.Left, false, "None")]
    [InlineData(CdsKey.PageDown, false, "None")]
    public void Open_keys_on_a_button_trigger(CdsKey key, bool handled, string action)
    {
        var result = Sample().HandleOpenKey(key, textField: false);
        Assert.Equal(handled, result.Handled);
        Assert.Equal(action, result.Action.ToString());
    }

    [Fact]
    public void Enter_and_space_choose_the_highlighted_row_or_close_when_nothing_is_highlighted()
    {
        var model = Sample();
        Assert.Equal(new ListBoxKeyResult(true, ListBoxAction.Close), model.HandleOpenKey(CdsKey.Enter, false));
        Assert.Equal(new ListBoxKeyResult(true, ListBoxAction.Close), model.HandleOpenKey(CdsKey.Space, false));
        model.SetHighlight(2);
        Assert.Equal(new ListBoxKeyResult(true, ListBoxAction.Choose), model.HandleOpenKey(CdsKey.Enter, false));
        Assert.Equal(new ListBoxKeyResult(true, ListBoxAction.Choose), model.HandleOpenKey(CdsKey.Space, false));
    }

    [Fact]
    public void Tab_closes_without_consuming_the_key()
    {
        var result = Sample().HandleOpenKey(CdsKey.Tab, false);
        Assert.False(result.Handled);
        Assert.Equal(ListBoxAction.Close, result.Action);
    }

    [Fact]
    public void A_text_field_keeps_home_end_and_space_for_the_text()
    {
        var model = Sample();
        Assert.False(model.HandleOpenKey(CdsKey.Home, true).Handled);
        Assert.False(model.HandleOpenKey(CdsKey.End, true).Handled);
        Assert.False(model.HandleOpenKey(CdsKey.Space, true).Handled);
        Assert.False(model.HandleOpenKey(CdsKey.Enter, true).Handled); // nothing highlighted: the entry completes
        Assert.True(model.HandleOpenKey(CdsKey.Down, true).Handled);
        model.SetHighlight(0);
        Assert.Equal(ListBoxAction.Choose, model.HandleOpenKey(CdsKey.Enter, true).Action);
    }

    [Fact]
    public void Down_and_up_open_a_closed_field()
    {
        Assert.Equal(ListBoxAction.Open, ListBoxModel.HandleClosedKey(CdsKey.Down).Action);
        Assert.Equal(ListBoxAction.Open, ListBoxModel.HandleClosedKey(CdsKey.Up).Action);
        Assert.False(ListBoxModel.HandleClosedKey(CdsKey.Home).Handled);
        Assert.False(ListBoxModel.HandleClosedKey(CdsKey.Enter).Handled);
    }

    [Fact]
    public void Type_ahead_jumps_to_the_next_match_and_cycles_on_a_repeated_letter()
    {
        var model = Sample();
        Assert.True(model.TypeAhead('c', 0));
        Assert.Equal(2, model.Highlighted); // Cherry
        Assert.True(model.TypeAhead('c', 100));
        Assert.Equal(3, model.Highlighted); // Cranberry
        Assert.True(model.TypeAhead('c', 200));
        Assert.Equal(2, model.Highlighted); // back to Cherry
    }

    [Fact]
    public void Type_ahead_builds_a_search_string_within_the_window_and_restarts_after_it()
    {
        var model = Sample();
        Assert.True(model.TypeAhead('c', 0));
        Assert.True(model.TypeAhead('r', 100));
        Assert.Equal(3, model.Highlighted); // "cr" = Cranberry
        Assert.True(model.TypeAhead('e', 5000)); // window expired: "e" alone
        Assert.Equal(5, model.Highlighted);
    }

    [Fact]
    public void Type_ahead_skips_disabled_rows_and_reports_no_match()
    {
        var model = Sample();
        Assert.False(model.TypeAhead('b', 0)); // Banana is disabled
        Assert.False(model.TypeAhead('z', 2000));
        Assert.Equal(-1, model.Highlighted);
    }

    [Fact]
    public void Type_ahead_is_case_insensitive_and_ignores_control_characters()
    {
        var model = Sample();
        Assert.True(model.TypeAhead('E', 0));
        Assert.Equal(5, model.Highlighted);
        Assert.False(model.TypeAhead('\n', 10));
    }

    [Fact]
    public void Default_filter_is_a_case_insensitive_contains_match()
    {
        var rows = Sample().Rows;
        var filtered = ListBoxLogic.Filter(rows, "ER", null, null);
        Assert.Equal(["Cherry", "Cranberry", "Elderberry"], filtered.Select(r => r.Text));
    }

    [Fact]
    public void Filter_shows_the_full_list_for_empty_input_or_the_selected_items_text()
    {
        var rows = Sample().Rows;
        Assert.Equal(6, ListBoxLogic.Filter(rows, "", null, null).Count);
        Assert.Equal(6, ListBoxLogic.Filter(rows, null, null, null).Count);
        Assert.Equal(6, ListBoxLogic.Filter(rows, "cherry", "Cherry", null).Count);
        Assert.Single(ListBoxLogic.Filter(rows, "cherr", "Cherry", null));
    }

    [Fact]
    public void A_custom_filter_replaces_the_default()
    {
        var rows = Sample().Rows;
        var filtered = ListBoxLogic.Filter(rows, "x", null, (item, input) => item is "a" or "e");
        Assert.Equal(["Apple", "Elderberry"], filtered.Select(r => r.Text));
    }

    [Fact]
    public void Selected_rows_move_to_the_top_keeping_both_orders()
    {
        var rows = Sample().Rows;
        var ordered = ListBoxLogic.SelectedFirst(rows, v => v is "e" or "c");
        Assert.Equal(["Cherry", "Elderberry", "Apple", "Banana", "Cranberry", "Date"], ordered.Select(r => r.Text));
    }

    [Fact]
    public void Index_of_finds_a_value_or_minus_one()
    {
        var rows = Sample().Rows;
        Assert.Equal(3, ListBoxLogic.IndexOf(rows, "cr"));
        Assert.Equal(-1, ListBoxLogic.IndexOf(rows, "nope"));
        Assert.Equal(-1, ListBoxLogic.IndexOf(rows, null));
    }

    [Theory]
    [InlineData(0, 220, 0, 40, 0)] // visible: unchanged
    [InlineData(0, 220, 200, 40, 20)] // below the viewport: scroll just enough
    [InlineData(100, 220, 40, 40, 40)] // above: align to the row's top
    [InlineData(100, 220, 120, 40, 100)] // inside: unchanged
    [InlineData(50, 0, 500, 40, 50)] // unknown viewport: unchanged
    public void Scroll_target_keeps_the_highlighted_row_in_view(double offset, double viewport, double rowTop, double rowHeight, double expected) =>
        Assert.Equal(expected, ListBoxLogic.ScrollTarget(offset, viewport, rowTop, rowHeight));

    [Theory]
    [InlineData(0, -1, -1, false, false, false)] // the first row has no rule
    [InlineData(1, -1, -1, false, false, true)]
    [InlineData(2, 2, -1, false, false, false)] // highlighted row
    [InlineData(3, 2, -1, false, false, false)] // row after the highlighted one
    [InlineData(1, -1, 1, false, false, false)] // hovered row
    [InlineData(2, -1, 1, false, false, false)] // row after the hovered one
    [InlineData(2, -1, -1, true, false, false)] // below the selected row (single select)
    [InlineData(2, -1, -1, true, true, true)] // multi-select keeps it
    public void Separators_hide_next_to_highlight_hover_and_selection(int index, int highlighted, int hovered, bool previousSelected, bool multi, bool expected) =>
        Assert.Equal(expected, ListBoxLogic.SeparatorVisible(index, highlighted, hovered, previousSelected, multi));

    [Fact]
    public void Selection_set_toggles_and_keeps_the_order_of_choice()
    {
        var set = new SelectionSet();
        Assert.True(set.Toggle("b"));
        Assert.True(set.Toggle("a"));
        Assert.Equal(["b", "a"], set.Items);
        Assert.False(set.Toggle("b"));
        Assert.Equal(["a"], set.Items);
        Assert.True(set.Contains("a"));
        Assert.False(set.Contains("b"));
        Assert.False(set.Toggle(null));
    }

    [Fact]
    public void Selection_set_replace_drops_duplicates_and_reports_changes()
    {
        var set = new SelectionSet();
        Assert.True(set.Replace(["a", "a", null, "b"]));
        Assert.Equal(["a", "b"], set.Items);
        Assert.False(set.Replace(["a", "b"]));
        Assert.True(set.Replace(["b", "a"]));
        Assert.True(set.Clear());
        Assert.False(set.Clear());
        Assert.False(set.Replace(null));
    }

    // ---- tokens ----

    [Theory]
    [InlineData(CdsSize.Xs, 32)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    public void Row_height_matches_the_field_height_and_clamps_to_sm_lg(CdsSize size, double height) =>
        Assert.Equal(height, CdsListBoxTokens.RowHeight(size));

    [Theory]
    [InlineData(CdsSize.Sm, 176)]
    [InlineData(CdsSize.Md, 220)]
    [InlineData(CdsSize.Lg, 264)]
    public void The_list_shows_five_and_a_half_rows(CdsSize size, double height) =>
        Assert.Equal(height, CdsListBoxTokens.MaxListHeight(size));

    [Fact]
    public void Row_colors_follow_state()
    {
        // idle, selected, hover, selected+hover, pressed, disabled
        Assert.Equal(new CdsListBoxRowVisuals(null, CdsColorKeys.TextSecondary), CdsListBoxTokens.ResolveRow(false, false, false, false, false, false));
        Assert.Equal(new CdsListBoxRowVisuals(CdsColorKeys.LayerSelected01, CdsColorKeys.TextPrimary), CdsListBoxTokens.ResolveRow(true, false, false, false, false, false));
        Assert.Equal(new CdsListBoxRowVisuals(CdsColorKeys.LayerHover01, CdsColorKeys.TextPrimary), CdsListBoxTokens.ResolveRow(false, false, true, false, false, false));
        Assert.Equal(new CdsListBoxRowVisuals(CdsColorKeys.LayerSelectedHover01, CdsColorKeys.TextPrimary), CdsListBoxTokens.ResolveRow(true, false, true, false, false, false));
        Assert.Equal(CdsColorKeys.LayerSelected01, CdsListBoxTokens.ResolveRow(false, false, true, true, false, false).Background);
        Assert.Equal(new CdsListBoxRowVisuals(null, CdsColorKeys.TextDisabled), CdsListBoxTokens.ResolveRow(true, true, true, true, true, false));
    }

    [Fact]
    public void A_highlighted_row_has_primary_text_but_no_fill()
    {
        var v = CdsListBoxTokens.ResolveRow(false, true, false, false, false, false);
        Assert.Null(v.Background);
        Assert.Equal(CdsColorKeys.TextPrimary, v.Text);
    }

    [Fact]
    public void Multi_select_rows_keep_primary_text_and_are_not_filled_by_selection()
    {
        var selected = CdsListBoxTokens.ResolveRow(true, false, false, false, false, multi: true);
        Assert.Null(selected.Background);
        Assert.Equal(CdsColorKeys.TextPrimary, selected.Text);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsListBoxTokens.ResolveRow(true, false, true, false, false, multi: true).Background);
        Assert.Equal(CdsColorKeys.TextPrimary, CdsListBoxTokens.ResolveRow(false, false, false, false, false, multi: true).Text);
    }

    [Fact]
    public void Field_text_icon_rule_and_inline_fill_follow_state()
    {
        Assert.Equal(CdsColorKeys.TextPrimary, CdsListBoxTokens.FieldText(false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsListBoxTokens.FieldText(true));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsListBoxTokens.FieldIcon(false, false));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsListBoxTokens.FieldIcon(true, false));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsListBoxTokens.FieldIcon(false, true));

        Assert.Equal(CdsColorKeys.BorderStrong01, CdsListBoxTokens.FieldRule(false, false, false, false));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsListBoxTokens.FieldRule(false, false, true, false)); // open
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsListBoxTokens.FieldRule(false, true, false, false)); // read only
        Assert.Null(CdsListBoxTokens.FieldRule(true, false, false, false)); // disabled
        Assert.Null(CdsListBoxTokens.FieldRule(false, false, false, true)); // inline

        Assert.Equal(CdsColorKeys.LayerHover01, CdsListBoxTokens.InlineFill(true, false, false, false));
        Assert.Null(CdsListBoxTokens.InlineFill(false, false, false, false));
        Assert.Null(CdsListBoxTokens.InlineFill(true, true, false, false));
        Assert.Null(CdsListBoxTokens.InlineFill(true, false, false, true));
    }

    [Fact]
    public void The_selection_tag_is_a_high_contrast_filter_tag()
    {
        var tag = CdsListBoxTokens.ResolveTag(false);
        Assert.Equal(CdsColorKeys.BackgroundInverse, tag.Background);
        Assert.Equal(CdsColorKeys.TextInverse, tag.Text);
        Assert.Equal(CdsColorKeys.IconInverse, tag.Icon);
        Assert.Equal(CdsColorKeys.ButtonSecondaryHover, tag.CloseHover);

        var disabled = CdsListBoxTokens.ResolveTag(true);
        Assert.Equal(CdsColorKeys.TextDisabled, disabled.Text);
        Assert.Equal(CdsColorKeys.IconDisabled, disabled.Icon);
        Assert.Null(disabled.CloseHover);
    }

    [Fact]
    public void Rows_inset_one_rem_or_half_a_rem_when_inline()
    {
        Assert.Equal(16, CdsListBoxTokens.RowInset(false));
        Assert.Equal(8, CdsListBoxTokens.RowInset(true));
    }
}
