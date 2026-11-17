using System.Globalization;

namespace Maui.Carbon;

/// <summary>One row as the <see cref="DataTableModel"/> sees it: a stable key, the disabled flag and the column values.</summary>
internal sealed class DataTableRowData
{
    /// <summary>Creates a row.</summary>
    public DataTableRowData(string key, object? item, IReadOnlyList<object?> values, IReadOnlyList<string> texts, bool isDisabled = false)
    {
        Key = key;
        Item = item;
        Values = values;
        Texts = texts;
        IsDisabled = isDisabled;
    }

    /// <summary>The stable row id. It does not change when rows are sorted or filtered.</summary>
    public string Key { get; }

    /// <summary>The source object (the row's data).</summary>
    public object? Item { get; }

    /// <summary>The raw value of each column, used for sorting.</summary>
    public IReadOnlyList<object?> Values { get; }

    /// <summary>The display text of each column, used for searching.</summary>
    public IReadOnlyList<string> Texts { get; }

    /// <summary>A disabled row's selection control is disabled; select-all skips it.</summary>
    public bool IsDisabled { get; }
}

/// <summary>
/// The pure logic of a data table: search filtering, stable culture-aware sorting (numbers numerically), the sort cycle,
/// selection (multi, radio and the tri-state select-all over enabled visible rows), expansion and the batch count.
/// No views: <see cref="CdsDataTable"/> feeds it rows and draws what it reports.
/// </summary>
internal sealed class DataTableModel
{
    readonly CultureInfo _culture;
    readonly HashSet<string> _selected = [];
    readonly HashSet<string> _expanded = [];
    IReadOnlyList<DataTableRowData> _rows = [];
    IReadOnlyList<string> _columns = [];
    List<DataTableRowData>? _visible;
    string _search = string.Empty;
    string? _sortKey;
    CdsSortDirection _direction;

    /// <summary>Creates a model. Sorting compares text with <paramref name="culture"/> (default: the current culture).</summary>
    public DataTableModel(CultureInfo? culture = null) => _culture = culture ?? CultureInfo.CurrentCulture;

    /// <summary>All rows in source order.</summary>
    public IReadOnlyList<DataTableRowData> Rows => _rows;

    /// <summary>The column keys, in order; they map a sort key to a value index.</summary>
    public IReadOnlyList<string> Columns => _columns;

    /// <summary>The search text; empty shows every row.</summary>
    public string SearchText
    {
        get => _search;
        set
        {
            value ??= string.Empty;
            if (_search == value) return;
            _search = value;
            _visible = null;
        }
    }

    /// <summary>The key of the sorted column, or <c>null</c>.</summary>
    public string? SortKey => _direction == CdsSortDirection.None ? null : _sortKey;

    /// <summary>The sort direction (<see cref="CdsSortDirection.None"/> when unsorted).</summary>
    public CdsSortDirection SortDirection => _sortKey is null ? CdsSortDirection.None : _direction;

    /// <summary>Replaces the column keys.</summary>
    public void SetColumns(IReadOnlyList<string> keys)
    {
        _columns = keys;
        _visible = null;
    }

    /// <summary>Replaces the rows. Selection and expansion of rows that are gone are dropped; returns <c>true</c> when the selection changed.</summary>
    public bool SetRows(IEnumerable<DataTableRowData> rows)
    {
        _rows = rows.ToList();
        _visible = null;
        var keys = new HashSet<string>(_rows.Select(r => r.Key));
        var changed = _selected.RemoveWhere(k => !keys.Contains(k)) > 0;
        _expanded.RemoveWhere(k => !keys.Contains(k));
        return changed;
    }

    /// <summary>Sets the sort. A <c>None</c> direction or a <c>null</c> key means unsorted.</summary>
    public void SetSort(string? key, CdsSortDirection direction)
    {
        if (key is null || direction == CdsSortDirection.None)
        {
            key = null;
            direction = CdsSortDirection.None;
        }
        if (_sortKey == key && _direction == direction) return;
        _sortKey = key;
        _direction = direction;
        _visible = null;
    }

