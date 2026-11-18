using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// One row of a <see cref="CdsSwitcher"/>: a 32 high link in heading-compact-01 (14 semibold) <c>text-secondary</c> with 6 / 16
/// padding. Hover <c>layer-hover-01</c> + <c>text-primary</c>, pressed <c>layer-active-01</c>, selected (the current product,
/// <see cref="CdsShellItem.IsActive"/>) <c>layer-selected-01</c> + <c>text-primary</c>; focus is a 2px inset <c>focus</c> ring. Labels are
/// one line and truncate. Items are in the tab order only while the switcher is expanded.
/// </summary>
public class CdsSwitcherItem : CdsShellItem
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsSwitcherItem), null,
        propertyChanged: static (b, _, _) => ((CdsSwitcherItem)b).Update());

    readonly Grid _root = new() { HeightRequest = UIShellModel.SideNavRowHeight };
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsText _label = new("", CdsTypeStyle.HeadingCompact01) { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(CdsSpacing.Spacing05, 0), InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;

    /// <summary>Creates an empty item.</summary>
    public CdsSwitcherItem()
    {
        HorizontalOptions = LayoutOptions.Fill;
        AutomationProperties.SetExcludedWithChildren(_label, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Link);
        _root.Add(_fill);
        _root.Add(_label);
        _root.Add(_ring);
        _root.Add(_hit);
        SetChrome(_root);
        Update();
    }

    /// <summary>Creates an item.</summary>
    public CdsSwitcherItem(string text, bool isSelected = false) : this()
    {
        Text = text;
        IsActive = isSelected;
    }

    /// <summary>The product name. One line.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Whether Tab can reach the item (the switcher turns it off while collapsed).</summary>
    internal void SetTabStop(bool value) => _hit.IsTabStop = value;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsUIShellTokens.SwitcherItem(State, IsActive);
        SetColor(_fill, BoxView.ColorProperty, v.Background ?? CdsColorKeys.Layer01);
        _fill.IsVisible = v.Background is not null;
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void Update()
    {
        _label.Text = Text;
        SemanticProperties.SetDescription(_hit, Text);
        _hit.IsSelected = IsActive;
        ToolTipProperties.SetText(_hit, Text ?? string.Empty);
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnActiveChanged()
    {
        _hit.IsSelected = IsActive;
        base.OnActiveChanged();
    }
}

/// <summary>A 1px <c>border-subtle-01</c> rule 224 wide with 8 above and below and 16 from each side. Group items with it (current product, related products, all products), not between every item.</summary>
public class CdsSwitcherDivider : ContentView
{
    /// <summary>Creates a divider.</summary>
    public CdsSwitcherDivider()
    {
        Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03);
        HorizontalOptions = LayoutOptions.Fill;
        InputTransparent = true;
        var line = new BoxView { HeightRequest = 1, InputTransparent = true };
        CdsTokens.SetColor(line, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        Content = line;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }
}

/// <summary>
/// The list inside a <see cref="CdsHeaderPanel"/> (or any 256 wide panel): <see cref="CdsSwitcherItem"/>s and
/// <see cref="CdsSwitcherDivider"/>s starting 16 from the top. ArrowUp and ArrowDown move focus between items and wrap, skipping
/// dividers. Items are focusable only while <see cref="IsExpanded"/>; <see cref="CdsHeaderPanel"/> keeps that in sync for a
/// switcher it holds. Give it a <see cref="Label"/> ("Switch product").
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsSwitcher : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsSwitcher), null,
        propertyChanged: static (b, _, n) => SemanticProperties.SetDescription((CdsSwitcher)b, (string?)n));

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsSwitcher), true,
        propertyChanged: static (b, _, _) => ((CdsSwitcher)b).SyncTabStops());

    readonly ObservableCollection<View> _items = [];
    readonly VerticalStackLayout _stack = new() { Spacing = 0, Padding = new Thickness(0, CdsSpacing.Spacing05, 0, 0) };

    /// <summary>Creates an empty switcher.</summary>
    public CdsSwitcher()
    {
        HorizontalOptions = LayoutOptions.Fill;
        SetChrome(_stack);
        _items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>The accessible name of the list.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Whether the switcher is shown: items are tabbable only while expanded. Default <c>true</c> (standalone use).</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>Items and dividers, in order.</summary>
    public IList<View> Items => _items;

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key is not (CdsKey.Up or CdsKey.Down or CdsKey.Home or CdsKey.End)) return false;
        var rows = _items.OfType<CdsSwitcherItem>().ToList();
        var current = rows.FindIndex(r => r.State.HasFlag(CdsInteractionState.Focused));
        var next = UIShellModel.Move(rows.Select(r => r.IsEnabled).ToList(), current, e.Key);
        if (next < 0) return false;
        rows[next].Focus();
        return true;
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) foreach (View v in e.OldItems) _stack.Remove(v);
        if (e.NewItems is not null) foreach (View v in e.NewItems) _stack.Add(v);
        SyncTabStops();
    }

    void SyncTabStops()
    {
        foreach (var item in _items.OfType<CdsSwitcherItem>()) item.SetTabStop(IsExpanded);
    }
}
