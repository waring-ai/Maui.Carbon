namespace Maui.Carbon.Tests.Components;

public class ComboBoxTests
{
    static readonly string[] Names = ["Alice Adams", "Bob Brown", "Carol Clark", "Dan Davis", "Eve Evans"];

    sealed class TestCombo : CdsComboBox
    {
        public CdsNativeEntry TextEntry => Entry;

        public bool ChromeIsInline => Chrome.IsInline;

        public void Complete() => ((IEntry)Entry).Completed();

        public bool WantsEscape => WantsEscapeKey;
    }

    static TestCombo Make() => new() { Label = "Person", Placeholder = "Type a name", ItemsSource = Names };

    [Fact]
    public void Constructing_and_setting_every_property_does_not_throw()
    {
        var combo = new CdsComboBox
        {
            Label = "Person",
            HideLabel = true,
            Placeholder = "Type",
            HelperText = "h",
            IsInvalid = true,
            InvalidText = "e",
            IsWarning = true,
            WarnText = "w",
            IsReadOnly = true,
            Size = CdsSize.Sm,
            Direction = CdsListBoxDirection.Top,
            ItemsSource = Names,
            ItemToString = o => o?.ToString()?.ToUpperInvariant() ?? "",
            ItemTemplate = new DataTemplate(() => new Label()),
            IsItemDisabled = _ => false,
            AllowCustomValue = true,
            ShouldFilterItem = (_, _) => true,
            SelectedItem = Names[1],
            InputText = "bob",
            IsOpen = true,
        };
        Assert.Equal("bob", combo.InputText);
        Assert.False(combo.IsOpen); // read only
    }

    [Fact]
    public void Rows_are_all_items_until_text_is_typed_then_filter_case_insensitively()
    {
        var combo = Make();
        Assert.Equal(5, combo.BuildRows().Count);
        combo.InputText = "ev";
        Assert.Equal(["Eve Evans"], combo.BuildRows().Select(r => r.Text));
    }

    [Fact]
    public void The_default_filter_matches_anywhere_in_the_text()
    {
        var combo = Make();
        combo.InputText = "AR";
        Assert.Equal(["Carol Clark"], combo.BuildRows().Select(r => r.Text));
        combo.InputText = "a";
        Assert.Equal(["Alice Adams", "Carol Clark", "Dan Davis", "Eve Evans"], combo.BuildRows().Select(r => r.Text));
    }

    [Fact]
    public void A_custom_filter_replaces_the_default()
    {
        var combo = Make();
        combo.ShouldFilterItem = (item, input) => ((string)item!).StartsWith(input, StringComparison.OrdinalIgnoreCase);
        combo.InputText = "c";
        Assert.Equal(["Carol Clark"], combo.BuildRows().Select(r => r.Text));
    }

    [Fact]
    public void The_full_list_shows_when_the_text_equals_the_selected_item()
    {
        var combo = Make();
        combo.SelectedItem = Names[2];
        Assert.Equal("Carol Clark", combo.InputText);
        Assert.Equal(5, combo.BuildRows().Count);
        combo.InputText = "Carol Cla";
        Assert.Single(combo.BuildRows());
    }

    [Fact]
    public void Selecting_an_item_fills_the_field_and_raises_the_event()
    {
        var combo = Make();
        CdsListBoxSelectionChangedEventArgs? args = null;
        combo.SelectionChanged += (_, e) => args = e;
        combo.SelectedItem = Names[1];
        Assert.Equal("Bob Brown", combo.InputText);
        Assert.Equal("Bob Brown", combo.TextEntry.Text);
        Assert.Equal(Names[1], args!.SelectedItem);
        Assert.Equal("Bob Brown", args.InputText);
    }

    [Fact]
    public void Choosing_a_row_selects_it_and_reports_it_even_when_it_was_already_selected()
    {
        var combo = Make();
        combo.Model.SetRows(combo.BuildRows());
        combo.SelectedItem = Names[0];
        var count = 0;
        combo.SelectionChanged += (_, _) => count++;
        combo.ChooseRow(0);
        Assert.Equal(1, count);
        combo.ChooseRow(1);
        Assert.Equal(2, count);
        Assert.Equal("Bob Brown", combo.InputText);
    }

    [Fact]
    public void Input_text_is_two_way_with_the_entry()
    {
        var combo = Make();
        combo.InputText = "dan";
        Assert.Equal("dan", combo.TextEntry.Text);
        combo.TextEntry.Text = "eve";
        Assert.Equal("eve", combo.InputText);
    }

