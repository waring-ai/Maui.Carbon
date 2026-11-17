namespace Maui.Carbon;

/// <summary>Maps a toggletip button's state to Carbon tokens.</summary>
public static class CdsToggletipTokens
{
    /// <summary>The button icon: <c>icon-secondary</c>, <c>icon-primary</c> on hover and while open.</summary>
    public static string Icon(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.Hover) || state.HasFlag(CdsInteractionState.Open)
            ? CdsColorKeys.IconPrimary
            : CdsColorKeys.IconSecondary;
}

/// <summary>
/// Actions row of a toggletip: a link on the start and a small button on the end, spaced apart. Use at most one button.
/// </summary>
public class CdsToggletipActions : FlexLayout
{
    /// <summary>Creates the row.</summary>
    public CdsToggletipActions()
    {
        JustifyContent = Microsoft.Maui.Layouts.FlexJustify.SpaceBetween;
        AlignItems = Microsoft.Maui.Layouts.FlexAlignItems.Center;
    }
}

/// <summary>
/// An information bubble that opens when its small button is clicked and can hold interactive content (links, a button).
/// The button shows the 16px information icon (<see cref="Icon"/>) in <c>icon-secondary</c>, <c>icon-primary</c> on hover
/// and while open. The bubble is high contrast with a caret, 16 padding, 16 between rows and 288 at most;
/// <see cref="Text"/> is its paragraph and <see cref="ToggletipContent"/> follows it (usually a
/// <see cref="CdsToggletipActions"/>). Enter or Space toggles; Escape and a tap outside close it and focus returns to the
/// button. Content: one to three short sentences, never forms or anything required.
/// </summary>
[ContentProperty(nameof(ToggletipContent))]
public class CdsToggletip : CdsControl
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsToggletip), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsToggletip)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsToggletip), "Show information",
        propertyChanged: static (b, _, _) => ((CdsToggletip)b).ApplySemantics());

    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsToggletip), null);

    /// <summary>Bindable property for <see cref="ToggletipContent"/>.</summary>
    public static readonly BindableProperty ToggletipContentProperty = BindableProperty.Create(
        nameof(ToggletipContent), typeof(View), typeof(CdsToggletip), null);

    /// <summary>Bindable property for <see cref="Placement"/>.</summary>
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
        nameof(Placement), typeof(CdsPlacement), typeof(CdsToggletip), CdsPlacement.Bottom);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName), typeof(CdsToggletip), CdsIconName.Information,
        propertyChanged: static (b, _, n) => ((CdsToggletip)b)._icon.Icon = (CdsIconName)n);

    readonly Grid _root = new() { WidthRequest = CdsSizing.IconSize01, HeightRequest = CdsSizing.IconSize01 };
    readonly CdsIcon _icon = new(CdsIconName.Information, 16) { InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    CdsOverlay? _overlay;
    VerticalStackLayout? _content;

    /// <summary>Creates a closed toggletip.</summary>
    public CdsToggletip()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        _hit = CreateHitTarget(CdsAccessibilityRole.Button);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _root.Add(_icon);
        _root.Add(_ring);
        _root.Add(_hit);
        SetChrome(_root);
        ApplySemantics();
        Loaded += (_, _) => { if (IsOpen) Show(); };
        Unloaded += (_, _) => Hide(silent: true);
    }

    /// <summary>Raised after the toggletip closes (Escape, a tap outside, the button or <see cref="IsOpen"/> = false).</summary>
    public event EventHandler? Closed;

    /// <summary>Open state (two-way).</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Accessible name and tooltip of the button (<c>ToggletipButton label</c>). Default <c>Show information</c>; make it specific when several share a view.</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The bubble's paragraph (<c>body-01</c> in <c>text-inverse</c>).</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Extra bubble content under <see cref="Text"/>: usually a <see cref="CdsToggletipActions"/> row. Interactive content works.</summary>
    public View? ToggletipContent
    {
        get => (View?)GetValue(ToggletipContentProperty);
        set => SetValue(ToggletipContentProperty, value);
    }

    /// <summary>Placement relative to the button. Default <see cref="CdsPlacement.Bottom"/>; flips when it does not fit.</summary>
    public CdsPlacement Placement
    {
        get => (CdsPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>The button icon. Default <see cref="CdsIconName.Information"/>.</summary>
    public CdsIconName Icon
    {
        get => (CdsIconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnActivated() => IsOpen = !IsOpen;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        SetColor(_icon, CdsIcon.ColorProperty, CdsToggletipTokens.Icon(State));
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void ApplySemantics()
    {
        SemanticProperties.SetDescription(_hit, Label);
        ToolTipProperties.SetText(_hit, Label);
        _hit.IsExpanded = IsOpen;
    }

    void OnIsOpenChanged(bool open)
    {
        _hit.IsExpanded = open;
        SetState(CdsInteractionState.Open, open);
        if (open) Show();
        else Hide(silent: false);
    }

    void Show()
    {
        if (_overlay is not null || !IsLoaded) return;
        if (string.IsNullOrEmpty(Text) && ToggletipContent is null) return;

        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing05, Padding = new Thickness(CdsSpacing.Spacing05) };
        if (!string.IsNullOrEmpty(Text))
            stack.Add(new CdsText(Text, CdsTypeStyle.Body01, CdsTextTone.Inverse) { LineBreakMode = LineBreakMode.WordWrap });
        if (ToggletipContent is { } extra) stack.Add(extra);
        _content = stack;

        var surface = new CdsPopoverSurface(stack, hasCaret: true, highContrast: true, dropShadow: false, maxWidth: 288) { BindingContext = BindingContext };
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = this,
            Placement = Placement,
            Offset = surface.OffsetFor(13),
            LightDismiss = true,
            CloseOnEscape = true,
        });
        surface.Track(_overlay);
        _overlay.Closed += OnOverlayClosed;
        if (!string.IsNullOrEmpty(Text)) CdsAnnouncer.Announce(Text);
    }

    void Hide(bool silent)
    {
        var overlay = _overlay;
        if (overlay is null) return;
        _overlay = null;
        overlay.Closed -= OnOverlayClosed;
        overlay.Close();
        Release();
        if (!silent) Closed?.Invoke(this, EventArgs.Empty);
    }

    void OnOverlayClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnOverlayClosed;
        _overlay = null;
        Release();
        if (IsOpen) IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    void Release()
    {
        // Hand the consumer's content back so the toggletip can show it again.
        if (_content is { } stack && ToggletipContent is { } extra) stack.Remove(extra);
        _content = null;
    }
}
