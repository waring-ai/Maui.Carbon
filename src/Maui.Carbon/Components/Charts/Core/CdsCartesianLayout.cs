namespace Maui.Carbon;

internal enum AxisPosition
{
    Left,
    Bottom,
    Right,
    Top,
}

/// <summary>A snapshot of one configured axis.</summary>
internal sealed class AxisSpec
{
    public AxisPosition Position { get; init; }
    public CdsChartScaleType ScaleType { get; init; }
    public string? Title { get; init; }
    public bool IsVisible { get; init; } = true;
    public bool IncludeZero { get; init; } = true;
    public double? Minimum { get; init; }
    public double? Maximum { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public IReadOnlyList<string>? Categories { get; init; }
    public int? TickCount { get; init; }
    public IReadOnlyList<double>? TickValues { get; init; }
    public string? TickFormat { get; init; }
    public Func<object, int, string>? TickFormatter { get; init; }
    public CdsChartTickRotation TickRotation { get; init; }
    public CdsChartTruncation Truncation { get; init; }
    public int TruncationThreshold { get; init; } = 16;
    public int TruncationLength { get; init; } = 14;
    public bool IsPercentage { get; init; }
    public bool? IsStacked { get; init; }
    public IReadOnlyList<string> CorrespondingDatasets { get; init; } = [];
    public bool IsMain { get; init; }
    public double LogBase { get; init; } = 10;
    public IReadOnlyList<CdsChartThreshold> Thresholds { get; init; } = [];

    public bool IsVertical => Position is AxisPosition.Left or AxisPosition.Right;

    public static AxisSpec From(AxisPosition position, CdsChartAxis a) => new()
    {
        Position = position,
        ScaleType = a.ScaleType,
        Title = a.Title,
        IsVisible = a.IsVisible,
        IncludeZero = a.IncludeZero,
        Minimum = a.Minimum,
        Maximum = a.Maximum,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        Categories = a.Categories?.ToList(),
        TickCount = a.TickCount,
        TickValues = a.TickValues?.ToList(),
        TickFormat = a.TickFormat,
        TickFormatter = a.TickFormatter,
        TickRotation = a.TickRotation,
        Truncation = a.Truncation,
        TruncationThreshold = a.TruncationThreshold,
        TruncationLength = a.TruncationLength,
        IsPercentage = a.IsPercentage,
        IsStacked = a.IsStacked,
        CorrespondingDatasets = a.CorrespondingDatasets?.ToList() ?? [],
        IsMain = a.IsMain,
        LogBase = a.LogBase,
        Thresholds = a.Thresholds.ToList(),
    };
}

/// <summary>The scale of one axis: a band scale for labels, else a continuous scale.</summary>
internal sealed class AxisScale
{
    public AxisScale(AxisSpec spec, BandScale band)
    {
        Spec = spec;
        Band = band;
    }

    public AxisScale(AxisSpec spec, ContinuousScale continuous)
    {
        Spec = spec;
        Continuous = continuous;
    }

    public AxisSpec Spec { get; }

    public BandScale? Band { get; }

    public ContinuousScale? Continuous { get; }

    public bool IsBand => Band is not null;

    public void SetRange(double r0, double r1)
    {
        if (Band is not null) Band.SetRange(r0, r1);
        else
        {
            Continuous!.R0 = r0;
            Continuous.R1 = r1;
        }
    }

    /// <summary>The pixel of a continuous value (a date as milliseconds).</summary>
    public double Map(double v) => Continuous?.Map(v) ?? double.NaN;

    /// <summary>The domain ends of a continuous scale.</summary>
    public (double Min, double Max) Domain => Continuous is { } c ? (Math.Min(c.D0, c.D1), Math.Max(c.D0, c.D1)) : (0, 0);
}

/// <summary>A tick of an axis: its position value and label.</summary>
internal readonly record struct AxisTick(double Pixel, string Label, bool Primary);

/// <summary>Cartesian options a chart contributes to the layout.</summary>
internal sealed class CartesianOptions
{
    /// <summary>Stacked charts take the range domain from the stacked sums.</summary>
    public bool Stacked { get; init; }

    /// <summary>A custom range extent (histogram bins, boxplot whiskers, bullet ranges).</summary>
    public Func<IReadOnlyList<CdsChartPoint>, IEnumerable<double>>? RangeValues { get; init; }

