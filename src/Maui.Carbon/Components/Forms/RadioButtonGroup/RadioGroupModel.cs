namespace Maui.Carbon;

/// <summary>
/// Pure logic of a group of radio buttons or radio tiles: which item arrow keys move to (wrapping, skipping disabled
/// items) and which item stays in the Tab order. Shared by <see cref="CdsRadioButtonGroup"/> and <see cref="CdsTileGroup"/>.
/// </summary>
internal static class RadioGroupModel
{
    /// <summary>Arrow-key direction: Down and Right move forward (+1), Up and Left move back (-1), others are not handled (0).</summary>
    public static int Direction(CdsKey key) => key switch
    {
        CdsKey.Down or CdsKey.Right => 1,
        CdsKey.Up or CdsKey.Left => -1,
        _ => 0,
    };

    /// <summary>
    /// The index of the next enabled item from <paramref name="current"/> in <paramref name="direction"/> (+1 or -1),
    /// wrapping around the ends. A <paramref name="current"/> of -1 (nothing selected or focused) starts at the first
    /// enabled item going forward and the last going back. Returns -1 when no item is enabled; returns
    /// <paramref name="current"/> when it is the only enabled one.
    /// </summary>
    public static int NextEnabled(IReadOnlyList<bool> enabled, int current, int direction)
    {
        var count = enabled.Count;
        if (count == 0 || direction == 0) return -1;
        direction = direction > 0 ? 1 : -1;
        if (current < 0 || current >= count)
            return direction > 0 ? FirstEnabled(enabled) : LastEnabled(enabled);
        for (var step = 1; step <= count; step++)
        {
            var index = (((current + direction * step) % count) + count) % count;
            if (enabled[index]) return index;
        }
        return -1;
    }

    /// <summary>The index of the first enabled item, or -1.</summary>
    public static int FirstEnabled(IReadOnlyList<bool> enabled)
    {
        for (var i = 0; i < enabled.Count; i++)
            if (enabled[i]) return i;
        return -1;
    }

    /// <summary>The index of the last enabled item, or -1.</summary>
    public static int LastEnabled(IReadOnlyList<bool> enabled)
    {
        for (var i = enabled.Count - 1; i >= 0; i--)
            if (enabled[i]) return i;
        return -1;
    }

    /// <summary>
    /// The one item that is in the Tab order: the selected item when it is enabled, otherwise the first enabled item
    /// (with nothing selected, Tab lands on the first). -1 when nothing can take focus.
    /// </summary>
    public static int TabStop(IReadOnlyList<bool> enabled, int selectedIndex) =>
        selectedIndex >= 0 && selectedIndex < enabled.Count && enabled[selectedIndex] ? selectedIndex : FirstEnabled(enabled);

    /// <summary>The index of the first item whose value equals <paramref name="value"/>, or -1 (a <c>null</c> value selects nothing).</summary>
    public static int IndexOfValue(IReadOnlyList<object?> values, object? value)
    {
        if (value is null) return -1;
        for (var i = 0; i < values.Count; i++)
            if (Equals(values[i], value)) return i;
        return -1;
    }
}
