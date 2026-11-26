using System.Globalization;

namespace Maui.Carbon;

/// <summary>
/// Number and date text of charts (Carbon's locale formatting: <c>toLocaleString</c> numbers with up to three decimals,
/// short-month dates) and the time-axis tick labels of Carbon's time-series service.
/// </summary>
internal static class CdsChartFormat
{
    /// <summary>A number with group separators and up to three decimals ("65,000", "0.125"), or with <paramref name="format"/>.</summary>
    public static string Number(double? value, CultureInfo culture, string? format = null)
    {
        if (value is not { } v || double.IsNaN(v)) return "";
        if (!string.IsNullOrEmpty(format)) return v.ToString(format, culture);
        if (Math.Abs(v) < 5e-4) v = 0;
        return v.ToString("#,##0.###", culture);
    }

    /// <summary>A date for tooltips and tables: "Jan 5, 2024", with the time when it has one ("Jan 5, 2024, 3:30 PM").</summary>
    public static string Date(DateTime value, CultureInfo culture, string? format = null)
    {
        if (!string.IsNullOrEmpty(format)) return value.ToString(format, culture);
        return value.TimeOfDay == TimeSpan.Zero
            ? value.ToString("MMM d, yyyy", culture)
            : value.ToString("MMM d, yyyy, h:mm tt", culture);
    }

    // ----- time axis (Carbon's time-series service) -----

    static readonly (CdsChartTimeInterval Interval, double Ms)[] Intervals =
    [
        (CdsChartTimeInterval.FifteenSeconds, 15e3), (CdsChartTimeInterval.Minute, 6e4), (CdsChartTimeInterval.ThirtyMinutes, 18e5),
        (CdsChartTimeInterval.Hourly, 36e5), (CdsChartTimeInterval.Daily, 864e5), (CdsChartTimeInterval.Monthly, 2592e6),
        (CdsChartTimeInterval.Quarterly, 7776e6), (CdsChartTimeInterval.Yearly, 31104e6),
    ];

    /// <summary>The tick interval: <paramref name="requested"/>, or the interval closest to the smallest tick spacing.</summary>
    public static CdsChartTimeInterval IntervalOf(IReadOnlyList<DateTime> ticks, CdsChartTimeInterval requested)
    {
        if (requested != CdsChartTimeInterval.Auto) return requested;
        if (ticks.Count <= 1) return CdsChartTimeInterval.FifteenSeconds;
        var min = double.MaxValue;
        for (var i = 1; i < ticks.Count; i++) min = Math.Min(min, (ticks[i] - ticks[i - 1]).TotalMilliseconds);
        var best = 0;
        for (var i = 0; i < Intervals.Length; i++)
            if (Math.Abs(Intervals[i].Ms - min) < Math.Abs(Intervals[best].Ms - min)) best = i;
        return Intervals[best].Interval;
    }

    /// <summary>Whether a tick starts a new day, month or year (or is the first): Carbon draws those bold.</summary>
    public static bool IsPrimary(IReadOnlyList<DateTime> ticks, int index, CdsChartTimeInterval interval, bool showDayName)
    {
        var t = ticks[index];
        var first = index == 0;
        DateTime? prev = index > 0 ? ticks[index - 1] : null;
        var dayChanged = t.TimeOfDay == TimeSpan.Zero;
        var monthChanged = prev is { } p && p.Month != t.Month;
        var yearChanged = t.Month == 1 && t.Day == 1 && t.TimeOfDay == TimeSpan.Zero;
        var weekStart = t.DayOfWeek == DayOfWeek.Monday;
        return interval switch
        {
            CdsChartTimeInterval.FifteenSeconds or CdsChartTimeInterval.Minute or CdsChartTimeInterval.ThirtyMinutes or CdsChartTimeInterval.Hourly
                => first || dayChanged || monthChanged || yearChanged,
            CdsChartTimeInterval.Daily => showDayName ? first || weekStart || yearChanged : first || monthChanged || yearChanged,
            CdsChartTimeInterval.Weekly => first || weekStart || yearChanged,
            CdsChartTimeInterval.Monthly => first || yearChanged,
            CdsChartTimeInterval.Quarterly => first || t.Month <= 3,
            _ => false,
        };
    }

    /// <summary>A time tick label: the longer primary form ("Jan 5") at day, month and year starts, else the short one ("6").</summary>
    public static string TimeTick(IReadOnlyList<DateTime> ticks, int index, CdsChartTimeInterval interval, bool showDayName, CultureInfo culture)
    {
        var t = ticks[index];
        var key = interval == CdsChartTimeInterval.Daily && showDayName ? CdsChartTimeInterval.Weekly : interval;
        var primary = IsPrimary(ticks, index, interval, showDayName);
        if (key == CdsChartTimeInterval.Quarterly)
        {
            var q = "Q" + ((t.Month - 1) / 3 + 1);
            return primary ? q + " '" + (t.Year % 100).ToString("00", culture) : q;
        }
        var format = key switch
        {
            CdsChartTimeInterval.FifteenSeconds => primary ? "MMM d, h:mm:ss tt" : "h:mm:ss tt",
            CdsChartTimeInterval.Minute or CdsChartTimeInterval.ThirtyMinutes => primary ? "MMM d, h:mm tt" : "h:mm tt",
            CdsChartTimeInterval.Hourly => primary ? "MMM d, hh tt" : "hh tt",
            CdsChartTimeInterval.Daily => primary ? "MMM d" : "%d",
            CdsChartTimeInterval.Weekly => primary ? "ddd, MMM d" : "ddd",
            CdsChartTimeInterval.Monthly => primary ? "MMM yyyy" : "MMM",
            _ => "yyyy",
        };
        if (key == CdsChartTimeInterval.FifteenSeconds && t.Millisecond != 0) format = format.Replace(":ss", ":ss.fff");
        return t.ToString(format, culture);
    }
}
