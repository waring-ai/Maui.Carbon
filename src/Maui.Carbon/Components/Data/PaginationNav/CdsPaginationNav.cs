namespace Maui.Carbon;

/// <summary>Tokens and measurements of <see cref="CdsPaginationNav"/> (from <c>packages/styles/scss/components/pagination-nav</c>).</summary>
public static class CdsPaginationNavTokens
{
    /// <summary>Hover fill of a page button: <c>background-hover</c>.</summary>
    public static string Hover => CdsColorKeys.BackgroundHover;

    /// <summary>The bar under the active page: <c>border-interactive</c>.</summary>
    public static string ActiveBar => CdsColorKeys.BorderInteractive;

    /// <summary>
    /// Page label and arrow glyph: <c>text-primary</c>; a disabled item (an arrow at the end) is <c>text-secondary</c> drawn
    /// at <see cref="DisabledOpacity"/>, as Carbon's PaginationNav specifies.
    /// </summary>
    public static string Text(bool disabled) => disabled ? CdsColorKeys.TextSecondary : CdsColorKeys.TextPrimary;

    /// <summary>The opacity of a disabled item's label or glyph (Carbon: <c>text-secondary</c> at 50%).</summary>
    public const double DisabledOpacity = 0.5;

    /// <summary>
    /// The "…" glyph of an overflow select: <c>text-primary</c>, <c>icon-disabled</c> when the overflow is disabled
    /// (Carbon's <c>select:disabled</c> icon rule).
    /// </summary>
    public static string OverflowGlyph(bool disabled) => disabled ? CdsColorKeys.IconDisabled : CdsColorKeys.TextPrimary;

    /// <summary>The nav height: sm 32, md 40, lg 48 (default). Other sizes clamp to the nearest.</summary>
    public static double Height(CdsSize size) => CdsPaginationTokens.Height(size);

    /// <summary>Width of the active page's underline bar: 16.</summary>
    public const double ActiveBarWidth = CdsSpacing.Spacing05;
}

/// <summary>Event data of <see cref="CdsPaginationNav.PageChanged"/>.</summary>
public sealed class CdsPageChangedEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsPageChangedEventArgs(int page) => Page = page;

    /// <summary>The new page, 0-based.</summary>
    public int Page { get; }
}

/// <summary>
/// Numbered page buttons with previous and next arrows, for moving through pages of content (search results, articles,
/// release notes) rather than table rows. <see cref="TotalItems"/> is the number of pages and <see cref="Page"/> the current one
/// (0-based, two-way). The first and last page always show; pages that do not fit collapse into "…" selects that list the
/// hidden pages, so <see cref="ItemsShown"/> bounds the width (md allows 4 or 5, sm 4 to 7, lg at least 4). The active page is
/// semibold with a 16 wide <c>border-interactive</c> bar. Place it below the content it pages, in the size of nearby controls,
/// and give it a specific <see cref="Label"/> when the screen has more than one. Under a table with a changeable page size use
/// <see cref="CdsPagination"/> instead.
/// </summary>
public class CdsPaginationNav : CdsControl
{
    /// <summary>Bindable property for <see cref="TotalItems"/>.</summary>
    public static readonly BindableProperty TotalItemsProperty = BindableProperty.Create(
        nameof(TotalItems), typeof(int), typeof(CdsPaginationNav), 1,
        coerceValue: static (_, v) => Math.Max(0, (int)v),
        propertyChanged: static (b, _, _) =>
        {
            var nav = (CdsPaginationNav)b;
            var clamped = PaginationNavModel.ClampPage(nav.Page, nav.TotalItems);
            if (clamped != nav.Page) nav.Page = clamped;
            nav.Rebuild();
        });

    /// <summary>Bindable property for <see cref="Page"/>.</summary>
    public static readonly BindableProperty PageProperty = BindableProperty.Create(
        nameof(Page), typeof(int), typeof(CdsPaginationNav), 0, BindingMode.TwoWay,
        coerceValue: static (b, v) => PaginationNavModel.ClampPage((int)v, ((CdsPaginationNav)b).TotalItems),
        propertyChanged: static (b, _, _) => ((CdsPaginationNav)b).OnPageChanged());

