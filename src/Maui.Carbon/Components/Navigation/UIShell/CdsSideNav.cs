namespace Maui.Carbon;

/// <summary>
/// The side navigation of the shell: a <c>background</c> panel under the header holding a <see cref="CdsSideNavItems"/> list (put it in
/// <see cref="CdsContainer.Body"/>), optionally topped by a <see cref="Header"/> and ended by a <see cref="Footer"/>. It is 256 wide
/// when expanded, a 48 icon rail with <see cref="IsRail"/> (which expands over the page on hover, focus or
/// <see cref="IsExpanded"/> and collapses on pointer exit, blur or Escape), always 256 with <see cref="IsFixed"/>, and by default
/// persistent from the lg breakpoint (1056) up and hidden (0 wide) below it until <see cref="IsExpanded"/>, with a full-screen
/// <c>overlay</c> scrim behind it. Widths animate over <c>duration-fast-02</c> and jump under reduced motion. A collapsed nav removes its links
/// from the tab order. Use it when there are more than five destinations; do not build a third navigation level. A navigation
/// landmark: give it a <see cref="Label"/>.
/// </summary>
public class CdsSideNav : CdsContainer
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsSideNav), "Side navigation",
        propertyChanged: static (b, _, n) => SemanticProperties.SetDescription((CdsSideNav)b, (string?)n));

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsSideNav), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsSideNav)b).OnModeChanged());

    /// <summary>Bindable property for <see cref="IsRail"/>.</summary>
    public static readonly BindableProperty IsRailProperty = BindableProperty.Create(
        nameof(IsRail), typeof(bool), typeof(CdsSideNav), false,
        propertyChanged: static (b, _, _) => ((CdsSideNav)b).OnModeChanged());

    /// <summary>Bindable property for <see cref="IsFixed"/>.</summary>
    public static readonly BindableProperty IsFixedProperty = BindableProperty.Create(
        nameof(IsFixed), typeof(bool), typeof(CdsSideNav), false,
        propertyChanged: static (b, _, _) => ((CdsSideNav)b).OnModeChanged());

    /// <summary>Bindable property for <see cref="IsPersistent"/>.</summary>
    public static readonly BindableProperty IsPersistentProperty = BindableProperty.Create(
        nameof(IsPersistent), typeof(bool), typeof(CdsSideNav), true,
        propertyChanged: static (b, _, _) => ((CdsSideNav)b).OnModeChanged());

    /// <summary>Bindable property for <see cref="Header"/>.</summary>
    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(
        nameof(Header), typeof(View), typeof(CdsSideNav), null,
        propertyChanged: static (b, o, n) => ((CdsSideNav)b).Place((View?)o, (View?)n, 0));

    /// <summary>Bindable property for <see cref="Footer"/>.</summary>
    public static readonly BindableProperty FooterProperty = BindableProperty.Create(
        nameof(Footer), typeof(View), typeof(CdsSideNav), null,
        propertyChanged: static (b, o, n) => ((CdsSideNav)b).Place((View?)o, (View?)n, 2));

    readonly Grid _clip = new() { IsClippedToBounds = true };
    readonly Grid _panel = new() { WidthRequest = UIShellModel.SideNavWidth, HorizontalOptions = LayoutOptions.Start, RowSpacing = 0 };
    readonly ScrollView _scroll = new() { VerticalScrollBarVisibility = ScrollBarVisibility.Default };
    readonly CdsBreakpointWatcher _watcher;
    const string WidthAnimation = "CdsSideNavWidth";

    double _width;
    double _animatingTo = double.NaN;
    int _widthRun;
    int _focusCount;
    bool _transient;
    bool _initialized;

    /// <summary>Creates a collapsed side nav (it expands from its <see cref="IsExpanded"/>, rail and persistence settings).</summary>
    public CdsSideNav()
    {
        TrackHover();
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Fill;
        var fill = new BoxView { InputTransparent = true };
        CdsTokens.SetColor(fill, BoxView.ColorProperty, CdsUIShellTokens.Surface);
        _panel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _panel.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        _panel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _panel.Add(_scroll, 0, 1);
        _clip.Add(fill);
        _clip.Add(_panel);
        SetChrome(_clip);
        SemanticProperties.SetDescription(this, Label);
        _watcher = new CdsBreakpointWatcher(this, () => ApplyWidth(animate: _initialized));
        Loaded += (_, _) =>
        {
            _initialized = true;
            ApplyWidth(animate: false);
        };
        ApplyWidth(animate: false);
    }

    /// <summary>Raised when the side nav's current width (animating) changed: the shell uses it to move the body.</summary>
    internal event EventHandler? WidthChanged;

    /// <summary>The accessible name of the navigation landmark.</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// Expanded (256 wide) or collapsed (two-way). The header menu button and the footer toggle set it; below lg a tap on the
    /// scrim clears it; Escape clears it.
    /// </summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>A 48 icon rail that expands on hover, focus or <see cref="IsExpanded"/>.</summary>
    public bool IsRail
    {
        get => (bool)GetValue(IsRailProperty);
        set => SetValue(IsRailProperty, value);
    }

    /// <summary>Always 256 wide, at every breakpoint (not collapsible).</summary>
    public bool IsFixed
    {
        get => (bool)GetValue(IsFixedProperty);
        set => SetValue(IsFixedProperty, value);
    }

    /// <summary>The default side nav stays open from lg up (default <c>true</c>); <c>false</c> lets it collapse there too.</summary>
    public bool IsPersistent
    {
        get => (bool)GetValue(IsPersistentProperty);
        set => SetValue(IsPersistentProperty, value);
    }

    /// <summary>A <see cref="CdsSideNavHeader"/> above the items.</summary>
    public View? Header
    {
        get => (View?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>A <see cref="CdsSideNavFooter"/> below the items.</summary>
    public View? Footer
    {
        get => (View?)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>The layout mode derived from <see cref="IsFixed"/> and <see cref="IsRail"/>.</summary>
    public CdsSideNavMode Mode => IsFixed ? CdsSideNavMode.Fixed : IsRail ? CdsSideNavMode.Rail : CdsSideNavMode.Default;

    /// <summary>The current (animating) width.</summary>
    public double CurrentWidth => _width;

    /// <summary>How far the page body is pushed by this side nav at the current breakpoint and width.</summary>
    public double BodyOffset => UIShellModel.BodyOffset(Mode, _watcher.Current, _width);

    /// <summary>Whether the full-screen scrim should show behind the side nav.</summary>
    public bool ShowsScrim => UIShellModel.ShowScrim(Mode, _watcher.Current, IsExpanded);

    /// <inheritdoc/>
    protected internal override bool WantsEscapeKey => IsRail || IsExpanded;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Escape || IsFixed) return false;
        var had = IsExpanded || _transient;
        _focusCount = 0;
        SetTransient(false);
        IsExpanded = false;
        return had;
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        _scroll.Content = newBody;
        UpdateReachability();
    }

    /// <inheritdoc/>
    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        if (IsRail && (previous & CdsInteractionState.Hover) != (State & CdsInteractionState.Hover))
            SetTransient(Is(CdsInteractionState.Hover) || _focusCount > 0);
    }

    /// <summary>A row inside gained or lost focus: a rail expands while focus is inside.</summary>
    internal void ItemFocusChanged(bool focused)
    {
        _focusCount = Math.Max(0, _focusCount + (focused ? 1 : -1));
        if (IsRail) SetTransient(Is(CdsInteractionState.Hover) || _focusCount > 0);
    }

    void SetTransient(bool value)
    {
        if (_transient == value) return;
        _transient = value;
        ApplyWidth(animate: true);
    }

    void Place(View? oldView, View? newView, int row)
    {
        if (oldView is not null) _panel.Remove(oldView);
        if (newView is not null) _panel.Add(newView, 0, row);
        if (newView is CdsSideNavFooter footer) footer.SetExpanded(IsExpanded || _width > UIShellModel.RailWidth);
    }

    void OnModeChanged()
    {
        if (Footer is CdsSideNavFooter footer) footer.SetExpanded(IsExpanded);
        ApplyWidth(animate: _initialized);
    }

    void ApplyWidth(bool animate)
    {
        if (_watcher is null) return;
        var target = UIShellModel.SideNavTargetWidth(Mode, _watcher.Current, IsExpanded, IsPersistent, _transient);
        // Already on its way there. A re-evaluation caused by the animation itself (its first frame runs synchronously, and
        // every frame resizes the nav) must not restart it: each restart's first frame would re-evaluate again, without end.
        if (target.Equals(_animatingTo)) return;
        if (!double.IsNaN(_animatingTo))
        {
            _widthRun++;
            _animatingTo = double.NaN;
            this.AbortAnimation(WidthAnimation);
        }
        if (Math.Abs(target - _width) < 0.1) return;
        if (!animate || Handler is null)
        {
            SetWidth(target);
            return;
        }
        _ = AnimateWidthAsync(target);
    }

    async Task AnimateWidthAsync(double target)
    {
        var run = ++_widthRun;
        _animatingTo = target;
        await CdsMotion.AnimateAsync(this, WidthAnimation, SetWidth, _width, target, CdsDuration.Fast02, CdsEasing.ExitProductive);
        if (run == _widthRun) _animatingTo = double.NaN;
    }

    void SetWidth(double width)
    {
        var wasReachable = UIShellModel.SideNavReachable(_width);
        _width = width;
        WidthRequest = width;
        _clip.IsVisible = width > 0.5;
        if (wasReachable != UIShellModel.SideNavReachable(width)) UpdateReachability();
        WidthChanged?.Invoke(this, EventArgs.Empty);
    }

    void UpdateReachability()
    {
        var reachable = UIShellModel.SideNavReachable(_width);
        foreach (var row in EnumerateRows(Body)) row.SetReachable(reachable);
    }

    static IEnumerable<CdsSideNavRow> EnumerateRows(Element? element)
    {
        switch (element)
        {
            case CdsSideNavRow row:
                yield return row;
                if (row is CdsSideNavMenu menu)
                    foreach (var child in menu.Items) yield return child;
                yield break;
            case CdsHeaderSideNavItems:
                yield break;
            case Layout layout:
                foreach (var child in layout.Children.OfType<Element>())
                    foreach (var row in EnumerateRows(child)) yield return row;
                yield break;
        }
    }
}
