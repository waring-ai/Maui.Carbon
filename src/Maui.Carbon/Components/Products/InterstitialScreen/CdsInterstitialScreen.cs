using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>Event data of <see cref="CdsInterstitialScreen.ActionRequested"/> and <see cref="CdsInterstitialScreen.ActionHandler"/>.</summary>
public sealed class CdsInterstitialScreenActionEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsInterstitialScreenActionEventArgs(CdsInterstitialScreenAction action, int step, int stepCount)
    {
        Action = action;
        Step = step;
        StepCount = stepCount;
    }

    /// <summary>The footer button that was activated (skip, back, next or start).</summary>
    public CdsInterstitialScreenAction Action { get; }

    /// <summary>The zero-based step shown when the button was activated.</summary>
    public int Step { get; }

    /// <summary>The number of steps.</summary>
    public int StepCount { get; }

    /// <summary>
    /// True to stop the default that follows (moving a step for back and next, closing for skip and start), as Carbon's
    /// <c>onAction</c> returning <c>true</c>. Go to another step with <see cref="CdsInterstitialScreen.CurrentStep"/> instead.
    /// </summary>
    public bool Cancel { get; set; }
}

/// <summary>Event data of <see cref="CdsInterstitialScreen.RequestClose"/>: set <see cref="CancelEventArgs.Cancel"/> to keep it open.</summary>
public sealed class CdsInterstitialScreenCloseEventArgs : CancelEventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsInterstitialScreenCloseEventArgs(CdsInterstitialScreenAction action) => Action = action;

    /// <summary>Why the screen closes: <see cref="CdsInterstitialScreenAction.Close"/> (close button or Escape), skip or start.</summary>
    public CdsInterstitialScreenAction Action { get; }
}

/// <summary>
/// An onboarding screen shown the first time someone reaches a new experience (Carbon for IBM Products'
/// <c>InterstitialScreen</c>): after the first login, or when a newly bought capability opens. A header (title, subtitle,
/// optional content and, for several steps, a progress indicator), the <see cref="Steps"/> one at a time, and a footer
/// with skip, back, next and start. It is a large modal over a scrim by default, or covers the whole page with
/// <see cref="IsFullScreen"/>; declare it anywhere in the page (it draws nothing there) and set <see cref="IsOpen"/>.
/// With <see cref="IsInline"/> it is drawn where it is declared instead (a first-run pane inside your own frame). Escape
/// and the modal's close button close it; a tap on the scrim does not. Show it once, never as a gate the person must
/// pass to reach work they came for, and keep it to a few steps.
/// </summary>
[ContentProperty(nameof(Steps))]
public partial class CdsInterstitialScreen : CdsControl
{
    /// <summary>The default accessible name (Carbon's <c>ariaLabel</c> default).</summary>
    public const string DefaultAccessibleName = "Interstitial screen";

    readonly CdsItemCollection<View> _steps = [];
    readonly ContentView _placeholder = new() { IsVisible = false };
    Screen? _screen;
    CdsOverlay? _overlay;
    CdsOverlayHost? _host;
    CdsInterstitialScreenAction? _running;
    bool _openedOnLoad;

    /// <summary>Creates a closed modal interstitial screen.</summary>
    public CdsInterstitialScreen()
    {
        _steps.CollectionChanged += OnStepsChanged;
        SetChrome(_placeholder);
        Loaded += (_, _) =>
        {
            if (!IsOpen || _screen is not null) return;
            _openedOnLoad = true;
            Show();
        };
        Unloaded += (_, _) => Hide();
    }

    /// <summary>
    /// Raised when a footer button is activated, before its default (back and next move a step, skip and start close).
    /// Set <see cref="CdsInterstitialScreenActionEventArgs.Cancel"/> to stop the default.
    /// </summary>
    public event EventHandler<CdsInterstitialScreenActionEventArgs>? ActionRequested;

    /// <summary>
    /// Raised when the screen is asked to close: the close button, Escape, skip or start. Set
    /// <see cref="CancelEventArgs.Cancel"/> to keep it open; otherwise <see cref="CloseCommand"/> runs (with the action as
    /// parameter), <see cref="IsOpen"/> becomes false and the screen goes back to the first step (Carbon's <c>onClose</c>).
    /// </summary>
    public event EventHandler<CdsInterstitialScreenCloseEventArgs>? RequestClose;

    /// <summary>Raised after <see cref="CurrentStep"/> changed.</summary>
    public event EventHandler<ValueChangedEventArgs>? CurrentStepChanged;

