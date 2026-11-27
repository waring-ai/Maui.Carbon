namespace Maui.Carbon;

/// <summary>An open overlay. Close it with <see cref="Close"/>; it also closes on light dismiss and Escape.</summary>
public sealed class CdsOverlay
{
    readonly CdsOverlayHost _host;
    readonly List<ScrollView> _scrollers = [];
    readonly CdsControl? _contextSource;
    bool _positioned;

    internal CdsOverlay(CdsOverlayHost host, View content, CdsOverlayOptions options, Element? context = null)
    {
        _host = host;
        Content = content;
        Options = options;
        Container = new CdsScopeView { Content = content, InputTransparent = options.InputTransparent };
        _contextElement = options.Anchor ?? context;
        _contextSource = FindContextSource(_contextElement);
        // A modal or dialog with no launcher returns focus to whatever was focused before it opened (Carbon's
        // "focus returns to launcherButtonRef or to the element focused before opening"). Captured before the overlay
        // moves focus into itself.
        if (options is { ReturnFocus: true, ReturnFocusTo: null, Anchor: null } && CdsFocusTracker.Focused is { } before)
            _focusedBefore = new WeakReference<VisualElement>(before);
        ApplyContext();
    }

    readonly Element? _contextElement;
    readonly WeakReference<VisualElement>? _focusedBefore;

    /// <summary>The overlay's content.</summary>
    public View Content { get; }

    /// <summary>The options it was opened with.</summary>
    public CdsOverlayOptions Options { get; }

    /// <summary>True until the overlay closes.</summary>
    public bool IsOpen { get; private set; } = true;

    /// <summary>The placement actually used (after flipping).</summary>
    public CdsPlacement ActualPlacement { get; private set; }

    /// <summary>Distance from the popup's start edge to the anchor's center, for drawing a caret.</summary>
    public double CaretOffset { get; private set; }

    /// <summary>Raised after the overlay closes.</summary>
    public event EventHandler? Closed;

    /// <summary>Raised after the overlay is (re)positioned.</summary>
    public event EventHandler? Positioned;

    internal CdsScopeView Container { get; }

    internal CdsOverlayLayer Layer { get; set; }

    /// <summary>
    /// Whether the overlay takes part in key routing: modals always do, other overlays when they handle keys or close on
    /// Escape. Passive overlays (toasts) never take the keyboard from a modal or menu under them.
    /// </summary>
    internal bool TakesKeys =>
        Options.IsModal || Options.KeyHandler is not null || Options.CloseOnEscape || Options.Keys != CdsOverlayKeys.None;

    /// <summary>
    /// Where focus goes when the overlay closes: <see cref="CdsOverlayOptions.ReturnFocusTo"/>, else the anchor, else (for
    /// an anchorless overlay) the Carbon control that had focus before it opened, while that control is still on the page.
    /// </summary>
    internal VisualElement? ReturnFocusTarget
    {
        get
        {
            if (!Options.ReturnFocus) return null;
            if ((Options.ReturnFocusTo ?? Options.Anchor) is { } explicitTarget) return explicitTarget;
            if (_focusedBefore is null || !_focusedBefore.TryGetTarget(out var before)) return null;
            return IsStillShown(before) ? before : null;
        }
    }

    bool IsStillShown(VisualElement element)
    {
        // Gone when it was removed from the page, hidden, disabled, or lived in an overlay that has closed since
        // (a menu item that opened the modal), or in this overlay itself.
        for (Element? e = element; e is not null; e = e.Parent)
        {
            if (ReferenceEquals(e, Container)) return false;
            if (CdsDeck.IsHidden(e) || e is VisualElement { IsEnabled: false }) return false;
            if (ReferenceEquals(e, _host)) return true;
        }
        return false;
    }