    /// <summary>A custom labels domain (histogram bins).</summary>
    public Func<IReadOnlyList<CdsChartPoint>, IReadOnlyList<string>>? DomainCategories { get; init; }

    /// <summary>The rows the domains come from (default the active points).</summary>
    public IReadOnlyList<CdsChartPoint>? Rows { get; init; }

    /// <summary>Time edge padding in ticks (Carbon's <c>timeScale.addSpaceOnEdges</c>).</summary>
    public int TimeEdgeSpace { get; init; } = 1;

    public CdsChartTimeInterval TimeInterval { get; init; }

    public bool ShowDayName { get; init; }
}

/// <summary>
/// The cartesian frame of an axis chart at one size (Carbon's cartesian scales service and axes): which axis is the
/// domain and which the range, their scales, ticks and margins, and how a point maps to pixels.
/// </summary>
internal sealed class CartesianLayout
{
    const float TickHeight = 16;
    const float TickPadding = 9;

    /// <summary>The least room between two flat tick labels.</summary>
    const float TickLabelGap = 8;
    const double PaddingRatio = 0.1;

    readonly ChartBuildContext _ctx;
    readonly CartesianOptions _options;
    readonly HashSet<string> _secondaryGroups;

    public CartesianLayout(ChartBuildContext ctx, IReadOnlyDictionary<AxisPosition, AxisSpec> axes, CartesianOptions options)
    {
        _ctx = ctx;
        _options = options;
        Axes = axes;
        Rows = options.Rows ?? ctx.ActivePoints;

        var (vPrimary, vSecondary) = Pick(AxisPosition.Left, AxisPosition.Right);
        var (hPrimary, hSecondary) = Pick(AxisPosition.Bottom, AxisPosition.Top);
        var vType = vPrimary is { } vp ? axes[vp].ScaleType : CdsChartScaleType.Linear;
        var hType = hPrimary is { } hp ? axes[hp].ScaleType : CdsChartScaleType.Linear;
        DomainPosition = hPrimary ?? AxisPosition.Bottom;
        RangePosition = vPrimary ?? AxisPosition.Left;
        SecondaryRange = vSecondary;
        if ((hType is not (CdsChartScaleType.Labels or CdsChartScaleType.Time) && vType == CdsChartScaleType.Labels) || vType == CdsChartScaleType.Time)
        {
            DomainPosition = vPrimary ?? AxisPosition.Left;
            RangePosition = hPrimary ?? AxisPosition.Bottom;
            SecondaryRange = hSecondary;
        }
        Vertical = RangePosition is AxisPosition.Left or AxisPosition.Right;
        _secondaryGroups = SecondaryRange is { } sr && axes.TryGetValue(sr, out var srSpec) && srSpec.CorrespondingDatasets.Count > 0
            ? new HashSet<string>(srSpec.CorrespondingDatasets, StringComparer.Ordinal)
            : [];
        IsDual = _secondaryGroups.Count > 0;

        foreach (var (pos, spec) in axes) Scales[pos] = MakeScale(spec);
        Measure(ctx.Width, ctx.Height);
    }

    public IReadOnlyDictionary<AxisPosition, AxisSpec> Axes { get; }

    public Dictionary<AxisPosition, AxisScale> Scales { get; } = [];

    public Dictionary<AxisPosition, IReadOnlyList<AxisTick>> Ticks { get; } = [];

    public Dictionary<AxisPosition, bool> Rotated { get; } = [];

    public IReadOnlyList<CdsChartPoint> Rows { get; }

    public AxisPosition DomainPosition { get; }

    public AxisPosition RangePosition { get; }

    public AxisPosition? SecondaryRange { get; }

    public bool IsDual { get; }

    /// <summary>True when the range (value) axis is vertical: columns. False: horizontal bars.</summary>
    public bool Vertical { get; }

    public RectF Plot { get; private set; }

    public AxisSpec DomainAxis => Axes[DomainPosition];

    public AxisScale DomainScale => Scales[DomainPosition];

    public AxisScale RangeScale => Scales[RangePosition];

    public CdsChartScaleType DomainType => DomainAxis.ScaleType;

    public bool IsBand => DomainScale.IsBand;

    public double Bandwidth => DomainScale.Band?.Bandwidth ?? 0;

    public double Step => DomainScale.Band?.Step ?? 0;

    public bool IsSecondary(string? group) => IsDual && group is not null && _secondaryGroups.Contains(group);

    public AxisPosition RangePositionOf(string? group) => IsSecondary(group) ? SecondaryRange!.Value : RangePosition;