    /// <summary>
    /// The steps, in order: usually <see cref="CdsInterstitialScreenView"/>s (their titles label the progress indicator);
    /// any view works. One step shows only start; several add the progress indicator, skip, back and next.
    /// </summary>
    public IList<View> Steps => _steps;

    /// <summary>The number of steps.</summary>
    public int StepCount => _steps.Count;

    /// <summary>The step shown: <see cref="CurrentStep"/> kept inside the steps.</summary>
    public int ShownStep => InterstitialScreenModel.ClampStep(CurrentStep, _steps.Count);

    /// <summary>True while the screen is shown (in the overlay host, or inline).</summary>
    public bool IsShown => _screen is not null;

    /// <summary>The action whose <see cref="ActionHandler"/> is running, if any (its button shows an inline loading).</summary>
    public CdsInterstitialScreenAction? RunningAction => _running;

    string AccessibleName => SemanticProperties.GetDescription(this) is { Length: > 0 } name ? name : DefaultAccessibleName;

    /// <summary>The footer button of an action while shown (for tests).</summary>
    internal CdsInterstitialScreenButton? ButtonFor(CdsInterstitialScreenAction action) =>
        _screen is { } s && s.Buttons.TryGetValue(action, out var b) && b.Parent is not null ? b : null;

    /// <summary>The progress indicator while shown (for tests).</summary>
    internal CdsProgressIndicator? Progress => _screen?.Progress;

    /// <summary>The step deck while shown (for tests).</summary>
    internal CdsDeck? Deck => _screen?.Deck;

    /// <summary>The title label while shown (for tests).</summary>
    internal CdsText? TitleLabel => _screen?.Title;

    /// <summary>The close button of the modal variant while shown (for tests).</summary>
    internal VisualElement? CloseTarget => _screen?.Close?.HitTarget;

