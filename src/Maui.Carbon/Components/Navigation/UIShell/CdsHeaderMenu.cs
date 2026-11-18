using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A header link that opens a dropdown menu of <see cref="CdsHeaderMenuItem"/>s instead of navigating (its label only opens
/// the menu, never a page). The trigger shows <see cref="Label"/> and a 16px chevron (<c>icon-secondary</c>, 8 after the text)
/// that turns 180 degrees while open; open, it fills with <c>layer-01</c> and a 200 wide <c>layer-01</c> list with a
/// <c>0 4px 8px</c> <c>shadow</c> drops below it: 48 high rows, hover <c>layer-hover-01</c>, current <c>layer-selected-01</c> with a
/// 3px start bar. While closed the trigger shows the current underline when a child is current.
/// <para>
/// Keyboard (the menu-button pattern, virtual focus on the trigger): Enter, Space or a click toggle; ArrowDown opens and
/// highlights the first item; ArrowUp, ArrowDown, Home and End move the highlight (wrapping), Enter or Space choose; Escape or
/// a tap outside closes and returns focus to the trigger.
/// </para>
/// In <see cref="CdsHeaderSideNavItems"/> the menu expands inline instead, with its rows indented 68.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsHeaderMenu : CdsShellItem
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsHeaderMenu), null,
        propertyChanged: static (b, _, _) => ((CdsHeaderMenu)b).Update());

    /// <summary>Bindable property for <see cref="MenuLabel"/>.</summary>
    public static readonly BindableProperty MenuLabelProperty = BindableProperty.Create(
        nameof(MenuLabel), typeof(string), typeof(CdsHeaderMenu), null,
        propertyChanged: static (b, _, _) => ((CdsHeaderMenu)b).Update());

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsHeaderMenu), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsHeaderMenu)b).OnExpandedChanged((bool)n));

    readonly ObservableCollection<CdsHeaderMenuItem> _items = [];
    readonly Grid _root = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, InputTransparent = true };
    readonly CdsIcon _chevron = new(CdsIconName.ChevronDown, 16) { InputTransparent = true };
    readonly BoxView _bar = new() { HeightRequest = 3, VerticalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly VerticalStackLayout _inline = new() { Spacing = 0, IsVisible = false };
    readonly CdsHitTarget _hit;
    readonly Grid _triggerRow = new();
    CdsShellPresentation _presentation = CdsShellPresentation.Bar;
    CdsOverlay? _overlay;
    VerticalStackLayout? _dropdown;
    int _highlight = -1;
    bool _openPending;

    /// <summary>Creates an empty menu.</summary>
    public CdsHeaderMenu()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Fill;
        _label.VerticalOptions = LayoutOptions.Center;
        _chevron.VerticalOptions = LayoutOptions.Center;
        AutomationProperties.SetExcludedWithChildren(_label, true);
        AutomationProperties.SetExcludedWithChildren(_chevron, true);

        var row = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing03, Padding = new Thickness(CdsSpacing.Spacing05, 0), InputTransparent = true };
        row.Add(_label);
        row.Add(_chevron);
        _triggerRow.Add(_fill);
        _triggerRow.Add(row);
        _triggerRow.Add(_bar);
        _triggerRow.Add(_ring);
        _hit = CreateHitTarget(CdsAccessibilityRole.Button);
        _triggerRow.Add(_hit);

        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_triggerRow);
        stack.Add(_inline);
        _root.Add(stack);
        SetChrome(_root);

        _items.CollectionChanged += OnItemsChanged;
        Loaded += (_, _) =>
        {
            if (_openPending) Dispatcher.Dispatch(() => OnExpandedChanged(true));
        };
        Unloaded += (_, _) => CloseDropdown();
        Update();
    }

    /// <summary>Raised after the menu opened.</summary>
    public event EventHandler? Opened;

    /// <summary>Raised after the menu closed.</summary>
    public event EventHandler? Closed;

    /// <summary>The trigger text ("Manage"). It only opens the menu.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The accessible name of the menu (<c>aria-label</c>). Defaults to <see cref="Label"/>.</summary>
    public string? MenuLabel
    {
        get => (string?)GetValue(MenuLabelProperty);
        set => SetValue(MenuLabelProperty, value);
    }

    /// <summary>Open state (two-way). A menu set open before it is loaded opens when it loads.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>The menu items, in order.</summary>
    public IList<CdsHeaderMenuItem> Items => _items;

    internal CdsShellPresentation Presentation
    {
        get => _presentation;
        set
        {
            if (_presentation == value) return;
            CloseDropdown();
            _presentation = value;
            foreach (var item in _items) PlaceInline(item);
            Update();
        }
    }

    /// <summary>True when a child item is current.</summary>
    public bool HasCurrentChild => _items.Any(i => i.IsActive);

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (_presentation != CdsShellPresentation.Bar) return false;
        if (e.Key == CdsKey.Down && !IsExpanded)
        {
            Open(keyboard: true);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    protected override void OnItemActivated()
    {
        IsExpanded = !IsExpanded;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var open = IsExpanded;
        var sideNav = _presentation == CdsShellPresentation.SideNav;
        var current = HasCurrentChild || IsActive;
        CdsShellVisuals v;
        if (sideNav)
        {
            v = CdsUIShellTokens.SideNavRow(State, false);
        }
        else
        {
            v = CdsUIShellTokens.HeaderMenuTrigger(State, open, current && !open);
        }
        SetColor(_fill, BoxView.ColorProperty, v.Background ?? CdsColorKeys.Layer01);
        _fill.IsVisible = v.Background is not null;
        SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty, v.Foreground);
        SetColor(_chevron, CdsIcon.ColorProperty, CdsColorKeys.IconSecondary);
        SetColor(_bar, BoxView.ColorProperty, v.Bar ?? CdsColorKeys.BorderInteractive);
        _bar.IsVisible = !sideNav && v.Bar is not null;
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void Update()
    {
        _label.Text = Label;
        var sideNav = _presentation == CdsShellPresentation.SideNav;
        _label.TypeStyle = sideNav ? CdsTypeStyle.HeadingCompact01 : CdsTypeStyle.BodyCompact01;
        _triggerRow.HeightRequest = sideNav ? UIShellModel.SideNavRowHeight : UIShellModel.HeaderHeight;
        HorizontalOptions = sideNav ? LayoutOptions.Fill : LayoutOptions.Start;
        _inline.IsVisible = sideNav && IsExpanded;
        SemanticProperties.SetDescription(_hit, Label);
        SemanticProperties.SetHint(_hit, MenuLabel ?? Label);
        _hit.IsExpanded = IsExpanded;
        RefreshVisuals();
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsHeaderMenuItem item in e.OldItems)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
                _inline.Remove(item);
            }
        if (e.NewItems is not null)
            foreach (CdsHeaderMenuItem item in e.NewItems)
            {
                item.PropertyChanged += OnItemPropertyChanged;
                PlaceInline(item);
            }
        RefreshVisuals();
    }

    void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsActive)) RefreshVisuals();
    }

    void PlaceInline(CdsHeaderMenuItem item)
    {
        if (_presentation == CdsShellPresentation.SideNav)
        {
            item.Presentation = CdsShellPresentation.SideNav;
            item.InSideNavMenu = true;
            if (item.Parent is null) _inline.Add(item);
        }
        else if (ReferenceEquals(item.Parent, _inline))
        {
            _inline.Remove(item);
        }
    }

    void OnExpandedChanged(bool open)
    {
        _hit.IsExpanded = open;
        SetState(CdsInteractionState.Open, open);
        if (_presentation == CdsShellPresentation.SideNav)
        {
            Update();
            AnimateChevron();
            if (open) Opened?.Invoke(this, EventArgs.Empty);
            else Closed?.Invoke(this, EventArgs.Empty);
            return;
        }
        if (open)
        {
            if (!IsLoaded)
            {
                _openPending = true;
                return;
            }
            Open(keyboard: false);
        }
        else
        {
            _openPending = false;
            CloseDropdown();
        }
        RefreshVisuals();
    }

    void AnimateChevron()
    {
        var target = IsExpanded ? 180d : 0d;
        _ = CdsMotion.AnimateAsync(_chevron, "CdsHeaderMenuChevron", v => _chevron.Rotation = v, _chevron.Rotation, target, CdsDuration.Fast02, CdsEasing.StandardProductive);
    }

    void Open(bool keyboard)
    {
        if (_overlay is not null) return;
        if (!IsLoaded)
        {
            _openPending = true;
            return;
        }
        _openPending = false;
        var surface = BuildSurface();
        _highlight = -1;
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = this,
            Placement = CdsPlacement.BottomStart,
            LightDismiss = true,
            CloseOnEscape = true,
            Keys = CdsOverlayKeys.Navigation | CdsOverlayKeys.Activation,
            KeyHandler = OnOverlayKey,
        });
        _overlay.Closed += OnOverlayClosed;
        if (!IsExpanded) SetValue(IsExpandedProperty, true);
        AnimateChevron();
        if (keyboard) Highlight(UIShellModel.FirstEnabled(EnabledFlags()));
        RefreshVisuals();
        Opened?.Invoke(this, EventArgs.Empty);
    }

    Border BuildSurface()
    {
        var stack = new VerticalStackLayout { Spacing = 0 };
        foreach (var item in _items)
        {
            item.Presentation = CdsShellPresentation.Dropdown;
            item.SetTabStop(false);
            item.IsHighlighted = false;
            item.ShowHighlightRing = true;
            item.Activated += OnItemChosen;
            stack.Add(item);
        }
        _dropdown = stack;
        var surface = new Border
        {
            StrokeShape = new Rectangle(),
            StrokeThickness = 0,
            WidthRequest = UIShellModel.HeaderMenuWidth,
            Content = stack,
        };
        CdsTokens.SetColor(surface, BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetShadow(surface, CdsColorKeys.Shadow, offsetY: 4, radius: 8);
        SemanticProperties.SetDescription(surface, MenuLabel ?? Label);
        return surface;
    }

    /// <summary>
    /// Builds the open dropdown as an ordinary view without showing an overlay (for documentation and previews). The items move
    /// into it, so use it on a menu that is not part of a live header.
    /// </summary>
    /// <param name="highlightedIndex">Index of the row to show highlighted by the keyboard, or -1.</param>
    public View CreateDropdownPreview(int highlightedIndex = -1)
    {
        var surface = BuildSurface();
        surface.HorizontalOptions = LayoutOptions.Start;
        surface.VerticalOptions = LayoutOptions.Start;
        if (highlightedIndex >= 0) Highlight(highlightedIndex);
        return surface;
    }

    IReadOnlyList<bool> EnabledFlags() => _items.Select(i => i.IsEnabled).ToList();

    void Highlight(int index)
    {
        _highlight = index;
        for (var i = 0; i < _items.Count; i++) _items[i].IsHighlighted = i == index;
    }

    bool OnOverlayKey(CdsKeyEventArgs e)
    {
        switch (e.Key)
        {
            case CdsKey.Up:
            case CdsKey.Down:
            case CdsKey.Home:
            case CdsKey.End:
                var next = UIShellModel.Move(EnabledFlags(), _highlight, e.Key);
                if (next >= 0) Highlight(next);
                return true;
            case CdsKey.Enter:
            case CdsKey.Space:
                if (_highlight >= 0 && _highlight < _items.Count) _items[_highlight].Activate();
                return true;
            case CdsKey.Left:
            case CdsKey.Right:
                return true;
            default:
                return false;
        }
    }

    void OnItemChosen(object? sender, EventArgs e) => _overlay?.Close();

    void OnOverlayClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnOverlayClosed;
        _overlay = null;
        ReleaseItems();
        if (IsExpanded) SetValue(IsExpandedProperty, false);
        SetState(CdsInteractionState.Open, false);
        AnimateChevron();
        RefreshVisuals();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    void CloseDropdown()
    {
        var overlay = _overlay;
        if (overlay is null) return;
        overlay.Close();
    }

    void ReleaseItems()
    {
        if (_dropdown is { } stack)
        {
            foreach (var item in _items)
            {
                item.Activated -= OnItemChosen;
                item.IsHighlighted = false;
                item.ShowHighlightRing = false;
                stack.Remove(item);
                item.Presentation = CdsShellPresentation.Bar;
                item.SetTabStop(true);
            }
        }
        _dropdown = null;
        _highlight = -1;
    }
}
