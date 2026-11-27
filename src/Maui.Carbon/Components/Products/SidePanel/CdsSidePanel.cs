using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// A full-height panel that slides in from the edge of the page so people can create, edit, view details or configure
/// something without leaving it (Carbon for IBM Products' <c>SidePanel</c>). Declare it in a page (it draws nothing
/// inline); it shows while <see cref="IsOpen"/> is true, under the UI shell header when there is one. It has a header
/// (<see cref="Label"/>, <see cref="Title"/>, <see cref="Subtitle"/>, an optional toolbar and the close button), a
/// scrolling body on the next layer and an action set of up to four buttons. Without <see cref="IncludeOverlay"/> it
/// slides over a page that stays usable; with it the page is dimmed and blocked; with <see cref="SlideIn"/> it pushes
/// <see cref="PageContent"/> aside. Use a modal for a short decision, a tearsheet for a long or complex task, and a side
/// panel for supporting tasks that need the page for context.
/// </summary>
[ContentProperty(nameof(Body))]
public partial class CdsSidePanel : CdsContainer
{
    readonly CdsItemCollection<CdsButton> _toolbarActions = [];
    readonly CdsActionSet _actionSet = new();
    CdsOverlay? _overlay;
    CdsOverlayHost? _host;
    bool _hiding;
    bool _openedOnLoad;
    double _pushed;

    // The surface, built on first open and reused.
    Grid? _surface;
    Grid? _header;
    VerticalStackLayout? _texts;
    CdsText? _label;
    Grid? _titleCell;
    CdsText? _title;
    CdsText? _collapsedTitle;
    CdsText? _subtitle;
    HorizontalStackLayout? _toolbar;
    HorizontalStackLayout? _corner;
    ContentView? _decoratorHost;
    CdsIconButton? _close;
    CdsIconButton? _back;
    BoxView? _divider;
    ScrollView? _scroll;
    CdsLayer? _layer;
    VerticalStackLayout? _footer;
    BoxView? _edge;

