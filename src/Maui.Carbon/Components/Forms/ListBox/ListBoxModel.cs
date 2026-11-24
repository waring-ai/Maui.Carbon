namespace Maui.Carbon;

/// <summary>One row of a list box.</summary>
/// <param name="Value">The data item the row stands for.</param>
/// <param name="Text">The row text (from <c>ItemToString</c>).</param>
/// <param name="IsDisabled">Disabled rows are skipped by the highlight and cannot be chosen.</param>
internal readonly record struct ListBoxEntry(object? Value, string Text, bool IsDisabled = false);

/// <summary>What the owner should do after a key reached a list box.</summary>
internal enum ListBoxAction
{
    /// <summary>Nothing beyond moving the highlight.</summary>
    None,

    /// <summary>Choose the highlighted row.</summary>
    Choose,

    /// <summary>Close the list.</summary>
    Close,

    /// <summary>Open the list (closed field only).</summary>
    Open,
}

/// <summary>The outcome of a key: whether the key was consumed and what the owner does.</summary>
internal readonly record struct ListBoxKeyResult(bool Handled, ListBoxAction Action)
{
    public static readonly ListBoxKeyResult Ignored = new(false, ListBoxAction.None);
}

/// <summary>
/// The pure state of a list box's virtual focus: the visible rows, the highlighted index, keyboard movement that skips
/// disabled rows and wraps at the ends, and type-ahead. Views only render it.
/// </summary>
internal sealed class ListBoxModel
{
    /// <summary>Type-ahead characters typed within this many milliseconds build one search string (Downshift: 500).</summary>
    public const long TypeAheadWindowMilliseconds = 500;

    const long NeverTyped = -1_000_000;

    IReadOnlyList<ListBoxEntry> _rows;
    string _typed = "";
    long _typedAt = NeverTyped;

    public ListBoxModel(IReadOnlyList<ListBoxEntry>? rows = null)
    {
        _rows = rows ?? [];
    }

    /// <summary>The visible rows in order.</summary>
    public IReadOnlyList<ListBoxEntry> Rows => _rows;

    /// <summary>The highlighted row index, or -1.</summary>
    public int Highlighted { get; private set; } = -1;

    /// <summary>The highlighted row, if any.</summary>
    public ListBoxEntry? HighlightedRow => Highlighted >= 0 && Highlighted < _rows.Count ? _rows[Highlighted] : null;

    /// <summary>Replaces the rows. With <paramref name="keepHighlight"/> the highlight follows its row when it is still present.</summary>
    public void SetRows(IReadOnlyList<ListBoxEntry> rows, bool keepHighlight = false)
    {
        var previous = HighlightedRow;
        _rows = rows;
        Highlighted = -1;
        if (keepHighlight && previous is { } p)
        {
            for (var i = 0; i < rows.Count; i++)
            {
                if (!rows[i].IsDisabled && Equals(rows[i].Value, p.Value) && rows[i].Text == p.Text)
                {
                    Highlighted = i;
                    break;
                }
            }
        }
    }

    /// <summary>Highlights a row; false (and no change) for an invalid or disabled index.</summary>
    public bool SetHighlight(int index)
    {
        if (index < 0 || index >= _rows.Count || _rows[index].IsDisabled) return false;
        Highlighted = index;
        return true;
    }

    /// <summary>Clears the highlight.</summary>
    public void ClearHighlight() => Highlighted = -1;

    /// <summary>Highlights the first enabled row; false when there is none.</summary>
    public bool MoveFirst() => Scan(0, 1, wrap: false);

    /// <summary>Highlights the last enabled row; false when there is none.</summary>
    public bool MoveLast() => Scan(_rows.Count - 1, -1, wrap: false);

    /// <summary>Moves down to the next enabled row, wrapping to the first. From nothing it lands on the first.</summary>
    public bool MoveNext() => Highlighted < 0 ? MoveFirst() : Scan(Highlighted + 1, 1, wrap: true);

    /// <summary>Moves up to the previous enabled row, wrapping to the last. From nothing it lands on the last.</summary>
    public bool MovePrevious() => Highlighted < 0 ? MoveLast() : Scan(Highlighted - 1, -1, wrap: true);

    bool Scan(int start, int step, bool wrap)
    {
        var count = _rows.Count;
        if (count == 0) return false;
        var index = start;
        for (var n = 0; n < count; n++)
        {
            if (index < 0 || index >= count)
            {
                if (!wrap) return false;
                index = index < 0 ? count - 1 : 0;
            }
            if (!_rows[index].IsDisabled)
            {
                Highlighted = index;
                return true;
            }
            index += step;
        }
        return false;
    }

