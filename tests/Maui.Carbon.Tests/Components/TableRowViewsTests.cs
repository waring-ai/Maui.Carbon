using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

/// <summary>A table builds as few views per row as it can: the first visit to a screen with a big table builds them all.</summary>
public class TableRowViewsTests
{
    sealed record Item(string Key, string Name);

    static CdsDataTable CreateTable(int rows = 3)
    {
        var table = new CdsDataTable
        {
            RowKeySelector = r => ((Item)r).Key,
            Selection = CdsDataTableSelection.Multi,
            IsExpandable = true,
            ExpandedRowTemplate = new DataTemplate(() => new CdsText { Text = "details" }),
            ItemsSource = Enumerable.Range(0, rows).Select(i => new Item($"k{i}", $"Row {i}")).ToList(),
        };
        table.RowActions.Add(new CdsDataTableRowAction { Label = "Open" });
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", ValueSelector = r => ((Item)r).Name });
        return table;
    }

    [Fact]
    public void Tool_controls_sit_in_the_row_without_a_cell_host()
    {
        var table = CreateTable();
        var row = table.Table.Rows[0];
        var check = (CdsCheckbox)row.Cells[1].Body!;
        var menu = (CdsOverflowMenu)row.Cells[^1].Body!;
        var name = ContainerSemanticsAuditTests.Walk(table.Table).OfType<CdsText>().First(t => t.Text == "Row 0");
        // The text cell keeps its host (padding, the row's height); the checkbox and the menu are children of the body grid.
        var body = (Grid)name.Parent.Parent;
        Assert.Same(body, check.Parent);
        Assert.Same(body, menu.Parent);
    }

    [Fact]
    public void A_child_row_is_built_on_first_expansion_right_after_its_row()
    {
        var table = CreateTable();
        var row = table.Table.Rows[1];
        var check = (CdsCheckbox)row.Cells[1].Body!;
        var body = (Grid)check.Parent;
        Assert.Null(row.ExpandedContent);
        var before = body.Children.Count;

        table.ExpandedRows = new List<object> { row.Tag! };

        var content = Assert.IsType<CdsText>(row.ExpandedContent);
        var host = Assert.IsAssignableFrom<Grid>(content.Parent);
        Assert.True(host.IsVisible);
        Assert.Equal(before + 3, body.Children.Count); // fill, rule, content host
        // In grid order (Tab and screen reader order) the child row comes after row 1's views and before row 2's.
        var nextRowCheck = (View)table.Table.Rows[2].Cells[1].Body!;
        var at = body.Children.IndexOf(host);
        Assert.True(at > body.Children.IndexOf(check) && at < body.Children.IndexOf(nextRowCheck));
    }

    [Theory]
    [InlineData(-5, -1)]  // the header row
    [InlineData(0, 0)]
    [InlineData(47.9, 0)]
    [InlineData(48, 1)]   // row 1 and its expanded child row
    [InlineData(150, 1)]
    [InlineData(160, 2)]
    [InlineData(500, -1)] // under the last row
    public void Hover_finds_the_row_under_the_pointer(double y, int expected)
    {
        (int, double, double)[] spans = [(0, 0, 48), (1, 48, 160), (2, 160, 208)];
        Assert.Equal(expected, CdsTable.RowAt(spans, y));
    }

    [Fact]
    public void A_focus_ring_draws_its_outline_when_first_shown()
    {
        var ring = new CdsFocusRing(CdsFocusRingStyle.Button);
        ring.SetColorKey(CdsColorKeys.SupportError);
        Assert.False(ring.IsDrawn);
        Assert.Empty(ring.Children);

        ring.IsActive = true;

        Assert.True(ring.IsDrawn);
        Assert.Equal(2, ring.Children.Count); // outer ring and the inner background ring
        ring.IsActive = false;
        ring.IsVisible = true; // controls that set IsVisible directly
        Assert.Equal(2, ring.Children.Count);
    }

    [Fact]
    public void A_hidden_label_checkbox_and_an_icon_only_menu_build_no_text_until_needed()
    {
        var check = new CdsCheckbox { HideLabel = true, Label = "Select row" };
        Assert.DoesNotContain(ContainerSemanticsAuditTests.Walk(check), e => e is CdsText or CdsFormMessage);
        check.HelperText = "Pick one";
        Assert.Contains(ContainerSemanticsAuditTests.Walk(check), e => e is CdsText { Text: "Pick one" });

        var menu = new CdsOverflowMenu();
        Assert.DoesNotContain(ContainerSemanticsAuditTests.Walk(menu), e => e is CdsText or CdsButtonOutline);
    }
}
