using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>The drawn segment of a <see cref="CdsContentSwitcher"/> (internal: the switcher builds one per <see cref="CdsSwitch"/>).</summary>
internal sealed class CdsSwitchView : CdsControl
{
    readonly CdsContentSwitcher _owner;
    readonly Grid _root = new();
    readonly Border _background = new() { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 4 }, InputTransparent = true };
    readonly Border _indicator = new() { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 4 }, InputTransparent = true };
    readonly CdsLayer _indicatorLayer;
    readonly BoxView _divider = new() { WidthRequest = 1, HeightRequest = 16, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, InputTransparent = true, IsVisible = false };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly CdsIcon _icon = new() { IsVisible = false };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset, 4);
    readonly Border _innerRing = new() { StrokeThickness = 1, Margin = new Thickness(2), BackgroundColor = Colors.Transparent, StrokeShape = new RoundRectangle { CornerRadius = 2 }, InputTransparent = true, IsVisible = false };
    readonly CdsHitTarget _hit;
    string? _dividerKey;

    public CdsSwitchView(CdsContentSwitcher owner, int index)
    {
        _owner = owner;
        Index = index;

        _indicatorLayer = new CdsLayer { Body = _indicator, InputTransparent = true, IsVisible = false };
        _hit = CreateHitTarget(CdsAccessibilityRole.Tab);
        AutomationProperties.SetExcludedWithChildren(_label, true);
        CdsTokens.SetBrush(_innerRing, Border.StrokeProperty, CdsColorKeys.FocusInset);

        _root.Add(_background);
        _root.Add(_indicatorLayer);
        _root.Add(_divider);
        _root.Add(_label);
        _root.Add(_icon);
        _root.Add(_ring);
        _root.Add(_innerRing);
        _root.Add(_hit);
        SetChrome(_root);
    }

    public int Index { get; }

    public bool IsSwitchSelected { get; private set; }

    public CdsHitTarget Hit => _hit;


    /// <summary>True while selected, hovered or focused: hides the dividers on both sides.</summary>
    public bool HidesDividers => IsSwitchSelected || Is(CdsInteractionState.Hover) || Is(CdsInteractionState.FocusVisible);

    protected internal override bool WantsNavigationKeys => true;

    public void Update(CdsSwitch segment, bool selected)
    {
        IsSwitchSelected = selected;
        var iconOnly = _owner.IsIconOnly;
        var size = _owner.Size;
        var height = CdsContentSwitcherTokens.Height(size);

        IsEnabled = segment.IsEnabled;
        SetState(CdsInteractionState.Selected, selected);

        _root.HeightRequest = height;
        _label.IsVisible = !iconOnly;
        _label.Text = segment.Text;
        _label.Margin = new Thickness(CdsContentSwitcherTokens.InlinePadding(size), 0);
        _icon.IsVisible = iconOnly;
        _icon.Icon = segment.Icon;
        _icon.HorizontalOptions = LayoutOptions.Center;
        _icon.VerticalOptions = LayoutOptions.Center;

        var name = segment.Text ?? string.Empty;
        SemanticProperties.SetDescription(_hit, name);
        if (!string.IsNullOrEmpty(name)) ToolTipProperties.SetText(_hit, name);
        else _hit.ClearValue(ToolTipProperties.TextProperty);
        _hit.IsSelected = selected;
        RefreshVisuals();
    }

    public void SetDivider(string? key)
    {
        _dividerKey = key;
        RefreshVisuals();
    }

    protected override void OnActivated() => _owner.OnSegmentActivated(Index);

    protected override void ApplyVisuals()
    {
        var v = CdsContentSwitcherTokens.Resolve(_owner.IsLowContrast, State, IsSwitchSelected);
        SetColor(_background, BackgroundColorProperty, v.Background);
        _indicatorLayer.IsVisible = v.Indicator is not null;
        SetColor(_indicator, BackgroundColorProperty, v.Indicator);
        SetBrush(_indicator, Border.StrokeProperty, v.IndicatorBorder);
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        SetColor(_divider, BoxView.ColorProperty, _dividerKey);
        _divider.IsVisible = _dividerKey is not null;
        var focus = Is(CdsInteractionState.FocusVisible);
        _ring.IsActive = focus;
        _innerRing.IsVisible = focus;
    }
}
