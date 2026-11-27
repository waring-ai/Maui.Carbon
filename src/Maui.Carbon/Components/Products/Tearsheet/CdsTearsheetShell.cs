using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Maui.Carbon;

/// <summary>
/// The shared behavior of <see cref="CdsTearsheet"/> and <see cref="CdsTearsheetNarrow"/> (Carbon for IBM Products'
/// <c>TearsheetShell</c>): a tall modal surface that rises from the bottom of the page over the overlay scrim, with a
/// header, a scrolling body and an action set. Up to three tearsheets stack; the ones underneath shrink so their tops
/// show. Declare a tearsheet in a page (it draws nothing inline); it shows while <see cref="IsOpen"/> is true.
/// </summary>
[ContentProperty(nameof(Body))]
public abstract partial class CdsTearsheetShell : CdsContainer
{
    static readonly ConditionalWeakTable<CdsOverlayHost, List<CdsTearsheetShell>> s_stacks = [];

    readonly CdsActionSet _actionSet = new();
    CdsOverlay? _overlay;
    CdsOverlayHost? _host;
    bool _hiding;
    bool _openedOnLoad;
    int _levelsAbove;

    // The surface, built on first open and reused.
    Grid? _surface;
    Grid? _header;
    VerticalStackLayout? _fields;
    CdsText? _label;
    CdsText? _title;
    CdsText? _description;
    ContentView? _navigationHost;
    ContentView? _headerActionsHost;
    ContentView? _decoratorHost;
    CdsModalCloseButton? _close;
    Grid? _main;
    ScrollView? _scroll;
    ContentView? _contentHost;
    ContentView? _influencerStart;
    ContentView? _influencerEnd;
    VerticalStackLayout? _buttons;

