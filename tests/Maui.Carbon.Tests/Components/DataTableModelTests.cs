using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class DataTableModelTests
{
    static DataTableRowData Row(string key, object? name, object? nodes = null, bool disabled = false) =>
        new(key, key, [name, nodes], [name?.ToString() ?? string.Empty, nodes?.ToString() ?? string.Empty], disabled);

    static DataTableModel Create(params DataTableRowData[] rows)
    {
        var model = new DataTableModel(CultureInfo.InvariantCulture);
        model.SetColumns(["name", "nodes"]);
        model.SetRows(rows);
        return model;
    }

    static string[] Keys(DataTableModel m) => m.VisibleRows.Select(r => r.Key).ToArray();

    // ----- search -----

    [Fact]
    public void Search_is_case_insensitive_over_every_cell()
    {
        var m = Create(Row("a", "Prod-EU-de-1", 12), Row("b", "staging", 3), Row("c", "dev", 8));
        m.SearchText = "EU-DE";
        Assert.Equal(["a"], Keys(m));
        m.SearchText = "3";
        Assert.Equal(["b"], Keys(m)); // matches the second cell
        m.SearchText = "  ";
        Assert.Equal(["a", "b", "c"], Keys(m));
        m.SearchText = "zzz";
        Assert.Empty(m.VisibleRows);
    }

    [Fact]
    public void Search_keeps_source_order_and_works_with_sorting()
    {
        var m = Create(Row("a", "alpha", 3), Row("b", "beta", 1), Row("c", "gamma", 2));
        m.SetSort("nodes", CdsSortDirection.Ascending);
        m.SearchText = "a";
        Assert.Equal(["b", "c", "a"], Keys(m)); // all three contain an a; sorted by nodes
    }

    // ----- sort -----

    [Fact]
    public void Sort_cycle_is_none_ascending_descending_none()
    {
        Assert.Equal(CdsSortDirection.Ascending, DataTableModel.NextDirection(CdsSortDirection.None));
        Assert.Equal(CdsSortDirection.Descending, DataTableModel.NextDirection(CdsSortDirection.Ascending));
        Assert.Equal(CdsSortDirection.None, DataTableModel.NextDirection(CdsSortDirection.Descending));
    }

    [Fact]
    public void CycleSort_starts_ascending_on_a_new_column_and_cycles_on_the_same_one()
    {
        var m = Create(Row("a", "x", 1));
        Assert.Equal(CdsSortDirection.Ascending, m.CycleSort("name"));
        Assert.Equal("name", m.SortKey);
        Assert.Equal(CdsSortDirection.Descending, m.CycleSort("name"));
        Assert.Equal(CdsSortDirection.None, m.CycleSort("name"));
        Assert.Null(m.SortKey);
        m.CycleSort("name");
        Assert.Equal(CdsSortDirection.Ascending, m.CycleSort("nodes")); // another column restarts
        Assert.Equal("nodes", m.SortKey);
    }

    [Fact]
    public void Numbers_sort_numerically_not_as_text()
    {
        var m = Create(Row("a", "x", 10), Row("b", "x", 9), Row("c", "x", 100), Row("d", "x", 2));
        m.SetSort("nodes", CdsSortDirection.Ascending);
        Assert.Equal(["d", "b", "a", "c"], Keys(m));
        m.SetSort("nodes", CdsSortDirection.Descending);
        Assert.Equal(["c", "a", "b", "d"], Keys(m));
    }

    [Fact]
    public void Numeric_strings_sort_numerically_and_mixed_numeric_types_compare()
    {
        var m = Create(Row("a", "x", "10"), Row("b", "x", "9"), Row("c", "x", 9.5), Row("d", "x", 2L));
        m.SetSort("nodes", CdsSortDirection.Ascending);
        Assert.Equal(["d", "b", "c", "a"], Keys(m));
    }

    [Fact]
    public void Text_sorts_by_culture_ignoring_nothing_but_casing_order()
    {
        var m = Create(Row("a", "cherry"), Row("b", "Banana"), Row("c", "apple"));
        m.SetSort("name", CdsSortDirection.Ascending);
        Assert.Equal(["c", "b", "a"], Keys(m));
    }

    [Fact]
    public void Sort_is_stable_for_equal_values_in_both_directions()
    {
        var m = Create(Row("a", "x", 1), Row("b", "x", 1), Row("c", "x", 0), Row("d", "x", 1));
        m.SetSort("nodes", CdsSortDirection.Ascending);
        Assert.Equal(["c", "a", "b", "d"], Keys(m));
        m.SetSort("nodes", CdsSortDirection.Descending);
        Assert.Equal(["a", "b", "d", "c"], Keys(m));
    }

    [Fact]
    public void Missing_values_sort_first_ascending()
    {
        var m = Create(Row("a", "x", 5), Row("b", "x", null), Row("c", "x", 1));
        m.SetSort("nodes", CdsSortDirection.Ascending);
        Assert.Equal(["b", "c", "a"], Keys(m));
    }

    [Fact]
    public void Dates_and_other_comparables_sort_by_type()
    {
        var m = Create(Row("a", "x", new DateTime(2026, 3, 1)), Row("b", "x", new DateTime(2025, 1, 1)));
        m.SetSort("nodes", CdsSortDirection.Ascending);
        Assert.Equal(["b", "a"], Keys(m));
    }

    [Fact]
    public void Sorting_does_not_change_row_keys_and_none_restores_source_order()
    {
        var m = Create(Row("a", "c", 3), Row("b", "a", 1), Row("c", "b", 2));
        m.SetSort("name", CdsSortDirection.Ascending);
        Assert.Equal(["b", "c", "a"], Keys(m));
        Assert.Equal("a", m.Rows[0].Key);
        m.SetSort("name", CdsSortDirection.None);
        Assert.Equal(["a", "b", "c"], Keys(m));
        Assert.Null(m.SortKey);
        Assert.Equal(CdsSortDirection.None, m.SortDirection);
    }

    [Fact]
    public void An_unknown_sort_key_leaves_the_order_alone()
    {
        var m = Create(Row("a", "c", 3), Row("b", "a", 1));
        m.SetSort("missing", CdsSortDirection.Ascending);
        Assert.Equal(["a", "b"], Keys(m));
    }

    [Fact]
    public void Selection_survives_sorting_and_searching()
    {
        var m = Create(Row("a", "c", 3), Row("b", "a", 1), Row("c", "b", 2));
        m.Toggle("b");
        m.SetSort("name", CdsSortDirection.Descending);
        m.SearchText = "c";
        Assert.True(m.IsSelected("b"));
        Assert.Equal(1, m.SelectedCount);
    }

    // ----- selection -----

    [Fact]
    public void Toggle_selects_and_deselects_and_counts()
    {
        var m = Create(Row("a", "x"), Row("b", "y"), Row("c", "z"));
        Assert.True(m.Toggle("a"));
        Assert.True(m.Toggle("c"));
        Assert.Equal(2, m.SelectedCount);
        Assert.Equal(["a", "c"], m.SelectedRows.Select(r => r.Key));
        Assert.True(m.Toggle("a"));
        Assert.Equal(1, m.SelectedCount);
        Assert.False(m.SetSelected("nope", true));
    }

    [Fact]
    public void Disabled_rows_cannot_be_selected()
    {
        var m = Create(Row("a", "x"), Row("b", "y", disabled: true));
        Assert.False(m.Toggle("b"));
        Assert.False(m.SelectOnly("b"));
        Assert.Equal(0, m.SelectedCount);
    }

    [Fact]
    public void SelectOnly_replaces_the_selection_radio_style()
    {
        var m = Create(Row("a", "x"), Row("b", "y"));
        Assert.True(m.SelectOnly("a"));
        Assert.True(m.SelectOnly("b"));
        Assert.Equal(["b"], m.SelectedKeys);
        Assert.False(m.SelectOnly("b"));
    }

    [Fact]
    public void Select_all_is_tri_state_over_enabled_visible_rows()
    {
        var m = Create(Row("a", "x"), Row("b", "y"), Row("c", "z", disabled: true));
        Assert.False(m.SelectAllState);
        m.Toggle("a");
        Assert.Null(m.SelectAllState); // some
        m.Toggle("b");
        Assert.True(m.SelectAllState); // the disabled row does not count
    }

    [Fact]
    public void Select_all_with_nothing_to_select_is_unchecked()
    {
        var m = Create(Row("a", "x", disabled: true));
        Assert.False(m.SelectAllState);
        Assert.False(m.ToggleSelectAll());
        Assert.False(Create().ToggleSelectAll());
    }

    [Fact]
    public void ToggleSelectAll_selects_every_enabled_row_then_clears_them()
    {
        var m = Create(Row("a", "x"), Row("b", "y"), Row("c", "z", disabled: true));
        Assert.True(m.ToggleSelectAll());
        Assert.Equal(["a", "b"], m.SelectedKeys.OrderBy(k => k));
        m.Toggle("a");
        Assert.Null(m.SelectAllState);
        Assert.True(m.ToggleSelectAll()); // indeterminate: selects the rest
        Assert.True(m.SelectAllState);
        Assert.True(m.ToggleSelectAll()); // all: clears
        Assert.Equal(0, m.SelectedCount);
    }

    [Fact]
    public void Select_all_only_covers_visible_rows_and_keeps_hidden_selection()
    {
        var m = Create(Row("a", "alpha"), Row("b", "beta"), Row("c", "gamma"));
        m.Toggle("c");
        m.SearchText = "a"; // alpha, beta, gamma all contain "a"
        m.SearchText = "alp";
        Assert.False(m.SelectAllState);
        m.ToggleSelectAll();
        Assert.Equal(["a", "c"], m.SelectedKeys.OrderBy(k => k));
        Assert.True(m.SelectAllState);
        m.ToggleSelectAll();
        Assert.Equal(["c"], m.SelectedKeys); // the hidden row stays selected
        Assert.Equal(1, m.SelectedCount);
    }

    [Fact]
    public void ClearSelection_reports_whether_anything_changed()
    {
        var m = Create(Row("a", "x"));
        Assert.False(m.ClearSelection());
        m.Toggle("a");
        Assert.True(m.ClearSelection());
        Assert.Equal(0, m.SelectedCount);
    }

    [Fact]
    public void ReplaceSelection_drops_unknown_keys()
    {
        var m = Create(Row("a", "x"), Row("b", "y"));
        Assert.True(m.ReplaceSelection(["a", "zzz"]));
        Assert.Equal(["a"], m.SelectedKeys);
        Assert.False(m.ReplaceSelection(["a"]));
    }

    [Fact]
    public void Replacing_rows_drops_selection_and_expansion_of_removed_rows_and_reports_it()
    {
        var m = Create(Row("a", "x"), Row("b", "y"));
        m.Toggle("a");
        m.Toggle("b");
        m.SetExpanded("a", true);
        var changed = m.SetRows([Row("b", "y"), Row("c", "z")]);
        Assert.True(changed);
        Assert.Equal(["b"], m.SelectedKeys);
        Assert.Empty(m.ExpandedKeys);
        Assert.False(m.SetRows([Row("b", "y")]));
    }

    [Theory]
    [InlineData(0, "0 items selected")]
    [InlineData(1, "1 item selected")]
    [InlineData(3, "3 items selected")]
    public void Batch_text_pluralizes(int count, string text) => Assert.Equal(text, DataTableModel.BatchText(count));

    // ----- expansion -----

    [Fact]
    public void Expansion_toggles_and_ignores_unknown_rows()
    {
        var m = Create(Row("a", "x"), Row("b", "y"));
        Assert.True(m.ToggleExpanded("a"));
        Assert.True(m.IsExpanded("a"));
        Assert.False(m.SetExpanded("a", true));
        Assert.True(m.ToggleExpanded("a"));
        Assert.False(m.IsExpanded("a"));
        Assert.False(m.SetExpanded("nope", true));
    }

    [Fact]
    public void ReplaceExpanded_drops_unknown_keys_and_reports_changes()
    {
        var m = Create(Row("a", "x"), Row("b", "y"));
        Assert.True(m.ReplaceExpanded(["a", "zzz"]));
        Assert.Equal(["a"], m.ExpandedKeys);
        Assert.False(m.ReplaceExpanded(["a"]));
    }

    [Fact]
    public void Expansion_is_independent_of_sorting()
    {
        var m = Create(Row("a", "c"), Row("b", "a"));
        m.SetExpanded("a", true);
        m.SetSort("name", CdsSortDirection.Ascending);
        Assert.True(m.IsExpanded("a"));
    }

    // ----- batch bar -----

    [Fact]
    public void Batch_actions_keep_their_labels_while_the_bar_has_room()
    {
        Assert.False(DataTableModel.BatchIconsOnly(800, "1 item selected", [("Restart", true), ("Delete", true)]));
    }

    [Fact]
    public void Batch_actions_drop_their_labels_for_icons_in_a_narrow_bar()
    {
        Assert.True(DataTableModel.BatchIconsOnly(186, "1 item selected", [("Restart", true)]));
    }

    [Fact]
    public void Batch_actions_without_icons_always_keep_their_labels()
    {
        Assert.False(DataTableModel.BatchIconsOnly(186, "1 item selected", [("Restart", false)]));
    }

    [Fact]
    public void Body_compact_text_is_measured_with_its_tracking()
    {
        Assert.Equal(0, DataTableModel.BodyCompactWidth(null));
        var cancel = DataTableModel.BodyCompactWidth("Cancel");
        Assert.InRange(cancel, 40, 50);
        Assert.True(DataTableModel.BodyCompactWidth("1 item selected") > cancel);
    }
}