    /// <summary>Closes the overlay.</summary>
    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Detach();
        _host.Remove(this);
        var focus = ReturnFocusTarget;
        if (focus is CdsControl control) control.Focus();
        else focus?.Focus();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Changes which keys the overlay captures while it is open (for example a date picker switching from typing in
    /// the field to moving through the calendar).
    /// </summary>
    public void SetKeys(CdsOverlayKeys keys)
    {
        if (Options.Keys == keys) return;
        Options.Keys = keys;
        _host.OnKeyStateChanged();
    }

    /// <summary>Recomputes the position next to the anchor (after the anchor moved or the content resized).</summary>
    public void Reposition()
    {
        if (!IsOpen || Options.Anchor is not { } anchor || Options.IsModal) return;
        var hostSize = new Size(_host.Width, _host.Height);
        if (hostSize.Width <= 0 || anchor.Width <= 0) return;

        var anchorBounds = CdsGeometry.GetBoundsRelativeTo(anchor, _host);
        var width = Options.MatchAnchorWidth ? anchorBounds.Width : Container.Width;
        var height = Container.Height;
        if (width <= 0 || height <= 0) return;

        var result = CdsPlacementResolver.Resolve(anchorBounds, new Size(width, height), hostSize, Options.Placement, Options.Offset, Options.AutoFlip);
        ActualPlacement = result.Placement;
        CaretOffset = result.CaretOffset;
        // An explicit width (the measured one) lets wrapping content re-measure its height under that constraint;
        // an auto-sized width measures it unconstrained, which clips multi-line text.
        AbsoluteLayout.SetLayoutBounds(Container, new Rect(result.Frame.X, result.Frame.Y,
            Options.MatchAnchorWidth ? anchorBounds.Width : Math.Ceiling(width), AbsoluteLayout.AutoSize));

        if (!_positioned)
        {
            _positioned = true;
            FadeIn();
        }
        Positioned?.Invoke(this, EventArgs.Empty);
    }

    void FadeIn()
    {
        if (!Options.Animate || CdsMotion.IsReducedMotion || Container.Handler is null)
        {
            Container.Opacity = 1;
            return;
        }
        Container.Opacity = 0;
        _ = CdsMotion.AnimateAsync(Container, "CdsOverlayFadeIn", v => Container.Opacity = v, 0, 1, CdsDuration.Fast02, CdsEasing.EntranceProductive);
        // Whatever happens to the animation, an open overlay must never stay invisible.
        Container.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(CdsDuration.Fast02 + 150), () =>
        {
            if (IsOpen) Container.Opacity = 1;
        });
    }

    internal void Attach()
    {
        if (Options.Anchor is { } anchor && !Options.IsModal)
        {
            Container.Opacity = 0;
            if (Options.MatchAnchorWidth) Container.WidthRequest = Math.Max(0, anchor.Width);
            Container.SizeChanged += OnLayoutChanged;
            anchor.SizeChanged += OnLayoutChanged;
            anchor.Unloaded += OnAnchorUnloaded;
            for (var e = anchor.Parent; e is not null; e = e.Parent)
            {
                if (e is ScrollView sv)
                {
                    sv.Scrolled += OnScrolled;
                    _scrollers.Add(sv);
                }
            }
        }
        if (_contextSource is not null) _contextSource.PropertyChanged += OnContextPropertyChanged;
        else CdsThemeManager.ThemeChanged += OnAppThemeChanged;
        Options.InitialFocus?.Focus();
    }

    void Detach()
    {
        Container.SizeChanged -= OnLayoutChanged;
        if (Options.Anchor is { } anchor)
        {
            anchor.SizeChanged -= OnLayoutChanged;
            anchor.Unloaded -= OnAnchorUnloaded;
        }
        foreach (var sv in _scrollers) sv.Scrolled -= OnScrolled;
        _scrollers.Clear();
        if (_contextSource is not null) _contextSource.PropertyChanged -= OnContextPropertyChanged;
        else CdsThemeManager.ThemeChanged -= OnAppThemeChanged;
    }

    void OnAppThemeChanged(object? sender, EventArgs e) => ApplyContext();

    internal bool HandleKey(CdsKeyEventArgs e)
    {
        if (Options.KeyHandler?.Invoke(e) == true) return true;
        if (e.Key == CdsKey.Escape && Options.CloseOnEscape)
        {
            Close();
            return true;
        }
        return false;
    }

    void OnLayoutChanged(object? sender, EventArgs e) => Reposition();

    void OnScrolled(object? sender, ScrolledEventArgs e) => Reposition();

    void OnAnchorUnloaded(object? sender, EventArgs e) => Close();

    void OnContextPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is "ContextTheme" or "ContextLevel") ApplyContext();
    }

    void ApplyContext()
    {
        // The popup lives in the page-root host, outside the anchor's theme zone and layers: mirror them.
        // With no anchor or context element it simply inherits the page (and so the live app theme).
        if (_contextElement is null)
        {
            Container.Resources = new ResourceDictionary();
            return;
        }
        var theme = _contextSource?.ContextTheme ?? CdsTheme.GetEffectiveTheme(_contextElement);
        var level = _contextSource?.ContextLayerLevel ?? CdsLayer.GetLevel(_contextElement);
        Container.Resources = CdsPalettes.CreateContextDictionary(theme, level);
    }

    static CdsControl? FindContextSource(Element? anchor)
    {
        for (var e = anchor; e is not null; e = e.Parent)
            if (e is CdsControl c) return c;
        return null;
    }
}
