namespace Maui.Carbon;

/// <summary>
/// Filled areas under one or more series showing how a quantity's volume changes across an ordered domain, usually time
/// (Carbon's area chart), with the ruler reading every series at the hovered position. Each area fills from the baseline to
/// its values at 40% opacity under a 1.5px line. Keep to two or three overlapping series; for parts of a total use
/// <see cref="CdsStackedAreaChart"/>. With a single series, points that set <see cref="CdsChartPoint.Low"/> and
/// <see cref="CdsChartPoint.High"/> shade the band between those bounds (a forecast or uncertainty range) instead, and
/// <see cref="IsGradientEnabled"/> fades the fill from the line down.
/// </summary>
public class CdsAreaChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="Curve"/>.</summary>
    public static readonly BindableProperty CurveProperty = BindableProperty.Create(
        nameof(Curve), typeof(CdsChartCurve), typeof(CdsAreaChart), CdsChartCurve.Linear, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ShowPoints"/>.</summary>
    public static readonly BindableProperty ShowPointsProperty = BindableProperty.Create(
        nameof(ShowPoints), typeof(bool), typeof(CdsAreaChart), true, propertyChanged: Changed,
        defaultValueCreator: static b => b is not CdsSparklineChart);

    /// <summary>Bindable property for <see cref="PointRadius"/>.</summary>
    public static readonly BindableProperty PointRadiusProperty = BindableProperty.Create(
        nameof(PointRadius), typeof(double), typeof(CdsAreaChart), 3d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ArePointsFilled"/>.</summary>
    public static readonly BindableProperty ArePointsFilledProperty = BindableProperty.Create(
        nameof(ArePointsFilled), typeof(bool), typeof(CdsAreaChart), false, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="IsGradientEnabled"/>.</summary>
    public static readonly BindableProperty IsGradientEnabledProperty = BindableProperty.Create(
        nameof(IsGradientEnabled), typeof(bool), typeof(CdsAreaChart), false, propertyChanged: Changed,
        defaultValueCreator: static b => b is CdsSparklineChart);

    /// <summary>How the line and the area's edge join the points (Carbon's <c>curve</c>). Default <see cref="CdsChartCurve.Linear"/>; <see cref="CdsChartCurve.MonotoneX"/> for smooth series.</summary>
    public CdsChartCurve Curve
    {
        get => (CdsChartCurve)GetValue(CurveProperty);
        set => SetValue(CurveProperty, value);
    }

    /// <summary>
    /// Whether points are drawn (Carbon's <c>points.enabled</c>): hollow circles that appear, filled, at the position under
    /// the ruler. Default <c>true</c> (<c>false</c> for a sparkline).
    /// </summary>
    public bool ShowPoints
    {
        get => (bool)GetValue(ShowPointsProperty);
        set => SetValue(ShowPointsProperty, value);
    }

    /// <summary>The point radius (Carbon's <c>points.radius</c>). Default 3.</summary>
    public double PointRadius
    {
        get => (double)GetValue(PointRadiusProperty);
        set => SetValue(PointRadiusProperty, value);
    }

    /// <summary>Whether points fill with the series color at 30% (Carbon's <c>points.filled</c>); default hollow, filled with <c>layer-01</c>.</summary>
    public bool ArePointsFilled
    {
        get => (bool)GetValue(ArePointsFilledProperty);
        set => SetValue(ArePointsFilledProperty, value);
    }

    /// <summary>
    /// Fills a single series with Carbon's gradient (<c>color.gradient.enabled</c>): the series color at 60% opacity at the
    /// top fading to 0 at 80% of the area's height; when the value axis crosses zero the fade meets zero and grows again
    /// below it. Ignored with more than one series, with bounds, or on horizontal charts. Default <c>false</c>
    /// (<c>true</c> for a sparkline).
    /// </summary>
    public bool IsGradientEnabled
    {
        get => (bool)GetValue(IsGradientEnabledProperty);
        set => SetValue(IsGradientEnabledProperty, value);
    }

    private protected override string ChartKind => "Area chart";

    private protected override bool UsesRuler => true;

    private protected override int DefaultTimeEdgeSpace => 0;

    /// <summary>Whether the areas are drawn (a line sparkline draws the line alone).</summary>
    private protected virtual bool ShowsArea => true;

    /// <summary>Whether the grid backdrop is transparent, so the chart sits on its surface (sparklines).</summary>
    private protected virtual bool TransparentBackdrop => false;

    private protected override CartesianOptions Options(ChartBuildContext ctx) => new() { RangeValues = RangeValues };

    /// <summary>The value domain covers the values and both bounds (Carbon adds the bound keys to the domain).</summary>
    static IEnumerable<double> RangeValues(IReadOnlyList<CdsChartPoint> rows)
    {
        foreach (var p in rows)
        {
            if (p.Value is { } v) yield return v;
            if (p.Low is { } lo) yield return lo;
            if (p.High is { } hi) yield return hi;
        }
    }

    /// <summary>A point without its bounds: tooltips show its value (Carbon's area tooltip), not a start and end.</summary>
    internal static CdsChartPoint ValueOnly(CdsChartPoint p) =>
        p.Low is null && p.High is null ? p : new CdsChartPoint { Group = p.Group, Key = p.Key, Date = p.Date, X = p.X, Value = p.Value, Size = p.Size, Tag = p.Tag };

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        if (TransparentBackdrop && scene.Shapes.Count > 0 && scene.Shapes[0] is ChartRect backdrop) backdrop.Fill = ChartPaint.None;
        var ruler = IsRulerEnabled;
        var rows = ctx.ActivePoints.Where(p => p.Value.HasValue).ToList();
        var columns = CartesianFrame.RulerMarks(scene, ctx, cs, rows, 0, pts => CartesianFrame.TooltipRows(ctx, cs, pts.Select(ValueOnly).ToList()));
        var groups = ctx.ActiveGroups
            .Select(g => (Group: g, Rows: ctx.ActivePoints.Where(p => (p.Group ?? "") == g).Select(p => (P: p, Px: cs.DomainPx(p))).Where(x => !double.IsNaN(x.Px)).OrderBy(x => x.Px).ToList()))
            .Where(s => s.Rows.Count > 0)
            .ToList();
        var bounds = groups.Count == 1 && groups[0].Rows.Any(x => x.P.Low.HasValue || x.P.High.HasValue);
        if (bounds) DescribeBounds(scene, ctx, cs, columns);

        ChartLines.Add(scene, ctx, cs, ctx.ActivePoints, Curve);
        if (ShowsArea)
        {
            var gradient = IsGradientEnabled && groups.Count == 1 && !bounds && cs.Vertical;
            foreach (var (g, list) in groups)
            {
                var scale = cs.ScaleFor(g);
                if (bounds)
                {
                    double Bound(CdsChartPoint p, double? b) => scale.Map(b ?? p.Value ?? double.NaN);
                    var runs = ChartAreas.Runs(cs, list.Select(x => (x.Px, x.P.Value is null ? (double?)null : Bound(x.P, x.P.Low), Bound(x.P, x.P.High))));
                    scene.Shapes.Add(new ChartPath
                    {
                        Path = ChartAreas.Path(runs, Curve),
                        Fill = ChartPaint.OfSeries(g),
                        FillOpacity = ChartAreas.SelectedOpacity,
                        Stroke = ChartPaint.OfSeries(g),
                        StrokeWidth = 0.7f,
                        Dash = [2, 2],
                        Group = g,
                        LegendFade = ChartAreas.UnselectedOpacity,
                        Hover = ChartHoverStyle.None,
                    });
                    continue;
                }
                var basePx = scale.Map(ChartAreas.BaseValue(cs, g));
                var areaRuns = ChartAreas.Runs(cs, list.Select(x => (x.Px, x.P.Value is { } v ? scale.Map(v) : (double?)null, basePx)));
                if (gradient)
                {
                    var (min, max) = scale.Domain;
                    var curves = areaRuns.Select(r => ChartAreas.Flatten(r.Forward, Curve)).ToList();
                    if (ChartAreas.Gradient(ChartAreas.Path(areaRuns, Curve), curves, (float)basePx, ChartAreas.GradientStops(min, max), g) is { } fill) scene.Shapes.Add(fill);
                    continue;
                }
                scene.Shapes.Add(new ChartPath
                {
                    Path = ChartAreas.Path(areaRuns, Curve),
                    Fill = ChartPaint.OfSeries(g),
                    Stroke = ChartPaint.OfSeries(g),
                    Opacity = ChartAreas.SelectedOpacity,
                    Group = g,
                    LegendFade = ChartAreas.UnselectedOpacity / ChartAreas.SelectedOpacity,
                    Hover = ChartHoverStyle.None,
                });
            }
        }

        if (ShowPoints) AddPoints(scene, ctx, cs, rows, columns, ruler);
        if (!ruler) scene.Marks.RemoveAll(m => m.RulerAt.HasValue);
        scene.Summary = groups.Count == 1 ? (ShowsArea ? "1 area" : "1 line") : $"{groups.Count} {(ShowsArea ? "areas" : "lines")}";
    }

    void AddPoints(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs, List<CdsChartPoint> rows, Dictionary<string, int> columns, bool ruler)
    {
        var anomaly = ChartScatter.Anomaly(cs);
        var dotId = columns.Count;
        foreach (var d in rows)
        {
            var px = cs.DomainPx(d);
            if (double.IsNaN(px)) continue;
            var center = cs.XY(px, cs.RangePx(d.Group, d.Value!.Value));
            if (float.IsNaN(center.X) || float.IsNaN(center.Y)) continue;
            var mark = ruler && columns.TryGetValue(cs.DomainKey(d), out var col) ? col : -1;
            var dot = ChartDots.Dot(center, d.Group, (float)PointRadius, ArePointsFilled, 0.3f, mark);
            dot.HoverOnly = true;
            if (anomaly?.Invoke(center) == true) dot.StrokeWidth = 3;
            if (!ruler)
            {
                dot.Mark = dotId;
                scene.Marks.Add(new ChartMark
                {
                    Id = dotId++,
                    Bounds = new RectF(center.X - 6, center.Y - 6, 12, 12),
                    Radius = 8,
                    Anchor = center,
                    Group = d.Group,
                    Rows = CartesianFrame.TooltipRows(ctx, cs, [ValueOnly(d)]),
                });
            }
            scene.Shapes.Add(dot);
        }
    }

    /// <summary>Screen readers hear the bounds with the value ("Orders 348, 324 to 371"); the tooltip shows the value, as Carbon's.</summary>
    static void DescribeBounds(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs, Dictionary<string, int> columns)
    {
        var byId = ctx.ActivePoints.Where(p => p.Value.HasValue).GroupBy(cs.DomainKey).ToDictionary(g => g.Key, g => g.First());
        for (var i = 0; i < scene.Marks.Count; i++)
        {
            var m = scene.Marks[i];
            var key = columns.FirstOrDefault(c => c.Value == m.Id).Key;
            if (key is null || !byId.TryGetValue(key, out var p) || (p.Low is null && p.High is null)) continue;
            var range = $"{ctx.Value(p.Low ?? p.Value)} to {ctx.Value(p.High ?? p.Value)}";
            scene.Marks[i] = ChartMarkCopy.With(m, description: $"{cs.DomainText(p)}, {ctx.Value(p.Value)}, range {range}");
        }
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var cs = Layout(ctx);
        var table = CartesianFrame.Table(ctx, cs);
        var points = ctx.ActivePoints;
        if (!points.Any(p => p.Low.HasValue || p.High.HasValue)) return table;
        // Carbon's bounded area table: the value stays in its column and the bounds get their own.
        var headers = table.Headers.Concat(["Upper bound", "Lower bound"]).ToList();
        var rows = new List<string[]>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var p = points[i];
            var row = table.Rows[i].ToList();
            for (var c = 2; c < row.Count; c++)
                if (row[c].Length > 0) row[c] = ctx.Value(p.Value);
            row.Add(p.High is { } hi ? ctx.Number(hi) : "");
            row.Add(p.Low is { } lo ? ctx.Number(lo) : "");
            rows.Add(row.ToArray());
        }
        return new ChartTable(headers, rows);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsAreaChart)b).RequestRebuild();
}