    /// <summary>The next direction in the cycle none, ascending, descending, none.</summary>
    public static CdsSortDirection NextDirection(CdsSortDirection current) => current switch
    {
        CdsSortDirection.None => CdsSortDirection.Ascending,
        CdsSortDirection.Ascending => CdsSortDirection.Descending,
        _ => CdsSortDirection.None,
    };

    /// <summary>
    /// Applies one header activation: a different column starts ascending; the same column cycles
    /// ascending, descending, none. Returns the new direction.
    /// </summary>
    public CdsSortDirection CycleSort(string key)
    {
        var next = SortKey == key ? NextDirection(SortDirection) : CdsSortDirection.Ascending;
        SetSort(key, next);
        return next;
    }

    /// <summary>The rows that pass the search, in sort order. The sort is stable: equal rows keep their source order.</summary>
    public IReadOnlyList<DataTableRowData> VisibleRows => _visible ??= ComputeVisible();

    List<DataTableRowData> ComputeVisible()
    {
        IEnumerable<DataTableRowData> rows = _rows;
        var term = _search.Trim();
        if (term.Length > 0) rows = rows.Where(r => Matches(r, term));
        var index = SortKey is { } key ? IndexOfColumn(key) : -1;
        if (index >= 0)
        {
            var comparer = Comparer<object?>.Create(Compare);
            rows = _direction == CdsSortDirection.Descending
                ? rows.OrderByDescending(r => ValueAt(r, index), comparer)
                : rows.OrderBy(r => ValueAt(r, index), comparer);
        }
        return rows.ToList();
    }

    int IndexOfColumn(string key)
    {
        for (var i = 0; i < _columns.Count; i++)
            if (_columns[i] == key) return i;
        return -1;
    }

    static object? ValueAt(DataTableRowData row, int index) => index < row.Values.Count ? row.Values[index] : null;

    bool Matches(DataTableRowData row, string term)
    {
        var info = _culture.CompareInfo;
        foreach (var text in row.Texts)
            if (info.IndexOf(text, term, CompareOptions.IgnoreCase) >= 0) return true;
        return false;
    }

    /// <summary>
    /// Compares two cell values: nothing sorts first; numbers (and strings that are numbers) compare numerically;
    /// values of one comparable type use that type; everything else compares as culture-aware text.
    /// </summary>
    public int Compare(object? a, object? b)
    {
        if (a is null && b is null) return 0;
        if (a is null) return -1;
        if (b is null) return 1;
        if (TryNumber(a, out var x) && TryNumber(b, out var y)) return x.CompareTo(y);
        if (a.GetType() == b.GetType() && a is IComparable c && a is not string) return c.CompareTo(b);
        return _culture.CompareInfo.Compare(a.ToString(), b.ToString(), CompareOptions.None);
    }

    bool TryNumber(object value, out double number)
    {
        switch (value)
        {
            case sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal:
                number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return !double.IsNaN(number);
            case string s:
                return double.TryParse(s, NumberStyles.Float, _culture, out number) && !double.IsNaN(number);
            default:
                number = 0;
                return false;
        }
    }

    // ----- selection -----

    /// <summary>The keys of the selected rows.</summary>
    public IReadOnlyCollection<string> SelectedKeys => _selected;

    /// <summary>The number of selected rows (the batch bar's count, including rows hidden by the search).</summary>
    public int SelectedCount => _selected.Count;

    /// <summary>The selected rows in source order.</summary>
    public IReadOnlyList<DataTableRowData> SelectedRows => _rows.Where(r => _selected.Contains(r.Key)).ToList();

    /// <summary>Whether a row is selected.</summary>
    public bool IsSelected(string key) => _selected.Contains(key);

    /// <summary>Selects or deselects a row (multi). Disabled and unknown rows cannot become selected. Returns <c>true</c> when the set changed.</summary>
    public bool SetSelected(string key, bool selected)
    {
        var row = Find(key);
        if (row is null || row.IsDisabled && selected) return false;
        return selected ? _selected.Add(key) : _selected.Remove(key);
    }

