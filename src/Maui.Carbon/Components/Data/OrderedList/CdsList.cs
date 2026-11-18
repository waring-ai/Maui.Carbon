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

    void Rebuild()
    {
        _rebuild.Cancel();
        _stack.Children.Clear();
        var nested = IsNested;
        _stack.Margin = new Thickness(CdsListMarkers.LeftMargin(IsOrdered, nested), 0, 0, 0);
        var width = CdsListMarkers.MarkerWidth(IsOrdered, nested);
        var expressive = IsExpressive || _parentExpressive;
        for (var i = 0; i < _items.Count; i++)
        {
            _items[i].Configure(MarkerFor(i), width, expressive, _depth, IsOrdered);
            _stack.Add(_items[i]);
        }
    }
}

/// <summary>
/// A numbered list for steps or ranked items where order matters. Top-level items are numbered <c>1.</c>, <c>2.</c> with the
/// marker 24 before the text; nested lists use lower-case letters (<c>a.</c>, <c>b.</c>). Items wrap under the text.
/// </summary>
public class CdsOrderedList : CdsList
{
    /// <inheritdoc/>
    protected override bool IsOrdered => true;
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
