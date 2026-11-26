namespace Maui.Carbon;

/// <summary>
/// Several measures per group plotted on spokes around a circle, each group drawn as a translucent shape (Carbon's radar
/// chart). Data: <see cref="CdsChartPoint"/>s with <see cref="CdsChartPoint.Group"/> (the shape and its legend item),
/// <see cref="CdsChartPoint.Key"/> (the spoke; spokes run clockwise from 12 o'clock in the order keys first appear) and
/// <see cref="CdsChartPoint.Value"/>. Use it for a few groups (1-3) across the same 5-12 measures on one scale; for exact
/// comparison use a grouped bar chart. A missing value is skipped (the shape joins its neighbours). Hovering near a spoke
/// shows every shown group's value there. Keep the legend on for more than one group and put the unit in the title.
/// </summary>
public class CdsRadarChart : CdsChart
{
    /// <summary>Bindable property for <see cref="MaxValue"/>.</summary>
    public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(
        nameof(MaxValue), typeof(double?), typeof(CdsRadarChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Alignment"/>.</summary>
    public static readonly BindableProperty AlignmentProperty = BindableProperty.Create(
        nameof(Alignment), typeof(CdsChartAlignment), typeof(CdsRadarChart), CdsChartAlignment.Left, propertyChanged: Changed);

    /// <summary>
    /// Raises the outer ring for a fixed scale (Carbon's <c>radar.maxValue</c>), for example 10 for scores out of 10. A value
    /// below the data maximum is ignored (the data maximum keeps every shape inside). Default <c>null</c>: the data maximum.
    /// </summary>
    public double? MaxValue
    {
        get => (double?)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    /// <summary>Where the radar sits across the graph (Carbon's <c>radar.alignment</c>). Default <see cref="CdsChartAlignment.Left"/>.</summary>
    public CdsChartAlignment Alignment
    {
        get => (CdsChartAlignment)GetValue(AlignmentProperty);
        set => SetValue(AlignmentProperty, value);
    }

    private protected override string ChartKind => "Radar chart";

    private protected override ChartSkeleton Skeleton => ChartSkeleton.Pie;

    static List<string> KeysOf(IEnumerable<CdsChartPoint> points) =>
        points.Where(p => p.Key is not null).Select(p => p.Key!).Distinct(StringComparer.Ordinal).ToList();

    static Dictionary<(string Key, string Group), double> Lookup(IEnumerable<CdsChartPoint> points)
    {
        var map = new Dictionary<(string, string), double>();
        foreach (var p in points)
            if (p.Key is not null && p.Value is { } v && !double.IsNaN(v)) map[(p.Key, p.Group ?? "")] = v;
        return map;
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var width = ctx.Width;
        var height = ctx.Height;
        var scene = new ChartScene { Width = width, Height = height, Plot = new RectF(0, 0, width, height) };
        var keys = KeysOf(ctx.Points);
        var at = Lookup(ctx.Points);
        var radius = (Math.Min(width, height) - 2 * (RadarLayout.LabelHeight + RadarLayout.YLabelPadding)) / 2;
        if (radius <= 0 || keys.Count == 0) return scene;

        // The scale covers every group, shown or not, so isolating a group keeps the rings.
        var all = new List<double>();
        foreach (var k in keys)
            foreach (var g in ctx.Groups)
                if (at.TryGetValue((k, g), out var v)) all.Add(v);
        var (min, max, ticks) = RadarLayout.Scale(all, MaxValue);
        double R(double v) => RadarLayout.Map(v, min, max, radius);
        var r0 = RadarLayout.MinRange;
        var r1 = radius;
        double A(int i) => RadarLayout.Angle(i, keys.Count);

        // Room on the left for the widest label, then the alignment offset.
        var font = ChartFont.Tick;
        var labelW = keys.Select(k => CdsChartTextMetrics.Width(k, font)).ToList();
        var leftPadding = keys.Select((_, i) => labelW[i] + radius * Math.Sin(A(i) - Math.PI / 2)).Max();
        var c0 = new PointF((float)(leftPadding + RadarLayout.XLabelPadding), height / 2);
        var placements = keys.Select((_, i) =>
        {
            var p = RadarLayout.Polar(A(i), r1 + RadarLayout.XLabelPadding, c0);
            var (anchor, baseline) = RadarLayout.LabelPlacement(A(i));
            var x0 = anchor == ChartTextAnchor.Start ? p.X : anchor == ChartTextAnchor.Middle ? p.X - labelW[i] / 2 : p.X - labelW[i];
            return (P: p, Anchor: anchor, Baseline: baseline, X0: x0, X1: x0 + labelW[i]);
        }).ToList();
        var yLabelTicks = ticks.Length > 0 ? new[] { ticks[0], ticks[^1] } : [];
        var yLabelRight = yLabelTicks.Length > 0 ? c0.X + RadarLayout.YLabelPadding + yLabelTicks.Max(t => CdsChartTextMetrics.Width(ctx.Number(t), font)) : c0.X;
        var minX = Math.Min(c0.X - r1, placements.Min(l => l.X0));
        var maxX = Math.Max(Math.Max(c0.X + r1, yLabelRight), placements.Max(l => l.X1));
        var dx = Alignment switch
        {
            CdsChartAlignment.Center => MathF.Floor((width - (maxX - minX)) / 2) - minX,
            CdsChartAlignment.Right => width - maxX,
            _ => 0,
        };
        var c = new PointF(c0.X + dx, c0.Y);

        // Rings (one polygon per tick) and spokes: 1px layer-accent-01.
        var accent = ChartPaint.OfKey(CdsColorKeys.LayerAccent01);
        foreach (var t in ticks)
            scene.Shapes.Add(new ChartPath { Path = Polygon(keys.Select((_, i) => RadarLayout.Polar(A(i), R(t), c)).ToList()), Stroke = accent, Join = LineJoin.Miter });
        for (var i = 0; i < keys.Count; i++)
            scene.Shapes.Add(new ChartLine { From = RadarLayout.Polar(A(i), r0, c), To = RadarLayout.Polar(A(i), r1, c), Stroke = accent });

        // Key labels 10px outside the outer ring, anchored away from the center.
        for (var i = 0; i < keys.Count; i++)
        {
            var l = placements[i];
            scene.Texts.Add(RoundChartText.At(keys[i], l.P.X + dx, l.P.Y, font, l.Anchor, l.Baseline));
        }

        // One closed shape per shown group: 30% fill and a 1.5px stroke; legend hover drops the others to 10%.
        var active = ctx.ActiveGroups;
        foreach (var g in active)
        {
            var points = keys.Select((k, i) => at.TryGetValue((k, g), out var v) ? RadarLayout.Polar(A(i), R(v), c) : (PointF?)null)
                .Where(p => p is not null).Select(p => p!.Value).ToList();
            if (points.Count == 0) continue;
            var path = Polygon(points);
            scene.Shapes.Add(new ChartPath { Path = path, Fill = ChartPaint.OfSeries(g), FillOpacity = RadarLayout.FillOpacity, Group = g, Hover = ChartHoverStyle.None, LegendFade = RadarLayout.FadedOpacity / RadarLayout.FillOpacity });
            scene.Shapes.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfSeries(g), StrokeWidth = 1.5f, Group = g, Join = LineJoin.Miter, Hover = ChartHoverStyle.None, LegendFade = RadarLayout.FadedOpacity });
        }

        // One mark per spoke: the dashed spoke, a dot per shown group, and a tooltip with each group's value.
        var inverse = ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute);
        for (var i = 0; i < keys.Count; i++)
        {
            var k = keys[i];
            scene.Shapes.Add(new ChartLine { From = RadarLayout.Polar(A(i), r0, c), To = RadarLayout.Polar(A(i), r1, c), Stroke = inverse, Dash = [4, 4], Mark = i, HoverOnly = true, Hover = ChartHoverStyle.None });
            var rows = new List<ChartTooltipRow>();
            var spoken = new List<string>();
            PointF? outer = null;
            double outerValue = double.MinValue;
            foreach (var g in active)
            {
                if (!at.TryGetValue((k, g), out var v)) continue;
                var p = RadarLayout.Polar(A(i), R(v), c);
                scene.Shapes.Add(new ChartCircle { Center = p, Radius = RadarLayout.DotRadius, Fill = ChartPaint.OfSeries(g), Group = g, Mark = i, HoverOnly = true, Hover = ChartHoverStyle.None });
                rows.Add(new ChartTooltipRow(CdsChartTruncate.Apply(g), ctx.Value(v), ChartPaint.OfSeries(g)));
                spoken.Add($"{g} {ctx.Value(v)}");
                if (v > outerValue)
                {
                    outerValue = v;
                    outer = p;
                }
            }
            var area = RadarLayout.SpokeArea(c, A(i), r1);
            scene.Marks.Add(new ChartMark
            {
                Id = i,
                Bounds = BoundsOf(area),
                Polygon = area,
                Anchor = outer ?? RadarLayout.Polar(A(i), r1 / 2, c),
                Title = rows.Count == 0 ? k : null,
                Rows = rows,
                Description = spoken.Count == 0 ? $"{k}, no values" : $"{k}: {string.Join(", ", spoken)}",
            });
        }

        // The innermost and outermost tick values, 8px right of the top spoke.
        foreach (var t in yLabelTicks)
        {
            var p = RadarLayout.Polar(-Math.PI / 2, R(t), c);
            scene.Texts.Add(RoundChartText.At(ctx.Number(t), p.X + RadarLayout.YLabelPadding, p.Y, font, ChartTextAnchor.Start, SvgBaseline.Middle));
        }

        var shapes = active.Count == 1 ? "1 group" : $"{active.Count} groups";
        scene.Summary = $"{shapes} on {keys.Count} axes";
        return scene;
    }

    /// <summary>Carbon's radar table: one row per shown group, one column per key (a dash for missing values).</summary>
    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var keys = KeysOf(ctx.Points);
        var at = Lookup(ctx.Points);
        var headers = new List<string> { ctx.GroupLabel };
        headers.AddRange(keys);
        var rows = ctx.ActiveGroups.Select(g =>
        {
            var r = new List<string> { g };
            r.AddRange(keys.Select(k => at.TryGetValue((k, g), out var v) ? ctx.Value(v) : "–"));
            return r.ToArray();
        }).ToList();
        return new ChartTable(headers, rows);
    }

    static PathF Polygon(IReadOnlyList<PointF> points)
    {
        var path = new PathF();
        if (points.Count == 0) return path;
        path.MoveTo(points[0]);
        for (var i = 1; i < points.Count; i++) path.LineTo(points[i]);
        path.Close();
        return path;
    }

    static RectF BoundsOf(IReadOnlyList<PointF> points)
    {
        var x0 = points.Min(p => p.X);
        var y0 = points.Min(p => p.Y);
        return new RectF(x0, y0, points.Max(p => p.X) - x0, points.Max(p => p.Y) - y0);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsRadarChart)b).RequestRebuild();
}
