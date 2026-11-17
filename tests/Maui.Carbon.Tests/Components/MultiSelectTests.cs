using System.Collections.ObjectModel;

namespace Maui.Carbon.Tests.Components;

public class MultiSelectTests
{
    static readonly string[] Statuses = ["Succeeded", "In progress", "Failed", "Cancelled", "Queued"];

    sealed class TestMulti : CdsMultiSelect
    {
        public void Opening() => OnListOpening();

        public CdsNativeEntry TextEntry => Entry;

        public bool ChromeIsFlat => Chrome.IsFlat;

        public bool UsesTextField => HasTextField;

        public IEnumerable<string> RowTexts => BuildRows().Select(r => r.Text);
    }

    static TestMulti Make() => new() { Label = "Status", Placeholder = "All", ItemsSource = Statuses };

    [Fact]
    public void Constructing_and_setting_every_property_does_not_throw()
    {
        var multi = new CdsMultiSelect
        {
            Label = "Status",
            HideLabel = true,
            Placeholder = "All",
            HelperText = "h",
            IsInvalid = true,
            InvalidText = "e",
            IsWarning = true,
            WarnText = "w",
            IsReadOnly = true,
            Size = CdsSize.Sm,
            Type = CdsListBoxType.Inline,
            Direction = CdsListBoxDirection.Top,
            ItemsSource = Statuses,
            ItemToString = o => o?.ToString() ?? "",
            ItemTemplate = new DataTemplate(() => new Label()),
            IsItemDisabled = _ => false,
            IsFilterable = true,
            SelectionFeedback = CdsMultiSelectFeedback.Top,
            SortSelectedFirst = false,
            ShouldFilterItem = (_, _) => true,
            SelectedItems = new List<string> { "Failed" },
            IsOpen = true,
        };
        Assert.Equal(1, multi.SelectedCount);
        Assert.False(multi.IsOpen);
    }

    [Fact]
    public void Selected_items_set_from_outside_drive_the_count_and_the_event()
    {
        var multi = Make();
        CdsListBoxSelectionChangedEventArgs? args = null;
        multi.SelectionChanged += (_, e) => args = e;
        multi.SelectedItems = new List<string> { "Failed", "Queued" };
        Assert.Equal(2, multi.SelectedCount);
        Assert.Equal(["Failed", "Queued"], args!.SelectedItems);
        Assert.Equal("Failed", args.SelectedItem);
    }

    [Fact]
    public void Choosing_rows_toggles_them_and_writes_back_to_a_new_list()
    {
        var multi = Make();
        multi.Model.SetRows(multi.BuildRows());
        multi.ChooseRow(0);
        multi.ChooseRow(2);
        Assert.Equal(["Succeeded", "Failed"], multi.SelectedItems!.Cast<string>());
        multi.ChooseRow(0);
        Assert.Equal(["Failed"], multi.SelectedItems!.Cast<string>());
    }

    [Fact]
    public void An_observable_collection_is_updated_in_place()
    {
        var multi = Make();
        var selected = new ObservableCollection<string> { "Queued" };
        multi.SelectedItems = selected;
        multi.Model.SetRows(multi.BuildRows());
        multi.ChooseRow(1);
        Assert.Same(selected, multi.SelectedItems);
        Assert.Equal(["Queued", "In progress"], selected);
        multi.ClearSelection();
        Assert.Empty(selected);
    }

    [Fact]
    public void Changes_to_an_observable_selection_update_the_control()
    {
        var multi = Make();
        var selected = new ObservableCollection<string>();
        multi.SelectedItems = selected;
        selected.Add("Failed");
        Assert.Equal(1, multi.SelectedCount);
        selected.Clear();
        Assert.Equal(0, multi.SelectedCount);
    }

    [Fact]
    public void Clear_selection_empties_and_reports_once()
    {
        var multi = Make();
        multi.SelectedItems = new List<string> { "Failed", "Queued" };
        var count = 0;
        multi.SelectionChanged += (_, _) => count++;
        multi.ClearSelection();
        Assert.Equal(0, multi.SelectedCount);
        Assert.Equal(1, count);
        multi.ClearSelection();
        Assert.Equal(1, count); // nothing to clear
    }

    [Fact]
    public void A_read_only_or_disabled_multi_select_cannot_be_cleared_or_opened()
    {
        var multi = Make();
        multi.SelectedItems = new List<string> { "Failed" };
        multi.IsReadOnly = true;
        multi.ClearSelection();
        Assert.Equal(1, multi.SelectedCount);
        multi.IsOpen = true;
        Assert.False(multi.IsOpen);
    }

