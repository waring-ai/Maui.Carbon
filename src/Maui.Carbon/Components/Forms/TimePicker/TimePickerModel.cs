using System.Text.RegularExpressions;

namespace Maui.Carbon;

/// <summary>AM or PM of a 12-hour time.</summary>
public enum CdsTimePeriod
{
    /// <summary>Before noon.</summary>
    Am,

    /// <summary>From noon.</summary>
    Pm,
}

/// <summary>Pure parsing and conversion logic of the time picker.</summary>
internal static partial class TimePickerModel
{
    // Carbon's default 12-hour pattern: (1[012]|[1-9]):[0-5][0-9](\s)?
    [GeneratedRegex(@"^(1[012]|[1-9]):([0-5][0-9])\s?$", RegexOptions.CultureInvariant)]
    private static partial Regex TwelveHour();

    [GeneratedRegex(@"^([01]?[0-9]|2[0-3]):([0-5][0-9])\s?$", RegexOptions.CultureInvariant)]
    private static partial Regex TwentyFourHour();

    /// <summary>Parses "h:mm" (12-hour, hour 1..12) or "H:mm" / "HH:mm" (24-hour, hour 0..23).</summary>
    public static bool TryParse(string? text, bool twelveHour, out int hour, out int minute)
    {
        hour = 0;
        minute = 0;
        if (string.IsNullOrEmpty(text)) return false;
        var match = (twelveHour ? TwelveHour() : TwentyFourHour()).Match(text);
        if (!match.Success) return false;
        hour = int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        minute = int.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
        return true;
    }

    /// <summary>True when the text is a well-formed time of the kind.</summary>
    public static bool IsValid(string? text, bool twelveHour) => TryParse(text, twelveHour, out _, out _);

    /// <summary>The time of day for typed text and a period (ignored for 24-hour), or <c>null</c> when the text is malformed.</summary>
    public static TimeSpan? ToTimeOfDay(string? text, bool twelveHour, CdsTimePeriod period)
    {
        if (!TryParse(text, twelveHour, out var hour, out var minute)) return null;
        if (twelveHour) hour = hour % 12 + (period == CdsTimePeriod.Pm ? 12 : 0);
        return new TimeSpan(hour, minute, 0);
    }

    /// <summary>Formats a time of day as picker text: "2:30" with a period (12-hour) or "14:30" (24-hour).</summary>
    public static (string Text, CdsTimePeriod Period) Format(TimeSpan time, bool twelveHour)
    {
        var hour = (int)time.TotalHours % 24;
        var minute = time.Minutes;
        if (!twelveHour) return ($"{hour:00}:{minute:00}", hour >= 12 ? CdsTimePeriod.Pm : CdsTimePeriod.Am);
        var period = hour >= 12 ? CdsTimePeriod.Pm : CdsTimePeriod.Am;
        var shown = hour % 12 == 0 ? 12 : hour % 12;
        return ($"{shown}:{minute:00}", period);
    }
}
