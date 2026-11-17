namespace Maui.Carbon;

/// <summary>
/// The shared row of the side nav (<see cref="CdsSideNavLink"/>, <see cref="CdsSideNavMenu"/>, <see cref="CdsSideNavMenuItem"/>): a
/// 32 high (48 with <see cref="IsLarge"/>) row with an optional 16px icon, 24 before the label, 16 of padding each side and
/// a 2px inset focus ring. Current rows fill with <c>background-selected</c> and show a 3px <c>border-interactive</c> bar on the start edge.
/// </summary>
public abstract class CdsSideNavRow : CdsShellItem
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsSideNavRow), null,
        propertyChanged: static (b, _, _) => ((CdsSideNavRow)b).UpdateRow());

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsSideNavRow), null,
        propertyChanged: static (b, _, _) => ((CdsSideNavRow)b).UpdateRow());

    /// <summary>Bindable property for <see cref="IsLarge"/>.</summary>
    public static readonly BindableProperty IsLargeProperty = BindableProperty.Create(
        nameof(IsLarge), typeof(bool), typeof(CdsSideNavRow), false,
        propertyChanged: static (b, _, _) => ((CdsSideNavRow)b).UpdateRow());

    readonly Grid _row = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly BoxView _bar = new() { WidthRequest = 3, HorizontalOptions = LayoutOptions.Start, InputTransparent = true, IsVisible = false };
    readonly CdsIcon _icon = new(CdsIconName.Add, 16) { VerticalOptions = LayoutOptions.Center, InputTransparent = true, IsVisible = false };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    double _indent = -1;

    /// <summary>Creates a row with the given accessibility role.</summary>
    protected CdsSideNavRow(CdsAccessibilityRole role)
    {
        HorizontalOptions = LayoutOptions.Fill;
        _row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        AutomationProperties.SetExcludedWithChildren(_label, true);
        _hit = CreateHitTarget(role);

        var content = new Grid();
        content.Add(_fill);
        content.Add(_bar);
        content.Add(_row);
        content.Add(_ring);
        content.Add(_hit);
        _row.Add(_icon, 0);
        _row.Add(_label, 1);
        SetChrome(WrapChrome(content));
    }

    /// <summary>The label.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The 16px icon before the label (required for the rail, which shows icons only).</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>A 48 high row instead of 32.</summary>
    public bool IsLarge
    {
        get => (bool)GetValue(IsLargeProperty);
        set => SetValue(IsLargeProperty, value);
    }

    /// <summary>The native hit target (for subclasses that add semantics).</summary>
    protected CdsHitTarget HitTarget => _hit;

    /// <summary>The trailing column of the row (the chevron of a menu).</summary>
    protected void SetTrailing(View trailing) => _row.Add(trailing, 2);

    /// <summary>Extra start padding for rows inside a menu (32, or 72 when the group has an icon). <c>-1</c>: the default 16.</summary>
    internal double Indent
    {
        get => _indent;
        set
        {
            _indent = value;
            UpdateRow();
        }
    }

    /// <summary>Lets a subclass add content below the row (a menu's items). Runs while the base constructor builds the chrome.</summary>
    protected virtual View WrapChrome(View row) => row;

    /// <summary>The type style of the label.</summary>
    protected virtual CdsTypeStyle LabelStyle => CdsTypeStyle.HeadingCompact01;

    /// <summary>The colors of the row in its current state.</summary>
    protected virtual CdsShellVisuals Resolve() => CdsUIShellTokens.SideNavRow(State, IsActive);

    /// <summary>Whether the row currently shows the selected fill and bar.</summary>
    protected virtual bool ShowsSelection => IsActive;

    /// <summary>Whether the row is reachable by Tab (a collapsed side nav removes its links from the tab order).</summary>
    internal void SetReachable(bool reachable) => _hit.IsTabStop = reachable;

    /// <inheritdoc/>
    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        if ((previous & CdsInteractionState.Focused) != (State & CdsInteractionState.Focused))
            CdsShellContext.FindSideNav(this)?.ItemFocusChanged(Is(CdsInteractionState.Focused));
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = Resolve();
        SetColor(_fill, BoxView.ColorProperty, v.Background);
        SetColor(_bar, BoxView.ColorProperty, v.Bar);
        _bar.IsVisible = v.Bar is not null;
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    /// <summary>Re-applies text, icon, size and accessibility.</summary>
    protected void UpdateRow()
    {
        _label.Text = Text;
        _label.TypeStyle = LabelStyle;
        _icon.Icon = Icon;
        _icon.IsVisible = Icon is not null;
        _icon.Margin = new Thickness(0, 0, CdsSpacing.Spacing06, 0);
        var start = _indent >= 0 ? _indent : CdsSpacing.Spacing05;
        _row.Padding = new Thickness(start, 0, CdsSpacing.Spacing05, 0);
        _row.HeightRequest = IsLarge ? UIShellModel.SideNavLargeRowHeight : UIShellModel.SideNavRowHeight;
        SemanticProperties.SetDescription(_hit, Text);
        ToolTipProperties.SetText(_hit, Text ?? string.Empty);
        RefreshVisuals();
    }
}
