namespace Maui.Carbon.Tests.Components;

public class GridModelTests
{
    static GridResponsive R(int? b = null, int? sm = null, int? md = null, int? lg = null, int? xlg = null, int? max = null) =>
        new(b, sm, md, lg, xlg, max);

    [Fact]
    public void Unset_span_is_the_full_row_at_every_breakpoint()
    {
        Assert.Equal(4, GridModel.ResolveSpan(R(), CdsBreakpoint.Sm));
        Assert.Equal(8, GridModel.ResolveSpan(R(), CdsBreakpoint.Md));
        Assert.Equal(16, GridModel.ResolveSpan(R(), CdsBreakpoint.Max));
    }

    [Fact]
    public void Values_cascade_upward()
    {
        var span = R(sm: 4, md: 8, lg: 4);
        Assert.Equal(4, GridModel.ResolveSpan(span, CdsBreakpoint.Sm));
        Assert.Equal(8, GridModel.ResolveSpan(span, CdsBreakpoint.Md));
        Assert.Equal(4, GridModel.ResolveSpan(span, CdsBreakpoint.Lg));
        Assert.Equal(4, GridModel.ResolveSpan(span, CdsBreakpoint.Xlg));
        Assert.Equal(4, GridModel.ResolveSpan(span, CdsBreakpoint.Max));
    }

    [Fact]
    public void Base_value_applies_below_the_first_override()
    {
        var span = R(b: 2, lg: 6);
        Assert.Equal(2, GridModel.ResolveSpan(span, CdsBreakpoint.Sm));
        Assert.Equal(2, GridModel.ResolveSpan(span, CdsBreakpoint.Md));
        Assert.Equal(6, GridModel.ResolveSpan(span, CdsBreakpoint.Lg));
    }

    [Fact]
    public void Span_is_clamped_to_the_column_count_and_zero_hides()
    {
        Assert.Equal(4, GridModel.ResolveSpan(R(b: 12), CdsBreakpoint.Sm));
        Assert.Equal(0, GridModel.ResolveSpan(R(sm: 4, md: 0), CdsBreakpoint.Md));
        Assert.Equal(0, GridModel.ResolveSpan(R(b: -3), CdsBreakpoint.Lg));
    }

    [Fact]
    public void Offset_defaults_to_zero_and_is_clamped()
    {
        Assert.Null(GridModel.ResolveOffset(R(), CdsBreakpoint.Lg));
        Assert.Equal(0, GridModel.ResolveOffset(R(b: 0), CdsBreakpoint.Lg));
        Assert.Equal(3, GridModel.ResolveOffset(R(b: 9), CdsBreakpoint.Sm));
        Assert.Equal(2, GridModel.ResolveOffset(R(sm: 1, md: 2), CdsBreakpoint.Md));
    }

    [Fact]
    public void Children_fill_a_row_then_wrap()
    {
        var cells = GridModel.Place(16, [(4, null), (4, null), (8, null), (4, null)]);
        Assert.Equal([(0, 0, 4), (0, 4, 4), (0, 8, 8), (1, 0, 4)], cells.Select(c => (c.Row, c.Column, c.Span)));
    }

    [Fact]
    public void A_child_that_does_not_fit_starts_a_new_row()
    {
        var cells = GridModel.Place(8, [(5, null), (4, null)]);
        Assert.Equal((0, 0), (cells[0].Row, cells[0].Column));
        Assert.Equal((1, 0), (cells[1].Row, cells[1].Column));
    }

    [Fact]
    public void Offset_moves_a_child_right_inside_its_row()
    {
        var cells = GridModel.Place(16, [(8, 4)]);
        Assert.Equal((0, 4, 8), (cells[0].Row, cells[0].Column, cells[0].Span));
    }

    [Fact]
    public void Offset_counts_toward_wrapping()
    {
        var cells = GridModel.Place(16, [(8, 4), (8, null)]);
        Assert.Equal((1, 0), (cells[1].Row, cells[1].Column));
        cells = GridModel.Place(16, [(4, null), (4, 12)]);
        Assert.Equal((0, 12), (cells[1].Row, cells[1].Column));
        cells = GridModel.Place(16, [(4, 2), (4, 12)]);
        Assert.Equal([(0, 2), (0, 12)], cells.Select(c => (c.Row, c.Column)));
    }

