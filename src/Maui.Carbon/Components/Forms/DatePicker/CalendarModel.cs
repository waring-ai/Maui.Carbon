using System.Globalization;

namespace Maui.Carbon;

/// <summary>One cell of the six-by-seven calendar grid.</summary>
/// <param name="Date">The day.</param>
/// <param name="InMonth">The day belongs to the visible month (the others are leading and trailing neighbours).</param>
/// <param name="IsToday">The day is today.</param>
/// <param name="IsDisabled">Outside min..max or on a disabled day.</param>
/// <param name="IsSelected">The selected day, or the start or end of the selected range.</param>
/// <param name="IsRangeStart">First day of a complete range.</param>
/// <param name="IsRangeEnd">Last day of a complete range.</param>
/// <param name="InRange">Strictly between the start and end of a complete range.</param>
internal readonly record struct CalendarDay(
    DateOnly Date, bool InMonth, bool IsToday, bool IsDisabled, bool IsSelected, bool IsRangeStart, bool IsRangeEnd, bool InRange);

/// <summary>What picking a day did.</summary>
internal enum CalendarPick
{
    /// <summary>The day is disabled; nothing changed.</summary>
    Ignored,

    /// <summary>A single date was chosen, or the end of a range: the selection is complete.</summary>
    Completed,

    /// <summary>The start of a range was chosen; the end is still open.</summary>
    RangeStarted,
}

/// <summary>
/// The pure model of the date picker's calendar: the visible month and its six-by-seven grid (first day of the week from
/// the culture, leading and trailing days), today, the keyboard cursor, single and range selection, min/max clamping and
/// disabled days, and keyboard movement (arrows by day and week, PageUp and PageDown by month, Home and End to the week's
/// ends). The calendar view only renders it.
/// </summary>
internal sealed class CalendarModel
{
    /// <summary>Weeks shown: always six so the calendar never changes height.</summary>
    public const int Weeks = 6;

    DateOnly _visibleMonth;

    public CalendarModel(CultureInfo culture, DateOnly today, bool isRange)
    {
        Culture = culture;
        Today = today;
        IsRange = isRange;
        Cursor = today;
        _visibleMonth = FirstOfMonth(today);
    }

    public CultureInfo Culture { get; }

    public DateOnly Today { get; }

    /// <summary>Range selection (two picks) instead of a single date.</summary>
    public bool IsRange { get; }

    public DateOnly? Min { get; set; }

    public DateOnly? Max { get; set; }

    /// <summary>Extra disabled days (the application's predicate).</summary>
    public Func<DateOnly, bool>? IsDisabledDate { get; set; }

    /// <summary>The selected date, or the start of the range.</summary>
    public DateOnly? Start { get; private set; }

    /// <summary>The end of the range (range mode only).</summary>
    public DateOnly? End { get; private set; }

    /// <summary>The day the keyboard is on.</summary>
    public DateOnly Cursor { get; private set; }

    /// <summary>The first day of the visible month.</summary>
    public DateOnly VisibleMonth => _visibleMonth;

    /// <summary>The first day of the week in this culture.</summary>
    public DayOfWeek FirstDayOfWeek => Culture.DateTimeFormat.FirstDayOfWeek;

    /// <summary>The heading of the visible month ("March 2026").</summary>
    public string MonthLabel => _visibleMonth.ToString("Y", Culture);

    /// <summary>The weekday headings in column order, shortened ("Su", "Mo"...).</summary>
    public IReadOnlyList<string> WeekdayLabels()
    {
        var labels = new string[7];
        for (var i = 0; i < 7; i++)
            labels[i] = Culture.DateTimeFormat.GetShortestDayName((DayOfWeek)(((int)FirstDayOfWeek + i) % 7));
        return labels;
    }

    /// <summary>The first day of <paramref name="date"/>'s month.</summary>
    public static DateOnly FirstOfMonth(DateOnly date) => new(date.Year, date.Month, 1);

    /// <summary>True when <paramref name="date"/> cannot be chosen.</summary>
    public bool IsDisabled(DateOnly date) =>
        (Min is { } lo && date < lo) || (Max is { } hi && date > hi) || IsDisabledDate?.Invoke(date) == true;

    /// <summary>Clamps a date into min..max.</summary>
    public DateOnly Clamp(DateOnly date)
    {
        if (Min is { } lo && date < lo) return lo;
        if (Max is { } hi && date > hi) return hi;
        return date;
    }

    /// <summary>Sets the selection (a single date: <paramref name="end"/> is ignored).</summary>
    public void SetSelection(DateOnly? start, DateOnly? end)
    {
        Start = start;
        End = IsRange && start is not null ? end : null;
        if (IsRange && Start is { } s && End is { } e && e < s) (Start, End) = (e, s);
    }

    /// <summary>Shows the month of <paramref name="date"/> and puts the cursor on the day (clamped into min..max).</summary>
    public void Show(DateOnly date)
    {
        Cursor = Clamp(date);
        _visibleMonth = FirstOfMonth(Cursor);
    }

    /// <summary>Starts the keyboard cursor on the selected date, else today (clamped).</summary>
    public void ShowSelectionOrToday() => Show(Start ?? Today);

    /// <summary>True when the previous month has at least one choosable day within min.</summary>
    public bool CanGoPrevious => Min is not { } lo || lo < _visibleMonth;

