namespace Maui.Carbon;

/// <summary>Whether a tooltip names its trigger or describes it.</summary>
public enum CdsTooltipRole
{
    /// <summary>The text is the trigger's accessible name (icon-only controls).</summary>
    Label,
    /// <summary>The text is extra description of the trigger.</summary>
    Description,
}

/// <summary>
/// A short text bubble that names an icon-only control or describes a trigger. Opens after 100 ms of pointer hover or on
/// keyboard focus, closes 300 ms after the pointer leaves, on blur and on Escape. <see cref="CdsContainer.Body"/> is the trigger
/// (exactly one focusable control). Touch screens have no hover: the text still reaches screen readers through
/// <see cref="Role"/>; use a toggletip when people must be able to tap for it.
/// </summary>
public partial class CdsTooltip : CdsContainer
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTooltip), null,
        propertyChanged: static (b, _, _) => ((CdsTooltip)b).ApplySemantics());

    /// <summary>Bindable property for <see cref="Placement"/>.</summary>
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
        nameof(Placement), typeof(CdsPlacement), typeof(CdsTooltip), CdsPlacement.Top);

    /// <summary>Bindable property for <see cref="Role"/>.</summary>
    public static readonly BindableProperty RoleProperty = BindableProperty.Create(
        nameof(Role), typeof(CdsTooltipRole), typeof(CdsTooltip), CdsTooltipRole.Label,
        propertyChanged: static (b, _, _) => ((CdsTooltip)b).ApplySemantics());

    /// <summary>Bindable property for <see cref="EnterDelay"/>.</summary>
    public static readonly BindableProperty EnterDelayProperty = BindableProperty.Create(
        nameof(EnterDelay), typeof(int), typeof(CdsTooltip), 100);

    /// <summary>Bindable property for <see cref="LeaveDelay"/>.</summary>
    public static readonly BindableProperty LeaveDelayProperty = BindableProperty.Create(
        nameof(LeaveDelay), typeof(int), typeof(CdsTooltip), 300);

    /// <summary>Bindable property for <see cref="IsHighContrast"/>.</summary>
    public static readonly BindableProperty IsHighContrastProperty = BindableProperty.Create(
        nameof(IsHighContrast), typeof(bool), typeof(CdsTooltip), true);

    readonly ContentView _slot = new();
    readonly CdsTooltipTimer _timer = new();
    CdsOverlay? _overlay;

    /// <summary>Creates a tooltip.</summary>
    public CdsTooltip()
    {
        TrackHover();
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        SetChrome(_slot);
        _timer.Changed += (_, open) => { if (open) Show(); else Hide(); };
        StateChanged += (_, _) =>
        {
            if (Is(CdsInteractionState.Hover)) _timer.PointerEntered(EnterDelay);
            else _timer.PointerExited(LeaveDelay);
        };
        Unloaded += (_, _) => { _timer.Reset(); Hide(); };
    }

    /// <summary>The tooltip text: one or two words for an icon-only control, a sentence for a description.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Placement. Default <see cref="CdsPlacement.Top"/>.</summary>
    public CdsPlacement Placement
    {
        get => (CdsPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>Whether the text names (default) or describes the trigger.</summary>
    public CdsTooltipRole Role
    {
        get => (CdsTooltipRole)GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    /// <summary>Milliseconds of hover before opening. Default 100.</summary>
    public int EnterDelay
    {
        get => (int)GetValue(EnterDelayProperty);
        set => SetValue(EnterDelayProperty, value);
    }

    /// <summary>Milliseconds after the pointer leaves before closing. Default 300.</summary>
    public int LeaveDelay
    {
        get => (int)GetValue(LeaveDelayProperty);
        set => SetValue(LeaveDelayProperty, value);
    }

    /// <summary><c>background-inverse</c> surface (default) or <c>layer-01</c>.</summary>
    public bool IsHighContrast
    {
        get => (bool)GetValue(IsHighContrastProperty);
        set => SetValue(IsHighContrastProperty, value);
    }

    /// <summary>True while the tooltip is shown.</summary>
    public bool IsOpen => _overlay is not null;

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        if (oldBody is CdsControl oc) oc.StateChanged -= OnTriggerStateChanged;
        else if (oldBody is not null)
        {
            oldBody.Focused -= OnTriggerFocused;
            oldBody.Unfocused -= OnTriggerUnfocused;
        }
        _slot.Content = newBody;
        if (newBody is CdsControl nc) nc.StateChanged += OnTriggerStateChanged;
        else if (newBody is not null)
        {
            newBody.Focused += OnTriggerFocused;
            newBody.Unfocused += OnTriggerUnfocused;
        }
        ApplySemantics();
    }

    void OnTriggerStateChanged(object? sender, EventArgs e)
    {
        if (sender is CdsControl c) _timer.FocusChanged(c.Is(CdsInteractionState.FocusVisible));
    }

    void OnTriggerFocused(object? sender, FocusEventArgs e) => _timer.FocusChanged(true);

    void OnTriggerUnfocused(object? sender, FocusEventArgs e) => _timer.FocusChanged(false);

    void ApplySemantics()
    {
        var target = Body is CdsControl c ? c.AccessibilityTarget ?? Body : Body;
        if (target is null) return;
        if (Role == CdsTooltipRole.Label) SemanticProperties.SetDescription(target, Text);
        else SemanticProperties.SetHint(target, Text);
    }

    void Show()
    {
        if (_overlay is not null || string.IsNullOrEmpty(Text) || !IsLoaded) return;
        var text = new CdsText(Text, CdsTypeStyle.Body01, IsHighContrast ? CdsTextTone.Inverse : CdsTextTone.Primary)
        {
            Padding = new Thickness(CdsSpacing.Spacing05),
            LineBreakMode = LineBreakMode.WordWrap,
        };
        var surface = new CdsPopoverSurface(text, hasCaret: true, highContrast: IsHighContrast, dropShadow: false, maxWidth: 288);
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = Body ?? this,
            Placement = Placement,
            Offset = surface.OffsetFor(12),
            LightDismiss = false,
            InputTransparent = true,
            ReturnFocus = false,
        });
        surface.Track(_overlay);
        _overlay.Closed += (_, _) =>
        {
            _overlay = null;
            _timer.Reset();
        };
    }

    void Hide()
    {
        var overlay = _overlay;
        _overlay = null;
        overlay?.Close();
    }
}