    [Fact]
    public void Input_text_changed_reports_typing()
    {
        var combo = Make();
        var texts = new List<string>();
        combo.InputTextChanged += (_, e) => texts.Add(e.NewTextValue);
        combo.TextEntry.Text = "a";
        combo.TextEntry.Text = "al";
        Assert.Equal(["a", "al"], texts);
    }

    [Fact]
    public void Typing_does_not_change_the_selection_until_it_is_committed()
    {
        var combo = Make();
        combo.SelectedItem = Names[0];
        combo.TextEntry.Text = "Bo";
        Assert.Equal(Names[0], combo.SelectedItem);
    }

    [Fact]
    public void Clear_empties_text_and_selection_and_raises_the_event()
    {
        var combo = Make();
        combo.SelectedItem = Names[3];
        CdsListBoxSelectionChangedEventArgs? args = null;
        combo.SelectionChanged += (_, e) => args = e;
        combo.Clear();
        Assert.Equal(string.Empty, combo.InputText);
        Assert.Null(combo.SelectedItem);
        Assert.Null(args!.SelectedItem);
    }

    [Fact]
    public void Clear_on_an_empty_field_still_raises_the_event_once()
    {
        var combo = Make();
        var count = 0;
        combo.SelectionChanged += (_, _) => count++;
        combo.InputText = "xyz";
        combo.Clear();
        Assert.Equal(1, count);
    }

    [Fact]
    public void A_custom_value_is_kept_on_enter_and_reported_without_an_item()
    {
        var combo = Make();
        combo.AllowCustomValue = true;
        combo.SelectedItem = Names[0];
        var events = new List<CdsListBoxSelectionChangedEventArgs>();
        combo.SelectionChanged += (_, e) => events.Add(e);
        combo.TextEntry.Text = "Zed Zimmer";
        combo.Complete();
        Assert.Null(combo.SelectedItem);
        Assert.Equal("Zed Zimmer", combo.InputText);
        var last = Assert.Single(events);
        Assert.Null(last.SelectedItem);
        Assert.Equal("Zed Zimmer", last.InputText);
    }

    [Fact]
    public void A_custom_value_with_no_prior_selection_is_reported_once()
    {
        var combo = Make();
        combo.AllowCustomValue = true;
        var count = 0;
        combo.SelectionChanged += (_, _) => count++;
        combo.TextEntry.Text = "Zed";
        combo.Complete();
        Assert.Equal(1, count);
    }

    [Fact]
    public void Enter_without_custom_values_keeps_the_text_for_the_list_to_resolve()
    {
        var combo = Make();
        combo.TextEntry.Text = "zzz";
        combo.Complete();
        Assert.Null(combo.SelectedItem);
        Assert.Equal("zzz", combo.InputText);
    }

    [Fact]
    public void A_disabled_combo_box_cannot_open()
    {
        var combo = Make();
        combo.IsEnabled = false;
        combo.IsOpen = true;
        Assert.False(combo.IsOpen);
    }

    [Fact]
    public void The_combo_box_has_no_inline_variant()
    {
        var combo = Make();
        combo.Type = CdsListBoxType.Inline;
        Assert.False(combo.ChromeIsInline);
    }

    [Fact]
    public void Escape_clears_the_text_only_when_the_list_is_closed_and_there_is_text()
    {
        var combo = Make();
        Assert.False(combo.WantsEscape);
        combo.InputText = "x";
        Assert.True(combo.WantsEscape);
        combo.IsOpen = true;
        Assert.False(combo.WantsEscape);
    }

    [Fact]
    public void Opening_on_typed_text_highlights_the_first_match()
    {
        using var host = FormsTestHost.Load(() => new CdsComboBox { Label = "Zone", ItemsSource = Names, InputText = "a" }, out var combo);
        FormsTestHost.SizeField(combo);
        combo.IsOpen = true;
        Assert.Equal("Alice Adams", combo.Model.HighlightedRow?.Text);
    }

    [Fact]
    public void Opening_with_a_chosen_item_highlights_it_not_the_first_row()
    {
        using var host = FormsTestHost.Load(() => new CdsComboBox { Label = "Zone", ItemsSource = Names, SelectedItem = Names[2] }, out var combo);
        FormsTestHost.SizeField(combo);
        combo.IsOpen = true;
        Assert.Equal("Carol Clark", combo.Model.HighlightedRow?.Text);
        combo.IsOpen = false;
        combo.SelectedItem = null;
        combo.InputText = string.Empty;
        combo.IsOpen = true;
        Assert.Null(combo.Model.HighlightedRow); // no text, no selection: nothing to highlight
    }
}
