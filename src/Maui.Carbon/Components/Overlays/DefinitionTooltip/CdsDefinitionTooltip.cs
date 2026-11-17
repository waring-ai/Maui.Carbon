namespace Maui.Carbon;

/// <summary>Maps a definition term's state to Carbon tokens.</summary>
public static class CdsDefinitionTooltipTokens
{
    /// <summary>The dotted underline: <c>border-strong-01</c>, <c>border-interactive</c> on hover and focus.</summary>
    public static string Underline(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.Hover) || state.HasFlag(CdsInteractionState.FocusVisible) || state.HasFlag(CdsInteractionState.Focused)
            ? CdsColorKeys.BorderInteractive
            : CdsColorKeys.BorderStrong01;

    /// <summary>The term text: always <c>text-primary</c> (disabled text is not part of this component).</summary>
    public static string Text => CdsColorKeys.TextPrimary;
}

/// <summary>
/// A term in running text with a dotted underline that shows a short definition in a high-contrast tooltip when focused,
/// tapped or clicked (a second tap closes it) and, with <see cref="OpenOnHover"/>, when hovered. Escape, blur and a tap
/// outside close it. The term is one focusable button inheriting <see cref="TypeStyle"/>; lay it out beside the
/// surrounding words (a wrapping flex or stack row), since MAUI has no inline flow. The definition is one short sentence
/// in sentence case that does not repeat the term. Use a toggletip for content with links or buttons.
/// </summary>
public class CdsDefinitionTooltip : CdsControl
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsDefinitionTooltip), null,
        propertyChanged: static (b, _, n) =>
        {
            var t = (CdsDefinitionTooltip)b;
            t._term.Text = (string?)n;
            t.ApplySemantics();
        });

    /// <summary>Bindable property for <see cref="Definition"/>.</summary>
    public static readonly BindableProperty DefinitionProperty = BindableProperty.Create(
        nameof(Definition), typeof(string), typeof(CdsDefinitionTooltip), null,
        propertyChanged: static (b, _, _) => ((CdsDefinitionTooltip)b).ApplySemantics());

    /// <summary>Bindable property for <see cref="OpenOnHover"/>.</summary>
    public static readonly BindableProperty OpenOnHoverProperty = BindableProperty.Create(
        nameof(OpenOnHover), typeof(bool), typeof(CdsDefinitionTooltip), false);

    /// <summary>Bindable property for <see cref="Placement"/>.</summary>
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
        nameof(Placement), typeof(CdsPlacement), typeof(CdsDefinitionTooltip), CdsPlacement.Bottom);

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsDefinitionTooltip), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsDefinitionTooltip)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="TypeStyle"/>.</summary>
    public static readonly BindableProperty TypeStyleProperty = BindableProperty.Create(
        nameof(TypeStyle), typeof(CdsTypeStyle), typeof(CdsDefinitionTooltip), CdsTypeStyle.Body01,
        propertyChanged: static (b, _, n) => ((CdsDefinitionTooltip)b)._term.TypeStyle = (CdsTypeStyle)n);

    readonly Grid _root = new();
    readonly Grid _text = new() { VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly CdsText _term = new() { TypeStyle = CdsTypeStyle.Body01, LineBreakMode = LineBreakMode.NoWrap, InputTransparent = true };
    readonly Microsoft.Maui.Controls.Shapes.Line _line = new()
    {
        X1 = 0, Y1 = 0, X2 = 1, Y2 = 0,
        Aspect = Stretch.Fill,
        StrokeThickness = 1,
        StrokeDashArray = [1, 1],
        HeightRequest = 1,
        VerticalOptions = LayoutOptions.End,
        InputTransparent = true,
    };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    readonly CdsTooltipTimer _timer = new();
    CdsOverlay? _overlay;
    bool _clicked;

    /// <summary>Creates a term.</summary>
    public CdsDefinitionTooltip()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        _hit = CreateHitTarget(CdsAccessibilityRole.Button);
        AutomationProperties.SetExcludedWithChildren(_term, true);
        AutomationProperties.SetIsInAccessibleTree(_line, false);
        _text.Add(_term);
        _text.Add(_line);
        _root.Add(_text);
        _root.Add(_ring);
        _root.Add(_hit);
        SetChrome(_root);
        _timer.Changed += (_, _) => UpdateOpen();
        Loaded += (_, _) => { if (IsOpen) Show(); };
        Unloaded += (_, _) => { _timer.Reset(); Hide(); };
        ApplySemantics();
    }

    /// <summary>The term (<c>children</c>).</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The definition shown in the tooltip: one short sentence in sentence case, plain text (<c>definition</c>).</summary>
    public string? Definition
    {
        get => (string?)GetValue(DefinitionProperty);
        set => SetValue(DefinitionProperty, value);
    }

    /// <summary>Also open on pointer hover (<c>openOnHover</c>). Tap, click and keyboard focus always open it.</summary>
    public bool OpenOnHover
    {
        get => (bool)GetValue(OpenOnHoverProperty);
        set => SetValue(OpenOnHoverProperty, value);
    }

    /// <summary>Placement relative to the term (<c>align</c>). Default <see cref="CdsPlacement.Bottom"/>.</summary>
    public CdsPlacement Placement
    {
        get => (CdsPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>Open state (two-way; <c>defaultOpen</c>).</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The type style the term inherits from its surroundings. Default <c>body-01</c>.</summary>
    public CdsTypeStyle TypeStyle
    {
        get => (CdsTypeStyle)GetValue(TypeStyleProperty);
        set => SetValue(TypeStyleProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        _clicked = !IsOpen;
        IsOpen = !IsOpen;
    }

    /// <inheritdoc/>
    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        if (Is(CdsInteractionState.Hover) && OpenOnHover) _timer.PointerEntered(100);
        else _timer.PointerExited(300);
        _timer.FocusChanged(Is(CdsInteractionState.FocusVisible));
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        SetColor(_term, Label.TextColorProperty, CdsDefinitionTooltipTokens.Text);
        SetBrush(_line, Microsoft.Maui.Controls.Shapes.Shape.StrokeProperty, CdsDefinitionTooltipTokens.Underline(State));
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void ApplySemantics()
    {
        SemanticProperties.SetDescription(_hit, Text);
        SemanticProperties.SetHint(_hit, Definition);
        _hit.IsExpanded = IsOpen;
    }

    void UpdateOpen()
    {
        if (_timer.IsOpen) IsOpen = true;
        else if (!_clicked) IsOpen = false;
    }

    void OnIsOpenChanged(bool open)
    {
        _hit.IsExpanded = open;
        if (open) Show();
        else
        {
            _clicked = false;
            Hide();
        }
    }

    void Show()
    {
        if (_overlay is not null || string.IsNullOrEmpty(Definition) || !IsLoaded) return;
        var text = new CdsText(Definition, CdsTypeStyle.Body01, CdsTextTone.Inverse)
        {
            Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03),
            LineBreakMode = LineBreakMode.WordWrap,
        };
        var surface = new CdsPopoverSurface(text, hasCaret: true, highContrast: true, dropShadow: false, maxWidth: 176);
        var hoverOnly = !_clicked && Is(CdsInteractionState.Hover) && !Is(CdsInteractionState.Focused);
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = this,
            Placement = Placement,
            Offset = surface.OffsetFor(10),
            LightDismiss = !hoverOnly,
            InputTransparent = true,
            ReturnFocus = false,
        });
        surface.Track(_overlay);
        _overlay.Closed += OnOverlayClosed;
    }

    void Hide()
    {
        var overlay = _overlay;
        if (overlay is null) return;
        _overlay = null;
        overlay.Closed -= OnOverlayClosed;
        overlay.Close();
    }

    void OnOverlayClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnOverlayClosed;
        _overlay = null;
        _clicked = false;
        _timer.Reset();
        if (IsOpen) IsOpen = false;
    }
}
