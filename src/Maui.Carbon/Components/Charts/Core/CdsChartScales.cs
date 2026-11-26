namespace Maui.Carbon;

/// <summary>d3-array's tick algorithms (ticks, tickIncrement, tickStep, nice), which Carbon Charts' axes use.</summary>
internal static class ChartTicks
{
    static readonly double E10 = Math.Sqrt(50), E5 = Math.Sqrt(10), E2 = Math.Sqrt(2);

    static (double I1, double I2, double Inc) Spec(double start, double stop, double count)
    {
        var step = (stop - start) / Math.Max(0, count);
        var power = Math.Floor(Math.Log10(step));
        var error = step / Math.Pow(10, power);
        var factor = error >= E10 ? 10 : error >= E5 ? 5 : error >= E2 ? 2 : 1;
        double i1, i2, inc;
        if (power < 0)
        {
            inc = Math.Pow(10, -power) / factor;
            i1 = Math.Round(start * inc);
            i2 = Math.Round(stop * inc);
            if (i1 / inc < start) ++i1;
            if (i2 / inc > stop) --i2;
            inc = -inc;
        }
        else
        {
            inc = Math.Pow(10, power) * factor;
            i1 = Math.Round(start / inc);
            i2 = Math.Round(stop / inc);
            if (i1 * inc < start) ++i1;
            if (i2 * inc > stop) --i2;
        }
        if (i2 < i1 && 0.5 <= count && count < 2) return Spec(start, stop, count * 2);
        return (i1, i2, inc);
    }

    /// <summary>About <paramref name="count"/> round tick values between start and stop (d3 <c>ticks</c>).</summary>
    public static double[] Ticks(double start, double stop, double count)
    {
        if (!(count > 0) || double.IsNaN(start) || double.IsNaN(stop)) return [];
        if (start == stop) return [start];
        var reverse = stop < start;
        var (i1, i2, inc) = reverse ? Spec(stop, start, count) : Spec(start, stop, count);
        if (!(i2 >= i1) || double.IsNaN(inc)) return [];
        var n = (int)(i2 - i1 + 1);
        if (n > 10000) return [];
        var ticks = new double[n];
        for (var i = 0; i < n; i++)
        {
            var k = reverse ? i2 - i : i1 + i;
            ticks[i] = inc < 0 ? k / -inc : k * inc;
        }
        return ticks;
    }

    /// <summary>d3 <c>tickIncrement</c>: the tick step, negative for steps below one (its inverse).</summary>
    public static double Increment(double start, double stop, double count) => Spec(start, stop, count).Inc;

    /// <summary>d3 <c>tickStep</c>: the tick step as a positive number.</summary>
    public static double Step(double start, double stop, double count)
    {
        var reverse = stop < start;
        var inc = reverse ? Increment(stop, start, count) : Increment(start, stop, count);
        return (reverse ? -1 : 1) * (inc < 0 ? 1 / -inc : inc);
    }

    /// <summary>d3 linear <c>nice</c>: extends the domain to round tick values.</summary>
    public static (double Start, double Stop) Nice(double start, double stop, int count = 10)
    {
        double? prestep = null;
        for (var iter = 0; iter < 10; iter++)
        {
            var step = Increment(start, stop, count);
            if (step == prestep) break;
            if (step > 0)
            {
                start = Math.Floor(start / step) * step;
                stop = Math.Ceiling(stop / step) * step;
            }
            else if (step < 0)
            {
                start = Math.Ceiling(start * step) / step;
                stop = Math.Floor(stop * step) / step;
            }
            else break;
            prestep = step;
        }
        return (start, stop);
    }
}

/// <summary>Converts chart dates to and from the milliseconds continuous scales work in.</summary>
internal static class ChartTime
{
    public static double ToMs(DateTime d) => d.Ticks / (double)TimeSpan.TicksPerMillisecond;

    public static DateTime FromMs(double ms) => new((long)Math.Round(ms * TimeSpan.TicksPerMillisecond));
}

/// <summary>A linear, log or time scale from a domain to a pixel range (d3 <c>scaleLinear</c>, <c>scaleLog</c>, <c>scaleTime</c>). Time values are milliseconds (<see cref="ChartTime"/>).</summary>
internal sealed class ContinuousScale
{
    public ContinuousScale(CdsChartScaleType type, double d0, double d1, double logBase = 10)
    {
        Type = type;
        D0 = d0;
        D1 = d1;
        LogBase = logBase;
    }

    public CdsChartScaleType Type { get; }

    public double D0 { get; set; }

    public double D1 { get; set; }

    public double R0 { get; set; }

    public double R1 { get; set; } = 1;

    public double LogBase { get; }

    double T(double v) => Type == CdsChartScaleType.Log ? (v > 0 ? Math.Log(v) : D0 < 0 ? -Math.Log(-v) : double.NaN) : v;

