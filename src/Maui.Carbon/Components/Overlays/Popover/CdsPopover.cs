using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>Maps the open state of a tab tip popover's trigger to Carbon tokens.</summary>
public static class CdsPopoverTokens
{
    /// <summary>
    /// How the trigger of a tab tip popover looks: while the popover is open it takes the <c>layer-01</c> fill of the
    /// content (with a shadow) so the two read as one tab, and a 2px strip of the same fill hides the shadow where they
    /// meet, except while the trigger has focus (Carbon: <c>.cds--popover--tab-tip__button:not(:focus)::after</c>).
    /// Otherwise the trigger is unstyled (<c>Fill</c> is <c>null</c>).
    /// </summary>
    public static (string? Fill, bool JoinVisible) TabTipTrigger(bool isTabTip, bool isOpen, bool triggerFocused) =>
        isTabTip && isOpen ? (CdsColorKeys.Layer01, !triggerFocused) : (null, false);
}

/// <summary>Pure popover rules (tested on their own).</summary>
static class CdsPopoverModel
{
    /// <summary>A tab tip aligns to the start or end of its trigger, below it: anything but BottomEnd becomes BottomStart.</summary>
    internal static CdsPlacement TabTipPlacement(CdsPlacement placement) =>
        placement == CdsPlacement.BottomEnd ? CdsPlacement.BottomEnd : CdsPlacement.BottomStart;
}