    /// <summary>Toggles a row (multi).</summary>
    public bool Toggle(string key) => SetSelected(key, !_selected.Contains(key));

    /// <summary>Selects exactly one row (radio). Disabled and unknown rows are ignored. Returns <c>true</c> when the set changed.</summary>
    public bool SelectOnly(string key)
    {
        var row = Find(key);
        if (row is null || row.IsDisabled) return false;
        if (_selected.Count == 1 && _selected.Contains(key)) return false;
        _selected.Clear();
        _selected.Add(key);
        return true;
    }

    /// <summary>Replaces the selection with the given keys (unknown keys are dropped). Returns <c>true</c> when it changed.</summary>
    public bool ReplaceSelection(IEnumerable<string> keys)
    {
        var known = new HashSet<string>(_rows.Select(r => r.Key));
        var next = new HashSet<string>(keys.Where(known.Contains));
        if (next.SetEquals(_selected)) return false;
        _selected.Clear();
        _selected.UnionWith(next);
        return true;
    }

    /// <summary>Clears the selection; returns <c>true</c> when something was selected.</summary>
    public bool ClearSelection()
    {
        if (_selected.Count == 0) return false;
        _selected.Clear();
        return true;
    }

    /// <summary>
    /// The state of the select-all checkbox over the enabled visible rows: <c>true</c> when all are selected,
    /// <c>null</c> (indeterminate) when some are, <c>false</c> when none are or there is nothing to select.
    /// </summary>
    public bool? SelectAllState
    {
        get
        {
            var enabled = VisibleRows.Where(r => !r.IsDisabled).ToList();
            if (enabled.Count == 0) return false;
            var count = enabled.Count(r => _selected.Contains(r.Key));
            return count == 0 ? false : count == enabled.Count ? true : null;
        }
    }

    /// <summary>
    /// Activates select-all: when every enabled visible row is selected they are all deselected, otherwise they are all
    /// selected. Rows hidden by the search keep their state. Returns <c>true</c> when the set changed.
    /// </summary>
    public bool ToggleSelectAll()
    {
        var enabled = VisibleRows.Where(r => !r.IsDisabled).ToList();
        if (enabled.Count == 0) return false;
        var changed = false;
        if (SelectAllState == true)
            foreach (var row in enabled) changed |= _selected.Remove(row.Key);
        else
            foreach (var row in enabled) changed |= _selected.Add(row.Key);
        return changed;
    }

    // ----- expansion -----

    /// <summary>The keys of the expanded rows.</summary>
    public IReadOnlyCollection<string> ExpandedKeys => _expanded;

    /// <summary>Whether a row is expanded.</summary>
    public bool IsExpanded(string key) => _expanded.Contains(key);

    /// <summary>Expands or collapses a row. Returns <c>true</c> when the set changed.</summary>
    public bool SetExpanded(string key, bool expanded)
    {
        if (Find(key) is null) return false;
        return expanded ? _expanded.Add(key) : _expanded.Remove(key);
    }

    /// <summary>Toggles a row's expansion.</summary>
    public bool ToggleExpanded(string key) => SetExpanded(key, !_expanded.Contains(key));

    /// <summary>Replaces the expanded set (unknown keys are dropped). Returns <c>true</c> when it changed.</summary>
    public bool ReplaceExpanded(IEnumerable<string> keys)
    {
        var known = new HashSet<string>(_rows.Select(r => r.Key));
        var next = new HashSet<string>(keys.Where(known.Contains));
        if (next.SetEquals(_expanded)) return false;
        _expanded.Clear();
        _expanded.UnionWith(next);
        return true;
    }

    /// <summary>The batch bar text: "1 item selected" or "3 items selected".</summary>
    public static string BatchText(int count) => count == 1 ? "1 item selected" : $"{count} items selected";

    DataTableRowData? Find(string key)
    {
        foreach (var row in _rows)
            if (row.Key == key) return row;
        return null;
    }
}
