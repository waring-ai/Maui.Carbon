namespace Maui.Carbon;

/// <summary>
/// A node-and-edge diagram drawn from nodes the app has placed (Carbon's diagram building blocks: card and shape nodes,
/// edges with variants and markers). Put <see cref="CdsNetworkNode"/>s in <see cref="CdsChart.Data"/> (their X and Y are
/// centres) and <see cref="CdsNetworkEdge"/>s in <see cref="Edges"/>; there is no automatic layout, as in Carbon. Straight
/// edges end at node outlines, elbow and bezier edges at the facing sides, and end markers touch the node. Describe the same
/// relationships in text or a table near the diagram for screen-reader users. Use a tree chart for pure hierarchies and an
/// alluvial chart for flows with quantities.
/// </summary>
public class CdsNetworkDiagram : CdsChart
{
    /// <summary>Bindable property for <see cref="Edges"/>.</summary>
    public static readonly BindableProperty EdgesProperty = BindableProperty.Create(
        nameof(Edges), typeof(IList<CdsNetworkEdge>), typeof(CdsNetworkDiagram), null, propertyChanged: static (b, _, _) => ((CdsNetworkDiagram)b).RequestRebuild());

    /// <summary>A card's width (Carbon's NetworkDiagram default).</summary>
    internal const double CardWidth = 216;

    /// <summary>The room for a shape node's text below it.</summary>
    internal const double ShapeBody = 56;

    /// <summary>The smallest text size a scaled-down diagram keeps (the charts' 12px minimum).</summary>
    internal const float MinTextSize = 12;

    /// <summary>The offsets of a stacked card's outlines, up and to the right.</summary>
    static readonly float[] StackOffsets = [5f, 11f];

    static readonly ChartFont Title14 = new(ChartFontFamily.SansSemiBold, 14);
    static readonly ChartFont Body14 = new(ChartFontFamily.Sans, 14);

    readonly Dictionary<int, CdsNetworkNode> _markNodes = [];

    /// <summary>Raised when a node with <see cref="CdsNetworkNode.IsClickable"/> is clicked or activated with Enter (Carbon's card <c>onClick</c>).</summary>
    public event EventHandler<CdsNetworkNodeEventArgs>? NodeClicked;

    /// <summary>The connections between nodes. The diagram redraws when the list is replaced, not when it is changed in place.</summary>
    public IList<CdsNetworkEdge>? Edges
    {
        get => (IList<CdsNetworkEdge>?)GetValue(EdgesProperty);
        set => SetValue(EdgesProperty, value);
    }

    private protected override string ChartKind => "Network diagram";

    private protected override bool DefaultLegendEnabled => false;

    private protected override bool DefaultToolbarEnabled => false;