    /// <summary>Creates a closed medium side panel at the end edge.</summary>
    public CdsSidePanel()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        WidthRequest = 0;
        HeightRequest = 0;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new ContentView());
        _toolbarActions.CollectionChanged += OnToolbarChanged;
        ((INotifyCollectionChanged)_actionSet.Actions).CollectionChanged += (_, _) => Reshape();
        Loaded += (_, _) =>
        {
            if (!IsOpen || _overlay is not null) return;
            _openedOnLoad = true;
            Show();
        };
        Unloaded += (_, _) => Hide();
    }

    /// <summary>
    /// Raised when the panel is asked to close: the close button, Escape (not for slide-in panels) or a tap on the
    /// overlay. Set <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to keep it open (for example to confirm
    /// discarding changes); otherwise <see cref="RequestCloseCommand"/> runs and <see cref="IsOpen"/> becomes false.
    /// </summary>
    public event EventHandler<CdsModalRequestCloseEventArgs>? RequestClose;

    /// <summary>Raised when the back button of a later step is activated. Move <see cref="CurrentStep"/> back here.</summary>
    public event EventHandler? NavigationBack;

    /// <summary>Raised after the panel has closed (Carbon: <c>onUnmount</c>), for clean-up such as resetting a form.</summary>
    public event EventHandler? Closed;

    /// <summary>
    /// The action buttons, in any order: up to four <see cref="CdsButton"/>s (three in panels up to 320 wide), one
    /// primary. They are sorted by kind (primary at the end, or on top when stacked) and sized by the panel.
    /// </summary>
    public IList<CdsButton> Actions => _actionSet.Actions;

    /// <summary>
    /// Buttons in a toolbar under the title (Carbon: <c>actionToolbarButtons</c>), usually small ghost buttons that act on
    /// the panel's content. Icon-only buttons (<see cref="CdsButton.IsIconOnly"/>) sit flush; a labelled one leads the row.
    /// </summary>
    public IList<CdsButton> ToolbarActions => _toolbarActions;

    /// <summary>True while the panel is on screen (for tests).</summary>
    internal bool IsShown => _overlay is not null;

    /// <summary>The shown surface (for tests).</summary>
    internal Grid? Surface => _surface;

    /// <summary>The action set at the bottom of the panel (for tests).</summary>
    internal CdsActionSet ActionSet => _actionSet;

    /// <summary>The close button (for tests).</summary>
    internal CdsIconButton? CloseButton => _close;

    /// <summary>The back button (for tests).</summary>
    internal CdsIconButton? BackButton => _back;

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        if (_layer is not null) _layer.Body = ComposeBody(newBody);
    }

    /// <summary>Wraps the body before it is shown (the create side panel adds its form title).</summary>
    private protected virtual View? ComposeBody(View? body) => body;

    /// <summary>Re-applies the body after a property that <see cref="ComposeBody"/> reads changed.</summary>
    private protected void RecomposeBody()
    {
        if (_layer is not null) _layer.Body = ComposeBody(Body);
    }

    /// <summary>The subtitle tone (the create side panel uses the secondary text color).</summary>
    private protected virtual CdsTextTone SubtitleTone => CdsTextTone.Primary;

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_surface is not null) _surface.BindingContext = BindingContext;
        foreach (var b in _actionSet.Actions) b.BindingContext ??= BindingContext;
    }

    void OnIsOpenChanged()
    {
        if (IsOpen)
        {
            if (IsLoaded) Show();
        }
        else
        {
            Hide();
        }
    }

    void Reshow()
    {
        if (_overlay is null) return;
        Hide();
        if (IsOpen) Show();
    }

    void OnStepChanged()
    {
        Sync();
        if (_scroll is not null) _ = _scroll.ScrollToAsync(0, 0, false);
    }

    void OnToolbarChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_toolbar is null) return;
        _toolbar.Children.Clear();
        foreach (var b in _toolbarActions)
        {
            b.Size = CdsSize.Sm;
            b.Margin = b.IsIconOnly ? 0 : new Thickness(0, 0, CdsSpacing.Spacing03, 0);
            _toolbar.Children.Add(b);
        }
        Reshape();
    }

    void EnsureSurface()
    {
        if (_surface is not null) return;

        _label = new CdsText { TypeStyle = CdsTypeStyle.Label01, LineBreakMode = LineBreakMode.WordWrap };
        _title = new CdsText { TypeStyle = CdsTypeStyle.Heading03, LineBreakMode = LineBreakMode.WordWrap, VerticalOptions = LayoutOptions.Start };
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level2);
        _collapsedTitle = new CdsText { TypeStyle = CdsTypeStyle.HeadingCompact02, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Start, Opacity = 0, InputTransparent = true };
        AutomationProperties.SetExcludedWithChildren(_collapsedTitle, true);
        _titleCell = new Grid();
        _titleCell.Add(_title);
        _titleCell.Add(_collapsedTitle);
        _subtitle = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) };
        _toolbar = new HorizontalStackLayout { Spacing = 0 };
        _texts = new VerticalStackLayout { Spacing = 0 };
        _texts.Add(_label);
        _texts.Add(_titleCell);
        _texts.Add(_subtitle);
        _texts.Add(_toolbar);

        _close = new CdsIconButton { Icon = CdsIconName.Close, Kind = CdsIconButtonKind.Ghost, TooltipPlacement = CdsPlacement.Left };
        _close.Clicked += (_, _) => TryClose();
        _decoratorHost = new ContentView { VerticalOptions = LayoutOptions.Center, Margin = new Thickness(CdsSpacing.Spacing02) };
        _corner = new HorizontalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start };
        _corner.Add(_decoratorHost);
        _corner.Add(_close);

        _back = new CdsIconButton { Icon = CdsIconName.ArrowLeft, Kind = CdsIconButtonKind.Ghost, TooltipPlacement = CdsPlacement.Bottom, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
        _back.Clicked += (_, _) =>
        {
            NavigationBack?.Invoke(this, EventArgs.Empty);
            if (NavigationBackCommand?.CanExecute(null) == true) NavigationBackCommand.Execute(null);
        };

        _divider = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, Opacity = 0, InputTransparent = true };
        CdsTokens.SetColor(_divider, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);

        _header = new Grid();
        _header.Add(_texts);
        _header.Add(_back);
        _header.Add(_corner);
        _header.Add(_divider);
        CdsTokens.SetColor(_header, BackgroundColorProperty, CdsColorKeys.Layer01);

        _layer = new CdsLayer { Body = ComposeBody(Body) };
        _scroll = new ScrollView { Orientation = ScrollOrientation.Vertical, Content = _layer };
        _scroll.Scrolled += (_, _) => UpdateTitleProgress();
        _scroll.SizeChanged += (_, _) => UpdateTitleProgress();
        _layer.SizeChanged += (_, _) => UpdateTitleProgress();

        var rule = new BoxView { HeightRequest = 1 };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        _footer = new VerticalStackLayout { Spacing = 0 };
        _footer.Add(rule);
        _footer.Add(_actionSet);

        var panel = new Grid
        {
            RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto)],
        };
        panel.Add(_header, 0, 0);
        panel.Add(_scroll, 0, 1);
        panel.Add(_footer, 0, 2);
        CdsTokens.SetColor(panel, BackgroundColorProperty, CdsColorKeys.Layer01);

        _edge = new BoxView { WidthRequest = 1, InputTransparent = true };
        CdsTokens.SetColor(_edge, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);

        _surface = new Grid { BindingContext = BindingContext, VerticalOptions = LayoutOptions.Fill };
        _surface.Add(panel);
        _surface.Add(_edge);
        CdsTokens.SetColor(_surface, BackgroundColorProperty, CdsColorKeys.Layer01);
        OnToolbarChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>Copies the texts, labels and visibility of the parts from the properties.</summary>
    void Sync()
    {
        if (_surface is null) return;
        var hasTitle = !string.IsNullOrEmpty(Title);
        _label!.Text = Label;
        _label.IsVisible = hasTitle && !string.IsNullOrEmpty(Label);
        _title!.Text = Title;
        _collapsedTitle!.Text = Title;
        _titleCell!.IsVisible = hasTitle;
        _subtitle!.Text = Subtitle;
        _subtitle.IsVisible = !string.IsNullOrEmpty(Subtitle);
        _subtitle.Tone = SubtitleTone;
        _close!.Label = CloseButtonLabel;
        _close.IsVisible = !HideCloseButton;
        _back!.Label = BackButtonLabel;
        _back.IsVisible = CurrentStep > 0;
        CdsSemantics.SetGroup(_surface, Title, CdsSemanticGroupKind.Group);
        if (hasTitle == false && !string.IsNullOrEmpty(Label))
            System.Diagnostics.Debug.WriteLine("CdsSidePanel: Label is shown only with a Title.");
        Reshape();
    }

    /// <summary>Applies the measurements that depend on the size, the parts present and the viewport.</summary>
    void Reshape()
    {
        if (_surface is null) return;
        var hasToolbar = _toolbarActions.Count > 0;
        var hasDecorator = Decorator is not null;
        var hasActions = _actionSet.Actions.Count > 0;
        var hasTitle = !string.IsNullOrEmpty(Title);
        var buttonSize = SidePanelModel.HeaderButtonSize(Size, hasActions);
        var endPad = SidePanelModel.TitleEndPadding(hasToolbar, hasDecorator);

        _decoratorHost!.Content = Decorator;
        _decoratorHost.IsVisible = hasDecorator;
        _close!.Size = buttonSize;
        _back!.Size = buttonSize;
        _toolbar!.IsVisible = hasToolbar;
        _label!.Margin = new Thickness(0, 0, endPad, 0);
        _titleCell!.Margin = new Thickness(0, 0, endPad, SidePanelModel.TitleBottomPadding(_subtitle!.IsVisible));
        _subtitle.Padding = new Thickness(0, 0, endPad, 0);
        var top = SidePanelModel.HeaderTopPadding(_back.IsVisible, buttonSize);
        _texts!.Margin = new Thickness(CdsSpacing.Spacing05, top, CdsSpacing.Spacing05, 0);
        _texts.IsVisible = hasTitle || hasToolbar;
        _header!.MinimumHeightRequest = hasTitle ? 0 : CdsSpacing.Spacing08 + (_back.IsVisible ? CdsSpacing.Spacing02 : 0);
        _layer!.Padding = new Thickness(CdsSpacing.Spacing05, SidePanelModel.BodyTopPadding(hasToolbar, AnimateTitle), CdsSpacing.Spacing05, CdsSpacing.Spacing05);

        _footer!.IsVisible = hasActions;
        _actionSet.Size = SidePanelModel.ActionSetSize(Size);
        _actionSet.ButtonSize = CondensedActions ? CdsSize.Lg : CdsSize.Xl;

        var start = Placement == CdsSidePanelPlacement.Start;
        _edge!.HorizontalOptions = start ? LayoutOptions.End : LayoutOptions.Start;
        // A panel that floats over a usable page casts a small shadow; with an overlay, or pushing the page, it has none.
        CdsTokens.SetShadow(_surface, IncludeOverlay || SlideIn ? null : CdsColorKeys.Overlay, 2, 4);
        ApplyViewport();
        UpdateTitleProgress();
    }

    void ApplyViewport()
    {
        if (_surface is null) return;
        var viewport = _host?.Width ?? 0;
        var width = SidePanelModel.Width(Size, viewport);
        _surface.WidthRequest = width;
        // Under the UI shell header (Carbon's side panel starts 3rem from the top).
        var top = CdsShellContext.FindShell(this) is not null ? UIShellModel.HeaderHeight : 0;
        _surface.Margin = new Thickness(0, top, 0, 0);
        if (_overlay is not null && SlideIn) Push(width, animate: false);
    }

    void OnHostSizeChanged(object? sender, EventArgs e) => ApplyViewport();

    void UpdateTitleProgress()
    {
        if (_scroll is null || _divider is null) return;
        var hasTitle = !string.IsNullOrEmpty(Title);
        var reduced = CdsMotion.IsReducedMotion;
        var labelHeight = _label!.IsVisible ? _label.Height : 0;
        var subtitleHeight = _subtitle!.IsVisible ? _subtitle.Height + _subtitle.Margin.Bottom : 0;
        var distance = Math.Max(0, labelHeight) + Math.Max(0, subtitleHeight);
        var collapsed = -_label.Margin.Top - _subtitle.Margin.Top;
        var range = _scroll.ContentSize.Height - _scroll.Height + collapsed;
        var animates = !reduced && SidePanelModel.CanAnimateTitle(AnimateTitle, hasTitle,
            _label.IsVisible || _subtitle.IsVisible || _toolbar!.IsVisible, range, distance);
        var p = animates ? SidePanelModel.Progress(_scroll.ScrollY, distance) : 0;

        var end = _label.Margin.Right;
        _label.Opacity = 1 - p;
        _label.Margin = new Thickness(0, -labelHeight * p, end, 0);
        _subtitle.Opacity = 1 - p;
        _subtitle.Margin = new Thickness(0, -(_subtitle.Height + CdsSpacing.Spacing05) * p, 0, CdsSpacing.Spacing05);
        _title!.Opacity = 1 - p;
        _collapsedTitle!.IsVisible = animates;
        _collapsedTitle.Opacity = p;
        var noLabel = !_label.IsVisible;
        _title.TranslationY = noLabel ? -CdsSpacing.Spacing05 * p : 0;
        _collapsedTitle.TranslationY = noLabel ? CdsSpacing.Spacing05 * (1 - p) : 0;
        // The divider fades in as the title collapses; a header that never animates keeps it.
        _divider.Opacity = hasTitle && (!AnimateTitle || reduced) ? 1 : p;
    }

    VisualElement? ResolveFocus()
    {
        if (InitialFocusElement is { IsEnabled: true, IsVisible: true } explicitTarget) return explicitTarget;
        if (_back is { IsVisible: true }) return _back;
        if (_close is { IsVisible: true }) return _close;
        foreach (var b in _toolbarActions) if (b.IsEnabled && b.IsVisible) return b;
        if (FindFocusable(Body) is { } field) return field;
        foreach (var b in _actionSet.Shown) if (b.IsEnabled && b.IsVisible) return b;
        return null;
    }

    static VisualElement? FindFocusable(Element? element) => element switch
    {
        null => null,
        CdsControl { IsEnabled: true, IsVisible: true } c when c.AccessibilityTarget is not null => c,
        InputView { IsEnabled: true, IsVisible: true } input => input,
        VisualElement { IsVisible: false } => null,
        IVisualTreeElement tree => tree.GetVisualChildren().Select(c => FindFocusable(c as Element)).FirstOrDefault(f => f is not null),
    };

    internal void Show()
    {
        if (_overlay is not null) return;
        EnsureSurface();
        _host = CdsOverlayHost.Find(this) ?? CdsOverlayService.FindCurrentHost();
        if (_host is not null) _host.SizeChanged += OnHostSizeChanged;
        Sync();

        var focus = SlideIn || _openedOnLoad ? null : ResolveFocus();
        _openedOnLoad = false;
        var surface = _surface!;
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            IsModal = IncludeOverlay && !SlideIn,
            Dock = Placement == CdsSidePanelPlacement.Start ? CdsOverlayDock.Start : CdsOverlayDock.End,
            LightDismiss = false,
            CloseOnEscape = false,
            ReturnFocus = !SlideIn || LauncherElement is not null,
            ReturnFocusTo = LauncherElement,
            InitialFocus = focus,
            ScrimTapped = () => { if (!PreventCloseOnClickOutside) TryClose(); },
            KeyHandler = SlideIn ? null : e =>
            {
                if (e.Key != CdsKey.Escape) return false;
                TryClose();
                return true;
            },
        }, this);
        _overlay.Closed += OnOverlayClosed;
        if (focus is not null)
        {
            // The target may not have a native view yet when the overlay attaches: focus again once it is loaded.
            void Refocus(object? s, EventArgs e)
            {
                surface.Loaded -= Refocus;
                focus.Focus();
            }
            surface.Loaded += Refocus;
        }
        if (SlideIn) Push(surface.WidthRequest, animate: true);
    }

    internal void Hide()
    {
        if (_overlay is not { } overlay) return;
        _hiding = true;
        overlay.Close();
        _hiding = false;
    }

    void OnOverlayClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnOverlayClosed;
        _overlay = null;
        if (_host is not null) _host.SizeChanged -= OnHostSizeChanged;
        _host = null;
        if (_pushed != 0) Push(0, animate: true);
        if (!_hiding && IsOpen) IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Narrows (or restores) <see cref="PageContent"/> by the panel width on the panel's side.</summary>
    void Push(double amount, bool animate)
    {
        if (PageContent is not { } page) return;
        var from = _pushed;
        _pushed = amount;
        void Apply(double v)
        {
            var m = page.Margin;
            page.Margin = Placement == CdsSidePanelPlacement.Start
                ? new Thickness(v, m.Top, m.Right, m.Bottom)
                : new Thickness(m.Left, m.Top, v, m.Bottom);
        }
        if (!animate || CdsMotion.IsReducedMotion || page.Handler is null)
        {
            Apply(amount);
            return;
        }
        _ = CdsMotion.AnimateAsync(page, "CdsSidePanelPush", Apply, from, amount, CdsDuration.Moderate02,
            amount > from ? CdsEasing.EntranceProductive : CdsEasing.ExitProductive);
    }

    /// <summary>
    /// Asks to close from the chrome (close button, Escape, overlay): refused when a <see cref="RequestClose"/> handler
    /// cancels; otherwise runs <see cref="RequestCloseCommand"/> and closes.
    /// </summary>
    internal bool TryClose()
    {
        var args = new CdsModalRequestCloseEventArgs();
        RequestClose?.Invoke(this, args);
        if (args.Cancel) return false;
        if (RequestCloseCommand?.CanExecute(null) == true) RequestCloseCommand.Execute(null);
        IsOpen = false;
        return true;
    }
}
