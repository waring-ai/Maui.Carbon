namespace Maui.Carbon;

/// <summary>
/// The application frame, built from ordinary views (not MAUI Shell): a <see cref="Header"/> pinned to the top (48), a
/// <see cref="SideNav"/> under it, an optional <see cref="HeaderPanel"/> that slides in from the end edge, and the page
/// <see cref="CdsContainer.Body"/>. The shell applies Carbon's offsets: the body starts 48 below the top and is pushed by the
/// side nav (256 expanded, 48 as a rail) from the lg breakpoint (1056) up; below lg the side nav floats over the body and a
/// full-screen <c>overlay</c> scrim sits behind it (tap it to collapse). A tap outside an open header panel that holds a switcher
/// closes it.
/// <para>
/// The breakpoint comes from the page (<see cref="CdsContextKeys.Breakpoint"/>) unless <see cref="Breakpoint"/> forces one.
/// The shell fills the space its parent gives it: give it a height when it sits in a scrolling page. Navigation is yours:
/// links raise <see cref="CdsShellItem.Clicked"/> and set <see cref="CdsShellItem.IsActive"/>; swap <see cref="CdsContainer.Body"/> (or
/// call your navigation) in response.
/// </para>
/// </summary>
public class CdsUIShell : CdsContainer
{
    /// <summary>Bindable property for <see cref="Header"/>.</summary>
    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(
        nameof(Header), typeof(View), typeof(CdsUIShell), null,
        propertyChanged: static (b, o, n) => ((CdsUIShell)b).OnSlotChanged((View?)o, (View?)n, ((CdsUIShell)b)._headerHost));

    /// <summary>Bindable property for <see cref="SideNav"/>.</summary>
    public static readonly BindableProperty SideNavProperty = BindableProperty.Create(
        nameof(SideNav), typeof(View), typeof(CdsUIShell), null,
        propertyChanged: static (b, o, n) => ((CdsUIShell)b).OnSideNavChanged((View?)o, (View?)n));

    /// <summary>Bindable property for <see cref="HeaderPanel"/>.</summary>
    public static readonly BindableProperty HeaderPanelProperty = BindableProperty.Create(
        nameof(HeaderPanel), typeof(View), typeof(CdsUIShell), null,
        propertyChanged: static (b, o, n) => ((CdsUIShell)b).OnPanelChanged((View?)o, (View?)n));

    /// <summary>Bindable property for <see cref="Breakpoint"/>.</summary>
    public static readonly BindableProperty BreakpointProperty = BindableProperty.Create(
        nameof(Breakpoint), typeof(CdsBreakpoint?), typeof(CdsUIShell), null,
        propertyChanged: static (b, _, _) => ((CdsUIShell)b).Refresh());

    static readonly BindableProperty ContextBreakpointProperty = BindableProperty.Create(
        "ContextBreakpoint", typeof(CdsBreakpoint?), typeof(CdsUIShell), null,
        propertyChanged: static (b, _, _) => ((CdsUIShell)b).Refresh());

    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly ContentView _bodyHost = new() { Margin = new Thickness(0, UIShellModel.HeaderHeight, 0, 0), IsClippedToBounds = true };
    readonly BoxView _panelCatcher = new() { Margin = new Thickness(0, UIShellModel.HeaderHeight, 0, 0), IsVisible = false, Color = Colors.Transparent };
    readonly BoxView _scrim = new() { Margin = new Thickness(0, UIShellModel.HeaderHeight, 0, 0), IsVisible = false };
    readonly ContentView _sideHost = new() { Margin = new Thickness(0, UIShellModel.HeaderHeight, 0, 0), HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Fill };
    readonly ContentView _panelHost = new() { Margin = new Thickness(0, UIShellModel.HeaderHeight, 0, 0), HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Fill };
    readonly ContentView _headerHost = new() { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Fill };
    CdsSideNav? _sideNav;
    CdsHeaderPanel? _panel;

