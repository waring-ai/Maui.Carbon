using System.Globalization;

namespace Maui.Carbon;

/// <summary>Why a number input's text is not acceptable.</summary>
internal enum NumberValidity
{
    /// <summary>A number inside the range.</summary>
    Valid,

    /// <summary>Empty while empty is not allowed.</summary>
    Empty,

    /// <summary>Not a number.</summary>
    NotANumber,

    /// <summary>Below the minimum.</summary>
    BelowMin,

    /// <summary>Above the maximum.</summary>
    AboveMax,
}

/// <summary>Pure parsing, stepping, clamping and precision logic of the number input.</summary>
internal static class NumberInputModel
{
    /// <summary>Parses user text. Empty and whitespace give <c>null</c> and <c>true</c>; text that is no finite number gives <c>false</c>.</summary>
    public static bool TryParse(string? text, IFormatProvider? culture, out double? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (double.TryParse(text.Trim(), NumberStyles.Float, culture ?? CultureInfo.CurrentCulture, out var parsed) && double.IsFinite(parsed))
        {
            value = parsed;
            return true;
        }
        return false;
    }

    /// <summary>The number of decimal places in a number (0.25 gives 2, 3 gives 0).</summary>
    public static int Decimals(double value)
    {
        if (!double.IsFinite(value)) return 0;
        var text = value.ToString("0.###############", CultureInfo.InvariantCulture);
        var dot = text.IndexOf('.');
        return dot < 0 ? 0 : text.Length - dot - 1;
    }

    /// <summary>The precision of a step: the larger of the decimals of the step and of the current value.</summary>
    public static int Precision(double step, double? current) => Math.Max(Decimals(step), current is { } c ? Decimals(c) : 0);

    /// <summary>Rounds away floating point noise (0.1 + 0.2 becomes 0.3).</summary>
    public static double Round(double value, int precision) =>
        Math.Round(value, Math.Clamp(precision, 0, 15), MidpointRounding.AwayFromZero);

    /// <summary>Limits <paramref name="value"/> to <paramref name="min"/>..<paramref name="max"/> (either may be <c>null</c>).</summary>
    public static double Clamp(double value, double? min, double? max)
    {
        if (max is { } hi && value > hi) value = hi;
        if (min is { } lo && value < lo) value = lo;
        return value;
    }

    /// <summary>
    /// Moves a value one step up (+1) or down (-1): an empty value counts as 0, the sum is rounded to the precision of the
    /// step and the value, then clamped to the range.
    /// </summary>
    public static double Step(double? current, int direction, double step, double? min, double? max)
    {
        var sign = Math.Sign(direction);
        var size = step > 0 ? step : 1;
        var from = current ?? 0;
        var result = Round(from + sign * size, Precision(size, from));
        return Clamp(result, min, max);
    }

    /// <summary>Checks typed text against the range.</summary>
    public static NumberValidity Validate(string? text, IFormatProvider? culture, double? min, double? max, bool allowEmpty)
    {
        if (!TryParse(text, culture, out var value)) return NumberValidity.NotANumber;
        if (value is not { } v) return allowEmpty ? NumberValidity.Valid : NumberValidity.Empty;
        if (min is { } lo && v < lo) return NumberValidity.BelowMin;
        if (max is { } hi && v > hi) return NumberValidity.AboveMax;
        return NumberValidity.Valid;
    }

    /// <summary>Formats a value with its precision (no grouping).</summary>
    public static string Format(double? value, double step, IFormatProvider? culture)
    {
        if (value is not { } v) return string.Empty;
        return v.ToString("F" + Precision(step, v), culture ?? CultureInfo.CurrentCulture);
    }
}
