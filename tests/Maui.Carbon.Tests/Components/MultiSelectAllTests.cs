namespace Maui.Carbon.Tests.Components;

public class MultiSelectAllTests
{
    static readonly string[] Roles = ["Viewer", "Operator", "Editor", "Owner"];

    sealed class TestMulti : CdsMultiSelect
    {
        public IReadOnlyList<ListBoxEntry> Rows => BuildRows();

        public IEnumerable<string> RowTexts => Rows.Select(r => r.Text);

        public CdsNativeEntry TextEntry => Entry;

        public void Sync() => Model.SetRows(BuildRows());

        public bool? ParentState => RowCheckState(VisibleRows[0]);
    }

    static TestMulti Make(bool filterable = false, bool disableOwner = true, params string[] selected)
    {
        var multi = new TestMulti
        {
            Label = "Roles",
            ItemsSource = Roles,
            SelectAllLabel = "All roles",
            IsFilterable = filterable,
            IsItemDisabled = disableOwner ? o => o as string == "Owner" : null,
            SelectedItems = selected.ToList(),
        };
        multi.Sync();
        return multi;
    }

    static ListBoxEntry E(string text, bool disabled = false) => new(text, text, disabled);

    static readonly IReadOnlyList<ListBoxEntry> Real = [E("Viewer"), E("Operator"), E("Editor"), E("Owner", true)];

    // ---- pure model ----

    [Fact]
    public void Non_filterable_state_counts_every_selected_item_against_the_enabled_items()
    {
        Assert.False(MultiSelectAllModel.State(false, Real, []));
        Assert.Null(MultiSelectAllModel.State(false, Real, ["Viewer"]));
        Assert.Null(MultiSelectAllModel.State(false, Real, ["Viewer", "Operator"]));
        Assert.True(MultiSelectAllModel.State(false, Real, ["Viewer", "Operator", "Editor"]));
        // MultiSelect.tsx: a selected disabled item counts too, so 3 of 3 enabled with the disabled one is 4 != 3.
        Assert.False(MultiSelectAllModel.State(false, Real, ["Viewer", "Operator", "Editor", "Owner"]));
        Assert.False(MultiSelectAllModel.State(false, [E("Owner", true)], []));
    }

    [Fact]
    public void Filterable_state_looks_at_the_enabled_visible_items_only()
    {
        Assert.False(MultiSelectAllModel.State(true, Real, []));
        Assert.Null(MultiSelectAllModel.State(true, Real, ["Operator"]));
        Assert.True(MultiSelectAllModel.State(true, Real, ["Viewer", "Operator", "Editor"]));
        Assert.True(MultiSelectAllModel.State(true, Real, ["Viewer", "Operator", "Editor", "Owner"]));
        Assert.False(MultiSelectAllModel.State(true, Real, ["Owner"]));
        Assert.False(MultiSelectAllModel.State(true, [E("Owner", true)], []));
    }

    [Fact]
    public void Non_filterable_activation_clears_everything_when_anything_is_selected_else_selects_enabled_items()
    {
        Assert.Equal(["Viewer", "Operator", "Editor"], MultiSelectAllModel.Activate(false, Real, []));
        Assert.Empty(MultiSelectAllModel.Activate(false, Real, ["Viewer"]));
        // Selection.tsx clears selected disabled items and items outside the list too.
        Assert.Empty(MultiSelectAllModel.Activate(false, Real, ["Owner"]));
        Assert.Empty(MultiSelectAllModel.Activate(false, Real, ["Viewer", "Operator", "Editor"]));
    }

    [Fact]
    public void Filterable_activation_acts_on_the_visible_items_and_keeps_hidden_selections()
    {
        IReadOnlyList<ListBoxEntry> visible = [E("Operator"), E("Owner", true)];
        // Nothing visible selected: add the enabled visible items after the existing selection.
        Assert.Equal(["Viewer", "Operator"], MultiSelectAllModel.Activate(true, visible, ["Viewer"]));
        // Checked or mixed: remove every visible item (the disabled one too), keep the hidden "Viewer".
        Assert.Equal(["Viewer"], MultiSelectAllModel.Activate(true, visible, ["Viewer", "Operator", "Owner"]));
        IReadOnlyList<ListBoxEntry> two = [E("Operator"), E("Editor")];
        Assert.Equal(["Viewer"], MultiSelectAllModel.Activate(true, two, ["Viewer", "Editor"]));
        // Only a disabled visible item selected: status is unchecked, so enabled items are added and it stays.
        Assert.Equal(["Owner", "Operator"], MultiSelectAllModel.Activate(true, visible, ["Owner"]));
    }

