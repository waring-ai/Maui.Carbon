using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// A destination in the side nav: a 32 high row (48 with <see cref="CdsSideNavRow.IsLarge"/>) with an optional 16px icon, label in
/// <c>text-secondary</c> (semibold, as in Carbon's side nav). The current page (<see cref="CdsShellItem.IsActive"/>) fills with
/// <c>background-selected</c> with a 3px <c>border-interactive</c> start bar. Navigate in <see cref="CdsShellItem.Clicked"/> (swap the
/// shell's <see cref="CdsContainer.Body"/> or call your navigation) and move <c>IsActive</c> yourself.
/// </summary>
public class CdsSideNavLink : CdsSideNavRow
{
    /// <summary>Creates an empty link.</summary>
    public CdsSideNavLink() : base(CdsAccessibilityRole.Link) => UpdateRow();

    /// <summary>Creates a link.</summary>
    public CdsSideNavLink(string text, CdsIconName? icon = null, bool isActive = false) : this()
    {
        Text = text;
        Icon = icon;
        IsActive = isActive;
    }
}

/// <summary>
/// A child of a <see cref="CdsSideNavMenu"/>: a 32 high row indented to 32 (72 when the group has an icon), regular weight,
/// semibold when current. Do not nest menus inside menus: put deeper levels in page tabs.
/// </summary>
public class CdsSideNavMenuItem : CdsSideNavRow
{
    /// <summary>Creates an empty item.</summary>
    public CdsSideNavMenuItem() : base(CdsAccessibilityRole.Link) => UpdateRow();

    /// <summary>Creates an item.</summary>
    public CdsSideNavMenuItem(string text, bool isActive = false) : this()
    {
        Text = text;
        IsActive = isActive;
    }

    /// <inheritdoc/>
    protected override CdsTypeStyle LabelStyle => IsActive ? CdsTypeStyle.HeadingCompact01 : CdsTypeStyle.BodyCompact01;

    /// <inheritdoc/>
    protected override void OnActiveChanged()
    {
        UpdateRow();
        base.OnActiveChanged();
    }
}

/// <summary>
/// A collapsible group of <see cref="CdsSideNavMenuItem"/>s: a 32 high (48 large) row with an optional icon, a semibold title and a
/// 16px chevron that turns 180 degrees when open. Collapsed, a group that holds the current page shows the selected treatment;
/// open, the title goes <c>text-primary</c>. Do not nest groups.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsSideNavMenu : CdsSideNavRow
{
    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsSideNavMenu), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsSideNavMenu)b).OnExpandedChanged((bool)n));

    readonly CdsItemCollection<CdsSideNavMenuItem> _items = [];
    readonly VerticalStackLayout _children = new() { Spacing = 0, IsVisible = false };
    readonly CdsIcon _chevron = new(CdsIconName.ChevronDown, 16) { VerticalOptions = LayoutOptions.Center, InputTransparent = true };

    /// <summary>Creates an empty group.</summary>
    public CdsSideNavMenu() : base(CdsAccessibilityRole.Expander)
    {
        AutomationProperties.SetExcludedWithChildren(_chevron, true);
        SetTrailing(_chevron);
        _items.CollectionChanged += OnItemsChanged;
        UpdateRow();
    }

    /// <inheritdoc/>
    protected override View WrapChrome(View row) => new VerticalStackLayout { Spacing = 0, Children = { row, _children } };

    /// <summary>The group's items.</summary>
    public IList<CdsSideNavMenuItem> Items => _items;

    /// <summary>Open state (two-way). A group that holds the current page is usually opened by its owner.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>True when a child item is current.</summary>
    public bool HasActiveChild => _items.Any(i => i.IsActive);

    /// <inheritdoc/>
    protected override bool ShowsSelection => (IsActive || HasActiveChild) && !IsExpanded;

    /// <inheritdoc/>
    protected override CdsShellVisuals Resolve()
    {
        var v = CdsUIShellTokens.SideNavRow(State, ShowsSelection);
        if ((IsActive || HasActiveChild) && IsExpanded && !State.HasFlag(CdsInteractionState.Disabled))
            v = v with { Foreground = CdsColorKeys.TextPrimary, Icon = CdsColorKeys.IconPrimary };
        return v;
    }

    /// <inheritdoc/>
    protected override void OnItemActivated() => IsExpanded = !IsExpanded;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        base.ApplyVisuals();
        var v = Resolve();
        SetColor(_chevron, CdsIcon.ColorProperty, v.Icon);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IconProperty.PropertyName && _items is not null) UpdateIndent();
    }

    void OnExpandedChanged(bool open)
    {
        HitTarget.IsExpanded = open;
        SetState(CdsInteractionState.Open, open);
        _children.IsVisible = open;
        _ = CdsMotion.AnimateAsync(_chevron, "CdsSideNavChevron", v => _chevron.Rotation = v, _chevron.Rotation, open ? 180 : 0, CdsDuration.Fast02, CdsEasing.StandardProductive);
        RefreshVisuals();
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsSideNavMenuItem item in e.OldItems)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
                _children.Remove(item);
            }
        if (e.NewItems is not null)
            foreach (CdsSideNavMenuItem item in e.NewItems)
            {
                item.PropertyChanged += OnItemPropertyChanged;
                _children.Add(item);
            }
        UpdateIndent();
        RefreshVisuals();
    }

    void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsActive)) RefreshVisuals();
    }

    void UpdateIndent()
    {
        var indent = Icon is not null ? 72 : 32;
        foreach (var item in _items) item.Indent = indent;
    }
}

