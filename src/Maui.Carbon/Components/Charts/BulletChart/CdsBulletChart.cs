namespace Maui.Carbon;

/// <summary>
/// Bars that measure progress toward a target against qualitative ranges (Carbon's bullet chart), one row per measure.
/// Data is <see cref="CdsBulletChartItem"/>s. Each row draws a 16px band of performance areas (shaded from poor to
/// great), an 8px value bar in its group's color, the target line and quartile ticks. Use it for KPIs against goals and
/// on dashboards that need many gauges in little space; normalize measures with different units (for example to % of
/// target) or give each unit its own chart. Grid lines are off. The tooltip and table name the performance area, so it is
/// never read from the shade alone.
/// </summary>
public class CdsBulletChart : CdsCartesianChart
{
    const float Zone = 16;
    const float Bar = 8;
    const float Marker = 24;

    /// <summary>Bindable property for <see cref="PerformanceAreaTitles"/>.</summary>
    public static readonly BindableProperty PerformanceAreaTitlesProperty = BindableProperty.Create(
        nameof(PerformanceAreaTitles), typeof(IList<string>), typeof(CdsBulletChart), null, propertyChanged: Changed,
        defaultValueCreator: static _ => new List<string> { "Poor", "Satisfactory", "Great" });

    /// <summary>Bindable property for <see cref="AreaLegendTitles"/>.</summary>
    public static readonly BindableProperty AreaLegendTitlesProperty = BindableProperty.Create(
        nameof(AreaLegendTitles), typeof(IList<string>), typeof(CdsBulletChart), null, propertyChanged: Changed,
        defaultValueCreator: static _ => new List<string> { "Poor area", "Satisfactory area", "Great area" });

    /// <summary>Bindable property for <see cref="QuartileLegendTitle"/>.</summary>
    public static readonly BindableProperty QuartileLegendTitleProperty = BindableProperty.Create(
        nameof(QuartileLegendTitle), typeof(string), typeof(CdsBulletChart), "Quartiles", propertyChanged: Changed);

    /// <summary>
    /// The names of the performance areas, poor to great (Carbon's <c>bullet.performanceAreaTitles</c>), shown as
    /// "Performance" in the tooltip and table. Default Poor, Satisfactory, Great.
    /// </summary>
    public IList<string>? PerformanceAreaTitles
    {
        get => (IList<string>?)GetValue(PerformanceAreaTitlesProperty);
        set => SetValue(PerformanceAreaTitlesProperty, value);
    }

    /// <summary>
    /// The legend's area swatches, one per performance area (Carbon's area <c>legend.additionalItems</c>). Default
    /// "Poor area", "Satisfactory area", "Great area"; empty hides them.
    /// </summary>
    public IList<string>? AreaLegendTitles
    {
        get => (IList<string>?)GetValue(AreaLegendTitlesProperty);
        set => SetValue(AreaLegendTitlesProperty, value);
    }

    /// <summary>The legend's quartile key (Carbon's quartile <c>legend.additionalItems</c>). Default "Quartiles"; empty hides it.</summary>
    public string? QuartileLegendTitle
    {
        get => (string?)GetValue(QuartileLegendTitleProperty);
        set => SetValue(QuartileLegendTitleProperty, value);
    }

    private protected override string ChartKind => "Bullet chart";

    private protected override bool DefaultXGrid => false;

    private protected override bool DefaultYGrid => false;

    private protected override IReadOnlyDictionary<AxisPosition, AxisSpec> FallbackAxes => new Dictionary<AxisPosition, AxisSpec>
    {
        [AxisPosition.Bottom] = new() { Position = AxisPosition.Bottom, ScaleType = CdsChartScaleType.Linear },
        [AxisPosition.Left] = new() { Position = AxisPosition.Left, ScaleType = CdsChartScaleType.Labels },
    };

