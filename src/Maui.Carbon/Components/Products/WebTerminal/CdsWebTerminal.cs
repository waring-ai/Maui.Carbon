namespace Maui.Carbon;

/// <summary>
/// A terminal panel docked at the end edge under the UI shell header (Carbon for IBM Products' <c>WebTerminal</c>): a
/// 48-high bar with documentation links, your actions and a close button over your terminal content, in the dark g90
/// and g100 themes whatever the page theme. Declare it in the page (it draws nothing inline) and toggle
/// <see cref="IsOpen"/> from a header action; set <see cref="PageContent"/> to narrow the page beside it (Carbon:
/// <c>WebTerminalContentWrapper</c>). The content (a command line, logs) is yours.
/// </summary>
[ContentProperty(nameof(Body))]
public class CdsWebTerminal : CdsContainer
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsWebTerminal), false,
        propertyChanged: static (b, _, _) => ((CdsWebTerminal)b).OnIsOpenChanged());

    /// <summary>Shows the terminal while true (Carbon: <c>isInitiallyOpen</c> and the provider's open state). Two-way: the close button sets it to false.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Bindable property for <see cref="CloseButtonLabel"/>.</summary>
    public static readonly BindableProperty CloseButtonLabelProperty = BindableProperty.Create(
        nameof(CloseButtonLabel), typeof(string), typeof(CdsWebTerminal), "Close terminal",
        propertyChanged: static (b, _, _) => ((CdsWebTerminal)b).Sync());

    /// <summary>The accessible name and tooltip of the close button (Carbon: <c>closeIconDescription</c>).</summary>
    public string CloseButtonLabel
    {
        get => (string)GetValue(CloseButtonLabelProperty);
        set => SetValue(CloseButtonLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="DocumentationLinksIconDescription"/>.</summary>
    public static readonly BindableProperty DocumentationLinksIconDescriptionProperty = BindableProperty.Create(
        nameof(DocumentationLinksIconDescription), typeof(string), typeof(CdsWebTerminal), "Show documentation links",
        propertyChanged: static (b, _, _) => ((CdsWebTerminal)b).Sync());

    /// <summary>The accessible name of the documentation links menu.</summary>
    public string DocumentationLinksIconDescription
    {
        get => (string)GetValue(DocumentationLinksIconDescriptionProperty);
        set => SetValue(DocumentationLinksIconDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="BarLabel"/>.</summary>
    public static readonly BindableProperty BarLabelProperty = BindableProperty.Create(
        nameof(BarLabel), typeof(string), typeof(CdsWebTerminal), "Web terminal header",
        propertyChanged: static (b, _, _) => ((CdsWebTerminal)b).Sync());

    /// <summary>The accessible name of the terminal bar (Carbon: <c>webTerminalAriaLabel</c>).</summary>
    public string BarLabel
    {
        get => (string)GetValue(BarLabelProperty);
        set => SetValue(BarLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="PageContent"/>.</summary>
    public static readonly BindableProperty PageContentProperty = BindableProperty.Create(
        nameof(PageContent), typeof(View), typeof(CdsWebTerminal), null);

    /// <summary>The page content the open terminal narrows by its width.</summary>
    public View? PageContent
    {
        get => (View?)GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }


    /// <summary>The terminal width (<c>$web-terminal-width</c>, 40rem).</summary>
    internal const double PanelWidth = 640;

    readonly CdsItemCollection<CdsIconButton> _actions = [];
    readonly CdsItemCollection<CdsMenuItemBase> _links = [];
    CdsOverlay? _overlay;
    bool _hiding;
    double _pushed;
    Grid? _surface;
    HorizontalStackLayout? _actionRow;
    CdsOverflowMenu? _linksMenu;
    CdsIconButton? _close;
    ContentView? _bodyHost;

    /// <summary>Creates a closed web terminal.</summary>
    public CdsWebTerminal()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        WidthRequest = 0;
        HeightRequest = 0;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new ContentView());
        _actions.CollectionChanged += (_, _) => Sync();
        _links.CollectionChanged += (_, _) => Sync();
        Loaded += (_, _) => { if (IsOpen && _overlay is null) Show(); };
        Unloaded += (_, _) => Hide();
    }

    /// <summary>Raised after the terminal has closed.</summary>
    public event EventHandler? Closed;

    /// <summary>Ghost icon buttons in the bar, before the close button (Carbon: <c>actions</c>).</summary>
    public IList<CdsIconButton> Actions => _actions;

    /// <summary>Items of the documentation links menu (Carbon: <c>documentationLinks</c>); the menu shows when there are some.</summary>
    public IList<CdsMenuItemBase> DocumentationLinks => _links;

    /// <summary>True while shown (for tests).</summary>
    internal bool IsShown => _overlay is not null;

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        if (_bodyHost is not null) _bodyHost.Content = newBody;
    }

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

    void EnsureSurface()
    {
        if (_surface is not null) return;
        _actionRow = new HorizontalStackLayout { Spacing = 0 };
        _close = new CdsIconButton(CdsIconName.Close, CloseButtonLabel, CdsIconButtonKind.Ghost) { Size = CdsSize.Lg, HorizontalOptions = LayoutOptions.End, TooltipPlacement = CdsPlacement.Left };
        _close.Clicked += (_, _) => IsOpen = false;
        var bar = new Grid { HeightRequest = CdsSpacing.Spacing09, ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)] };
        bar.Add(_actionRow, 0);
        bar.Add(_close, 1);
        CdsTokens.SetColor(bar, BackgroundColorProperty, CdsColorKeys.Background);
        CdsSemantics.SetGroup(bar, BarLabel, CdsSemanticGroupKind.Banner);

        _bodyHost = new ContentView { Content = Body };
        var body = new CdsTheme { Theme = CdsThemeName.G100, Body = _bodyHost };
        var panel = new Grid { RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star)] };
        panel.Add(bar, 0, 0);
        panel.Add(body, 0, 1);
        CdsTokens.SetColor(panel, BackgroundColorProperty, CdsColorKeys.Background);
        // The terminal is always dark: g90 for the bar and frame, g100 for the content. 1px gray-90 start border.
        var edge = new BoxView { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true };
        CdsTokens.SetColor(edge, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        var frame = new Grid();
        frame.Add(panel);
        frame.Add(edge);
        _surface = new Grid { BindingContext = BindingContext, VerticalOptions = LayoutOptions.Fill };
        _surface.Add(new CdsTheme { Theme = CdsThemeName.G90, Body = frame });
        Sync();
    }

    void Sync()
    {
        if (_surface is null) return;
        _close!.Label = CloseButtonLabel;
        _actionRow!.Children.Clear();
        if (_links.Count > 0)
        {
            _linksMenu ??= new CdsOverflowMenu { Icon = CdsIconName.Help };
            _linksMenu.IconDescription = DocumentationLinksIconDescription;
            _linksMenu.Items.Clear();
            foreach (var link in _links) _linksMenu.Items.Add(link);
            _actionRow.Add(_linksMenu);
        }
        foreach (var action in _actions)
        {
            action.Kind = CdsIconButtonKind.Ghost;
            action.Size = CdsSize.Lg;
            _actionRow.Add(action);
        }
    }

    void ApplyViewport()
    {
        if (_surface is null) return;
        var host = CdsOverlayHost.Find(this) ?? CdsOverlayService.FindCurrentHost();
        _surface.WidthRequest = host is { Width: > 0 } ? Math.Min(PanelWidth, host.Width) : PanelWidth;
        var top = CdsShellContext.FindShell(this) is not null ? UIShellModel.HeaderHeight : 0;
        _surface.Margin = new Thickness(0, top, 0, 0);
    }

    internal void Show()
    {
        if (_overlay is not null) return;
        EnsureSurface();
        Sync();
        ApplyViewport();
        _overlay = CdsOverlayService.Open(_surface!, new CdsOverlayOptions
        {
            Dock = CdsOverlayDock.End,
            LightDismiss = false,
            CloseOnEscape = false,
            ReturnFocus = false,
        }, this);
        _overlay.Closed += OnOverlayClosed;
        Push(_surface!.WidthRequest);
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
        Push(0);
        if (!_hiding && IsOpen) IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    void Push(double amount)
    {
        if (PageContent is not { } page) return;
        var from = _pushed;
        _pushed = amount;
        void Apply(double v)
        {
            var m = page.Margin;
            page.Margin = new Thickness(m.Left, m.Top, v, m.Bottom);
        }
        if (CdsMotion.IsReducedMotion || page.Handler is null)
        {
            Apply(amount);
            return;
        }
        _ = CdsMotion.AnimateAsync(page, "CdsWebTerminalPush", Apply, from, amount, CdsDuration.Moderate02, CdsEasing.StandardProductive);
    }
}