    [Fact]
    public void An_offset_behind_the_used_columns_moves_to_a_new_row()
    {
        var cells = GridModel.Place(16, [(8, 4), (4, 2)]);
        Assert.Equal((1, 2), (cells[1].Row, cells[1].Column));
        cells = GridModel.Place(16, [(4, null), (4, 0)]);
        Assert.Equal((1, 0), (cells[1].Row, cells[1].Column));
    }

    [Fact]
    public void Offset_that_overflows_an_empty_row_is_pulled_back_in()
    {
        var cells = GridModel.Place(16, [(8, 12)]);
        Assert.Equal(8, cells[0].Column);
    }

    [Fact]
    public void Hidden_children_take_no_space()
    {
        var cells = GridModel.Place(16, [(8, null), (0, null), (8, null)]);
        Assert.True(cells[1].Hidden);
        Assert.Equal((0, 8), (cells[2].Row, cells[2].Column));
    }

    [Fact]
    public void A_full_row_child_always_starts_a_row()
    {
        var cells = GridModel.Place(4, [(1, null), (4, null), (1, null)]);
        Assert.Equal([0, 1, 2], cells.Select(c => c.Row));
    }

    [Fact]
    public void Empty_list_places_nothing() => Assert.Empty(GridModel.Place(16, []));

    [Fact]
    public void Wide_lg_grid_geometry()
    {
        var m = GridModel.Metrics(1056, CdsBreakpoint.Lg, CdsGridMode.Wide, false, CdsGridAlign.Center);
        Assert.Equal(16, m.Columns);
        Assert.Equal(16, m.Margin);
        Assert.Equal(64, m.Pitch);
        Assert.Equal(32, m.BoxX(0));
        Assert.Equal(224, m.BoxWidth(4));
        Assert.Equal(1056 - 32, m.InnerWidth);
    }

    [Fact]
    public void Sm_grid_has_no_margin_and_text_sits_16_in()
    {
        var m = GridModel.Metrics(400, CdsBreakpoint.Sm, CdsGridMode.Wide, false, CdsGridAlign.Center);
        Assert.Equal(0, m.Margin);
        Assert.Equal(100, m.Pitch);
        Assert.Equal(16, m.BoxX(0));
        Assert.Equal(168, m.BoxWidth(2));
    }

    [Fact]
    public void Narrow_mode_hangs_into_the_gutter()
    {
        var wide = GridModel.Metrics(1056, CdsBreakpoint.Lg, CdsGridMode.Wide, false, CdsGridAlign.Center);
        var narrow = GridModel.Metrics(1056, CdsBreakpoint.Lg, CdsGridMode.Narrow, false, CdsGridAlign.Center);
        Assert.Equal(wide.BoxX(0) - 16, narrow.BoxX(0));
        Assert.Equal(wide.BoxWidth(4) + 16, narrow.BoxWidth(4));
    }

    [Fact]
    public void Condensed_mode_has_a_one_pixel_gutter()
    {
        var m = GridModel.Metrics(1056, CdsBreakpoint.Lg, CdsGridMode.Condensed, false, CdsGridAlign.Center);
        Assert.Equal(16.5, m.BoxX(0));
        Assert.Equal(4 * 64 - 1, m.BoxWidth(4));
        Assert.Equal(1, m.RowGap);
    }

    [Fact]
    public void Grid_is_centered_and_capped_at_1584_unless_full_width()
    {
        var centered = GridModel.Metrics(2000, CdsBreakpoint.Max, CdsGridMode.Wide, false, CdsGridAlign.Center);
        Assert.Equal(208, centered.Left);
        Assert.Equal(24, centered.Margin);
        Assert.Equal(1584 - 48, centered.InnerWidth);
        Assert.Equal(0, GridModel.Metrics(2000, CdsBreakpoint.Max, CdsGridMode.Wide, false, CdsGridAlign.Start).Left);
        Assert.Equal(416, GridModel.Metrics(2000, CdsBreakpoint.Max, CdsGridMode.Wide, false, CdsGridAlign.End).Left);
        var full = GridModel.Metrics(2000, CdsBreakpoint.Max, CdsGridMode.Wide, true, CdsGridAlign.Center);
        Assert.Equal(0, full.Left);
        Assert.Equal(2000 - 48, full.InnerWidth);
    }

