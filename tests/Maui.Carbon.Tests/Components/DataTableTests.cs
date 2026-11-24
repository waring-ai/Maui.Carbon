using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

public class DataTableTests
{
    // ----- CN-11: row controls are named by their row -----

    [Fact]
    public void Row_checkboxes_and_menus_are_named_by_the_first_cell()
    {
        var table = CreateTable();
        table.Selection = CdsDataTableSelection.Multi;
        table.RowActions.Add(new CdsDataTableRowAction { Label = "Delete" });
        var names = table.Table.Rows.Select(r => ((CdsCheckbox)r.Cells[0].Body!).Label ?? "").ToArray();
        Assert.Equal(["Select row, prod-eu-de-1", "Select row, staging-us-east", "Select row, dev-sandbox"], names);
        Assert.Equal("Options for prod-eu-de-1", ((CdsOverflowMenu)table.Table.Rows[0].Cells[^1].Body!).IconDescription);
    }

    [Fact]
    public void Row_names_can_come_from_a_selector_and_follow_sorting()
    {
        var table = CreateTable();
        table.Selection = CdsDataTableSelection.Radio;
        table.RowNameSelector = r => $"cluster {((Cluster)r).Id}";
        Assert.Equal("Select row, cluster c1", ((CdsRadioButton)table.Table.Rows[0].Cells[0].Body!).Label);
        table.SortKey = "nodes";
        table.SortDirection = CdsSortDirection.Ascending;
        Assert.Equal("Select row, cluster c3", ((CdsRadioButton)table.Table.Rows[0].Cells[0].Body!).Label);
        // An empty name falls back to the first cell.
        table.RowNameSelector = _ => "";
        Assert.Equal("Select row, dev-sandbox", ((CdsRadioButton)table.Table.Rows[0].Cells[0].Body!).Label);
    }

    // ----- CN-12: text cells carry their column header -----

    [Fact]
    public void Text_cells_are_read_with_their_column_header()
    {
        Assert.Equal("Region: eu-de", CdsTable.CellName("Region", "eu-de"));
        Assert.Null(CdsTable.CellName(null, "eu-de"));
        Assert.Null(CdsTable.CellName("Region", ""));

        var table = CreateTable();
        var descriptions = ContainerSemanticsAuditTests.Walk(table.Table).OfType<CdsText>()
            .Select(SemanticProperties.GetDescription).Where(d => d is not null).ToList();
        Assert.Contains("Name: prod-eu-de-1", descriptions);
        Assert.Contains("Region: us-east", descriptions);
        Assert.Contains("Nodes: 12", descriptions);
    }

    [Fact]
    public void A_cell_text_change_updates_its_name()
    {
        var table = new CdsTable();
        var head = new CdsTableHead();
        head.Headers.Add(new CdsTableHeader { Text = "Status" });
        table.Head = head;
        var cell = new CdsTableCell { Text = "Running" };
        var row = new CdsTableRow();
        row.Cells.Add(cell);
        table.Rows.Add(row);
        cell.Text = "Stopped";
        Assert.Contains(ContainerSemanticsAuditTests.Walk(table).OfType<CdsText>(), l => SemanticProperties.GetDescription(l) == "Status: Stopped");
    }

    // ----- CN-14: actions follow Command.CanExecute -----

    sealed class Gate(Func<object?, bool> canExecute) : System.Windows.Input.ICommand
    {
        public int Executed;
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter) => canExecute(parameter);
        public void Execute(object? parameter) => Executed++;
        public void Raise() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    [Fact]
    public void Batch_buttons_are_disabled_while_their_command_cannot_run_for_the_selection()
    {
        var allow = false;
        var gate = new Gate(p => allow && p is IReadOnlyList<object> { Count: > 0 });
        var table = CreateTable();
        table.Selection = CdsDataTableSelection.Multi;
        table.BatchActions.Add(new CdsDataTableBatchAction { Label = "Restart", Command = gate });
        table.SelectedRows = new List<object> { Clusters[0] };
        var button = ContainerSemanticsAuditTests.Walk(table).OfType<CdsBatchBarButton>()
            .Single(b => ContainerSemanticsAuditTests.Walk(b).OfType<CdsHitTarget>().Any(h => SemanticProperties.GetDescription(h) == "Restart"));
        Assert.False(button.IsEnabled);
        allow = true;
        gate.Raise();
        Assert.True(button.IsEnabled);
        // CanExecute is asked again with the new selection.
        table.ClearSelection();
        Assert.False(button.IsEnabled);
    }