/// <summary>
/// A small surface of content anchored to a trigger (filters, settings). <see cref="CdsContainer.Body"/> is the trigger;
/// <see cref="PopoverContent"/> is shown in the page's overlay host while <see cref="IsOpen"/> is true. A tap outside or
/// Escape closes it. Give the trigger an expanded state and the content its own padding. Show one popover at a time.
/// With <see cref="IsTabTip"/> the popover is attached to a toolbar or header: no caret, square corners, no gap.
/// </summary>
public partial class CdsPopover : CdsContainer
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsPopover), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsPopover)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="PopoverContent"/>.</summary>
    public static readonly BindableProperty PopoverContentProperty = BindableProperty.Create(
        nameof(PopoverContent), typeof(View), typeof(CdsPopover), null);

    /// <summary>Bindable property for <see cref="Placement"/>.</summary>
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
        nameof(Placement), typeof(CdsPlacement), typeof(CdsPopover), CdsPlacement.Bottom);

    /// <summary>Bindable property for <see cref="HasCaret"/>.</summary>
    public static readonly BindableProperty HasCaretProperty = BindableProperty.Create(
        nameof(HasCaret), typeof(bool), typeof(CdsPopover), true);

    /// <summary>Bindable property for <see cref="HasDropShadow"/>.</summary>
    public static readonly BindableProperty HasDropShadowProperty = BindableProperty.Create(
        nameof(HasDropShadow), typeof(bool), typeof(CdsPopover), true);

    /// <summary>Bindable property for <see cref="IsHighContrast"/>.</summary>
    public static readonly BindableProperty IsHighContrastProperty = BindableProperty.Create(
        nameof(IsHighContrast), typeof(bool), typeof(CdsPopover), false);

    /// <summary>Bindable property for <see cref="HasBorder"/>.</summary>
    public static readonly BindableProperty HasBorderProperty = BindableProperty.Create(
        nameof(HasBorder), typeof(bool), typeof(CdsPopover), false);

    /// <summary>Bindable property for <see cref="IsTabTip"/>.</summary>
    public static readonly BindableProperty IsTabTipProperty = BindableProperty.Create(
        nameof(IsTabTip), typeof(bool), typeof(CdsPopover), false,
        propertyChanged: static (b, _, _) => ((CdsPopover)b).RefreshVisuals());

    readonly ContentView _slot = new();
    readonly Grid _face = new();
    BoxView? _join;
    CdsControl? _trigger;
    bool _faceShadow;
    CdsOverlay? _overlay;

    /// <summary>Creates a closed popover.</summary>
    public CdsPopover()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        _face.Children.Add(_slot);
        SetChrome(_face);
        Loaded += (_, _) => { if (IsOpen) Show(); };
        Unloaded += (_, _) => Hide();
    }

    /// <summary>Raised after the popover closes (light dismiss, Escape or <see cref="IsOpen"/> = false).</summary>
    public event EventHandler? Closed;

    /// <summary>Open state (two-way): light dismiss and Escape set it to false.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The floating content; give it its own padding.</summary>
    public View? PopoverContent
    {
        get => (View?)GetValue(PopoverContentProperty);
        set => SetValue(PopoverContentProperty, value);
    }

    /// <summary>
    /// Placement relative to the trigger. Default <see cref="CdsPlacement.Bottom"/>; flips when it does not fit. With
    /// <see cref="IsTabTip"/> only <see cref="CdsPlacement.BottomStart"/> and <see cref="CdsPlacement.BottomEnd"/> apply;
    /// anything else (including the default) is treated as <see cref="CdsPlacement.BottomStart"/>.
    /// </summary>
    public CdsPlacement Placement
    {
        get => (CdsPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Draw a 12x6 caret and keep the content 10 from the trigger; without it the content sits 4 from the trigger.
    /// Default <c>true</c>. Ignored (no caret) when <see cref="IsTabTip"/> is set.
    /// </summary>
    public bool HasCaret
    {
        get => (bool)GetValue(HasCaretProperty);
        set => SetValue(HasCaretProperty, value);
    }

    /// <summary>Carbon drop shadow in <c>shadow</c>. Default <c>true</c>.</summary>
    public bool HasDropShadow
    {
        get => (bool)GetValue(HasDropShadowProperty);
        set => SetValue(HasDropShadowProperty, value);
    }

    /// <summary><c>background-inverse</c> / <c>text-inverse</c> surface.</summary>
    public bool IsHighContrast
    {
        get => (bool)GetValue(IsHighContrastProperty);
        set => SetValue(IsHighContrastProperty, value);
    }

    /// <summary>1px <c>border-subtle-01</c> outline.</summary>
    public bool HasBorder
    {
        get => (bool)GetValue(HasBorderProperty);
        set => SetValue(HasBorderProperty, value);
    }

    /// <summary>
    /// Tab tip variant for a popover attached to a toolbar or header (Carbon <c>isTabTip</c>): no caret, square corners,
    /// no gap to the trigger, aligned start (default) or end, and flush with the window edge instead of 8 inside it.
    /// While open, the trigger is drawn as a tab: a <c>layer-01</c> fill and shadow that join the content. Use a
    /// transparent trigger (an icon-only ghost button). Takes effect the next time the popover opens. Default <c>false</c>.
    /// </summary>
    public bool IsTabTip
    {
        get => (bool)GetValue(IsTabTipProperty);
        set => SetValue(IsTabTipProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        _slot.Content = newBody;
        if (_trigger is not null) _trigger.StateChanged -= OnTriggerStateChanged;
        _trigger = newBody as CdsControl;
        if (_trigger is not null) _trigger.StateChanged += OnTriggerStateChanged;
        RefreshVisuals();
    }

    void OnTriggerStateChanged(object? sender, EventArgs e)
    {
        if (IsTabTip && IsOpen) RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var focused = _trigger is { } t && (t.Is(CdsInteractionState.FocusVisible) || t.Is(CdsInteractionState.Focused));
        var (fill, joinVisible) = CdsPopoverTokens.TabTipTrigger(IsTabTip, IsOpen, focused);
        SetColor(_face, BackgroundColorProperty, fill);
        if (fill is not null || _faceShadow)
        {
            CdsTokens.SetShadow(_face, fill is null ? null : CdsColorKeys.Shadow);
            _faceShadow = fill is not null;
        }
        if (_join is null && joinVisible)
        {
            _join = new BoxView { HeightRequest = 2, VerticalOptions = LayoutOptions.End, InputTransparent = true };
            AutomationProperties.SetExcludedWithChildren(_join, true);
            _face.Children.Add(_join);
        }
        if (_join is not null)
        {
            _join.IsVisible = joinVisible;
            SetColor(_join, BoxView.ColorProperty, joinVisible ? fill : null);
        }
    }

    void OnIsOpenChanged(bool open)
    {
        RefreshVisuals();
        if (open) Show();
        else Hide();
    }

    void Show()
    {
        if (_overlay is not null || PopoverContent is null || !IsLoaded) return;
        var (surface, options) = Compose(PopoverContent);
        _overlay = CdsOverlayService.Open(surface, options);
        surface.Track(_overlay);
        _overlay.Closed += OnOverlayClosed;
    }

    // The surface and overlay options for the current properties (a tab tip is square, caretless, flush and edge to edge).
    internal (CdsPopoverSurface Surface, CdsOverlayOptions Options) Compose(View content)
    {
        var tab = IsTabTip;
        var surface = new CdsPopoverSurface(content, HasCaret && !tab, IsHighContrast, HasDropShadow, HasBorder);
        if (tab) surface.Box.StrokeShape = new Rectangle();
        var options = new CdsOverlayOptions
        {
            Anchor = this,
            Placement = tab ? CdsPopoverModel.TabTipPlacement(Placement) : Placement,
            Offset = tab ? 0 : surface.OffsetFor(10),
            LightDismiss = true,
        };
        if (tab) options.EdgeMargin = 0;
        return (surface, options);
    }

    void Hide()
    {
        var overlay = _overlay;
        _overlay = null;
        overlay?.Close();
    }

    void OnOverlayClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnOverlayClosed;
        _overlay = null;
        IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
