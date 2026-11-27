using Microsoft.Maui.Layouts;

namespace Maui.Carbon;

/// <summary>
/// Carbon's 2x layout grid: 4, 8 or 16 equal columns by breakpoint with 16 of padding on each side of a column (a 32
/// gutter) and outer margins per breakpoint (<see cref="CdsBreakpointExtensions.Margin"/>). Children take a
/// <see cref="SpanProperty">Span</see> and an <see cref="OffsetProperty">Offset</see> (the column it starts at; right after the previous child by default), with
/// responsive overrides per breakpoint (<c>SpanSm</c>…<c>SpanMax</c>, <c>OffsetSm</c>…<c>OffsetMax</c>; a breakpoint
/// without a value inherits the smaller one, a span of 0 hides the child: it takes no space and is hidden, so it is neither
/// drawn, hit-tested nor in the accessibility tree, until its span is non-zero again) and wrap to new rows. Use it for page
/// structure, not inside components (use <see cref="CdsStack"/> there). The grid is centered and never wider than
/// 1584 unless <see cref="FullWidth"/>. The breakpoint comes from the page (<see cref="CdsContextKeys.Breakpoint"/>) unless
/// <see cref="Breakpoint"/> is set, and falls back to the grid's own width.
/// <para>
/// A grid inside a grid's column (the column itself, its content, or deeper inside it) is a subgrid, as in Carbon: it has
/// as many columns as that column spans, reaches over the column's gutters so its columns line up with the parent's, and
/// adds no outer margin. It takes the parent's breakpoint unless <see cref="Breakpoint"/> is set, ignores
/// <see cref="FullWidth"/> and <see cref="Align"/>, and its own <see cref="Mode"/> sets the gutters inside it (a narrow
/// subgrid in a wide grid, say). Its children's spans count the parent's columns.
/// </para>
/// </summary>
public class CdsGrid : Layout
{
    static readonly BindableProperty ContextBreakpointProperty = BindableProperty.Create(
        "ContextBreakpoint", typeof(CdsBreakpoint?), typeof(CdsGrid), null,
        propertyChanged: static (b, _, _) => ((CdsGrid)b).InvalidateMeasure());

    /// <summary>Bindable property for <see cref="Mode"/>.</summary>
    public static readonly BindableProperty ModeProperty = BindableProperty.Create(
        nameof(Mode), typeof(CdsGridMode), typeof(CdsGrid), CdsGridMode.Wide,
        propertyChanged: static (b, _, _) => ((CdsGrid)b).InvalidateMeasure());

    /// <summary>Bindable property for <see cref="FullWidth"/>.</summary>
    public static readonly BindableProperty FullWidthProperty = BindableProperty.Create(
        nameof(FullWidth), typeof(bool), typeof(CdsGrid), false,
        propertyChanged: static (b, _, _) => ((CdsGrid)b).InvalidateMeasure());

    /// <summary>Bindable property for <see cref="Align"/>.</summary>
    public static readonly BindableProperty AlignProperty = BindableProperty.Create(
        nameof(Align), typeof(CdsGridAlign), typeof(CdsGrid), CdsGridAlign.Center,
        propertyChanged: static (b, _, _) => ((CdsGrid)b).InvalidateMeasure());

    /// <summary>Bindable property for <see cref="Breakpoint"/>.</summary>
    public static readonly BindableProperty BreakpointProperty = BindableProperty.Create(
        nameof(Breakpoint), typeof(CdsBreakpoint?), typeof(CdsGrid), null,
        propertyChanged: static (b, _, _) => ((CdsGrid)b).InvalidateMeasure());

    /// <summary>Attached: the number of columns a child spans (null: the full row, 0: hidden), at every breakpoint without an override.</summary>
    public static readonly BindableProperty SpanProperty = Attach("Span");

    /// <summary>Attached: the span from the sm breakpoint (4 columns) up.</summary>
    public static readonly BindableProperty SpanSmProperty = Attach("SpanSm");

    /// <summary>Attached: the span from the md breakpoint (8 columns) up.</summary>
    public static readonly BindableProperty SpanMdProperty = Attach("SpanMd");

