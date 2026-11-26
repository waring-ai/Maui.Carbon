namespace Maui.Carbon;

/// <summary>
/// Dots placed by two measures and sized by a third (Carbon's bubble chart): <see cref="CdsChartPoint.X"/> (or a date or
/// category) and <see cref="CdsChartPoint.Value"/> place a bubble, <see cref="CdsChartPoint.Size"/> maps linearly onto its
/// radius, from <c>e * 3 / 400</c> to <c>e * 25 / 400</c> px where <c>e</c> is the smaller side of the graph. Bubbles fill
/// with the series color at 20% with a 1.5px stroke; larger bubbles are drawn first so small ones stay on top, and they are
/// not clipped at the plot edge. The legend ends with a radius item. Radius is hard to read exactly: never encode the only
/// important measure in size, and say what size means in the title or <see cref="RadiusLegendLabel"/>.
/// </summary>
public class CdsBubbleChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="RadiusLabel"/>.</summary>
    public static readonly BindableProperty RadiusLabelProperty = BindableProperty.Create(
        nameof(RadiusLabel), typeof(string), typeof(CdsBubbleChart), "Radius", propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="RadiusLegendLabel"/>.</summary>
    public static readonly BindableProperty RadiusLegendLabelProperty = BindableProperty.Create(
        nameof(RadiusLegendLabel), typeof(string), typeof(CdsBubbleChart), "Radius", propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="MinimumRadius"/>.</summary>
    public static readonly BindableProperty MinimumRadiusProperty = BindableProperty.Create(
        nameof(MinimumRadius), typeof(double?), typeof(CdsBubbleChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="MaximumRadius"/>.</summary>
    public static readonly BindableProperty MaximumRadiusProperty = BindableProperty.Create(
        nameof(MaximumRadius), typeof(double?), typeof(CdsBubbleChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="BubbleFillOpacity"/>.</summary>
    public static readonly BindableProperty BubbleFillOpacityProperty = BindableProperty.Create(
        nameof(BubbleFillOpacity), typeof(double), typeof(CdsBubbleChart), 0.2, propertyChanged: Changed,
        coerceValue: static (_, v) => Math.Clamp((double)v, 0, 1));

    /// <summary>Bindable property for <see cref="ShowBubbles"/>.</summary>
    public static readonly BindableProperty ShowBubblesProperty = BindableProperty.Create(
        nameof(ShowBubbles), typeof(bool), typeof(CdsBubbleChart), true, propertyChanged: Changed);

    /// <summary>What the size means, in the tooltip and the table column (Carbon's <c>bubble.radiusLabel</c>), with its unit ("Revenue (M USD)"). Default "Radius".</summary>
    public string? RadiusLabel
    {
        get => (string?)GetValue(RadiusLabelProperty);
        set => SetValue(RadiusLabelProperty, value);
    }

    /// <summary>The name of the legend's radius item (Carbon's <c>legend.additionalItems</c> radius entry), such as "Engineers". Default "Radius"; empty hides the item.</summary>
    public string? RadiusLegendLabel
    {
        get => (string?)GetValue(RadiusLegendLabelProperty);
        set => SetValue(RadiusLegendLabelProperty, value);
    }

    /// <summary>The radius of the smallest size (Carbon's <c>bubble.radiusRange</c>); <c>null</c> (default) is 3/400 of the graph's smaller side.</summary>
    public double? MinimumRadius
    {
        get => (double?)GetValue(MinimumRadiusProperty);
        set => SetValue(MinimumRadiusProperty, value);
    }

    /// <summary>The radius of the largest size (Carbon's <c>bubble.radiusRange</c>); <c>null</c> (default) is 25/400 of the graph's smaller side.</summary>
    public double? MaximumRadius
    {
        get => (double?)GetValue(MaximumRadiusProperty);
        set => SetValue(MaximumRadiusProperty, value);
    }

    /// <summary>The bubbles' fill opacity (Carbon's <c>bubble.fillOpacity</c>), 0 to 1; a hovered bubble fills fully. Default 0.2.</summary>
    public double BubbleFillOpacity
    {
        get => (double)GetValue(BubbleFillOpacityProperty);
        set => SetValue(BubbleFillOpacityProperty, value);
    }

    /// <summary>Whether the bubbles are drawn (Carbon's <c>bubble.enabled</c>). Default <c>true</c>.</summary>
    public bool ShowBubbles
    {
        get => (bool)GetValue(ShowBubblesProperty);
        set => SetValue(ShowBubblesProperty, value);
    }

    private protected override string ChartKind => "Bubble chart";

    private protected override bool UsesRuler => true;

    private protected override bool ClipMarks => false;

    private protected override IReadOnlyList<ChartLegendExtra> LegendExtras =>
        string.IsNullOrEmpty(RadiusLegendLabel) ? [] : [new ChartLegendExtra(ChartLegendExtraKind.Radius, RadiusLegendLabel!)];

    string SizeLabel => string.IsNullOrEmpty(RadiusLabel) ? "Radius" : RadiusLabel!;

    /// <summary>Carbon's default radius range from the graph size: e * 3 / 400 to e * 25 / 400 with e the smaller side.</summary>
    internal static (double Min, double Max) DefaultRadiusRange(double width, double height)
    {
        var e = Math.Min(width, height);
        return (e * 3 / 400, e * 25 / 400);
    }

    /// <summary>
    /// Carbon's Bubble.getRadiusScale: sizes map linearly from their extent onto the radius range (one size: the middle of
    /// the range); a point without a size gets the smallest radius, and no size anywhere gives 4px bubbles.
    /// </summary>
    internal static Func<double?, double> RadiusScale(IEnumerable<double?> sizes, double r0, double r1)
    {
        var valid = sizes.Where(s => s is { } v && v != 0 && !double.IsNaN(v)).Select(s => s!.Value).ToList();
        if (valid.Count == 0) return _ => 4;
        var min = valid.Min();
        var max = valid.Max();
        return s =>
        {
            if (s is not { } v || v == 0 || double.IsNaN(v)) return Math.Max(0, r0);
            if (max == min) return Math.Max(0, (r0 + r1) / 2);
            return Math.Max(0, r0 + (v - min) / (max - min) * (r1 - r0));
        };
    }

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var ruler = IsRulerEnabled;
        var rows = ctx.ActivePoints.Where(p => p.Value.HasValue).ToList();
        var columns = CartesianFrame.RulerMarks(scene, ctx, cs, rows, 0, pts => pts.Count == 1 ? SingleRows(ctx, cs, pts[0]) : CartesianFrame.TooltipRows(ctx, cs, pts));
        if (!ShowBubbles)
        {
            scene.Marks.Clear();
            scene.Summary = "0 bubbles";
            return;
        }
        var (d0, d1) = DefaultRadiusRange(ctx.Width, ctx.Height);
        var scale = RadiusScale(rows.Select(p => p.Size), MinimumRadius ?? d0, MaximumRadius ?? d1);
        var items = new List<(CdsChartPoint P, PointF Center, float R)>();
        foreach (var d in rows)
        {
            var px = cs.DomainPx(d);
            if (double.IsNaN(px)) continue;
            var center = cs.XY(px, cs.RangePx(d.Group, d.Value!.Value));
            if (float.IsNaN(center.X) || float.IsNaN(center.Y)) continue;
            items.Add((d, center, (float)scale(d.Size)));
        }
        // Largest first so small bubbles stay on top and reachable (Carbon sorts the same way, so its reading order is by size too).
        items = items.OrderByDescending(i => i.R).ToList();
        var opacity = (float)BubbleFillOpacity;
        var id = columns.Count;
        foreach (var (d, center, r) in items)
        {
            var mark = ruler && columns.TryGetValue(cs.DomainKey(d), out var col) ? col : -1;
            var dot = ChartDots.Dot(center, d.Group, r, filled: true, opacity, mark);
            if (!ruler)
            {
                dot.Mark = id;
                var hit = Math.Max(r, 6);
                scene.Marks.Add(new ChartMark
                {
                    Id = id++,
                    Bounds = new RectF(center.X - hit, center.Y - hit, 2 * hit, 2 * hit),
                    Radius = hit,
                    Anchor = center,
                    Group = d.Group,
                    Rows = SingleRows(ctx, cs, d),
                    Description = ChartScatter.Describe(ctx, cs, d, $"{SizeLabel} {ctx.Number(d.Size)}"),
                });
            }
            scene.Shapes.Add(dot);
        }
        if (!ruler) scene.Marks.RemoveAll(m => m.RulerAt.HasValue);
        scene.Summary = items.Count == 1 ? "1 bubble" : $"{items.Count} bubbles";
    }

    /// <summary>A single bubble's tooltip: domain, value, the size under <see cref="RadiusLabel"/>, then the group.</summary>
    List<ChartTooltipRow> SingleRows(ChartBuildContext ctx, CartesianLayout cs, CdsChartPoint d) =>
        CartesianFrame.TooltipRows(ctx, cs, [d], [new ChartTooltipRow(CdsChartTruncate.Apply(SizeLabel), ctx.Number(d.Size))]);

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var table = CartesianFrame.Table(ctx, Layout(ctx));
        var points = ctx.ActivePoints;
        var rows = table.Rows.Select((r, i) => r.Append(points[i].Size is { } s ? ctx.Number(s) : "").ToArray()).ToList();
        return new ChartTable(table.Headers.Append(SizeLabel).ToList(), rows);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsBubbleChart)b).RequestRebuild();
}
