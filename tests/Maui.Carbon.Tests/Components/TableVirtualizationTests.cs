namespace Maui.Carbon.Tests.Components;

public class TableVirtualizationTests
{
    sealed class Cluster(string name, string region)
    {
        public string Name { get; } = name;
        public string Region { get; } = region;
    }

    static List<Cluster> Clusters(int count) =>
        Enumerable.Range(0, count).Select(i => new Cluster($"cluster-{i:D4}", i % 2 == 0 ? "us-south" : "eu-de")).ToList();

    static CdsDataTable Table(int count, bool sticky, CdsDataTableSelection selection = CdsDataTableSelection.None)
    {
        var table = new CdsDataTable { StickyHeader = sticky, Selection = selection, IsSortable = true };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", ValueSelector = o => ((Cluster)o).Name });
        table.Headers.Add(new CdsDataTableHeader { Key = "region", Header = "Region", ValueSelector = o => ((Cluster)o).Region });
        table.ItemsSource = Clusters(count);
        return table;
    }

    /// <summary>The name cell's text of every realized row, by data index.</summary>
    static Dictionary<int, string?> Names(CdsDataTable table, int nameColumn = 0) =>
        table.Table.RealizedRows.ToDictionary(r => r.Index, r => r.Row.Cells[nameColumn].Text);

    // ----- window math -----

    [Fact]
    public void Index_at_finds_the_row_under_a_position()
    {
        double[] offsets = [0, 48, 96, 240, 288];
        Assert.Equal(0, CdsTable.IndexAt(offsets, 4, 0));
        Assert.Equal(0, CdsTable.IndexAt(offsets, 4, 47.9));
        Assert.Equal(1, CdsTable.IndexAt(offsets, 4, 48));
        Assert.Equal(2, CdsTable.IndexAt(offsets, 4, 200));
        Assert.Equal(3, CdsTable.IndexAt(offsets, 4, 5000));
        Assert.Equal(-1, CdsTable.IndexAt([0], 0, 10));
    }

    [Fact]
    public void The_window_is_the_rows_in_view_plus_overscan_clamped_to_the_data()
    {
        var offsets = Enumerable.Range(0, 101).Select(i => i * 48d).ToArray();
        Assert.Equal((0, 9), CdsTable.RowWindow(offsets, 100, 0, 150, 6));
        Assert.Equal((44, 59), CdsTable.RowWindow(offsets, 100, 50 * 48, 150, 6));
        Assert.Equal((93, 99), CdsTable.RowWindow(offsets, 100, 99 * 48, 150, 6));
        Assert.Equal((0, -1), CdsTable.RowWindow([0], 0, 0, 150, 6));
    }

    // ----- virtualized (sticky header) -----

    [Fact]
    public void A_sticky_data_table_realizes_only_the_rows_in_view()
    {
        var table = Table(1000, sticky: true);
        Assert.True(table.Table.IsVirtual);
        Assert.Empty(table.Table.Rows);
        Assert.Equal(1000, table.Table.BodyRowCount);
        // 300 of body at 48 a row is 7 rows, plus 6 below.
        Assert.InRange(table.Table.RealizedRowCount, 7, 20);
        var names = Names(table);
        Assert.Equal("cluster-0000", names[0]);
        Assert.Equal("cluster-0005", names[5]);
    }

    [Fact]
    public void Scrolling_re_binds_the_same_rows_to_the_rows_that_come_into_view()
    {
        var table = Table(1000, sticky: true);
        var pool = table.Table.RealizedRows.Select(r => r.Row).ToHashSet();

        table.Table.RealizeAt(table.Table.RowTop(500));

        var realized = table.Table.RealizedRows;
        Assert.Contains(realized, r => r.Index == 500);
        Assert.DoesNotContain(realized, r => r.Index == 0);
        Assert.All(realized, r => Assert.Equal($"cluster-{r.Index:D4}", r.Row.Cells[0].Text));
        // Every first row is reused; the pool only grows by the overscan above (6 rows above, 7 in view, 6 below).
        Assert.Subset(realized.Select(r => r.Row).ToHashSet(), pool);
        Assert.InRange(realized.Count, pool.Count, 19);
    }

    [Fact]
    public void Sorting_and_searching_re_bind_the_realized_rows()
    {
        var table = Table(1000, sticky: true);
        table.SortKey = "name";
        table.SortDirection = CdsSortDirection.Descending;
        Assert.Equal("cluster-0999", Names(table)[0]);

        table.SearchText = "cluster-002";
        Assert.Equal(10, table.Table.BodyRowCount);
        Assert.Equal(10, table.Table.RealizedRowCount);
        Assert.Equal("cluster-0029", Names(table)[0]);

        table.SearchText = "nothing matches";
        Assert.Equal(0, table.Table.RealizedRowCount);
    }

