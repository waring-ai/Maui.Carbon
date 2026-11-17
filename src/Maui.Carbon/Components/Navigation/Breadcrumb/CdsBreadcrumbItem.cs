using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// One level of a <see cref="CdsBreadcrumb"/>. With <see cref="Href"/>, <see cref="Command"/> or a <see cref="Clicked"/> handler it is a
/// <c>link-primary</c> link; without any it is plain text. Mark the last crumb with <see cref="IsCurrentPage"/> when the page has no
/// visible title (it is then not a link). Fill <see cref="OverflowItems"/> to make the item the "…" overflow crumb: it opens a
/// list of the collapsed levels. Labels are short and match the titles of the pages they link to.
/// </summary>
public class CdsBreadcrumbItem : CdsControl
{
    const double OverflowWidth = CdsSpacing.Spacing06;
    const double OverflowHeight = 18;

    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsBreadcrumbItem), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Href"/>.</summary>
    public static readonly BindableProperty HrefProperty = BindableProperty.Create(
        nameof(Href), typeof(string), typeof(CdsBreadcrumbItem), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsCurrentPage"/>.</summary>
    public static readonly BindableProperty IsCurrentPageProperty = BindableProperty.Create(
        nameof(IsCurrentPage), typeof(bool), typeof(CdsBreadcrumbItem), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsBreadcrumbItem), "Show more breadcrumbs", propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsBreadcrumbItem), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsBreadcrumbItem)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsBreadcrumbItem), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsBreadcrumbItem), null, propertyChanged: OnCommandChanged);

    readonly ObservableCollection<CdsBreadcrumbItem> _overflowItems = [];
    readonly Grid _root = new() { HorizontalOptions = LayoutOptions.Start };
    readonly Grid _crumb = new() { VerticalOptions = LayoutOptions.Center };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.NoWrap, InputTransparent = true };
    readonly CdsIcon _ellipsis = new(CdsIconName.OverflowMenuHorizontal) { IsVisible = false, InputTransparent = true };
    readonly BoxView _underline = new() { HeightRequest = 1, WidthRequest = 12, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.End, Margin = new Thickness(0, 0, 0, 2), IsVisible = false, InputTransparent = true };
    readonly Border _focus = new() { StrokeThickness = 1, BackgroundColor = Colors.Transparent, InputTransparent = true, IsVisible = false, Margin = new Thickness(-1) };
    readonly CdsText _separator = new("/") { InputTransparent = true, VerticalOptions = LayoutOptions.Center };
    readonly CdsHitTarget _hit;
    EventHandler? _clicked;
    CdsOverlay? _overlay;
    CdsBreadcrumbSize _size = CdsBreadcrumbSize.Md;
    bool _showSeparator = true;

    /// <summary>Creates a breadcrumb item.</summary>
    public CdsBreadcrumbItem()
    {
        _crumb.Add(_label);
        _crumb.Add(_ellipsis);
        _crumb.Add(_underline);
        CdsTokens.SetBrush(_focus, Border.StrokeProperty, CdsColorKeys.Focus);
        AutomationProperties.SetIsInAccessibleTree(_separator, false);

        _hit = CreateHitTarget(CdsAccessibilityRole.Link);
        _crumb.Add(_focus);
        _crumb.Add(_hit);

        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.Add(_crumb, 0);
        _root.Add(_separator, 1);
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        SetChrome(_root);

        _overflowItems.CollectionChanged += (_, _) => UpdateLayout();
        Loaded += (_, _) => { if (IsOpen) ShowMenu(); };
        Unloaded += (_, _) => CloseMenu();
        UpdateLayout();
    }

    /// <summary>Creates a link item.</summary>
    public CdsBreadcrumbItem(string text, string? href = null) : this()
    {
        Text = text;
        Href = href;
    }

    /// <summary>Raised on activation (click, tap or Enter), before <see cref="Command"/> and <see cref="Href"/>.</summary>
    public event EventHandler? Clicked
    {
        add
        {
            _clicked += value;
            UpdateLayout();
        }
        remove
        {
            _clicked -= value;
            UpdateLayout();
        }
    }

    /// <summary>The level's name, matching the title of the page it links to.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>An absolute address opened with the system launcher on activation.</summary>
    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    /// <summary>Marks the current page (<c>aria-current</c>): <c>text-primary</c>, not a link. Include it only when the page has no visible title.</summary>
    public bool IsCurrentPage
    {
        get => (bool)GetValue(IsCurrentPageProperty);
        set => SetValue(IsCurrentPageProperty, value);
    }

    /// <summary>The accessible name of the "…" overflow button. Default "Show more breadcrumbs".</summary>
    public string IconDescription
    {
        get => (string)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>Open state of the overflow list (two-way): choosing a level, a tap outside or Escape set it to <c>false</c>.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Run on activation; <c>IsEnabled</c> follows <c>CanExecute</c>.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>The parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>The hidden levels. When not empty this item is the "…" overflow crumb and opens a list of these levels.</summary>
    public IList<CdsBreadcrumbItem> OverflowItems => _overflowItems;

    /// <summary>True when the item is the overflow crumb.</summary>
    public bool IsOverflow => _overflowItems.Count > 0;

    bool IsLink => !IsOverflow && !IsCurrentPage && (!string.IsNullOrEmpty(Href) || Command is not null || _clicked is not null);

    internal void Configure(CdsBreadcrumbSize size, bool showSeparator)
    {
        _size = size;
        _showSeparator = showSeparator;
        UpdateLayout();
    }

    /// <summary>Activates the item as if it were clicked.</summary>
    internal void Activate()
    {
        if (!IsEnabled) return;
        _clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        if (!string.IsNullOrEmpty(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
            _ = Launcher.Default.OpenAsync(uri);
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (IsOverflow) ToggleMenu();
        else if (IsLink) Activate();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var gap = CdsBreadcrumbTokens.SeparatorGap(_size);
        var v = CdsBreadcrumbTokens.Resolve(State, IsLink || IsOverflow);
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        _label.TextDecorations = v.Underline ? TextDecorations.Underline : TextDecorations.None;
        SetColor(_ellipsis, CdsIcon.ColorProperty, v.Foreground);
        SetColor(_underline, BoxView.ColorProperty, CdsColorKeys.LinkPrimaryHover);
        _underline.IsVisible = IsOverflow && Is(CdsInteractionState.Hover);
        SetColor(_separator, Label.TextColorProperty, CdsColorKeys.TextPrimary);
        _separator.Margin = new Thickness(gap, 0, gap, 0);
        _focus.IsVisible = Is(CdsInteractionState.FocusVisible) && (IsLink || IsOverflow);
    }

    void UpdateLayout()
    {
        if (_hit is null) return;
        var typeStyle = CdsBreadcrumbTokens.TypeStyle(_size);
        _label.TypeStyle = typeStyle;
        _separator.TypeStyle = typeStyle;
        _separator.IsVisible = _showSeparator;
        _label.IsVisible = !IsOverflow;
        _label.Text = Text;
        _ellipsis.IsVisible = IsOverflow;
        _crumb.WidthRequest = IsOverflow ? OverflowWidth : -1;
        _crumb.HeightRequest = IsOverflow ? OverflowHeight : -1;
        _hit.IsVisible = IsLink || IsOverflow;
        _hit.Role = IsOverflow ? CdsAccessibilityRole.Button : CdsAccessibilityRole.Link;
        _hit.IsExpanded = IsOverflow ? _overlay is not null : null;
        SemanticProperties.SetDescription(_hit, IsOverflow ? IconDescription : Text);
        if (IsOverflow) ToolTipProperties.SetText(_hit, IconDescription);
        else _hit.ClearValue(ToolTipProperties.TextProperty);
        // Screen readers announce the current page through the description (there is no aria-current in MAUI).
        SemanticProperties.SetDescription(_label, IsCurrentPage ? $"{Text}, current page" : Text);
        AutomationProperties.SetExcludedWithChildren(_crumb, IsLink || IsOverflow);
        RefreshVisuals();
    }

    void ToggleMenu() => IsOpen = !IsOpen;

    void ShowMenu()
    {
        if (_overlay is not null || !IsLoaded || _overflowItems.Count == 0) return;
        var menu = new CdsBreadcrumbMenu(_overflowItems.ToArray(), item =>
        {
            IsOpen = false;
            item.Activate();
        });
        var overlay = CdsOverlayService.Open(menu, new CdsOverlayOptions
        {
            Anchor = this,
            Placement = CdsPlacement.BottomStart,
            Offset = CdsSpacing.Spacing03,
            Keys = CdsOverlayKeys.Navigation | CdsOverlayKeys.Activation,
            KeyHandler = menu.HandleKey,
        });
        _overlay = overlay;
        overlay.Closed += (_, _) =>
        {
            if (!ReferenceEquals(_overlay, overlay)) return;
            _overlay = null;
            IsOpen = false;
            UpdateLayout();
        };
        UpdateLayout();
    }

    void CloseMenu()
    {
        var overlay = _overlay;
        _overlay = null;
        overlay?.Close();
        if (overlay is not null) UpdateLayout();
    }

    void OnIsOpenChanged(bool open)
    {
        if (open) ShowMenu();
        else CloseMenu();
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsBreadcrumbItem)b).UpdateLayout();

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var item = (CdsBreadcrumbItem)b;
        item.SetCommand(item.Command, item.CommandParameter);
        item.UpdateLayout();
    }
}
