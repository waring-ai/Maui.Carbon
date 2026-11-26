namespace Maui.Carbon;

/// <summary>
/// Areas stacked on each other (Carbon's stacked area chart): each band is one group's share and the top edge is the
/// total, across an ordered domain. Groups stack in data order from the bottom, each band at 40% opacity with a 1.5px line
/// on its top edge; a missing point stacks as 0. Set <see cref="CdsChartAxis.IsPercentage"/> on the value axis for 100%
/// stacking. Keep to five groups or fewer, no negative values, and put the most important group first (at the bottom).
/// The ruler tooltip and the table give the raw values.
/// </summary>
public class CdsStackedAreaChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="Curve"/>.</summary>
    public static readonly BindableProperty CurveProperty = BindableProperty.Create(
        nameof(Curve), typeof(CdsChartCurve), typeof(CdsStackedAreaChart), CdsChartCurve.Linear, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ShowPoints"/>.</summary>
    public static readonly BindableProperty ShowPointsProperty = BindableProperty.Create(
        nameof(ShowPoints), typeof(bool), typeof(CdsStackedAreaChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="PointRadius"/>.</summary>
    public static readonly BindableProperty PointRadiusProperty = BindableProperty.Create(
        nameof(PointRadius), typeof(double), typeof(CdsStackedAreaChart), 3d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ArePointsFilled"/>.</summary>
    public static readonly BindableProperty ArePointsFilledProperty = BindableProperty.Create(
        nameof(ArePointsFilled), typeof(bool), typeof(CdsStackedAreaChart), false, propertyChanged: Changed);

    /// <summary>How the bands' edges join the points (Carbon's <c>curve</c>). Default <see cref="CdsChartCurve.Linear"/>.</summary>
    public CdsChartCurve Curve
    {
        get => (CdsChartCurve)GetValue(CurveProperty);
        set => SetValue(CurveProperty, value);
    }

    /// <summary>Whether points are drawn on each band's top edge (Carbon's <c>points.enabled</c>); they appear, filled, under the ruler. Default <c>true</c>.</summary>
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

    private protected override string ChartKind => "Stacked area chart";

    private protected override bool UsesRuler => true;

    private protected override int DefaultTimeEdgeSpace => 0;

    private protected override CartesianOptions Options(ChartBuildContext ctx) => new() { Stacked = true };

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var ruler = IsRulerEnabled;
        var drawable = ctx.ActivePoints.Where(p => !double.IsNaN(cs.DomainPx(p))).ToList();
        var percentage = cs.Axes.Values.Any(a => a.IsPercentage);
        var series = StackedAreaModel.Stack(drawable, ctx.ActiveGroups, cs.DomainKey, cs.DomainPx, percentage);
        var scale = cs.RangeScale;

        var columns = CartesianFrame.RulerMarks(scene, ctx, cs, drawable.Where(p => p.Value.HasValue).ToList());
        // Anchor each ruler tooltip at the top of the stack, where the eye is.
        var tops = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var s in series)
            foreach (var r in s.Rows)
                tops[r.Key] = tops.TryGetValue(r.Key, out var t) ? Math.Max(t, r.Y1) : r.Y1;
        for (var i = 0; i < scene.Marks.Count; i++)
        {
            var m = scene.Marks[i];
            var key = columns.FirstOrDefault(c => c.Value == m.Id).Key;
            if (key is null || !tops.TryGetValue(key, out var top) || m.RulerAt is not { } at) continue;
            scene.Marks[i] = ChartMarkCopy.With(m, anchor: cs.XY(at, scale.Map(top)));
        }

        var count = 0;
        foreach (var s in series)
        {
            if (s.Rows.Count == 0) continue;
            var runs = ChartAreas.Runs(cs, s.Rows.Select(r => (cs.DomainPx(r.At), (double?)scale.Map(r.Y1), scale.Map(r.Y0))));
            scene.Shapes.Add(new ChartPath
            {
                Path = ChartAreas.Path(runs, Curve),
                Fill = ChartPaint.OfSeries(s.Group),
                Opacity = ChartAreas.SelectedOpacity,
                Group = s.Group,
                LegendFade = ChartAreas.UnselectedOpacity / ChartAreas.SelectedOpacity,
                Hover = ChartHoverStyle.None,
            });
            count++;
        }

        var lineRows = series.Where(s => s.HasLine).SelectMany(s => s.Rows.Select(r => Top(r, s.Group))).ToList();
        ChartLines.Add(scene, ctx, cs, lineRows, Curve);

        if (ShowPoints)
        {
            var anomaly = ChartScatter.Anomaly(cs);
            var dotId = columns.Count;
            foreach (var s in series)
                foreach (var r in s.Rows)
                {
                    if (r.Source is not { } src) continue;
                    var center = cs.XY(cs.DomainPx(r.At), scale.Map(r.Y1));
                    if (float.IsNaN(center.X) || float.IsNaN(center.Y)) continue;
                    var mark = ruler && columns.TryGetValue(r.Key, out var col) ? col : -1;
                    var dot = ChartDots.Dot(center, s.Group, (float)PointRadius, ArePointsFilled, 0.3f, mark);
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
                            Group = s.Group,
                            Rows = CartesianFrame.TooltipRows(ctx, cs, [src]),
                        });
                    }
                    scene.Shapes.Add(dot);
                }
        }
        if (!ruler) scene.Marks.RemoveAll(m => m.RulerAt.HasValue);
        scene.Summary = count == 1 ? "1 area" : $"{count} areas";
    }

    /// <summary>A point on a band's top edge, for the line along it.</summary>
    static CdsChartPoint Top(StackedAreaModel.Row r, string group) =>
        new() { Group = group, Key = CartesianLayout.CategoryOf(r.At), Date = r.At.Date, X = r.At.X, Value = r.Y1 };

    static void Changed(BindableObject b, object? o, object? n) => ((CdsStackedAreaChart)b).RequestRebuild();
}