    public AxisScale ScaleFor(string? group) => Scales[RangePositionOf(group)];

    /// <summary>The category of a point on a labels axis: its <see cref="CdsChartPoint.Key"/>, else its group (Carbon's <c>mapsTo: 'group'</c>).</summary>
    public static string? CategoryOf(CdsChartPoint p) => p.Key ?? p.Group;

    /// <summary>The domain value of a point as a number (category index is not used: bands map by key).</summary>
    public double DomainNumber(CdsChartPoint p) => DomainType switch
    {
        CdsChartScaleType.Time => p.Date is { } d ? ChartTime.ToMs(d) : double.NaN,
        CdsChartScaleType.Labels => double.NaN,
        _ => p.X ?? double.NaN,
    };

    /// <summary>The pixel of a point along the domain axis (a band's center for labels).</summary>
    public double DomainPx(CdsChartPoint p) => IsBand ? DomainScale.Band!.Center(CategoryOf(p)) : DomainScale.Map(DomainNumber(p));

    /// <summary>The pixel of a range value for a point's group.</summary>
    public double RangePx(string? group, double value) => ScaleFor(group).Map(value);

    /// <summary>(x, y) from domain and range pixels.</summary>
    public PointF XY(double domainPx, double rangePx) => Vertical ? new((float)domainPx, (float)rangePx) : new((float)rangePx, (float)domainPx);

    /// <summary>A stable key of a point's domain value (stacking, grouping into ruler columns).</summary>
    public string DomainKey(CdsChartPoint p) => DomainType switch
    {
        CdsChartScaleType.Labels => CategoryOf(p) ?? "",
        CdsChartScaleType.Time => p.Date?.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "",
        _ => (p.X ?? double.NaN).ToString("R", System.Globalization.CultureInfo.InvariantCulture),
    };

    /// <summary>The domain value of a point as tooltip and table text.</summary>
    public string DomainText(CdsChartPoint p) => DomainType switch
    {
        CdsChartScaleType.Labels => CategoryOf(p) ?? "",
        CdsChartScaleType.Time => p.Date is { } d ? _ctx.Date(d) : "",
        _ => _ctx.Number(p.X),
    };

    public string DomainLabel => AxisLabel(DomainPosition);

    public string RangeLabel(string? group) => AxisLabel(RangePositionOf(group));

    string AxisLabel(AxisPosition pos) => Axes.TryGetValue(pos, out var a) && !string.IsNullOrEmpty(a.Title)
        ? a.Title!
        : pos is AxisPosition.Bottom or AxisPosition.Top ? "x-value" : "y-value";

    /// <summary>The baseline pixel of bars on a group's range scale: max(0, domain start).</summary>
    public double Baseline(string? group)
    {
        var s = ScaleFor(group);
        var (min, _) = s.Domain;
        return s.Map(Math.Max(0, min));
    }

    // ----- axes and domains -----

    (AxisPosition? Primary, AxisPosition? Secondary) Pick(AxisPosition a, AxisPosition b)
    {
        var hasA = Axes.ContainsKey(a);
        var hasB = Axes.ContainsKey(b);
        if (hasA && hasB) return Axes[b].IsMain && !Axes[a].IsMain ? (b, a) : (a, b);
        return (hasA ? a : hasB ? b : null, null);
    }

    bool IsRangeAxis(AxisPosition pos) => pos == RangePosition || pos == SecondaryRange;

