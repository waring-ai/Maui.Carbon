using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

public abstract partial class CdsTileBase
{
    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsTileBase), null,
        propertyChanged: static (b, o, n) => ((CdsTileBase)b).OnDecoratorChanged((View?)o, (View?)n));

    /// <summary>Bindable property for <see cref="HasRoundedCorners"/>.</summary>
    public static readonly BindableProperty HasRoundedCornersProperty = BindableProperty.Create(
        nameof(HasRoundedCorners), typeof(bool), typeof(CdsTileBase), false,
        propertyChanged: static (b, _, _) => ((CdsTileBase)b).RefreshVisuals());

    // The brushes of the AI surface, rebuilt from the bound token colors when the theme or layer changes. The proxies are bound
    // once while the tile has AI presence; state changes only pick which brush shows.
    Brush? _aiFill;
    Brush? _aiHoverFill;
    Brush? _aiBorder;
    Brush? _aiInverseBorder;
    CdsTileAiVisuals? _aiVisuals;
    bool _aiBound;
    bool _rounded;
    double _decoratorInset = CdsSpacing.Spacing05;
    int _insetVersion;

    /// <summary>
    /// A view at the top end of the tile, 16 from the top and the end (Carbon's <c>decorator</c> slot, experimental): usually a
    /// <see cref="CdsAILabel"/>, which gives the tile Carbon's AI presence (see <see cref="IsAiPresence"/>). The selectable, radio
    /// and expandable tiles show the label at size xs, as Carbon does; the plain and clickable tiles keep the label's own size.
    /// The decorator takes its own taps and keys: activating it never toggles, selects, expands or follows the tile. Carbon
    /// reserves no space for it in a plain, clickable or expandable tile (keep the first line of the body short enough), and the
    /// selectable and radio tiles keep 72 of end padding so text stays clear of the label and the glyph. An expandable tile with a
    /// decorator is the interactive variant: only its chevron toggles it. Put the label's own
    /// <see cref="CdsAILabel.IsRevertActive"/> state on the label; the tile keeps its AI styling while the label shows revert,
    /// because Carbon's tile styles key off the label being present, not its state.
    /// </summary>
    public View? Decorator
    {
        get => (View?)GetValue(DecoratorProperty);
        set => SetValue(DecoratorProperty, value);
    }

    /// <summary>
    /// Rounds the corners to 4 (<c>spacing-03</c>) when the tile has AI presence (Carbon: "only valid when an AILabel is present").
    /// The square system's only tile with a radius. No effect without an <see cref="CdsAILabel"/> decorator.
    /// </summary>
    public bool HasRoundedCorners
    {
        get => (bool)GetValue(HasRoundedCornersProperty);
        set => SetValue(HasRoundedCornersProperty, value);
    }

    /// <summary>
    /// Whether the tile shows Carbon's AI presence: its <see cref="Decorator"/> is a <see cref="CdsAILabel"/>, in any state of the
    /// label. The aura rises from the bottom edge into the <c>layer-01</c> fill, the 1px border is the <c>ai-border</c> gradient,
    /// and the tile casts the <c>ai-drop-shadow</c>.
    /// </summary>
    public bool IsAiPresence => Decorator is CdsAILabel;

    /// <summary>The size a variant forces on an <see cref="CdsAILabel"/> decorator (Carbon clones it with size xs); <c>null</c> keeps the label's own.</summary>
    protected virtual CdsAILabelSize? DecoratorSize => null;

    /// <summary>The distance of the decorator from the end edge: 16, 40 beside a glyph.</summary>
    protected virtual double DecoratorEndInset => CdsSpacing.Spacing05;

    /// <summary>Called after <see cref="Decorator"/> changed (variants adjust their padding or variant).</summary>
    protected virtual void OnDecoratorReplaced()
    {
    }

    /// <summary>Moves the decorator to <see cref="DecoratorEndInset"/>, over <c>duration-fast-02</c> when <paramref name="animate"/> is set (reduced motion jumps).</summary>
    protected void UpdateDecoratorInset(bool animate)
    {
        if (Decorator is not { } decorator) return;
        var target = DecoratorEndInset;
        var version = ++_insetVersion;
        if (!animate || !IsLoaded || Handler is null || CdsMotion.IsReducedMotion || _decoratorInset == target)
        {
            SetDecoratorInset(decorator, target);
            return;
        }
        _ = CdsMotion.AnimateAsync(this, "CdsTileDecoratorInset", p =>
        {
            if (version == _insetVersion) SetDecoratorInset(decorator, p);
        }, _decoratorInset, target, CdsDuration.Fast02, CdsEasing.StandardProductive);
    }

    void SetDecoratorInset(View decorator, double inset)
    {
        _decoratorInset = inset;
        decorator.Margin = new Thickness(0, CdsSpacing.Spacing05, inset, 0);
    }

    void OnDecoratorChanged(View? oldDecorator, View? newDecorator)
    {
        if (oldDecorator is not null) Stack.Remove(oldDecorator);
        if (newDecorator is not null)
        {
            if (newDecorator is CdsAILabel label && DecoratorSize is { } size) label.Size = size;
            newDecorator.HorizontalOptions = LayoutOptions.End;
            newDecorator.VerticalOptions = LayoutOptions.Start;
            _decoratorInset = DecoratorEndInset;
            SetDecoratorInset(newDecorator, _decoratorInset);
            // Last in the cell: above the tile's hit target, so the label takes its own taps and keys.
            Stack.Add(newDecorator);
        }
        _insetVersion++;
        OnDecoratorReplaced();
        RefreshVisuals();
    }

    // The tile without AI presence: the flat fill, and a 1px border for the selectable variants.
    void ApplyFlatSurface(CdsTileVisuals v)
    {
        if (_aiBound) LeaveAi();
        SetShape(false);
        Surface.StrokeThickness = BorderThickness;
        SetColor(Surface, BackgroundColorProperty, v.Background);
        SetBrush(Surface, Border.StrokeProperty, v.Border);
    }

    // The tile with AI presence: fill and border are gradient brushes that replace the token-bound fill and stroke (a flat color
    // set on every state change would wipe them), so the surface is resolved here from the AI tokens instead.
    void ApplyAiSurface(CdsInteractionState state)
    {
        EnterAi();
        _aiVisuals = CdsTileTokens.ResolveAi(TileKind, state);
        Surface.StrokeThickness = 1;
        SetShape(HasRoundedCorners);
        ApplyAiBrushes();
    }

    void EnterAi()
    {
        if (_aiBound) return;
        _aiBound = true;
        Surface.RemoveDynamicResource(BackgroundColorProperty);
        // Park the stroke's token proxy on transparent: the brushes below are set directly and a theme change must not set it again.
        SetBrush(Surface, Border.StrokeProperty, null);
        CdsColorProxies.Bind(Surface, colors =>
        {
            if (colors[0] is { } aura && colors[1] is { } surface) _aiFill = CdsAiTreatment.CreateFill(aura, surface);
            if (colors[2] is { } start && colors[3] is { } end) _aiBorder = CdsAiTreatment.CreateBorder(start, end);
            ApplyAiBrushes();
        }, CdsAiTokens.Aura, CdsColorKeys.Layer01, CdsAiTokens.BorderStart, CdsAiTokens.BorderEnd);
        CdsColorProxies.Bind(Stack, colors =>
        {
            if (colors[0] is { } aura && colors[1] is { } surface) _aiHoverFill = CdsAiTreatment.CreateFill(aura, surface);
            if (colors[2] is { } inverse) _aiInverseBorder = new SolidColorBrush(inverse);
            ApplyAiBrushes();
        }, CdsAiTokens.AuraHoverStart, CdsAiTokens.AuraHoverBackground, CdsColorKeys.BorderInverse);
        CdsTokens.SetShadow(Surface, CdsAiTokens.DropShadow, offsetY: 4, radius: 8);
        Surface.PropertyChanged += OnSurfacePropertyChanged;
    }

    // The stroke's token proxy (parked on transparent above) sets the stroke once the theme resolves, and a later brush of
    // ours would not outlast it: whatever sets the surface's stroke or fill while AI is shown, the AI brushes are set again.
    void OnSurfacePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == Border.StrokeProperty.PropertyName || e.PropertyName == BackgroundProperty.PropertyName) ApplyAiBrushes();
    }

    void LeaveAi()
    {
        _aiBound = false;
        _aiVisuals = null;
        _aiFill = _aiHoverFill = _aiBorder = _aiInverseBorder = null;
        Surface.PropertyChanged -= OnSurfacePropertyChanged;
        CdsColorProxies.Unbind(Surface);
        CdsColorProxies.Unbind(Stack);
        Surface.ClearValue(BackgroundProperty);
        CdsTokens.SetShadow(Surface, null);
        Surface.Stroke = Brush.Transparent;
        // The flat fill and stroke bind again with the next SetColor and SetBrush.
        ForgetColors();
    }

    void ApplyAiBrushes()
    {
        if (!_aiBound || _aiVisuals is not { } ai) return;
        var fill = ai.AuraKey == CdsAiTokens.AuraHoverStart ? _aiHoverFill : _aiFill;
        if (fill is not null && !ReferenceEquals(Surface.Background, fill)) Surface.Background = fill;
        var stroke = ai.Border is null ? _aiBorder : _aiInverseBorder;
        if (stroke is not null && !ReferenceEquals(Surface.Stroke, stroke)) Surface.Stroke = stroke;
    }

    void SetShape(bool rounded)
    {
        if (_rounded == rounded) return;
        _rounded = rounded;
        Surface.StrokeShape = rounded ? new RoundRectangle { CornerRadius = CdsSpacing.Spacing03 } : new Rectangle();
    }
}
