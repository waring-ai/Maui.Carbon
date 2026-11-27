using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// Base of <see cref="CdsOrderedList"/> and <see cref="CdsUnorderedList"/>: a stack of <see cref="CdsListItem"/>s in the productive
/// (<c>body-01</c>) or expressive (<c>body-02</c>) type size. Use lists for short, related items; use a data table for data people
/// sort or filter and a structured list for label and value pairs. Set an item's <see cref="CdsListItem.NestedList"/> to nest
/// (nested lists indent 32, items get 4 of start padding; go no deeper than two levels).
/// </summary>
[ContentProperty(nameof(Items))]
public abstract class CdsList : ContentView
{
    /// <summary>Bindable property for <see cref="IsExpressive"/>.</summary>
    public static readonly BindableProperty IsExpressiveProperty = BindableProperty.Create(
        nameof(IsExpressive), typeof(bool), typeof(CdsList), false,
        propertyChanged: static (b, _, _) => ((CdsList)b).Rebuild());

    readonly CdsItemCollection<CdsListItem> _items = [];
    readonly CdsCoalescedAction _rebuild;
    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    int _depth;
    bool _parentExpressive;

    /// <summary>Initializes the list.</summary>
    protected CdsList()
    {
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        Content = _stack;
        _items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>The items, in order.</summary>
    public IList<CdsListItem> Items => _items;

    /// <summary>Uses <c>body-02</c> (16/24) instead of <c>body-01</c> (14/20). Nested lists follow their parent. Default <c>false</c>.</summary>
    public bool IsExpressive
    {
        get => (bool)GetValue(IsExpressiveProperty);
        set => SetValue(IsExpressiveProperty, value);
    }

    /// <summary>True when the list is the <see cref="CdsListItem.NestedList"/> of an item.</summary>
    public bool IsNested => _depth > 0;

    /// <summary>True for numbered lists.</summary>
    protected abstract bool IsOrdered { get; }

    /// <summary>True when the top-level markers are the platform-style ones of <see cref="CdsOrderedList.IsNative"/>.</summary>
    internal virtual bool IsNativeMarkers => false;

    /// <summary>The list's marker for the item at <paramref name="index"/>.</summary>
    internal string MarkerFor(int index) => CdsListMarkers.Marker(IsOrdered, IsNested, index);

    internal void Attach(int depth, bool parentExpressive)
    {
        if (_depth == depth && _parentExpressive == parentExpressive) return;
        _depth = depth;
        _parentExpressive = parentExpressive;
        Rebuild();
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => _rebuild.Request();

    /// <summary>Lays the items out again, once after the current batch of changes.</summary>
    internal void RequestRebuild() => _rebuild.Request();

    void Rebuild()
    {
        _rebuild.Cancel();
        var nested = IsNested;
        _stack.Margin = new Thickness(CdsListMarkers.LeftMargin(IsOrdered, nested), 0, 0, 0);
        var width = CdsListMarkers.MarkerWidth(IsOrdered, nested);
        var expressive = IsExpressive || _parentExpressive;
        var markerAtEnd = CdsListMarkers.IsMarkerEndAligned(IsOrdered, nested, IsNativeMarkers);
        for (var i = 0; i < _items.Count; i++)
        {
            _items[i].Configure(MarkerFor(i), width, expressive, _depth, IsOrdered, markerAtEnd);
        }
        SyncItems();
    }

    // Keeps the items already in place instead of clearing and re-adding every one (each re-add costs MAUI the item's
    // native view and a layout pass).
    void SyncItems()
    {
        var wanted = new HashSet<CdsListItem>(_items);
        for (var i = _stack.Children.Count - 1; i >= 0; i--)
            if (_stack.Children[i] is not CdsListItem shown || !wanted.Contains(shown)) _stack.Children.RemoveAt(i);
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            if (i < _stack.Children.Count && ReferenceEquals(_stack.Children[i], item)) continue;
            var at = _stack.Children.IndexOf(item);
            if (at >= 0) _stack.Children.RemoveAt(at);
            _stack.Children.Insert(i, item);
        }
    }
}

/// <summary>
/// A numbered list for steps or ranked items where order matters. Top-level items are numbered <c>1.</c>, <c>2.</c> with the
/// marker placed 24 before the text, outside the list's box as in Carbon: the text lines up with the content around the
/// list, so leave 24 of space before it (or pad the container). Nested lists use lower-case letters (<c>a.</c>, <c>b.</c>).
/// Items wrap under the text.
/// </summary>
public class CdsOrderedList : CdsList
{
    /// <summary>Bindable property for <see cref="IsNative"/>.</summary>
    public static readonly BindableProperty IsNativeProperty = BindableProperty.Create(
        nameof(IsNative), typeof(bool), typeof(CdsOrderedList), false,
        propertyChanged: static (b, _, _) => ((CdsOrderedList)b).RequestRebuild());

    /// <inheritdoc/>
    protected override bool IsOrdered => true;

    /// <summary>
    /// Uses native-style <c>decimal</c> markers (Carbon's <c>native</c>): the numbers end just before the text, so they line up
    /// on the dot past 9, instead of Carbon's counters that start 24 before the text. Default <c>false</c>.
    /// </summary>
    public bool IsNative
    {
        get => (bool)GetValue(IsNativeProperty);
        set => SetValue(IsNativeProperty, value);
    }

    internal override bool IsNativeMarkers => IsNative;
}

/// <summary>
/// A bulleted list for a short set of related items of equal weight. Top-level items use an en dash 16 before the text,
/// nested lists a small square. Items wrap under the text.
/// </summary>
public class CdsUnorderedList : CdsList
{
    /// <inheritdoc/>
    protected override bool IsOrdered => false;
}