    [Fact]
    public void Items_that_leave_the_source_leave_the_selection()
    {
        var items = new ObservableCollection<string>(Statuses);
        var multi = new CdsMultiSelect { ItemsSource = items };
        multi.SelectedItems = new List<string> { "Failed", "Queued" };
        items.Remove("Queued");
        Assert.Equal(1, multi.SelectedCount);
        Assert.Equal(["Failed"], multi.SelectedItems!.Cast<string>());
    }

    [Fact]
    public void Top_after_reopen_lists_the_rows_selected_when_the_list_opened_first()
    {
        var multi = Make();
        multi.SelectedItems = new List<string> { "Queued", "Failed" };
        // Not opened yet: nothing was selected when "opening", so the order is the items' order.
        Assert.Equal(Statuses, multi.RowTexts);
        multi.Opening();
        Assert.Equal(["Failed", "Queued", "Succeeded", "In progress", "Cancelled"], multi.RowTexts);
    }

    [Fact]
    public void Top_after_reopen_does_not_move_rows_while_toggling()
    {
        var multi = Make();
        multi.Opening();
        multi.Model.SetRows(multi.BuildRows());
        multi.ChooseRow(3);
        Assert.Equal(Statuses, multi.RowTexts);
        multi.Opening(); // reopen
        Assert.Equal("Cancelled", multi.RowTexts.First());
    }

    [Fact]
    public void Top_moves_rows_as_they_are_chosen()
    {
        var multi = Make();
        multi.SelectionFeedback = CdsMultiSelectFeedback.Top;
        multi.Model.SetRows(multi.BuildRows());
        multi.ChooseRow(3);
        Assert.Equal("Cancelled", multi.RowTexts.First());
        multi.ChooseRow(multi.VisibleRows.ToList().FindIndex(r => (string?)r.Value == "Queued"));
        Assert.Equal(["Cancelled", "Queued", "Succeeded", "In progress", "Failed"], multi.RowTexts);
    }

    [Fact]
    public void Fixed_keeps_the_order_of_the_items()
    {
        var multi = Make();
        multi.SelectionFeedback = CdsMultiSelectFeedback.Fixed;
        multi.SelectedItems = new List<string> { "Queued" };
        multi.Opening();
        Assert.Equal(Statuses, multi.RowTexts);
    }

    [Fact]
    public void Turning_sort_selected_first_off_keeps_the_order_whatever_the_feedback()
    {
        var multi = Make();
        multi.SelectionFeedback = CdsMultiSelectFeedback.Top;
        multi.SortSelectedFirst = false;
        multi.SelectedItems = new List<string> { "Queued" };
        Assert.Equal(Statuses, multi.RowTexts);
    }

    [Fact]
    public void A_filterable_multi_select_filters_the_rows_by_the_typed_text()
    {
        var multi = Make();
        multi.IsFilterable = true;
        Assert.True(multi.UsesTextField);
        multi.TextEntry.Text = "ed";
        Assert.Equal(["Succeeded", "Failed", "Cancelled", "Queued"], multi.RowTexts);
        multi.TextEntry.Text = "";
        Assert.Equal(5, multi.RowTexts.Count());
    }

    [Fact]
    public void Filtering_keeps_selected_rows_first_after_reopen()
    {
        var multi = Make();
        multi.IsFilterable = true;
        multi.SelectedItems = new List<string> { "Cancelled" };
        multi.Opening();
        multi.TextEntry.Text = "ed";
        Assert.Equal(["Cancelled", "Succeeded", "Failed", "Queued"], multi.RowTexts);
    }

    [Fact]
    public void A_non_filterable_multi_select_has_no_text_field()
    {
        Assert.False(Make().UsesTextField);
    }

    [Fact]
    public void The_inline_type_makes_the_field_flat()
    {
        var multi = Make();
        multi.Type = CdsListBoxType.Inline;
        Assert.True(multi.ChromeIsFlat);
    }

    [Fact]
    public void Rows_report_checked_state_from_the_selection()
    {
        var multi = Make();
        multi.SelectedItems = new List<string> { "Failed" };
        Assert.True(multi.IsRowChosen(multi.AllRows[2]));
        Assert.False(multi.IsRowChosen(multi.AllRows[0]));
    }
}