    [Fact]
    public void Adding_the_last_enabled_item_collapses_to_every_enabled_item_in_list_order()
    {
        Assert.Equal(["Viewer", "Operator", "Editor"], MultiSelectAllModel.CollapseOnAdd(Real, ["Editor", "Viewer", "Operator"]));
        Assert.Null(MultiSelectAllModel.CollapseOnAdd(Real, ["Viewer", "Operator"]));
    }

    [Fact]
    public void Visibility_rule_differs_by_variant()
    {
        IReadOnlyList<ListBoxEntry> disabledOnly = [E("Owner", true)];
        Assert.True(MultiSelectAllModel.IsShown(false, disabledOnly));
        Assert.False(MultiSelectAllModel.IsShown(true, disabledOnly));
        Assert.False(MultiSelectAllModel.IsShown(false, []));
        Assert.True(MultiSelectAllModel.IsShown(true, Real));
        Assert.True(MultiSelectAllModel.MatchesFilter("All roles", null));
        Assert.True(MultiSelectAllModel.MatchesFilter("All roles", "ROL"));
        Assert.False(MultiSelectAllModel.MatchesFilter("All roles", "adm"));
    }

    [Fact]
    public void Announcement_says_partially_checked_for_a_mixed_parent()
    {
        var row = new ListBoxEntry(null, "All roles", false, true);
        Assert.Equal("All roles, partially checked, 1 of 5", ListBoxLogic.RowAnnouncement(row, 0, 5, false, true, mixed: true));
        Assert.Equal("All roles, checked, 1 of 5", ListBoxLogic.RowAnnouncement(row, 0, 5, true, true));
        Assert.Equal("All roles, not checked, 1 of 5", ListBoxLogic.RowAnnouncement(row, 0, 5, false, true));
    }

    [Fact]
    public void The_rule_below_the_parent_is_always_drawn()
    {
        Assert.True(ListBoxLogic.SeparatorVisible(1, 1, 1, false, true, afterSelectAll: true));
        Assert.False(ListBoxLogic.SeparatorVisible(1, 1, 1, false, true));
        Assert.False(ListBoxLogic.SeparatorVisible(0, -1, -1, false, true, afterSelectAll: true));
    }

    // ---- the control ----

    [Fact]
    public void The_parent_row_is_first_and_never_a_data_item()
    {
        var multi = Make(selected: "Editor");
        Assert.Equal(["All roles", "Viewer", "Operator", "Editor", "Owner"], multi.RowTexts);
        Assert.True(multi.Rows[0].IsSelectAll);
        Assert.Equal(["Editor"], multi.SelectedItems!.Cast<string>());
        Assert.Equal(1, multi.SelectedCount);
    }

    [Fact]
    public void No_label_means_no_parent_row()
    {
        var multi = Make();
        multi.SelectAllLabel = null;
        Assert.DoesNotContain(multi.Rows, r => r.IsSelectAll);
    }

    [Fact]
    public void Row_check_state_follows_the_selection()
    {
        var multi = Make();
        Assert.False(multi.ParentState);
        multi.ChooseRow(1);
        multi.Sync();
        Assert.Null(multi.ParentState);
        multi.SelectedItems = new List<string> { "Viewer", "Operator", "Editor" };
        multi.Sync();
        Assert.True(multi.ParentState);
    }

    [Fact]
    public void Choosing_the_parent_selects_enabled_items_then_clears_with_one_event_each()
    {
        var multi = Make();
        var raised = 0;
        multi.SelectionChanged += (_, _) => raised++;
        multi.ChooseRow(0);
        Assert.Equal(["Viewer", "Operator", "Editor"], multi.SelectedItems!.Cast<string>());
        Assert.Equal(3, multi.SelectedCount);
        Assert.Equal(1, raised);
        multi.Sync();
        Assert.True(multi.ParentState);
        multi.ChooseRow(0);
        Assert.Empty(multi.SelectedItems!);
        Assert.Equal(2, raised);
    }

    [Fact]
    public void Choosing_the_parent_from_the_mixed_state_clears()
    {
        var multi = Make(selected: "Viewer");
        multi.Sync();
        Assert.Null(multi.ParentState);
        multi.ChooseRow(0);
        Assert.Equal(0, multi.SelectedCount);
    }

