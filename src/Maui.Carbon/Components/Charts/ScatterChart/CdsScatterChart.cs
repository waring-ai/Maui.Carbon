namespace Maui.Carbon;

/// <summary>
/// Dots plotting each observation by two measures, or along a time or category axis, to show correlation, clusters,
/// distribution and outliers (Carbon's scatter chart). Give the domain axis <see cref="CdsChartScaleType.Linear"/> to place
/// points by <see cref="CdsChartPoint.X"/> (or a time or labels axis for <see cref="CdsChartPoint.Date"/> or
/// <see cref="CdsChartPoint.Key"/>); <see cref="CdsChartPoint.Value"/> is the vertical position. Dots are 4px with a 1.5px
/// series stroke and a 30% fill, and are not clipped at the plot edge. With axis thresholds, dots at or beyond the highest
/// threshold get a 3px stroke. For a third measure use <see cref="CdsBubbleChart"/>; for thousands of points aggregate
/// into a heat map or histogram.
/// </summary>
public class CdsScatterChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="ShowPoints"/>.</summary>
    public static readonly BindableProperty ShowPointsProperty = BindableProperty.Create(
        nameof(ShowPoints), typeof(bool), typeof(CdsScatterChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="PointRadius"/>.</summary>
    public static readonly BindableProperty PointRadiusProperty = BindableProperty.Create(
        nameof(PointRadius), typeof(double), typeof(CdsScatterChart), 4d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ArePointsFilled"/>.</summary>
    public static readonly BindableProperty ArePointsFilledProperty = BindableProperty.Create(
        nameof(ArePointsFilled), typeof(bool), typeof(CdsScatterChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="PointFillOpacity"/>.</summary>
    public static readonly BindableProperty PointFillOpacityProperty = BindableProperty.Create(
        nameof(PointFillOpacity), typeof(double), typeof(CdsScatterChart), 0.3, propertyChanged: Changed,
        coerceValue: static (_, v) => Math.Clamp((double)v, 0, 1));

    /// <summary>Bindable property for <see cref="IsPointFilled"/>.</summary>
    public static readonly BindableProperty IsPointFilledProperty = BindableProperty.Create(
        nameof(IsPointFilled), typeof(Func<CdsChartPoint, bool>), typeof(CdsScatterChart), null, propertyChanged: Changed);

    /// <summary>Whether the dots are drawn (Carbon's <c>points.enabled</c>). Default <c>true</c>.</summary>
    public bool ShowPoints
    {
        get => (bool)GetValue(ShowPointsProperty);
        set => SetValue(ShowPointsProperty, value);
    }

    /// <summary>The dot radius (Carbon's <c>points.radius</c>). Default 4.</summary>
    public double PointRadius
    {
        get => (double)GetValue(PointRadiusProperty);
        set => SetValue(PointRadiusProperty, value);
    }

    /// <summary>Whether dots fill with the series color at <see cref="PointFillOpacity"/> (Carbon's <c>points.filled</c>); <c>false</c> fills them <c>layer-01</c>. Default <c>true</c>.</summary>
    public bool ArePointsFilled
    {
        get => (bool)GetValue(ArePointsFilledProperty);
        set => SetValue(ArePointsFilledProperty, value);
    }

    /// <summary>The fill opacity of filled dots (Carbon's <c>points.fillOpacity</c>), 0 to 1; a hovered dot fills fully. Default 0.3.</summary>
    public double PointFillOpacity
    {
        get => (double)GetValue(PointFillOpacityProperty);
        set => SetValue(PointFillOpacityProperty, value);
    }

    /// <summary>Decides per point whether its dot is filled (Carbon's <c>getIsFilled</c>), for example to mark outliers; wins over <see cref="ArePointsFilled"/>.</summary>
    public Func<CdsChartPoint, bool>? IsPointFilled
    {
        get => (Func<CdsChartPoint, bool>?)GetValue(IsPointFilledProperty);
        set => SetValue(IsPointFilledProperty, value);
    }

    private protected override string ChartKind => "Scatter chart";

    private protected override bool UsesRuler => true;

    private protected override bool ClipMarks => false;

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var ruler = IsRulerEnabled;
        var rows = ctx.ActivePoints.Where(p => p.Value.HasValue).ToList();
        var columns = CartesianFrame.RulerMarks(scene, ctx, cs, rows);
        var count = 0;
        if (ShowPoints)
        {
            var anomaly = ChartScatter.Anomaly(cs);
            var dotId = columns.Count;
            var opacity = ArePointsFilled ? (float)PointFillOpacity : 1;
            foreach (var d in rows)
            {
                var px = cs.DomainPx(d);
                if (double.IsNaN(px)) continue;
                var center = cs.XY(px, cs.RangePx(d.Group, d.Value!.Value));
                if (float.IsNaN(center.X) || float.IsNaN(center.Y)) continue;
                var filled = IsPointFilled?.Invoke(d) ?? ArePointsFilled;
                var mark = ruler && columns.TryGetValue(cs.DomainKey(d), out var col) ? col : -1;
                var dot = ChartDots.Dot(center, d.Group, (float)PointRadius, filled, opacity, mark);
                if (anomaly?.Invoke(center) == true) dot.StrokeWidth = 3;
                if (!ruler)
                {
                    dot.Mark = dotId;
                    var r = (float)Math.Max(PointRadius + 2, 6);
                    scene.Marks.Add(new ChartMark
                    {
                        Id = dotId++,
                        Bounds = new RectF(center.X - r, center.Y - r, 2 * r, 2 * r),
                        Radius = r,
                        Anchor = center,
                        Group = d.Group,
                        Rows = CartesianFrame.TooltipRows(ctx, cs, [d]),
                        Description = ChartScatter.Describe(ctx, cs, d),
                    });
                }
                scene.Shapes.Add(dot);
                count++;
            }
        }
        else scene.Marks.Clear();
        if (!ruler) scene.Marks.RemoveAll(m => m.RulerAt.HasValue);
        scene.Summary = count == 1 ? "1 point" : $"{count} points";
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsScatterChart)b).RequestRebuild();
}