    [Fact]
    public void Selection_reaches_realized_rows_and_rows_bound_later()
    {
        var table = Table(1000, sticky: true, CdsDataTableSelection.Multi);
        var items = (List<Cluster>)table.ItemsSource!;
        table.SelectedRows = new List<object> { items[1], items[600] };

        Assert.True(table.Table.RealizedRows.Single(r => r.Index == 1).Row.IsSelected);
        Assert.False(table.Table.RealizedRows.Single(r => r.Index == 0).Row.IsSelected);

        table.Table.RealizeAt(table.Table.RowTop(598));
        var row = table.Table.RealizedRows.Single(r => r.Index == 600).Row;
        Assert.True(row.IsSelected);
        Assert.Equal(2, table.SelectedCount);
        Assert.Contains(table.Table.RealizedRows, r => r.Index != 600 && !r.Row.IsSelected);
    }

    [Fact]
    public void A_row_checkbox_selects_the_data_row_it_shows_now()
    {
        var table = Table(1000, sticky: true, CdsDataTableSelection.Multi);
        var items = (List<Cluster>)table.ItemsSource!;
        table.Table.RealizeAt(table.Table.RowTop(300));
        var row = table.Table.RealizedRows.Single(r => r.Index == 303).Row;
        var check = (CdsCheckbox)row.Cells[0].Body!;

        check.GetVisualTreeDescendants().OfType<CdsHitTarget>().First().RaiseKeyboardActivation();

        Assert.Equal([items[303]], table.SelectedRows.Cast<Cluster>());
    }

    [Fact]
    public void Expanded_rows_push_the_rows_below_them_down_and_bind_their_content()
    {
        var table = Table(100, sticky: true);
        table.IsExpandable = true;
        table.ExpandedRowTemplate = new DataTemplate(() => new Label());
        var items = (List<Cluster>)table.ItemsSource!;
        var rowHeight = CdsTableTokens.RowHeight(CdsSize.Lg);
        Assert.Equal(rowHeight * 3, table.Table.RowTop(3));

        table.ExpandedRows = new List<object> { items[1] };

        Assert.True(table.Table.RowTop(3) > rowHeight * 3);
        var row = table.Table.RealizedRows.Single(r => r.Index == 1).Row;
        Assert.True(row.IsExpanded);
        Assert.Same(items[1], row.ExpandedContent!.BindingContext);
    }

    [Fact]
    public void A_recycled_expanded_row_shows_the_new_rows_data_collapsed()
    {
        var table = Table(1000, sticky: true);
        table.IsExpandable = true;
        table.ExpandedRowTemplate = new DataTemplate(() => new Label());
        var items = (List<Cluster>)table.ItemsSource!;
        table.ExpandedRows = new List<object> { items[0] };
        var row = table.Table.RealizedRows.Single(r => r.Index == 0).Row;

        table.Table.RealizeAt(table.Table.RowTop(500));

        var now = table.Table.RealizedRows.Single(r => ReferenceEquals(r.Row, row));
        Assert.False(row.IsExpanded);
        Assert.Same(items[now.Index], row.ExpandedContent!.BindingContext);
    }

    [Fact]
    public void A_row_whose_menu_is_open_or_focused_is_kept_while_it_scrolls_away()
    {
        var table = Table(1000, sticky: true);
        table.RowActions.Add(new CdsDataTableRowAction { Label = "Delete" });
        var row = table.Table.RealizedRows.Single(r => r.Index == 0).Row;
        row.Cells[^1].KeepVisible = true;

        table.Table.RealizeAt(table.Table.RowTop(500));

        Assert.Contains(table.Table.RealizedRows, r => r.Index == 0 && ReferenceEquals(r.Row, row));
        Assert.Contains(table.Table.RealizedRows, r => r.Index == 500);
    }

    [Fact]
    public void Row_menus_act_on_the_data_row_shown_now()
    {
        var table = Table(1000, sticky: true);
        var items = (List<Cluster>)table.ItemsSource!;
        object? deleted = null;
        var action = new CdsDataTableRowAction { Label = "Delete" };
        action.Invoked += (_, e) => deleted = e.Row;
        table.RowActions.Add(action);
        table.Table.RealizeAt(table.Table.RowTop(400));
        var row = table.Table.RealizedRows.Single(r => r.Index == 402).Row;
        var menu = (CdsOverflowMenu)row.Cells[^1].Body!;

        ((CdsMenuItem)menu.Items[0]).Invoke();

        Assert.Same(items[402], deleted);
    }