    AxisScale MakeScale(AxisSpec a)
    {
        var rows = Rows;
        if (IsDual && a.Position == RangePosition) rows = Rows.Where(p => !IsSecondary(p.Group)).ToList();
        if (IsDual && a.Position == SecondaryRange) rows = Rows.Where(p => IsSecondary(p.Group)).ToList();

        if (a.ScaleType == CdsChartScaleType.Labels)
        {
            IReadOnlyList<string> domain = a.Categories
                ?? (!IsRangeAxis(a.Position) && _options.DomainCategories is { } custom ? custom(rows)
                : rows.Select(CategoryOf).Where(k => k is not null).Distinct().ToList()!);
            return new AxisScale(a, new BandScale(domain));
        }

        if (a.ScaleType == CdsChartScaleType.Time)
        {
            double lo, hi;
            if (a.StartDate is not null || a.EndDate is not null)
            {
                var dates = rows.Where(p => p.Date.HasValue).Select(p => ChartTime.ToMs(p.Date!.Value)).ToList();
                lo = a.StartDate is { } s ? ChartTime.ToMs(s) : dates.DefaultIfEmpty(0).Min();
                hi = a.EndDate is { } e ? ChartTime.ToMs(e) : dates.DefaultIfEmpty(0).Max();
            }
            else
            {
                var values = rows.Where(p => p.Date.HasValue).Select(p => ChartTime.ToMs(p.Date!.Value)).ToList();
                foreach (var t in a.Thresholds) if (t.Date is { } td) values.Add(ChartTime.ToMs(td));
                if (values.Count == 0) values.AddRange([ChartTime.ToMs(DateTime.Today), ChartTime.ToMs(DateTime.Today.AddDays(1))]);
                lo = values.Min();
                hi = values.Max();
            }
            var (d0, d1) = SpaceTimeDomain(ChartTime.FromMs(lo), ChartTime.FromMs(hi), _options.TimeEdgeSpace);
            return new AxisScale(a, new ContinuousScale(CdsChartScaleType.Time, ChartTime.ToMs(d0), ChartTime.ToMs(d1)));
        }

        var type = a.ScaleType;
        if (a.IsPercentage) return new AxisScale(a, new ContinuousScale(type, 0, 100, a.LogBase));
        var list = new List<double>();
        var isRange = IsRangeAxis(a.Position);
        if (isRange && _options.RangeValues is { } rv) list.AddRange(rv(rows));
        else if (isRange && _options.Stacked && a.IsStacked != false)
        {
            foreach (var column in Stack(rows, _ctx.ActiveGroups, false).Values)
                foreach (var (y0, y1, _) in column.Values)
                {
                    list.Add(y0);
                    list.Add(y1);
                }
        }
        else if (isRange)
        {
            foreach (var p in rows)
            {
                if (p.HasRange)
                {
                    list.Add(p.Low!.Value);
                    list.Add(p.High!.Value);
                }
                else if (p.Value is { } v) list.Add(v);
            }
        }
        else
        {
            foreach (var p in rows) if (p.X is { } x) list.Add(x);
        }
        list.RemoveAll(double.IsNaN);
        if (type != CdsChartScaleType.Log && a.IncludeZero) list.Add(0);
        foreach (var t in a.Thresholds) if (t.Value is { } tv) list.Add(tv);
        if (type == CdsChartScaleType.Log) list.RemoveAll(v => v <= 0);
        double min = list.Count > 0 ? list.Min() : 0, max = list.Count > 0 ? list.Max() : 1;
        if (a.Minimum is { } amin) min = amin;
        if (a.Maximum is { } amax) max = amax;
        if (type == CdsChartScaleType.Linear && min == 0 && max == 0) max = 1;
        var (lo2, hi2) = SpaceContinuousDomain(min, max, type);
        return new AxisScale(a, new ContinuousScale(type, lo2, hi2, a.LogBase));
    }

    /// <summary>Carbon's addSpacingToContinuousDomain: 10% headroom, never crossing zero.</summary>
    internal static (double, double) SpaceContinuousDomain(double lo, double hi, CdsChartScaleType type)
    {
        var pad = (hi - lo) * PaddingRatio;
        var up = hi <= 0 && hi + pad > 0 ? 0 : hi + pad;
        var low = lo >= 0 && lo - pad < 0 ? 0 : lo - pad;
        if (type == CdsChartScaleType.Log && low <= 0) low = lo;
        return (low, up);
    }

    /// <summary>Carbon's addSpacingToTimeDomain: pads by <paramref name="n"/> of the unit the range spans.</summary>
    internal static (DateTime, DateTime) SpaceTimeDomain(DateTime start, DateTime end, int n)
    {
        if (n == 0) return (start, end);
        var ms = (end - start).TotalMilliseconds;
        var years = end.Year - start.Year - (end < new DateTime(end.Year, start.Month, Math.Min(start.Day, DateTime.DaysInMonth(end.Year, start.Month))) ? 1 : 0);
        var months = years * 12 + end.Month - start.Month;
        if (years > 1) return (start.AddYears(-n), end.AddYears(n));
        if (months > 1) return (start.AddMonths(-n), end.AddMonths(n));
        if (ms / 864e5 > 1) return (start.AddDays(-n), end.AddDays(n));
        if (ms / 36e5 > 1) return (start.AddHours(-n), end.AddHours(n));
        if (ms / 6e4 > 30) return (start.AddMinutes(-n * 30), end.AddMinutes(n * 30));
        if (ms / 6e4 > 1) return (start.AddMinutes(-n), end.AddMinutes(n));
        if (ms / 1e3 > 15) return (start.AddSeconds(-n * 15), end.AddSeconds(n * 15));
        if (ms / 1e3 > 1) return (start.AddSeconds(-n), end.AddSeconds(n));
        return (start, end);
    }

