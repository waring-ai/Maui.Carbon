using System.Globalization;

namespace Maui.Carbon;

/// <summary>Which handle of a slider an operation applies to.</summary>
internal enum SliderHandle
{
    /// <summary>The only handle of a single slider, or the lower handle of a range.</summary>
    Lower,
    /// <summary>The upper handle of a range.</summary>
    Upper,
}

/// <summary>
/// Pure logic of <see cref="CdsSlider"/>: clamping, snapping to the step, converting between values and positions,
/// keyboard steps, range ordering, picking the handle a tap moves, and number formatting and parsing.
/// </summary>
internal static class SliderModel
{
    /// <summary>Clamps to [min, max]; a max below min yields min.</summary>
    public static double Clamp(double value, double min, double max)
    {
        if (double.IsNaN(value)) return min;
        if (max < min) return min;
        return Math.Min(Math.Max(value, min), max);
    }

    /// <summary>The number of decimal places needed to show multiples of <paramref name="step"/> (0.25 gives 2; capped at 8).</summary>
    public static int Decimals(double step)
    {
        if (double.IsNaN(step) || double.IsInfinity(step) || step <= 0) return 0;
        var text = Math.Round(step, 8).ToString("0.########", CultureInfo.InvariantCulture);
        var dot = text.IndexOf('.');
        return dot < 0 ? 0 : text.Length - dot - 1;
    }

    /// <summary>Rounds to the nearest step counted from <paramref name="min"/>, then clamps (a step of 0 or less only clamps).</summary>
    public static double Snap(double value, double min, double max, double step)
    {
        value = Clamp(value, min, max);
        if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step)) return value;
        var steps = Math.Round((value - min) / step, MidpointRounding.AwayFromZero);
        var snapped = min + steps * step;
        // Remove floating-point noise such as 0.30000000000000004.
        snapped = Math.Round(snapped, Math.Max(Decimals(step), Decimals(min)));
        return Clamp(snapped, min, max);
    }

    /// <summary>The position of a value on the track as a fraction in [0, 1] (0 when the range is empty).</summary>
    public static double ToFraction(double value, double min, double max) =>
        max <= min ? 0 : (Clamp(value, min, max) - min) / (max - min);

    /// <summary>The snapped value at a position (a fraction, clamped to [0, 1]) on the track.</summary>
    public static double FromFraction(double fraction, double min, double max, double step)
    {
        if (double.IsNaN(fraction)) fraction = 0;
        fraction = Math.Min(Math.Max(fraction, 0), 1);
        return Snap(min + fraction * (max - min), min, max, step);
    }

    /// <summary>
    /// The value after a key press, or <c>null</c> when the key is not a slider key. Arrows move by one step (times the
    /// multiplier with Shift); PageUp and PageDown move by the step times the multiplier; Home and End jump to the ends.
    /// </summary>
    public static double? KeyboardValue(CdsKey key, bool shift, double value, double min, double max, double step, double multiplier)
    {
        var unit = step > 0 ? step : (max - min) / 100;
        var large = unit * Math.Max(multiplier, 1);
        var amount = shift ? large : unit;
        return key switch
        {
            CdsKey.Left or CdsKey.Down => Snap(value - amount, min, max, step),
            CdsKey.Right or CdsKey.Up => Snap(value + amount, min, max, step),
            CdsKey.PageDown => Snap(value - large, min, max, step),
            CdsKey.PageUp => Snap(value + large, min, max, step),
            CdsKey.Home => min,
            CdsKey.End => max,
            _ => null,
        };
    }

    /// <summary>Keeps a range ordered: the lower value never passes the upper one, and the upper never drops below the lower.</summary>
    public static (double Lower, double Upper) OrderRange(double lower, double upper, SliderHandle moved)
    {
        if (lower <= upper) return (lower, upper);
        return moved == SliderHandle.Lower ? (upper, upper) : (lower, lower);
    }

    /// <summary>The handle a tap at <paramref name="value"/> moves: the nearer one; at equal distance, toward the side of the tap.</summary>
    public static SliderHandle NearestHandle(double value, double lower, double upper)
    {
        var toLower = Math.Abs(value - lower);
        var toUpper = Math.Abs(value - upper);
        if (toLower < toUpper) return SliderHandle.Lower;
        if (toUpper < toLower) return SliderHandle.Upper;
        return value > upper ? SliderHandle.Upper : SliderHandle.Lower;
    }

    /// <summary>True when the value is outside [min, max] (a typed value that is not yet clamped).</summary>
    public static bool IsOutOfRange(double value, double min, double max) => value < min || value > max;

    /// <summary>Parses a typed number (invariant or current culture); <c>false</c> for empty or non-numeric text.</summary>
    public static bool TryParse(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var s = text.Trim();
        if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            && !double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
            return false;
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            value = 0;
            return false;
        }
        return true;
    }

    /// <summary>Formats a value with as many decimals as the step needs ("120", "0.25").</summary>
    public static string Format(double value, double step) =>
        Math.Round(value, Math.Max(Decimals(step), 0)).ToString("0." + new string('#', Math.Max(Decimals(step), 1)), CultureInfo.InvariantCulture);
}
