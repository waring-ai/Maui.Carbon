namespace Maui.Carbon;

/// <summary>
/// Flows between columns of nodes, each band as wide as its value (Carbon's alluvial chart, a Sankey diagram). Put
/// <see cref="CdsAlluvialLink"/>s in <see cref="CdsChart.Data"/> and every node in <see cref="Nodes"/>; a node's
/// <see cref="CdsAlluvialNode.Category"/> heads its column. Nodes are 4px bars at least 24px apart, labelled "Name (value)";
/// links take their source's palette color at 80%. Always give <see cref="Units"/>. The graph must be acyclic. No legend,
/// as in Carbon. Use a table or bars for precise values or many small flows.
/// </summary>
public class CdsAlluvialChart : CdsChart
{
    /// <summary>Bindable property for <see cref="Nodes"/>.</summary>
    public static readonly BindableProperty NodesProperty = BindableProperty.Create(
        nameof(Nodes), typeof(IList<CdsAlluvialNode>), typeof(CdsAlluvialChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Units"/>.</summary>
    public static readonly BindableProperty UnitsProperty = BindableProperty.Create(
        nameof(Units), typeof(string), typeof(CdsAlluvialChart), "", propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="NodeAlignment"/>.</summary>
    public static readonly BindableProperty NodeAlignmentProperty = BindableProperty.Create(
        nameof(NodeAlignment), typeof(CdsAlluvialNodeAlignment), typeof(CdsAlluvialChart), CdsAlluvialNodeAlignment.Center, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="NodePadding"/>.</summary>
    public static readonly BindableProperty NodePaddingProperty = BindableProperty.Create(
        nameof(NodePadding), typeof(double), typeof(CdsAlluvialChart), 24d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="IsMonochrome"/>.</summary>
    public static readonly BindableProperty IsMonochromeProperty = BindableProperty.Create(
        nameof(IsMonochrome), typeof(bool), typeof(CdsAlluvialChart), false, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="IsGradientEnabled"/>.</summary>
    public static readonly BindableProperty IsGradientEnabledProperty = BindableProperty.Create(
        nameof(IsGradientEnabled), typeof(bool), typeof(CdsAlluvialChart), false, propertyChanged: Changed);

    // Carbon's fixed.alluvial values.
    internal const float NodeWidth = 4;
    internal const float NodeHoveredWidth = 8;
    internal const double MinNodePadding = 24;
    internal const float OpacityUnfocus = 0.3f;
    internal const float OpacityDefault = 0.8f;
    internal const float OpacitySelected = 1;
    static readonly ChartFont CategoryFont = new(ChartFontFamily.Condensed, 14);

    /// <summary>
    /// Every node (Carbon's <c>alluvial.nodes</c>): a name and its column's category. Nodes no link uses are not drawn.
    /// When empty, the nodes are taken from the links in order (without column headers).
    /// </summary>
    public IList<CdsAlluvialNode>? Nodes
    {
        get => (IList<CdsAlluvialNode>?)GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }

    /// <summary>The unit of the values ("leads", "TWh"), shown after every value (Carbon's <c>alluvial.units</c>). Default none.</summary>
    public string? Units
    {
        get => (string?)GetValue(UnitsProperty);
        set => SetValue(UnitsProperty, value);
    }

    /// <summary>How nodes are placed in columns (Carbon's <c>alluvial.nodeAlignment</c>). Default <see cref="CdsAlluvialNodeAlignment.Center"/>.</summary>
    public CdsAlluvialNodeAlignment NodeAlignment
    {
        get => (CdsAlluvialNodeAlignment)GetValue(NodeAlignmentProperty);
        set => SetValue(NodeAlignmentProperty, value);
    }

    /// <summary>The vertical gap between nodes of a column (Carbon's <c>alluvial.nodePadding</c>); never less than 24. Default 24.</summary>
    public double NodePadding
    {
        get => (double)GetValue(NodePaddingProperty);
        set => SetValue(NodePaddingProperty, value);
    }

    /// <summary>Whether every link takes the first palette color instead of its source's (Carbon's <c>alluvial.monochrome</c>). Default <c>false</c>.</summary>
    public bool IsMonochrome
    {
        get => (bool)GetValue(IsMonochromeProperty);
        set => SetValue(IsMonochromeProperty, value);
    }

    /// <summary>
    /// Whether links fade from their source's to their target's color (Carbon's <c>color.gradient.enabled</c>); needs
    /// <see cref="CdsChart.ColorScale"/> naming every node. Default <c>false</c>.
    /// </summary>
    public bool IsGradientEnabled
    {
        get => (bool)GetValue(IsGradientEnabledProperty);
        set => SetValue(IsGradientEnabledProperty, value);
    }

    private protected override string ChartKind => "Alluvial chart";

    private protected override bool DefaultLegendEnabled => false;

    private protected override bool IsEmpty(IReadOnlyList<object> items) => !items.OfType<CdsAlluvialLink>().Any();

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items)
    {
        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
            if (item is CdsAlluvialLink { Source: { } s } && seen.Add(s)) list.Add(s);
        return list;
    }

    /// <inheritdoc/>
    protected override void OnContextChanged()
    {
        base.OnContextChanged();
        // Gradient links blend the theme's colors at build time.
        if (IsGradientEnabled && Scene != ChartScene.Empty) RequestRebuild();
    }

    List<CdsAlluvialLink> ActiveLinks(ChartBuildContext ctx) =>
        ctx.Items.OfType<CdsAlluvialLink>().Where(l => l.Source is not null && l.Target is not null && ctx.IsActive(l.Source)).ToList();

    /// <summary>The d3-sankey graph as Carbon builds it: extent [[2, 30], [width - 2, height]], unused nodes dropped.</summary>
    internal (List<SankeyNode> Nodes, List<SankeyLink> Links) Graph(ChartBuildContext ctx)
    {
        var links = ActiveLinks(ctx);
        IEnumerable<(string, string?)> nodes = Nodes is { Count: > 0 } given
            ? given.Where(n => n.Name is not null).Select(n => (n.Name!, n.Category))
            : links.SelectMany(l => new[] { l.Source!, l.Target! }).Distinct(StringComparer.Ordinal).Select(n => (n, (string?)null));
        var sankey = new ChartSankey
        {
            X0 = 2,
            Y0 = 30,
            X1 = ctx.Width - 2,
            Y1 = ctx.Height,
            NodeWidth = NodeWidth,
            NodePadding = Math.Max(MinNodePadding, NodePadding),
            Align = NodeAlignment switch { CdsAlluvialNodeAlignment.Left => SankeyAlign.Left, CdsAlluvialNodeAlignment.Right => SankeyAlign.Right, _ => SankeyAlign.Justify },
        };
        var (ns, ls) = sankey.Layout(nodes, links.Select(l => (l.Source!, l.Target!, l.Value, (object?)l)));
        return (ns.Where(n => n.Value != 0).ToList(), ls);
    }

    string ValueText(ChartBuildContext ctx, double v)
    {
        var s = ctx.Value(v);
        if (string.IsNullOrEmpty(s)) s = "-";
        return string.IsNullOrEmpty(Units) ? s : s + " " + Units;
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = new RectF(0, 0, ctx.Width, ctx.Height) };
        List<SankeyNode> nodes;
        List<SankeyLink> links;
        try
        {
            (nodes, links) = Graph(ctx);
        }
        catch (InvalidOperationException ex)
        {
            // As in Carbon: a link to an unknown node or a cycle draws nothing.
            System.Diagnostics.Trace.TraceWarning("CdsAlluvialChart: " + ex.Message);
            scene.Summary = "No data";
            return scene;
        }

        // Column headers: one per column, left of the column where it fits.
        var columns = new SortedDictionary<double, string>();
        foreach (var n in nodes)
            if (!string.IsNullOrEmpty(n.Category)) columns[n.X0] = n.Category!;
        foreach (var (x, category) in columns)
        {
            var tw = CdsChartTextMetrics.Width(category, CategoryFont);
            scene.Texts.Add(new ChartText
            {
                Text = category,
                X = (float)(x >= tw ? x - tw + 4 : x),
                Y = ChartDraw.MiddleForBaseline(20, CategoryFont),
                Baseline = ChartTextBaseline.Middle,
                Font = CategoryFont,
                Color = ChartPaint.OfKey(CdsColorKeys.TextPrimary),
            });
        }

        var gradient = IsGradientEnabled && ColorScale is { Count: > 0 };
        var first = ctx.Groups.Count > 0 ? ctx.Groups[0] : "";
        ChartPaint Paint(SankeyLink l) => ChartPaint.OfSeries(IsMonochrome ? first : l.Source.Name);

        // Geometry of each link: the centre line of d3's sankeyLinkHorizontal and its band outline.
        var geometry = new Dictionary<SankeyLink, (PathF Path, PointF[] Band, float Width)>();
        foreach (var l in links)
        {
            var s = new PointF((float)l.Source.X1, (float)l.Y0);
            var t = new PointF((float)l.Target.X0, (float)l.Y1);
            var mx = (s.X + t.X) / 2;
            var width = (float)Math.Max(1, l.Width);
            var samples = ChartDraw.SampleCubic(s, new PointF(mx, s.Y), new PointF(mx, t.Y), t, 32);
            geometry[l] = (ChartDraw.HorizontalLink(s, t), ChartDraw.StrokePolygon(samples, width), width);
        }

        // Marks: the links (in data order), then the nodes column by column, so a node's label wins over the links under it.
        var markOfLink = new Dictionary<SankeyLink, int>();
        var id = 0;
        foreach (var l in links) markOfLink[l] = id++;
        var ordered = nodes.OrderBy(n => n.X0).ThenBy(n => n.Y0).ToList();
        var markOfNode = new Dictionary<SankeyNode, int>();
        foreach (var n in ordered) markOfNode[n] = id++;

        void AddLink(SankeyLink l, float opacity, bool hoverOnly, int mark)
        {
            var (path, band, width) = geometry[l];
            if (gradient && band.Length >= 4)
            {
                // The whole band in one fill, so it has no seams; Carbon's gradient spans the whole graph (userSpaceOnUse,
                // 0% to 100% of the width) from the source's color to the target's.
                var p = new PathF();
                p.MoveTo(band[0]);
                for (var i = 1; i < band.Length; i++) p.LineTo(band[i]);
                p.Close();
                var y = band[0].Y;
                scene.Shapes.Add(new ChartPath
                {
                    Path = p,
                    FillGradient = new ChartGradient(new PointF(0, y), new PointF(Math.Max(1, ctx.Width), y),
                        [new ChartGradientStop(0, ChartPaint.OfSeries(l.Source.Name)), new ChartGradientStop(1, ChartPaint.OfSeries(l.Target.Name))]),
                    FillOpacity = opacity,
                    HoverOnly = hoverOnly,
                    Mark = mark,
                    Hover = ChartHoverStyle.None,
                });
                return;
            }
            scene.Shapes.Add(new ChartPath
            {
                Path = path, Stroke = Paint(l), StrokeOpacity = opacity, StrokeWidth = width, HoverOnly = hoverOnly, Mark = mark, Hover = ChartHoverStyle.None,
            });
        }

        foreach (var l in links) AddLink(l, OpacityDefault, false, markOfLink[l]);
        // Hovering a link raises it to 100%; hovering a node raises every path through it.
        foreach (var l in links) AddLink(l, OpacitySelected, true, markOfLink[l]);
        foreach (var n in ordered)
            foreach (var l in Connected(n)) AddLink(l, OpacitySelected, true, markOfNode[n]);

        var inverse = ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute);
        foreach (var n in ordered)
        {
            var mark = markOfNode[n];
            var rect = new RectF((float)n.X0, (float)n.Y0, (float)(n.X1 - n.X0), (float)Math.Max(0, n.Y1 - n.Y0));
            scene.Shapes.Add(new ChartRect { Rect = rect, Fill = inverse, Mark = mark, Hover = ChartHoverStyle.None });
            scene.Shapes.Add(new ChartRect { Rect = new RectF(rect.X - 2, rect.Y, NodeHoveredWidth, rect.Height), Fill = inverse, HoverOnly = true, Mark = mark, Hover = ChartHoverStyle.None });
            var label = $"{n.Name} ({ctx.Number(n.Value)})";
            var tw = CdsChartTextMetrics.Width(label, ChartFont.Tick);
            var tx = (float)(n.X1 - n.X0);
            if (n.X1 >= tw) tx -= tw + 16;
            else tx += 4;
            var box = new RectF(rect.X + tx, rect.Y + rect.Height / 2 - 9, tw + 8, 18);
            scene.Shapes.Add(new ChartRect { Rect = box, Fill = inverse, Mark = mark, Hover = ChartHoverStyle.None });
            scene.Texts.Add(new ChartText
            {
                Text = label,
                X = box.X + 4,
                Y = box.Y + ChartDraw.MiddleForBaseline(13, ChartFont.Tick),
                Baseline = ChartTextBaseline.Middle,
                Color = ChartPaint.OfToken(CdsChartToken.Layer01Absolute),
            });
            n.Box = box;
        }

        foreach (var l in links)
        {
            var (_, band, _) = geometry[l];
            var hit = band.Length >= 4 ? band : [];
            var color = gradient ? ChartPaint.OfSeries(l.Source.Name) : Paint(l);
            scene.Marks.Add(new ChartMark
            {
                Id = markOfLink[l],
                Polygon = hit,
                Bounds = ChartDraw.Bounds(hit),
                Anchor = new PointF((float)((l.Source.X1 + l.Target.X0) / 2), (float)((l.Y0 + l.Y1) / 2)),
                Group = l.Source.Name,
                Rows = [new ChartTooltipRow(l.Target.Name + " →", ValueText(ctx, l.Value), color)],
                Description = $"{l.Source.Name} → {l.Target.Name} ({ValueText(ctx, l.Value)})",
            });
        }
        foreach (var n in ordered)
        {
            var rect = new RectF((float)n.X0 - 2, (float)n.Y0, NodeHoveredWidth, (float)Math.Max(0, n.Y1 - n.Y0));
            var box = n.Box;
            // The node and its label box, joined across the gap between them.
            PointF[] poly = box.X < rect.X
                ? [new(box.Left, box.Top), new(rect.Left, box.Top), new(rect.Left, rect.Top), new(rect.Right, rect.Top), new(rect.Right, rect.Bottom), new(rect.Left, rect.Bottom), new(rect.Left, box.Bottom), new(box.Left, box.Bottom)]
                : [new(rect.Left, rect.Top), new(rect.Right, rect.Top), new(rect.Right, box.Top), new(box.Right, box.Top), new(box.Right, box.Bottom), new(rect.Right, box.Bottom), new(rect.Right, rect.Bottom), new(rect.Left, rect.Bottom)];
            scene.Marks.Add(new ChartMark
            {
                Id = markOfNode[n],
                Polygon = poly,
                Bounds = RectF.Union(rect, box),
                Anchor = new PointF(box.Center.X, box.Center.Y),
                Description = $"{n.Name} ({ValueText(ctx, n.Value)})" + (string.IsNullOrEmpty(n.Category) ? "" : $", {n.Category}"),
            });
        }
        scene.Summary = links.Count == 1 ? "1 flow" : $"{links.Count} flows";
        return scene;
    }

    /// <summary>Every link on a path through the node, downstream and upstream (Carbon's traverse).</summary>
    internal static List<SankeyLink> Connected(SankeyNode node)
    {
        var list = new List<SankeyLink>();
        var seen = new HashSet<SankeyLink>();
        void Down(SankeyNode n)
        {
            foreach (var l in n.SourceLinks)
                if (seen.Add(l))
                {
                    list.Add(l);
                    Down(l.Target);
                }
        }
        void Up(SankeyNode n)
        {
            foreach (var l in n.TargetLinks)
                if (seen.Add(l))
                {
                    list.Add(l);
                    Up(l.Source);
                }
        }
        Down(node);
        Up(node);
        return list;
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var culture = ctx.Culture;
        var rows = ActiveLinks(ctx)
            .Select((l, i) => (l, i))
            .OrderBy(x => x.l.Source, StringComparer.Create(culture, false))
            .ThenBy(x => x.i)
            .Select(x => new[] { x.l.Source!, x.l.Target!, ctx.Value(x.l.Value) })
            .ToList();
        return new ChartTable(["Source", "Target", "Value"], rows);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsAlluvialChart)b).RequestRebuild();
}