    [Fact]
    public void Cell_templates_are_re_bound_to_the_item_they_show()
    {
        var table = Table(1000, sticky: true);
        table.Headers[0].CellTemplate = new DataTemplate(() => new Label());
        var items = (List<Cluster>)table.ItemsSource!;
        table.Table.RealizeAt(table.Table.RowTop(700));
        Assert.All(table.Table.RealizedRows, r => Assert.Same(items[r.Index], r.Row.Cells[0].Body!.BindingContext));
    }

    [Fact]
    public void Turning_the_sticky_header_off_renders_every_row()
    {
        var table = Table(50, sticky: true);
        table.StickyHeader = false;
        Assert.False(table.Table.IsVirtual);
        Assert.Null(table.Table.RowSource);
        Assert.Equal(50, table.Table.Rows.Count);
        Assert.Equal(50, table.Table.RealizedRowCount);
    }

    // ----- every row rendered -----

    [Fact]
    public void Sorting_re_binds_rows_instead_of_rebuilding_them()
    {
        var table = Table(20, sticky: false, CdsDataTableSelection.Multi);
        var rows = table.Table.Rows.ToList();
        var checks = rows.Select(r => r.Cells[0].Body).ToList();

        table.SortKey = "name";
        table.SortDirection = CdsSortDirection.Descending;

        Assert.Equal(rows, table.Table.Rows);
        Assert.Equal(checks, table.Table.Rows.Select(r => r.Cells[0].Body));
        Assert.Equal("cluster-0019", table.Table.Rows[0].Cells[1].Text);
    }

    [Fact]
    public void A_search_that_narrows_then_widens_brings_back_the_same_rows()
    {
        var table = Table(30, sticky: false);
        var rows = table.Table.Rows.ToList();

        table.SearchText = "cluster-001";
        Assert.Equal(10, table.Table.BodyRowCount);
        Assert.Equal("cluster-0019", table.Table.Rows[9].Cells[0].Text);

        table.SearchText = string.Empty;
        Assert.Equal(rows, table.Table.Rows);
        Assert.Equal(30, table.Table.BodyRowCount);
        Assert.Equal("cluster-0029", table.Table.Rows[29].Cells[0].Text);
    }

    [Fact]
    public void A_cell_text_change_updates_its_label_in_place()
    {
        var table = new CdsTable { Head = new CdsTableHead() };
        table.Head.Headers.Add(new CdsTableHeader { Text = "Name" });
        var row = new CdsTableRow("before");
        table.Rows.Add(row);
        var label = table.GetVisualTreeDescendants().OfType<CdsText>().Single(t => t.Text == "before");

        row.Cells[0].Text = "after";

        Assert.Equal("after", label.Text);
        Assert.Contains(label, table.GetVisualTreeDescendants());
    }

    [Fact]
    public void Cleared_rows_no_longer_reach_the_table()
    {
        var table = new CdsTable { Head = new CdsTableHead() };
        table.Head.Headers.Add(new CdsTableHeader { Text = "Name" });
        var old = new CdsTableRow("old");
        table.Rows.Add(old);
        table.Rows.Clear();
        table.Rows.Add(new CdsTableRow("new"));
        var label = table.GetVisualTreeDescendants().OfType<CdsText>().Single(t => t.Text == "new");

        // A structural change to a row the table no longer has must not rebuild it.
        old.Cells.Add(new CdsTableCell("extra"));

        Assert.Contains(label, table.GetVisualTreeDescendants());
    }

    [Fact]
    public void Rows_added_at_the_end_keep_the_existing_rows_views()
    {
        var table = new CdsTable { Head = new CdsTableHead() };
        table.Head.Headers.Add(new CdsTableHeader { Text = "Name" });
        table.Rows.Add(new CdsTableRow("first"));
        var label = table.GetVisualTreeDescendants().OfType<CdsText>().Single(t => t.Text == "first");

        table.Rows.Add(new CdsTableRow("second"));
        table.Rows.RemoveAt(1);
        table.Rows.Add(new CdsTableRow("third"));

        Assert.Contains(label, table.GetVisualTreeDescendants());
        Assert.Equal(2, table.BodyRowCount);
        Assert.Contains(table.GetVisualTreeDescendants().OfType<CdsText>(), t => t.Text == "third" && t.IsVisible);
    }
}