    /// <summary>Attached: the span from the lg breakpoint (16 columns) up.</summary>
    public static readonly BindableProperty SpanLgProperty = Attach("SpanLg");

    /// <summary>Attached: the span from the xlg breakpoint up.</summary>
    public static readonly BindableProperty SpanXlgProperty = Attach("SpanXlg");

    /// <summary>Attached: the span from the max breakpoint up.</summary>
    public static readonly BindableProperty SpanMaxProperty = Attach("SpanMax");

    /// <summary>Attached: the column a child starts at, counted from the start of its row (0 is the first column; unset: right after the previous child), at every breakpoint without an override.</summary>
    public static readonly BindableProperty OffsetProperty = Attach("Offset");

    /// <summary>Attached: the offset from the sm breakpoint up.</summary>
    public static readonly BindableProperty OffsetSmProperty = Attach("OffsetSm");

    /// <summary>Attached: the offset from the md breakpoint up.</summary>
    public static readonly BindableProperty OffsetMdProperty = Attach("OffsetMd");

    /// <summary>Attached: the offset from the lg breakpoint up.</summary>
    public static readonly BindableProperty OffsetLgProperty = Attach("OffsetLg");

    /// <summary>Attached: the offset from the xlg breakpoint up.</summary>
    public static readonly BindableProperty OffsetXlgProperty = Attach("OffsetXlg");

    /// <summary>Attached: the offset from the max breakpoint up.</summary>
    public static readonly BindableProperty OffsetMaxProperty = Attach("OffsetMax");

    // Set on a child the grid hid because its span is 0 at the current breakpoint: the grid shows it again when its span
    // becomes non-zero, while a child the consumer hid (IsVisible false) stays hidden.
    internal static readonly BindableProperty HiddenBySpanProperty = BindableProperty.CreateAttached(
        "CdsHiddenBySpan", typeof(bool), typeof(CdsGrid), false);

    // Set by the grid on each child it places: the child's columns, gutters and breakpoint. A grid inside the child reads
    // it from the nearest grid's direct child up its parents and lays out as a subgrid. A grid that is the child, or the
    // content of a column child, is invalidated here; one deeper is re-measured because the column's width changes with
    // every change of these values.
    internal static readonly BindableProperty CellProperty = BindableProperty.CreateAttached(
        "CdsGridCell", typeof(GridSubgrid?), typeof(CdsGrid), null,
        propertyChanged: static (b, _, _) =>
        {
            if (b is CdsGrid nested) ((IView)nested).InvalidateMeasure();
            else if (b is ContentView { Content: CdsGrid inner }) ((IView)inner).InvalidateMeasure();
        });

    /// <summary>Creates a wide grid that follows the page breakpoint.</summary>
    public CdsGrid()
    {
        SetDynamicResource(ContextBreakpointProperty, CdsContextKeys.Breakpoint);
    }