    [Fact]
    public void Row_menu_items_are_disabled_for_rows_their_command_cannot_run_for()
    {
        var gate = new Gate(r => r is Cluster { Nodes: > 5 });
        var action = new CdsDataTableRowAction { Label = "Resize", Command = gate };
        var invoked = new List<object>();
        action.Invoked += (_, e) => invoked.Add(e.Row);
        var table = CreateTable();
        table.RowActions.Add(action);
        CdsMenuItem Item(int row) => (CdsMenuItem)((CdsOverflowMenu)table.Table.Rows[row].Cells[^1].Body!).Items[0];
        Assert.True(Item(0).IsActionable);   // prod-eu-de-1, 12 nodes
        Assert.False(Item(1).IsActionable);  // staging-us-east, 3 nodes
        Item(0).Invoke();
        Item(1).Invoke();
        Assert.Equal([Clusters[0]], invoked.Where(r => ReferenceEquals(r, Clusters[0])));
        Assert.Equal(1, gate.Executed);
        // The item follows the row it is bound to now.
        table.SortKey = "nodes";
        table.SortDirection = CdsSortDirection.Descending;
        Assert.True(Item(0).IsActionable);
        Assert.False(Item(2).IsActionable);
    }

    // ----- CN-15: title heading level -----

    [Fact]
    public void The_title_is_a_level_4_heading_like_the_page_header_and_can_be_changed()
    {
        var table = CreateTable();
        table.Title = "Clusters";
        var title = ContainerSemanticsAuditTests.Walk(table).OfType<CdsText>().Single(t => t.Text == "Clusters");
        Assert.Equal(SemanticHeadingLevel.Level4, SemanticProperties.GetHeadingLevel(title));
        table.TitleHeadingLevel = 2;
        Assert.Equal(SemanticHeadingLevel.Level2, SemanticProperties.GetHeadingLevel(title));
        table.TitleHeadingLevel = 9;
        Assert.Equal(6, table.TitleHeadingLevel);
    }

    // ----- tokens -----

