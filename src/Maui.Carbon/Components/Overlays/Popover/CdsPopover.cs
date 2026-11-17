namespace Maui.Carbon;

/// <summary>
/// A small surface of content anchored to a trigger (filters, settings). <see cref="CdsContainer.Body"/> is the trigger;
/// <see cref="PopoverContent"/> is shown in the page's overlay host while <see cref="IsOpen"/> is true. A tap outside or
/// Escape closes it. Give the trigger an expanded state and the content its own padding. Show one popover at a time.
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

    readonly ContentView _slot = new();
    CdsOverlay? _overlay;

    /// <summary>Creates a closed popover.</summary>
    public CdsPopover()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        SetChrome(_slot);
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

    /// <summary>Placement relative to the trigger. Default <see cref="CdsPlacement.Bottom"/>; flips when it does not fit.</summary>
    public CdsPlacement Placement
    {
        get => (CdsPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>Draw a 12x6 caret and keep the content 10 from the trigger. Default <c>true</c>.</summary>
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

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => _slot.Content = newBody;

    void OnIsOpenChanged(bool open)
    {
        if (open) Show();
        else Hide();
    }

    void Show()
    {
        if (_overlay is not null || PopoverContent is null || !IsLoaded) return;
        var surface = new CdsPopoverSurface(PopoverContent, HasCaret, IsHighContrast, HasDropShadow, HasBorder);
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = this,
            Placement = Placement,
            Offset = surface.OffsetFor(10),
            LightDismiss = true,
        });
        surface.Track(_overlay);
        _overlay.Closed += OnOverlayClosed;
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
