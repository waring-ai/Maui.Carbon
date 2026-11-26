namespace Maui.Carbon;

/// <summary>
/// Slices of one whole, one <see cref="CdsChartPoint"/> per part (<see cref="CdsChartPoint.Group"/> names the slice, its
/// legend item and color; <see cref="CdsChartPoint.Value"/> is its size), with percentage labels outside the pie and
/// callouts for the smallest slices (Carbon's pie chart). Use it for a few parts (ideally 2 to 6) of a meaningful whole;
/// compare parts with a bar chart instead. Rows are sorted largest first, so slices run clockwise from 12 o'clock from
/// largest to smallest; zero and negative values are not drawn. Clicking legend items isolates slices and recomputes the
/// percentages for the shown ones.
/// </summary>
public class CdsPieChart : CdsChart
{
    /// <summary>Bindable property for <see cref="ShowLabels"/>.</summary>
    public static readonly BindableProperty ShowLabelsProperty = BindableProperty.Create(
        nameof(ShowLabels), typeof(bool), typeof(CdsPieChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="LabelFormatter"/>.</summary>
    public static readonly BindableProperty LabelFormatterProperty = BindableProperty.Create(
        nameof(LabelFormatter), typeof(Func<CdsChartPoint, double, string>), typeof(CdsPieChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Alignment"/>.</summary>
    public static readonly BindableProperty AlignmentProperty = BindableProperty.Create(
        nameof(Alignment), typeof(CdsChartAlignment), typeof(CdsPieChart), CdsChartAlignment.Left, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="SortComparison"/>.</summary>
    public static readonly BindableProperty SortComparisonProperty = BindableProperty.Create(
        nameof(SortComparison), typeof(Comparison<CdsChartPoint>), typeof(CdsPieChart), null, propertyChanged: Changed);

    /// <summary>
    /// Whether each slice shows its percentage of the shown total outside the arc (Carbon's <c>pie.labels.enabled</c>).
    /// Default <c>true</c>; keep them on (or the legend) so the chart does not rely on color alone.
    /// </summary>
    public bool ShowLabels
    {
        get => (bool)GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    /// <summary>
    /// Formats the slice labels (Carbon's <c>pie.labels.formatter</c>): receives the point and its exact percentage of the
    /// shown total. Default: the percentage with one decimal, locale formatted, then "%".
    /// </summary>
    public Func<CdsChartPoint, double, string>? LabelFormatter
    {
        get => (Func<CdsChartPoint, double, string>?)GetValue(LabelFormatterProperty);
        set => SetValue(LabelFormatterProperty, value);
    }

    /// <summary>
    /// Where the pie sits across the graph (Carbon's <c>pie.alignment</c>, <c>donut.alignment</c>): 30px from the left edge
    /// (default), centered, or 30px from the right edge.
    /// </summary>
    public CdsChartAlignment Alignment
    {
        get => (CdsChartAlignment)GetValue(AlignmentProperty);
        set => SetValue(AlignmentProperty, value);
    }

    /// <summary>
    /// Orders the slices clockwise from 12 o'clock (Carbon's <c>pie.sortFunction</c>); default largest first. It changes only
    /// the slice order: colors and legend items still follow the values, largest first.
    /// </summary>
    public Comparison<CdsChartPoint>? SortComparison
    {
        get => (Comparison<CdsChartPoint>?)GetValue(SortComparisonProperty);
        set => SetValue(SortComparisonProperty, value);
    }

    private protected override string ChartKind => "Pie chart";

    private protected override ChartSkeleton Skeleton => ChartSkeleton.Pie;

    /// <summary>The groups by value, largest first: colors and legend items follow this order.</summary>
    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) =>
        PieLayout.Sorted(items.OfType<CdsChartPoint>()).Select(p => p.Group ?? "").Distinct(StringComparer.Ordinal).ToList();

    /// <summary>The inner radius of the slices: 2px for a pie.</summary>
    private protected virtual float InnerRadius(float radius) => PieLayout.PieInnerRadius;

    /// <summary>Draws in the middle of the slices (the donut's center figure).</summary>
    private protected virtual void AddCenter(ChartScene scene, ChartBuildContext ctx, PointF center, float radius)
    {
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = new RectF(0, 0, ctx.Width, ctx.Height) };
        var rows = PieLayout.Sorted(ctx.ActivePoints).Where(p => p.Value is > 0).ToList();
        var values = rows.Select(p => p.Value!.Value).ToList();
        var total = values.Sum();
        var labelsOn = ShowLabels;
        var radius = PieLayout.Radius(ctx.Width, ctx.Height, labelsOn);
        var inner = Math.Min(InnerRadius(radius), radius);
        var sort = SortComparison;
        var arcs = PieLayout.Arcs(values, sort is null ? null : (a, b) => sort(rows[a], rows[b]));

        var font = ChartFont.Tick;
        var formatter = LabelFormatter;
        var texts = rows.Select(p => formatter is not null
            ? formatter(p, PieLayout.Percent(p.Value!.Value, total, exact: true)) ?? ""
            : ctx.Number(PieLayout.Percent(p.Value!.Value, total)) + "%").ToList();
        var labels = labelsOn ? PieLayout.Labels(arcs, texts, radius, font) : [];
        var callouts = labels.Count(l => l.Callout is not null);

        var xOffset = labelsOn ? PieLayout.XOffset : 0;
        var yOffset = labelsOn ? PieLayout.YOffset : 0;
        var tx = Alignment switch
        {
            CdsChartAlignment.Center => ctx.Width / 2,
            CdsChartAlignment.Right => ctx.Width - radius - PieLayout.XOffset,
            _ => radius + xOffset,
        };
        var ty = radius + yOffset + (callouts > 0 ? PieLayout.YOffsetCallout : 0);
        // Carbon translates the slices 7px further right than the computed center.
        var center = new PointF(tx + PieLayout.LabelDistance, ty);

        // Slices in angular (reading) order: that is also the data cursor's order.
        foreach (var a in arcs.OrderBy(a => a.Order))
        {
            var p = rows[a.Index];
            var g = p.Group ?? "";
            var mark = a.Order;
            scene.Shapes.Add(new ChartPath
            {
                Path = ChartGeometry.ArcPath(center, inner, radius, a.Start, a.End, PieLayout.PadAngle),
                Fill = ChartPaint.OfSeries(g),
                Group = g,
                Mark = mark,
            });
            // Hovering grows the slice by 3px (Carbon's hover arc) and darkens it 7%.
            scene.Shapes.Add(new ChartPath
            {
                Path = ChartGeometry.ArcPath(center, inner, radius + PieLayout.HoverOffset, a.Start, a.End, PieLayout.PadAngle),
                Fill = ChartPaint.OfSeries(g),
                Group = g,
                Mark = mark,
                HoverOnly = true,
            });
            var pct = PieLayout.Percent(p.Value!.Value, total);
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = new RectF(center.X - radius, center.Y - radius, radius * 2, radius * 2),
                Polygon = ChartGeometry.ArcPolygon(center, inner, radius, a.Start, a.End),
                Anchor = ChartGeometry.Polar(center, (inner + radius) / 2, a.Mid),
                Group = g,
                Rows = [new ChartTooltipRow(CdsChartTruncate.Apply(g), ctx.Value(p.Value))],
                Description = $"{ctx.Value(p.Value)}, {ctx.Number(pct)}%, {g}",
            });
        }

        var stroke = ChartPaint.OfKey(CdsColorKeys.TextSecondary);
        foreach (var l in labels)
        {
            scene.Texts.Add(RoundChartText.At(l.Text, center.X + l.At.X, center.Y + l.At.Y, font, ChartTextAnchor.Middle));
            if (l.Callout is not { } c) continue;
            scene.Shapes.Add(new ChartLine { From = Offset(c.Start, center), To = Offset(c.Bend, center), Stroke = stroke });
            scene.Shapes.Add(new ChartLine { From = Offset(c.Bend, center), To = Offset(c.End, center), Stroke = stroke });
        }

        scene.Summary = arcs.Length == 1 ? "1 slice" : $"{arcs.Length} slices";
        AddCenter(scene, ctx, center, radius);
        return scene;
    }

    /// <summary>Carbon's pie table: Group and Value, largest first.</summary>
    private protected override ChartTable BuildTable(ChartBuildContext ctx) => new(
        [ctx.GroupLabel, "Value"],
        PieLayout.Sorted(ctx.ActivePoints).Select(p => new[] { p.Group ?? "", p.Value is null ? "–" : ctx.Value(p.Value) }).ToList());

    static PointF Offset(PointF p, PointF by) => new(p.X + by.X, p.Y + by.Y);

    static void Changed(BindableObject b, object? o, object? n) => ((CdsPieChart)b).RequestRebuild();
}
