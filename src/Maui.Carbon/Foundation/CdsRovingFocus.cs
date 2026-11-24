using System.ComponentModel;

namespace Maui.Carbon;

/// <summary>Which arrow keys move focus in a <see cref="CdsRovingFocus"/>.</summary>
internal enum CdsRovingOrientation
{
    /// <summary>Left and Right (tabs, content switcher, a horizontal radio group).</summary>
    Horizontal,
    /// <summary>Up and Down (tree view, structured list, menus, a vertical radio group).</summary>
    Vertical,
    /// <summary>All four arrows (a radio group or tile group whose layout can wrap).</summary>
    Both,
}

/// <summary>
/// A roving tab stop for a composite widget (the web's <c>tabindex="0"</c> / <c>"-1"</c> pattern): of an ordered set of
/// <see cref="CdsHitTarget"/>s exactly one has <see cref="CdsHitTarget.IsTabStop"/> on, so Tab enters and leaves the widget
/// in one step, and the arrow keys move focus inside it.
/// <para>
/// The tab stop is, in order: the item focused or moved to last (after focus leaves the widget only while
/// <see cref="RemembersFocus"/> is on); the item that has focus; the <see cref="PreferredIndex"/> (the selected item); else
/// the first item that <see cref="CanFocus"/> (enabled and visible by default). Disabled and hidden items are skipped by the arrows and never
/// take the stop, and the stop follows when items are added, removed, enabled, disabled, shown or hidden.
/// </para>
/// <para>Adopting it takes a few lines in a <see cref="CdsControl"/>:</para>
/// <code>
/// readonly CdsRovingFocus _roving = new(CdsRovingOrientation.Horizontal) { Wraps = true, RemembersFocus = false };
///
/// // in the constructor:
/// _roving.PreferredIndex = () => SelectedIndex;      // the selected item takes the stop
/// _roving.Navigated += (_, index) => Select(index);  // only when selection follows focus (radios, tabs)
///
/// // after building or rebuilding the item views (and when the selection changes):
/// _roving.SetItems(_views.Select(v => v.Hit));
/// _roving.Refresh();                                  // after SelectedIndex changed
///
/// protected internal override bool WantsNavigationKeys => true;
/// protected internal override bool OnKeyDown(CdsKeyEventArgs e) => _roving.HandleKey(e);
/// </code>
/// <para>
/// Items are tracked through their <c>Focused</c>, <c>Unfocused</c> and <c>PropertyChanged</c> events; the helper holds
/// no references beyond the items themselves, so it lives and dies with its control.
/// </para>
/// </summary>
internal sealed class CdsRovingFocus
{
    /// <summary><see cref="Navigate"/>: the key is not one this helper handles.</summary>
    internal const int NotHandled = -2;

    readonly List<CdsHitTarget> _items = [];
    CdsHitTarget? _current;
    Func<int>? _preferredIndex;
    Func<int, bool>? _canFocus;

    /// <summary>Creates a roving tab stop.</summary>
    /// <param name="orientation">Which arrow keys move focus.</param>
    /// <param name="wraps">Whether moving past the last item goes to the first (and back).</param>
    public CdsRovingFocus(CdsRovingOrientation orientation = CdsRovingOrientation.Vertical, bool wraps = false)
    {
        Orientation = orientation;
        Wraps = wraps;
    }

    /// <summary>Raised after an arrow, Home or End key moved focus to the item at the given index.</summary>
    public event EventHandler<int>? Navigated;

    /// <summary>Which arrow keys move focus.</summary>
    public CdsRovingOrientation Orientation { get; set; }

    /// <summary>Whether moving past the last item goes to the first (and before the first to the last). Default off.</summary>
    public bool Wraps { get; set; }

    /// <summary>Whether Home and End move to the first and last item. Default on.</summary>
    public bool HandlesHomeEnd { get; set; } = true;

    /// <summary>
    /// Whether the item focused last keeps the tab stop after focus leaves the widget (tree view: on, the default). Turn it
    /// off when the stop must return to the selected item (<see cref="PreferredIndex"/>) once focus leaves: radio
    /// groups, tabs.
    /// </summary>
    public bool RemembersFocus { get; set; } = true;

