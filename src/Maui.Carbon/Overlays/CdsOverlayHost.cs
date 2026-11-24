namespace Maui.Carbon;

/// <summary>
/// The per-page host for Carbon overlays (popovers, menus, tooltips, modals, toasts) and the page's keyboard router.
/// Every ContentPage gets one through the implicit Carbon page style; when that is turned off
/// (<see cref="CdsOptions.InjectPageOverlayHost"/>), make a <see cref="CdsOverlayHost"/> the root of each page and put
/// the page content in <see cref="Body"/>. It also publishes the page's <see cref="CdsBreakpoint"/>.
/// </summary>
[ContentProperty(nameof(Body))]
public partial class CdsOverlayHost : Grid
{
    /// <summary>The page template used by the implicit page style: a host around the page's content.</summary>
    public static ControlTemplate PageTemplate { get; } = new(() => new CdsOverlayHost(new ContentPresenter()));

    readonly View _content;
    readonly ContentView _dismissLayer;
    readonly AbsoluteLayout _popupLayer;
    readonly Grid _modalLayer;
    readonly BoxView _scrim;
    readonly VerticalStackLayout _toastLayer;
    readonly List<CdsOverlay> _overlays = [];
    readonly CdsScopeView _breakpointScope;

    /// <summary>Creates a host. Put the page content in <see cref="Body"/>.</summary>
    public CdsOverlayHost() : this(new ContentView())
    {
    }

    internal CdsOverlayHost(View content)
    {
        _content = content;
        _breakpointScope = new CdsScopeView { Content = content };

        _dismissLayer = new ContentView { IsVisible = false, BackgroundColor = Colors.Transparent };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => DismissTop();
        _dismissLayer.GestureRecognizers.Add(tap);

        _popupLayer = new AbsoluteLayout { InputTransparent = true, CascadeInputTransparent = false };

        _scrim = new BoxView();
        _scrim.SetDynamicResource(BoxView.ColorProperty, CdsColorKeys.Overlay);
        _modalLayer = new Grid { IsVisible = false };
        _modalLayer.Children.Add(_scrim);

        _toastLayer = new VerticalStackLayout
        {
            Spacing = CdsSpacing.Spacing03,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing09, CdsSpacing.Spacing05, 0),
            InputTransparent = true,
            CascadeInputTransparent = false,
        };

        Children.Add(_breakpointScope);
        Children.Add(_dismissLayer);
        Children.Add(_popupLayer);
        Children.Add(_modalLayer);
        Children.Add(_toastLayer);

