namespace Maui.Carbon;

/// <summary>Pure logic and measurements of the structured list (from <c>packages/styles/scss/components/structured-list</c>).</summary>
public static class CdsStructuredListModel
{
    /// <summary>Width of the leading radio column of a selectable list (2rem).</summary>
    public const double IconColumnWidth = 32;

    /// <summary>Minimum height of the header row (2.5rem).</summary>
    public const double HeaderHeight = 40;

    /// <summary>
    /// Moves the selection by <paramref name="delta"/> (+1 / -1) to the next enabled row, wrapping around like a native
    /// radio group. With nothing selected (-1) +1 goes to the first and -1 to the last enabled row. Returns
    /// <paramref name="current"/> when no row is enabled.
    /// </summary>
    public static int Move(int current, int delta, IReadOnlyList<bool> enabled)
    {
        var count = enabled.Count;
        if (count == 0 || delta == 0) return current;
        var step = delta > 0 ? 1 : -1;
        var index = current;
        if (index < 0 || index >= count) index = step > 0 ? -1 : count;
        for (var i = 0; i < count; i++)
        {
            index = ((index + step) % count + count) % count;
            if (enabled[index]) return index;
        }
        return current;
    }

    /// <summary>
    /// Padding of a content cell, following Carbon's cascade. Header cells have 8 below instead of 24. Condensed cells have 8
    /// all round, also in a selectable list (condensed wins over the selection's 16 side padding). The first cell of a
    /// non-selectable row keeps 16 at the start, condensed or not. Otherwise a selectable list pads cells 16 on both sides
    /// and other cells have 8. Flush removes the start padding (and pads 16 at the end) and wins over everything.
    /// </summary>
    public static Thickness CellPadding(bool header, bool condensed, bool flush, bool selection, bool first)
    {
        var left = flush ? 0 : first && !selection ? 16 : condensed ? 8 : selection ? 16 : 8;
        var right = flush ? 16 : condensed ? 8 : selection ? 16 : 8;
        var top = condensed ? 8 : 16;
        var bottom = condensed ? 8 : header ? 8 : 24;
        return new Thickness(left, top, right, bottom);
    }

    /// <summary>Padding of the radio cell: 16 at the start, none at the end; 10 above and below when condensed.</summary>
    public static Thickness IconCellPadding(bool header, bool condensed)
    {
        if (condensed) return new Thickness(16, header ? 8 : 10, 0, header ? 8 : 10);
        return new Thickness(16, 16, 0, header ? 8 : 24);
    }
}

/// <summary>Maps structured list parts and states to Carbon tokens.</summary>
public static class CdsStructuredListTokens
{
    /// <summary>Row fill: <c>layer-selected-01</c> when selected, <c>layer-hover-01</c> on hover, none otherwise (the list is <c>layer-01</c>).</summary>
    public static string? RowFill(bool selected, bool hover, bool disabled)
    {
        if (disabled) return null;
        if (selected) return CdsColorKeys.LayerSelected01;
        return hover ? CdsColorKeys.LayerHover01 : null;
    }

    /// <summary>
    /// The 1px rule above a body row: <c>border-subtle-01</c>, taking the selected or hover color of the row itself or of
    /// the row above it so no hairline shows next to a filled row.
    /// </summary>
    public static string Rule(bool rowSelected, bool rowHover, bool aboveSelected, bool aboveHover)
    {
        if (rowSelected || aboveSelected) return CdsColorKeys.LayerSelected01;
        if (rowHover || aboveHover) return CdsColorKeys.LayerHover01;
        return CdsColorKeys.BorderSubtle01;
    }

    /// <summary>Cell text: header and active rows <c>text-primary</c>, body <c>text-secondary</c>, disabled <c>text-disabled</c>.</summary>
    public static string CellText(bool header, bool active, bool disabled)
    {
        if (header) return CdsColorKeys.TextPrimary;
        if (disabled) return CdsColorKeys.TextDisabled;
        return active ? CdsColorKeys.TextPrimary : CdsColorKeys.TextSecondary;
    }

    /// <summary>The radio glyph color.</summary>
    public static string Icon(bool disabled) => disabled ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;

    /// <summary>The list background.</summary>
    public static string Background => CdsColorKeys.Layer01;
}
