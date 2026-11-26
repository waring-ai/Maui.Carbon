namespace Maui.Carbon;

/// <summary>
/// Dots on thin stems that compare one value per category (Carbon's lollipop chart), a lighter alternative to bars when
/// there are many categories. Stems run from the start of the value axis to the edge of the dot. Hovering the plot draws
/// the dashed ruler at the nearest category, fills its dots, thickens their stems to 2px and lists every group there.
/// A labels axis on the left makes horizontal lollipops. Sort categories by value when ranking is the point; for several
/// groups per category use <see cref="CdsGroupedBarChart"/> instead, since their stems overlap.
/// </summary>
public class CdsLollipopChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="PointRadius"/>.</summary>
    public static readonly BindableProperty PointRadiusProperty = BindableProperty.Create(
        nameof(PointRadius), typeof(double), typeof(CdsLollipopChart), 4d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ArePointsFilled"/>.</summary>
    public static readonly BindableProperty ArePointsFilledProperty = BindableProperty.Create(
        nameof(ArePointsFilled), typeof(bool), typeof(CdsLollipopChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="PointFillOpacity"/>.</summary>
    public static readonly BindableProperty PointFillOpacityProperty = BindableProperty.Create(
        nameof(PointFillOpacity), typeof(double), typeof(CdsLollipopChart), 0.3d, propertyChanged: Changed);

    /// <summary>The dot radius (Carbon's <c>points.radius</c>). Default 4.</summary>
    public double PointRadius
    {
        get => (double)GetValue(PointRadiusProperty);
        set => SetValue(PointRadiusProperty, value);
    }

    /// <summary>Whether dots fill with the group color at <see cref="PointFillOpacity"/> (Carbon's <c>points.filled</c>); unfilled dots fill with <c>layer-01</c>. Default <c>true</c>.</summary>
    public bool ArePointsFilled
    {
        get => (bool)GetValue(ArePointsFilledProperty);
        set => SetValue(ArePointsFilledProperty, value);
    }

    /// <summary>The fill opacity of filled dots (Carbon's <c>points.fillOpacity</c>); hovered dots fill solid. Default 0.3.</summary>
    public double PointFillOpacity
    {
        get => (double)GetValue(PointFillOpacityProperty);
        set => SetValue(PointFillOpacityProperty, value);
    }

    private protected override string ChartKind => "Lollipop chart";

    private protected override bool UsesRuler => true;

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var ruler = IsRulerEnabled;
        var columns = ruler ? CartesianFrame.RulerMarks(scene, ctx, cs, ctx.ActivePoints) : [];
        var r = (float)PointRadius;
        var start = cs.Vertical ? cs.Plot.Bottom : cs.Plot.Left;
        var dotId = columns.Count;
        var count = 0;
        var stems = new List<ChartLine>();
        var dots = new List<ChartCircle>();
        foreach (var d in ctx.ActivePoints)
        {
            if (d.Value is not { } v) continue;
            var dp = cs.DomainPx(d);
            var rp = cs.RangePx(d.Group, v);
            if (double.IsNaN(dp) || double.IsNaN(rp)) continue;
            var center = cs.XY(dp, rp);
            int mark;
            if (ruler) mark = columns.TryGetValue(cs.DomainKey(d), out var col) ? col : -1;
            else
            {
                mark = dotId++;
                scene.Marks.Add(new ChartMark
                {
                    Id = mark,
                    Bounds = new RectF(center.X - r - 2, center.Y - r - 2, 2 * r + 4, 2 * r + 4),
                    Radius = r + 4,
                    Anchor = center,
                    Group = d.Group,
                    Rows = CartesianFrame.TooltipRows(ctx, cs, [d]),
                    Description = $"{cs.DomainText(d)}, {ctx.Value(v)}" + (d.Group is { Length: > 0 } g && g != cs.DomainText(d) ? $", {g}" : ""),
                });
            }
            var end = cs.Vertical ? Math.Min(start, rp + r) : Math.Max(start, rp - r);
            stems.Add(new ChartLine
            {
                From = cs.XY(dp, start),
                To = cs.XY(dp, end),
                Stroke = ChartPaint.OfSeries(d.Group),
                StrokeWidth = 1,
                HoverStrokeWidth = 2,
                Hover = ChartHoverStyle.None,
                Group = d.Group,
                Mark = mark,
            });
            dots.Add(ChartDots.Dot(center, d.Group, r, ArePointsFilled, (float)PointFillOpacity, mark));
            count++;
        }
        scene.Shapes.AddRange(stems);
        scene.Shapes.AddRange(dots);
        scene.Summary = count == 1 ? "1 lollipop" : $"{count} lollipops";
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsLollipopChart)b).RequestRebuild();
}
