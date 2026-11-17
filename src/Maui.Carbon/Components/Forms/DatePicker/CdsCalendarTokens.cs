namespace Maui.Carbon;

/// <summary>Color keys of one calendar day. <c>null</c> means transparent (background) or none (ring, dot).</summary>
/// <param name="Background">Cell fill.</param>
/// <param name="Text">Day number color.</param>
/// <param name="Ring">2px inset outline color (keyboard cursor, today when selected), or <c>null</c>.</param>
/// <param name="Dot">Color of the 4px dot under today's number, or <c>null</c> when there is none.</param>
/// <param name="IsSemiBold">Today's number is semibold.</param>
public readonly record struct CdsCalendarDayVisuals(string? Background, string Text, string? Ring, string? Dot, bool IsSemiBold);

/// <summary>
/// Maps a calendar day's state to Carbon tokens (from the flatpickr styles in <c>packages/styles/scss/components/date-picker</c>)
/// and holds the calendar's measurements.
/// </summary>
public static class CdsCalendarTokens
{
    /// <summary>Calendar width: 18rem.</summary>
    public const double Width = 288;

    /// <summary>Calendar height: 21rem. It does not grow with the field size.</summary>
    public const double Height = 336;

    /// <summary>Height of the month header and the weekday row: 2.5rem.</summary>
    public const double HeaderHeight = 40;

    /// <summary>Size of a day cell: 2.5rem.</summary>
    public const double DayHeight = 40;

    /// <summary>Resolves a day's colors.</summary>
    /// <param name="inMonth">The day belongs to the visible month (neighbouring-month days use <c>text-helper</c>).</param>
    /// <param name="isToday">Today: <c>link-primary</c>, semibold, with a dot.</param>
    /// <param name="isDisabled">Outside min..max or disabled: <c>text-disabled</c>, no hover.</param>
    /// <param name="isSelected">The selected day or an end of the selected range: <c>button-primary</c> with <c>text-on-color</c>.</param>
    /// <param name="inRange">Between the ends of a range: <c>highlight</c>.</param>
    /// <param name="isHover">The pointer is over the day: <c>layer-hover-01</c> (not on selected or range days).</param>
    /// <param name="isCursor">The keyboard cursor is on the day: a 2px <c>button-primary</c> inset outline (<c>focus</c> on a selected day).</param>
    public static CdsCalendarDayVisuals Resolve(bool inMonth, bool isToday, bool isDisabled, bool isSelected, bool inRange, bool isHover, bool isCursor)
    {
        if (isDisabled)
            return new(null, CdsColorKeys.TextDisabled, isCursor ? CdsColorKeys.ButtonPrimary : null, isToday ? CdsColorKeys.LinkPrimary : null, isToday);
        if (isSelected)
            return new(CdsColorKeys.ButtonPrimary, CdsColorKeys.TextOnColor, isToday || isCursor ? CdsColorKeys.Focus : null, null, isToday);

        string? background = inRange ? CdsColorKeys.Highlight : isHover ? CdsColorKeys.LayerHover01 : null;
        var text = inRange ? CdsColorKeys.TextPrimary
            : isToday ? CdsColorKeys.LinkPrimary
            : inMonth ? CdsColorKeys.TextPrimary
            : CdsColorKeys.TextHelper;
        return new(background, text, isCursor ? CdsColorKeys.ButtonPrimary : null, isToday ? CdsColorKeys.LinkPrimary : null, isToday);
    }

    /// <summary>Resolves a month navigation button: hover <c>layer-hover-01</c>, <c>icon-primary</c>, <c>icon-disabled</c> at the limit.</summary>
    public static (string? Background, string Icon) ResolveNavigation(bool isEnabled, bool isHover) =>
        isEnabled
            ? (isHover ? CdsColorKeys.LayerHover01 : null, CdsColorKeys.IconPrimary)
            : (null, CdsColorKeys.IconDisabled);

    /// <summary>Field width of a date picker type in dips (single 288, simple 120 or 91.2 short, range 143.5 per field).</summary>
    public static double FieldWidth(CdsDatePickerType type, bool isShort, bool hasStatusIcon) => type switch
    {
        CdsDatePickerType.Single => 288,
        CdsDatePickerType.Range => 143.5,
        _ => isShort ? 91.2 : hasStatusIcon ? 152 : 120,
    };
}
