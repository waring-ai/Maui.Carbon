using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// The product navigation of the header: <see cref="CdsHeaderMenuItem"/> links and <see cref="CdsHeaderMenu"/> dropdowns in a
/// row after the product name, behind a 1 x 24 <c>border-subtle-00</c> divider and 16 of padding. It is hidden below the lg
/// breakpoint (1056), where the same links go into a <see cref="CdsHeaderSideNavItems"/> in the side nav. A
/// navigation landmark: give it a <see cref="Label"/> that is unique when the page has several.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsHeaderNavigation : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsHeaderNavigation), "Navigation",
        propertyChanged: static (b, _, n) => SemanticProperties.SetDescription((CdsHeaderNavigation)b, (string?)n));

    readonly CdsItemCollection<View> _items = [];
    readonly HorizontalStackLayout _row = new() { Spacing = 0 };
    readonly Grid _root = new();
    readonly CdsBreakpointWatcher _watcher;

    /// <summary>Creates an empty navigation.</summary>
    public CdsHeaderNavigation()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Fill;
        var divider = new BoxView { WidthRequest = 1, HeightRequest = 24, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        CdsTokens.SetColor(divider, BoxView.ColorProperty, CdsUIShellTokens.Divider);
        _row.Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0);
        _root.Add(divider);
        _root.Add(_row);
        SetChrome(_root);
        SemanticProperties.SetDescription(this, Label);
        _items.CollectionChanged += OnItemsChanged;
        _watcher = new CdsBreakpointWatcher(this, Sync);
        Sync();
    }

    /// <summary>The accessible name of the navigation landmark.</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The links and menus, in order.</summary>
    public IList<View> Items => _items;

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) foreach (View v in e.OldItems) _row.Remove(v);
        if (e.NewItems is not null)
        {
            foreach (View v in e.NewItems)
            {
                switch (v)
                {
                    case CdsHeaderMenuItem item: item.Presentation = CdsShellPresentation.Bar; break;
                    case CdsHeaderMenu menu: menu.Presentation = CdsShellPresentation.Bar; break;
                }
                _row.Add(v);
            }
        }
    }

    void Sync()
    {
        if (_watcher is null) return;
        _root.IsVisible = UIShellModel.HeaderNavigationVisible(_watcher.Current);
    }
}

/// <summary>
/// The header links repeated inside the side nav below the lg breakpoint (1056) as 32 high rows (the first child of
/// <see cref="CdsSideNavItems"/>). Hidden from lg up. With <see cref="HasDivider"/> a 1px <c>border-subtle-01</c> rule sits 16
/// below the group. Children are <see cref="CdsHeaderMenuItem"/>s and <see cref="CdsHeaderMenu"/>s (which expand inline).
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsHeaderSideNavItems : CdsControl
{
    /// <summary>Bindable property for <see cref="HasDivider"/>.</summary>
    public static readonly BindableProperty HasDividerProperty = BindableProperty.Create(
        nameof(HasDivider), typeof(bool), typeof(CdsHeaderSideNavItems), false,
        propertyChanged: static (b, _, _) => ((CdsHeaderSideNavItems)b).Sync());

    readonly CdsItemCollection<View> _items = [];
    readonly VerticalStackLayout _column = new() { Spacing = 0 };
    readonly BoxView _divider = new() { HeightRequest = 1, Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0), IsVisible = false, InputTransparent = true };
    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly CdsBreakpointWatcher _watcher;

    /// <summary>Creates an empty group.</summary>
    public CdsHeaderSideNavItems()
    {
        HorizontalOptions = LayoutOptions.Fill;
        CdsTokens.SetColor(_divider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        _root.Add(_column);
        _root.Add(_divider);
        SetChrome(_root);
        _items.CollectionChanged += OnItemsChanged;
        _watcher = new CdsBreakpointWatcher(this, Sync);
        Sync();
    }

    /// <summary>Draws a 1px divider 16 below the group.</summary>
    public bool HasDivider
    {
        get => (bool)GetValue(HasDividerProperty);
        set => SetValue(HasDividerProperty, value);
    }

    /// <summary>The links and menus, in order.</summary>
    public IList<View> Items => _items;

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) foreach (View v in e.OldItems) _column.Remove(v);
        if (e.NewItems is not null)
        {
            foreach (View v in e.NewItems)
            {
                switch (v)
                {
                    case CdsHeaderMenuItem item: item.Presentation = CdsShellPresentation.SideNav; break;
                    case CdsHeaderMenu menu: menu.Presentation = CdsShellPresentation.SideNav; break;
                }
                _column.Add(v);
            }
        }
    }

    void Sync()
    {
        if (_watcher is null) return;
        var show = !UIShellModel.HeaderNavigationVisible(_watcher.Current);
        _root.IsVisible = show;
        _divider.IsVisible = HasDivider;
        _root.Margin = new Thickness(0, 0, 0, show ? CdsSpacing.Spacing07 : 0);
    }
}
