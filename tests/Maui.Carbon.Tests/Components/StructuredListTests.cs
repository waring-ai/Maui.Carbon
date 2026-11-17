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
}