    /// <summary>
    /// Carbon's stacked data: for each domain value, every group's [y0, y1] with positives stacking up and negatives down,
    /// optionally as percentages of the column's absolute total.
    /// </summary>
    public Dictionary<string, Dictionary<string, (double Y0, double Y1, CdsChartPoint Point)>> Stack(IReadOnlyList<CdsChartPoint> rows, IReadOnlyList<string> groups, bool percentage)
    {
        var byDomain = new Dictionary<string, List<CdsChartPoint>>();
        var order = new List<string>();
        foreach (var p in rows)
        {
            var k = DomainKey(p);
            if (!byDomain.TryGetValue(k, out var l))
            {
                byDomain[k] = l = [];
                order.Add(k);
            }
            l.Add(p);
        }
        var result = new Dictionary<string, Dictionary<string, (double, double, CdsChartPoint)>>();
        foreach (var k in order)
        {
            var list = byDomain[k];
            var total = percentage ? list.Sum(p => Math.Abs(p.Value ?? 0)) : 1;
            if (total == 0) total = 1;
            double pos = 0, neg = 0;
            var column = new Dictionary<string, (double, double, CdsChartPoint)>(StringComparer.Ordinal);
            foreach (var g in groups)
            {
                var p = list.FirstOrDefault(x => (x.Group ?? "") == g);
                if (p is null) continue;
                var raw = p.Value ?? 0;
                var v = percentage ? raw / total * 100 : raw;
                if (v >= 0)
                {
                    column[g] = (pos, pos + v, p);
                    pos += v;
                }
                else
                {
                    column[g] = (neg + v, neg, p);
                    neg += v;
                }
            }
            result[k] = column;
        }
        return result;
    }

    // ----- ticks and margins -----

    static int FittingTicks(double size, double ratio) => Math.Max((int)Math.Floor(size / (TickHeight * ratio)), 2);

    (List<double> Values, List<string> Labels, List<bool> Primary, List<string>? Categories) TickSet(AxisPosition pos, double length, bool rotated)
    {
        var a = Axes[pos];
        var s = Scales[pos];
        if (s.Band is { } band)
        {
            var cats = band.Domain.ToList();
            var labels = cats.Select((c, i) => a.TickFormatter is { } f ? f(c, i) : CdsChartTruncate.Apply(c, a.Truncation, a.TruncationThreshold, a.TruncationLength)).ToList();
            return ([], labels, cats.Select(_ => false).ToList(), cats);
        }
        var c = s.Continuous!;
        List<double> values;
        var (min, max) = s.Domain;
        if (a.TickValues is { } explicitValues) values = explicitValues.Where(v => v >= min - 1e-9 && v <= max + 1e-9).ToList();
        else
        {
            var n = a.TickCount ?? (a.IsVertical ? FittingTicks(length, 2.5) : rotated ? FittingTicks(length, 3.5) : 7);
            if (a.ScaleType == CdsChartScaleType.Time)
            {
                var t = new ContinuousScale(CdsChartScaleType.Time, c.D0, c.D1);
                var custom = a.StartDate is not null || a.EndDate is not null;
                if (_options.TimeEdgeSpace != 0 && !custom) t.NiceTime(n);
                values = t.Ticks(n).ToList();
                if (_options.TimeEdgeSpace != 0 && values.Count > 2 && !custom)
                {
                    values.RemoveAt(values.Count - 1);
                    values.RemoveAt(0);
                }
            }
            else values = c.Ticks(n).ToList();
        }
        if (a.ScaleType == CdsChartScaleType.Time)
        {
            var dates = values.Select(ChartTime.FromMs).ToList();
            var interval = CdsChartFormat.IntervalOf(dates, _options.TimeInterval);
            var labels = new List<string>();
            var primary = new List<bool>();
            for (var i = 0; i < dates.Count; i++)
            {
                var def = string.IsNullOrEmpty(a.TickFormat) ? CdsChartFormat.TimeTick(dates, i, interval, _options.ShowDayName, _ctx.Culture) : dates[i].ToString(a.TickFormat, _ctx.Culture);
                labels.Add(a.TickFormatter is { } f ? f(dates[i], i) : def);
                primary.Add(CdsChartFormat.IsPrimary(dates, i, interval, _options.ShowDayName));
            }
            return (values, labels, primary, null);
        }
        var numLabels = values.Select((v, i) => a.TickFormatter is { } f ? f(v, i) : CdsChartFormat.Number(v, _ctx.Culture, a.TickFormat)).ToList();
        return (values, numLabels, values.Select(_ => false).ToList(), null);
    }