    [Fact]
    public void Breakpoint_precedence_is_explicit_then_page_then_width()
    {
        Assert.Equal(CdsBreakpoint.Sm, GridModel.Effective(CdsBreakpoint.Sm, CdsBreakpoint.Lg, 2000));
        Assert.Equal(CdsBreakpoint.Lg, GridModel.Effective(null, CdsBreakpoint.Lg, 400));
        Assert.Equal(CdsBreakpoint.Md, GridModel.Effective(null, null, 700));
        Assert.Equal(CdsBreakpoint.Max, GridModel.Effective(null, null, 1600));
    }
}

public class GridControlTests
{
    [Fact]
    public void Attached_values_round_trip_and_feed_the_model()
    {
        var column = new CdsColumn { Span = 4, SpanMd = 8, SpanLg = 6, OffsetLg = 2 };
        Assert.Equal(4, CdsGrid.GetSpan(column));
        Assert.Equal(8, column.SpanMd);
        var span = CdsGrid.SpanOf(column);
        Assert.Equal(6, GridModel.ResolveSpan(span, CdsBreakpoint.Lg));
        Assert.Equal(2, GridModel.ResolveOffset(CdsGrid.OffsetOf(column), CdsBreakpoint.Lg));
        Assert.Null(GridModel.ResolveOffset(CdsGrid.OffsetOf(column), CdsBreakpoint.Md));
    }

    [Fact]
    public void Any_view_can_be_a_grid_child()
    {
        var label = new Label();
        CdsGrid.SetSpanLg(label, 4);
        Assert.Equal(4, CdsGrid.GetSpanLg(label));
        var grid = new CdsGrid { Mode = CdsGridMode.Narrow, FullWidth = true, Align = CdsGridAlign.Start, Breakpoint = CdsBreakpoint.Md };
        grid.Add(label);
        grid.Add(new CdsColumn { Content = new Label() });
        Assert.Equal(CdsBreakpoint.Md, grid.GetEffectiveBreakpoint(2000));
    }

    [Fact]
    public void Grid_without_context_uses_its_own_width()
    {
        var grid = new CdsGrid();
        Assert.Equal(CdsBreakpoint.Lg, grid.GetEffectiveBreakpoint(1100));
        Assert.Equal(CdsBreakpoint.Sm, grid.GetEffectiveBreakpoint(320));
    }

    [Fact]
    public void Layout_manager_places_columns_on_the_grid()
    {
        var grid = new CdsGrid { Breakpoint = CdsBreakpoint.Lg };
        var a = new BoxView { HeightRequest = 20 };
        var b = new BoxView { HeightRequest = 30 };
        CdsGrid.SetSpan(a, 4);
        CdsGrid.SetSpan(b, 12);
        grid.Add(a);
        grid.Add(b);
        var layout = (ICrossPlatformLayout)grid;
        Assert.Equal(1056, layout.CrossPlatformMeasure(1056, double.PositiveInfinity).Width);
        layout.CrossPlatformArrange(new Rect(0, 0, 1056, 30));
        Assert.Equal(32, a.Frame.X);
        Assert.Equal(224, a.Frame.Width);
        Assert.Equal(16 + 4 * 64 + 16, b.Frame.X);
        Assert.Equal(12 * 64 - 32, b.Frame.Width);
    }

    [Fact]
    public void A_child_whose_span_becomes_zero_is_hidden_and_comes_back()
    {
        var grid = new CdsGrid { Breakpoint = CdsBreakpoint.Lg };
        var side = new BoxView { HeightRequest = 20 };
        var main = new BoxView { HeightRequest = 20 };
        CdsGrid.SetSpanSm(side, 0);
        CdsGrid.SetSpanLg(side, 4);
        CdsGrid.SetSpan(main, 12);
        grid.Add(side);
        grid.Add(main);
        var layout = (ICrossPlatformLayout)grid;

        layout.CrossPlatformMeasure(1056, double.PositiveInfinity);
        layout.CrossPlatformArrange(new Rect(0, 0, 1056, 20));
        Assert.True(side.IsVisible);

        // Below lg its span is 0: it must not stay drawn (and hit-testable) at its old frame.
        grid.Breakpoint = CdsBreakpoint.Sm;
        layout.CrossPlatformMeasure(320, double.PositiveInfinity);
        layout.CrossPlatformArrange(new Rect(0, 0, 320, 20));
        Assert.False(side.IsVisible);
        Assert.True(main.IsVisible);
        Assert.Equal(16, main.Frame.X);

        grid.Breakpoint = CdsBreakpoint.Lg;
        layout.CrossPlatformMeasure(1056, double.PositiveInfinity);
        layout.CrossPlatformArrange(new Rect(0, 0, 1056, 20));
        Assert.True(side.IsVisible);
        Assert.Equal(32, side.Frame.X);
    }