    /// <inheritdoc/>
    protected internal override bool WantsEscapeKey => IsInline && _screen is not null;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Escape || !IsInline || _screen is null) return false;
        TryClose(CdsInterstitialScreenAction.Close);
        return true;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        if (_screen is not { } s) return;
        SetColor(s.Header, BackgroundColorProperty, CdsInterstitialScreenTokens.HeaderBackground(HasTitle));
        SetColor(s.HeaderRule, BoxView.ColorProperty, CdsInterstitialScreenTokens.Divider);
        SetColor(s.Body, BackgroundColorProperty, CdsInterstitialScreenTokens.Background);
        SetColor(s.Footer, BackgroundColorProperty, CdsInterstitialScreenTokens.Background);
        SetColor(s.FooterRule, BoxView.ColorProperty, CdsInterstitialScreenTokens.Divider);
        if (s.Box is not null)
        {
            SetBrush(s.Box, Border.StrokeProperty, CdsInterstitialScreenTokens.ContainerBorder);
            SetColor(s.Box, BackgroundColorProperty, CdsInterstitialScreenTokens.ContainerFill);
        }
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_screen?.Surface is { } surface) surface.BindingContext = BindingContext;
    }

    bool HasTitle => !string.IsNullOrEmpty(HeaderTitle) || !string.IsNullOrEmpty(HeaderSubtitle) || HeaderContent is not null;

    // ----- open and close -----

    void OnIsOpenChanged(bool open)
    {
        if (open)
        {
            if (IsLoaded) Show();
            return;
        }
        Hide();
        if (CurrentStep != 0) CurrentStep = 0;
    }

    void Reshow()
    {
        if (_screen is null) return;
        Hide();
        if (IsOpen) Show();
    }

    /// <summary>Shows the screen (callers check that it is loaded and open).</summary>
    internal void Show()
    {
        if (_screen is not null) return;
        var modalLook = !IsFullScreen;
        var s = Build(modalLook, overlay: !IsInline);
        _screen = s;
        PopulateSteps(s);
        Sync(s);
        RefreshVisuals();

        var focus = _openedOnLoad ? null : InitialFocusTarget();
        _openedOnLoad = false;

        if (IsInline)
        {
            SetChrome(s.Root, fillsWidth: true);
            focus?.Focus();
            return;
        }

        s.Surface!.BindingContext = BindingContext;
        _host = CdsOverlayHost.Find(this) ?? CdsOverlayService.FindCurrentHost();
        if (_host is not null)
        {
            _host.SizeChanged += OnHostSizeChanged;
            ApplyViewport();
        }
        _overlay = CdsOverlayService.Open(s.Surface, new CdsOverlayOptions
        {
            IsModal = true,
            // Carbon: preventCloseOnClickOutside. The person leaves through the footer, the close button or Escape.
            LightDismiss = false,
            CloseOnEscape = false,
            ReturnFocusTo = LauncherElement,
            InitialFocus = focus,
            KeyHandler = e =>
            {
                if (e.Key != CdsKey.Escape) return false;
                TryClose(CdsInterstitialScreenAction.Close);
                return true;
            },
        }, this);
        _overlay.Closed += OnOverlayClosed;
        if (focus is not null)
        {
            // The target may have no native view yet when the overlay attaches: focus again once it is loaded.
            void Refocus(object? sender, EventArgs e)
            {
                s.Surface.Loaded -= Refocus;
                focus.Focus();
            }
            s.Surface.Loaded += Refocus;
        }
    }

    void Hide()
    {
        if (_screen is not { } s) return;
        _screen = null;
        UnhookHost();
        if (_overlay is { } overlay)
        {
            _overlay = null;
            overlay.Closed -= OnOverlayClosed;
            // Hand the views back once the overlay is gone (after its exit motion).
            overlay.Closed += (_, _) => ReleaseScreen(s);
            overlay.Close();
            return;
        }
        SetChrome(_placeholder);
        ReleaseScreen(s);
    }

    // Closed by the host rather than by Hide.
    void OnOverlayClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnOverlayClosed;
        if (!ReferenceEquals(sender, _overlay) || _screen is not { } s) return;
        _overlay = null;
        _screen = null;
        UnhookHost();
        ReleaseScreen(s);
        if (IsOpen) IsOpen = false;
    }

    void UnhookHost()
    {
        if (_host is not null) _host.SizeChanged -= OnHostSizeChanged;
        _host = null;
    }

    // Hands the app's views back so the screen can show them again; views already moved to a newer showing stay there.
    static void ReleaseScreen(Screen s)
    {
        s.Deck.Clear();
        s.HeaderContentHost.Content = null;
        s.FooterContentHost.Content = null;
        s.Panel.Release();
    }

    void OnHostSizeChanged(object? sender, EventArgs e) => ApplyViewport();

    void ApplyViewport()
    {
        if (_screen is not { Box: { } box } s || _host is not { Width: > 0, Height: > 0 } host) return;
        box.WidthRequest = CdsModalSizing.ModalWidth(CdsModalSize.Lg, host.Width);
        s.Panel.MaxPanelHeight = Math.Max(0, CdsModalSizing.ModalMaxHeight(CdsModalSize.Lg, host.Width, host.Height) - 2);
        box.VerticalOptions = host.Width < CdsModalSizing.BreakpointMd ? LayoutOptions.Start : LayoutOptions.Center;
    }

    /// <summary>
    /// Asks to close (close button, Escape, skip or start): refused when a <see cref="RequestClose"/> handler cancels;
    /// otherwise runs <see cref="CloseCommand"/> and closes.
    /// </summary>
    internal bool TryClose(CdsInterstitialScreenAction action)
    {
        var args = new CdsInterstitialScreenCloseEventArgs(action);
        RequestClose?.Invoke(this, args);
        if (args.Cancel) return false;
        if (CloseCommand?.CanExecute(action) == true) CloseCommand.Execute(action);
        IsOpen = false;
        if (CurrentStep != 0) CurrentStep = 0;
        return true;
    }

    // ----- actions and steps -----

    /// <summary>
    /// Runs a footer action as Carbon's footer does: <see cref="ActionRequested"/>, then <see cref="ActionHandler"/> (its
    /// button shows an inline loading meanwhile), then, unless cancelled, the move or the close.
    /// </summary>
    internal async Task RunActionAsync(CdsInterstitialScreenAction action)
    {
        if (_running is not null) return;
        var count = _steps.Count;
        var args = new CdsInterstitialScreenActionEventArgs(action, ShownStep, count);
        ActionRequested?.Invoke(this, args);
        if (ActionHandler is { } handler)
        {
            _running = action;
            SyncFooter();
            try
            {
                await handler(args);
            }
            finally
            {
                _running = null;
                SyncFooter();
            }
        }
        if (args.Cancel) return;
        var (step, close) = InterstitialScreenModel.Navigate(action, ShownStep, count);
        if (close)
        {
            TryClose(action);
            return;
        }
        CurrentStep = step;
        RestoreFocus(action);
    }

    void OnButtonClicked(object? sender, EventArgs e)
    {
        if (sender is CdsInterstitialScreenButton button) _ = RunActionAsync(button.Action);
    }

    // The button that moved the step may be gone (next on the last step, back on the first): focus the way forward.
    void RestoreFocus(CdsInterstitialScreenAction activated)
    {
        if (_screen is null || ButtonFor(activated) is not null) return;
        (ButtonFor(CdsInterstitialScreenAction.Next) ?? ButtonFor(CdsInterstitialScreenAction.Start))?.HitTarget.Focus();
    }

    VisualElement? InitialFocusTarget() =>
        (ButtonFor(CdsInterstitialScreenAction.Next) ?? ButtonFor(CdsInterstitialScreenAction.Start))?.HitTarget;

    void OnCurrentStepChanged(int oldValue, int newValue)
    {
        CurrentStepChanged?.Invoke(this, new ValueChangedEventArgs(oldValue, newValue));
        if (_screen is not { } s) return;
        var previous = s.Deck.Active;
        ShowStep(s, animateFrom: previous);
        SyncHeader(s);
        SyncProgress(s, rebuild: false);
        SyncFooter();
        if (InterstitialScreenModel.IsMultiStep(_steps.Count) && CdsDeck.IsShown(s.Root))
            CdsAnnouncer.Announce(InterstitialScreenModel.StepAnnouncement(ShownStep, _steps.Count));
    }

    void OnStepsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (var item in e.OldItems.OfType<CdsInterstitialScreenView>()) item.StepTitleChanged -= OnStepTitleChanged;
        if (e.NewItems is not null)
            foreach (var item in e.NewItems.OfType<CdsInterstitialScreenView>()) item.StepTitleChanged += OnStepTitleChanged;
        if (_screen is not { } s) return;
        PopulateSteps(s);
        Sync(s);
    }

    void OnStepTitleChanged(object? sender, EventArgs e)
    {
        if (_screen is { } s) SyncProgress(s, rebuild: true);
    }

    void OnPropertyAffectingScreen()
    {
        if (_screen is not { } s) return;
        Sync(s);
        RefreshVisuals();
    }
}