    private protected override ChartSkeleton Skeleton => ChartSkeleton.None;

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) => [];

    private protected override bool IsEmpty(IReadOnlyList<object> items) => !items.OfType<CdsNetworkNode>().Any() && (Edges?.Count ?? 0) == 0;

    /// <summary>The content's size: the nodes' extent plus 16 (Carbon's NetworkDiagram svg size).</summary>
    internal Size ContentSize()
    {
        var (_, _, maxX, maxY) = ContentBounds();
        return new Size(Math.Ceiling(maxX + 16), Math.Ceiling(maxY + 16));
    }

    /// <summary>The extent of the nodes (with a shape node's text below it) and free edge ends.</summary>
    (double MinX, double MinY, double MaxX, double MaxY) ContentBounds()
    {
        double minX = double.MaxValue, minY = double.MaxValue, maxX = 0, maxY = 0;
        void Add(double x0, double y0, double x1, double y1)
        {
            minX = Math.Min(minX, x0);
            minY = Math.Min(minY, y0);
            maxX = Math.Max(maxX, x1);
            maxY = Math.Max(maxY, y1);
        }
        if (Data is not null)
            foreach (var n in Data.OfType<CdsNetworkNode>())
            {
                var b = Box(n);
                var stack = n.Kind == CdsNetworkNodeKind.Card && n.IsStacked ? StackOffsets[^1] : 0;
                Add(b.X - b.W / 2, b.Y - b.H / 2 - stack, b.X + b.W / 2 + stack, b.Y + b.H / 2 + (n.Kind == CdsNetworkNodeKind.Shape ? ShapeBody : 0));
            }
        if (Edges is not null)
            foreach (var e in Edges)
                foreach (var p in new[] { e.Start, e.End })
                    if (p is { } q) Add(q.X, q.Y, q.X, q.Y);
        return minX == double.MaxValue ? (0, 0, 0, 0) : (Math.Max(0, minX), Math.Max(0, minY), maxX, maxY);
    }

    private protected override double DefaultHeight => ContentSize().Height + (string.IsNullOrEmpty(Title) ? 0 : 48);

    internal readonly record struct NodeBox(double X, double Y, double W, double H, bool Round, double Stack = 0)
    {
        public RectF Rect => new((float)(X - W / 2), (float)(Y - H / 2), (float)W, (float)H);

        public double Left => X - W / 2;

        public double Right => X + W / 2 + Stack;

        public double Top => Y - H / 2 - Stack;

        public double Bottom => Y + H / 2;

        /// <summary>The outline edges end at (with a stack's outlines), as a centred box.</summary>
        public NodeBox Outline => Stack == 0 ? this : new NodeBox((Left + Right) / 2, (Top + Bottom) / 2, Right - Left, Bottom - Top, Round);
    }

    /// <summary>Card height from its content: 16px padding top and bottom, 18px per title or subtitle line, 40 for a label.</summary>
    internal static double CardHeight(CdsNetworkNode n) =>
        n.Height ?? Math.Max(64, 32 + (string.IsNullOrEmpty(n.Title) ? 0 : 18) + (string.IsNullOrEmpty(n.Subtitle) ? 0 : 18) + (string.IsNullOrEmpty(n.Label) ? 0 : 40));

    internal static NodeBox Box(CdsNetworkNode n) => n.Kind == CdsNetworkNodeKind.Shape
        ? new NodeBox(n.X, n.Y, n.Size, n.Size, n.Shape == CdsNetworkNodeShape.Circle)
        : new NodeBox(n.X, n.Y, n.Width, CardHeight(n), false);

    /// <summary>The box edges end at: a stacked card's top and right sides move out to its outermost outline.</summary>
    internal static NodeBox EdgeBox(CdsNetworkNode n) =>
        n.Kind == CdsNetworkNodeKind.Card && n.IsStacked ? Box(n) with { Stack = StackOffsets[^1] } : Box(n);

    /// <summary>Where the line from the centre of <paramref name="b"/> towards <paramref name="p"/> leaves its outline.</summary>
    internal static PointF Boundary(NodeBox b, PointF p)
    {
        b = b.Outline;
        var dx = p.X - b.X;
        var dy = p.Y - b.Y;
        if (dx == 0 && dy == 0) return new PointF((float)b.X, (float)b.Y);
        if (b.Round)
        {
            var r = b.W / 2;
            var l = Math.Sqrt(dx * dx + dy * dy);
            return new PointF((float)(b.X + dx / l * r), (float)(b.Y + dy / l * r));
        }
        var sx = dx != 0 ? b.W / 2 / Math.Abs(dx) : double.PositiveInfinity;
        var sy = dy != 0 ? b.H / 2 / Math.Abs(dy) : double.PositiveInfinity;
        var s = Math.Min(sx, sy);
        return new PointF((float)(b.X + dx * s), (float)(b.Y + dy * s));
    }

    /// <summary>The facing sides' centres (left and right, or top and bottom) for elbow and bezier edges.</summary>
    internal static (bool Horizontal, PointF S, PointF T) Ports(NodeBox a, NodeBox b)
    {
        if (Math.Abs(b.X - a.X) >= Math.Abs(b.Y - a.Y))
        {
            var right = b.X >= a.X;
            return (true, new PointF((float)(right ? a.Right : a.Left), (float)a.Y), new PointF((float)(right ? b.Left : b.Right), (float)b.Y));
        }
        var down = b.Y >= a.Y;
        return (false, new PointF((float)a.X, (float)(down ? a.Bottom : a.Top)), new PointF((float)b.X, (float)(down ? b.Top : b.Bottom)));
    }

    /// <summary>An edge's geometry: its path, points along it (for markers and hit areas).</summary>
    internal static (PathF Path, List<PointF> Points) EdgeGeometry(CdsNetworkEdge e, NodeBox? a, NodeBox? b)
    {
        PointF s, t;
        var path = new PathF();
        if (a is { } A && b is { } B && e.Path != CdsNetworkEdgePath.Straight)
        {
            var (horizontal, ps, pt) = Ports(A, B);
            if (e.Path == CdsNetworkEdgePath.Elbow)
            {
                var pct = (float)e.BendPercent;
                List<PointF> pts = horizontal
                    ? [ps, new(ps.X + (pt.X - ps.X) * pct, ps.Y), new(ps.X + (pt.X - ps.X) * pct, pt.Y), pt]
                    : [ps, new(ps.X, ps.Y + (pt.Y - ps.Y) * pct), new(pt.X, ps.Y + (pt.Y - ps.Y) * pct), pt];
                path.MoveTo(pts[0]);
                for (var i = 1; i < pts.Count; i++) path.LineTo(pts[i]);
                return (path, pts);
            }
            const float k = 0.5f;
            var c1 = horizontal ? new PointF(ps.X + (pt.X - ps.X) * k, ps.Y) : new PointF(ps.X, ps.Y + (pt.Y - ps.Y) * k);
            var c2 = horizontal ? new PointF(pt.X - (pt.X - ps.X) * k, pt.Y) : new PointF(pt.X, pt.Y - (pt.Y - ps.Y) * k);
            path.MoveTo(ps);
            path.CurveTo(c1, c2, pt);
            return (path, ChartDraw.SampleCubic(ps, c1, c2, pt, 24));
        }
        if (a is { } A2 && b is { } B2)
        {
            s = Boundary(A2, new PointF((float)B2.X, (float)B2.Y));
            t = Boundary(B2, new PointF((float)A2.X, (float)A2.Y));
        }
        else
        {
            s = a is { } A3 ? new PointF((float)A3.X, (float)A3.Y) : e.Start is { } p0 ? new PointF((float)p0.X, (float)p0.Y) : PointF.Zero;
            t = b is { } B3 ? new PointF((float)B3.X, (float)B3.Y) : e.End is { } p1 ? new PointF((float)p1.X, (float)p1.Y) : PointF.Zero;
        }
        path.MoveTo(s);
        path.LineTo(t);
        return (path, [s, t]);
    }

    /// <summary>Carbon's marker definitions: the outline (or circle), and the marker box.</summary>
    internal static (PointF[]? Polygon, PointF Center, float Radius, float Width, float Height) MarkerShape(CdsNetworkMarker m) => m switch
    {
        CdsNetworkMarker.ArrowLeft => ([new(7, 0), new(7, 8), new(0, 4)], default, 0, 7.5f, 8),
        CdsNetworkMarker.ArrowRight => ([new(0, 0), new(0, 8), new(7, 4)], default, 0, 7.5f, 8),
        CdsNetworkMarker.Circle => (null, new PointF(5.32f, 4.763f), 4.75f, 10, 10),
        CdsNetworkMarker.Diamond => ([new(5.035f, 0), new(10.035f, 5), new(5.035f, 10), new(0.035f, 5)], default, 0, 10, 10),
        CdsNetworkMarker.Square => ([new(0, 0.45f), new(8, 0.45f), new(8, 8.45f), new(0, 8.45f)], default, 0, 8, 9),
        _ => ([new(0, 0), new(1.5f, 0), new(1.5f, 8), new(0, 8)], default, 0, 2, 8),
    };

    /// <summary>
    /// A marker placed at <paramref name="at"/> along <paramref name="angle"/> (SVG <c>orient="auto"</c>) with its
    /// reference point <paramref name="refX"/>, half its height, on the vertex.
    /// </summary>
    internal static ChartShape Marker(CdsNetworkMarker m, PointF at, float angle, float refX, int mark)
    {
        var (poly, center, radius, _, h) = MarkerShape(m);
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        PointF Map(PointF p)
        {
            var x = p.X - refX;
            var y = p.Y - h / 2;
            return new PointF(at.X + x * cos - y * sin, at.Y + x * sin + y * cos);
        }
        var fill = ChartPaint.OfKey(CdsColorKeys.BorderInverse);
        if (poly is null) return new ChartCircle { Center = Map(center), Radius = radius, Fill = fill, Mark = mark, Hover = ChartHoverStyle.None };
        var path = new PathF();
        path.MoveTo(Map(poly[0]));
        for (var i = 1; i < poly.Length; i++) path.LineTo(Map(poly[i]));
        path.Close();
        return new ChartPath { Path = path, Fill = fill, Mark = mark, Hover = ChartHoverStyle.None };
    }

    static float[]? DashOf(CdsNetworkEdgeVariant v) => v switch
    {
        CdsNetworkEdgeVariant.DashSm => [2, 4],
        CdsNetworkEdgeVariant.DashMd => [4, 4],
        CdsNetworkEdgeVariant.DashLg => [8, 4],
        CdsNetworkEdgeVariant.DashXl => [16, 4],
        _ => null,
    };

    static string KeyOr(string? key, string fallback) => key is { Length: > 0 } && CdsPalettes.IsColorKey(key) ? key : fallback;

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = new RectF(0, 0, ctx.Width, ctx.Height) };
        var nodes = ctx.Items.OfType<CdsNetworkNode>().ToList();
        var byId = new Dictionary<string, CdsNetworkNode>(StringComparer.Ordinal);
        foreach (var n in nodes)
            if (n.Id is { } nid) byId.TryAdd(nid, n);
        var edges = Edges?.Where(e => e is not null).ToList() ?? [];
        var nodeMarks = nodes.Select((n, i) => (n, i)).ToDictionary(x => x.n, x => x.i);
        var id = nodes.Count;
        _markNodes.Clear();
        var edgeMarks = new List<ChartMark>();
        // Each text hangs from a point of its node, so a scaled-down diagram can keep its text readable.
        var pivots = new Dictionary<ChartText, PointF>(ReferenceEqualityComparer.Instance);
        // A card's text stays inside its card (cut with an ellipsis, lines that do not fit dropped); a shape node's text
        // sits on the diagram surface and gets a halo of that surface, so edges passing under it never cross the glyphs.
        var clips = new Dictionary<ChartText, (RectF Box, bool First)>(ReferenceEqualityComparer.Instance);
        var haloed = new HashSet<ChartText>(ReferenceEqualityComparer.Instance);
        void AddText(ChartText t, PointF pivot)
        {
            scene.Texts.Add(t);
            pivots[t] = pivot;
        }
        void AddCardText(ChartText t, PointF pivot, RectF box, bool first)
        {
            AddText(t, pivot);
            clips[t] = (box, first);
        }
        void AddShapeText(ChartText t, PointF pivot)
        {
            AddText(t, pivot);
            haloed.Add(t);
        }

        foreach (var e in edges)
        {
            CdsNetworkNode? a = e.Source is { } sid && byId.TryGetValue(sid, out var na) ? na : null;
            CdsNetworkNode? b = e.Target is { } tid && byId.TryGetValue(tid, out var nb) ? nb : null;
            if ((a is null && e.Start is null) || (b is null && e.End is null)) continue;
            var (path, pts) = EdgeGeometry(e, a is null ? null : EdgeBox(a), b is null ? null : EdgeBox(b));
            if (pts.Count < 2) continue;
            var mark = id++;
            if (e.Variant == CdsNetworkEdgeVariant.Tunnel)
                scene.Shapes.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfKey(CdsColorKeys.LayerAccent01), StrokeWidth = 6, Mark = mark, Hover = ChartHoverStyle.None });
            else if (e.Variant == CdsNetworkEdgeVariant.Double)
                scene.Shapes.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfKey(CdsColorKeys.BorderInverse), StrokeWidth = 4.5f, Mark = mark, Hover = ChartHoverStyle.None });
            var inner = e.Variant == CdsNetworkEdgeVariant.Double ? CdsColorKeys.Background : KeyOr(e.Color, CdsColorKeys.BorderStrong01);
            scene.Shapes.Add(new ChartPath
            {
                Path = path, Stroke = ChartPaint.OfKey(inner), StrokeWidth = 1, HoverStrokeWidth = 2.5f, Dash = DashOf(e.Variant), Mark = mark, Hover = ChartHoverStyle.None,
            });
            // Between nodes the start marker begins at the edge start and the end marker's tip touches the node; free
            // edges centre their markers on the ends, as Carbon's building-block markers do.
            var free = a is null || b is null;
            if (e.MarkerStart != CdsNetworkMarker.None)
            {
                var w = MarkerShape(e.MarkerStart).Width;
                var angle = MathF.Atan2(pts[1].Y - pts[0].Y, pts[1].X - pts[0].X);
                scene.Shapes.Add(Marker(e.MarkerStart, pts[0], angle, free ? w / 2 + 0.5f : 0.5f, mark));
            }
            if (e.MarkerEnd != CdsNetworkMarker.None)
            {
                var w = MarkerShape(e.MarkerEnd).Width;
                var angle = MathF.Atan2(pts[^1].Y - pts[^2].Y, pts[^1].X - pts[^2].X);
                scene.Shapes.Add(Marker(e.MarkerEnd, pts[^1], angle, free ? w / 2 + 0.5f : w, mark));
            }
            var hit = ChartDraw.StrokePolygon(pts, 24);
            var from = a?.Title ?? a?.Id ?? "Start";
            var to = b?.Title ?? b?.Id ?? "End";
            edgeMarks.Add(new ChartMark
            {
                Id = mark,
                Polygon = hit,
                Bounds = ChartDraw.Bounds(hit),
                Anchor = pts[pts.Count / 2],
                Title = string.IsNullOrEmpty(e.Label) ? null : e.Label,
                Description = $"{from} to {to}" + (string.IsNullOrEmpty(e.Label) ? "" : ": " + e.Label),
            });
        }

        foreach (var n in nodes)
        {
            var mark = nodeMarks[n];
            var box = Box(n);
            var r = box.Rect;
            var clickable = n.IsClickable;
            var hover = clickable ? ChartPaint.OfKey(CdsColorKeys.LayerHover01) : default;
            var primary = ChartPaint.OfKey(CdsColorKeys.TextPrimary);
            if (n.Kind == CdsNetworkNodeKind.Card)
            {
                if (n.IsStacked)
                    foreach (var o in StackOffsets)
                    {
                        var accent = ChartPaint.OfKey(CdsColorKeys.LayerAccent01);
                        scene.Shapes.Add(new ChartRect { Rect = new RectF(r.X + o, r.Y - o, r.Width, 2), Fill = accent, Hover = ChartHoverStyle.None });
                        scene.Shapes.Add(new ChartRect { Rect = new RectF(r.Right + o - 2, r.Y - o, 2, r.Height), Fill = accent, Hover = ChartHoverStyle.None });
                    }
                scene.Shapes.Add(new ChartRect { Rect = r, Fill = ChartPaint.OfKey(CdsColorKeys.Layer01), HoverFill = hover, Mark = mark, Hover = ChartHoverStyle.None });
                scene.Shapes.Add(new ChartRect { Rect = new RectF(r.X, r.Y, 4, r.Height), Fill = ChartPaint.OfKey(KeyOr(n.Color, CdsColorKeys.BorderInverse)), Mark = mark, Hover = ChartHoverStyle.None });
                if (clickable)
                    scene.Shapes.Add(new ChartRect { Rect = new RectF(r.X + 1, r.Y + 1, r.Width - 2, r.Height - 2), Stroke = ChartPaint.OfKey(CdsColorKeys.Focus), StrokeWidth = 2, HoverOnly = true, Mark = mark, Hover = ChartHoverStyle.None });
                var x = r.X + 20;
                var y = r.Y + 16;
                var cardPivot = new PointF(r.X, r.Y);
                // The text column ends at the card's 16px end padding, or before the icon.
                var textBox = new RectF(r.X, r.Y, r.Width - 16 - (n.Icon is null ? 0 : 20 + 8), r.Height);
                var first = true;
                if (!string.IsNullOrEmpty(n.Title))
                {
                    AddCardText(new ChartText { Text = n.Title, X = x, Y = y, Font = Title14, Color = primary }, cardPivot, textBox, first);
                    first = false;
                    y += 18;
                }
                if (!string.IsNullOrEmpty(n.Subtitle))
                {
                    AddCardText(new ChartText { Text = n.Subtitle, X = x, Y = y, Font = Body14, Color = primary }, cardPivot, textBox, first);
                    first = false;
                    y += 18;
                }
                if (!string.IsNullOrEmpty(n.Label))
                    AddCardText(new ChartText { Text = n.Label, X = x, Y = y + 24, Font = ChartFont.Sans12, Color = ChartPaint.OfKey(CdsColorKeys.TextSecondary) }, cardPivot, textBox, first);
                if (n.Icon is { } icon)
                    scene.Shapes.AddRange(ChartDraw.Icon(icon, new PointF(r.Right - 16 - 20, r.Y + 16), 20, primary, mark));
            }
            else
            {
                var fill = ChartPaint.OfKey(CdsColorKeys.Layer01);
                if (n.Shape == CdsNetworkNodeShape.Circle)
                {
                    scene.Shapes.Add(new ChartCircle { Center = new PointF((float)box.X, (float)box.Y), Radius = r.Width / 2, Fill = fill, HoverFill = hover, Mark = mark, Hover = ChartHoverStyle.None });
                    if (clickable)
                        scene.Shapes.Add(new ChartCircle { Center = new PointF((float)box.X, (float)box.Y), Radius = r.Width / 2 + 1, Stroke = ChartPaint.OfKey(CdsColorKeys.Focus), StrokeWidth = 2, HoverOnly = true, Mark = mark, Hover = ChartHoverStyle.None });
                }
                else
                {
                    var corner = n.Shape == CdsNetworkNodeShape.RoundedSquare ? 8f : 0f;
                    scene.Shapes.Add(new ChartRect { Rect = r, CornerRadius = corner, Fill = fill, HoverFill = hover, Mark = mark, Hover = ChartHoverStyle.None });
                    if (clickable)
                        scene.Shapes.Add(new ChartRect { Rect = r.Inflate(1, 1), CornerRadius = corner, Stroke = ChartPaint.OfKey(CdsColorKeys.Focus), StrokeWidth = 2, HoverOnly = true, Mark = mark, Hover = ChartHoverStyle.None });
                }
                if (n.Icon is { } icon)
                    scene.Shapes.AddRange(ChartDraw.Icon(icon, new PointF((float)box.X - 10, (float)box.Y - 10), 20, primary, mark));
                var cx = (float)box.X;
                var y = r.Bottom + 2 + 2;
                var shapePivot = new PointF(cx, r.Bottom);
                if (!string.IsNullOrEmpty(n.Title))
                {
                    AddShapeText(new ChartText { Text = n.Title, X = cx, Y = y, Anchor = ChartTextAnchor.Middle, Font = Body14, Color = primary }, shapePivot);
                    y += 19;
                }
                if (!string.IsNullOrEmpty(n.Subtitle))
                {
                    AddShapeText(new ChartText { Text = n.Subtitle, X = cx, Y = y, Anchor = ChartTextAnchor.Middle, Font = Body14, Color = primary }, shapePivot);
                    y += 20;
                }
                if (!string.IsNullOrEmpty(n.Description))
                    AddShapeText(new ChartText { Text = n.Description, X = cx, Y = y, Anchor = ChartTextAnchor.Middle, Font = ChartFont.Sans12, Color = ChartPaint.OfKey(CdsColorKeys.TextSecondary) }, shapePivot);
            }
            _markNodes[mark] = n;
            var parts = new[] { n.Title ?? n.Id, n.Subtitle, n.Kind == CdsNetworkNodeKind.Card ? n.Label : n.Description }.Where(s => !string.IsNullOrEmpty(s));
            var bounds = n.Kind == CdsNetworkNodeKind.Shape ? new RectF(r.X, r.Y, r.Width, r.Height + 2) : r;
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = bounds,
                Radius = n.Kind == CdsNetworkNodeKind.Shape && n.Shape == CdsNetworkNodeShape.Circle ? r.Width / 2 : null,
                Anchor = new PointF((float)box.X, (float)box.Y),
                Description = string.Join(", ", parts) + (clickable ? ", button" : ""),
            });
        }
        // Reading order: the nodes (in data order), then the edges.
        scene.Marks.AddRange(edgeMarks);
        // Carbon's diagram scrolls when it is wider than its holder; a chart has no scroller, so it scales down to fit,
        // centred, with its text kept at 12px or more.
        var (minX, minY, maxX, maxY) = ContentBounds();
        var k = (float)Math.Min(1, Math.Min(ctx.Width / Math.Max(1, maxX), ctx.Height / Math.Max(1, maxY)));
        var shift = PointF.Zero;
        var sources = scene.Texts;
        if (k < 0.999f)
        {
            shift = new PointF((float)Math.Max(0, (ctx.Width - (minX + maxX) * k) / 2), (float)Math.Max(0, (ctx.Height - (minY + maxY) * k) / 2));
            scene = Scaled(scene, k, shift, pivots);
        }
        else
            k = 1;
        FitTexts(scene.Texts, sources, k, shift, clips, haloed);
        scene.Summary = $"{nodes.Count} {(nodes.Count == 1 ? "node" : "nodes")}, {edgeMarks.Count} {(edgeMarks.Count == 1 ? "edge" : "edges")}";
        return scene;
    }

    /// <summary>
    /// Fits the drawn <paramref name="texts"/> (one per <paramref name="sources"/> text, scaled by <paramref name="k"/> and moved by
    /// <paramref name="shift"/>): card text is cut with an ellipsis at its card's text column and lines below the card are
    /// dropped (a card keeps its first line); shape node text gets a halo of the surface the diagram sits on.
    /// </summary>
    static void FitTexts(List<ChartText> texts, List<ChartText> sources, float k, PointF shift, Dictionary<ChartText, (RectF Box, bool First)> clips, HashSet<ChartText> haloed)
    {
        var fitted = new List<ChartText>(texts.Count);
        var halo = ChartPaint.OfKey(CdsColorKeys.LayerBackground01);
        for (var i = 0; i < texts.Count; i++)
        {
            var t = texts[i];
            var source = sources[i];
            if (clips.TryGetValue(source, out var clip))
            {
                var box = new RectF(clip.Box.X * k + shift.X, clip.Box.Y * k + shift.Y, clip.Box.Width * k, clip.Box.Height * k);
                if (!clip.First && t.Y + t.Font.LineHeight > box.Bottom + 0.5f)
                    continue;
                var text = Ellipsize(t.Text, t.Font, box.Right - t.X);
                if (text != t.Text)
                    t = new ChartText
                    {
                        Text = text, X = t.X, Y = t.Y, Font = t.Font, Color = t.Color, Anchor = t.Anchor, Baseline = t.Baseline, Rotation = t.Rotation,
                        Opacity = t.Opacity, Group = t.Group, FadesWithLegend = t.FadesWithLegend,
                    };
            }
            if (haloed.Contains(source))
                ChartTextHalo.Add(fitted, t, halo);
            else
                fitted.Add(t);
        }
        texts.Clear();
        texts.AddRange(fitted);
    }

    /// <summary><paramref name="text"/> cut at the end with an ellipsis so it is at most <paramref name="width"/> wide.</summary>
    internal static string Ellipsize(string text, ChartFont font, float width)
    {
        if (CdsChartTextMetrics.Width(text, font) <= width + 0.5f) return text;
        for (var n = text.Length - 1; n > 0; n--)
        {
            var cut = text[..n].TrimEnd() + "…";
            if (CdsChartTextMetrics.Width(cut, font) <= width + 0.5f) return cut;
        }
        return "…";
    }

    /// <summary>
    /// The scene scaled by <paramref name="k"/> about the origin, then moved by <paramref name="offset"/>: geometry, stroke
    /// widths and hit areas. Text shrinks no further than <see cref="MinTextSize"/> (smaller text keeps its size); each
    /// text keeps its offset from its node's <paramref name="pivots"/> point at its block's text scale, so lines under a
    /// node never overlap.
    /// </summary>
    internal static ChartScene Scaled(ChartScene source, float k, PointF offset = default, IReadOnlyDictionary<ChartText, PointF>? pivots = null)
    {
        var scene = new ChartScene { Width = source.Width, Height = source.Height, Plot = source.Plot, HitMode = source.HitMode };
        PointF P(PointF p) => new(p.X * k + offset.X, p.Y * k + offset.Y);
        RectF R(RectF r) => new(r.X * k + offset.X, r.Y * k + offset.Y, r.Width * k, r.Height * k);
        float TextScale(ChartText t) => Math.Max(k, Math.Min(1, MinTextSize / Math.Max(1, t.Font.Size)));
        PointF PivotOf(ChartText t) => pivots is not null && pivots.TryGetValue(t, out var p) ? p : new PointF(t.X, t.Y);
        // A block of text under one node moves apart at its largest text scale.
        var blockScale = new Dictionary<PointF, float>();
        foreach (var t in source.Texts)
        {
            var pivot = PivotOf(t);
            blockScale[pivot] = Math.Max(blockScale.GetValueOrDefault(pivot), TextScale(t));
        }
        // Edge casings share their path with the inner line: scale each path once.
        var scaledPaths = new HashSet<PathF>(ReferenceEqualityComparer.Instance);
        foreach (var shape in source.Shapes)
        {
            shape.StrokeWidth *= k;
            if (shape.HoverStrokeWidth is { } h) shape.HoverStrokeWidth = h * k;
            if (shape.Dash is { } dash) shape.Dash = dash.Select(d => d * k).ToArray();
            switch (shape)
            {
                case ChartRect r:
                    r.Rect = R(r.Rect);
                    r.CornerRadius *= k;
                    break;
                case ChartCircle c:
                    c.Center = P(c.Center);
                    c.Radius *= k;
                    break;
                case ChartLine l:
                    l.From = P(l.From);
                    l.To = P(l.To);
                    break;
                case ChartPath p when scaledPaths.Add(p.Path):
                    p.Path.Transform(System.Numerics.Matrix3x2.CreateScale(k) * System.Numerics.Matrix3x2.CreateTranslation(offset.X, offset.Y));
                    break;
            }
            scene.Shapes.Add(shape);
        }
        foreach (var t in source.Texts)
        {
            var pivot = PivotOf(t);
            var at = P(pivot);
            var spread = blockScale[pivot];
            scene.Texts.Add(new ChartText
            {
                Text = t.Text, X = at.X + (t.X - pivot.X) * spread, Y = at.Y + (t.Y - pivot.Y) * spread, Font = t.Font with { Size = t.Font.Size * TextScale(t) },
                Color = t.Color, Anchor = t.Anchor, Baseline = t.Baseline, Rotation = t.Rotation, Opacity = t.Opacity, Group = t.Group, FadesWithLegend = t.FadesWithLegend,
            });
        }
        foreach (var m in source.Marks)
            scene.Marks.Add(new ChartMark
            {
                Id = m.Id, Bounds = R(m.Bounds), Polygon = m.Polygon?.Select(P).ToArray(), Radius = m.Radius * k, Group = m.Group, Rows = m.Rows,
                Title = m.Title, Description = m.Description, IsReference = m.IsReference, Anchor = m.Anchor is { } a ? P(a) : null,
            });
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

    /// <summary>Raises <see cref="NodeClicked"/> for a clickable node's mark.</summary>
    internal void ActivateMark(int id)
    {
        if (_markNodes.TryGetValue(id, out var n) && n.IsClickable) NodeClicked?.Invoke(this, new CdsNetworkNodeEventArgs(n));
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var byId = ctx.Items.OfType<CdsNetworkNode>().Where(n => n.Id is not null).GroupBy(n => n.Id!).ToDictionary(g => g.Key, g => g.First());
        string Name(string? nid) => nid is not null && byId.TryGetValue(nid, out var n) ? n.Title ?? nid : nid ?? "–";
        return new ChartTable(["Source", "Target", "Label"],
            (Edges ?? []).Where(e => e is not null).Select(e => new[] { Name(e.Source), Name(e.Target), e.Label ?? "" }).ToList());
    }
}

/// <summary>The network diagram node that was clicked.</summary>
public sealed class CdsNetworkNodeEventArgs(CdsNetworkNode node) : EventArgs
{
    /// <summary>The clicked node.</summary>
    public CdsNetworkNode Node { get; } = node;
}
