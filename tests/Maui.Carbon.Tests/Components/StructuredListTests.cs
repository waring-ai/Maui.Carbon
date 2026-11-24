using Microsoft.Maui;

namespace Maui.Carbon.Tests.Components;

public class StructuredListTests
{
    static readonly bool[] AllEnabled = [true, true, true];

    [Fact]
    public void Move_steps_and_wraps()
    {
        Assert.Equal(1, CdsStructuredListModel.Move(0, 1, AllEnabled));
        Assert.Equal(0, CdsStructuredListModel.Move(2, 1, AllEnabled));
        Assert.Equal(2, CdsStructuredListModel.Move(0, -1, AllEnabled));
        Assert.Equal(1, CdsStructuredListModel.Move(2, -1, AllEnabled));
    }

    [Fact]
    public void Move_skips_disabled_rows()
    {
        bool[] enabled = [true, false, true, false];
        Assert.Equal(2, CdsStructuredListModel.Move(0, 1, enabled));
        Assert.Equal(0, CdsStructuredListModel.Move(2, 1, enabled));
        Assert.Equal(2, CdsStructuredListModel.Move(0, -1, enabled));
    }

    [Fact]
    public void Move_from_nothing_goes_to_the_first_or_last_enabled_row()
    {
        bool[] enabled = [false, true, true, false];
        Assert.Equal(1, CdsStructuredListModel.Move(-1, 1, enabled));
        Assert.Equal(2, CdsStructuredListModel.Move(-1, -1, enabled));
    }

    [Fact]
    public void Move_returns_current_when_nothing_can_be_selected()
    {
        Assert.Equal(-1, CdsStructuredListModel.Move(-1, 1, [false, false]));
        Assert.Equal(1, CdsStructuredListModel.Move(1, 1, [false, true]));
        Assert.Equal(5, CdsStructuredListModel.Move(5, 1, []));
        Assert.Equal(0, CdsStructuredListModel.Move(0, 0, AllEnabled));
    }

    [Fact]
    public void Default_cell_padding_is_16_over_24()
    {
        Assert.Equal(new Thickness(16, 16, 8, 24), CdsStructuredListModel.CellPadding(false, false, false, false, true));
        Assert.Equal(new Thickness(8, 16, 8, 24), CdsStructuredListModel.CellPadding(false, false, false, false, false));
    }

    [Fact]
    public void Header_cells_have_8_below()
    {
        Assert.Equal(new Thickness(16, 16, 8, 8), CdsStructuredListModel.CellPadding(true, false, false, false, true));
    }

    [Fact]
    public void Condensed_cells_have_8_vertically()
    {
        Assert.Equal(new Thickness(8, 8, 8, 8), CdsStructuredListModel.CellPadding(false, true, false, false, false));
        Assert.Equal(new Thickness(16, 8, 8, 8), CdsStructuredListModel.CellPadding(false, true, false, false, true));
    }

    [Fact]
    public void Condensed_wins_over_the_selection_side_padding()
    {
        // .cds--structured-list--condensed (padding: 0.5rem) outranks .cds--structured-list--selection (padding-inline: 1rem).
        Assert.Equal(new Thickness(8, 8, 8, 8), CdsStructuredListModel.CellPadding(false, true, false, true, true));
        Assert.Equal(new Thickness(8, 8, 8, 8), CdsStructuredListModel.CellPadding(true, true, false, true, false));
        Assert.Equal(new Thickness(16, 16, 16, 24), CdsStructuredListModel.CellPadding(false, false, false, true, true));
        // Flush still wins over condensed.
        Assert.Equal(new Thickness(0, 8, 16, 8), CdsStructuredListModel.CellPadding(false, true, true, false, true));
    }

    static CdsStructuredList SelectableList(params bool[] enabled)
    {
        var list = new CdsStructuredList { IsSelectable = true, Label = "Flavors" };
        list.Columns.Add(new CdsStructuredListColumn { Header = "Flavor" });
        for (var i = 0; i < enabled.Length; i++)
            list.Rows.Add(new CdsStructuredListRow($"flavor-{i}") { Label = $"flavor {i}", IsEnabled = enabled[i] });
        return list;
    }