    [Fact]
    public void A_child_the_consumer_hid_stays_hidden_and_a_removed_child_leaves_visible()
    {
        var grid = new CdsGrid { Breakpoint = CdsBreakpoint.Sm };
        var hidden = new BoxView { IsVisible = false };
        var zero = new BoxView();
        CdsGrid.SetSpan(zero, 0);
        grid.Add(hidden);
        grid.Add(zero);
        var layout = (ICrossPlatformLayout)grid;
        layout.CrossPlatformMeasure(320, double.PositiveInfinity);
        layout.CrossPlatformArrange(new Rect(0, 0, 320, 20));
        Assert.False(hidden.IsVisible);
        Assert.False(zero.IsVisible);

        grid.Breakpoint = CdsBreakpoint.Lg;
        CdsGrid.SetSpan(zero, 4);
        layout.CrossPlatformMeasure(1056, double.PositiveInfinity);
        Assert.False(hidden.IsVisible);
        Assert.True(zero.IsVisible);

        CdsGrid.SetSpan(zero, 0);
        layout.CrossPlatformMeasure(1056, double.PositiveInfinity);
        Assert.False(zero.IsVisible);
        grid.Remove(zero);
        Assert.True(zero.IsVisible);
    }
}

public class SubgridTests
{
    static readonly GridMetrics Parent = GridModel.Metrics(1056, CdsBreakpoint.Lg, CdsGridMode.Wide, false, CdsGridAlign.Center);

    static GridSubgrid CellOf(GridMetrics parent, int span) =>
        new(span, parent.GutterStart, parent.GutterEnd, CdsBreakpoint.Lg);

    [Fact]
    public void A_subgrid_has_the_parent_span_as_columns_and_lines_up_with_the_parent()
    {
        var sub = GridModel.SubgridMetrics(Parent.BoxWidth(8), CellOf(Parent, 8), CdsGridMode.Wide);
        Assert.Equal(8, sub.Columns);
        Assert.Equal(0, sub.Margin);
        Assert.Equal(Parent.Pitch, sub.Pitch);
        // The subgrid sits in the parent's column box at column 0; its column k is the parent's column k.
        for (var k = 0; k < 8; k++) Assert.Equal(Parent.BoxX(k), Parent.BoxX(0) + sub.BoxX(k));
        Assert.Equal(Parent.BoxWidth(4), sub.BoxWidth(4));
    }

    [Fact]
    public void A_narrow_subgrid_hangs_into_the_parent_gutter()
    {
        var sub = GridModel.SubgridMetrics(Parent.BoxWidth(8), CellOf(Parent, 8), CdsGridMode.Narrow);
        Assert.Equal(-16, sub.BoxX(0));
        Assert.Equal(Parent.BoxWidth(2) + 16, sub.BoxWidth(2));
    }

    [Fact]
    public void A_wide_subgrid_in_a_narrow_grid_reaches_over_its_hang()
    {
        var narrow = GridModel.Metrics(1056, CdsBreakpoint.Lg, CdsGridMode.Narrow, false, CdsGridAlign.Center);
        var sub = GridModel.SubgridMetrics(narrow.BoxWidth(8), CellOf(narrow, 8), CdsGridMode.Wide);
        Assert.Equal(64, sub.Pitch);
        Assert.Equal(Parent.BoxX(0), narrow.BoxX(0) + sub.BoxX(0));
    }

    [Fact]
    public void A_condensed_subgrid_has_a_one_pixel_gutter_and_row_gap()
    {
        var sub = GridModel.SubgridMetrics(Parent.BoxWidth(4), CellOf(Parent, 4), CdsGridMode.Condensed);
        Assert.Equal(4, sub.Columns);
        Assert.Equal(2 * 64 - 1, sub.BoxWidth(2));
        Assert.Equal(1, sub.RowGap);
    }

