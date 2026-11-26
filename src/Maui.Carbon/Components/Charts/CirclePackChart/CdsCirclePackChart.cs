namespace Maui.Carbon;

/// <summary>
/// Nested circles that show a hierarchy of up to three levels, sized by value (Carbon's circle pack chart). Each top-level
/// <see cref="CdsChartNode"/> in <see cref="CdsChart.Data"/> is a group (legend item and color); one top-level node draws
/// its children in its single color. Circles fill with their group color at 30% and stroke it at 1.5px; the hovered circle
/// fills solid and outlines its children in white. With <see cref="IsZoomEnabled"/> and three-level data, clicking a
/// second-level circle zooms in on it. Use a treemap or bars when sizes must be compared, and a tree chart for structure
/// without values.
/// </summary>
public class CdsCirclePackChart : CdsChart
{
    /// <summary>Bindable property for <see cref="HierarchyLevel"/>.</summary>
    public static readonly BindableProperty HierarchyLevelProperty = BindableProperty.Create(
        nameof(HierarchyLevel), typeof(int), typeof(CdsCirclePackChart), 2, propertyChanged: Changed, coerceValue: static (_, v) => Math.Clamp((int)v, 1, 3));

    /// <summary>Bindable property for <see cref="MainGroupPadding"/>.</summary>
    public static readonly BindableProperty MainGroupPaddingProperty = BindableProperty.Create(
        nameof(MainGroupPadding), typeof(double), typeof(CdsCirclePackChart), 4d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ChildrenPadding"/>.</summary>
    public static readonly BindableProperty ChildrenPaddingProperty = BindableProperty.Create(
        nameof(ChildrenPadding), typeof(double), typeof(CdsCirclePackChart), 2d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="IsZoomEnabled"/>.</summary>
    public static readonly BindableProperty IsZoomEnabledProperty = BindableProperty.Create(
        nameof(IsZoomEnabled), typeof(bool), typeof(CdsCirclePackChart), false, propertyChanged: static (b, _, _) =>
        {
            var chart = (CdsCirclePackChart)b;
            chart._focal = null;
            chart.RequestRebuild();
        });

    /// <summary>Fill opacity of circles (Carbon's <c>circlePack.circles.fillOpacity</c>).</summary>
    const float FillOpacity = 0.3f;

    /// <summary>Carbon adds 3px to the padding for the 1.5px stroke.</summary>
    const double StrokeAllowance = 3;

    /// <summary>Carbon's zoom scales the canvas by 2 around the focal circle.</summary>
    const float ZoomLevel = 2;

    CdsChartNode? _focal;
    IReadOnlyList<object>? _lastItems;
    readonly Dictionary<int, HierarchyNode> _markNodes = [];
    int _level = 2;

    /// <summary>Raised when a circle is clicked or activated with Enter (Carbon's <c>circlePack.onCircleClick</c>).</summary>
    public event EventHandler<CdsChartNodeEventArgs>? CircleClicked;

    /// <summary>How many levels are drawn, 1 to 3 (Carbon's <c>circlePack.hierarchyLevel</c>). Default 2. With zoom on, the third level shows only inside the zoomed circle.</summary>
    public int HierarchyLevel
    {
        get => (int)GetValue(HierarchyLevelProperty);
        set => SetValue(HierarchyLevelProperty, value);
    }

    /// <summary>The gap between top-level circles, in pixels (Carbon's <c>circlePack.padding.mainGroup</c>). Default 4.</summary>
    public double MainGroupPadding
    {
        get => (double)GetValue(MainGroupPaddingProperty);
        set => SetValue(MainGroupPaddingProperty, value);
    }

    /// <summary>The gap between circles inside a group, in pixels (Carbon's <c>circlePack.padding.children</c>). Default 2.</summary>
    public double ChildrenPadding
    {
        get => (double)GetValue(ChildrenPaddingProperty);
        set => SetValue(ChildrenPaddingProperty, value);
    }

    /// <summary>
    /// Whether clicking a second-level circle with children zooms in to show its children (Carbon's <c>canvasZoom.enabled</c>).
    /// Default <c>false</c>. Carbon keeps zoom for data with more than two levels; clicking again (or the background) zooms out.
    /// </summary>
    public bool IsZoomEnabled
    {
        get => (bool)GetValue(IsZoomEnabledProperty);
        set => SetValue(IsZoomEnabledProperty, value);
    }

    /// <summary>The zoomed-in circle's node, or null (for tests).</summary>
    internal CdsChartNode? Focal => _focal;

    private protected override string ChartKind => "Circle pack chart";

    private protected override ChartSkeleton Skeleton => ChartSkeleton.Pie;

    private protected override IReadOnlyList<ChartLegendExtra> LegendExtras => ZoomLegend(IsZoomEnabled, Data);

    /// <summary>Carbon's "Click to zoom" legend item, shown when zoom is on and the data has three levels.</summary>
    internal static IReadOnlyList<ChartLegendExtra> ZoomLegend(bool zoom, System.Collections.IEnumerable? data) =>
        zoom && data is not null && DataDepth(data.OfType<CdsChartNode>().ToList()) > 2
            ? [new ChartLegendExtra(ChartLegendExtraKind.Zoom, "Click to zoom")]
            : [];

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) => CdsTreemapChart.TopLevelNames(items);

    private protected override bool IsEmpty(IReadOnlyList<object> items) => !items.OfType<CdsChartNode>().Any();

    /// <summary>Carbon's data depth: 3 when any group has a child with children (one top-level node: measured below it).</summary>
    internal static int DataDepth(IReadOnlyList<CdsChartNode> data)
    {
        IEnumerable<CdsChartNode> list = data.Count == 1 && data[0].Children.Count > 0 ? data[0].Children : data;
        return list.Any(d => d.Children.Any(c => c.Children.Count > 0)) ? 3 : 2;
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        if (!ReferenceEquals(_lastItems, ctx.Items))
        {
            _lastItems = ctx.Items;
            _focal = null;
        }
        var all = ctx.Items.OfType<CdsChartNode>().ToList();
        var singleParent = all.Count == 1;
        var zoom = IsZoomEnabled;
        _level = zoom ? (_focal is not null ? 3 : Math.Min(HierarchyLevel, 2)) : HierarchyLevel;
        var level = _level;
        var data = CdsTreemapChart.ActiveNodes(ctx);
        var top = singleParent && data.Count == 1 && data[0].Children.Count > 0 ? data[0].Children : (IEnumerable<CdsChartNode>)data;
        var root = HierarchyNode.Build("", top).Sum().SortByValueDescending();
        foreach (var n in root.PreOrder())
            n.Group = n.Depth == 0 ? "" : singleParent ? all[0].Name ?? "" : n.TopParent().Name;
        var main = MainGroupPadding;
        var child = ChildrenPadding;
        ChartPack.Pack(root, ctx.Width, ctx.Height, d => (d.Depth >= 1 ? child : main) + StrokeAllowance);

        var nodes = root.Descendants().Skip(1).ToList();
        var focal = _focal is null ? null : nodes.FirstOrDefault(n => ReferenceEquals(n.Data, _focal));
        var zoomed = zoom && focal is not null && level == 3;
        var w = ctx.Width;
        var h = ctx.Height;
        PointF Center(HierarchyNode n) => zoomed
            ? new PointF((float)(w / 2 + ZoomLevel * (n.X - focal!.X)), (float)(h / 2 + ZoomLevel * (n.Y - focal!.Y)))
            : new PointF((float)n.X, (float)n.Y);
        float Radius(HierarchyNode n) => (float)(zoomed ? n.R * ZoomLevel : n.R);
        bool IsFocal(HierarchyNode n) => focal is not null && (ReferenceEquals(n, focal) || (focal.Children?.Contains(n) ?? false));

        var scene = new ChartScene { Width = w, Height = h, Plot = new RectF(0, 0, w, h) };
        var shown = nodes.Where(n => n.Depth <= level).ToList();
        var marks = new Dictionary<HierarchyNode, int>();
        // Marks in reading order (pre-order: a circle, then the circles inside it); later marks are hit first.
        var id = 0;
        _markNodes.Clear();
        foreach (var n in root.PreOrder())
        {
            if (n.Depth == 0 || n.Depth > level) continue;
            marks[n] = id;
            _markNodes[id] = n;
            id++;
        }

        // Circles in breadth-first order (Carbon's descendants()), parents below their children.
        foreach (var n in shown)
        {
            var nonFocal = zoomed && !IsFocal(n);
            var paint = nonFocal ? ChartPaint.OfKey(CdsColorKeys.IconDisabled) : ChartPaint.OfSeries(n.Group);
            scene.Shapes.Add(new ChartCircle
            {
                Center = Center(n),
                Radius = Radius(n),
                Fill = paint,
                FillOpacity = FillOpacity,
                HoverFillOpacity = 1,
                Stroke = paint,
                StrokeWidth = 1.5f,
                Group = singleParent ? null : n.Group,
                Mark = marks[n],
                Hover = ChartHoverStyle.None,
                Clip = zoomed,
            });
        }
        // The hovered circle outlines its children (below the top level) in white.
        foreach (var n in shown)
        {
            if (n.Children is not { } kids) continue;
            foreach (var c in kids)
            {
                if (c.Depth <= 1 || c.Depth > level) continue;
                scene.Shapes.Add(new ChartCircle
                {
                    Center = Center(c), Radius = Radius(c), Stroke = ChartPaint.OfKey(CdsColorKeys.TextOnColor), StrokeWidth = 1.5f,
                    HoverOnly = true, Mark = marks[n], Group = singleParent ? null : n.Group, Clip = zoomed,
                });
            }
        }

        foreach (var (n, mark) in marks.OrderBy(kv => kv.Value))
        {
            var c = Center(n);
            var r = Radius(n);
            var disabled = level > 2 && !IsFocal(n);
            var rows = new List<ChartTooltipRow>();
            if (!disabled)
            {
                rows.Add(new ChartTooltipRow(n.Name, n.Children is null ? ctx.Value(n.Value) : null, ChartPaint.OfSeries(n.Group)));
                if (n.Children is { } kids)
                {
                    foreach (var k in kids) rows.Add(new ChartTooltipRow(k.Name, ctx.Value(k.Children is null || k.Data?.Value is null ? k.Value : k.Data.Value)));
                    rows.Add(new ChartTooltipRow(ctx.TotalLabel, ctx.Value(n.Value), Bold: true));
                }
            }
            var zoomable = zoom && n.Depth == 2 && n.Children is not null && level <= 2;
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = new RectF(c.X - r, c.Y - r, 2 * r, 2 * r),
                Radius = r,
                Anchor = c,
                Group = n.Group,
                Rows = rows,
                Description = n.Data?.Description ?? $"{n.Name}: {ctx.Value(n.Value)}"
                    + (n.Depth > 1 || singleParent ? $", in {(n.Parent?.Depth > 0 ? n.Parent.Name : n.Group)}" : "")
                    + (zoomable ? ", press Enter to zoom in" : ""),
            });
        }
        scene.Summary = id == 1 ? "1 circle" : $"{id} circles";
        return scene;
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (ActiveMark >= 0 && _markNodes.ContainsKey(ActiveMark))
        {
            ActivateMark(ActiveMark);
            return;
        }
        if (_focal is not null)
        {
            // A click on the background zooms out.
            _focal = null;
            ClearActive();
            RequestRebuild();
            return;
        }
        base.OnActivated();
    }

    /// <summary>Carbon's circle click: zoom out when zoomed, zoom into a second-level parent, and report the circle.</summary>
    internal void ActivateMark(int id)
    {
        if (!_markNodes.TryGetValue(id, out var n)) return;
        if (IsZoomEnabled && _level > 2)
        {
            _focal = null;
            RequestRebuild();
        }
        else if (IsZoomEnabled && n.Depth == 2 && n.Children is not null)
        {
            _focal = n.Data;
            RequestRebuild();
        }
        if (n.Data is { } data) CircleClicked?.Invoke(this, new CdsChartNodeEventArgs(data));
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var rows = new List<string[]>();
        double Walk(IList<CdsChartNode> children, string parent)
        {
            double total = 0;
            foreach (var c in children)
            {
                if (c.Children.Count > 0)
                {
                    if (c.Value is { } own) total += own;
                    var s = Walk(c.Children, c.Name ?? "");
                    rows.Add([c.Name ?? "", parent, ctx.Value(s)]);
                    total += s;
                }
                else
                {
                    var v = c.Value ?? 0;
                    total += v;
                    rows.Add([c.Name ?? "", parent, ctx.Value(v)]);
                }
            }
            return total;
        }
        foreach (var d in CdsTreemapChart.ActiveNodes(ctx))
        {
            var v = d.Value ?? 0;
            if (d.Children.Count > 0) v += Walk(d.Children, d.Name ?? "");
            rows.Add(["–", d.Name ?? "", ctx.Value(v)]);
        }
        return new ChartTable(["Child", "Parent", "Value"], rows);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsCirclePackChart)b).RequestRebuild();
}