    /// <summary>Bindable property for <see cref="ItemsShown"/>.</summary>
    public static readonly BindableProperty ItemsShownProperty = BindableProperty.Create(
        nameof(ItemsShown), typeof(int), typeof(CdsPaginationNav), 10, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsPaginationNav), CdsSize.Lg, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Loop"/>.</summary>
    public static readonly BindableProperty LoopProperty = BindableProperty.Create(
        nameof(Loop), typeof(bool), typeof(CdsPaginationNav), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsOverflowDisabled"/>.</summary>
    public static readonly BindableProperty IsOverflowDisabledProperty = BindableProperty.Create(
        nameof(IsOverflowDisabled), typeof(bool), typeof(CdsPaginationNav), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsPaginationNav), "pagination", propertyChanged: OnChanged);

    readonly HorizontalStackLayout _row = new() { Spacing = 0 };
    readonly CdsPageButton _previous = new() { HoverKey = CdsPaginationNavTokens.Hover, UsesNavDisabledStyle = true };
    readonly CdsPageButton _next = new() { HoverKey = CdsPaginationNavTokens.Hover, UsesNavDisabledStyle = true };
    readonly List<(NavSlot Slot, CdsPageButton? Button)> _slots = [];
    bool _userChange;

    /// <summary>Creates a nav with one page.</summary>
    public CdsPaginationNav()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        _previous.SetIcon(CdsIconName.ChevronLeft);
        _next.SetIcon(CdsIconName.ChevronRight);
        _previous.Clicked += (_, _) => Go(PaginationNavModel.Previous(Page, TotalItems, Loop));
        _next.Clicked += (_, _) => Go(PaginationNavModel.Next(Page, TotalItems, Loop));
        SetChrome(_row);
        Rebuild();
    }

    /// <summary>Raised when the user chose a page (<c>onChange(pageIndex)</c>).</summary>
    public event EventHandler<CdsPageChangedEventArgs>? PageChanged;

    /// <summary>The number of pages (the web version's <c>totalItems</c>).</summary>
    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    /// <summary>The current page, 0-based (two-way).</summary>
    public int Page
    {
        get => (int)GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }

    /// <summary>How many slots the strip uses, counting the first, last and the "…" selects. Default 10; md allows 4 or 5, sm 4 to 7.</summary>
    public int ItemsShown
    {
        get => (int)GetValue(ItemsShownProperty);
        set => SetValue(ItemsShownProperty, value);
    }

    /// <summary>Height of the buttons: sm 32, md 40, lg 48 (default).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Previous and next wrap around at the ends instead of disabling.</summary>
    public bool Loop
    {
        get => (bool)GetValue(LoopProperty);
        set => SetValue(LoopProperty, value);
    }

    /// <summary>Shows the "…" but makes it unselectable.</summary>
    public bool IsOverflowDisabled
    {
        get => (bool)GetValue(IsOverflowDisabledProperty);
        set => SetValue(IsOverflowDisabledProperty, value);
    }

    /// <summary>The accessible name of the navigation (<c>aria-label</c>); default "pagination". Be specific ("Search results pages") when there are several.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The slots currently shown (for tests): how many page buttons and overflow selects.</summary>
    internal IReadOnlyList<NavSlot> Slots => _slots.Select(s => s.Slot).ToList();

    void Go(int page)
    {
        if (page < 0) return;
        Choose(page);
    }

    void Choose(int page)
    {
        page = PaginationNavModel.ClampPage(page, TotalItems);
        if (page == Page) return;
        _userChange = true;
        try
        {
            Page = page;
        }
        finally
        {
            _userChange = false;
        }
        PageChanged?.Invoke(this, new CdsPageChangedEventArgs(Page));
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsPaginationNav)b).Rebuild();

    // Every change of the current page is announced (the web's polite live region), whether the user or a binding made it;
    // not before the nav is on screen, where it would be read out at page load.
    void OnPageChanged()
    {
        Rebuild();
        if (IsLoaded) CdsAnnouncer.Announce(PaginationNavModel.Announcement(Page, TotalItems));
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
    }

    void Rebuild()
    {
        var size = CdsPaginationTokens.Coerce(Size);
        var height = CdsPaginationNavTokens.Height(size);
        var shown = PaginationNavModel.CoerceItemsShown(size, ItemsShown);
        var slots = PaginationNavModel.Build(Page, TotalItems, shown);
        var hadFocus = _userChange && _slots.Any(s => s.Button is { } b && b.Is(CdsInteractionState.Focused));

        _row.Children.Clear();
        _slots.Clear();

        _previous.SetSize(height, height);
        _next.SetSize(height, height);
        _previous.SetName("Previous", "Previous");
        _next.SetName("Next", "Next");
        _previous.IsEnabled = PaginationNavModel.Previous(Page, TotalItems, Loop) >= 0;
        _next.IsEnabled = PaginationNavModel.Next(Page, TotalItems, Loop) >= 0;
        _row.Add(_previous);

        CdsPageButton? current = null;
        foreach (var slot in slots)
        {
            if (slot.Kind == NavSlotKind.Page)
            {
                var button = new CdsPageButton { HoverKey = CdsPaginationNavTokens.Hover };
                var active = slot.Start == Page;
                button.SetSize(0, height, height);
                button.SetText((slot.Start + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), active);
                button.SetName(active ? $"Active, Page {slot.Start + 1}" : $"Page {slot.Start + 1}");
                var target = slot.Start;
                button.Clicked += (_, _) => Choose(target);
                _row.Add(button);
                _slots.Add((slot, button));
                if (active) current = button;
            }
            else
            {
                _row.Add(BuildOverflow(slot, height));
                _slots.Add((slot, null));
            }
        }
        _row.Add(_next);
        CdsSemantics.SetGroup(_row, Label, CdsSemanticGroupKind.Navigation);
        if (hadFocus && current is not null) Dispatcher.Dispatch(() => current.Focus());
    }

    View BuildOverflow(NavSlot slot, double height)
    {
        var host = new Grid { WidthRequest = height, HeightRequest = height };
        var fill = new BoxView { InputTransparent = true };
        // The native picker's own focus visuals are stripped: draw Carbon's 2px inset focus outline while it has focus.
        var ring = new CdsFocusRing(CdsFocusRingStyle.Inset);
        CdsTokens.SetColor(fill, BoxView.ColorProperty, null);
        var picker = new CdsNativePicker
        {
            Title = "…",
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            ItemsSource = Enumerable.Range(slot.Start, slot.Count).Select(p => (p + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)).ToList(),
            IsEnabled = !IsOverflowDisabled,
        };
        picker.SetDynamicResource(Picker.TitleColorProperty, CdsPaginationNavTokens.OverflowGlyph(IsOverflowDisabled));
        picker.Focused += (_, _) => ring.IsActive = true;
        picker.Unfocused += (_, _) => ring.IsActive = false;
        SemanticProperties.SetDescription(picker, "Select Page number");
        picker.SelectedIndexChanged += (_, _) =>
        {
            var index = picker.SelectedIndex;
            if (index < 0) return;
            picker.SelectedIndex = -1;
            Choose(slot.Start + index);
        };
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { if (!IsOverflowDisabled) CdsTokens.SetColor(fill, BoxView.ColorProperty, CdsPaginationNavTokens.Hover); };
        pointer.PointerExited += (_, _) => CdsTokens.SetColor(fill, BoxView.ColorProperty, null);
        host.GestureRecognizers.Add(pointer);
        host.Add(fill);
        host.Add(picker);
        host.Add(ring);
        return host;
    }
}
