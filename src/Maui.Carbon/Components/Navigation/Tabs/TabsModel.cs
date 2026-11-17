namespace Maui.Carbon;

/// <summary>
/// Pure index logic for roving-focus tab-like controls (<see cref="CdsTabs"/>, <see cref="CdsContentSwitcher"/>, the
/// breadcrumb overflow list): next enabled with wrap, first and last enabled, selection coercion and selection
/// after a removal.
/// </summary>
internal static class TabsModel
{
    /// <summary>
    /// The next enabled index from <paramref name="from"/> in <paramref name="direction"/> (+1 or -1), wrapping at the ends when
    /// <paramref name="wrap"/>; -1 when nothing is enabled. A negative <paramref name="from"/> starts before the first item.
    /// </summary>
    public static int Next(IReadOnlyList<bool> enabled, int from, int direction, bool wrap = true)
    {
        var n = enabled.Count;
        if (n == 0) return -1;
        var step = direction >= 0 ? 1 : -1;
        var start = from < 0 || from >= n ? (step > 0 ? -1 : n) : from;
        for (var i = 1; i <= n; i++)
        {
            var index = start + step * i;
            if (index < 0 || index >= n)
            {
                if (!wrap) return -1;
                index = ((index % n) + n) % n;
            }
            if (enabled[index]) return index;
        }
        return -1;
    }

    /// <summary>The first enabled index, or -1.</summary>
    public static int First(IReadOnlyList<bool> enabled) => Next(enabled, -1, 1, wrap: false);

    /// <summary>The last enabled index, or -1.</summary>
    public static int Last(IReadOnlyList<bool> enabled) => Next(enabled, enabled.Count, -1, wrap: false);

    /// <summary>
    /// Coerces a requested selection: itself when in range and enabled, else the next enabled item after it, else the
    /// previous one; -1 when nothing is enabled or the list is empty.
    /// </summary>
    public static int Coerce(IReadOnlyList<bool> enabled, int selected)
    {
        if (enabled.Count == 0) return -1;
        selected = Math.Clamp(selected, 0, enabled.Count - 1);
        if (enabled[selected]) return selected;
        var after = Next(enabled, selected, 1, wrap: false);
        return after >= 0 ? after : Next(enabled, selected, -1, wrap: false);
    }

    /// <summary>The index a key moves to from <paramref name="current"/> (Left/Up previous, Right/Down next, Home, End), or -1 for other keys.</summary>
    public static int Target(CdsKey key, IReadOnlyList<bool> enabled, int current, bool vertical = false) => key switch
    {
        CdsKey.Left when !vertical => Next(enabled, current, -1),
        CdsKey.Right when !vertical => Next(enabled, current, 1),
        CdsKey.Up when vertical => Next(enabled, current, -1),
        CdsKey.Down when vertical => Next(enabled, current, 1),
        CdsKey.Home => First(enabled),
        CdsKey.End => Last(enabled),
        _ => -1,
    };

    /// <summary>
    /// The selection after the item at <paramref name="removed"/> left a list that now has <paramref name="enabledAfter"/>
    /// entries: items before the selection shift it back, removing the selected item selects its neighbour.
    /// </summary>
    public static int AfterRemoval(IReadOnlyList<bool> enabledAfter, int selected, int removed)
    {
        if (enabledAfter.Count == 0) return -1;
        if (removed < selected) selected--;
        return Coerce(enabledAfter, selected);
    }

    /// <summary>The horizontal scroll offset that brings an item (from <paramref name="itemStart"/> to <paramref name="itemEnd"/>) into a viewport, keeping the current offset when it is already visible.</summary>
    public static double ScrollToReveal(double itemStart, double itemEnd, double viewStart, double viewWidth)
    {
        if (itemStart < viewStart) return itemStart;
        if (itemEnd > viewStart + viewWidth) return itemEnd - viewWidth;
        return viewStart;
    }
}
