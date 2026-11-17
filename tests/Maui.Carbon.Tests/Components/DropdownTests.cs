using System.Collections.ObjectModel;

namespace Maui.Carbon.Tests.Components;

public class DropdownTests
{
    sealed record Region(string Id, string Name);

    static readonly Region[] Regions =
    [
        new("us-south", "Dallas"),
        new("us-east", "Washington DC"),
        new("eu-de", "Frankfurt"),
        new("eu-gb", "London"),
    ];

    static CdsDropdown Make() => new()
    {
        Label = "Region",
        Placeholder = "Choose a region",
        ItemsSource = Regions,
        ItemToString = r => ((Region)r!).Name,
    };

    [Fact]
    public void Constructing_and_setting_every_property_does_not_throw()
    {
        var dropdown = new CdsDropdown
        {
            Label = "Region",
            HideLabel = true,
            Placeholder = "Choose",
            HelperText = "Pick one",
            IsInvalid = true,
            InvalidText = "Required",
            IsWarning = true,
            WarnText = "Careful",
            IsReadOnly = true,
            IsEnabled = false,
            Size = CdsSize.Lg,
            Type = CdsListBoxType.Inline,
            Direction = CdsListBoxDirection.Top,
            ItemsSource = new[] { "a", "b" },
            ItemTemplate = new DataTemplate(() => new Label()),
            IsItemDisabled = _ => false,
            SelectedItem = "a",
            IsOpen = true,
        };
        Assert.Equal("a", dropdown.SelectedItem);
        Assert.False(dropdown.IsOpen); // disabled controls cannot open
    }

    [Fact]
    public void Rows_come_from_the_items_source_with_item_to_string()
    {
        var dropdown = Make();
        Assert.Equal(["Dallas", "Washington DC", "Frankfurt", "London"], dropdown.AllRows.Select(r => r.Text));
        Assert.Equal(Regions[2], dropdown.AllRows[2].Value);
    }

    [Fact]
    public void Without_item_to_string_the_row_text_is_to_string()
    {
        var dropdown = new CdsDropdown { ItemsSource = new object?[] { "one", 2, null } };
        Assert.Equal(["one", "2", ""], dropdown.AllRows.Select(r => r.Text));
    }

    [Fact]
    public void Disabled_items_come_from_the_predicate()
    {
        var dropdown = Make();
        dropdown.IsItemDisabled = r => ((Region)r!).Id == "us-east";
        Assert.Equal([false, true, false, false], dropdown.AllRows.Select(r => r.IsDisabled));
    }

    [Fact]
    public void An_observable_source_updates_the_rows()
    {
        var source = new ObservableCollection<string> { "a", "b" };
        var dropdown = new CdsDropdown { ItemsSource = source };
        source.Add("c");
        Assert.Equal(3, dropdown.AllRows.Count);
        source.RemoveAt(0);
        Assert.Equal(["b", "c"], dropdown.AllRows.Select(r => r.Text));
    }

    [Fact]
    public void Selecting_an_item_raises_selection_changed_with_the_item()
    {
        var dropdown = Make();
        CdsListBoxSelectionChangedEventArgs? args = null;
        dropdown.SelectionChanged += (_, e) => args = e;
        dropdown.SelectedItem = Regions[1];
        Assert.NotNull(args);
        Assert.Equal(Regions[1], args!.SelectedItem);
        Assert.Equal([Regions[1]], args.SelectedItems);

        dropdown.SelectedItem = null;
        Assert.Null(args!.SelectedItem);
    }

    [Fact]
    public void Clearing_the_selection_reports_no_items()
    {
        var dropdown = Make();
        dropdown.SelectedItem = Regions[0];
        CdsListBoxSelectionChangedEventArgs? args = null;
        dropdown.SelectionChanged += (_, e) => args = e;
        dropdown.SelectedItem = null;
        Assert.NotNull(args);
        Assert.Null(args!.SelectedItem);
        Assert.Empty(args.SelectedItems);
    }

