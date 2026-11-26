namespace Maui.Carbon;

/// <summary>
/// A left-to-right node-link diagram of a hierarchy (Carbon's tree chart), as a tidy tree or a dendrogram. Put the root's
/// children in <see cref="CdsChart.Data"/> as nested <see cref="CdsChartNode"/>s; <see cref="RootTitle"/> names the root.
/// Parents below the root collapse and expand on click (or Enter on the data cursor). Links are curves in
/// <c>border-strong-01</c> at 40%; parents are <c>text-secondary</c> dots labelled before them, leaves
/// <c>border-strong-01</c> dots labelled after them. No legend or tooltips, as in Carbon. Use a treemap or circle pack to
/// compare quantities and a tree view for navigation.
/// </summary>
public class CdsTreeChart : CdsChart
{
    /// <summary>Bindable property for <see cref="TreeType"/>.</summary>
    public static readonly BindableProperty TreeTypeProperty = BindableProperty.Create(
        nameof(TreeType), typeof(CdsTreeChartType), typeof(CdsTreeChart), CdsTreeChartType.Tree, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="RootTitle"/>.</summary>
    public static readonly BindableProperty RootTitleProperty = BindableProperty.Create(
        nameof(RootTitle), typeof(string), typeof(CdsTreeChart), "Tree", propertyChanged: Changed);

    /// <summary>Gap between a node and its label (Carbon's NODE_OFFSET).</summary>
    internal const float NodeOffset = 6;

    readonly HashSet<int> _collapsed = [];
    readonly Dictionary<int, HierarchyNode> _markNodes = [];
    IReadOnlyList<object>? _lastItems;

    /// <summary>Raised when a parent node is clicked or activated and collapses or expands (Carbon's <c>tree.onNodeClick</c>).</summary>
    public event EventHandler<CdsChartNodeEventArgs>? NodeClicked;

    /// <summary>A tidy tree or a dendrogram (Carbon's <c>tree.type</c>). Default <see cref="CdsTreeChartType.Tree"/>.</summary>
    public CdsTreeChartType TreeType
    {
        get => (CdsTreeChartType)GetValue(TreeTypeProperty);
        set => SetValue(TreeTypeProperty, value);
    }

    /// <summary>The root node's label (Carbon's <c>tree.rootTitle</c>). Default "Tree".</summary>
    public string? RootTitle
    {
        get => (string?)GetValue(RootTitleProperty);
        set => SetValue(RootTitleProperty, value);
    }

    private protected override string ChartKind => "Tree chart";

    private protected override bool DefaultLegendEnabled => false;

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) => [];

    private protected override bool IsEmpty(IReadOnlyList<object> items) => !items.OfType<CdsChartNode>().Any();

    /// <summary>Carbon's getLongestLabel: the name with the most characters anywhere in the data.</summary>
    internal static string LongestLabel(IEnumerable<CdsChartNode> data)
    {
        var longest = "";
        foreach (var d in data)
        {
            var inner = d.Children.Count > 0 ? LongestLabel(d.Children) : "";
            var name = d.Name ?? "";
            if (inner.Length > longest.Length || name.Length > longest.Length) longest = inner.Length > name.Length ? inner : name;
        }
        return longest;
    }

    /// <summary>Carbon's tree layout: the label widths and 6px per level come off the width; returns the root and left margin.</summary>
    internal static (HierarchyNode Root, float MarginLeft) Layout(IReadOnlyList<CdsChartNode> data, float width, float height, CdsTreeChartType type, string rootTitle)
    {
        var rootW = CdsChartTextMetrics.Width(rootTitle, ChartFont.Tick);
        var longW = CdsChartTextMetrics.Width(LongestLabel(data), ChartFont.Tick);
        var marginLeft = rootW > 0 ? rootW + NodeOffset : 30 - NodeOffset;
        var root = HierarchyNode.Build(rootTitle, data);
        var all = root.Descendants();
        var maxDepth = all[^1].Depth;
        var extent = Math.Max(1, width - longW - maxDepth * NodeOffset - rootW);
        if (type == CdsTreeChartType.Dendrogram) ChartTreeLayout.Cluster(root, height, extent);
        else ChartTreeLayout.Tree(root, height, extent);
        return (root, marginLeft);
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        if (!ReferenceEquals(_lastItems, ctx.Items))
        {
            _lastItems = ctx.Items;
            _collapsed.Clear();
        }
        var data = ctx.Items.OfType<CdsChartNode>().ToList();
        var (root, marginLeft) = Layout(data, ctx.Width, ctx.Height, TreeType, RootTitle ?? "Tree");

        // Visible nodes: every node whose ancestors are all expanded.
        var nodes = new List<HierarchyNode>();
        var links = new List<(HierarchyNode Source, HierarchyNode Target)>();
        void Visit(HierarchyNode d)
        {
            nodes.Add(d);
            if (d.Children is null || _collapsed.Contains(d.Index)) return;
            foreach (var c in d.Children)
            {
                links.Add((d, c));
                Visit(c);
            }
        }
        Visit(root);
        double top = root.X, bottom = root.X;
        foreach (var d in nodes)
        {
            top = Math.Min(top, d.X);
            bottom = Math.Max(bottom, d.X);
        }
        // Carbon's viewBox centres the visible rows vertically.
        var dy = (ctx.Height - (bottom - top)) / 2 - top;
        PointF At(HierarchyNode d) => new(marginLeft + (float)d.Y, (float)(dy + d.X));

        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = new RectF(0, 0, ctx.Width, ctx.Height) };
        foreach (var (s, t) in links)
            scene.Shapes.Add(new ChartPath
            {
                Path = ChartDraw.HorizontalLink(At(s), At(t)),
                Stroke = ChartPaint.OfKey(CdsColorKeys.BorderStrong01),
                StrokeOpacity = 0.4f,
                StrokeWidth = 1.5f,
                Hover = ChartHoverStyle.None,
            });