/// <summary>The stack of rows inside a <see cref="CdsSideNav"/>: starts 16 from the top and holds links, menus, dividers and <see cref="CdsHeaderSideNavItems"/>.</summary>
public class CdsSideNavItems : VerticalStackLayout
{
    /// <summary>Creates an empty list.</summary>
    public CdsSideNavItems()
    {
        Spacing = 0;
        Padding = new Thickness(0, CdsSpacing.Spacing05, 0, 0);
    }
}

/// <summary>A 1px <c>border-subtle-00</c> rule with 8 above and below and 16 from each side, between groups of side nav rows.</summary>
public class CdsSideNavDivider : ContentView
{
    /// <summary>Creates a divider.</summary>
    public CdsSideNavDivider()
    {
        Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03);
        HorizontalOptions = LayoutOptions.Fill;
        InputTransparent = true;
        var line = new BoxView { HeightRequest = 1, InputTransparent = true };
        CdsTokens.SetColor(line, BoxView.ColorProperty, CdsUIShellTokens.Divider);
        Content = line;
        AutomationProperties.SetIsInAccessibleTree(this, false);
    }
}

/// <summary>
/// A 48 high row at the top of the side nav: a 16px icon and a semibold label over a <c>border-subtle-00</c> rule (the product or
/// section the side nav belongs to). Carbon v11.99 ships no styles for it; it follows the v10 shell on v11 tokens.
/// </summary>
public class CdsSideNavHeader : CdsControl
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsSideNavHeader), null,
        propertyChanged: static (b, _, n) => ((CdsSideNavHeader)b)._label.Text = (string?)n);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsSideNavHeader), null,
        propertyChanged: static (b, _, n) =>
        {
            var header = (CdsSideNavHeader)b;
            header._icon.Icon = (CdsIconName?)n;
            header._icon.IsVisible = n is not null;
        });

    readonly CdsIcon _icon = new() { IsVisible = false, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, CdsSpacing.Spacing06, 0) };
    readonly CdsText _label = new("", CdsTypeStyle.HeadingCompact01) { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center };

    /// <summary>Creates an empty header.</summary>
    public CdsSideNavHeader()
    {
        HorizontalOptions = LayoutOptions.Fill;
        var rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsUIShellTokens.Divider);
        var row = new HorizontalStackLayout { Padding = new Thickness(CdsSpacing.Spacing05, 0), HeightRequest = UIShellModel.HeaderHeight };
        row.Add(_icon);
        row.Add(_label);
        var root = new Grid();
        root.Add(row);
        root.Add(rule);
        SemanticProperties.SetHeadingLevel(_label, SemanticHeadingLevel.Level2);
        SetChrome(root);
    }

    /// <summary>The label.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The 16px icon before the label.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}

/// <summary>
/// A 48 high row at the bottom of the side nav (over a <c>border-subtle-00</c> rule) with a full-width toggle that expands and collapses the
/// side nav it sits in: a 20px chevron while collapsed, a close icon while expanded (also while a rail is expanded by hover or
/// focus), <c>background-hover</c> on hover and a 2px inset focus ring. <see cref="Label"/> is its accessible name and tooltip.
/// The toggle hides itself where the side nav cannot collapse from it: a fixed side nav, and a persistent one from lg up.
/// </summary>
public class CdsSideNavFooter : CdsShellItem
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsSideNavFooter), null,
        propertyChanged: static (b, _, _) => ((CdsSideNavFooter)b).Update());

    readonly Grid _root = new() { HeightRequest = UIShellModel.HeaderHeight };
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsIcon _icon = new(CdsIconName.ChevronRight, CdsSizing.IconSize02) { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(CdsSpacing.Spacing05 - 2, 0, 0, 0), InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    bool _expanded;

    /// <summary>Creates a footer.</summary>
    public CdsSideNavFooter()
    {
        HorizontalOptions = LayoutOptions.Fill;
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        var rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsUIShellTokens.Divider);
        var bg = new BoxView { InputTransparent = true };
        CdsTokens.SetColor(bg, BoxView.ColorProperty, CdsUIShellTokens.Surface);
        _hit = CreateHitTarget(CdsAccessibilityRole.Button);
        _root.Add(bg);
        _root.Add(_fill);
        _root.Add(rule);
        _root.Add(_icon);
        _root.Add(_ring);
        _root.Add(_hit);
        SetChrome(_root);
        Update();
    }

    /// <summary>The accessible name and tooltip. Default "Expand side navigation" / "Collapse side navigation".</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>True while the footer shows the side nav as expanded (close icon, "Collapse side navigation").</summary>
    internal bool ShowsExpanded => _expanded;

    /// <summary>
    /// Mirrors the side nav's open state and whether the toggle can change it (set by <see cref="CdsSideNav"/>); the toggle hides
    /// itself when it cannot.
    /// </summary>
    internal void SetSideNavState(bool expanded, bool toggles)
    {
        _root.IsVisible = toggles;
        if (_expanded == expanded) return;
        _expanded = expanded;
        Update();
    }

    /// <inheritdoc/>
    protected override void OnItemActivated() => CdsShellContext.FindSideNav(this)?.Toggle();

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsUIShellTokens.SideNavToggle(State);
        SetColor(_fill, BoxView.ColorProperty, v.Background ?? CdsColorKeys.Layer01);
        _fill.IsVisible = v.Background is not null;
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void Update()
    {
        _icon.Icon = _expanded ? CdsIconName.Close : CdsIconName.ChevronRight;
        var name = Label ?? (_expanded ? "Collapse side navigation" : "Expand side navigation");
        SemanticProperties.SetDescription(_hit, name);
        ToolTipProperties.SetText(_hit, name);
        _hit.IsExpanded = _expanded;
        RefreshVisuals();
    }
}