    /// <summary>Creates an empty shell.</summary>
    public CdsUIShell()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        SetDynamicResource(ContextBreakpointProperty, CdsContextKeys.Breakpoint);
        CdsTokens.SetColor(_scrim, BoxView.ColorProperty, CdsColorKeys.Overlay);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (_sideNav is not null) _sideNav.IsExpanded = false;
        };
        _scrim.GestureRecognizers.Add(tap);
        var catcher = new TapGestureRecognizer();
        catcher.Tapped += (_, _) =>
        {
            if (_panel is not null) _panel.IsExpanded = false;
        };
        _panelCatcher.GestureRecognizers.Add(catcher);
        AutomationProperties.SetIsInAccessibleTree(_scrim, false);
        AutomationProperties.SetIsInAccessibleTree(_panelCatcher, false);

        _root.Add(_bodyHost);
        _root.Add(_panelCatcher);
        _root.Add(_scrim);
        _root.Add(_sideHost);
        _root.Add(_panelHost);
        _root.Add(_headerHost);
        SetChrome(_root);
        SizeChanged += (_, _) => Refresh();
    }

    /// <summary>Raised when the breakpoint, the side nav state or its width changed (shell parts re-evaluate themselves).</summary>
    internal event EventHandler? ShellChanged;

    /// <summary>The <see cref="CdsHeader"/>: pinned to the top, 48 high.</summary>
    public View? Header
    {
        get => (View?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>The <see cref="CdsSideNav"/>, under the header at the start edge.</summary>
    public View? SideNav
    {
        get => (View?)GetValue(SideNavProperty);
        set => SetValue(SideNavProperty, value);
    }

    /// <summary>The <see cref="CdsHeaderPanel"/> that slides in from the end edge under the header.</summary>
    public View? HeaderPanel
    {
        get => (View?)GetValue(HeaderPanelProperty);
        set => SetValue(HeaderPanelProperty, value);
    }

    /// <summary>Forces a breakpoint instead of the page's (for frames and previews). Default <c>null</c>.</summary>
    public CdsBreakpoint? Breakpoint
    {
        get => (CdsBreakpoint?)GetValue(BreakpointProperty);
        set => SetValue(BreakpointProperty, value);
    }

    /// <summary>The breakpoint in effect: the forced one, else the page's, else the shell's own width.</summary>
    public CdsBreakpoint EffectiveBreakpoint =>
        GridModel.Effective(Breakpoint, (CdsBreakpoint?)GetValue(ContextBreakpointProperty), Width > 0 ? Width : CdsBreakpoints.Lg);

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => _bodyHost.Content = newBody;

    void OnSlotChanged(View? oldView, View? newView, ContentView host) => host.Content = newView;

    void OnSideNavChanged(View? oldView, View? newView)
    {
        if (_sideNav is not null)
        {
            _sideNav.WidthChanged -= OnSideNavWidthChanged;
            _sideNav.PropertyChanged -= OnSideNavPropertyChanged;
        }
        _sideNav = newView as CdsSideNav;
        _sideHost.Content = newView;
        if (_sideNav is not null)
        {
            _sideNav.WidthChanged += OnSideNavWidthChanged;
            _sideNav.PropertyChanged += OnSideNavPropertyChanged;
        }
        Refresh();
    }

    void OnPanelChanged(View? oldView, View? newView)
    {
        if (_panel is not null)
        {
            _panel.PropertyChanged -= OnPanelPropertyChanged;
        }
        _panel = newView as CdsHeaderPanel;
        _panelHost.Content = newView;
        if (_panel is not null) _panel.PropertyChanged += OnPanelPropertyChanged;
        UpdateCatcher();
    }

    void OnSideNavWidthChanged(object? sender, EventArgs e) => Refresh();

    void OnSideNavPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CdsSideNav.IsExpanded) or nameof(CdsSideNav.IsRail) or nameof(CdsSideNav.IsFixed))
            Refresh();
    }

    void OnPanelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CdsHeaderPanel.IsExpanded)) UpdateCatcher();
    }

    void UpdateCatcher() =>
        _panelCatcher.IsVisible = _panel is { IsExpanded: true } panel && panel.ClosesOnOutsideTapEffective;

    void Refresh()
    {
        if (_sideNav is { } nav)
        {
            _bodyHost.Margin = new Thickness(nav.BodyOffset, UIShellModel.HeaderHeight, 0, 0);
            _scrim.IsVisible = nav.ShowsScrim;
        }
        else
        {
            _bodyHost.Margin = new Thickness(0, UIShellModel.HeaderHeight, 0, 0);
            _scrim.IsVisible = false;
        }
        ShellChanged?.Invoke(this, EventArgs.Empty);
    }
}