    /// <summary>The pixel of a domain value.</summary>
    public double Map(double v)
    {
        var t0 = T(D0);
        var t1 = T(D1);
        if (t1 == t0) return (R0 + R1) / 2;
        return R0 + (T(v) - t0) / (t1 - t0) * (R1 - R0);
    }

    /// <summary>The domain value at a pixel.</summary>
    public double Invert(double px)
    {
        var t0 = T(D0);
        var t1 = T(D1);
        if (R1 == R0) return D0;
        var t = t0 + (px - R0) / (R1 - R0) * (t1 - t0);
        return Type == CdsChartScaleType.Log ? Math.Exp(t) : t;
    }

    /// <summary>About <paramref name="count"/> ticks in the domain.</summary>
    public IReadOnlyList<double> Ticks(int count) => Type switch
    {
        CdsChartScaleType.Time => ChartTimeTicks.Ticks(D0, D1, count).Select(ChartTime.ToMs).ToArray(),
        CdsChartScaleType.Log => LogTicks(count),
        _ => ChartTicks.Ticks(D0, D1, count),
    };

    /// <summary>Extends a time domain to whole tick intervals (d3 time <c>nice</c>).</summary>
    public void NiceTime(int count)
    {
        var (a, b) = ChartTimeTicks.Nice(ChartTime.FromMs(D0), ChartTime.FromMs(D1), count);
        D0 = ChartTime.ToMs(a);
        D1 = ChartTime.ToMs(b);
    }

    double[] LogTicks(int count)
    {
        var u = Math.Min(D0, D1);
        var v = Math.Max(D0, D1);
        if (!(u > 0) || !(v > 0)) return [];
        var b = LogBase;
        var i = Math.Log(u) / Math.Log(b);
        var j = Math.Log(v) / Math.Log(b);
        var list = new List<double>();
        if (b % 1 == 0 && j - i < count)
        {
            for (var p = Math.Floor(i); p <= Math.Ceiling(j); p++)
                for (var k = 1; k < b; k++)
                {
                    var t = k * Math.Pow(b, p);
                    if (t < u) continue;
                    if (t > v) break;
                    list.Add(t);
                }
            if (list.Count * 2 < count) return ChartTicks.Ticks(u, v, count);
        }
        else
        {
            foreach (var e in ChartTicks.Ticks(i, j, Math.Min(j - i, count))) list.Add(Math.Pow(b, e));
        }
        return list.ToArray();
    }
}

/// <summary>d3-time's tick intervals for time scales.</summary>
internal static class ChartTimeTicks
{
    internal enum Unit { Millisecond, Second, Minute, Hour, Day, Week, Month, Year }

    const double Second = 1e3, Minute = 6e4, Hour = 36e5, Day = 864e5, Week = 6048e5, Month = 2592e6, Year = 31536e6;

    static readonly (Unit Unit, int Step, double Ms)[] Intervals =
    [
        (Unit.Second, 1, Second), (Unit.Second, 5, 5 * Second), (Unit.Second, 15, 15 * Second), (Unit.Second, 30, 30 * Second),
        (Unit.Minute, 1, Minute), (Unit.Minute, 5, 5 * Minute), (Unit.Minute, 15, 15 * Minute), (Unit.Minute, 30, 30 * Minute),
        (Unit.Hour, 1, Hour), (Unit.Hour, 3, 3 * Hour), (Unit.Hour, 6, 6 * Hour), (Unit.Hour, 12, 12 * Hour),
        (Unit.Day, 1, Day), (Unit.Day, 2, 2 * Day), (Unit.Week, 1, Week), (Unit.Month, 1, Month), (Unit.Month, 3, 3 * Month),
        (Unit.Year, 1, Year),
    ];

    /// <summary>The interval d3 picks for about <paramref name="count"/> ticks between two times (milliseconds).</summary>
    internal static (Unit Unit, int Step) Interval(double start, double stop, int count)
    {
        var target = Math.Abs(stop - start) / Math.Max(1, count);
        var i = 0;
        while (i < Intervals.Length && Intervals[i].Ms <= target) i++;
        if (i == Intervals.Length) return (Unit.Year, Math.Max(1, (int)Math.Round(ChartTicks.Step(start / Year, stop / Year, count))));
        if (i == 0) return (Unit.Millisecond, Math.Max(1, (int)Math.Round(ChartTicks.Step(start, stop, count))));
        var pick = target / Intervals[i - 1].Ms < Intervals[i].Ms / target ? i - 1 : i;
        return (Intervals[pick].Unit, Intervals[pick].Step);
    }

    /// <summary>Tick dates between two times (milliseconds), both ends included when they fall on a tick.</summary>
    public static IReadOnlyList<DateTime> Ticks(double startMs, double stopMs, int count)
    {
        if (double.IsNaN(startMs) || double.IsNaN(stopMs) || count <= 0) return [];
        var reverse = stopMs < startMs;
        var a = Math.Min(startMs, stopMs);
        var b = Math.Max(startMs, stopMs);
        var (unit, step) = Interval(a, b, count);
        var list = new List<DateTime>();
        var start = ChartTime.FromMs(a);
        var stop = ChartTime.FromMs(b);
        var t = Floor(start, unit);
        if (t < start) t = Add(t, unit, 1);
        for (var guard = 0; t <= stop && guard < 20000; guard++, t = Add(t, unit, 1))
            if (Matches(t, unit, step)) list.Add(t);
        if (reverse) list.Reverse();
        return list;
    }