    [Fact]
    public void Spans_and_offsets_count_the_subgrid_columns()
    {
        Assert.Equal(8, GridModel.ResolveSpan(new GridResponsive(null, null, null, null, null, null), CdsBreakpoint.Lg, 8));
        Assert.Equal(8, GridModel.ResolveSpan(new GridResponsive(12, null, null, null, null, null), CdsBreakpoint.Lg, 8));
        Assert.Equal(7, GridModel.ResolveOffset(new GridResponsive(10, null, null, null, null, null), CdsBreakpoint.Lg, 8));
    }

    [Fact]
    public void A_grid_placed_in_a_grid_is_a_subgrid_on_its_columns()
    {
        var outer = new CdsGrid { Breakpoint = CdsBreakpoint.Lg };
        var inner = new CdsGrid();
        CdsGrid.SetSpan(inner, 8);
        CdsGrid.SetOffset(inner, 8);
        var a = new BoxView { HeightRequest = 20 };
        var b = new BoxView { HeightRequest = 20 };
        CdsGrid.SetSpan(a, 4);
        inner.Add(a);
        inner.Add(b);
        outer.Add(inner);

        var o = (ICrossPlatformLayout)outer;
        o.CrossPlatformMeasure(1056, double.PositiveInfinity);
        o.CrossPlatformArrange(new Rect(0, 0, 1056, 40));
        Assert.Equal(16 + 8 * 64 + 16, inner.Frame.X);
        Assert.Equal(8 * 64 - 32, inner.Frame.Width);
        Assert.Equal(CdsBreakpoint.Lg, inner.GetEffectiveBreakpoint(inner.Frame.Width));

        var i = (ICrossPlatformLayout)inner;
        i.CrossPlatformMeasure(inner.Frame.Width, double.PositiveInfinity);
        i.CrossPlatformArrange(new Rect(0, 0, inner.Frame.Width, 40));
        // No outer margin: a starts where the parent's column 8 starts, and an unset span is the subgrid's full row.
        Assert.Equal(0, a.Frame.X);
        Assert.Equal(4 * 64 - 32, a.Frame.Width);
        Assert.Equal(0, b.Frame.X);
        Assert.Equal(8 * 64 - 32, b.Frame.Width);
    }

    [Fact]
    public void A_grid_deeper_inside_a_column_is_a_subgrid_too()
    {
        var outer = new CdsGrid { Breakpoint = CdsBreakpoint.Lg };
        var inner = new CdsGrid { Mode = CdsGridMode.Narrow };
        var cell = new BoxView { HeightRequest = 20 };
        CdsGrid.SetSpan(cell, 2);
        inner.Add(cell);
        var stack = new CdsStack();
        stack.Add(new Label());
        stack.Add(inner);
        outer.Add(new CdsColumn { Span = 4, Content = stack });

        var o = (ICrossPlatformLayout)outer;
        o.CrossPlatformMeasure(1056, double.PositiveInfinity);
        o.CrossPlatformArrange(new Rect(0, 0, 1056, 40));

        var i = (ICrossPlatformLayout)inner;
        i.CrossPlatformMeasure(4 * 64 - 32, double.PositiveInfinity);
        i.CrossPlatformArrange(new Rect(0, 0, 4 * 64 - 32, 20));
        Assert.Equal(-16, cell.Frame.X);
        Assert.Equal(2 * 64 - 16, cell.Frame.Width);
    }

    [Fact]
    public void A_grid_moved_out_of_a_grid_is_top_level_again()
    {
        var outer = new CdsGrid { Breakpoint = CdsBreakpoint.Lg };
        var inner = new CdsGrid();
        CdsGrid.SetSpan(inner, 8);
        outer.Add(inner);
        ((ICrossPlatformLayout)outer).CrossPlatformMeasure(1056, double.PositiveInfinity);
        Assert.NotNull(inner.ParentCell());

        outer.Remove(inner);
        Assert.Null(inner.GetValue(CdsGrid.CellProperty));
        Assert.Null(inner.ParentCell());
        Assert.Equal(CdsBreakpoint.Sm, inner.GetEffectiveBreakpoint(480));
    }
}