public partial class CdsInterstitialScreen
{
    /// <summary>The views of one showing of the screen (built on open, released on close).</summary>
    sealed class Screen
    {
        public required View Root;
        public required CdsModalPanel Panel;
        public Grid? Surface;
        public Border? Box;
        public required Grid Header;
        public required VerticalStackLayout Titles;
        public required CdsText Title;
        public required CdsText Subtitle;
        public required ContentView HeaderContentHost;
        public required ScrollView ProgressScroll;
        public required CdsProgressIndicator Progress;
        public required BoxView HeaderRule;
        public CdsModalCloseButton? Close;
        public required Grid Body;
        public required ScrollView Scroller;
        public required CdsDeck Deck;
        public required Grid Footer;
        public required BoxView FooterRule;
        public required CdsInterstitialFooterLayout FooterButtons;
        public required ContentView FooterContentHost;
        public readonly Dictionary<CdsInterstitialScreenAction, CdsInterstitialScreenButton> Buttons = [];
        public int ProgressCount = -1;
    }

    Screen Build(bool modalLook, bool overlay)
    {
        // Header: productive-heading-04 title (8 below), productive-heading-03 subtitle, 16 below the titles, then the
        // app's content and the progress indicator (12 below). 24 on top, 32 at the start (16 below md).
        var title = new CdsText { TypeStyle = CdsTypeStyle.Heading04, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
        var subtitle = new CdsText { TypeStyle = CdsTypeStyle.Heading03, LineBreakMode = LineBreakMode.WordWrap };
        SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level1);
        SemanticProperties.SetHeadingLevel(subtitle, SemanticHeadingLevel.Level2);
        var titles = new VerticalStackLayout { Spacing = 0, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) };
        titles.Add(title);
        titles.Add(subtitle);
        var headerContent = new ContentView { IsVisible = false };
        var progress = new CdsProgressIndicator();
        var progressScroll = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = progress,
            Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing04),
        };
        var headerStack = new VerticalStackLayout { Spacing = 0 };
        headerStack.Add(titles);
        headerStack.Add(headerContent);
        headerStack.Add(progressScroll);
        var headerLayout = new CdsInterstitialHeaderLayout { IsModal = modalLook };
        headerLayout.Add(headerStack);
        var headerRule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        AutomationProperties.SetExcludedWithChildren(headerRule, true);
        var header = new Grid();
        header.Add(headerLayout);
        header.Add(headerRule);
        CdsModalCloseButton? close = null;
        if (modalLook)
        {
            close = new CdsModalCloseButton();
            close.Clicked += (_, _) => TryClose(CdsInterstitialScreenAction.Close);
            header.Add(close);
            header.MinimumHeightRequest = CdsModalSizing.CloseButtonSize;
        }

        // Body: background fill; only the body scrolls, and only the current step is laid out.
        var deck = new CdsDeck();
        var scroller = new ScrollView { Orientation = ScrollOrientation.Vertical, HorizontalScrollBarVisibility = ScrollBarVisibility.Never, Content = deck };
        var body = new Grid { IsClippedToBounds = true };
        body.Add(scroller);

        // Footer: background fill and a 1px border-subtle-01 rule on top; 64px buttons.
        var footerRule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
        AutomationProperties.SetExcludedWithChildren(footerRule, true);
        var footerButtons = new CdsInterstitialFooterLayout { Margin = new Thickness(0, 1, 0, 0) };
        var footerContent = new ContentView { Margin = new Thickness(0, 1, 0, 0), IsVisible = false };
        var footer = new Grid();
        footer.Add(footerButtons);
        footer.Add(footerContent);
        footer.Add(footerRule);

        var panel = new CdsModalPanel();
        panel.SetParts(header, body, footer);

        Border? box = null;
        View root = panel;
        if (modalLook)
        {
            box = new Border { StrokeThickness = 1, StrokeShape = new Rectangle(), Padding = 0, Content = panel };
            root = box;
        }
        Grid? surface = null;
        if (overlay)
        {
            surface = new Grid { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
            if (box is not null)
            {
                // The scrim area around the container takes taps without closing (preventCloseOnClickOutside).
                var backdrop = new ContentView { BackgroundColor = Colors.Transparent };
                AutomationProperties.SetIsInAccessibleTree(backdrop, false);
                surface.Add(backdrop);
                box.HorizontalOptions = LayoutOptions.Center;
                box.VerticalOptions = LayoutOptions.Center;
            }
            surface.Add(root);
            CdsSemantics.SetGroupName(root, AccessibleName);
        }
        else if (string.IsNullOrEmpty(SemanticProperties.GetDescription(this)))
        {
            // Inline, a Description set on the control names the control itself (CdsControl forwards it as a group name).
            CdsSemantics.SetGroupName(root, AccessibleName);
        }

        return new Screen
        {
            Root = root,
            Panel = panel,
            Surface = surface,
            Box = box,
            Header = header,
            Titles = titles,
            Title = title,
            Subtitle = subtitle,
            HeaderContentHost = headerContent,
            ProgressScroll = progressScroll,
            Progress = progress,
            HeaderRule = headerRule,
            Close = close,
            Body = body,
            Scroller = scroller,
            Deck = deck,
            Footer = footer,
            FooterRule = footerRule,
            FooterButtons = footerButtons,
            FooterContentHost = footerContent,
        };
    }

    void PopulateSteps(Screen s)
    {
        for (var i = s.Deck.Count - 1; i >= 0; i--)
            if (s.Deck[i] is View v && !_steps.Contains(v)) s.Deck.RemoveAt(i);
        foreach (var step in _steps)
        {
            if (ReferenceEquals(step.Parent, s.Deck)) continue;
            if (step.Parent is Layout layout) layout.Remove(step);
            else if (step.Parent is ContentView cv && ReferenceEquals(cv.Content, step)) cv.Content = null;
            s.Deck.Add(step);
        }
        ShowStep(s, animateFrom: null);
        SyncProgress(s, rebuild: true);
    }

    void ShowStep(Screen s, View? animateFrom)
    {
        var next = _steps.Count == 0 ? null : _steps[ShownStep];
        if (ReferenceEquals(s.Deck.Active, next)) return;
        var forward = animateFrom is null || _steps.IndexOf(animateFrom) < ShownStep;
        s.Deck.Active = next;
        if (s.Scroller.Handler is not null) _ = s.Scroller.ScrollToAsync(0, 0, false);
        if (next is null || animateFrom is null || s.Body.Width <= 0) return;
        // Carbon slides the next step in like a carousel; under reduced motion it simply appears.
        var distance = s.Body.Width * (forward ? 1 : -1);
        next.TranslationX = distance;
        _ = CdsMotion.AnimateAsync(next, "CdsInterstitialStep", x => next.TranslationX = x, distance, 0,
            CdsDuration.Moderate02, CdsEasing.StandardProductive);
    }

    void Sync(Screen s)
    {
        SyncHeader(s);
        SyncProgress(s, rebuild: false);
        SyncFooter();
        var name = AccessibleName;
        if (s.Surface is not null || string.IsNullOrEmpty(SemanticProperties.GetDescription(this)))
            CdsSemantics.SetGroupName(s.Root, name);
    }

    void SyncHeader(Screen s)
    {
        s.Title.Text = HeaderTitle;
        s.Title.IsVisible = !string.IsNullOrEmpty(HeaderTitle);
        s.Subtitle.Text = HeaderSubtitle;
        s.Subtitle.IsVisible = !string.IsNullOrEmpty(HeaderSubtitle);
        s.Titles.IsVisible = s.Title.IsVisible || s.Subtitle.IsVisible;
        if (!ReferenceEquals(s.HeaderContentHost.Content, HeaderContent))
        {
            if (HeaderContent?.Parent is ContentView old && !ReferenceEquals(old, s.HeaderContentHost)) old.Content = null;
            s.HeaderContentHost.Content = HeaderContent;
        }
        s.HeaderContentHost.IsVisible = HeaderContent is not null;
        s.HeaderRule.IsVisible = HasTitle;
        if (s.Close is not null) s.Close.Description = string.IsNullOrEmpty(CloseIconDescription) ? "Close" : CloseIconDescription;
        var showsProgress = InterstitialScreenModel.ShowsProgress(_steps.Count, HideProgressIndicator);
        s.ProgressScroll.IsVisible = showsProgress;
        // Full screen with nothing to show: no header at all (Carbon's :empty header).
        s.Header.IsVisible = s.Close is not null || s.Titles.IsVisible || HeaderContent is not null || showsProgress;
    }

    void SyncProgress(Screen s, bool rebuild)
    {
        if (rebuild || s.ProgressCount != _steps.Count)
        {
            s.Progress.Steps.Clear();
            for (var i = 0; i < _steps.Count; i++)
                s.Progress.Steps.Add(new CdsProgressStep(InterstitialScreenModel.StepTitle((_steps[i] as CdsInterstitialScreenView)?.StepTitle, i)));
            s.ProgressCount = _steps.Count;
        }
        s.Progress.CurrentIndex = ShownStep;
        s.ProgressScroll.IsVisible = InterstitialScreenModel.ShowsProgress(_steps.Count, HideProgressIndicator);
    }

    void SyncFooter()
    {
        if (_screen is not { } s) return;
        if (FooterContent is not null)
        {
            if (!ReferenceEquals(s.FooterContentHost.Content, FooterContent))
            {
                if (FooterContent.Parent is ContentView old && !ReferenceEquals(old, s.FooterContentHost)) old.Content = null;
                s.FooterContentHost.Content = FooterContent;
            }
            s.FooterContentHost.IsVisible = true;
            s.FooterButtons.IsVisible = false;
            return;
        }
        s.FooterContentHost.Content = null;
        s.FooterContentHost.IsVisible = false;
        s.FooterButtons.IsVisible = true;

        var footer = InterstitialScreenModel.Footer(ShownStep, _steps.Count, !string.IsNullOrEmpty(SkipButtonLabel), _running);
        var shown = new List<CdsInterstitialScreenButton>();
        foreach (var action in footer.Actions)
        {
            if (!s.Buttons.TryGetValue(action, out var button))
            {
                button = new CdsInterstitialScreenButton(action);
                button.Clicked += OnButtonClicked;
                s.Buttons[action] = button;
            }
            button.Text = LabelOf(action);
            button.HasIcon = InterstitialScreenModel.HasIcon(action, footer, _running);
            button.IsLoading = _running == action;
            button.IsEnabled = !DisabledButtons.HasFlag(InterstitialScreenModel.ButtonOf(action));
            shown.Add(button);
        }
        s.FooterButtons.SetButtons(shown, footer);
    }

    string? LabelOf(CdsInterstitialScreenAction action) => action switch
    {
        CdsInterstitialScreenAction.Skip => SkipButtonLabel,
        CdsInterstitialScreenAction.Back => PreviousButtonLabel,
        CdsInterstitialScreenAction.Next => NextButtonLabel,
        CdsInterstitialScreenAction.Start => StartButtonLabel,
        _ => CloseIconDescription,
    };
}