    static int[] Stops(CdsStructuredList list) =>
        list.RowTargets.Select((h, i) => (h, i)).Where(p => p.h.IsTabStop).Select(p => p.i).ToArray();

    [Fact]
    public void Selectable_rows_are_one_tab_stop_on_the_selected_row()
    {
        var list = SelectableList(true, true, true);
        Assert.Equal(3, list.RowTargets.Count);
        Assert.Equal([0], Stops(list));     // nothing selected: the first row
        list.SelectedIndex = 2;
        Assert.Equal([2], Stops(list));
    }

    [Fact]
    public void Disabled_rows_never_take_the_tab_stop()
    {
        var list = SelectableList(false, true, true);
        Assert.Equal([1], Stops(list));
    }

    [Fact]
    public void Arrow_keys_move_the_selection_and_the_tab_stop_and_skip_disabled_rows()
    {
        var list = SelectableList(true, false, true);
        list.SelectedIndex = 0;
        list.RowTargets[0].ReportFocus(true, fromKeyboard: true);
        Assert.True(list.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
        Assert.Equal(2, list.SelectedIndex);
        Assert.Equal([2], Stops(list));
        // Headlessly Focus() does nothing: move the platform focus by hand.
        list.RowTargets[0].ReportFocus(false, fromKeyboard: true);
        list.RowTargets[2].ReportFocus(true, fromKeyboard: true);
        Assert.True(list.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down))); // wraps like a radio group
        Assert.Equal(0, list.SelectedIndex);
        Assert.False(list.OnKeyDown(new CdsKeyEventArgs(CdsKey.Enter)));
    }

    [Fact]
    public void Header_cells_wrap_unless_the_column_does_not_wrap()
    {
        var list = new CdsStructuredList();
        list.Columns.Add(new CdsStructuredListColumn { Header = "Best for" });
        list.Columns.Add(new CdsStructuredListColumn { Header = "vCPU / memory", IsNoWrap = true });
        list.Rows.Add(new CdsStructuredListRow("General web apps", "4 vCPU"));
        var headers = Maui.Carbon.Tests.Foundation.ContainerSemanticsAuditTests.Walk(list).OfType<CdsText>()
            .Where(t => t.TypeStyle == CdsTypeStyle.HeadingCompact01).ToDictionary(t => t.Text);
        Assert.Equal(LineBreakMode.WordWrap, headers["Best for"].LineBreakMode);
        Assert.Equal(LineBreakMode.NoWrap, headers["vCPU / memory"].LineBreakMode);
    }

    [Fact]
    public void Flush_removes_the_start_padding_and_selection_pads_both_sides()
    {
        Assert.Equal(new Thickness(0, 16, 16, 24), CdsStructuredListModel.CellPadding(false, false, true, false, true));
        Assert.Equal(new Thickness(16, 16, 16, 24), CdsStructuredListModel.CellPadding(false, false, false, true, false));
    }

    [Fact]
    public void Radio_cell_padding()
    {
        Assert.Equal(new Thickness(16, 16, 0, 24), CdsStructuredListModel.IconCellPadding(false, false));
        Assert.Equal(new Thickness(16, 10, 0, 10), CdsStructuredListModel.IconCellPadding(false, true));
        Assert.Equal(new Thickness(16, 16, 0, 8), CdsStructuredListModel.IconCellPadding(true, false));
    }

    [Fact]
    public void Row_fill_is_selected_then_hover_and_never_for_disabled()
    {
        Assert.Equal(CdsColorKeys.LayerSelected01, CdsStructuredListTokens.RowFill(true, true, false));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsStructuredListTokens.RowFill(false, true, false));
        Assert.Null(CdsStructuredListTokens.RowFill(false, false, false));
        Assert.Null(CdsStructuredListTokens.RowFill(true, true, true));
    }

    [Fact]
    public void Rules_take_the_color_of_a_filled_neighbour()
    {
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsStructuredListTokens.Rule(false, false, false, false));
        Assert.Equal(CdsColorKeys.LayerSelected01, CdsStructuredListTokens.Rule(true, false, false, false));
        Assert.Equal(CdsColorKeys.LayerSelected01, CdsStructuredListTokens.Rule(false, true, true, false));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsStructuredListTokens.Rule(false, true, false, false));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsStructuredListTokens.Rule(false, false, false, true));
    }

    [Fact]
    public void Cell_text_and_icon_tokens()
    {
        Assert.Equal(CdsColorKeys.TextPrimary, CdsStructuredListTokens.CellText(true, false, false));
        Assert.Equal(CdsColorKeys.TextSecondary, CdsStructuredListTokens.CellText(false, false, false));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsStructuredListTokens.CellText(false, true, false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsStructuredListTokens.CellText(false, true, true));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsStructuredListTokens.Icon(true));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsStructuredListTokens.Icon(false));
        Assert.Equal(CdsColorKeys.Layer01, CdsStructuredListTokens.Background);
    }

    static CdsStructuredList Sample(bool selectable)
    {
        var list = new CdsStructuredList { IsSelectable = selectable, Label = "Worker node flavors" };
        list.Columns.Add(new CdsStructuredListColumn { Header = "Flavor", Width = GridLength.Auto });
        list.Columns.Add(new CdsStructuredListColumn { Header = "vCPU / memory", IsNoWrap = true });
        list.Rows.Add(new CdsStructuredListRow("bx2.4x16", "4 vCPU / 16 GB") { Id = "a" });
        list.Rows.Add(new CdsStructuredListRow("cx2.8x16", "8 vCPU / 16 GB") { Id = "b" });
        list.Rows.Add(new CdsStructuredListRow("mx2.8x64", "8 vCPU / 64 GB") { Id = "c", IsEnabled = false });
        return list;
    }

    [Fact]
    public void Selection_keeps_index_and_item_in_sync()
    {
        var list = Sample(true);
        Assert.Equal(-1, list.SelectedIndex);
        list.SelectedIndex = 1;
        Assert.Same(list.Rows[1], list.SelectedItem);
        list.SelectedItem = list.Rows[0];
        Assert.Equal(0, list.SelectedIndex);
        list.SelectedItem = null;
        Assert.Equal(-1, list.SelectedIndex);
        list.SelectedIndex = -5;
        Assert.Equal(-1, list.SelectedIndex);
    }

    [Fact]
    public void Selection_changed_is_raised_once_per_change()
    {
        var list = Sample(true);
        var count = 0;
        list.SelectionChanged += (_, _) => count++;
        list.SelectedIndex = 0;
        list.SelectedIndex = 0;
        list.SelectedIndex = 1;
        Assert.Equal(2, count);
    }

    [Fact]
    public void Removing_the_selected_row_clears_the_selection()
    {
        var list = Sample(true);
        list.SelectedIndex = 2;
        list.Rows.RemoveAt(2);
        Assert.Equal(-1, list.SelectedIndex);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void List_can_be_rebuilt_with_every_option(bool selectable)
    {
        var list = Sample(selectable);
        list.IsCondensed = true;
        list.IsFlush = true;
        list.IsNoWrap = true;
        list.Rows[0].Cells.Add(new Label { Text = "extra" });
        list.Rows[0].Label = "bx2.4x16 flavor";
        list.Columns.Clear();
        Assert.Equal(3, list.Rows.Count);
    }

    [Fact]
    public void A_custom_cell_leaves_its_old_wrapper_when_the_list_is_rebuilt()
    {
        var cell = new Label { Text = "Open" };
        var row = new CdsStructuredListRow("PRJ-1");
        row.Cells.Add(cell);
        var list = new CdsStructuredList();
        list.Rows.Add(row);
        var first = Assert.IsType<ContentView>(cell.Parent);
        list.IsCondensed = true;
        Assert.Null(first.Content);
        var second = Assert.IsType<ContentView>(cell.Parent);
        Assert.NotSame(first, second);
        Assert.Same(cell, second.Content);
    }
}