        SizeChanged += OnHostSizeChanged;
    }

    /// <summary>The page content (when the host is used explicitly).</summary>
    public View? Body
    {
        get => (_content as ContentView)?.Content;
        set { if (_content is ContentView cv) cv.Content = value; }
    }

    /// <summary>The current breakpoint of this page.</summary>
    public CdsBreakpoint Breakpoint { get; private set; } = CdsBreakpoint.Lg;

    /// <summary>Open overlays, bottom to top.</summary>
    public IReadOnlyList<CdsOverlay> Overlays => _overlays;

    /// <summary>The overlay on top, if any.</summary>
    public CdsOverlay? TopOverlay => _overlays.Count > 0 ? _overlays[^1] : null;

    /// <summary>
    /// The overlay keys go to: the topmost one that takes keys (<see cref="CdsOverlay.TakesKeys"/>). A toast that opens
    /// while a modal or menu is open sits on top but never takes Escape or the arrows from it.
    /// </summary>
    internal CdsOverlay? KeyOverlay
    {
        get
        {
            for (var i = _overlays.Count - 1; i >= 0; i--)
                if (_overlays[i].TakesKeys) return _overlays[i];
            return null;
        }
    }

    /// <summary>The easing of the modal and dialog entrance (Carbon: <c>ease-entrance-expressive</c>, 240ms, 24px slide down).</summary>
    internal static Easing EntranceEasing => CdsEasing.EntranceExpressive;

    /// <summary>The easing of the modal and dialog exit (Carbon: <c>ease-exit-expressive</c>, 240ms, back up 24px).</summary>
    internal static Easing ExitEasing => CdsEasing.ExitExpressive;

    /// <summary>How far the modal and dialog container slides: it enters from 24px above its place and leaves the same way.</summary>
    internal const double SlideDistance = -24;

    /// <summary>Finds the host of the page that contains <paramref name="element"/>.</summary>
    public static CdsOverlayHost? Find(Element? element)
    {
        for (var e = element; e is not null; e = e.Parent)
        {
            if (e is CdsOverlayHost host) return host;
            if (e is ContentPage page) return FindInPage(page);
        }
        return null;
    }

    internal static CdsOverlayHost? FindInPage(ContentPage page)
    {
        foreach (var child in ((IElementController)page).LogicalChildren)
            if (child is CdsOverlayHost h) return h;
        return page.Content as CdsOverlayHost;
    }

    internal CdsOverlay Open(View content, CdsOverlayOptions options, Element? context = null)
    {
        var overlay = new CdsOverlay(this, content, options, context);
        _overlays.Add(overlay);

        overlay.Layer = options.IsModal ? CdsOverlayLayer.Modal : options.Anchor is null ? CdsOverlayLayer.Toast : CdsOverlayLayer.Popup;
        if (options.IsModal)
        {
            overlay.Container.HorizontalOptions = content.HorizontalOptions;
            overlay.Container.VerticalOptions = content.VerticalOptions;
            _modalLayer.Children.Add(overlay.Container);
            _modalLayer.IsVisible = true;
            _modalLayer.InputTransparent = false;
            SetPageBlocked(true);
            // A scrim still fading out after the last modal closed must not finish that fade under this one.
            if (_scrim.Handler is not null) _scrim.CancelAnimations();
            if (ShouldAnimate(overlay))
            {
                _scrim.Opacity = 0;
                _ = _scrim.FadeToAsync(1, CdsDuration.Moderate02, EntranceEasing);
                AnimateIn(overlay.Container);
            }
            else
            {
                _scrim.Opacity = 1;
            }
        }
        else if (options.Anchor is null)
        {
            // Carbon stacks toasts newest on top; a non-modal dialog docked here slides in like a modal one.
            _toastLayer.Children.Insert(0, overlay.Container);
            if (ShouldAnimate(overlay)) AnimateIn(overlay.Container);
        }
        else
        {
            AbsoluteLayout.SetLayoutBounds(overlay.Container, new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            _popupLayer.Children.Add(overlay.Container);
        }

        UpdateDismissLayer();
        overlay.Attach();
        OnKeyStateChanged();
        return overlay;
    }

    internal void Remove(CdsOverlay overlay)
    {
        _overlays.Remove(overlay);
        if (overlay.Layer == CdsOverlayLayer.Modal)
        {
            var last = !HasModal;
            // The page takes input and focus again at once; only the picture fades out.
            if (last) SetPageBlocked(false);
            if (ShouldAnimate(overlay))
            {
                if (last) _modalLayer.InputTransparent = true;
                _ = AnimateOutAsync(overlay.Container, last ? _scrim : null, () =>
                {
                    _modalLayer.Children.Remove(overlay.Container);
                    if (!HasModal) _modalLayer.IsVisible = false;
                });
            }
            else
            {
                _modalLayer.Children.Remove(overlay.Container);
                if (last) _modalLayer.IsVisible = false;
            }
        }
        else if (overlay.Layer == CdsOverlayLayer.Toast)
        {
            if (ShouldAnimate(overlay)) _ = AnimateOutAsync(overlay.Container, null, () => _toastLayer.Children.Remove(overlay.Container));
            else _toastLayer.Children.Remove(overlay.Container);
        }
        else
        {
            _popupLayer.Children.Remove(overlay.Container);
        }
        UpdateDismissLayer();
        OnKeyStateChanged();
    }

    /// <summary>Closes every open overlay.</summary>
    public void CloseAll()
    {
        foreach (var o in _overlays.ToArray()) o.Close();
    }

    /// <summary>
    /// Routes a key: the top overlay that takes keys first (toasts are skipped, so they never take Escape or the arrows
    /// from a modal or menu), then the focused Carbon control and its ancestors.
    /// </summary>
    public bool RouteKey(CdsKeyEventArgs e)
    {
        if (KeyOverlay is { } top)
        {
            if (top.HandleKey(e)) return true;
            if (top.Options.IsModal) return false;
        }
        foreach (var control in CdsFocusTracker.FocusChain())
        {
            if (control.OnKeyDown(e)) return true;
        }
        return false;
    }

    /// <summary>
    /// Routes a printable character the platform did not give to a text input (type-ahead) to the focused Carbon control
    /// and its ancestors (<see cref="CdsControl.OnCharacter"/>); returns true when one consumed it. Control characters and
    /// white space (Space activates) are not routed.
    /// </summary>
    public bool RouteCharacter(char character)
    {
        if (char.IsControl(character) || char.IsWhiteSpace(character)) return false;
        foreach (var control in CdsFocusTracker.FocusChain())
        {
            if (control.OnCharacter(character)) return true;
        }
        return false;
    }

    /// <summary>Keys the platform layer should capture right now.</summary>
    internal CdsWantedKeys WantedKeys
    {
        get
        {
            if (KeyOverlay is { } top)
            {
                var k = top.Options.Keys;
                var full = k.HasFlag(CdsOverlayKeys.Navigation);
                return new(true, full || k.HasFlag(CdsOverlayKeys.VerticalNavigation), full, k.HasFlag(CdsOverlayKeys.Activation));
            }
            var chain = CdsFocusTracker.FocusChain().ToList();
            var nav = chain.Any(c => c.WantsNavigationKeys);
            // While a toast is shown Escape stays captured, so a focused toast still closes on Escape.
            return new(_overlays.Count > 0 || chain.Any(c => c.WantsEscapeKey), nav, nav, false);
        }
    }

    /// <summary>Raised when the set of wanted keys may have changed (platform layer re-queries).</summary>
    internal event EventHandler? KeyStateChanged;

    internal void OnKeyStateChanged() => KeyStateChanged?.Invoke(this, EventArgs.Empty);

    bool HasModal => _overlays.Any(o => o.Layer == CdsOverlayLayer.Modal);

    static bool ShouldAnimate(CdsOverlay overlay) =>
        overlay.Options.Animate && !CdsMotion.IsReducedMotion && overlay.Container.Handler is not null;

    static void AnimateIn(View container)
    {
        container.Opacity = 0;
        container.TranslationY = SlideDistance;
        _ = container.FadeToAsync(1, CdsDuration.Moderate02, EntranceEasing);
        _ = container.TranslateToAsync(0, 0, CdsDuration.Moderate02, EntranceEasing);
    }

    static async Task AnimateOutAsync(View container, View? scrim, Action remove)
    {
        // Already out of the overlay list: it must not take taps or be read while it fades.
        container.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(container, true);
        var fades = new List<Task>
        {
            container.FadeToAsync(0, CdsDuration.Moderate02, ExitEasing),
            container.TranslateToAsync(0, SlideDistance, CdsDuration.Moderate02, ExitEasing),
        };
        if (scrim is not null) fades.Add(scrim.FadeToAsync(0, CdsDuration.Moderate02, ExitEasing));
        try
        {
            await Task.WhenAll(fades);
        }
        finally
        {
            remove();
        }
    }

    void DismissTop()
    {
        for (var i = _overlays.Count - 1; i >= 0; i--)
        {
            var o = _overlays[i];
            if (o.Options.IsModal) return;
            if (o.Options.LightDismiss)
            {
                o.Close();
                return;
            }
        }
    }

    void UpdateDismissLayer() =>
        _dismissLayer.IsVisible = _overlays.Any(o => o.Options.LightDismiss && o.Layer == CdsOverlayLayer.Popup);

    void SetPageBlocked(bool blocked)
    {
        _breakpointScope.InputTransparent = blocked;
        AutomationProperties.SetExcludedWithChildren(_breakpointScope, blocked);
    }

    void OnHostSizeChanged(object? sender, EventArgs e)
    {
        var bp = CdsBreakpointExtensions.FromWidth(Width);
        _breakpointScope.Resources[CdsContextKeys.ViewportWidth] = Width;
        _toastLayer.Resources[CdsContextKeys.ViewportWidth] = Width; // toasts are sized by the viewport too
        if (bp != Breakpoint || !_breakpointScope.Resources.ContainsKey(CdsContextKeys.Breakpoint))
        {
            Breakpoint = bp;
            _breakpointScope.Resources[CdsContextKeys.Breakpoint] = bp;
        }
        foreach (var o in _overlays) o.Reposition();
    }
}

/// <summary>The host layer an overlay lives in.</summary>
internal enum CdsOverlayLayer
{
    Popup,
    Modal,
    Toast,
}

/// <summary>Keys the platform layer should capture right now.</summary>
internal readonly record struct CdsWantedKeys(bool Escape, bool Vertical, bool Horizontal, bool Activation);
