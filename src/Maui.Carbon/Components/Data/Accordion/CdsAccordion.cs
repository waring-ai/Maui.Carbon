using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// A vertical list of headings that each expand to reveal a panel, for hiding secondary detail until the reader asks
/// (FAQs, settings groups). Put <see cref="CdsAccordionItem"/>s in <see cref="Items"/>. The chevron sits at the end
/// (<see cref="CdsAccordionAlign.End"/>, default) or the start of each heading; use one alignment per page.
/// <see cref="Size"/> sets the heading height (sm 32, md 40, lg 48); <see cref="IsFlush"/> insets the dividers 16 on both
/// sides so text and rules line up with neighbouring components. <c>IsEnabled="False"</c> disables every item (an item's
/// own <c>IsEnabled</c> wins). Dividers are 1px <c>border-subtle-01</c> and hover fills a heading with <c>layer-hover-01</c>.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsAccordion : ContentView
{
    /// <summary>Bindable property for <see cref="Align"/>.</summary>
    public static readonly BindableProperty AlignProperty = BindableProperty.Create(
        nameof(Align), typeof(CdsAccordionAlign), typeof(CdsAccordion), CdsAccordionAlign.End,
        propertyChanged: static (b, _, _) => ((CdsAccordion)b).Configure());

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsAccordion), CdsSize.Md,
        propertyChanged: static (b, _, _) => ((CdsAccordion)b).Configure());

    /// <summary>Bindable property for <see cref="IsFlush"/>.</summary>
    public static readonly BindableProperty IsFlushProperty = BindableProperty.Create(
        nameof(IsFlush), typeof(bool), typeof(CdsAccordion), false,
        propertyChanged: static (b, _, _) => ((CdsAccordion)b).Configure());

    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    readonly CdsItemCollection<CdsAccordionItem> _items = [];

    /// <summary>Creates an accordion.</summary>
    public CdsAccordion()
    {
        Content = _stack;
        _items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>The items, in order.</summary>
    public IList<CdsAccordionItem> Items => _items;

    /// <summary>Chevron at the end (default) or start of the headings.</summary>
    public CdsAccordionAlign Align
    {
        get => (CdsAccordionAlign)GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>Heading height: <see cref="CdsSize.Sm"/> 32, <see cref="CdsSize.Md"/> 40 (default) or <see cref="CdsSize.Lg"/> 48.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Inset the dividers 16 from both edges so they line up with neighbouring content. Use in narrow spaces.</summary>
    public bool IsFlush
    {
        get => (bool)GetValue(IsFlushProperty);
        set => SetValue(IsFlushProperty, value);
    }

    internal void OnItemToggled(CdsAccordionItem item) => UpdateNeighbours();

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName) Configure();
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsAccordionItem old in e.OldItems)
            {
                old.HeadingHoverChanged -= OnHeadingHoverChanged;
                old.Attach(null);
            }
        if (e.NewItems is not null)
            foreach (CdsAccordionItem item in e.NewItems)
            {
                item.HeadingHoverChanged += OnHeadingHoverChanged;
                item.Attach(this);
            }
        _stack.Clear();
        foreach (var item in _items) _stack.Add(item);
        Configure();
    }

    void OnHeadingHoverChanged(object? sender, EventArgs e) => UpdateNeighbours();

    void Configure()
    {
        for (var i = 0; i < _items.Count; i++)
            _items[i].Configure(Size, Align, IsFlush, !IsEnabled, i == _items.Count - 1);
        UpdateNeighbours();
    }

    void UpdateNeighbours()
    {
        for (var i = 1; i < _items.Count; i++)
        {
            var above = _items[i - 1];
            _items[i].SetNeighbourHover(above.IsHeadingHovered && !above.IsOpen && !above.IsItemDisabled);
        }
        if (_items.Count > 0) _items[0].SetNeighbourHover(false);
    }
}
