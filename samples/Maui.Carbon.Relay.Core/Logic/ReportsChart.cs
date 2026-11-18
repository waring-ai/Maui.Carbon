using System.Globalization;
using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Logic;

/// <summary>Chart helpers for the reports screen: custom-range buckets and a readable value axis (pure, unit-tested).</summary>
public static class ReportsChart
{
    /// <summary>The most bars a custom range is split into.</summary>
    public const int MaxCustomBars = 16;

    /// <summary>
    /// Buckets created and resolved issues over <paramref name="start"/>..<paramref name="end"/> (whole days, both
    /// included): one bar per day up to two weeks, else bars of whole weeks so there are at most
    /// <see cref="MaxCustomBars"/>. The last bar is cut at the end date. Resolved means closed and last updated in the
    /// bar, as in <see cref="ReportMath.Throughput"/>.
    /// </summary>
    public static IReadOnlyList<ReportBucket> CustomThroughput(IEnumerable<Issue> issues, DateTime start, DateTime end)
    {
        var from = start.Date;
        var to = end.Date;
        if (to < from) (from, to) = (to, from);
        var stop = to.AddDays(1);
        var days = (stop - from).Days;
        var stepDays = days <= 14 ? 1 : 7 * (int)Math.Ceiling(days / (7.0 * MaxCustomBars));
        var list = issues.ToList();
        var buckets = new List<ReportBucket>();
        for (var bStart = from; bStart < stop; bStart = bStart.AddDays(stepDays))
        {
            var bEnd = bStart.AddDays(stepDays) < stop ? bStart.AddDays(stepDays) : stop;
            var created = list.Count(x => x.Created >= bStart && x.Created < bEnd);
            var resolved = list.Count(x => !x.IsOpen && x.Updated >= bStart && x.Updated < bEnd);
            buckets.Add(new ReportBucket(bStart.ToString("MMM d", CultureInfo.InvariantCulture), bStart, bEnd, created, resolved));
        }
        return buckets;
    }

    /// <summary>
    /// The top of the value axis for a largest value of <paramref name="max"/>: a round number (1, 2, 4, 5, 8 or 10 times a
    /// power of ten, at least 4) so the ticks (<see cref="Ticks"/>) are whole numbers.
    /// </summary>
    public static int AxisMax(int max)
    {
        if (max <= 4) return 4;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(max)));
        foreach (var step in new[] { 1, 2, 4, 5, 8, 10 })
        {
            var candidate = (int)(step * magnitude);
            if (candidate >= max) return candidate;
        }
        return (int)(10 * magnitude);
    }

    /// <summary>
    /// Tick values from the top of the axis down to 0 in whole-number steps: quarters when <paramref name="axisMax"/>
    /// divides by 4, else fifths, else halves.
    /// </summary>
    public static IReadOnlyList<int> Ticks(int axisMax)
    {
        var parts = axisMax % 4 == 0 ? 4 : axisMax % 5 == 0 ? 5 : 2;
        return Enumerable.Range(0, parts + 1).Select(i => axisMax - axisMax / parts * i).ToList();
    }
}