    private protected CdsTearsheetShell()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        WidthRequest = 0;
        HeightRequest = 0;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new ContentView());
        ((INotifyCollectionChanged)_actionSet.Actions).CollectionChanged += (_, _) => Sync();
        Loaded += (_, _) =>
        {
            if (!IsOpen || _overlay is not null) return;
            _openedOnLoad = true;
            Show();
        };
        Unloaded += (_, _) => Hide();
    }

    /// <summary>
    /// Raised when the tearsheet is asked to close: the close button, Escape, or a tap outside a passive tearsheet. Set
    /// <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to keep it open (for example to confirm discarding
    /// changes); otherwise <see cref="RequestCloseCommand"/> runs and <see cref="IsOpen"/> becomes false.
    /// </summary>
    public event EventHandler<CdsModalRequestCloseEventArgs>? RequestClose;

    /// <summary>Raised after the tearsheet has closed.</summary>
    public event EventHandler? Closed;

    /// <summary>
    /// The action buttons, in any order: up to four <see cref="CdsButton"/>s, one primary. They are sorted by kind
    /// (primary at the end) and sized by the tearsheet. Without actions the tearsheet is passive.
    /// </summary>
    public IList<CdsButton> Actions => _actionSet.Actions;

    /// <summary>True for the wide variant.</summary>
    private protected abstract bool IsWide { get; }

    /// <summary>The wide variant's header actions, navigation and influencer (null for the narrow one).</summary>
    private protected virtual View? HeaderActionsView => null;

    private protected virtual View? NavigationView => null;

    private protected virtual View? InfluencerView => null;

    private protected virtual CdsTearsheetInfluencerPosition InfluencerSide => CdsTearsheetInfluencerPosition.Start;

    private protected virtual CdsTearsheetInfluencerWidth InfluencerSize => CdsTearsheetInfluencerWidth.Narrow;

    /// <summary>A fixed influencer width (the multi add-select sidebar is 360 wide), or null for <see cref="InfluencerSize"/>.</summary>
    private protected virtual double? InfluencerWidthOverride => null;

    /// <summary>Wraps the body before it is shown (the create tearsheets add their form title and steps).</summary>
    private protected virtual View? ComposeBody(View? body) => body;

    /// <summary>Called when the tearsheet is about to open (the create tearsheet resets to its first step).</summary>
    private protected virtual void OnOpening()
    {
    }

    /// <summary>True while the tearsheet is on screen (for tests).</summary>
    internal bool IsShown => _overlay is not null;

    /// <summary>The action set at the bottom (for tests).</summary>
    internal CdsActionSet ActionSet => _actionSet;

    /// <summary>The close button, once built (for tests).</summary>
    internal VisualElement? CloseButton => _close;

    /// <summary>The surface, once built (for tests).</summary>
    internal Grid? Surface => _surface;

    /// <summary>How many shown tearsheets are stacked over this one (for tests).</summary>
    internal int LevelsAbove => _levelsAbove;

    /// <summary>True when the close button is shown.</summary>
    internal bool EffectiveHasCloseIcon => TearsheetModel.HasCloseIcon(HasCloseIcon, _actionSet.Actions.Count);

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => Sync();

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_surface is not null) _surface.BindingContext = BindingContext;
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

    /// <summary>Re-applies the parts after a wide-only property changed.</summary>
    private protected void OnPartsChanged() => Sync();

    void EnsureSurface()
    {
        if (_surface is not null) return;
        var wide = IsWide;

        _label = new CdsText { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing02) };
        _title = new CdsText { TypeStyle = wide ? CdsTypeStyle.Heading04 : CdsTypeStyle.Heading03, LineBreakMode = LineBreakMode.WordWrap };
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level2);
        _description = new CdsText
        {
            TypeStyle = CdsTypeStyle.BodyCompact01,
            LineBreakMode = LineBreakMode.WordWrap,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, wide ? CdsSpacing.Spacing05 : CdsSpacing.Spacing03, 0, 0),
        };
        _navigationHost = new ContentView { Margin = new Thickness(0, CdsSpacing.Spacing04, 0, 0) };
        _fields = new VerticalStackLayout { Spacing = 0 };
        _fields.Add(_label);
        _fields.Add(_title);
        _fields.Add(_description);
        _fields.Add(_navigationHost);

        _headerActionsHost = new ContentView
        {
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, CdsSpacing.Spacing05, CdsSpacing.Spacing13, 0),
        };
        _decoratorHost = new ContentView
        {
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, CdsSpacing.Spacing03 + CdsSpacing.Spacing03, CdsSpacing.Spacing09 + CdsSpacing.Spacing05, 0),
        };
        _close = new CdsModalCloseButton { Margin = new Thickness(0, CdsSpacing.Spacing03, CdsSpacing.Spacing05, 0) };
        _close.Clicked += (_, _) => TryClose();

        var rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        _header = new Grid();
        _header.Add(_fields);
        _header.Add(_headerActionsHost);
        _header.Add(_decoratorHost);
        _header.Add(_close);
        _header.Add(rule);
        CdsTokens.SetColor(_header, BackgroundColorProperty, CdsColorKeys.Layer01);
        _header.SizeChanged += (_, _) => UpdateDescriptionWidth();

        _contentHost = wide ? new ContentView() : new CdsLayer();
        _scroll = new ScrollView { Orientation = ScrollOrientation.Vertical, Content = _contentHost };
        _influencerStart = BuildInfluencerHost(start: true);
        _influencerEnd = BuildInfluencerHost(start: false);

        _main = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)] };
        _main.Add(_scroll, 0);
        _main.Add(_influencerEnd, 1);
        CdsTokens.SetColor(_main, BackgroundColorProperty, wide ? CdsColorKeys.Background : CdsColorKeys.Layer01);

        var buttonRule = new BoxView { HeightRequest = 1 };
        CdsTokens.SetColor(buttonRule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        _actionSet.Size = wide ? CdsActionSetSize.Xxl : CdsActionSetSize.Lg;
        _actionSet.ButtonSize = wide ? CdsSize.Xxl : CdsSize.Xl;
        _actionSet.FillKey = wide ? CdsColorKeys.Background : CdsColorKeys.Layer01;
        _buttons = new VerticalStackLayout { Spacing = 0 };
        _buttons.Add(buttonRule);
        _buttons.Add(_actionSet);

        var right = new Grid { RowDefinitions = [new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto)] };
        right.Add(_main, 0, 0);
        right.Add(_buttons, 0, 1);

        var body = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)] };
        body.Add(_influencerStart, 0);
        body.Add(right, 1);

        _surface = new Grid
        {
            BindingContext = BindingContext,
            RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star)],
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            // A stacked tearsheet shrinks towards its top edge, which stays 16 above the one in front.
            AnchorY = 0,
        };
        _surface.Add(_header, 0, 0);
        _surface.Add(body, 0, 1);
        CdsTokens.SetColor(_surface, BackgroundColorProperty, CdsColorKeys.Layer01);
    }

    static ContentView BuildInfluencerHost(bool start)
    {
        var edge = new BoxView { WidthRequest = 1, HorizontalOptions = start ? LayoutOptions.End : LayoutOptions.Start, InputTransparent = true };
        CdsTokens.SetColor(edge, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        var layer = new CdsLayer();
        var scroll = new ScrollView { Orientation = ScrollOrientation.Vertical, Content = layer, Margin = start ? new Thickness(0, 0, 1, 0) : new Thickness(1, 0, 0, 0) };
        var grid = new Grid();
        grid.Add(scroll);
        grid.Add(edge);
        var host = new ContentView { Content = grid, IsVisible = false };
        if (!start) CdsTokens.SetColor(host, BackgroundColorProperty, CdsColorKeys.Layer01);
        return host;
    }

    static void SetInfluencer(ContentView host, View? content, double width)
    {
        var layer = (CdsLayer)((ScrollView)((Grid)host.Content).Children[0]).Content;
        if (!ReferenceEquals(layer.Body, content)) layer.Body = content;
        host.IsVisible = content is not null;
        host.WidthRequest = width;
    }

    void Sync()
    {
        if (_surface is null) return;
        var wide = IsWide;
        var hasClose = EffectiveHasCloseIcon;
        var hasDecorator = Decorator is not null;

        _label!.Text = Label;
        _label.IsVisible = !string.IsNullOrEmpty(Label);
        _title!.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description!.Text = Description;
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        var navigation = NavigationView;
        _navigationHost!.Content = navigation;
        _navigationHost.IsVisible = navigation is not null;
        _headerActionsHost!.Content = HeaderActionsView;
        _headerActionsHost.IsVisible = HeaderActionsView is not null;
        _decoratorHost!.Content = Decorator;
        _decoratorHost.IsVisible = hasDecorator;
        _decoratorHost.Margin = new Thickness(0, _decoratorHost.Margin.Top, hasClose ? CdsSpacing.Spacing09 + CdsSpacing.Spacing05 : CdsSpacing.Spacing05, 0);
        _close!.Description = CloseButtonLabel;
        _close.IsVisible = hasClose;

        // Wide: 24 / 32 padding (no bottom padding under navigation), 80 at the end for the close button or an AI label,
        // 128 for both. Narrow: 16, with room for the close button.
        double end;
        if (wide) end = hasClose && hasDecorator ? CdsSpacing.Spacing11 + CdsSpacing.Spacing09 : hasClose || hasDecorator ? CdsSpacing.Spacing11 : CdsSpacing.Spacing07;
        else end = CdsSpacing.Spacing05 + (hasClose ? CdsModalSizing.CloseButtonSize : 0) + (hasDecorator ? CdsSpacing.Spacing09 : 0);
        var pad = wide ? CdsSpacing.Spacing06 : CdsSpacing.Spacing05;
        var side = wide ? CdsSpacing.Spacing07 : CdsSpacing.Spacing05;
        _fields!.Margin = new Thickness(side, pad, end, navigation is not null && wide ? 0 : pad);
        var includeHeader = !string.IsNullOrEmpty(Label) || !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Description)
            || HeaderActionsView is not null || navigation is not null || hasClose;
        _header!.IsVisible = includeHeader;

        var body = ComposeBody(Body);
        if (_contentHost is CdsLayer layer) layer.Body = body;
        else _contentHost!.Content = body;

        var influencer = InfluencerView;
        var start = InfluencerSide == CdsTearsheetInfluencerPosition.Start;
        var influencerWidth = InfluencerWidthOverride ?? TearsheetModel.InfluencerWidth(InfluencerSize);
        SetInfluencer(_influencerStart!, start ? influencer : null, influencerWidth);
        SetInfluencer(_influencerEnd!, start ? null : influencer, influencerWidth);

        _buttons!.IsVisible = _actionSet.Actions.Count > 0;
        CdsSemantics.SetGroup(_surface, Title, CdsSemanticGroupKind.Group);
        UpdateDescriptionWidth();
    }

    void UpdateDescriptionWidth()
    {
        if (_description is null || _header is null || _header.Width <= 0) return;
        // 60% of the header from the md breakpoint (80% in the narrow variant), else the full width.
        var fraction = !IsWide ? 0.8 : (_host?.Width ?? _header.Width) >= CdsModalSizing.BreakpointMd ? 0.6 : 1;
        _description.MaximumWidthRequest = Math.Max(0, _header.Width * fraction);
    }

    void ApplyViewport()
    {
        if (_surface is null || _host is not { Width: > 0, Height: > 0 } host) return;
        var width = IsWide ? TearsheetModel.WideWidth(host.Width) : TearsheetModel.NarrowWidth(host.Width);
        var sameSize = Stack(host).All(t => t.IsWide == IsWide);
        _surface.WidthRequest = width;
        _surface.HeightRequest = TearsheetModel.Height(host.Width, host.Height, _levelsAbove, sameSize);
        var scale = TearsheetModel.StackScale(width, _levelsAbove, sameSize);
        if (CdsMotion.IsReducedMotion || _surface.Handler is null) _surface.Scale = scale;
        else _ = _surface.ScaleToAsync(scale, CdsDuration.Moderate02, CdsEasing.EntranceExpressive);
        UpdateDescriptionWidth();
    }

    void OnHostSizeChanged(object? sender, EventArgs e) => ApplyViewport();

    static List<CdsTearsheetShell> Stack(CdsOverlayHost host) => s_stacks.GetOrCreateValue(host);

    static void Restack(CdsOverlayHost host)
    {
        var stack = Stack(host);
        for (var i = 0; i < stack.Count; i++)
        {
            stack[i]._levelsAbove = stack.Count - 1 - i;
            stack[i].ApplyViewport();
        }
    }

    VisualElement? ResolveFocus()
    {
        if (InitialFocusElement is { IsEnabled: true, IsVisible: true } explicitTarget) return explicitTarget;
        View?[] order = [HeaderActionsView, _close is { IsVisible: true } ? _close : null, NavigationView,
            InfluencerSide == CdsTearsheetInfluencerPosition.Start ? InfluencerView : null, Body,
            InfluencerSide == CdsTearsheetInfluencerPosition.End ? InfluencerView : null];
        foreach (var part in order)
            if (FindFocusable(part) is { } found) return found;
        foreach (var b in _actionSet.Shown) if (b.IsEnabled && b.IsVisible) return b;
        return null;
    }

    static VisualElement? FindFocusable(Element? element) => element switch
    {
        null => null,
        CdsModalCloseButton { IsVisible: true } close => close,
        CdsControl { IsEnabled: true, IsVisible: true } c when c.AccessibilityTarget is not null => c,
        InputView { IsEnabled: true, IsVisible: true } input => input,
        VisualElement { IsVisible: false } => null,
        IVisualTreeElement tree => tree.GetVisualChildren().Select(c => FindFocusable(c as Element)).FirstOrDefault(f => f is not null),
    };

    /// <summary>Moves focus back into the tearsheet (after a step change or an error), as Carbon's <c>claimFocus</c>.</summary>
    internal void ClaimFocus()
    {
        if (_overlay is null) return;
        ResolveFocus()?.Focus();
    }

    internal void Show()
    {
        if (_overlay is not null) return;
        var host = CdsOverlayHost.Find(this) ?? CdsOverlayService.FindCurrentHost();
        if (host is not null && Stack(host).Count >= TearsheetModel.MaxDepth)
        {
            System.Diagnostics.Debug.WriteLine("CdsTearsheet: not shown, at most three tearsheets can be stacked.");
            return;
        }
        EnsureSurface();
        _host = host;
        OnOpening();
        Sync();
        if (host is not null)
        {
            host.SizeChanged += OnHostSizeChanged;
            Stack(host).Add(this);
            Restack(host);
        }

        var focus = _openedOnLoad ? null : ResolveFocus();
        _openedOnLoad = false;
        var surface = _surface!;
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            IsModal = true,
            Dock = CdsOverlayDock.Bottom,
            LightDismiss = false,
            CloseOnEscape = false,
            ReturnFocusTo = LauncherElement,
            InitialFocus = focus,
            ScrimTapped = () => { if (TearsheetModel.IsPassive(_actionSet.Actions.Count)) TryClose(); },
            KeyHandler = e =>
            {
                if (e.Key != CdsKey.Escape) return false;
                TryClose();
                return true;
            },
        }, this);
        _overlay.Closed += OnOverlayClosed;
        if (focus is not null)
        {
            void Refocus(object? s, EventArgs e)
            {
                surface.Loaded -= Refocus;
                focus.Focus();
            }
            surface.Loaded += Refocus;
        }
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
        if (_host is { } host)
        {
            host.SizeChanged -= OnHostSizeChanged;
            Stack(host).Remove(this);
            _levelsAbove = 0;
            Restack(host);
        }
        _host = null;
        if (!_hiding && IsOpen) IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Asks to close from the chrome (close button, Escape, a tap outside a passive tearsheet): refused when a
    /// <see cref="RequestClose"/> handler cancels; otherwise runs <see cref="RequestCloseCommand"/> and closes.
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
