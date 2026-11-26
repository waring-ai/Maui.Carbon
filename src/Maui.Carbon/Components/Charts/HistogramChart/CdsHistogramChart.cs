namespace Maui.Carbon;

/// <summary>
/// Bars that count how many observations fall into each value range (Carbon's histogram), with groups stacked inside each
/// bin. Give one <see cref="CdsChartPoint"/> per observation: its <see cref="CdsChartPoint.Value"/> is binned and its
/// <see cref="CdsChartPoint.Group"/> is its series. Use it for the shape of a distribution (ages, response times, order
/// values) from about 30 observations up; for values already counted per category use <see cref="CdsSimpleBarChart"/>,
/// and to compare the spread of many groups <see cref="CdsBoxplotChart"/>. The bottom axis is the binned value axis and
/// the left axis the count: title the first with its unit and the second with what is counted. On a bottom axis of your
/// own, set <see cref="CdsChartAxis.IncludeZero"/> to <c>false</c> so the axis runs exactly over the bins.
/// </summary>
public class CdsHistogramChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="Bins"/>.</summary>
    public static readonly BindableProperty BinsProperty = BindableProperty.Create(
        nameof(Bins), typeof(int), typeof(CdsHistogramChart), 10, propertyChanged: Changed, coerceValue: static (_, v) => Math.Max(1, (int)v));

    /// <summary>Bindable property for <see cref="BinThresholds"/>.</summary>
    public static readonly BindableProperty BinThresholdsProperty = BindableProperty.Create(
        nameof(BinThresholds), typeof(IList<double>), typeof(CdsHistogramChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="LimitDomainToBins"/>.</summary>
    public static readonly BindableProperty LimitDomainToBinsProperty = BindableProperty.Create(
        nameof(LimitDomainToBins), typeof(bool), typeof(CdsHistogramChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="RangeLabel"/>.</summary>
    public static readonly BindableProperty RangeLabelProperty = BindableProperty.Create(
        nameof(RangeLabel), typeof(string), typeof(CdsHistogramChart), "Range", propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="BarDividerSize"/>.</summary>
    public static readonly BindableProperty BarDividerSizeProperty = BindableProperty.Create(
        nameof(BarDividerSize), typeof(double), typeof(CdsHistogramChart), 1.5d, propertyChanged: Changed);

    (ChartBuildContext Ctx, ChartBins.Result<CdsChartPoint> Bins)? _cache;

    /// <summary>
    /// How many bins to aim for (Carbon's <c>bins</c> as a number): d3 picks that many nice thresholds over the data and
    /// widens the last bin to the common width. Default 10. Ignored when <see cref="BinThresholds"/> is set.
    /// </summary>
    public int Bins
    {
        get => (int)GetValue(BinsProperty);
        set => SetValue(BinsProperty, value);
    }

    /// <summary>
    /// Exact bin thresholds in ascending order (Carbon's <c>bins</c> as an array), such as fixed price bands
    /// [0, 25, 50, 100]; values below the first or above the last are left out. <c>null</c> (default) uses <see cref="Bins"/>.
    /// </summary>
    public IList<double>? BinThresholds
    {
        get => (IList<double>?)GetValue(BinThresholdsProperty);
        set => SetValue(BinThresholdsProperty, value);
    }

    /// <summary>Whether the binned axis runs exactly over the bins (Carbon's <c>limitDomainToBins</c>), without headroom. Default <c>true</c>.</summary>
    public bool LimitDomainToBins
    {
        get => (bool)GetValue(LimitDomainToBinsProperty);
        set => SetValue(LimitDomainToBinsProperty, value);
    }

    /// <summary>The tooltip and table label of a bin's range (Carbon's <c>bins.rangeLabel</c>), such as "Ages" or "Price band". Default "Range".</summary>
    public string? RangeLabel
    {
        get => (string?)GetValue(RangeLabelProperty);
        set => SetValue(RangeLabelProperty, value);
    }

    /// <summary>Stacked segments taller than this lose 1px at the top, separating the groups of a bin (Carbon's <c>bars.dividerSize</c>). Default 1.5.</summary>
    public double BarDividerSize
    {
        get => (double)GetValue(BarDividerSizeProperty);
        set => SetValue(BarDividerSizeProperty, value);
    }

    private protected override string ChartKind => "Histogram";

    private protected override int DefaultTimeEdgeSpace => 0;

    private protected override IReadOnlyDictionary<AxisPosition, AxisSpec> FallbackAxes => new Dictionary<AxisPosition, AxisSpec>
    {
        [AxisPosition.Bottom] = new() { Position = AxisPosition.Bottom, ScaleType = CdsChartScaleType.Linear, IncludeZero = !LimitDomainToBins },
        [AxisPosition.Left] = new() { Position = AxisPosition.Left, ScaleType = CdsChartScaleType.Linear },
    };

    string Label => string.IsNullOrEmpty(RangeLabel) ? "Range" : RangeLabel!;

    ChartBins.Result<CdsChartPoint> BinsOf(ChartBuildContext ctx)
    {
        if (_cache is { } c && ReferenceEquals(c.Ctx, ctx)) return c.Bins;
        var result = ChartBins.Compute(ctx.ActivePoints, p => p.Value, Bins, BinThresholds?.ToList());
        _cache = (ctx, result);
        return result;
    }

    /// <summary>A pre-padding extent that the axis' 10% headroom turns back into exactly [lo, hi] (Carbon gives binned axes an exact domain).</summary>
    internal static (double Lo, double Hi) ExactExtent(double lo, double hi)
    {
        if (!(hi > lo)) hi = lo + 1;
        var w = (hi - lo) / 1.2;
        return (lo + 0.1 * w, hi - 0.1 * w);
    }

    private protected override CartesianOptions Options(ChartBuildContext ctx)
    {
        var bins = BinsOf(ctx);
        List<CdsChartPoint> rows;
        if (LimitDomainToBins)
        {
            var (lo, hi) = ExactExtent(bins.Lo, bins.Hi);
            rows = [new CdsChartPoint { X = lo }, new CdsChartPoint { X = hi }];
        }
        else rows = ctx.ActivePoints.Where(p => p.Value.HasValue).Select(p => new CdsChartPoint { X = p.Value }).ToList();
        var max = Math.Max(1, bins.Bins.Select(b => b.Items.Count).DefaultIfEmpty(0).Max());
        // The count axis runs exactly from 0 to the largest bin: the axis adds 10% to max / 1.1.
        return new CartesianOptions { Rows = rows, RangeValues = _ => [0, max / 1.1] };
    }

    /// <summary>Per bin: its range, the count per group and the stacked segments of the active groups.</summary>
    internal static List<(double X0, double X1, Dictionary<string, int> Counts, int Total, List<(string Group, int V0, int V1)> Segments)> Stacks(
        ChartBuildContext ctx, IReadOnlyList<ChartBin<CdsChartPoint>> bins)
    {
        var list = new List<(double, double, Dictionary<string, int>, int, List<(string, int, int)>)>();
        foreach (var b in bins)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var g in ctx.Groups) counts[g] = 0;
            foreach (var p in b.Items) counts[p.Group ?? ""] = counts.GetValueOrDefault(p.Group ?? "") + 1;
            var acc = 0;
            var segs = new List<(string, int, int)>();
            foreach (var g in ctx.ActiveGroups)
            {
                var v0 = acc;
                acc += counts.GetValueOrDefault(g);
                segs.Add((g, v0, acc));
            }
            list.Add((b.X0, b.X1, counts, acc, segs));
        }
        return list;
    }

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var stacks = Stacks(ctx, BinsOf(ctx).Bins);
        var x = cs.DomainScale;
        var y = cs.RangeScale;
        var p = cs.Plot;
        var groupCount = ctx.ActiveGroups.Count;
        var ruler = IsRulerEnabled;
        var id = 0;
        foreach (var s in stacks)
        {
            var range = ctx.Number(s.X0) + " – " + ctx.Number(s.X1);
            var left = x.Map(s.X0);
            var right = x.Map(s.X1);
            if (double.IsNaN(left) || double.IsNaN(right)) continue;
            var colId = -1;
            if (ruler)
            {
                var present = ctx.ActiveGroups.Reverse().Where(g => s.Counts.GetValueOrDefault(g) != 0).ToList();
                if (present.Count > 0)
                {
                    colId = id++;
                    var rows = new List<ChartTooltipRow> { new(Label, range) };
                    rows.AddRange(present.Select(g => new ChartTooltipRow(CdsChartTruncate.Apply(g), ctx.Value(s.Counts[g]), ChartPaint.OfSeries(g))));
                    if (ctx.ShowTotal) rows.Add(new ChartTooltipRow(ctx.TotalLabel, ctx.Value(s.Total), Bold: true));
                    var at = (float)x.Map((s.X0 + s.X1) / 2);
                    scene.Marks.Add(new ChartMark
                    {
                        Id = colId,
                        Bounds = cs.Vertical
                            ? new RectF((float)Math.Min(left, right), p.Top, (float)Math.Abs(right - left), p.Height)
                            : new RectF(p.Left, (float)Math.Min(left, right), p.Width, (float)Math.Abs(right - left)),
                        RulerAt = at,
                        Anchor = cs.XY(at, y.Map(s.Total)),
                        Rows = rows,
                        Description = $"{range}, " + string.Join(", ", present.AsEnumerable().Reverse().Select(g => $"{g} {ctx.Number(s.Counts[g])}"))
                            + (ctx.ShowTotal && present.Count > 1 ? $", {ctx.TotalLabel} {ctx.Number(s.Total)}" : ""),
                    });
                }
            }
            var width = right - left - 1;
            foreach (var (g, v0, v1) in s.Segments)
            {
                if (v0 == v1) continue;
                var y0 = y.Map(v0);
                var y1 = ChartStackedBars.DividedEnd(y0, y.Map(v1), v0, v1, cs.Vertical, BarDividerSize, groupCount);
                var rect = ChartBars.Rect(left, left + width, y0, y1, cs.Vertical);
                var mark = id++;
                scene.Shapes.Add(new ChartRect { Rect = rect, Fill = ChartPaint.OfSeries(g), Group = g, Mark = mark });
                if (colId >= 0) scene.Shapes.Add(new ChartRect { Rect = rect, Fill = ChartPaint.OfSeries(g), Group = g, Mark = colId, HoverOnly = true });
                var count = s.Counts[g];
                scene.Marks.Add(new ChartMark
                {
                    Id = mark,
                    Bounds = rect,
                    Group = g,
                    Rows =
                    [
                        new ChartTooltipRow(Label, range),
                        new ChartTooltipRow(ctx.GroupLabel, CdsChartTruncate.Apply(g), ChartPaint.OfSeries(g)),
                        new ChartTooltipRow(CdsChartTruncate.Apply(cs.RangeLabel(g)), ctx.Value(count)),
                    ],
                    Description = $"{range}, {g}, {ctx.Number(count)}",
                });
            }
        }
        scene.Summary = stacks.Count == 1 ? "1 bin" : $"{stacks.Count} bins";
    }

    /// <summary>Carbon's histogram table: each bin's range, then the count per active group.</summary>
    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var stacks = Stacks(ctx, BinsOf(ctx).Bins);
        var headers = new List<string> { Label };
        headers.AddRange(ctx.ActiveGroups);
        var rows = stacks.Select(s =>
        {
            var row = new List<string> { ctx.Number(s.X0) + " – " + ctx.Number(s.X1) };
            row.AddRange(ctx.ActiveGroups.Select(g => ctx.Number(s.Counts.GetValueOrDefault(g))));
            return row.ToArray();
        }).ToList();
        return new ChartTable(headers, rows);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsHistogramChart)b).RequestRebuild();
}