    [Fact]
    public void Selected_item_and_is_open_are_two_way_properties()
    {
        Assert.Equal(BindingMode.TwoWay, CdsDropdown.SelectedItemProperty.DefaultBindingMode);
        Assert.Equal(BindingMode.TwoWay, CdsListBox.IsOpenProperty.DefaultBindingMode);
    }

    [Fact]
    public void A_disabled_or_read_only_dropdown_cannot_be_opened()
    {
        var disabled = Make();
        disabled.IsEnabled = false;
        disabled.IsOpen = true;
        Assert.False(disabled.IsOpen);

        var readOnly = Make();
        readOnly.IsReadOnly = true;
        readOnly.IsOpen = true;
        Assert.False(readOnly.IsOpen);
    }

    [Fact]
    public void Setting_is_open_before_the_control_loads_is_remembered()
    {
        var dropdown = Make();
        dropdown.IsOpen = true;
        Assert.True(dropdown.IsOpen);
        Assert.False(dropdown.IsLoaded);
    }

    [Fact]
    public void Disabling_an_open_dropdown_closes_it()
    {
        var dropdown = Make();
        dropdown.IsOpen = true;
        dropdown.IsEnabled = false;
        Assert.False(dropdown.IsOpen);
    }

    [Fact]
    public void Making_the_dropdown_read_only_closes_it()
    {
        var dropdown = Make();
        dropdown.IsOpen = true;
        dropdown.IsReadOnly = true;
        Assert.False(dropdown.IsOpen);
    }

    [Fact]
    public void Type_ahead_matches_by_prefix_and_reports_misses()
    {
        var dropdown = Make();
        Assert.True(dropdown.TypeAhead('f'));
        Assert.Equal(2, dropdown.Model.Highlighted);
        Assert.False(dropdown.TypeAhead('q'));
    }

    [Fact]
    public void Type_ahead_does_nothing_on_a_disabled_dropdown()
    {
        var dropdown = Make();
        dropdown.IsEnabled = false;
        Assert.False(dropdown.TypeAhead('f'));
    }

    [Fact]
    public void The_chosen_row_is_the_selected_item()
    {
        var dropdown = Make();
        dropdown.SelectedItem = Regions[3];
        Assert.False(dropdown.IsRowChosen(dropdown.AllRows[0]));
        Assert.True(dropdown.IsRowChosen(dropdown.AllRows[3]));
    }

    [Fact]
    public void Choosing_a_row_selects_its_item()
    {
        var dropdown = Make();
        dropdown.Model.SetRows(dropdown.BuildRows());
        dropdown.ChooseRow(2);
        Assert.Equal(Regions[2], dropdown.SelectedItem);
    }

    [Fact]
    public void Inline_type_makes_the_field_flat_with_the_label_to_the_start()
    {
        var dropdown = new TestDropdown { Type = CdsListBoxType.Inline };
        Assert.True(dropdown.ChromeIsFlat);
        Assert.True(dropdown.ChromeIsInline);
        dropdown.Type = CdsListBoxType.Default;
        Assert.False(dropdown.ChromeIsFlat);
        Assert.False(dropdown.ChromeIsInline);
    }

    [Fact]
    public void The_field_height_follows_the_size_and_clamps_to_sm_to_lg()
    {
        var dropdown = new TestDropdown();
        dropdown.Size = CdsSize.Sm;
        Assert.Equal(32, dropdown.FieldHeight);
        dropdown.Size = CdsSize.Lg;
        Assert.Equal(48, dropdown.FieldHeight);
        dropdown.Size = CdsSize.Xs;
        Assert.Equal(32, dropdown.FieldHeight);
    }

    sealed class TestDropdown : CdsDropdown
    {
        public bool ChromeIsFlat => Chrome.IsFlat;

        public bool ChromeIsInline => Chrome.IsInline;

        public double FieldHeight => Chrome.FieldHeight;
    }

    [Fact]
    public void The_skeleton_follows_size_and_hides_its_label()
    {
        var skeleton = new CdsDropdownSkeleton { Size = CdsSize.Lg, HideLabel = true };
        Assert.Equal(CdsSize.Lg, skeleton.Size);
        Assert.True(skeleton.HideLabel);
    }
}
