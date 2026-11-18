using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// Shows where the current page sits in a hierarchy as a row of links separated by <c>/</c>. Add
/// <see cref="CdsBreadcrumbItem"/>s from the highest level down. By default leave the current page out (the trail ends with its
/// parent and the trailing slash leads into the page title); include it with <see cref="CdsBreadcrumbItem.IsCurrentPage"/> only
/// when the page has no visible title, and then set <see cref="NoTrailingSlash"/>. Long trails keep the first level and the last
/// two and collapse the middle levels into a "…" crumb that opens a list (use <see cref="MaxVisibleItems"/>, or give an item
/// <see cref="CdsBreadcrumbItem.OverflowItems"/>). A breadcrumb never wraps. Use it for hierarchies more than two levels deep,
/// not for primary navigation or the steps of a task.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsBreadcrumb : CdsControl
{
    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsBreadcrumbSize), typeof(CdsBreadcrumb), CdsBreadcrumbSize.Md,
        propertyChanged: static (b, _, _) => ((CdsBreadcrumb)b).Rebuild());

    /// <summary>Bindable property for <see cref="NoTrailingSlash"/>.</summary>
    public static readonly BindableProperty NoTrailingSlashProperty = BindableProperty.Create(
        nameof(NoTrailingSlash), typeof(bool), typeof(CdsBreadcrumb), false,
        propertyChanged: static (b, _, _) => ((CdsBreadcrumb)b).Rebuild());

    /// <summary>Bindable property for <see cref="MaxVisibleItems"/>.</summary>
    public static readonly BindableProperty MaxVisibleItemsProperty = BindableProperty.Create(
        nameof(MaxVisibleItems), typeof(int), typeof(CdsBreadcrumb), 0,
        coerceValue: static (_, v) => Math.Max(0, (int)v),
        propertyChanged: static (b, _, _) => ((CdsBreadcrumb)b).Rebuild());

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsBreadcrumb), "Breadcrumb",
        propertyChanged: static (b, _, _) => ((CdsBreadcrumb)b).Rebuild());

    readonly CdsItemCollection<CdsBreadcrumbItem> _items = [];
    readonly CdsCoalescedAction _rebuild;
    readonly HorizontalStackLayout _row = new() { Spacing = 0 };

    /// <summary>Creates an empty breadcrumb.</summary>
    public CdsBreadcrumb()
    {
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(_row);
        _items.CollectionChanged += OnItemsChanged;
        Rebuild();
    }

    /// <summary>The levels, from the highest down.</summary>
    public IList<CdsBreadcrumbItem> Items => _items;

    /// <summary><see cref="CdsBreadcrumbSize.Md"/> (default, 14) or <see cref="CdsBreadcrumbSize.Sm"/> (12, for page headers and dense areas).</summary>
    public CdsBreadcrumbSize Size
    {
        get => (CdsBreadcrumbSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Omits the slash after the last crumb. Use it when no page title follows. Default <c>false</c>.</summary>
    public bool NoTrailingSlash
    {
        get => (bool)GetValue(NoTrailingSlashProperty);
        set => SetValue(NoTrailingSlashProperty, value);
    }

    /// <summary>
    /// The most crumbs to show, counting the "…" crumb; longer trails keep the first level and the last <c>MaxVisibleItems - 2</c> and
    /// collapse the rest into the overflow list. 0 (default) never collapses.
    /// </summary>
    public int MaxVisibleItems
    {
        get => (int)GetValue(MaxVisibleItemsProperty);
        set => SetValue(MaxVisibleItemsProperty, value);
    }

    /// <summary>Names the trail for screen readers (not drawn). Default "Breadcrumb".</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => _rebuild.Request();

    void Rebuild()
    {
        _rebuild.Cancel();
        _row.Children.Clear();
        var (start, count) = BreadcrumbModel.Collapse(_items.Count, MaxVisibleItems);
        var shown = new List<CdsBreadcrumbItem>();
        for (var i = 0; i < _items.Count; i++)
        {
            if (count > 0 && i == start)
            {
                var overflow = new CdsBreadcrumbItem();
                foreach (var hidden in _items.Skip(start).Take(count)) overflow.OverflowItems.Add(hidden);
                shown.Add(overflow);
            }
            if (count > 0 && i >= start && i < start + count) continue;
            shown.Add(_items[i]);
        }
        for (var i = 0; i < shown.Count; i++)
        {
            var last = i == shown.Count - 1;
            shown[i].Configure(Size, !(last && NoTrailingSlash));
            _row.Add(shown[i]);
        }
        SemanticProperties.SetDescription(_row, Label);
    }
}
