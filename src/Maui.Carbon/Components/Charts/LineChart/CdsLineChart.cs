namespace Maui.Carbon;

/// <summary>
/// Lines showing how one or more measures change across an ordered domain, usually time (Carbon's line chart), with a
/// ruler that reads every series at the hovered position. Points with a <c>null</c> value break the line: never connect
/// across missing periods. <see cref="Curve"/> <see cref="CdsChartCurve.StepAfter"/> makes Carbon's step chart.
/// </summary>
public class CdsLineChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="Curve"/>.</summary>
    public static readonly BindableProperty CurveProperty = BindableProperty.Create(
        nameof(Curve), typeof(CdsChartCurve), typeof(CdsLineChart), CdsChartCurve.Linear, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ShowPoints"/>.</summary>
    public static readonly BindableProperty ShowPointsProperty = BindableProperty.Create(
        nameof(ShowPoints), typeof(bool), typeof(CdsLineChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="PointRadius"/>.</summary>
    public static readonly BindableProperty PointRadiusProperty = BindableProperty.Create(
        nameof(PointRadius), typeof(double), typeof(CdsLineChart), 3d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ArePointsFilled"/>.</summary>
    public static readonly BindableProperty ArePointsFilledProperty = BindableProperty.Create(
        nameof(ArePointsFilled), typeof(bool), typeof(CdsLineChart), false, propertyChanged: Changed);

    /// <summary>How the line joins its points (Carbon's <c>curve</c>). Default <see cref="CdsChartCurve.Linear"/>; <see cref="CdsChartCurve.MonotoneX"/> for smooth series.</summary>
    public CdsChartCurve Curve
    {
        get => (CdsChartCurve)GetValue(CurveProperty);
        set => SetValue(CurveProperty, value);
    }

    /// <summary>Whether points are drawn on the line (Carbon's <c>points.enabled</c>). Default <c>true</c>.</summary>
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

    /// <summary>Whether points fill with the series color (Carbon's <c>points.filled</c>); default hollow, filled with <c>layer-01</c>.</summary>
    public bool ArePointsFilled
    {
        get => (bool)GetValue(ArePointsFilledProperty);
        set => SetValue(ArePointsFilledProperty, value);
    }

    private protected override string ChartKind => "Line chart";

    private protected override bool UsesRuler => true;

    private protected override CartesianOptions Options(ChartBuildContext ctx) => new();

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var ruler = IsRulerEnabled;
        var columns = CartesianFrame.RulerMarks(scene, ctx, cs, ctx.ActivePoints);
        var lines = ChartLines.Add(scene, ctx, cs, ctx.ActivePoints, Curve);
        if (ShowPoints)
        {
            var dotId = columns.Count;
            foreach (var d in ctx.ActivePoints)
            {
                if (d.Value is not { } v) continue;
                var px = cs.DomainPx(d);
                if (double.IsNaN(px)) continue;
                var center = cs.XY(px, cs.RangePx(d.Group, v));
                var mark = ruler && columns.TryGetValue(cs.DomainKey(d), out var col) ? col : -1;
                var dot = ChartDots.Dot(center, d.Group, (float)PointRadius, ArePointsFilled, 1, mark);
                if (!ruler)
                {
                    dot.Mark = dotId;
                    scene.Marks.Add(new ChartMark { Id = dotId++, Bounds = new RectF(center.X - 6, center.Y - 6, 12, 12), Radius = 8, Anchor = center, Group = d.Group, Rows = CartesianFrame.TooltipRows(ctx, cs, [d]) });
                }
                scene.Shapes.Add(dot);
            }
        }
        if (!ruler) scene.Marks.RemoveAll(m => m.RulerAt.HasValue);
        scene.Summary = lines == 1 ? "1 line" : $"{lines} lines";
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsLineChart)b).RequestRebuild();
}

/// <summary>Carbon's line graph: one 1.5px path per active group, sorted along the domain, broken at missing values.</summary>
internal static class ChartLines
{
    /// <summary>Adds the lines; returns how many were drawn.</summary>
    public static int Add(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs, IReadOnlyList<CdsChartPoint> rows, CdsChartCurve curve, float strokeWidth = 1.5f)
    {
        var count = 0;
        foreach (var g in ctx.ActiveGroups)
        {
            var points = rows.Where(p => (p.Group ?? "") == g)
                .Select(p => (P: p, Px: cs.DomainPx(p)))
                .Where(x => !double.IsNaN(x.Px))
                .OrderBy(x => x.Px)
                .Select(x => x.P.Value is { } v ? cs.XY(x.Px, cs.RangePx(g, v)) : (PointF?)null)
                .ToList();
            if (points.Count == 0) continue;
            scene.Shapes.Add(new ChartPath
            {
                Path = ChartCurves.Line(points, curve),
                Stroke = ChartPaint.OfSeries(g),
                StrokeWidth = strokeWidth,
                Group = g,
                Join = LineJoin.Round,
            });
            count++;
        }
        return count;
    }
}