    /// <summary>The gutter mode. Default <see cref="CdsGridMode.Wide"/>.</summary>
    public CdsGridMode Mode
    {
        get => (CdsGridMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <summary>Lifts the 1584 maximum width. Default <c>false</c>. A subgrid has no maximum width of its own.</summary>
    public bool FullWidth
    {
        get => (bool)GetValue(FullWidthProperty);
        set => SetValue(FullWidthProperty, value);
    }

    /// <summary>Where the grid sits when wider than its maximum. Default <see cref="CdsGridAlign.Center"/>. Not used by a subgrid.</summary>
    public CdsGridAlign Align
    {
        get => (CdsGridAlign)GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>Forces a breakpoint (column count and margin) instead of the page's (a subgrid's: the parent's). Default <c>null</c>.</summary>
    public CdsBreakpoint? Breakpoint
    {
        get => (CdsBreakpoint?)GetValue(BreakpointProperty);
        set => SetValue(BreakpointProperty, value);
    }

    /// <summary>The breakpoint in effect for a grid <paramref name="width"/> wide.</summary>
    public CdsBreakpoint GetEffectiveBreakpoint(double width) => EffectiveBreakpoint(width, ParentCell());

    internal CdsBreakpoint EffectiveBreakpoint(double width, GridSubgrid? parent) =>
        GridModel.Effective(Breakpoint, parent?.Breakpoint ?? (CdsBreakpoint?)GetValue(ContextBreakpointProperty), width);

    /// <summary>
    /// The column this grid sits in when it is nested in another grid: what that grid set on its direct child on the way
    /// up (stopping at the page), or <c>null</c> for a top-level grid.
    /// </summary>
    internal GridSubgrid? ParentCell()
    {
        Element cell = this;
        for (var parent = Parent; parent is not null and not Page; cell = parent, parent = parent.Parent)
            if (parent is CdsGrid) return (GridSubgrid?)cell.GetValue(CellProperty);
        return null;
    }

    /// <summary>Gets <see cref="SpanProperty"/>.</summary>
    public static int? GetSpan(BindableObject view) => (int?)view.GetValue(SpanProperty);
    /// <summary>Sets <see cref="SpanProperty"/>.</summary>
    public static void SetSpan(BindableObject view, int? value) => view.SetValue(SpanProperty, value);
    /// <summary>Gets <see cref="SpanSmProperty"/>.</summary>
    public static int? GetSpanSm(BindableObject view) => (int?)view.GetValue(SpanSmProperty);
    /// <summary>Sets <see cref="SpanSmProperty"/>.</summary>
    public static void SetSpanSm(BindableObject view, int? value) => view.SetValue(SpanSmProperty, value);
    /// <summary>Gets <see cref="SpanMdProperty"/>.</summary>
    public static int? GetSpanMd(BindableObject view) => (int?)view.GetValue(SpanMdProperty);
    /// <summary>Sets <see cref="SpanMdProperty"/>.</summary>
    public static void SetSpanMd(BindableObject view, int? value) => view.SetValue(SpanMdProperty, value);
    /// <summary>Gets <see cref="SpanLgProperty"/>.</summary>
    public static int? GetSpanLg(BindableObject view) => (int?)view.GetValue(SpanLgProperty);
    /// <summary>Sets <see cref="SpanLgProperty"/>.</summary>
    public static void SetSpanLg(BindableObject view, int? value) => view.SetValue(SpanLgProperty, value);
    /// <summary>Gets <see cref="SpanXlgProperty"/>.</summary>
    public static int? GetSpanXlg(BindableObject view) => (int?)view.GetValue(SpanXlgProperty);
    /// <summary>Sets <see cref="SpanXlgProperty"/>.</summary>
    public static void SetSpanXlg(BindableObject view, int? value) => view.SetValue(SpanXlgProperty, value);
    /// <summary>Gets <see cref="SpanMaxProperty"/>.</summary>
    public static int? GetSpanMax(BindableObject view) => (int?)view.GetValue(SpanMaxProperty);
    /// <summary>Sets <see cref="SpanMaxProperty"/>.</summary>
    public static void SetSpanMax(BindableObject view, int? value) => view.SetValue(SpanMaxProperty, value);
    /// <summary>Gets <see cref="OffsetProperty"/>.</summary>
    public static int? GetOffset(BindableObject view) => (int?)view.GetValue(OffsetProperty);
    /// <summary>Sets <see cref="OffsetProperty"/>.</summary>
    public static void SetOffset(BindableObject view, int? value) => view.SetValue(OffsetProperty, value);
    /// <summary>Gets <see cref="OffsetSmProperty"/>.</summary>
    public static int? GetOffsetSm(BindableObject view) => (int?)view.GetValue(OffsetSmProperty);
    /// <summary>Sets <see cref="OffsetSmProperty"/>.</summary>
    public static void SetOffsetSm(BindableObject view, int? value) => view.SetValue(OffsetSmProperty, value);
    /// <summary>Gets <see cref="OffsetMdProperty"/>.</summary>
    public static int? GetOffsetMd(BindableObject view) => (int?)view.GetValue(OffsetMdProperty);
    /// <summary>Sets <see cref="OffsetMdProperty"/>.</summary>
    public static void SetOffsetMd(BindableObject view, int? value) => view.SetValue(OffsetMdProperty, value);
    /// <summary>Gets <see cref="OffsetLgProperty"/>.</summary>
    public static int? GetOffsetLg(BindableObject view) => (int?)view.GetValue(OffsetLgProperty);
    /// <summary>Sets <see cref="OffsetLgProperty"/>.</summary>
    public static void SetOffsetLg(BindableObject view, int? value) => view.SetValue(OffsetLgProperty, value);
    /// <summary>Gets <see cref="OffsetXlgProperty"/>.</summary>
    public static int? GetOffsetXlg(BindableObject view) => (int?)view.GetValue(OffsetXlgProperty);
    /// <summary>Sets <see cref="OffsetXlgProperty"/>.</summary>
    public static void SetOffsetXlg(BindableObject view, int? value) => view.SetValue(OffsetXlgProperty, value);
    /// <summary>Gets <see cref="OffsetMaxProperty"/>.</summary>
    public static int? GetOffsetMax(BindableObject view) => (int?)view.GetValue(OffsetMaxProperty);
    /// <summary>Sets <see cref="OffsetMaxProperty"/>.</summary>
    public static void SetOffsetMax(BindableObject view, int? value) => view.SetValue(OffsetMaxProperty, value);

    internal static GridResponsive SpanOf(BindableObject view) => new(
        GetSpan(view), GetSpanSm(view), GetSpanMd(view), GetSpanLg(view), GetSpanXlg(view), GetSpanMax(view));

    internal static GridResponsive OffsetOf(BindableObject view) => new(
        GetOffset(view), GetOffsetSm(view), GetOffsetMd(view), GetOffsetLg(view), GetOffsetXlg(view), GetOffsetMax(view));

    /// <inheritdoc/>
    protected override ILayoutManager CreateLayoutManager() => new CdsGridLayoutManager(this);

    /// <inheritdoc/>
    protected override void OnChildRemoved(Element child, int oldLogicalIndex)
    {
        base.OnChildRemoved(child, oldLogicalIndex);
        // A grid moved out of this one is a top-level grid again.
        child.ClearValue(CellProperty);
        // A child hidden for a span of 0 leaves the grid as it came in: visible.
        if (child is VisualElement view && (bool)view.GetValue(HiddenBySpanProperty))
        {
            view.ClearValue(HiddenBySpanProperty);
            view.IsVisible = true;
        }
    }

    static BindableProperty Attach(string name) => BindableProperty.CreateAttached(
        name, typeof(int?), typeof(CdsGrid), null,
        propertyChanged: static (b, _, _) =>
        {
            if ((b as Element)?.Parent is CdsGrid grid) grid.InvalidateMeasure();
        });
}

sealed class CdsGridLayoutManager(CdsGrid grid) : LayoutManager(grid)
{
    sealed record Plan(GridMetrics Metrics, IReadOnlyList<GridCell> Cells, IReadOnlyList<IView> Views, IReadOnlyList<bool> HiddenByConsumer);

    Plan MakePlan(double width)
    {
        var parent = grid.ParentCell();
        var breakpoint = grid.EffectiveBreakpoint(width, parent);
        var metrics = parent is { } cell
            ? GridModel.SubgridMetrics(width, cell, grid.Mode)
            : GridModel.Metrics(width, breakpoint, grid.Mode, grid.FullWidth, grid.Align);
        var views = new List<IView>();
        var items = new List<(int, int?)>();
        var byConsumer = new List<bool>();
        foreach (var child in grid)
        {
            views.Add(child);
            // A child the grid itself hid for a span of 0 is not "collapsed": its span decides again.
            var hidden = child.Visibility == Visibility.Collapsed
                         && !(child is BindableObject flagged && (bool)flagged.GetValue(CdsGrid.HiddenBySpanProperty));
            byConsumer.Add(hidden);
            if (child is BindableObject b)
                items.Add((hidden ? 0 : GridModel.ResolveSpan(CdsGrid.SpanOf(b), breakpoint, metrics.Columns),
                    GridModel.ResolveOffset(CdsGrid.OffsetOf(b), breakpoint, metrics.Columns)));
            else
                items.Add((hidden ? 0 : metrics.Columns, null));
        }
        var plan = new Plan(metrics, GridModel.Place(metrics.Columns, items), views, byConsumer);
        SyncSpanVisibility(plan);
        StampCells(plan, breakpoint);
        return plan;
    }

    // Tell every placed child its columns, so a grid inside it lays out as a subgrid on them.
    static void StampCells(Plan plan, CdsBreakpoint breakpoint)
    {
        foreach (var cell in plan.Cells)
        {
            if (cell.Hidden || plan.Views[cell.Index] is not BindableObject child) continue;
            child.SetValue(CdsGrid.CellProperty,
                new GridSubgrid(cell.Span, plan.Metrics.GutterStart, plan.Metrics.GutterEnd, breakpoint));
        }
    }

    // Skipping a span-0 child in arrange leaves its platform view at its last frame: still drawn over the reflowed
    // columns, hit-testable and in the accessibility tree. Hide it instead, and show it again once its span is non-zero.
    static void SyncSpanVisibility(Plan plan)
    {
        foreach (var cell in plan.Cells)
        {
            if (plan.Views[cell.Index] is not VisualElement view) continue;
            var bySpan = cell.Hidden && !plan.HiddenByConsumer[cell.Index];
            var flagged = (bool)view.GetValue(CdsGrid.HiddenBySpanProperty);
            if (bySpan)
            {
                if (!flagged) view.SetValue(CdsGrid.HiddenBySpanProperty, true);
                if (view.IsVisible) view.IsVisible = false;
            }
            else if (flagged)
            {
                view.SetValue(CdsGrid.HiddenBySpanProperty, false);
                view.IsVisible = true;
            }
        }
    }

    static double[] RowHeights(Plan plan, double[] heights)
    {
        var rows = plan.Cells.Where(c => !c.Hidden).Select(c => c.Row).DefaultIfEmpty(-1).Max() + 1;
        var result = new double[rows];
        foreach (var cell in plan.Cells.Where(c => !c.Hidden))
            result[cell.Row] = Math.Max(result[cell.Row], heights[cell.Index]);
        return result;
    }

    static double[] MeasureChildren(Plan plan)
    {
        var heights = new double[plan.Views.Count];
        foreach (var cell in plan.Cells.Where(c => !c.Hidden))
        {
            var size = plan.Views[cell.Index].Measure(plan.Metrics.BoxWidth(cell.Span), double.PositiveInfinity);
            heights[cell.Index] = size.Height;
        }
        return heights;
    }

    public override Size Measure(double widthConstraint, double heightConstraint)
    {
        var padding = grid.Padding;
        var available = double.IsFinite(widthConstraint) ? widthConstraint : CdsBreakpoints.Lg;
        var width = Math.Max(0, available - padding.HorizontalThickness);
        var plan = MakePlan(width);
        var rows = RowHeights(plan, MeasureChildren(plan));
        var height = rows.Sum() + Math.Max(0, rows.Length - 1) * plan.Metrics.RowGap + padding.VerticalThickness;
        return new Size(available, height);
    }

    public override Size ArrangeChildren(Rect bounds)
    {
        var padding = grid.Padding;
        var width = Math.Max(0, bounds.Width - padding.HorizontalThickness);
        var plan = MakePlan(width);
        var rows = RowHeights(plan, MeasureChildren(plan));
        var rowTop = new double[rows.Length];
        var y = bounds.Y + padding.Top;
        for (var r = 0; r < rows.Length; r++)
        {
            rowTop[r] = y;
            y += rows[r] + plan.Metrics.RowGap;
        }
        foreach (var cell in plan.Cells)
        {
            if (cell.Hidden) continue;
            var view = plan.Views[cell.Index];
            var boxWidth = plan.Metrics.BoxWidth(cell.Span);
            var x = bounds.X + padding.Left + plan.Metrics.BoxX(cell.Column);
            // The view applies its own margin and alignment inside the slot.
            view.Arrange(new Rect(x, rowTop[cell.Row], boxWidth, rows[cell.Row]));
        }
        return new Size(bounds.Width, bounds.Height);
    }
}
