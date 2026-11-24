using System.Globalization;
using System.Text;

namespace Maui.Carbon;

/// <summary>Why a typed date was rejected.</summary>
public enum CdsDateEntryError
{
    /// <summary>The text is a valid date (or empty).</summary>
    None,

    /// <summary>The text does not match the date format.</summary>
    Unparseable,

    /// <summary>The date is before <c>MinDate</c>.</summary>
    BeforeMin,

    /// <summary>The date is after <c>MaxDate</c>.</summary>
    AfterMax,

    /// <summary>The date is one of the disabled dates.</summary>
    Disabled,

    /// <summary>The end date of a range is before its start date.</summary>
    EndBeforeStart,
}

/// <summary>
/// Pure date text logic of the date picker: Carbon's flatpickr-style format tokens (<c>m n d j Y y</c>, default
/// <c>m/d/Y</c>) mapped to .NET, formatting, lenient parsing and validation against min, max and disabled dates.
/// </summary>
internal static class DatePickerFormat
{
    /// <summary>Carbon's default date format.</summary>
    public const string DefaultFormat = "m/d/Y";

    /// <summary>Maps a flatpickr-style format to a .NET custom format used to <b>write</b> dates: <c>m</c> is MM, <c>n</c> M, <c>d</c> dd, <c>j</c> d, <c>Y</c> yyyy, <c>y</c> yy; everything else is literal.</summary>
    public static string ToNetFormat(string? format) => Convert(format, strict: true);

    /// <summary>The .NET format used to <b>read</b> dates: month and day accept one or two digits whatever the token.</summary>
    public static string ToNetParseFormat(string? format) => Convert(format, strict: false);

    static string Convert(string? format, bool strict)
    {
        var f = string.IsNullOrEmpty(format) ? DefaultFormat : format;
        var sb = new StringBuilder();
        foreach (var c in f)
        {
            switch (c)
            {
                case 'm': sb.Append(strict ? "MM" : "M"); break;
                case 'n': sb.Append('M'); break;
                case 'd': sb.Append(strict ? "dd" : "d"); break;
                case 'j': sb.Append('d'); break;
                case 'Y': sb.Append("yyyy"); break;
                case 'y': sb.Append("yy"); break;
                default: sb.Append('\\').Append(c); break;
            }
        }
        // A lone specifier would be read as a standard format ("M" is month and day): force a custom one.
        if (sb.Length == 1) sb.Insert(0, '%');
        return sb.ToString();
    }

    /// <summary>The placeholder a format shows: <c>m/d/Y</c> gives <c>mm/dd/yyyy</c>.</summary>
    public static string Placeholder(string? format)
    {
        var f = string.IsNullOrEmpty(format) ? DefaultFormat : format;
        var sb = new StringBuilder();
        foreach (var c in f)
        {
            switch (c)
            {
                case 'm': sb.Append("mm"); break;
                case 'n': sb.Append('m'); break;
                case 'd': sb.Append("dd"); break;
                case 'j': sb.Append('d'); break;
                case 'Y': sb.Append("yyyy"); break;
                case 'y': sb.Append("yy"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }

    /// <summary>Formats a date in <paramref name="format"/>.</summary>
    public static string Format(DateOnly date, string? format) =>
        date.ToString(ToNetFormat(format), CultureInfo.InvariantCulture);

    /// <summary>Parses text in <paramref name="format"/> (digits only; surrounding blanks ignored).</summary>
    public static bool TryParse(string? text, string? format, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        return DateOnly.TryParseExact(text.Trim(), ToNetParseFormat(format), CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    /// <summary>
    /// Validates typed text. Empty text is valid and gives no date. A date outside <paramref name="min"/>..<paramref name="max"/>
    /// or on a disabled day is rejected.
    /// </summary>
    public static (DateOnly? Date, CdsDateEntryError Error) Validate(string? text, string? format, DateOnly? min, DateOnly? max, Func<DateOnly, bool>? isDisabled)
    {
        if (string.IsNullOrWhiteSpace(text)) return (null, CdsDateEntryError.None);
        if (!TryParse(text, format, out var date)) return (null, CdsDateEntryError.Unparseable);
        if (min is { } lo && date < lo) return (null, CdsDateEntryError.BeforeMin);
        if (max is { } hi && date > hi) return (null, CdsDateEntryError.AfterMax);
        if (isDisabled?.Invoke(date) == true) return (null, CdsDateEntryError.Disabled);
        return (date, CdsDateEntryError.None);
    }

    /// <summary>The message shown for a rejected entry.</summary>
    public static string Message(CdsDateEntryError error, string? format, DateOnly? min, DateOnly? max) => error switch
    {
        CdsDateEntryError.Unparseable => $"Enter a date as {Placeholder(format)}",
        CdsDateEntryError.BeforeMin => min is { } lo ? $"Choose {Format(lo, format)} or later" : "Choose a later date",
        CdsDateEntryError.AfterMax => max is { } hi ? $"Choose {Format(hi, format)} or earlier" : "Choose an earlier date",
        CdsDateEntryError.Disabled => "This date is not available",
        CdsDateEntryError.EndBeforeStart => "The end date can't be before the start date",
        _ => string.Empty,
    };

    /// <summary>
    /// True when <paramref name="newText"/> is <paramref name="oldText"/> with one space typed into it (anywhere): the
    /// Space key pressed in a field that keeps focus while the calendar is driven by the keyboard.
    /// </summary>
    public static bool IsTypedSpace(string? oldText, string? newText)
    {
        oldText ??= string.Empty;
        newText ??= string.Empty;
        if (newText.Length != oldText.Length + 1) return false;
        var i = 0;
        while (i < oldText.Length && oldText[i] == newText[i]) i++;
        return newText[i] == ' ' && string.CompareOrdinal(oldText, i, newText, i + 1, oldText.Length - i) == 0;
    }

    /// <summary>Converts a nullable <see cref="DateTime"/> to a date.</summary>
    public static DateOnly? ToDate(DateTime? value) => value is { } v ? DateOnly.FromDateTime(v) : null;

    /// <summary>Converts a nullable date to a <see cref="DateTime"/> at midnight.</summary>
    public static DateTime? ToDateTime(DateOnly? value) => value is { } v ? v.ToDateTime(TimeOnly.MinValue) : null;
}