    /// <summary>
    /// The index that takes the tab stop when no item has or had focus (usually the selected item); -1 or an index that
    /// cannot take focus falls back to the first focusable item. Call <see cref="Refresh"/> when its result changes.
    /// </summary>
    public Func<int>? PreferredIndex
    {
        get => _preferredIndex;
        set
        {
            _preferredIndex = value;
            Refresh();
        }
    }

    /// <summary>
    /// Whether the item at an index can take focus. Default: the hit target is enabled and visible. Call
    /// <see cref="Refresh"/> when its result changes for a reason the helper cannot see.
    /// </summary>
    public Func<int, bool> CanFocus
    {
        get => _canFocus ?? DefaultCanFocus;
        set
        {
            _canFocus = value;
            Refresh();
        }
    }

    /// <summary>The items, in navigation order.</summary>
    public IReadOnlyList<CdsHitTarget> Items => _items;

    /// <summary>The index of the item that has the tab stop, or -1 when no item can take focus.</summary>
    public int TabStopIndex { get; private set; } = -1;

    /// <summary>Replaces the items (call after rebuilding the item views).</summary>
    public void SetItems(IEnumerable<CdsHitTarget> items)
    {
        foreach (var item in _items) Detach(item);
        _items.Clear();
        foreach (var item in items)
        {
            _items.Add(item);
            Attach(item);
        }
        if (_current is not null && !_items.Contains(_current)) _current = null;
        Refresh();
    }

    /// <summary>Adds an item at the end.</summary>
    public void Add(CdsHitTarget item) => Insert(_items.Count, item);

    /// <summary>Inserts an item at <paramref name="index"/>.</summary>
    public void Insert(int index, CdsHitTarget item)
    {
        _items.Insert(index, item);
        Attach(item);
        Refresh();
    }

    /// <summary>Removes an item (its tab stop is turned back on, the default of a lone hit target).</summary>
    public bool Remove(CdsHitTarget item)
    {
        if (!_items.Remove(item)) return false;
        Detach(item);
        item.IsTabStop = true;
        if (ReferenceEquals(_current, item)) _current = null;
        Refresh();
        return true;
    }

    /// <summary>Removes every item.</summary>
    public void Clear() => SetItems([]);

    /// <summary>Forgets the item focused last, so the stop goes back to <see cref="PreferredIndex"/> (unless an item has focus).</summary>
    public void ForgetFocus()
    {
        _current = null;
        Refresh();
    }

    /// <summary>Recomputes the tab stop and updates every item's <see cref="CdsHitTarget.IsTabStop"/>.</summary>
    public void Refresh()
    {
        var canFocus = CanFocus;
        var focused = _items.FindIndex(i => i.IsFocused);
        var current = _current is null ? -1 : _items.IndexOf(_current);
        var stop = TabStop(_items.Count, canFocus, current, focused, _preferredIndex?.Invoke() ?? -1);
        TabStopIndex = stop;
        for (var i = 0; i < _items.Count; i++) _items[i].IsTabStop = i == stop;
    }

    /// <summary>The index of the item with focus, else the tab stop.</summary>
    public int CurrentIndex()
    {
        var focused = _items.FindIndex(i => i.IsFocused);
        return focused >= 0 ? focused : TabStopIndex;
    }

    /// <summary>Gives the item at <paramref name="index"/> the tab stop and focus; false when it cannot take focus.</summary>
    public bool FocusIndex(int index)
    {
        if (index < 0 || index >= _items.Count || !CanFocus(index)) return false;
        _current = _items[index];
        Refresh();
        _items[index].Focus();
        return true;
    }

    /// <summary>
    /// Handles an arrow (for the <see cref="Orientation"/>), Home or End key: moves focus and raises
    /// <see cref="Navigated"/>. Returns (and sets <see cref="CdsKeyEventArgs.Handled"/>) true for those keys even at the
    /// end of a non-wrapping set, so the key does not scroll the page.
    /// </summary>
    public bool HandleKey(CdsKeyEventArgs e)
    {
        if (!HandleKey(e.Key)) return false;
        e.Handled = true;
        return true;
    }

