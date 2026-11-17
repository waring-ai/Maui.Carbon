using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>The 48x48 ghost close button in the top end corner of a modal header (20px close icon).</summary>
internal sealed class CdsModalCloseButton : CdsControl
{
    readonly Grid _root = new() { WidthRequest = CdsModalSizing.CloseButtonSize, HeightRequest = CdsModalSizing.CloseButtonSize };
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsIcon _icon = new(CdsIconName.Close, 20) { InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;

    public CdsModalCloseButton()
    {
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.Start;
        _hit = CreateHitTarget();
        AutomationProperties.SetExcludedWithChildren(_fill, true);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _root.Add(_fill);
        _root.Add(_icon);
        _root.Add(_ring);
        _root.Add(_hit);
        SetChrome(_root);
        Description = "Close";
        RefreshVisuals();
    }

    public event EventHandler? Clicked;

    /// <summary>The accessible name and tooltip.</summary>
    public string Description
    {
        get => SemanticProperties.GetDescription(_hit);
        set
        {
            SemanticProperties.SetDescription(_hit, value);
            ToolTipProperties.SetText(_hit, value);
        }
    }

    internal VisualElement HitTarget => _hit;

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        SetColor(_fill, BoxView.ColorProperty, CdsModalTokens.CloseButtonBackground(State));
        SetColor(_icon, CdsIcon.ColorProperty, CdsModalTokens.CloseButtonIcon(State));
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}

/// <summary>A footer button: 64px high, fills its column, label at the top start corner, no radius or gap.</summary>
internal sealed class CdsModalButton : CdsControl
{
    readonly Grid _root = new() { HeightRequest = CdsModalSizing.FooterHeight };
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), Padding = 0, InputTransparent = true };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(15, 14, 15, 0), InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Button);
    readonly CdsHitTarget _hit;
    CdsButtonKind _kind = CdsButtonKind.Secondary;

    public CdsModalButton(string text, CdsButtonKind kind)
    {
        HorizontalOptions = LayoutOptions.Fill;
        _label.Text = text;
        _kind = kind;
        _hit = CreateHitTarget();
        AutomationProperties.SetExcludedWithChildren(_box, true);
        _box.Content = _label;
        _root.Add(_box);
        _root.Add(_ring);
        _root.Add(_hit);
        SemanticProperties.SetDescription(_hit, text);
        SetChrome(_root);
        RefreshVisuals();
    }

    public event EventHandler? Clicked;

    public string? Text
    {
        get => _label.Text;
        set
        {
            _label.Text = value;
            SemanticProperties.SetDescription(_hit, value);
        }
    }

    public CdsButtonKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            RefreshVisuals();
        }
    }

    internal VisualElement HitTarget => _hit;

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var v = CdsButtonTokens.Resolve(_kind, State);
        SetColor(_box, BackgroundColorProperty, v.Background);
        SetBrush(_box, Border.StrokeProperty, v.Border);
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}