    /// <summary>True when the next month starts on or before max.</summary>
    public bool CanGoNext => Max is not { } hi || NextMonth(_visibleMonth) <= hi;

    /// <summary>Shows the previous month (the cursor keeps its day of the month, clamped). False at the min limit.</summary>
    public bool GoPrevious() => GoMonth(-1);

    /// <summary>Shows the next month (the cursor keeps its day of the month, clamped). False at the max limit.</summary>
    public bool GoNext() => GoMonth(1);

    bool GoMonth(int delta)
    {
        if (delta < 0 ? !CanGoPrevious : !CanGoNext) return false;
        var target = AddMonths(Cursor, delta);
        Cursor = Clamp(target);
        _visibleMonth = FirstOfMonth(AddMonths(_visibleMonth, delta));
        // After clamping the cursor may sit in another month: keep the grid on the cursor's month.
        if (FirstOfMonth(Cursor) != _visibleMonth) _visibleMonth = FirstOfMonth(Cursor);
        return true;
    }

    static DateOnly NextMonth(DateOnly firstOfMonth) =>
        firstOfMonth.Year == 9999 && firstOfMonth.Month == 12 ? DateOnly.MaxValue : firstOfMonth.AddMonths(1);

    static DateOnly AddMonths(DateOnly date, int months)
    {
        var minDate = DateOnly.MinValue;
        var maxDate = DateOnly.MaxValue;
        var total = (date.Year * 12) + (date.Month - 1) + months;
        var year = Math.Clamp(total / 12, minDate.Year, maxDate.Year);
        var month = Math.Clamp(total - (total / 12 * 12), 0, 11) + 1;
        var day = Math.Min(date.Day, DateTime.DaysInMonth(year, month));
        return new DateOnly(year, month, day);
    }

    static DateOnly AddDays(DateOnly date, int days)
    {
        var number = Math.Clamp(date.DayNumber + days, DateOnly.MinValue.DayNumber, DateOnly.MaxValue.DayNumber);
        return DateOnly.FromDayNumber(number);
    }

    /// <summary>
    /// Moves the keyboard cursor: Left and Right by a day, Up and Down by a week, PageUp and PageDown by a month, Home and End
    /// to the start and end of the week. The result is clamped into min..max and the visible month follows. Returns true when
    /// the key is a calendar key (even when clamping kept the cursor in place).
    /// </summary>
    public bool MoveCursor(CdsKey key)
    {
        DateOnly target;
        switch (key)
        {
            case CdsKey.Left: target = AddDays(Cursor, -1); break;
            case CdsKey.Right: target = AddDays(Cursor, 1); break;
            case CdsKey.Up: target = AddDays(Cursor, -7); break;
            case CdsKey.Down: target = AddDays(Cursor, 7); break;
            case CdsKey.PageUp: target = AddMonths(Cursor, -1); break;
            case CdsKey.PageDown: target = AddMonths(Cursor, 1); break;
            case CdsKey.Home: target = AddDays(Cursor, -DaysFromWeekStart(Cursor)); break;
            case CdsKey.End: target = AddDays(Cursor, 6 - DaysFromWeekStart(Cursor)); break;
            default: return false;
        }
        Cursor = Clamp(target);
        _visibleMonth = FirstOfMonth(Cursor);
        return true;
    }

    /// <summary>How many days <paramref name="date"/> is after the first day of its week (0..6).</summary>
    public int DaysFromWeekStart(DateOnly date) => (((int)date.DayOfWeek - (int)FirstDayOfWeek) + 7) % 7;

    /// <summary>The 42 cells of the visible month, row by row, starting on the first day of the week.</summary>
    public IReadOnlyList<CalendarDay> BuildGrid()
    {
        var first = AddDays(_visibleMonth, -DaysFromWeekStart(_visibleMonth));
        var days = new CalendarDay[Weeks * 7];
        var complete = IsRange && Start is not null && End is not null;
        for (var i = 0; i < days.Length; i++)
        {
            var date = AddDays(first, i);
            var selected = (Start is { } s && date == s) || (End is { } e && date == e);
            days[i] = new CalendarDay(
                date,
                date.Month == _visibleMonth.Month && date.Year == _visibleMonth.Year,
                date == Today,
                IsDisabled(date),
                selected,
                complete && date == Start,
                complete && date == End,
                complete && date > Start && date < End);
        }
        return days;
    }

    /// <summary>
    /// Picks a day. Single: it becomes the selection. Range: the first pick starts a range (and clears the end), the second
    /// completes it (an earlier second pick swaps start and end), a third starts again.
    /// </summary>
    public CalendarPick Pick(DateOnly date)
    {
        if (IsDisabled(date)) return CalendarPick.Ignored;
        Cursor = date;
        _visibleMonth = FirstOfMonth(date);
        if (!IsRange)
        {
            Start = date;
            End = null;
            return CalendarPick.Completed;
        }
        if (Start is null || End is not null)
        {
            Start = date;
            End = null;
            return CalendarPick.RangeStarted;
        }
        if (date < Start)
        {
            End = Start;
            Start = date;
        }
        else
        {
            End = date;
        }
        return CalendarPick.Completed;
    }

    /// <summary>Picks the day under the keyboard cursor.</summary>
    public CalendarPick PickCursor() => Pick(Cursor);
}