    [Fact]
    public void Header_is_layer_accent_and_the_active_or_hovered_sort_header_is_selected_hover()
    {
        Assert.Equal(CdsColorKeys.LayerAccent01, CdsTableTokens.HeaderFill(sortable: true, hover: false, active: false));
        Assert.Equal(CdsColorKeys.LayerSelectedHover01, CdsTableTokens.HeaderFill(true, true, false));
        Assert.Equal(CdsColorKeys.LayerSelectedHover01, CdsTableTokens.HeaderFill(true, false, true));
        Assert.Equal(CdsColorKeys.LayerAccent01, CdsTableTokens.HeaderFill(false, true, false));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsTableTokens.HeaderText);
    }

    [Fact]
    public void Row_fill_follows_selected_hover_and_zebra()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsTableTokens.RowFill(false, false, false));
        Assert.Equal(CdsColorKeys.LayerAccent01, CdsTableTokens.RowFill(false, false, true));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsTableTokens.RowFill(false, true, true));
        Assert.Equal(CdsColorKeys.LayerSelected01, CdsTableTokens.RowFill(true, false, true));
        Assert.Equal(CdsColorKeys.LayerSelectedHover01, CdsTableTokens.RowFill(true, true, false));
    }

    [Fact]
    public void Row_rules_are_subtle_and_vanish_into_hover_and_zebra_stripes()
    {
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsTableTokens.RowRule(false, false, false, false));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsTableTokens.RowRule(false, true, false, false));
        Assert.Equal(CdsColorKeys.LayerSelectedHover01, CdsTableTokens.RowRule(true, true, false, false));
        Assert.Equal(CdsColorKeys.LayerActive01, CdsTableTokens.RowRule(true, false, false, false));
        Assert.Equal(CdsColorKeys.Layer01, CdsTableTokens.RowRule(false, false, true, false));
        Assert.Equal(CdsColorKeys.LayerAccent01, CdsTableTokens.RowRule(false, false, true, true));
    }

    [Fact]
    public void Cell_text_is_secondary_until_hovered_or_selected()
    {
        Assert.Equal(CdsColorKeys.TextSecondary, CdsTableTokens.CellText(false));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsTableTokens.CellText(true));
    }

    [Fact]
    public void Batch_bar_is_background_brand_with_text_on_color()
    {
        Assert.Equal(CdsColorKeys.BackgroundBrand, CdsTableTokens.BatchBar);
        Assert.Equal(CdsColorKeys.TextOnColor, CdsTableTokens.BatchText);
        Assert.Equal(CdsColorKeys.Layer01, CdsTableTokens.Toolbar);
    }

    [Theory]
    [InlineData(CdsSize.Xs, 24)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 64)]
    [InlineData(CdsSize.Xxl, 64)]
    public void Row_heights(CdsSize size, double height) => Assert.Equal(height, CdsTableTokens.RowHeight(size));

    [Theory]
    [InlineData(CdsSize.Xs, 32)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 48)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    public void Toolbar_is_32_for_xs_and_sm_and_48_otherwise(CdsSize size, double height) =>
        Assert.Equal(height, CdsTableTokens.ToolbarHeight(size));

    [Fact]
    public void Column_widths_and_child_insets_follow_the_readme()
    {
        Assert.Equal(40, CdsTableTokens.CheckboxColumnWidth);
        Assert.Equal(48, CdsTableTokens.RadioColumnWidth);
        Assert.Equal(52, CdsTableTokens.MenuColumnWidth);
        Assert.Equal(56, CdsTableTokens.ChildInset(CdsDataTableSelection.None));
        Assert.Equal(88, CdsTableTokens.ChildInset(CdsDataTableSelection.Multi));
        Assert.Equal(88, CdsTableTokens.ChildInset(CdsDataTableSelection.Radio));
        Assert.Equal(300, CdsTableTokens.StickyBodyHeight);
    }

    [Theory]
    [InlineData(CdsSize.Xs, CdsSize.Xs)]
    [InlineData(CdsSize.Md, CdsSize.Md)]
    [InlineData(CdsSize.Lg, CdsSize.Lg)]
    [InlineData(CdsSize.Xl, CdsSize.Lg)]
    public void Menu_trigger_matches_the_row_height_with_xl_using_lg(CdsSize table, CdsSize trigger) =>
        Assert.Equal(trigger, CdsTableTokens.MenuTriggerSize(table));

    [Fact]
    public void Overflow_menu_is_hidden_until_hover_or_focus_except_on_touch_or_when_always_visible()
    {
        Assert.False(CdsTableTokens.IsOverflowRevealed(hoverOnly: true, touchPlatform: false, rowHover: false, focusedOrOpen: false));
        Assert.True(CdsTableTokens.IsOverflowRevealed(true, false, true, false));
        Assert.True(CdsTableTokens.IsOverflowRevealed(true, false, false, true));
        Assert.True(CdsTableTokens.IsOverflowRevealed(true, true, false, false));
        Assert.True(CdsTableTokens.IsOverflowRevealed(false, false, false, false));
    }

    [Fact]
    public void Cell_padding_follows_the_row_size()
    {
        Assert.Equal((2d, 2d), CdsTableTokens.CellPaddingY(CdsSize.Xs));
        Assert.Equal((7d, 6d), CdsTableTokens.CellPaddingY(CdsSize.Md));
        Assert.Equal((0d, 0d), CdsTableTokens.CellPaddingY(CdsSize.Lg));
        Assert.Equal((16d, 16d), CdsTableTokens.CellPaddingY(CdsSize.Xl));
    }

    // ----- constructing -----

    sealed record Cluster(string Id, string Name, string Region, int Nodes);

    static readonly Cluster[] Clusters =
    [
        new("c1", "prod-eu-de-1", "eu-de", 12),
        new("c2", "staging-us-east", "us-east", 3),
        new("c3", "dev-sandbox", "us-south", 1),
    ];

    static CdsDataTable CreateTable()
    {
        var table = new CdsDataTable
        {
            RowKeySelector = r => ((Cluster)r).Id,
            ItemsSource = Clusters,
        };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", ValueSelector = r => ((Cluster)r).Name });
        table.Headers.Add(new CdsDataTableHeader { Key = "region", Header = "Region", ValueSelector = r => ((Cluster)r).Region });
        table.Headers.Add(new CdsDataTableHeader { Key = "nodes", Header = "Nodes", Align = CdsTableAlignment.End, ValueSelector = r => ((Cluster)r).Nodes });
        return table;
    }

    [Fact]
    public void A_table_can_be_created_and_every_property_set_without_a_platform()
    {
        var table = CreateTable();
        table.Title = "Clusters";
        table.Description = "Kubernetes clusters in your account.";
        table.Size = CdsSize.Md;
        table.UseZebraStyles = true;
        table.IsSortable = true;
        table.Selection = CdsDataTableSelection.Multi;
        table.IsSearchable = true;
        table.IsPersistentSearch = true;
        table.SearchPlaceholder = "Filter clusters";
        table.StickyHeader = true;
        table.OverflowMenuOnHover = false;
        table.RowActions.Add(new CdsDataTableRowAction { Label = "View details" });
        table.RowActions.Add(new CdsDataTableRowAction { Label = "Delete", Kind = CdsMenuItemKind.Danger });
        table.BatchActions.Add(new CdsDataTableBatchAction { Label = "Restart", Icon = CdsIconName.Restart });
        table.EmptyState = new CdsDataTableEmptyState { Title = "No clusters yet", Description = "Create a cluster.", ActionText = "Create cluster" };
        table.IsRowDisabled = r => ((Cluster)r).Id == "c3";
        table.IsExpandable = true;
        table.ExpandedRowTemplate = new DataTemplate(() => new Label());
        table.SortKey = "nodes";
        table.SortDirection = CdsSortDirection.Descending;
        table.SearchText = "prod";
        Assert.Equal(1, table.VisibleRowCount);
        Assert.Equal(CdsSize.Md, table.Size);
        Assert.Equal(3, table.Headers.Count);
    }

    [Fact]
    public void Defaults_follow_the_readme()
    {
        var table = new CdsDataTable();
        Assert.Equal(CdsSize.Lg, table.Size);
        Assert.Equal(CdsDataTableSelection.None, table.Selection);
        Assert.False(table.IsSortable);
        Assert.False(table.UseZebraStyles);
        Assert.True(table.OverflowMenuOnHover);
        Assert.Equal("Actions", table.RowActionsHeader);
        Assert.Equal(CdsSortDirection.None, table.SortDirection);
        Assert.Empty(table.SelectedRows);
    }

    [Fact]
    public void Rows_come_from_the_items_in_order_and_sorting_reorders_them()
    {
        var table = CreateTable();
        table.IsSortable = true;
        Assert.Equal(3, table.Table.Rows.Count);
        Assert.Equal(["prod-eu-de-1", "staging-us-east", "dev-sandbox"], table.Table.Rows.Select(r => r.Cells[0].Text));
        table.SortKey = "nodes";
        table.SortDirection = CdsSortDirection.Ascending;
        Assert.Equal(["dev-sandbox", "staging-us-east", "prod-eu-de-1"], table.Table.Rows.Select(r => r.Cells[0].Text));
        table.SortDirection = CdsSortDirection.Descending;
        Assert.Equal("prod-eu-de-1", table.Table.Rows[0].Cells[0].Text);
        table.SortDirection = CdsSortDirection.None;
        Assert.Equal("prod-eu-de-1", table.Table.Rows[0].Cells[0].Text);
        Assert.Equal("dev-sandbox", table.Table.Rows[2].Cells[0].Text);
    }

    [Fact]
    public void The_search_text_filters_the_rows()
    {
        var table = CreateTable();
        table.SearchText = "US-";
        Assert.Equal(2, table.VisibleRowCount);
        Assert.Equal(2, table.Table.Rows.Count);
        table.SearchText = null;
        Assert.Equal(3, table.Table.Rows.Count);
    }

    [Fact]
    public void Selected_rows_are_two_way_and_drive_the_row_state()
    {
        var table = CreateTable();
        table.Selection = CdsDataTableSelection.Multi;
        table.SelectedRows = new List<object> { Clusters[1] };
        Assert.Equal(1, table.SelectedCount);
        Assert.True(table.Table.Rows[1].IsSelected);
        Assert.False(table.Table.Rows[0].IsSelected);
        table.SelectedRows = new List<object>();
        Assert.Equal(0, table.SelectedCount);
        Assert.False(table.Table.Rows[1].IsSelected);
    }

    [Fact]
    public void Selection_none_ignores_selected_rows_and_radio_keeps_one()
    {
        var table = CreateTable();
        table.SelectedRows = new List<object> { Clusters[0], Clusters[1] };
        Assert.Equal(0, table.SelectedCount); // no selection column: nothing can be selected
        table.Selection = CdsDataTableSelection.Multi;
        table.SelectedRows = new List<object> { Clusters[0], Clusters[1] };
        Assert.Equal(2, table.SelectedCount);
        table.Selection = CdsDataTableSelection.Radio;
        Assert.Equal(1, table.SelectedCount);
        table.Selection = CdsDataTableSelection.None;
        Assert.Equal(0, table.SelectedCount);
        Assert.Empty(table.SelectedRows);
    }

    [Fact]
    public void Clear_selection_empties_the_selected_rows_and_raises_the_event()
    {
        var table = CreateTable();
        table.Selection = CdsDataTableSelection.Multi;
        table.SelectedRows = new List<object> { Clusters[0], Clusters[2] };
        IReadOnlyList<object>? seen = null;
        table.SelectionChanged += (_, e) => seen = e.Rows;
        table.ClearSelection();
        Assert.NotNull(seen);
        Assert.Empty(seen!);
        Assert.Empty(table.SelectedRows);
    }

    [Fact]
    public void Expanded_rows_create_their_content_lazily_from_the_template()
    {
        var table = CreateTable();
        table.IsExpandable = true;
        var created = 0;
        table.ExpandedRowTemplate = new DataTemplate(() =>
        {
            created++;
            return new Label();
        });
        Assert.Equal(0, created);
        Assert.All(table.Table.Rows, r => Assert.Null(r.ExpandedContent));
        table.ExpandedRows = new List<object> { Clusters[0] };
        Assert.Equal(1, created);
        Assert.True(table.Table.Rows[0].IsExpanded);
        Assert.NotNull(table.Table.Rows[0].ExpandedContent);
        Assert.Same(Clusters[0], table.Table.Rows[0].ExpandedContent!.BindingContext);
        Assert.False(table.Table.Rows[1].IsExpanded);
    }

    [Fact]
    public void System_columns_are_added_for_expansion_selection_and_row_actions()
    {
        var table = CreateTable();
        table.Selection = CdsDataTableSelection.Multi;
        table.IsExpandable = true;
        table.ExpandedRowTemplate = new DataTemplate(() => new Label());
        table.RowActions.Add(new CdsDataTableRowAction { Label = "Delete" });
        // expand + checkbox + 3 data + menu
        Assert.Equal(6, table.Table.Head!.Headers.Count);
        Assert.All(table.Table.Rows, r => Assert.Equal(6, r.Cells.Count));
        Assert.Equal(40, table.Table.Head!.Headers[0].Width.Value);
        Assert.Equal(40, table.Table.Head!.Headers[1].Width.Value);
        Assert.Equal(52, table.Table.Head!.Headers[5].Width.Value);
        Assert.Equal(88, table.Table.ChildInset);
    }

    [Fact]
    public void Header_sortability_follows_the_table_and_can_be_opted_out()
    {
        var table = CreateTable();
        table.IsSortable = true;
        table.Headers[1].IsSortable = false;
        Assert.True(table.Table.Head!.Headers[0].IsSortable);
        Assert.False(table.Table.Head!.Headers[1].IsSortable);
        table.IsSortable = false;
        table.Headers[1].IsSortable = true;
        Assert.False(table.Table.Head!.Headers[0].IsSortable);
        Assert.True(table.Table.Head!.Headers[1].IsSortable);
    }

    [Fact]
    public void Duplicate_row_keys_are_made_unique_and_missing_keys_use_the_position()
    {
        var table = new CdsDataTable { ItemsSource = new[] { "a", "b" } };
        table.Headers.Add(new CdsDataTableHeader { Key = "v", Header = "V", ValueSelector = r => r });
        table.Selection = CdsDataTableSelection.Multi;
        table.SelectedRows = new List<object> { "b" };
        Assert.Equal(1, table.SelectedCount);
        table.RowKeySelector = _ => "same";
        Assert.Equal(2, table.Table.Rows.Count);
    }

    [Fact]
    public void Dictionary_rows_are_read_by_key_without_a_value_selector()
    {
        var table = new CdsDataTable
        {
            ItemsSource = new[] { new Dictionary<string, object?> { ["name"] = "ci-deploy" } },
        };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name" });
        Assert.Equal("ci-deploy", table.Table.Rows[0].Cells[0].Text);
    }

    [Fact]
    public void String_format_formats_formattable_values()
    {
        var table = new CdsDataTable { ItemsSource = new[] { 1234.5 } };
        table.Headers.Add(new CdsDataTableHeader { Key = "v", Header = "V", ValueSelector = r => r, StringFormat = "N0" });
        Assert.Equal(1234.5.ToString("N0"), table.Table.Rows[0].Cells[0].Text);
    }

    [Fact]
    public void Observable_sources_rebuild_the_rows()
    {
        var items = new System.Collections.ObjectModel.ObservableCollection<Cluster>(Clusters);
        var table = new CdsDataTable { ItemsSource = items, RowKeySelector = r => ((Cluster)r).Id };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", ValueSelector = r => ((Cluster)r).Name });
        Assert.Equal(3, table.Table.Rows.Count);
        items.Add(new Cluster("c4", "qa", "eu-gb", 2));
        Assert.Equal(4, table.Table.Rows.Count);
        items.RemoveAt(0);
        Assert.Equal(3, table.Table.Rows.Count);
    }

    [Fact]
    public void Row_actions_provider_can_vary_per_row()
    {
        var table = CreateTable();
        table.RowActionsProvider = r => ((Cluster)r).Nodes > 5
            ? [new CdsDataTableRowAction { Label = "Resize" }]
            : [];
        Assert.Equal(4, table.Table.Rows[0].Cells.Count); // 3 data + the menu column
    }

    [Fact]
    public void Batch_and_row_actions_invoke_commands_and_events_with_their_rows()
    {
        object? commandArg = null;
        IReadOnlyList<object>? eventRows = null;
        var batch = new CdsDataTableBatchAction { Label = "Restart", Command = new Command<object>(o => commandArg = o) };
        batch.Invoked += (_, e) => eventRows = e.Rows;
        IReadOnlyList<object> rows = [Clusters[0], Clusters[1]];
        batch.Invoke(rows);
        Assert.Same(rows, commandArg);
        Assert.Same(rows, eventRows);

        object? rowArg = null;
        var action = new CdsDataTableRowAction { Label = "Delete", Command = new Command<object>(o => rowArg = o) };
        CdsDataTableRowEventArgs? seen = null;
        action.Invoked += (_, e) => seen = e;
        action.Invoke(Clusters[2]);
        Assert.Same(Clusters[2], rowArg);
        Assert.Same(Clusters[2], seen!.Row);
    }

    [Fact]
    public void Empty_state_runs_its_action()
    {
        var state = new CdsDataTableEmptyState { ActionText = "Create cluster" };
        var raised = 0;
        state.ActionInvoked += (_, _) => raised++;
        var ran = 0;
        state.ActionCommand = new Command(() => ran++);
        var table = new CdsDataTable { EmptyState = state };
        ((IButton)table.EmptyActionButton!.AccessibilityTarget!).Clicked();
        Assert.Equal(1, raised);
        Assert.Equal(1, ran);
    }

    // ----- row counts are observable -----

    [Fact]
    public void Visible_and_total_row_counts_are_read_only_bindable_properties()
    {
        Assert.True(CdsDataTable.VisibleRowCountProperty.IsReadOnly);
        Assert.True(CdsDataTable.TotalRowCountProperty.IsReadOnly);
        var empty = new CdsDataTable();
        Assert.Equal(0, empty.VisibleRowCount);
        Assert.Equal(0, empty.TotalRowCount);

        var rows = new System.Collections.ObjectModel.ObservableCollection<Cluster>(Clusters);
        CdsDataTable table = null!;
        Label shown = null!;
        // A page binds a label to the counts ("Showing X of N"); bindings need a dispatcher.
        using var host = FormsTestHost.Load(() =>
        {
            table = CreateTable();
            table.ItemsSource = rows;
            shown = new Label();
            shown.SetBinding(Label.TextProperty, new MultiBinding
            {
                Bindings =
                {
                    new Binding(nameof(CdsDataTable.VisibleRowCount), source: table),
                    new Binding(nameof(CdsDataTable.TotalRowCount), source: table),
                },
                StringFormat = "Showing {0} of {1}",
            });
            return new VerticalStackLayout { shown, table };
        }, out VerticalStackLayout _);
        Assert.Equal(3, table.VisibleRowCount);
        Assert.Equal(3, table.TotalRowCount);
        Assert.Equal("Showing 3 of 3", shown.Text);

        var changed = new List<string?>();
        table.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        // A search narrows the visible set; the total stays.
        table.SearchText = "us-";
        Assert.Equal(2, table.VisibleRowCount);
        Assert.Equal(3, table.TotalRowCount);
        Assert.Contains(nameof(CdsDataTable.VisibleRowCount), changed);
        Assert.DoesNotContain(nameof(CdsDataTable.TotalRowCount), changed);
        Assert.Equal("Showing 2 of 3", shown.Text);

        // The toolbar search does the same.
        table.SearchField.Text = "prod";
        Assert.Equal(1, table.VisibleRowCount);
        Assert.Equal("Showing 1 of 3", shown.Text);

        // The source changing moves both.
        changed.Clear();
        rows.Add(new Cluster("c4", "prod-us-east", "us-east", 4));
        Assert.Equal(2, table.VisibleRowCount);
        Assert.Equal(4, table.TotalRowCount);
        Assert.Contains(nameof(CdsDataTable.TotalRowCount), changed);
        Assert.Equal("Showing 2 of 4", shown.Text);

        // Unchanged counts raise nothing (sorting reorders but shows the same rows).
        changed.Clear();
        table.SortKey = "nodes";
        table.SortDirection = CdsSortDirection.Ascending;
        Assert.DoesNotContain(nameof(CdsDataTable.VisibleRowCount), changed);

        table.SearchText = null;
        table.ItemsSource = null;
        Assert.Equal(0, table.VisibleRowCount);
        Assert.Equal(0, table.TotalRowCount);
        Assert.Equal("Showing 0 of 0", shown.Text);
    }

    [Fact]
    public void Virtualized_tables_publish_the_counts_too()
    {
        var table = CreateTable();
        table.StickyHeader = true;
        Assert.Equal(3, table.VisibleRowCount);
        table.SearchText = "dev";
        Assert.Equal(1, table.VisibleRowCount);
        Assert.Equal(3, table.TotalRowCount);
    }

    // ----- the empty state's action follows its command -----

    [Fact]
    public void Empty_state_action_button_follows_CanExecute_with_its_parameter()
    {
        var allow = false;
        object? asked = null;
        var gate = new Gate(p => { asked = p; return allow; });
        var state = new CdsDataTableEmptyState
        {
            Title = "No keys match",
            ActionText = "Clear filter",
            ActionCommand = gate,
            ActionCommandParameter = "filter",
        };
        var invoked = 0;
        state.ActionInvoked += (_, _) => invoked++;
        var table = CreateTable();
        table.EmptyState = state;
        table.SearchText = "nothing matches this";

        var button = Assert.IsType<CdsButton>(table.EmptyActionButton);
        var hit = Assert.IsType<CdsHitTarget>(button.AccessibilityTarget);
        Assert.Equal("filter", asked);
        Assert.False(button.IsEnabled);
        Assert.False(hit.IsEnabled);
        Assert.True(button.State.HasFlag(CdsInteractionState.Disabled));
        ((IButton)hit).Clicked();
        Assert.Equal(0, invoked);
        Assert.Equal(0, gate.Executed);

        // CanExecuteChanged re-evaluates it.
        allow = true;
        gate.Raise();
        Assert.True(button.IsEnabled);
        Assert.True(hit.IsEnabled);
        Assert.False(button.State.HasFlag(CdsInteractionState.Disabled));
        ((IButton)hit).Clicked();
        Assert.Equal(1, invoked);
        Assert.Equal(1, gate.Executed);

        // A new command or parameter is picked up.
        var other = new Gate(p => Equals(p, "other"));
        state.ActionCommand = other;
        button = table.EmptyActionButton!;
        Assert.False(button.IsEnabled);
        state.ActionCommandParameter = "other";
        button = table.EmptyActionButton!;
        Assert.True(button.IsEnabled);
    }

    [Fact]
    public void Empty_state_without_a_command_stays_enabled_and_raises_its_event()
    {
        var state = new CdsDataTableEmptyState { ActionText = "Create cluster" };
        var invoked = 0;
        state.ActionInvoked += (_, _) => invoked++;
        var table = new CdsDataTable { EmptyState = state };
        var button = table.EmptyActionButton!;
        Assert.True(button.IsEnabled);
        ((IButton)button.AccessibilityTarget!).Clicked();
        Assert.Equal(1, invoked);
    }

    sealed record OwnTitle(string Title);

    sealed class EmptyViewModel
    {
        public string Title => "No keys yet";
        public Gate Clear { get; } = new(_ => true);
    }

    [Fact]
    public void Table_parts_get_the_table_binding_context()
    {
        var vm = new EmptyViewModel();
        var state = new CdsDataTableEmptyState { ActionText = "Clear filter" };
        state.SetBinding(CdsDataTableEmptyState.TitleProperty, new Binding(nameof(EmptyViewModel.Title)));
        state.SetBinding(CdsDataTableEmptyState.ActionCommandProperty, new Binding(nameof(EmptyViewModel.Clear)));
        var batch = new CdsDataTableBatchAction { Label = "Delete" };
        batch.SetBinding(CdsDataTableBatchAction.CommandProperty, new Binding(nameof(EmptyViewModel.Clear)));
        var table = new CdsDataTable { EmptyState = state };
        table.BatchActions.Add(batch);
        var page = new ContentPage { Content = table };
        page.BindingContext = vm;
        Assert.Same(vm, state.BindingContext);
        Assert.Equal("No keys yet", state.Title);
        Assert.Same(vm.Clear, state.ActionCommand);
        Assert.Same(vm.Clear, batch.Command);
        ((IButton)table.EmptyActionButton!.AccessibilityTarget!).Clicked();
        Assert.Equal(1, vm.Clear.Executed);

        // A state set after the context, and one with its own context.
        var late = new CdsDataTableEmptyState();
        late.SetBinding(CdsDataTableEmptyState.TitleProperty, new Binding(nameof(EmptyViewModel.Title)));
        table.EmptyState = late;
        Assert.Equal("No keys yet", late.Title);
        var own = new CdsDataTableEmptyState { BindingContext = new OwnTitle("Own") };
        own.SetBinding(CdsDataTableEmptyState.TitleProperty, new Binding("Title"));
        table.EmptyState = own;
        page.BindingContext = new EmptyViewModel();
        Assert.Equal("Own", own.Title);
    }

    // ----- the table primitives -----

    [Fact]
    public void A_hand_built_table_builds_from_head_rows_and_cells()
    {
        var table = new CdsTable { Size = CdsSize.Md, UseZebraStyles = true, StickyHeader = true, IsSortable = true, Label = "Keys" };
        table.Head = new CdsTableHead();
        table.Head.Headers.Add(new CdsTableHeader { Text = "Name" });
        table.Head.Headers.Add(new CdsTableHeader { Text = "Calls", Align = CdsTableAlignment.End, IsSortable = false });
        table.Rows.Add(new CdsTableRow("ci-deploy", "1204"));
        table.Rows.Add(new CdsTableRow("web", "88") { IsSelected = true });
        Assert.Equal(2, table.HeaderCount);
        Assert.Equal(2, table.BodyRowCount);
        table.Rows[0].IsSelected = true;
        table.Rows[0].ExpandedContent = new Label { Text = "details" };
        table.Rows[0].IsExpanded = true;
        table.Head.Headers[0].SortDirection = CdsSortDirection.Descending;
        table.StickyHeader = false;
        Assert.Equal(2, table.BodyRowCount);
    }

    [Fact]
    public void Table_parts_can_be_changed_after_the_table_is_built()
    {
        var table = new CdsTable();
        var head = new CdsTableHead();
        var header = new CdsTableHeader { Text = "Name" };
        head.Headers.Add(header);
        table.Head = head;
        var row = new CdsTableRow("a");
        table.Rows.Add(row);
        header.Text = "Key";
        header.Width = new GridLength(120);
        row.Cells[0].Text = "b";
        row.Cells.Add(new CdsTableCell { Body = new Label() });
        table.Rows.Remove(row);
        Assert.Equal(0, table.BodyRowCount);
    }

    [Fact]
    public void Batch_update_rebuilds_once()
    {
        var table = new CdsTable();
        table.Head = new CdsTableHead();
        table.Head.Headers.Add(new CdsTableHeader { Text = "Name" });
        table.BatchUpdate(() =>
        {
            for (var i = 0; i < 50; i++) table.Rows.Add(new CdsTableRow($"row {i}"));
        });
        Assert.Equal(50, table.BodyRowCount);
    }
}
