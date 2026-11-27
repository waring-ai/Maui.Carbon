using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// A list with a header and optional actions, for groups of similar items inside a panel, card or popover. Put
/// <see cref="CdsContainedListItem"/>s in <see cref="Items"/>. <see cref="Label"/> is the header text (and names the list)
/// and <see cref="Action"/> the header action slot (usually an icon-only ghost button). <see cref="Kind"/> is
/// <see cref="CdsContainedListKind.OnPage"/> for persistent areas or <see cref="CdsContainedListKind.Disclosed"/> for
/// popovers; <see cref="Size"/> (sm 32, md 40, lg 48, xl 64) sets the header and one-line item height; <see cref="IsInset"/>
/// insets the 1px dividers by the 16px padding. <see cref="Search"/> takes any view and pins it under the header. Lists
/// span the full width of their container.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsContainedList : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsContainedList), null,
        propertyChanged: static (b, _, _) => ((CdsContainedList)b).Rebuild());

    /// <summary>Bindable property for <see cref="Action"/>.</summary>
    public static readonly BindableProperty ActionProperty = BindableProperty.Create(
        nameof(Action), typeof(View), typeof(CdsContainedList), null,
        propertyChanged: static (b, _, _) => ((CdsContainedList)b).Rebuild());

    /// <summary>Bindable property for <see cref="Search"/>.</summary>
    public static readonly BindableProperty SearchProperty = BindableProperty.Create(
        nameof(Search), typeof(View), typeof(CdsContainedList), null,
        propertyChanged: static (b, _, _) => ((CdsContainedList)b).Rebuild());

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsContainedListKind), typeof(CdsContainedList), CdsContainedListKind.OnPage,
        propertyChanged: static (b, _, _) => ((CdsContainedList)b).Rebuild());

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsContainedList), CdsSize.Lg,
        propertyChanged: static (b, _, _) => ((CdsContainedList)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsInset"/>.</summary>
    public static readonly BindableProperty IsInsetProperty = BindableProperty.Create(
        nameof(IsInset), typeof(bool), typeof(CdsContainedList), false,
        propertyChanged: static (b, _, _) => ((CdsContainedList)b).Rebuild());

    readonly CdsItemCollection<CdsContainedListItem> _items = [];
    readonly CdsCoalescedAction _rebuild;
    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    readonly Grid _header = new();
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, VerticalOptions = LayoutOptions.Center };
    readonly BoxView _headerRule = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
    readonly ContentView _actionSlot = new() { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start };
    readonly ContentView _searchSlot = new();
    readonly BoxView _searchRule = new() { HeightRequest = 1, InputTransparent = true };
    readonly VerticalStackLayout _searchHost = new() { Spacing = 0 };
    readonly VerticalStackLayout _list = new() { Spacing = 0 };

    /// <summary>Creates an empty on-page list.</summary>
    public CdsContainedList()
    {
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        AutomationProperties.SetIsInAccessibleTree(_headerRule, false);
        AutomationProperties.SetIsInAccessibleTree(_searchRule, false);
        _items.CollectionChanged += OnItemsChanged;
        _searchSlot.Content = _searchHost;
        _header.Add(_label);
        _header.Add(_headerRule);
        _header.Add(_actionSlot);
        _stack.Add(_header);
        _stack.Add(_searchSlot);
        _stack.Add(_list);
        SetChrome(_stack);
        Rebuild();
    }

    /// <summary>The items, in order.</summary>
    public IList<CdsContainedListItem> Items => _items;

    /// <summary>The rows as laid out (for tests).</summary>
    internal IList<IView> ShownItems => _list.Children;

    /// <summary>The header text: one to three words in sentence case. It also names the list.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The header action, usually an icon-only ghost button; capped at the list height (32 when disclosed).</summary>
    public View? Action
    {
        get => (View?)GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    /// <summary>A search field (any view) pinned under the header, with a 1px rule below it.</summary>
    public View? Search
    {
        get => (View?)GetValue(SearchProperty);
        set => SetValue(SearchProperty, value);
    }

    /// <summary>On page (default) or disclosed.</summary>
    public CdsContainedListKind Kind
    {
        get => (CdsContainedListKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Height of the header and of one-line items: sm 32, md 40, lg 48 (default), xl 64.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Inset the dividers by the 16px padding (use when the list sits next to other divided components).</summary>
    public bool IsInset
    {
        get => (bool)GetValue(IsInsetProperty);
        set => SetValue(IsInsetProperty, value);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var kind = Kind;
        SetColor(_header, BackgroundColorProperty, CdsContainedListTokens.HeaderFill(kind));
        SetColor(_label, CdsText.TextColorProperty, CdsContainedListTokens.HeaderText(kind));
        SetColor(_headerRule, BoxView.ColorProperty, CdsContainedListTokens.HeaderRule(kind));
        SetColor(_searchRule, BoxView.ColorProperty, CdsContainedListTokens.Divider);
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => _rebuild.Request();

    // Brings the shown rows in line with Items without re-adding the ones already in place: removing or re-adding a row
    // costs MAUI its native view and layout, and every item would rebuild its chrome. Configure is a no-op for an item
    // whose size, inset and last-row state did not change.
    void SyncItems()
    {
        var wanted = new HashSet<CdsContainedListItem>(_items);
        for (var i = _list.Children.Count - 1; i >= 0; i--)
            if (_list.Children[i] is not CdsContainedListItem shown || !wanted.Contains(shown)) _list.Children.RemoveAt(i);
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            item.Configure(Size, IsInset, i == _items.Count - 1);
            if (i < _list.Children.Count && ReferenceEquals(_list.Children[i], item)) continue;
            var at = _list.Children.IndexOf(item);
            if (at >= 0) _list.Children.RemoveAt(at);
            _list.Children.Insert(i, item);
        }
    }

    void Rebuild()
    {
        _rebuild.Cancel();
        var kind = Kind;
        var disclosed = kind == CdsContainedListKind.Disclosed;
        var headerHeight = CdsContainedListMetrics.HeaderHeight(kind, Size);

        _label.TypeStyle = disclosed ? CdsTypeStyle.Label01 : CdsTypeStyle.HeadingCompact01;
        _label.Text = Label;
        _label.Margin = new Thickness(CdsContainedListMetrics.SidePadding, 0, CdsContainedListMetrics.ActionReserve, 0);
        _header.HeightRequest = headerHeight;
        _header.IsVisible = !string.IsNullOrEmpty(Label) || Action is not null;
        _actionSlot.Content = null;
        if (Action is not null)
        {
            _actionSlot.Content = Action;
            _actionSlot.MaximumHeightRequest = CdsContainedListMetrics.HeaderActionHeight(kind, Size);
        }

        _searchHost.Children.Clear();
        if (Search is not null)
        {
            _searchHost.Add(Search);
            _searchHost.Add(_searchRule);
        }
        _searchSlot.IsVisible = Search is not null;

        SyncItems();

        CdsSemantics.SetGroup(_list, Label, CdsSemanticGroupKind.List);
        RefreshVisuals();
    }
}