    private protected override IReadOnlyList<ChartLegendExtra> LegendExtras
    {
        get
        {
            var list = new List<ChartLegendExtra>();
            if (AreaLegendTitles is { } areas)
                for (var i = 0; i < areas.Count; i++)
                    if (!string.IsNullOrEmpty(areas[i])) list.Add(new ChartLegendExtra(ChartLegendExtraKind.Area, areas[i], Math.Min(i, 2)));
            if (!string.IsNullOrEmpty(QuartileLegendTitle)) list.Add(new ChartLegendExtra(ChartLegendExtraKind.Quartile, QuartileLegendTitle!));
            return list;
        }
    }

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items)
    {
        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
            if (item is CdsBulletChartItem b && seen.Add(b.Group ?? "")) list.Add(b.Group ?? "");
        return list;
    }

    static List<CdsBulletChartItem> ActiveItems(ChartBuildContext ctx) =>
        ctx.Items.OfType<CdsBulletChartItem>().Where(b => ctx.IsActive(b.Group ?? "")).ToList();

    /// <summary>A row on the labels axis: the title is its category.</summary>
    static CdsChartPoint Row(CdsBulletChartItem b) => new() { Key = b.Title ?? b.Group ?? "", Group = b.Group ?? "", Value = b.Value, Tag = b };

    /// <summary>The index of the performance area a value falls in (Carbon's getMatchingRangeIndexForDatapoint).</summary>
    internal static int PerformanceIndex(IList<double>? ranges, double value)
    {
        if (ranges is null) return 0;
        for (var i = ranges.Count - 1; i > 0; i--)
            if (value >= ranges[i]) return i;
        return 0;
    }

    string? Performance(CdsBulletChartItem b)
    {
        var titles = PerformanceAreaTitles;
        var i = PerformanceIndex(b.Ranges, b.Value ?? 0);
        return titles is not null && i < titles.Count ? titles[i] : null;
    }

    static string? Percentage(ChartBuildContext ctx, CdsBulletChartItem b) =>
        b.Target is { } t && t != 0 && b.Value is { } v ? ctx.Number(Math.Floor(v / t * 100)) + "%" : null;

    private protected override CartesianOptions Options(ChartBuildContext ctx)
    {
        var items = ActiveItems(ctx);
        return new CartesianOptions
        {
            Rows = items.Select(Row).ToList(),
            // Carbon's extendLinearDomainBy: 'marker': the value axis reaches the larger of each value and its target.
            RangeValues = _ =>
            {
                var values = new List<double>();
                foreach (var b in items)
                {
                    if (b.Target is { } t) values.Add(Math.Max(b.Value ?? 0, t));
                    if (b.Value is { } v) values.Add(v);
                }
                return values;
            },
        };
    }

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var rows = cs.Rows;
        var s = cs.RangeScale;
        var rStart = s.Continuous!.R0;
        var rEnd = s.Continuous.R1;
        var domMax = s.Domain.Max;
        var zoneStroke = ChartPaint.OfToken(CdsChartToken.ZoneStroke);
        RectF Box(double x0, double x1, double y0, double y1) => ChartBars.Rect(x0, x1, y0, y1, cs.Vertical);
        ChartLine Tick(double x, double half, double y, CdsChartToken token) => new()
        {
            From = cs.XY(x - half, y),
            To = cs.XY(x + half, y),
            Stroke = ChartPaint.OfToken(token),
            StrokeWidth = 1.5f,
            Hover = ChartHoverStyle.None,
        };

        // Performance areas: the whole axis in zone-fill-01, from the second range in 02, from the third in 03.
        foreach (var row in rows)
        {
            var b = (CdsBulletChartItem)row.Tag!;
            var x = cs.DomainPx(row);
            if (double.IsNaN(x)) continue;
            var areas = b.Ranges is { } ranges
                ? ranges.Select((v, j) => (V: (double?)v, Order: j + 1)).Where(r => r.V < domMax).ToList()
                : [(null, 1)];
            foreach (var (v, order) in areas)
            {
                var x0 = x - Zone / 2;
                var rect = order == 1 ? Box(x0, x0 + Zone, rEnd - 2, rStart + 1) : Box(x0, x0 + Zone, s.Map(v!.Value), rEnd);
                var token = order switch { 1 => CdsChartToken.ZoneFill01, 2 => CdsChartToken.ZoneFill02, _ => CdsChartToken.ZoneFill03 };
                scene.Shapes.Add(new ChartRect { Rect = rect, Fill = ChartPaint.OfToken(token), Stroke = zoneStroke, StrokeWidth = 1, Hover = ChartHoverStyle.None });
            }
        }

        var id = 0;
        foreach (var row in rows)
        {
            var b = (CdsBulletChartItem)row.Tag!;
            var x = cs.DomainPx(row);
            if (b.Value is not { } v || double.IsNaN(x)) continue;
            var rect = Box(x - Bar / 2, x + Bar / 2, s.Map(0) + 1, s.Map(v));
            var mark = id++;
            scene.Shapes.Add(new ChartRect { Rect = rect, Fill = ChartPaint.OfSeries(b.Group), Group = b.Group, Mark = mark });
            var tooltip = new List<ChartTooltipRow>
            {
                new(ctx.GroupLabel, CdsChartTruncate.Apply(b.Group ?? ""), ChartPaint.OfSeries(b.Group)),
                new("Value", ctx.Value(v)),
            };
            if (b.Target is { } t) tooltip.Add(new ChartTooltipRow("Target", ctx.Value(t)));
            var pct = Percentage(ctx, b);
            if (pct is not null) tooltip.Add(new ChartTooltipRow("Percentage", pct));
            var perf = Performance(b);
            if (perf is not null) tooltip.Add(new ChartTooltipRow("Performance", perf));
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = Box(x - Zone / 2, x + Zone / 2, s.Map(0), s.Map(v)),
                Anchor = cs.XY(x, s.Map(v)),
                Group = b.Group,
                Rows = tooltip,
                Description = $"{row.Key}: {ctx.Value(v)}"
                    + (b.Target is { } target ? $", target {ctx.Value(target)}" : "")
                    + (pct is null ? "" : $", {pct} of target")
                    + (perf is null ? "" : $", {perf}")
                    + (string.IsNullOrEmpty(b.Group) ? "" : $", {b.Group}"),
            });
        }

        // The target line, then the quartile ticks (25, 50, 75% of the target; 8px where they meet a range), which switch
        // to layer-01-absolute over the bar.
        foreach (var row in rows)
        {
            var b = (CdsBulletChartItem)row.Tag!;
            var x = cs.DomainPx(row);
            if (b.Target is not { } t || double.IsNaN(x)) continue;
            scene.Shapes.Add(Tick(x, Marker / 2, s.Map(t), CdsChartToken.LayerInverseAbsolute));
        }
        foreach (var row in rows)
        {
            var b = (CdsBulletChartItem)row.Tag!;
            var x = cs.DomainPx(row);
            if (b.Target is not { } t || double.IsNaN(x)) continue;
            foreach (var f in new[] { 0.25, 0.5, 0.75 })
            {
                var value = t * f;
                var half = b.Ranges is { } r && r.Contains(value) ? 4 : 2;
                var over = value <= (b.Value ?? 0);
                scene.Shapes.Add(Tick(x, half, s.Map(value), over ? CdsChartToken.Layer01Absolute : CdsChartToken.LayerInverseAbsolute));
            }
        }
        scene.Summary = id == 1 ? "1 bar" : $"{id} bars";
    }

    /// <summary>Carbon's bullet table: Title, Group, Value, Target, Percentage and Performance of every row.</summary>
    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        const string dash = "–";
        var rows = ActiveItems(ctx).Select(b => new[]
        {
            b.Title ?? "",
            b.Group ?? "",
            b.Value is { } v ? ctx.Value(v) : dash,
            b.Target is { } t ? ctx.Value(t) : dash,
            Percentage(ctx, b) ?? dash,
            Performance(b) ?? "",
        }).ToList();
        return new ChartTable(["Title", ctx.GroupLabel, "Value", "Target", "Percentage", "Performance"], rows);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsBulletChart)b).RequestRebuild();
}