    /// <inheritdoc cref="HandleKey(CdsKeyEventArgs)"/>
    public bool HandleKey(CdsKey key)
    {
        var current = CurrentIndex();
        var target = Navigate(_items.Count, CanFocus, current, key, Orientation, Wraps, HandlesHomeEnd);
        if (target == NotHandled) return false;
        if (target >= 0 && target != current && FocusIndex(target)) Navigated?.Invoke(this, target);
        return true;
    }

    /// <summary>
    /// Where <paramref name="key"/> moves focus from <paramref name="current"/> (-1 for none): the target index, -1 when
    /// the key is handled but there is nowhere to go, or <see cref="NotHandled"/>. Pure (unit-tested).
    /// </summary>
    internal static int Navigate(int count, Func<int, bool> canFocus, int current, CdsKey key, CdsRovingOrientation orientation, bool wraps, bool homeEnd)
    {
        var horizontal = orientation != CdsRovingOrientation.Vertical;
        var vertical = orientation != CdsRovingOrientation.Horizontal;
        int step;
        switch (key)
        {
            case CdsKey.Left when horizontal:
            case CdsKey.Up when vertical:
                step = -1;
                break;
            case CdsKey.Right when horizontal:
            case CdsKey.Down when vertical:
                step = 1;
                break;
            case CdsKey.Home when homeEnd:
                return Next(count, canFocus, 0, 1);
            case CdsKey.End when homeEnd:
                return Next(count, canFocus, count - 1, -1);
            default:
                return NotHandled;
        }
        if (current < 0 || current >= count) return step > 0 ? Next(count, canFocus, 0, 1) : Next(count, canFocus, count - 1, -1);
        var next = Next(count, canFocus, current + step, step);
        if (next < 0 && wraps) next = step > 0 ? Next(count, canFocus, 0, 1) : Next(count, canFocus, count - 1, -1);
        return next;
    }

    /// <summary>
    /// The tab stop: the item focused or moved to last, else the focused one, else the preferred one, else the first
    /// focusable one (each only when it can take focus); -1 when none can. The item moved to comes first so that focus can
    /// land on it while the item it leaves still has focus. Pure (unit-tested).
    /// </summary>
    internal static int TabStop(int count, Func<int, bool> canFocus, int current, int focused, int preferred)
    {
        foreach (var candidate in new[] { current, focused, preferred })
            if (candidate >= 0 && candidate < count && canFocus(candidate)) return candidate;
        return Next(count, canFocus, 0, 1);
    }

    static int Next(int count, Func<int, bool> canFocus, int from, int step)
    {
        for (var i = from; i >= 0 && i < count; i += step)
            if (canFocus(i)) return i;
        return -1;
    }

    bool DefaultCanFocus(int index) => index >= 0 && index < _items.Count && _items[index] is { IsEnabled: true, IsVisible: true };

    void Attach(CdsHitTarget item)
    {
        item.Focused += OnItemFocused;
        item.Unfocused += OnItemUnfocused;
        item.PropertyChanged += OnItemPropertyChanged;
    }

    void Detach(CdsHitTarget item)
    {
        item.Focused -= OnItemFocused;
        item.Unfocused -= OnItemUnfocused;
        item.PropertyChanged -= OnItemPropertyChanged;
    }

    void OnItemFocused(object? sender, FocusEventArgs e)
    {
        _current = sender as CdsHitTarget;
        Refresh();
    }

    void OnItemUnfocused(object? sender, FocusEventArgs e)
    {
        // Focus moved on inside the widget: the newly focused item already took over. Focus left: the stop goes back to
        // the preferred item unless the last one is remembered.
        if (!RemembersFocus && ReferenceEquals(_current, sender)) _current = null;
        Refresh();
    }

    void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName || e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
            Refresh();
    }
}