    /// <summary>Highlights the enabled row whose value equals <paramref name="value"/>; false (and no change) when there is none.</summary>
    public bool HighlightValue(object? value)
    {
        if (value is null) return false;
        for (var i = 0; i < _rows.Count; i++)
        {
            if (!_rows[i].IsDisabled && Equals(_rows[i].Value, value))
            {
                Highlighted = i;
                return true;
            }
        }
        return false;
    }

    /// <summary>Highlights the row whose value equals <paramref name="value"/> (enabled only); else the first enabled row.</summary>
    public bool HighlightValueOrFirst(object? value)
    {
        if (value is not null)
        {
            for (var i = 0; i < _rows.Count; i++)
            {
                if (!_rows[i].IsDisabled && Equals(_rows[i].Value, value))
                {
                    Highlighted = i;
                    return true;
                }
            }
        }
        return MoveFirst();
    }

    /// <summary>
    /// Handles a key while the list is open. <paramref name="textField"/> is true when a text field has focus (combo box,
    /// filterable multi-select): Home, End and Space then belong to the text and are not handled.
    /// </summary>
    public ListBoxKeyResult HandleOpenKey(CdsKey key, bool textField)
    {
        switch (key)
        {
            case CdsKey.Down:
                MoveNext();
                return new(true, ListBoxAction.None);
            case CdsKey.Up:
                MovePrevious();
                return new(true, ListBoxAction.None);
            case CdsKey.Home when !textField:
                MoveFirst();
                return new(true, ListBoxAction.None);
            case CdsKey.End when !textField:
                MoveLast();
                return new(true, ListBoxAction.None);
            case CdsKey.Enter:
                if (Highlighted >= 0) return new(true, ListBoxAction.Choose);
                return textField ? ListBoxKeyResult.Ignored : new(true, ListBoxAction.Close);
            case CdsKey.Space when !textField:
                return new(true, Highlighted >= 0 ? ListBoxAction.Choose : ListBoxAction.Close);
            case CdsKey.Escape:
                return new(true, ListBoxAction.Close);
            case CdsKey.Tab:
                return new(false, ListBoxAction.Close);
            default:
                return ListBoxKeyResult.Ignored;
        }
    }

    /// <summary>Handles a key on the closed field: Down and Up open the list.</summary>
    public static ListBoxKeyResult HandleClosedKey(CdsKey key) =>
        key is CdsKey.Down or CdsKey.Up ? new(true, ListBoxAction.Open) : ListBoxKeyResult.Ignored;

    /// <summary>
    /// Type-ahead: jumps to the next enabled row whose text starts with the typed characters. A single repeated letter
    /// cycles through the rows that start with it. <paramref name="nowMilliseconds"/> is a monotonic clock.
    /// </summary>
    public bool TypeAhead(char character, long nowMilliseconds)
    {
        if (char.IsControl(character)) return false;
        if (nowMilliseconds - _typedAt > TypeAheadWindowMilliseconds) _typed = "";
        _typedAt = nowMilliseconds;
        _typed += char.ToLowerInvariant(character);

        var allSame = _typed.All(c => c == _typed[0]);
        var search = allSame ? _typed[..1] : _typed;
        // One letter (or a repeated one) moves on from the current row; a longer string may stay where it is.
        var from = allSame ? Highlighted + 1 : Math.Max(Highlighted, 0);
        var count = _rows.Count;
        for (var n = 0; n < count; n++)
        {
            var index = (((from + n) % count) + count) % count;
            if (!_rows[index].IsDisabled && _rows[index].Text.StartsWith(search, StringComparison.OrdinalIgnoreCase))
            {
                Highlighted = index;
                return true;
            }
        }
        return false;
    }

    /// <summary>Forgets the type-ahead buffer.</summary>
    public void ResetTypeAhead()
    {
        _typed = "";
        _typedAt = NeverTyped;
    }
}

/// <summary>Pure helpers of the list box layer: filtering, ordering, scrolling and the row separators.</summary>
internal static class ListBoxLogic
{
    /// <summary>
    /// What a screen reader hears for a highlighted row: its text, its state ("selected" for the chosen row of a single
    /// select; "checked" or "not checked" in a multi-select; "unavailable" for a disabled row) and its position, as
    /// <c>aria-activedescendant</c> with <c>aria-selected</c>, <c>aria-posinset</c> and <c>aria-setsize</c> would:
    /// "Frankfurt, selected, 3 of 4".
    /// </summary>
    public static string RowAnnouncement(ListBoxEntry row, int index, int count, bool chosen, bool multi)
    {
        var parts = new List<string>(4) { row.Text };
        if (multi) parts.Add(chosen ? "checked" : "not checked");
        else if (chosen) parts.Add("selected");
        if (row.IsDisabled) parts.Add("unavailable");
        parts.Add($"{index + 1} of {count}");
        return string.Join(", ", parts.Where(p => p.Length > 0));
    }