/// <summary>Open/close timing for hover tooltips (pure; driven by pointer and focus events).</summary>
internal sealed class CdsTooltipTimer
{
    CancellationTokenSource? _pending;
    bool _hover;
    bool _focus;
    bool _open;

    /// <summary>Raised with the new open state.</summary>
    public event EventHandler<bool>? Changed;

    public bool IsOpen => _open;

    public void PointerEntered(int delayMs)
    {
        _hover = true;
        Schedule(true, delayMs);
    }

    public void PointerExited(int delayMs)
    {
        _hover = false;
        if (!_focus) Schedule(false, delayMs);
    }

    public void FocusChanged(bool focused)
    {
        _focus = focused;
        if (focused) Schedule(true, 0);
        else if (!_hover) Schedule(false, 0);
    }

    public void Reset()
    {
        _pending?.Cancel();
        _pending = null;
        _hover = _focus = _open = false;
    }

    void Schedule(bool open, int delayMs)
    {
        _pending?.Cancel();
        if (open == _open) return;
        if (delayMs <= 0)
        {
            Set(open);
            return;
        }
        var cts = _pending = new CancellationTokenSource();
        var dispatcher = Dispatcher.GetForCurrentThread();
        _ = Task.Delay(delayMs, cts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled || cts.IsCancellationRequested) return;
            if (dispatcher is not null) dispatcher.Dispatch(() => { if (!cts.IsCancellationRequested) Set(open); });
            else Set(open);
        }, TaskScheduler.Default);
    }

    void Set(bool open)
    {
        if (_open == open) return;
        _open = open;
        Changed?.Invoke(this, open);
    }
}