        _markNodes.Clear();
        var halo = ChartPaint.OfKey(CdsColorKeys.TextInverse);
        foreach (var d in Enumerable.Reverse(nodes))
        {
            var p = At(d);
            var hasKids = d.HasChildren;
            var clickable = d.Depth != 0 && hasKids;
            scene.Shapes.Add(new ChartCircle
            {
                Center = p,
                Radius = 2.5f,
                Fill = ChartPaint.OfKey(hasKids ? CdsColorKeys.TextSecondary : CdsColorKeys.BorderStrong01),
                HoverFill = clickable ? ChartPaint.OfKey(CdsColorKeys.TextPrimary) : default,
                Mark = d.Index,
                Hover = ChartHoverStyle.None,
            });
            var x = p.X + (hasKids ? -NodeOffset : NodeOffset);
            var anchor = hasKids ? ChartTextAnchor.End : ChartTextAnchor.Start;
            // Carbon's 2px text-inverse halo keeps labels readable over the links.
            foreach (var (ox, oy) in new[] { (-1f, 0f), (1f, 0f), (0f, -1f), (0f, 1f) })
                scene.Texts.Add(new ChartText { Text = d.Name, X = x + ox, Y = p.Y + oy, Anchor = anchor, Baseline = ChartTextBaseline.Middle, Color = halo });
            scene.Texts.Add(new ChartText { Text = d.Name, X = x, Y = p.Y, Anchor = anchor, Baseline = ChartTextBaseline.Middle, Color = ChartPaint.OfKey(CdsColorKeys.TextPrimary) });
        }

        // Marks in reading order (pre-order).
        foreach (var d in nodes)
        {
            var p = At(d);
            var hasKids = d.HasChildren;
            var clickable = d.Depth != 0 && hasKids;
            var w = CdsChartTextMetrics.Width(d.Name, ChartFont.Tick);
            var label = hasKids ? new RectF(p.X - NodeOffset - w, p.Y - 8, w, 16) : new RectF(p.X + NodeOffset, p.Y - 8, w, 16);
            var dot = new RectF(p.X - 7.5f, p.Y - 7.5f, 15, 15);
            _markNodes[d.Index] = d;
            var state = clickable ? (_collapsed.Contains(d.Index) ? ", collapsed" : ", expanded") : "";
            var parent = d.Parent is { } pa ? $", in {pa.Name}" : "";
            var count = hasKids ? $", {d.Children!.Count} {(d.Children.Count == 1 ? "child" : "children")}" : "";
            scene.Marks.Add(new ChartMark
            {
                Id = d.Index,
                Bounds = RectF.Union(label, dot),
                Anchor = p,
                Description = (d.Data?.Description ?? d.Name) + parent + count + state,
            });
        }
        scene.Summary = nodes.Count == 1 ? "1 node" : $"{nodes.Count} nodes";
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
        base.OnActivated();
    }

    /// <summary>Collapses or expands a parent below the root (Carbon's node click) and raises <see cref="NodeClicked"/>.</summary>
    internal void ActivateMark(int id)
    {
        if (!_markNodes.TryGetValue(id, out var d) || d.Depth == 0 || !d.HasChildren) return;
        if (!_collapsed.Remove(d.Index)) _collapsed.Add(d.Index);
        RequestRebuild();
        if (d.Data is { } data) NodeClicked?.Invoke(this, new CdsChartNodeEventArgs(data));
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var rows = new List<string[]>();
        void Walk(CdsChartNode d)
        {
            foreach (var c in d.Children)
            {
                Walk(c);
                rows.Add([c.Name ?? "", d.Name ?? ""]);
            }
        }
        foreach (var d in ctx.Items.OfType<CdsChartNode>())
        {
            Walk(d);
            rows.Add([d.Name ?? "", "–"]);
        }
        return new ChartTable(["Child", "Parent"], rows);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsTreeChart)b).RequestRebuild();
}