    /// <summary>The announcement of an open list with no row highlighted: "4 options available", "No options available".</summary>
    public static string CountAnnouncement(int count) => count switch
    {
        0 => "No options available",
        1 => "1 option available",
        _ => $"{count} options available",
    };

    /// <summary>The default combo box filter: case-insensitive "contains" on the row text.</summary>
    public static bool DefaultFilter(string text, string input) =>
        text.Contains(input, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Filters the rows for the text typed into a combo box. An empty input, or an input equal to the selected item's text,
    /// shows the full list (Carbon).
    /// </summary>
    public static List<ListBoxEntry> Filter(IReadOnlyList<ListBoxEntry> all, string? input, string? selectedText,
        Func<object?, string, bool>? shouldFilterItem)
    {
        if (string.IsNullOrEmpty(input) ||
            (selectedText is not null && string.Equals(input, selectedText, StringComparison.OrdinalIgnoreCase)))
            return [.. all];
        var result = new List<ListBoxEntry>();
        foreach (var entry in all)
        {
            var keep = shouldFilterItem is null ? DefaultFilter(entry.Text, input) : shouldFilterItem(entry.Value, input);
            if (keep) result.Add(entry);
        }
        return result;
    }

    /// <summary>Moves the entries for which <paramref name="isSelected"/> is true to the top; both groups keep their order.</summary>
    public static List<ListBoxEntry> SelectedFirst(IReadOnlyList<ListBoxEntry> entries, Func<object?, bool> isSelected)
    {
        var top = new List<ListBoxEntry>();
        var rest = new List<ListBoxEntry>();
        foreach (var entry in entries) (isSelected(entry.Value) ? top : rest).Add(entry);
        top.AddRange(rest);
        return top;
    }

    /// <summary>Index of the first row whose value equals <paramref name="value"/>, or -1.</summary>
    public static int IndexOf(IReadOnlyList<ListBoxEntry> rows, object? value)
    {
        if (value is null) return -1;
        for (var i = 0; i < rows.Count; i++)
            if (Equals(rows[i].Value, value)) return i;
        return -1;
    }

    /// <summary>
    /// The scroll offset that brings a row fully into view with the least movement (unchanged when it is already visible).
    /// </summary>
    public static double ScrollTarget(double offset, double viewport, double rowTop, double rowHeight)
    {
        if (viewport <= 0) return offset;
        if (rowTop < offset) return rowTop;
        if (rowTop + rowHeight > offset + viewport) return rowTop + rowHeight - viewport;
        return offset;
    }

    /// <summary>
    /// Whether the 1px <c>border-subtle-01</c> rule above a row is drawn: never on the first row, and not next to the
    /// highlighted or hovered row, nor (single-select) below the selected row.
    /// </summary>
    public static bool SeparatorVisible(int index, int highlighted, int hovered, bool previousSelected, bool multi)
    {
        if (index <= 0) return false;
        if (index == highlighted || index - 1 == highlighted) return false;
        if (index == hovered || index - 1 == hovered) return false;
        if (!multi && previousSelected) return false;
        return true;
    }
}

/// <summary>An ordered set of selected items (multi-select), compared with <see cref="object.Equals(object, object)"/>.</summary>
internal sealed class SelectionSet
{
    readonly List<object?> _items = [];

    public IReadOnlyList<object?> Items => _items;

    public int Count => _items.Count;

    public bool Contains(object? value) => value is not null && _items.Exists(i => Equals(i, value));

    /// <summary>Selects a missing item or deselects a present one; returns the new state.</summary>
    public bool Toggle(object? value)
    {
        if (value is null) return false;
        var index = _items.FindIndex(i => Equals(i, value));
        if (index >= 0)
        {
            _items.RemoveAt(index);
            return false;
        }
        _items.Add(value);
        return true;
    }

    /// <summary>Empties the set; false when it already was.</summary>
    public bool Clear()
    {
        if (_items.Count == 0) return false;
        _items.Clear();
        return true;
    }

    /// <summary>Replaces the content (duplicates and nulls dropped); true when it changed.</summary>
    public bool Replace(IEnumerable<object?>? values)
    {
        var next = new List<object?>();
        if (values is not null)
            foreach (var v in values)
                if (v is not null && !next.Exists(i => Equals(i, v))) next.Add(v);
        if (next.Count == _items.Count && next.Zip(_items).All(p => Equals(p.First, p.Second))) return false;
        _items.Clear();
        _items.AddRange(next);
        return true;
    }
}
