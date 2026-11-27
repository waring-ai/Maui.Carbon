namespace Maui.Carbon;

/// <summary>The span or offset values of one grid child: a base value and optional per-breakpoint overrides.</summary>
internal readonly record struct GridResponsive(int? Base, int? Sm, int? Md, int? Lg, int? Xlg, int? Max)
{
    /// <summary>
    /// The value at a breakpoint. Values cascade upward: a breakpoint without a value inherits the next smaller
    /// one, and the base value applies below every override.
    /// </summary>
    public int? For(CdsBreakpoint breakpoint)
    {
        var bp = (int)breakpoint;
        for (var i = bp; i >= 0; i--)
        {
            var value = i switch { 0 => Sm, 1 => Md, 2 => Lg, 3 => Xlg, _ => Max };
            if (value is not null) return value;
        }
        return Base;
    }
}

/// <summary>
/// What a grid tells each child it places: the columns the child spans, the gutters on either side of its column box and
/// the breakpoint. A grid inside that child (directly, or deeper inside a column) lays out as a subgrid on these columns.
/// </summary>
internal readonly record struct GridSubgrid(int Columns, double BleedStart, double BleedEnd, CdsBreakpoint Breakpoint);

/// <summary>One child placed in the grid. <c>Hidden</c> children (span 0 or collapsed) take no space.</summary>
internal readonly record struct GridCell(int Index, int Row, int Column, int Span, bool Hidden);

/// <summary>Resolved geometry of the grid at a width: where the columns sit.</summary>
internal readonly record struct GridMetrics(
    int Columns, double Margin, double Left, double InnerWidth, double Pitch, double GutterStart, double GutterEnd, double RowGap)
{
    /// <summary>The x of a column box (the area inside the gutters) for a cell at a column.</summary>
    public double BoxX(int column) => Left + Margin + column * Pitch + GutterStart;

    /// <summary>The width of the column box of a cell spanning <paramref name="span"/> columns.</summary>
    public double BoxWidth(int span) => Math.Max(0, span * Pitch - GutterStart - GutterEnd);
}

/// <summary>Pure span, offset, row-wrapping and column geometry for <see cref="CdsGrid"/> (Carbon's 2x grid).</summary>
internal static class GridModel
{
    /// <summary>Gutter of the wide mode: 16 of padding on each side of a column.</summary>
    public const double WideGutter = 32;

    /// <summary>Gutter of the condensed mode.</summary>
    public const double CondensedGutter = 1;

    /// <summary>The widest a grid gets unless it is full width (99rem).</summary>
    public const double MaxWidth = CdsBreakpoints.Max;

    /// <summary>
    /// The span of a child at a breakpoint: its resolved value, or the full row; clamped to 0 (hidden) through the column
    /// count (the breakpoint's, or a subgrid's <paramref name="columns"/>).
    /// </summary>
    public static int ResolveSpan(GridResponsive span, CdsBreakpoint breakpoint, int? columns = null)
    {
        var count = columns ?? breakpoint.Columns();
        return Math.Clamp(span.For(breakpoint) ?? count, 0, count);
    }

    /// <summary>
    /// The offset of a child at a breakpoint: the number of columns between the start of the row and the child, clamped to
    /// the column count minus one; <c>null</c> when unset (the child follows the previous one).
    /// </summary>
    public static int? ResolveOffset(GridResponsive offset, CdsBreakpoint breakpoint, int? columns = null) =>
        offset.For(breakpoint) is { } value ? Math.Clamp(value, 0, (columns ?? breakpoint.Columns()) - 1) : null;

    /// <summary>
    /// Places children left to right, wrapping to a new row when a child no longer fits. A span of 0 hides the child. An offset
    /// is the child's column counted from the start of its row (so <c>{ span 4, offset 12 }</c> fills the last four columns
    /// of a 16 column row); an offset behind the columns already used moves the child to a new row.
    /// </summary>
    public static IReadOnlyList<GridCell> Place(int columns, IReadOnlyList<(int Span, int? Offset)> items)
    {
        var cells = new List<GridCell>(items.Count);
        var row = 0;
        var next = 0;
        for (var i = 0; i < items.Count; i++)
        {
            var span = Math.Clamp(items[i].Span, 0, columns);
            if (span == 0)
            {
                cells.Add(new GridCell(i, row, next, 0, true));
                continue;
            }
            int? offset = items[i].Offset is { } o ? Math.Clamp(o, 0, columns - 1) : null;
            var start = offset ?? next;
            if (next > 0 && ((offset is not null && start < next) || start + span > columns))
            {
                row++;
                start = offset ?? 0;
            }
            start = Math.Clamp(start, 0, columns - span);
            cells.Add(new GridCell(i, row, start, span, false));
            next = start + span;
            if (next >= columns)
            {
                row++;
                next = 0;
            }
        }
        return cells;
    }

    /// <summary>The column geometry of a grid <paramref name="width"/> wide.</summary>
    public static GridMetrics Metrics(double width, CdsBreakpoint breakpoint, CdsGridMode mode, bool fullWidth, CdsGridAlign align)
    {
        var columns = breakpoint.Columns();
        var total = fullWidth ? width : Math.Min(width, MaxWidth);
        var left = align switch
        {
            CdsGridAlign.Start => 0,
            CdsGridAlign.End => width - total,
            _ => (width - total) / 2,
        };
        var margin = Math.Min(breakpoint.Margin(), total / 2);
        var inner = Math.Max(0, total - 2 * margin);
        var (start, end, rowGap) = Gutters(mode);
        return new GridMetrics(columns, margin, left, inner, inner / columns, start, end, rowGap);
    }

    /// <summary>
    /// The column geometry of a subgrid <paramref name="width"/> wide placed in a parent's column box (Carbon's
    /// <c>cds--subgrid</c>): it has as many columns as the parent column spans and reaches over the parent's gutters on
    /// both sides (negative margins of the parent's gutter start and end), so its columns line up with the parent's. It
    /// has no outer margin and no maximum width; its own <paramref name="mode"/> sets the gutters inside it.
    /// </summary>
    public static GridMetrics SubgridMetrics(double width, GridSubgrid parent, CdsGridMode mode)
    {
        var columns = Math.Max(1, parent.Columns);
        var total = Math.Max(0, width + parent.BleedStart + parent.BleedEnd);
        var (start, end, rowGap) = Gutters(mode);
        return new GridMetrics(columns, 0, -parent.BleedStart, total, total / columns, start, end, rowGap);
    }

    /// <summary>The gutter before and after each column box, and the row gap, of a mode.</summary>
    static (double Start, double End, double RowGap) Gutters(CdsGridMode mode) => mode switch
    {
        CdsGridMode.Narrow => (0d, WideGutter / 2, 0d),
        CdsGridMode.Condensed => (CondensedGutter / 2, CondensedGutter / 2, CondensedGutter),
        _ => (WideGutter / 2, WideGutter / 2, 0d),
    };

    /// <summary>The breakpoint a grid uses: an explicit one, else the page's, else the one its own width falls in.</summary>
    public static CdsBreakpoint Effective(CdsBreakpoint? explicitBreakpoint, CdsBreakpoint? context, double width) =>
        explicitBreakpoint ?? context ?? CdsBreakpointExtensions.FromWidth(width);
}
