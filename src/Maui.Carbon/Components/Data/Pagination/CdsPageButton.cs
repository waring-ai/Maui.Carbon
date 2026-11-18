namespace Maui.Carbon;

/// <summary>Colors and sizes of the buttons shared by <see cref="CdsPagination"/> and <see cref="CdsPaginationNav"/>.</summary>
internal static class CdsPageButtonTokens
{
    /// <summary>The fill: transparent, or the hover token while hovered or pressed (not when disabled).</summary>
    public static string? Fill(bool hover, bool disabled, string hoverKey) => hover && !disabled ? hoverKey : null;

    /// <summary>Glyph color: <c>icon-primary</c>, <c>icon-disabled</c> when disabled.</summary>
    public static string Icon(bool disabled) => disabled ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;

    /// <summary>Text color: <c>text-primary</c>, <c>text-disabled</c> when disabled.</summary>
    public static string Text(bool disabled) => disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;
}

/// <summary>
/// A flat square-ish button used by pagination: an arrow icon or a page label, optional 1px start divider, hover fill, an
/// optional active underline bar (16 wide, 4 high, <c>border-interactive</c>) and an inset focus ring. Internal building block.
/// </summary>
internal sealed class CdsPageButton : CdsControl
{
    readonly Grid _root = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly BoxView _divider = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true, IsVisible = false };
    readonly BoxView _bar = new() { WidthRequest = CdsSpacing.Spacing05, HeightRequest = CdsSpacing.Spacing02, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.BodyCompact01, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true, IsVisible = false };
    readonly CdsIcon _icon = new() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true, IsVisible = false };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    string _hoverKey = CdsColorKeys.LayerHover01;
    bool _active;

    public CdsPageButton()
    {
        AutomationProperties.SetExcludedWithChildren(_label, true);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _root.Add(_fill);
        _root.Add(_divider);
        _root.Add(_bar);
        _root.Add(_label);
        _root.Add(_icon);
        _root.Add(_ring);
        _hit = CreateHitTarget();
        _root.Add(_hit);
        SetChrome(_root);
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
    }

    public event EventHandler? Clicked;

    /// <summary>The hover token: <c>layer-hover-01</c> for the pagination bar, <c>background-hover</c> for the nav.</summary>
    public string HoverKey
    {
        get => _hoverKey;
        set
        {
            _hoverKey = value;
            RefreshVisuals();
        }
    }

    public void SetIcon(CdsIconName icon)
    {
        _icon.Icon = icon;
        _icon.IsVisible = true;
        _label.IsVisible = false;
    }

    public void SetText(string text, bool active)
    {
        _label.Text = text;
        _label.TypeStyle = active ? CdsTypeStyle.HeadingCompact01 : CdsTypeStyle.BodyCompact01;
        _label.IsVisible = true;
        _icon.IsVisible = false;
        _active = active;
        _bar.IsVisible = active;
        _hit.IsSelected = active;
        RefreshVisuals();
    }

    public void SetSize(double width, double height, double minWidth = 0)
    {
        _root.HeightRequest = height;
        _root.WidthRequest = width > 0 ? width : -1;
        _root.MinimumWidthRequest = minWidth;
        _label.Margin = new Thickness(width > 0 ? 0 : CdsSpacing.Spacing02, 0);
    }

    public bool HasStartBorder
    {
        get => _divider.IsVisible;
        set => _divider.IsVisible = value;
    }

    public void SetName(string name, string? tooltip = null)
    {
        SemanticProperties.SetDescription(_hit, name);
        if (tooltip is not null) ToolTipProperties.SetText(_hit, tooltip);
    }

    public void SetCurrent(bool current) => _hit.IsSelected = current;

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        var hover = Is(CdsInteractionState.Hover) || Is(CdsInteractionState.Pressed);
        SetColor(_fill, BoxView.ColorProperty, CdsPageButtonTokens.Fill(hover, disabled, _hoverKey));
        SetColor(_divider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        SetColor(_bar, BoxView.ColorProperty, CdsColorKeys.BorderInteractive);
        SetColor(_icon, CdsIcon.ColorProperty, CdsPageButtonTokens.Icon(disabled));
        SetColor(_label, CdsText.TextColorProperty, CdsPageButtonTokens.Text(disabled));
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
        _bar.IsVisible = _active && !disabled;
        // Carbon drops the divider while a button has keyboard focus so the ring is not cut.
        _divider.Opacity = Is(CdsInteractionState.FocusVisible) ? 0 : 1;
    }
}