    /// <summary>Extends a time domain to the interval d3 would tick it with.</summary>
    public static (DateTime Start, DateTime Stop) Nice(DateTime start, DateTime stop, int count)
    {
        var (unit, step) = Interval(ChartTime.ToMs(start), ChartTime.ToMs(stop), count);
        var a = Floor(start, unit);
        for (var g = 0; !Matches(a, unit, step) && g < 1000; g++) a = Add(a, unit, -1);
        var b = Floor(stop, unit);
        if (b < stop) b = Add(b, unit, 1);
        for (var g = 0; !Matches(b, unit, step) && g < 1000; g++) b = Add(b, unit, 1);
        return (a, b);
    }

    static DateTime Floor(DateTime d, Unit unit) => unit switch
    {
        Unit.Millisecond => new DateTime(d.Ticks - d.Ticks % TimeSpan.TicksPerMillisecond),
        Unit.Second => new DateTime(d.Ticks - d.Ticks % TimeSpan.TicksPerSecond),
        Unit.Minute => new DateTime(d.Ticks - d.Ticks % TimeSpan.TicksPerMinute),
        Unit.Hour => new DateTime(d.Ticks - d.Ticks % TimeSpan.TicksPerHour),
        Unit.Day => d.Date,
        Unit.Week => d.Date.AddDays(-(int)d.DayOfWeek),
        Unit.Month => new DateTime(d.Year, d.Month, 1),
        _ => new DateTime(d.Year, 1, 1),
    };

    static DateTime Add(DateTime d, Unit unit, int k) => unit switch
    {
        Unit.Millisecond => d.AddMilliseconds(k),
        Unit.Second => d.AddSeconds(k),
        Unit.Minute => d.AddMinutes(k),
        Unit.Hour => d.AddHours(k),
        Unit.Day => d.AddDays(k),
        Unit.Week => d.AddDays(7 * k),
        Unit.Month => d.AddMonths(k),
        _ => d.AddYears(k),
    };

    static bool Matches(DateTime d, Unit unit, int step) => step <= 1 || unit switch
    {
        Unit.Millisecond => d.Millisecond % step == 0,
        Unit.Second => d.Second % step == 0,
        Unit.Minute => d.Minute % step == 0,
        Unit.Hour => d.Hour % step == 0,
        Unit.Day => (d.Day - 1) % step == 0,
        Unit.Month => (d.Month - 1) % step == 0,
        Unit.Year => d.Year % step == 0,
        _ => true,
    };
}

/// <summary>A band scale for a labels axis (d3 <c>scaleBand().rangeRound</c>): each category gets an equal band.</summary>
internal sealed class BandScale
{
    readonly Dictionary<string, int> _index = new(StringComparer.Ordinal);
    double[] _starts = [];

    public BandScale(IReadOnlyList<string> domain)
    {
        Domain = domain;
        for (var i = 0; i < domain.Count; i++) _index.TryAdd(domain[i], i);
    }

    public IReadOnlyList<string> Domain { get; }

    public double R0 { get; private set; }

    public double R1 { get; private set; }

    public double PaddingInner { get; set; }

    public double PaddingOuter { get; set; }

    public double Step { get; private set; }

    public double Bandwidth { get; private set; }

    /// <summary>Sets the pixel range (rounded like d3's <c>rangeRound</c>).</summary>
    public void SetRange(double r0, double r1)
    {
        R0 = r0;
        R1 = r1;
        var n = Domain.Count;
        var reverse = r1 < r0;
        var start = reverse ? r1 : r0;
        var stop = reverse ? r0 : r1;
        Step = (stop - start) / Math.Max(1, n - PaddingInner + PaddingOuter * 2);
        Step = Math.Floor(Step);
        start += (stop - start - Step * (n - PaddingInner)) * 0.5;
        Bandwidth = Step * (1 - PaddingInner);
        start = Math.Round(start);
        Bandwidth = Math.Round(Bandwidth);
        _starts = new double[n];
        for (var i = 0; i < n; i++) _starts[reverse ? n - 1 - i : i] = start + Step * i;
    }

    /// <summary>The start pixel of a category's band, or NaN when it is not in the domain.</summary>
    public double Map(string? key) => key is not null && _index.TryGetValue(key, out var i) && i < _starts.Length ? _starts[i] : double.NaN;

    /// <summary>The center pixel of a category's band.</summary>
    public double Center(string? key) => Map(key) + Bandwidth / 2;

    public bool Contains(string? key) => key is not null && _index.ContainsKey(key);
}
