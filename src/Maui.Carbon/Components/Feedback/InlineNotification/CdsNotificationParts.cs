namespace Maui.Carbon;

/// <summary>The 48 x 48 close button of a notification: 16px close icon, hover fill, inset focus ring.</summary>
internal sealed class CdsNotificationCloseButton : CdsControl
{
    readonly Grid _root = new() { WidthRequest = 48, HeightRequest = 48 };
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsIcon _icon = new(CdsIconName.Close, 16) { InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    CdsNotificationVisuals _visuals = CdsNotificationTokens.Resolve(CdsNotificationKind.Info, false);

    public CdsNotificationCloseButton()
    {
        AutomationProperties.SetExcludedWithChildren(_fill, true);
        _hit = CreateHitTarget();
        _root.Add(_fill);
        _root.Add(_icon);
        _root.Add(_ring);
        _root.Add(_hit);
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.Start;
        SetChrome(_root);
        Description = "Close notification";
    }

    public event EventHandler? Clicked;

    public string Description
    {
        get => SemanticProperties.GetDescription(_hit);
        set
        {
            SemanticProperties.SetDescription(_hit, value);
            ToolTipProperties.SetText(_hit, value);
        }
    }

    public CdsNotificationVisuals Visuals
    {
        get => _visuals;
        set
        {
            _visuals = value;
            RefreshVisuals();
        }
    }

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var active = Is(CdsInteractionState.Hover) || Is(CdsInteractionState.Pressed);
        SetColor(_fill, BoxView.ColorProperty, active ? _visuals.CloseHover : null);
        SetColor(_icon, CdsIcon.ColorProperty, _visuals.CloseIcon);
        _ring.SetColorKey(_visuals.Focus);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}

/// <summary>The small (32px) action button of an actionable notification or callout: ghost or tertiary.</summary>
internal sealed class CdsNotificationAction : CdsControl
{
    readonly Grid _root = new() { HeightRequest = CdsSizing.Small, MinimumHeightRequest = CdsSizing.Small };
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle(), InputTransparent = true };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.BodyCompact01, LineBreakMode = LineBreakMode.NoWrap, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    bool _tertiary;
    bool _lowContrast;
    CdsNotificationVisuals _visuals = CdsNotificationTokens.Resolve(CdsNotificationKind.Info, false);

    public CdsNotificationAction()
    {
        AutomationProperties.SetExcludedWithChildren(_box, true);
        _box.Content = new Grid { Padding = new Thickness(CdsSpacing.Spacing05 - 1, 0), Children = { _label } };
        _hit = CreateHitTarget();
        _root.Add(_box);
        _root.Add(_ring);
        _root.Add(_hit);
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(_root);
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

    public bool Tertiary
    {
        get => _tertiary;
        set
        {
            _tertiary = value;
            RefreshVisuals();
        }
    }

    public bool LowContrast
    {
        get => _lowContrast;
        set
        {
            _lowContrast = value;
            RefreshVisuals();
        }
    }

    public CdsNotificationVisuals Visuals
    {
        get => _visuals;
        set
        {
            _visuals = value;
            RefreshVisuals();
        }
    }

    public string? DescribedBy
    {
        set => SemanticProperties.SetHint(_hit, value);
    }

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var v = CdsNotificationTokens.ResolveAction(_lowContrast, _tertiary, State);
        SetColor(_box, BackgroundColorProperty, v.Background);
        SetBrush(_box, Border.StrokeProperty, v.Border);
        SetColor(_label, Label.TextColorProperty, v.Text);
        _ring.SetColorKey(_visuals.Focus);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}
