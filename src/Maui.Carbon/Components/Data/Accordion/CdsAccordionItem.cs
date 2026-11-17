namespace Maui.Carbon;

/// <summary>
/// One heading and panel of a <see cref="CdsAccordion"/>. <see cref="Title"/> is the heading (short, one line, no
/// controls); <see cref="CdsContainer.Body"/> is the panel, which opens with 8 above and 24 below. The heading is a
/// button (role expander) that toggles <see cref="IsOpen"/>; Enter or Space toggles and Escape closes. Several items can
/// be open at once. Disable an item with <c>IsEnabled="False"</c>: it leaves the tab order and reports collapsed.
/// </summary>
public class CdsAccordionItem : CdsContainer
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsAccordionItem), null,
        propertyChanged: static (b, _, n) =>
        {
            var item = (CdsAccordionItem)b;
            item._title.Text = (string?)n;
            SemanticProperties.SetDescription(item._hit, (string?)n);
        });

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsAccordionItem), false, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsAccordionItem)b).OnIsOpenChanged((bool)o, (bool)n));

    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    readonly BoxView _top = new() { HeightRequest = 1 };
    readonly BoxView _bottom = new() { HeightRequest = 1, IsVisible = false };
    readonly Grid _heading = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.Body01, LineBreakMode = LineBreakMode.WordWrap, InputTransparent = true };
    readonly CdsIcon _chevron = new(CdsIconName.ChevronDown, 16) { InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    readonly ContentView _panel = new() { IsClippedToBounds = true, IsVisible = false };
    readonly ContentView _content = new();
    CdsAccordion? _owner;
    CdsSize _size = CdsSize.Md;
    CdsAccordionAlign _align = CdsAccordionAlign.End;
    bool _flush;
    bool _groupDisabled;
    bool _headingHover;
    bool _neighbourHover;
    bool _isLast;
    double _naturalHeight;

    /// <summary>Creates a closed item.</summary>
    public CdsAccordionItem()
    {
        HorizontalOptions = LayoutOptions.Fill;
        _hit = CreateHitTarget(CdsAccessibilityRole.Expander);
        _hit.IsExpanded = false;
        AutomationProperties.SetExcludedWithChildren(_title, true);
        AutomationProperties.SetExcludedWithChildren(_chevron, true);

        _heading.Add(_fill);
        _heading.Add(_title);
        _heading.Add(_chevron);
        _heading.Add(_ring);
        _heading.Add(_hit);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => SetHeadingHover(true);
        pointer.PointerExited += (_, _) => SetHeadingHover(false);
        _heading.GestureRecognizers.Add(pointer);

        _panel.Content = _content;
        _stack.Add(_top);
        _stack.Add(_heading);
        _stack.Add(_panel);
        _stack.Add(_bottom);
        SetChrome(_stack);
        _heading.SizeChanged += (_, _) => UpdateLayout();
        Rebuild();
    }

    /// <summary>Raised when the user toggles the heading, after <see cref="IsOpen"/> changed (<c>onHeadingClick</c>).</summary>
    public event EventHandler? HeadingClicked;

    /// <summary>The heading text.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Whether the panel is open (two-way). Several items of an accordion can be open at once.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>True when this item is disabled, by itself or by its accordion (an item that sets <c>IsEnabled</c> itself wins).</summary>
    internal bool IsItemDisabled => !IsEnabled || (_groupDisabled && !IsSet(IsEnabledProperty));

    internal bool IsHeadingHovered => _headingHover;

    internal event EventHandler? HeadingHoverChanged;

    internal void Attach(CdsAccordion? owner) => _owner = owner;

    internal void Configure(CdsSize size, CdsAccordionAlign align, bool flush, bool groupDisabled, bool isLast)
    {
        _size = size;
        _align = align;
        _flush = flush;
        _groupDisabled = groupDisabled;
        _isLast = isLast;
        Rebuild();
    }

    internal void SetNeighbourHover(bool hover)
    {
        if (_neighbourHover == hover) return;
        _neighbourHover = hover;
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => _content.Content = newBody;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (IsItemDisabled) return;
        IsOpen = !IsOpen;
        HeadingClicked?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected internal override bool WantsEscapeKey => IsOpen;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Escape || !IsOpen) return false;
        IsOpen = false;
        return true;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName) Rebuild();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = IsItemDisabled;
        SetColor(_fill, BoxView.ColorProperty, CdsAccordionTokens.Heading(_headingHover, disabled));
        SetColor(_title, Label.TextColorProperty, CdsAccordionTokens.Title(disabled));
        SetColor(_chevron, CdsIcon.ColorProperty, CdsAccordionTokens.Chevron(disabled));
        _ring.IsActive = Is(CdsInteractionState.FocusVisible) && !disabled;

        // The rule above the item is tinted while this closed item or the item above it is hovered.
        var tintedTop = !disabled && ((_headingHover && !IsOpen) || _neighbourHover);
        SetColor(_top, BoxView.ColorProperty, CdsAccordionTokens.Divider(tintedTop));
        SetColor(_bottom, BoxView.ColorProperty, CdsAccordionTokens.Divider(_headingHover && !IsOpen && !disabled));
    }

    void SetHeadingHover(bool hover)
    {
        if (_headingHover == hover) return;
        _headingHover = hover;
        RefreshVisuals();
        HeadingHoverChanged?.Invoke(this, EventArgs.Empty);
    }

    void OnIsOpenChanged(bool oldValue, bool open)
    {
        _hit.IsExpanded = open && !IsItemDisabled;
        SetState(CdsInteractionState.Open, open);
        _ = AnimateAsync(open);
        RefreshVisuals();
        _owner?.OnItemToggled(this);
    }

    async Task AnimateAsync(bool open)
    {
        var rotation = CdsAccordionMetrics.ChevronRotation(open);
        var width = _panel.Width > 0 ? _panel.Width : Width;
        if (open)
        {
            _panel.IsVisible = true;
            _naturalHeight = width > 0 ? _content.Measure(width, double.PositiveInfinity).Height : 0;
        }
        if (_naturalHeight <= 0 || Handler is null || CdsMotion.IsReducedMotion)
        {
            _chevron.Rotation = rotation;
            _panel.HeightRequest = -1;
            _panel.IsVisible = open;
            return;
        }

        var from = open ? 0 : _naturalHeight;
        var to = open ? _naturalHeight : 0;
        var fromRotation = _chevron.Rotation;
        var easing = open ? CdsEasing.EntranceProductive : CdsEasing.ExitProductive;
        await CdsMotion.AnimateAsync(this, "accordion", t =>
        {
            _panel.HeightRequest = from + (to - from) * t;
            _chevron.Rotation = fromRotation + (rotation - fromRotation) * t;
        }, 0, 1, CdsDuration.Fast02, easing);
        if (IsOpen == open)
        {
            _panel.HeightRequest = -1;
            _panel.IsVisible = open;
        }
    }

    void UpdateLayout()
    {
        if (_heading.Width <= 0) return;
        var end = CdsAccordionMetrics.ContentEndPadding(_heading.Width);
        var start = CdsAccordionMetrics.ContentStartPadding(_align);
        _content.Padding = new Thickness(start, CdsSpacing.Spacing03, end, CdsSpacing.Spacing06);
    }

    void Rebuild()
    {
        var disabled = IsItemDisabled;
        var height = CdsAccordionMetrics.HeadingHeight(_size);
        var pad = (height - 20) / 2;
        _hit.IsEnabled = !disabled;
        _hit.IsExpanded = IsOpen && !disabled;

        _heading.ColumnDefinitions.Clear();
        _heading.MinimumHeightRequest = height;
        _title.Margin = new Thickness(0, pad);
        if (_align == CdsAccordionAlign.Start)
        {
            // chevron, then the title (16 start margin on the chevron, 16 padding before the title, 16 after it).
            _heading.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _heading.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            Grid.SetColumn(_chevron, 0);
            Grid.SetColumn(_title, 1);
            _chevron.HorizontalOptions = LayoutOptions.Start;
            _chevron.VerticalOptions = LayoutOptions.Start;
            _chevron.Margin = new Thickness(CdsSpacing.Spacing05, pad + 2, 0, 0);
            _title.Margin = new Thickness(CdsSpacing.Spacing05, pad, CdsSpacing.Spacing05, pad);
        }
        else
        {
            _heading.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            _heading.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            Grid.SetColumn(_title, 0);
            Grid.SetColumn(_chevron, 1);
            _chevron.HorizontalOptions = LayoutOptions.End;
            _chevron.VerticalOptions = LayoutOptions.Center;
            _chevron.Margin = new Thickness(CdsSpacing.Spacing05, 0, CdsSpacing.Spacing05, 0);
            _title.Margin = new Thickness(CdsSpacing.Spacing05, pad, 0, pad);
        }
        Grid.SetColumnSpan(_fill, 2);
        Grid.SetColumnSpan(_ring, 2);
        Grid.SetColumnSpan(_hit, 2);

        _top.Margin = _flush ? new Thickness(CdsSpacing.Spacing05, 0) : new Thickness(0);
        _bottom.Margin = _top.Margin;
        _bottom.IsVisible = _isLast;
        _chevron.Rotation = CdsAccordionMetrics.ChevronRotation(IsOpen);
        _panel.IsVisible = IsOpen;
        RefreshVisuals();
    }
}
