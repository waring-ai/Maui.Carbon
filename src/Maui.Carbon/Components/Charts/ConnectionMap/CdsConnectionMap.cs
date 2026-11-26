using System.Collections.Specialized;
using System.Globalization;

namespace Maui.Carbon;

/// <summary>How a connection map draws its lines (Carbon's <c>connectionMap.lineType</c>).</summary>
public enum CdsConnectionLineType
{
    /// <summary>The shortest path on the globe (default): long routes curve on a flat map.</summary>
    GreatCircle,
    /// <summary>A straight line between the projected ends.</summary>
    Straight,
    /// <summary>An arc bowed to the left of the direction of travel, which separates return routes.</summary>
    Arc,
}

/// <summary>
/// Lines between pairs of locations over a muted base map (a connection map): routes, links and flows between places,
/// drawn as great circles, straight lines or arcs, with line width by value and location markers. Data is
/// <see cref="CdsGeoConnection"/>s; give each its coordinates, or name its ends and list them once in <see cref="Nodes"/>.
/// Lines are the group color at 80% opacity with round caps; hovering a line raises it, hovering a location shows its
/// connection count and total. Use a <see cref="CdsProportionalSymbolMap"/> for values at places without links.
/// </summary>
public class CdsConnectionMap : CdsThematicChart
{
    /// <summary>Bindable property for <see cref="LineType"/>.</summary>
    public static readonly BindableProperty LineTypeProperty = BindableProperty.Create(
        nameof(LineType), typeof(CdsConnectionLineType), typeof(CdsConnectionMap), CdsConnectionLineType.GreatCircle, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="MinimumLineWidth"/>.</summary>
    public static readonly BindableProperty MinimumLineWidthProperty = BindableProperty.Create(
        nameof(MinimumLineWidth), typeof(double), typeof(CdsConnectionMap), 1d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="MaximumLineWidth"/>.</summary>
    public static readonly BindableProperty MaximumLineWidthProperty = BindableProperty.Create(
        nameof(MaximumLineWidth), typeof(double), typeof(CdsConnectionMap), 6d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ArePointsEnabled"/>.</summary>
    public static readonly BindableProperty ArePointsEnabledProperty = BindableProperty.Create(
        nameof(ArePointsEnabled), typeof(bool), typeof(CdsConnectionMap), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="PointRadius"/>.</summary>
    public static readonly BindableProperty PointRadiusProperty = BindableProperty.Create(
        nameof(PointRadius), typeof(double), typeof(CdsConnectionMap), 3d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ArePointLabelsEnabled"/>.</summary>
    public static readonly BindableProperty ArePointLabelsEnabledProperty = BindableProperty.Create(
        nameof(ArePointLabelsEnabled), typeof(bool), typeof(CdsConnectionMap), false, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ValueLabel"/>.</summary>
    public static readonly BindableProperty ValueLabelProperty = BindableProperty.Create(
        nameof(ValueLabel), typeof(string), typeof(CdsConnectionMap), "Value", propertyChanged: Changed);

    readonly CdsItemCollection<CdsGeoPoint> _nodes = [];

    /// <summary>Creates a connection map.</summary>
    public CdsConnectionMap()
    {
        _nodes.CollectionChanged += OnNodesChanged;
    }

    /// <summary>Great circles (default), straight lines or arcs (Carbon's <c>lineType</c>).</summary>
    public CdsConnectionLineType LineType
    {
        get => (CdsConnectionLineType)GetValue(LineTypeProperty);
        set => SetValue(LineTypeProperty, value);
    }

    /// <summary>The width of a line with no value or the smallest value (Carbon's <c>lineWidthRange[0]</c>). Default 1.</summary>
    public double MinimumLineWidth
    {
        get => (double)GetValue(MinimumLineWidthProperty);
        set => SetValue(MinimumLineWidthProperty, value);
    }

    /// <summary>The width of the largest value's line (Carbon's <c>lineWidthRange[1]</c>); widths grow in proportion to the value. Default 6.</summary>
    public double MaximumLineWidth
    {
        get => (double)GetValue(MaximumLineWidthProperty);
        set => SetValue(MaximumLineWidthProperty, value);
    }

    /// <summary>Whether locations show as circles (Carbon's <c>points.enabled</c>): <c>layer-01</c> with a 1.5px <c>text-primary</c> ring. Default <c>true</c>.</summary>
    public bool ArePointsEnabled
    {
        get => (bool)GetValue(ArePointsEnabledProperty);
        set => SetValue(ArePointsEnabledProperty, value);
    }

    /// <summary>The location circles' radius (Carbon's <c>points.radius</c>). Default 3.</summary>
    public double PointRadius
    {
        get => (double)GetValue(PointRadiusProperty);
        set => SetValue(PointRadiusProperty, value);
    }

    /// <summary>Whether location names show beside their circles (Carbon's <c>points.labels</c>), 12px <c>text-primary</c>. Default <c>false</c>.</summary>
    public bool ArePointLabelsEnabled
    {
        get => (bool)GetValue(ArePointLabelsEnabledProperty);
        set => SetValue(ArePointLabelsEnabledProperty, value);
    }

    /// <summary>The name of the value in the table (Carbon's <c>valueLabel</c>). Default "Value"; give it a unit ("Departures per week").</summary>
    public string? ValueLabel
    {
        get => (string?)GetValue(ValueLabelProperty);
        set => SetValue(ValueLabelProperty, value);
    }

    /// <summary>Named locations that connections refer to by <see cref="CdsGeoConnection.Source"/> and <see cref="CdsGeoConnection.Target"/> (Carbon's <c>connectionMap.nodes</c>).</summary>
    public IList<CdsGeoPoint> Nodes => _nodes;

    private protected override string ChartKind => "Connection map";

    private protected override bool IsEmpty(IReadOnlyList<object> items) => !items.OfType<CdsGeoConnection>().Any();

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) =>
        items.OfType<CdsGeoConnection>().Select(p => p.Group ?? "").Distinct().ToList();

    void OnNodesChanged(object? sender, NotifyCollectionChangedEventArgs e) => RequestRebuild();

    /// <summary>The ends of each connection: its own coordinates, else the named node; connections without both ends are dropped.</summary>
    internal static List<(CdsGeoConnection C, GeoPos From, GeoPos To)> Resolve(IEnumerable<CdsGeoConnection> rows, IEnumerable<CdsGeoPoint> nodes)
    {
        var named = new Dictionary<string, GeoPos>(StringComparer.Ordinal);
        foreach (var n in nodes)
            if (n.Name is not null && n.HasLocation) named[n.Name] = new GeoPos(n.Longitude!.Value, n.Latitude!.Value);
        GeoPos? At(double? lon, double? lat, string? name) =>
            lon is { } x && lat is { } y && double.IsFinite(x) && double.IsFinite(y) ? new GeoPos(x, y)
            : name is not null && named.TryGetValue(name, out var p) ? p : null;
        var list = new List<(CdsGeoConnection, GeoPos, GeoPos)>();
        foreach (var c in rows)
            if (At(c.FromLongitude, c.FromLatitude, c.Source) is { } from && At(c.ToLongitude, c.ToLatitude, c.Target) is { } to)
                list.Add((c, from, to));
        return list;
    }

    /// <summary>The line width of a value: <paramref name="w0"/> plus its share of the largest value.</summary>
    internal static double LineWidth(double? value, double maxValue, double w0, double w1) =>
        value is { } v && !double.IsNaN(v) && maxValue > 0 ? w0 + (w1 - w0) * Math.Abs(v) / maxValue : w0;

    static string CoordKey(GeoPos p) => p.Lon.ToString("F3", CultureInfo.InvariantCulture) + "," + p.Lat.ToString("F3", CultureInfo.InvariantCulture);

    /// <summary>The projected polylines of a connection (several when a great circle crosses the antimeridian).</summary>
    List<PointF[]> LinePoints(GeoProjection projection, GeoPos from, GeoPos to)
    {
        if (LineType == CdsConnectionLineType.GreatCircle)
            return projection.CutLine(GeoProjection.GreatCircle(from, to)).Select(part => part.Select(projection.ProjectRotated).ToArray()).ToList();
        var a = projection.Project(from.Lon, from.Lat);
        var b = projection.Project(to.Lon, to.Lat);
        if (LineType == CdsConnectionLineType.Straight) return [[a, b]];
        // Carbon's arc: a quadratic curve whose control point is the midpoint pushed left of travel by 0.2 of the length.
        var mx = (a.X + b.X) / 2;
        var my = (a.Y + b.Y) / 2;
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var c = new PointF(mx + dy * 0.2f, my - dx * 0.2f);
        var pts = new PointF[25];
        for (var i = 0; i < pts.Length; i++)
        {
            var t = i / (float)(pts.Length - 1);
            var u = 1 - t;
            pts[i] = new PointF(u * u * a.X + 2 * u * t * c.X + t * t * b.X, u * u * a.Y + 2 * u * t * c.Y + t * t * b.Y);
        }
        return [pts];
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var plot = new RectF(0, 0, ctx.Width, ctx.Height);
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = plot, HitMode = ChartHitMode.Contains };
        var all = Resolve(ctx.Items.OfType<CdsGeoConnection>(), _nodes);
        var ends = all.SelectMany(c => new[] { c.From, c.To }).ToList();
        var projection = Fit(plot, null, ends, EffectiveFit == CdsMapFit.Data ? 16 : 0);
        GeoScene.BaseMap(scene, projection, MapFeatures);

        var maxValue = all.Select(c => c.C.Value is { } v && !double.IsNaN(v) ? Math.Abs(v) : 0).DefaultIfEmpty(0).Max();
        var links = all.Where(c => ctx.IsActive(c.C.Group ?? "")).ToList();
        var nodes = new Dictionary<string, (GeoPos P, string? Name, List<int> Links)>();
        var nodeOrder = new List<string>();
        for (var i = 0; i < links.Count; i++)
        {
            foreach (var (p, name) in new[] { (links[i].From, links[i].C.Source), (links[i].To, links[i].C.Target) })
            {
                var k = CoordKey(p);
                if (!nodes.TryGetValue(k, out var n))
                {
                    n = (p, name, []);
                    nodeOrder.Add(k);
                }
                if (n.Name is null && name is not null) n = (n.P, name, n.Links);
                n.Links.Add(i);
                nodes[k] = n;
            }
        }

        const float rest = 0.8f;
        var id = 0;
        var linkShapes = new List<(PathF Path, float Width, string Group)>();
        var overlays = new List<ChartShape>();
        for (var i = 0; i < links.Count; i++)
        {
            var (c, from, to) = links[i];
            var g = c.Group ?? "";
            var sw = (float)LineWidth(c.Value, maxValue, MinimumLineWidth, MaximumLineWidth);
            var parts = LinePoints(projection, from, to).Where(p => p.Length >= 2 && p.All(q => float.IsFinite(q.X) && float.IsFinite(q.Y))).ToList();
            var path = new PathF();
            foreach (var part in parts)
            {
                path.MoveTo(part[0]);
                for (var j = 1; j < part.Length; j++) path.LineTo(part[j]);
            }
            linkShapes.Add((path, sw, g));
            if (parts.Count == 0) continue;
            var mark = id++;
            scene.Shapes.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfSeries(g), StrokeWidth = sw, Opacity = rest, Cap = LineCap.Round, Join = LineJoin.Round, Group = g, LegendFade = 0.3f / rest, Mark = mark, Hover = ChartHoverStyle.None, Clip = true });
            overlays.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfSeries(g), StrokeWidth = sw, Cap = LineCap.Round, Join = LineJoin.Round, Group = g, Mark = mark, HoverOnly = true, Hover = ChartHoverStyle.None, Clip = true });
            var longest = parts.OrderByDescending(p => p.Length).First();
            var mid = longest[longest.Length / 2];
            var arrow = (c.Source ?? "") + " → " + (c.Target ?? "");
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = GeoScene.Bounds(parts.SelectMany(p => p)),
                Polygon = GeoScene.HitPolygon(parts.Select(p => GeoScene.StrokeHit(p, Math.Max(8, sw + 4))).ToList()),
                Anchor = mid,
                Group = g,
                Rows =
                [
                    new ChartTooltipRow(arrow, c.Value is { } v ? ctx.Value(v) : null, ChartPaint.OfSeries(g)),
                    new ChartTooltipRow(ctx.GroupLabel, c.Group ?? ""),
                ],
                Description = $"{c.Source} to {c.Target}" + (c.Value is { } dv ? $": {ctx.Value(dv)}" : "") + (string.IsNullOrEmpty(c.Group) ? "" : $", {c.Group}"),
            });
        }
        scene.Shapes.AddRange(overlays);
        var lines = id;

        var shownNodes = 0;
        if (ArePointsEnabled)
        {
            var r = (float)PointRadius;
            var projected = nodeOrder.Select(k => (Key: k, N: nodes[k], At: projection.Project(nodes[k].P.Lon, nodes[k].P.Lat)))
                .Where(x => float.IsFinite(x.At.X) && float.IsFinite(x.At.Y)).ToList();
            var nodeMarks = new List<ChartMark>();
            var circles = new List<ChartShape>();
            foreach (var (key, n, at) in projected)
            {
                var mark = id++;
                foreach (var li in n.Links)
                {
                    var (path, sw, g) = linkShapes[li];
                    scene.Shapes.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfSeries(g), StrokeWidth = sw, Cap = LineCap.Round, Join = LineJoin.Round, Mark = mark, HoverOnly = true, Hover = ChartHoverStyle.None, Clip = true });
                }
                circles.Add(new ChartCircle { Center = at, Radius = r, Fill = ChartPaint.OfKey(CdsColorKeys.Layer01), HoverFill = ChartPaint.OfKey(CdsColorKeys.TextPrimary), Stroke = ChartPaint.OfKey(CdsColorKeys.TextPrimary), StrokeWidth = 1.5f, Mark = mark, Hover = ChartHoverStyle.None, Clip = true });
                var values = n.Links.Select(i => links[i].C.Value).Where(v => v is { } x && !double.IsNaN(x)).Select(v => v!.Value).ToList();
                var name = n.Name ?? key;
                var rows = new List<ChartTooltipRow> { new(name, null, Bold: true), new("Connections", ctx.Number(n.Links.Count)) };
                if (values.Count > 0) rows.Add(new ChartTooltipRow(ctx.TotalLabel, ctx.Value(values.Sum())));
                nodeMarks.Add(new ChartMark
                {
                    Id = mark,
                    Bounds = new RectF(at.X - r, at.Y - r, 2 * r, 2 * r),
                    Radius = Math.Max(r + 2, 6),
                    Anchor = at,
                    Rows = rows,
                    Description = $"{name}, {n.Links.Count} connection{(n.Links.Count == 1 ? "" : "s")}" + (values.Count > 0 ? $", total {ctx.Value(values.Sum())}" : ""),
                });
                if (ArePointLabelsEnabled && n.Name is not null)
                {
                    var right = at.X < ctx.Width - 90;
                    var x = at.X + (right ? 1 : -1) * (r + 4);
                    // Carbon's background halo: a 3px background stroke around the glyphs, so routes stay visible around the name.
                    var label = new ChartText { Text = n.Name, X = x, Y = at.Y, Baseline = ChartTextBaseline.Middle, Anchor = right ? ChartTextAnchor.Start : ChartTextAnchor.End, Color = ChartPaint.OfKey(CdsColorKeys.TextPrimary) };
                    ChartTextHalo.Add(scene.Texts, label, ChartPaint.OfKey(CdsColorKeys.Background));
                }
            }
            scene.Shapes.AddRange(circles);
            scene.Marks.AddRange(nodeMarks);
            shownNodes = nodeMarks.Count;
        }
        scene.Summary = (lines == 1 ? "1 connection" : $"{lines} connections") + (shownNodes > 0 ? $", {shownNodes} location{(shownNodes == 1 ? "" : "s")}" : "");
        return scene;
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx) => new(
        [ctx.GroupLabel, "Source", "Target", ValueLabel ?? "Value"],
        ctx.Items.OfType<CdsGeoConnection>().Where(c => ctx.IsActive(c.Group ?? "")).Select(c => new[]
        {
            c.Group ?? "", c.Source ?? "", c.Target ?? "", ctx.Value(c.Value),
        }).ToList());

    static void Changed(BindableObject b, object? o, object? n) => ((CdsConnectionMap)b).RequestRebuild();
}
