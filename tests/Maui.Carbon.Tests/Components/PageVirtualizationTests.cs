namespace Maui.Carbon.Tests.Components;

/// <summary>A data table in a page's vertical scroll view builds only the rows in (and near) the viewport.</summary>
public class PageVirtualizationTests
{
    sealed record Item(string Key, string Name);

    static List<Item> Items(int n) => Enumerable.Range(0, n).Select(i => new Item($"k{i}", $"Row {i}")).ToList();

    static CdsDataTable CreateTable(GridLength? width = null)
    {
        var table = new CdsDataTable { RowKeySelector = r => ((Item)r).Key, Selection = CdsDataTableSelection.Multi };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", Width = width ?? GridLength.Star, ValueSelector = r => ((Item)r).Name });
        return table;
    }

    static ScrollView InPage(View view) => new() { Content = new VerticalStackLayout { new CdsText { Text = "Above" }, view } };

    [Fact]
    public void In_a_scroll_view_only_rows_near_the_viewport_are_built()
    {
        var table = CreateTable();
        _ = InPage(table);
        table.ItemsSource = Items(200);

        Assert.True(table.Table.IsVirtual);
        Assert.Empty(table.Table.Rows);
        Assert.Equal(200, table.Table.BodyRowCount);
        Assert.InRange(table.Table.RealizedRowCount, 1, 60);
        Assert.Equal(200, table.VisibleRowCount);
    }

    [Fact]
    public void Scrolling_the_page_re_binds_the_pooled_rows_to_the_rows_in_view()
    {
        var table = CreateTable();
        _ = InPage(table);
        table.ItemsSource = Items(200);
        table.Table.RealizeAt(table.Table.RowTop(60));
        var pool = table.Table.RealizedRowCount; // the viewport plus overscan on both sides

        table.Table.RealizeAt(table.Table.RowTop(150));

        var realized = table.Table.RealizedRows;
        Assert.Contains(realized, r => r.Index == 150);
        Assert.DoesNotContain(realized, r => r.Index == 0);
        Assert.Equal(pool, table.Table.RealizedRowCount); // the same pool, re-bound
        Assert.Equal("Row 150", RowText(realized.First(r => r.Index == 150).Row));
    }

    static string? RowText(CdsTableRow row) =>
        row.Cells.Select(c => c.Text).FirstOrDefault(t => t is not null);

    [Fact]
    public void Selection_and_expansion_work_on_rows_out_of_view()
    {
        var table = CreateTable();
        _ = InPage(table);
        var items = Items(200);
        table.ItemsSource = items;
        table.SelectedRows = new List<object> { items[180] };
        Assert.Equal([items[180]], table.SelectedRows.Cast<object>());
        table.Table.RealizeAt(table.Table.RowTop(180));
        var row = table.Table.RealizedRows.Single(r => r.Index == 180).Row;
        Assert.True(row.IsSelected);
    }

    [Fact]
    public void Clearing_the_selection_or_new_rows_with_the_same_keys_update_the_realized_rows()
    {
        var table = CreateTable();
        _ = InPage(table);
        var items = Items(40);
        table.ItemsSource = items;
        table.SelectedRows = new List<object> { items[0], items[1] };
        Assert.Equal(2, table.Table.RealizedRows.Count(r => r.Row.IsSelected));

        // A refresh with new objects for the same rows keeps the selection by key.
        var fresh = Items(40);
        table.ItemsSource = fresh;
        Assert.Equal(["k0", "k1"], table.Table.RealizedRows.Where(r => r.Row.IsSelected).Select(r => ((Item)r.Row.Tag!).Key));

        table.SelectedRows = new List<object>();
        Assert.DoesNotContain(table.Table.RealizedRows, r => r.Row.IsSelected);
    }

    [Theory]
    [InlineData(-2000, 900, 0, 1)]   // the body starts below the fold: nothing in view
    [InlineData(-300, 900, 0, 600)]  // the body starts 300 down the viewport
    [InlineData(500, 900, 500, 900)] // scrolled into the body
    public void The_visible_span_is_the_part_of_the_body_in_the_viewport(double top, double viewport, double from, double height)
    {
        Assert.Equal((from, height), CdsTable.VisibleSpan(top, viewport));
    }

    [Fact]
    public void Turning_it_off_or_an_auto_column_builds_every_row()
    {
        var off = CreateTable();
        off.VirtualizeInScrollView = false;
        _ = InPage(off);
        off.ItemsSource = Items(50);
        Assert.False(off.Table.IsVirtual);
        Assert.Equal(50, off.Table.Rows.Count);

        var auto = CreateTable(GridLength.Auto);
        _ = InPage(auto);
        auto.ItemsSource = Items(50);
        Assert.False(auto.Table.IsVirtual);
        Assert.Equal(50, auto.Table.Rows.Count);
    }

    [Fact]
    public void Outside_a_scroll_view_every_row_is_built_and_moving_into_one_virtualizes()
    {
        var table = CreateTable();
        var stack = new VerticalStackLayout { table };
        table.ItemsSource = Items(100);
        Assert.False(table.Table.IsVirtual);
        Assert.Equal(100, table.Table.Rows.Count);

        _ = new ScrollView { Content = stack };
        table.ItemsSource = Items(100); // the next refresh notices the new ancestor

        Assert.True(table.Table.IsVirtual);
        Assert.Empty(table.Table.Rows);
        Assert.InRange(table.Table.RealizedRowCount, 1, 60);
    }
}