    [Fact]
    public void Non_filterable_clearing_also_removes_a_selected_disabled_item()
    {
        var multi = Make(selected: "Owner");
        multi.ChooseRow(0);
        Assert.Equal(0, multi.SelectedCount);
    }

    [Fact]
    public void Selecting_the_last_enabled_item_checks_the_parent()
    {
        var multi = Make(selected: ["Viewer", "Operator"]);
        multi.Sync();
        var editor = multi.VisibleRows.ToList().FindIndex(r => r.Text == "Editor");
        multi.ChooseRow(editor);
        Assert.Equal(["Viewer", "Operator", "Editor"], multi.SelectedItems!.Cast<string>());
        multi.Sync();
        Assert.True(multi.ParentState);
    }

    [Fact]
    public void The_parent_stays_first_with_selected_first_sorting()
    {
        var multi = Make(selected: ["Operator", "Editor"]);
        multi.SelectionFeedback = CdsMultiSelectFeedback.Top;
        Assert.Equal("All roles", multi.RowTexts.First());
        Assert.Equal(["All roles", "Operator", "Editor", "Viewer", "Owner"], multi.RowTexts);
    }

    [Fact]
    public void Non_filterable_parent_is_listed_even_when_every_item_is_disabled()
    {
        var multi = Make();
        multi.IsItemDisabled = _ => true;
        Assert.True(multi.Rows[0].IsSelectAll);
    }

    [Fact]
    public void Filterable_parent_stays_first_while_filtering_and_acts_on_the_matches()
    {
        var multi = Make(filterable: true, selected: "Owner");
        multi.TextEntry.Text = "o";
        multi.Sync();
        Assert.Equal(["All roles", "Operator", "Editor", "Owner"], multi.RowTexts);
        multi.ChooseRow(0);
        Assert.Equal(["Owner", "Operator", "Editor"], multi.SelectedItems!.Cast<string>());
        multi.Sync();
        Assert.True(multi.ParentState);
        // Checked: clears the visible items, the disabled "Owner" included.
        multi.ChooseRow(0);
        Assert.Empty(multi.SelectedItems!);
    }

    [Fact]
    public void Filterable_clearing_keeps_selections_the_filter_hides()
    {
        var multi = Make(filterable: true, selected: ["Viewer", "Operator"]);
        multi.TextEntry.Text = "ope";
        multi.Sync();
        Assert.True(multi.ParentState);
        multi.ChooseRow(0);
        Assert.Equal(["Viewer"], multi.SelectedItems!.Cast<string>());
    }

    [Fact]
    public void Filterable_parent_is_hidden_when_only_disabled_items_are_shown()
    {
        var multi = Make(filterable: true);
        multi.TextEntry.Text = "own";
        Assert.Equal(["Owner"], multi.RowTexts);
    }

    [Fact]
    public void Keyboard_reaches_the_parent_as_the_first_row()
    {
        var multi = Make();
        multi.Sync();
        Assert.True(multi.Model.MoveNext());
        Assert.Equal(0, multi.Model.Highlighted);
        Assert.True(multi.Model.TypeAhead('a', 0));
        Assert.Equal(0, multi.Model.Highlighted);
        multi.Model.MoveLast();
        multi.Model.MoveFirst();
        Assert.True(multi.Model.HighlightedRow!.Value.IsSelectAll);
    }

    [Fact]
    public void Highlight_announcement_speaks_the_mixed_state()
    {
        var multi = Make(selected: "Viewer");
        multi.Sync();
        Assert.Equal("All roles, partially checked, 1 of 5", ListBoxLogic.RowAnnouncement(multi.VisibleRows[0], 0, multi.VisibleRows.Count, false, true, mixed: multi.ParentState is null));
    }

    [Fact]
    public void Fluid_multi_select_inherits_the_label()
    {
        var multi = new CdsFluidMultiSelect { Label = "Roles", ItemsSource = Roles, SelectAllLabel = "All" };
        Assert.Equal("All", multi.SelectAllLabel);
    }

    [Fact]
    public void Dropdown_rows_are_unaffected()
    {
        var dropdown = new CdsDropdown { ItemsSource = Roles, SelectedItem = "Editor" };
        var entry = new ListBoxEntry("Editor", "Editor");
        Assert.True(dropdown.RowCheckState(entry));
        Assert.False(dropdown.RowCheckState(new ListBoxEntry("Viewer", "Viewer")));
    }
}