    void Measure(float width, float height)
    {
        var est = new Dictionary<AxisPosition, (List<double> Values, List<string> Labels, List<bool> Primary, List<string>? Categories)>();
        foreach (var pos in Axes.Keys)
            est[pos] = TickSet(pos, Axes[pos].IsVertical ? height : width, false);
        float TitleSpace(AxisPosition p) => string.IsNullOrEmpty(Axes[p].Title) ? 0 : TickHeight + 8;
        float left = 0, right = 0, top = 0, bottom = 0;
        foreach (var p in new[] { AxisPosition.Left, AxisPosition.Right })
        {
            if (!Axes.TryGetValue(p, out var a) || !a.IsVisible) continue;
            var w = (float)Math.Ceiling(CdsChartTextMetrics.MaxWidth(est[p].Labels, ChartFont.Tick) + TickPadding + TitleSpace(p));
            if (p == AxisPosition.Left) left = w;
            else right = w;
        }
        foreach (var p in new[] { AxisPosition.Bottom, AxisPosition.Top })
        {
            if (!Axes.TryGetValue(p, out var a) || !a.IsVisible) continue;
            var span = Math.Max(1, width - left - right);
            var rotate = a.TickRotation == CdsChartTickRotation.Always;
            if (a.TickRotation == CdsChartTickRotation.Auto)
            {
                var count = Math.Max(1, est[p].Labels.Count);
                var step = span / count;
                rotate = a.ScaleType == CdsChartScaleType.Labels
                    ? est[p].Labels.Any(l => CdsChartTextMetrics.Width(l, ChartFont.Tick) >= step)
                    : est[p].Labels.Any(l => l.Length * 6.2 * 0.8 >= step);
            }
            if (rotate && a.ScaleType != CdsChartScaleType.Labels && a.TickCount is null)
            {
                // Rotated labels take fewer ticks; when the sparser labels fit flat, they need not rotate after all.
                est[p] = TickSet(p, span, true);
                var fewer = span / Math.Max(1, est[p].Labels.Count);
                if (a.TickRotation == CdsChartTickRotation.Auto && est[p].Labels.All(l => CdsChartTextMetrics.Width(l, ChartFont.Tick) + TickLabelGap < fewer)) rotate = false;
            }
            Rotated[p] = rotate;
            var maxW = CdsChartTextMetrics.MaxWidth(est[p].Labels, ChartFont.Tick);
            var labelH = rotate ? Math.Min(maxW, 140) * MathF.Sqrt(0.5f) + TickHeight * MathF.Sqrt(0.5f) : TickHeight;
            var h = (float)Math.Ceiling(labelH + TickPadding + TitleSpace(p));
            if (p == AxisPosition.Bottom) bottom = h;
            else top = h;
        }
        var x0 = left;
        var x1 = Math.Max(left + 1, width - right);
        var y0 = top;
        var y1 = Math.Max(top + 1, height - bottom);
        Plot = new RectF(x0, y0, x1 - x0, y1 - y0);
        foreach (var p in new[] { AxisPosition.Left, AxisPosition.Right })
            if (Axes.ContainsKey(p)) est[p] = TickSet(p, Plot.Height, false);
        foreach (var (pos, scale) in Scales)
        {
            if (Axes[pos].IsVertical) scale.SetRange(y1, y0);
            else scale.SetRange(x0, x1);
            var e = est[pos];
            var ticks = new List<AxisTick>();
            if (scale.Band is { } band)
                for (var i = 0; i < e.Categories!.Count; i++) ticks.Add(new AxisTick(band.Center(e.Categories[i]), e.Labels[i], false));
            else
                for (var i = 0; i < e.Values.Count; i++) ticks.Add(new AxisTick(scale.Map(e.Values[i]), e.Labels[i], e.Primary[i]));
            Ticks[pos] = ticks;
        }
    }
}
