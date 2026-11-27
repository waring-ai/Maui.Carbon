namespace Maui.Carbon;

/// <summary>The drawn tab of a <see cref="CdsTabs"/> row (internal: Tabs builds one per <see cref="CdsTab"/>).</summary>
internal sealed class CdsTabItemView : CdsControl
{
    readonly CdsTabs _owner;
    readonly Grid _root = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly BoxView _bottomRule = new() { HeightRequest = 2, VerticalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
    readonly BoxView _topRule = new() { HeightRequest = 2, VerticalOptions = LayoutOptions.Start, InputTransparent = true, IsVisible = false };
    readonly Grid _content = new() { InputTransparent = true, VerticalOptions = LayoutOptions.Center };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 };
    readonly CdsText _secondary = new(string.Empty, CdsTypeStyle.Label01) { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, IsVisible = false };
    readonly CdsIcon _icon = new() { IsVisible = false };
    readonly Border _close = new() { WidthRequest = 24, HeightRequest = 24, StrokeThickness = 0, IsVisible = false, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, 8, 0) };
    readonly CdsIcon _closeIcon = new(CdsIconName.Close);
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    bool _closeHover;
    CdsTabVisuals _visuals;
    // Vertical tabs only, built on first use: the 3px start edge and the 1px end rule.
    BoxView? _startEdge;
    BoxView? _endRule;

    public CdsTabItemView(CdsTabs owner, int index)
    {
        _owner = owner;
        Index = index;

        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _content.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _content.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _content.Add(_label, 0, 0);
        _content.Add(_icon, 1, 0);
        _content.Add(_secondary, 0, 1);
        Grid.SetColumnSpan(_secondary, 2);
        _label.VerticalOptions = LayoutOptions.Center;
        _icon.Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0);
        _icon.VerticalOptions = LayoutOptions.Center;
        _secondary.Margin = new Thickness(0, CdsSpacing.Spacing01, 0, 0);
        AutomationProperties.SetExcludedWithChildren(_content, true);

        _close.Content = _closeIcon;
        _close.StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle();
        AutomationProperties.SetExcludedWithChildren(_close, true);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => _owner.RequestClose(Index);
        _close.GestureRecognizers.Add(tap);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => SetCloseHover(true);
        pointer.PointerExited += (_, _) => SetCloseHover(false);
        _close.GestureRecognizers.Add(pointer);

        _hit = CreateHitTarget(CdsAccessibilityRole.Tab);
        _root.Add(_fill);
        _root.Add(_content);
        _root.Add(_bottomRule);
        _root.Add(_topRule);
        _root.Add(_ring);
        _root.Add(_hit);
        _root.Add(_close);
        SetChrome(_root);
    }

    public int Index { get; }

    public CdsHitTarget Hit => _hit;

    public bool IsTabSelected { get; private set; }


    protected internal override bool WantsNavigationKeys => true;

    /// <summary>Applies the tab data and the owner's settings.</summary>
    public void Update(CdsTab tab, bool selected)
    {
        IsTabSelected = selected;
        if (_owner.IsVertical)
        {
            UpdateVertical(tab, selected);
            return;
        }
        if (_startEdge is not null) _startEdge.IsVisible = false;
        if (_endRule is not null) _endRule.IsVisible = false;
        _bottomRule.HeightRequest = 2;
        _label.MaxLines = 1;
        HorizontalOptions = LayoutOptions.Fill;
        var type = _owner.Type;
        var iconOnly = _owner.IsIconOnly && tab.Icon is not null;
        var hasSecondary = type == CdsTabsType.Contained && _owner.HasSecondaryLabels;
        var height = CdsTabsTokens.Height(type, iconOnly, _owner.IconSize, hasSecondary);

        IsEnabled = tab.IsEnabled;
        SetState(CdsInteractionState.Selected, selected);

        _label.IsVisible = !iconOnly;
        _label.Text = tab.Text;
        _icon.Icon = tab.Icon;
        _icon.IsVisible = tab.Icon is not null;
        _icon.Margin = iconOnly ? Thickness.Zero : new Thickness(CdsSpacing.Spacing03, 0, 0, 0);
        _icon.HorizontalOptions = iconOnly ? LayoutOptions.Center : LayoutOptions.End;
        Grid.SetColumn(_icon, iconOnly ? 0 : 1);
        Grid.SetColumnSpan(_icon, iconOnly ? 2 : 1);
        _secondary.Text = tab.SecondaryLabel;
        _secondary.IsVisible = type == CdsTabsType.Contained && !string.IsNullOrEmpty(tab.SecondaryLabel) && !iconOnly;

        var dismissible = _owner.IsDismissible && !iconOnly;
        _close.IsVisible = dismissible;
        _content.Margin = iconOnly ? Thickness.Zero : new Thickness(CdsSpacing.Spacing05, 0, dismissible ? CdsSpacing.Spacing08 : CdsSpacing.Spacing05, 0);

        _root.HeightRequest = height;
        if (iconOnly)
        {
            MinimumWidthRequest = height;
            WidthRequest = height;
            MaximumWidthRequest = height;
        }
        else
        {
            WidthRequest = -1;
            MinimumWidthRequest = CdsTabsTokens.MinWidth;
            MaximumWidthRequest = _owner.IsFullWidthActive ? double.PositiveInfinity : CdsTabsTokens.MaxWidth;
        }
        MinimumHeightRequest = height;

        // The tab margin that separates contained tabs is a 1px gap.
        Margin = type == CdsTabsType.Contained ? new Thickness(0, 0, 1, 0) : Thickness.Zero;

        var name = tab.Text ?? string.Empty;
        SemanticProperties.SetDescription(_hit, string.IsNullOrEmpty(tab.SecondaryLabel) ? name : $"{name}, {tab.SecondaryLabel}");
        if (iconOnly && !string.IsNullOrEmpty(tab.Text)) ToolTipProperties.SetText(_hit, tab.Text);
        else _hit.ClearValue(ToolTipProperties.TextProperty);
        _hit.IsSelected = selected;

        RefreshVisuals();
    }

    // Carbon's vertical tab: 64 high and as wide as the column, a label of up to two lines (and the icon) 16 from each side,
    // a 1px rule under it and after it and a 3px edge at its start. No secondary label, close icon or icon-only form.
    void UpdateVertical(CdsTab tab, bool selected)
    {
        IsEnabled = tab.IsEnabled;
        SetState(CdsInteractionState.Selected, selected);
        if (_startEdge is null)
        {
            _startEdge = new BoxView { WidthRequest = CdsTabsTokens.VerticalEdgeWidth, HorizontalOptions = LayoutOptions.Start, InputTransparent = true };
            _endRule = new BoxView { WidthRequest = 1, HorizontalOptions = LayoutOptions.End, InputTransparent = true };
            // Above the fill and under the focus ring.
            _root.Insert(_root.IndexOf(_ring), _startEdge);
            _root.Insert(_root.IndexOf(_ring), _endRule);
        }
        _startEdge.IsVisible = true;

        _label.IsVisible = true;
        _label.Text = tab.Text;
        _label.MaxLines = 2;
        _icon.Icon = tab.Icon;
        _icon.IsVisible = tab.Icon is not null;
        _icon.Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0);
        _icon.HorizontalOptions = LayoutOptions.End;
        Grid.SetColumn(_icon, 1);
        Grid.SetColumnSpan(_icon, 1);
        _secondary.IsVisible = false;
        _close.IsVisible = false;
        _content.Margin = new Thickness(CdsSpacing.Spacing05, 0);
        _topRule.IsVisible = false;
        _bottomRule.HeightRequest = 1;

        _root.HeightRequest = CdsTabsTokens.VerticalHeight;
        MinimumHeightRequest = CdsTabsTokens.VerticalHeight;
        WidthRequest = -1;
        MinimumWidthRequest = -1;
        MaximumWidthRequest = double.PositiveInfinity;
        HorizontalOptions = LayoutOptions.Fill;
        Margin = Thickness.Zero;

        SemanticProperties.SetDescription(_hit, tab.Text ?? string.Empty);
        _hit.ClearValue(ToolTipProperties.TextProperty);
        _hit.IsSelected = selected;
        RefreshVisuals();
    }

    /// <summary>The vertical visuals last applied (for tests); default while horizontal.</summary>
    internal CdsVerticalTabVisuals VerticalVisuals { get; private set; }

    void ApplyVertical()
    {
        var v = CdsTabsTokens.ResolveVertical(State, IsTabSelected);
        VerticalVisuals = v;
        SetColor(_fill, BoxView.ColorProperty, v.Background);
        SetColor(_startEdge!, BoxView.ColorProperty, v.StartEdge);
        SetColor(_endRule!, BoxView.ColorProperty, v.EndRule);
        _endRule!.IsVisible = v.EndRule is not null;
        SetColor(_bottomRule, BoxView.ColorProperty, v.BottomRule);
        _bottomRule.IsVisible = true;
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        _label.TypeStyle = v.IsSemiBold ? CdsTypeStyle.HeadingCompact01 : CdsTypeStyle.BodyCompact01;
        SetColor(_icon, CdsIcon.ColorProperty, v.Foreground);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    protected override void OnActivated() => _owner.OnTabActivated(Index);

    protected override void ApplyVisuals()
    {
        if (_owner.IsVertical && _startEdge is not null)
        {
            ApplyVertical();
            return;
        }
        var iconOnly = _owner.IsIconOnly && _icon.Icon is not null;
        _visuals = CdsTabsTokens.Resolve(_owner.Type, State, IsTabSelected, iconOnly);
        var v = _visuals;
        SetColor(_fill, BoxView.ColorProperty, v.Background);
        SetColor(_bottomRule, BoxView.ColorProperty, v.BottomRule);
        _bottomRule.IsVisible = v.BottomRule is not null;
        SetColor(_topRule, BoxView.ColorProperty, v.TopRule);
        _topRule.IsVisible = v.TopRule is not null;
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        _label.TypeStyle = v.IsSemiBold ? CdsTypeStyle.HeadingCompact01 : CdsTypeStyle.BodyCompact01;
        SetColor(_secondary, Label.TextColorProperty, v.SecondaryLabel);
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        SetColor(_closeIcon, CdsIcon.ColorProperty, v.CloseIcon);
        SetColor(_close, BackgroundColorProperty, _closeHover && !Is(CdsInteractionState.Disabled) ? CdsColorKeys.BackgroundHover : null);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void SetCloseHover(bool hover)
    {
        _closeHover = hover;
        RefreshVisuals();
    }
}
