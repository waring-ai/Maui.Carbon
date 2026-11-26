namespace Maui.Carbon;

/// <summary>
/// Lets people move through a long list that comes in pages: an items-per-page select, the range ("1–10 of 103 items"), a
/// page select with "of N pages", and previous and next buttons. Put it directly under a <see cref="CdsDataTable"/> (it then
/// drops its top rule), <see cref="CdsContainedList"/> or <see cref="CdsStructuredList"/> and give it the width of the list: below
/// 672 only the range and the arrows remain. Set <see cref="TotalItems"/>, <see cref="PageSizes"/> and bind <see cref="Page"/>
/// and <see cref="PageSize"/> two-way (or handle <see cref="PageChanged"/>) and slice your data from them. Changing the page
/// size keeps the first visible item on screen. For numbered pages of content (search results, articles) use
/// <see cref="CdsPaginationNav"/>.
/// </summary>
public partial class CdsPagination : CdsControl
{
    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly BoxView _topRule = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
    readonly HorizontalStackLayout _left = new() { Spacing = 0 };
    readonly HorizontalStackLayout _right = new() { Spacing = 0 };
    readonly CdsText _perPageText = new() { TypeStyle = CdsTypeStyle.BodyCompact01, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(CdsSpacing.Spacing05, 0, 1, 0), LineBreakMode = LineBreakMode.NoWrap };
    readonly CdsSelect _sizeSelect = new() { IsInline = true, HideLabel = true, VerticalOptions = LayoutOptions.Fill, MinimumWidthRequest = 72 };
    readonly BoxView _sizeDivider = new() { WidthRequest = 1, InputTransparent = true };
    readonly CdsText _rangeText = new() { TypeStyle = CdsTypeStyle.BodyCompact01, Tone = CdsTextTone.Secondary, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(CdsSpacing.Spacing05, 0, CdsSpacing.Spacing05, 0), LineBreakMode = LineBreakMode.NoWrap, HorizontalOptions = LayoutOptions.Start };
    readonly BoxView _rightDivider = new() { WidthRequest = 1, InputTransparent = true };
    readonly CdsSelect _pageSelect = new() { IsInline = true, HideLabel = true, VerticalOptions = LayoutOptions.Fill, MinimumWidthRequest = 72 };
    readonly CdsNativeEntry _pageEntry = new() { Keyboard = Keyboard.Numeric, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 56, VerticalOptions = LayoutOptions.Center, IsVisible = false };
    readonly CdsText _pageRangeText = new() { TypeStyle = CdsTypeStyle.BodyCompact01, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(1, 0, CdsSpacing.Spacing05, 0), LineBreakMode = LineBreakMode.NoWrap };
    readonly CdsText _pageText = new() { TypeStyle = CdsTypeStyle.BodyCompact01, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(CdsSpacing.Spacing05, 0), LineBreakMode = LineBreakMode.NoWrap, IsVisible = false };
    readonly CdsPageButton _back = new();
    readonly CdsPageButton _forward = new();
    List<int> _sizes = [];
    int _builtPageCount = -1;
    bool _syncing;
    bool _compact;
    bool _attached;

    /// <summary>Creates a pagination bar with page sizes 10 to 50.</summary>
    public CdsPagination()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;

        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        CdsTokens.SetColor(_root, BackgroundColorProperty, CdsPaginationTokens.Background);

        _sizeSelect.ItemToString = static o => o.ToString() ?? string.Empty;
        _pageSelect.ItemToString = static o => o.ToString() ?? string.Empty;
        _sizeSelect.SelectedItemChanged += OnSizePicked;
        _pageSelect.SelectedItemChanged += OnPagePicked;
        _pageEntry.Completed += (_, _) => CommitPageEntry();
        _pageEntry.Unfocused += (_, _) => CommitPageEntry();

        _left.Add(_perPageText);
        _left.Add(_sizeSelect);
        _left.Add(_sizeDivider);
        _left.Add(_rangeText);
        _right.Add(_rightDivider);
        _right.Add(_pageSelect);
        _right.Add(_pageEntry);
        _right.Add(_pageText);
        _right.Add(_pageRangeText);
        _right.Add(_back);
        _right.Add(_forward);
        _root.Add(_left, 0, 0);
        _root.Add(_right, 2, 0);
        _root.Add(_topRule, 0, 0);
        Grid.SetColumnSpan(_topRule, 3);

        _back.SetIcon(CdsIconName.ChevronLeft);
        _forward.SetIcon(CdsIconName.ChevronRight);
        _back.HasStartBorder = true;
        _forward.HasStartBorder = true;
        _back.Clicked += (_, _) => Step(-1);
        _forward.Clicked += (_, _) => Step(1);

        SizeChanged += (_, _) => UpdateCompact();
        SetChrome(_root);
        OnPaging();
    }

    /// <summary>Raised when the user changed the page or the page size (<c>onChange({ page, pageSize })</c>).</summary>
    public event EventHandler<CdsPaginationChangedEventArgs>? PageChanged;

    /// <summary>The number of pages: at least 1.</summary>
    public int PageCount => PaginationModel.PageCount(TotalItems, PageSize);

    /// <summary>The zero-based index of the first item on the current page, for slicing data.</summary>
    public int FirstItemIndex => PaginationModel.FirstIndex(Page, PageSize);

    /// <summary>Whether the bar currently counts as attached to a data table (its top rule is hidden).</summary>
    internal bool IsAttached => _attached;

    /// <summary>Re-checks whether the bar sits directly under a data table (called by the table when it moves).</summary>
    internal void RefreshAttachment()
    {
        var attached = IsAttachedToTable ?? DetectAttached();
        if (attached == _attached) return;
        _attached = attached;
        _topRule.IsVisible = CdsPaginationTokens.ShowsTopRule(attached);
    }

    bool DetectAttached()
    {
        if (Parent is not Layout layout) return false;
        var index = layout.Children.IndexOf(this);
        return index > 0 && layout.Children[index - 1] is CdsDataTable;
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();
        RefreshAttachment();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _back is not null) UpdateAll();
    }

    /// <summary>Normalizes sizes, page size and page after any paging property changed, then redraws.</summary>
    void OnPaging()
    {
        _sizes = PaginationModel.NormalizeSizes(PageSizes).ToList();
        var size = PaginationModel.ResolvePageSize(PageSize, _sizes);
        var count = PaginationModel.PageCount(TotalItems, size);
        var page = PaginationModel.ClampPage(Page, count);
        if (size != PageSize) SetValue(PageSizeProperty, size);
        if (page != Page) SetValue(PageProperty, page);
        UpdateAll();
    }

    void Step(int delta)
    {
        var count = PageCount;
        var next = Page + delta;
        if (delta < 0 ? !PaginationModel.CanGoBack(Page) : !PaginationModel.CanGoForward(Page, count, IsPagesUnknown, IsLastPage)) return;
        SetPage(next);
        // Reaching an end with an arrow moves focus to the other arrow so keyboard users are not left on a disabled button.
        if (delta < 0 && !PaginationModel.CanGoBack(Page)) _forward.Focus();
        else if (delta > 0 && !PaginationModel.CanGoForward(Page, count, IsPagesUnknown, IsLastPage)) _back.Focus();
    }

    void SetPage(int page)
    {
        page = IsPagesUnknown ? Math.Max(1, page) : PaginationModel.ClampPage(page, PageCount);
        if (page == Page) return;
        Page = page;
        PageChanged?.Invoke(this, new CdsPaginationChangedEventArgs(Page, PageSize));
    }

    void OnSizePicked(object? sender, SelectedItemChangedEventArgs e)
    {
        if (_syncing || e.SelectedItem is not int size) return;
        ChangePageSize(size);
    }

    /// <summary>The user picked a page size: Carbon returns to page 1 and reports both in one change.</summary>
    internal void ChangePageSize(int size)
    {
        if (size == PageSize) return;
        PageSize = size;
        Page = 1;
        PageChanged?.Invoke(this, new CdsPaginationChangedEventArgs(Page, PageSize));
    }

    void OnPagePicked(object? sender, SelectedItemChangedEventArgs e)
    {
        if (_syncing || e.SelectedItem is not int page) return;
        SetPage(page);
    }

    void CommitPageEntry()
    {
        if (_syncing) return;
        if (PaginationModel.ParsePageInput(_pageEntry.Text, PageCount) is { } page) SetPage(page);
        _syncing = true;
        _pageEntry.Text = Page.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _syncing = false;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        // Switch before the groups are measured: switching from SizeChanged left them measured at their old widths, so
        // the arrows were drawn over the range.
        var width = WidthRequest >= 0 ? Math.Min(WidthRequest, widthConstraint) : widthConstraint;
        width = Math.Min(width, MaximumWidthRequest) - Margin.HorizontalThickness;
        if (width > 0 && !double.IsInfinity(width))
        {
            SetCompact(CdsPaginationTokens.IsCompact(width));
            // The range sits in a stack, which measures it unconstrained: cap it so a long range truncates in a narrow
            // bar instead of squeezing the arrows.
            var cap = CdsPaginationTokens.CompactRangeWidth(_compact, width, CdsPaginationTokens.Height(CdsPaginationTokens.Coerce(Size)), _rangeText.Margin.HorizontalThickness);
            if (Math.Abs(_rangeText.MaximumWidthRequest - cap) > 0.5 && !(double.IsPositiveInfinity(cap) && double.IsPositiveInfinity(_rangeText.MaximumWidthRequest)))
                _rangeText.MaximumWidthRequest = cap;
        }
        return base.MeasureOverride(widthConstraint, heightConstraint);
    }

    void UpdateCompact() => SetCompact(CdsPaginationTokens.IsCompact(Width));

    void SetCompact(bool compact)
    {
        if (compact == _compact) return;
        _compact = compact;
        UpdateAll();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetColor(_topRule, BoxView.ColorProperty, CdsPaginationTokens.Rule);
        SetColor(_sizeDivider, BoxView.ColorProperty, CdsPaginationTokens.Rule);
        SetColor(_rightDivider, BoxView.ColorProperty, CdsPaginationTokens.Rule);
        SetColor(_perPageText, CdsText.TextColorProperty, CdsPaginationTokens.Label(disabled));
        SetColor(_pageRangeText, CdsText.TextColorProperty, CdsPaginationTokens.Label(disabled));
        SetColor(_pageText, CdsText.TextColorProperty, CdsPaginationTokens.Label(disabled));
        SetColor(_rangeText, CdsText.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsPaginationTokens.RangeText);
        SetColor(_pageEntry, Entry.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
    }

    void UpdateAll()
    {
        if (_back is null) return;
        var size = CdsPaginationTokens.Coerce(Size);
        var height = CdsPaginationTokens.Height(size);
        var count = PageCount;
        var disabled = !IsEnabled;
        var unknown = IsPagesUnknown;

        _root.MinimumHeightRequest = height;
        _back.SetSize(height, height);
        _forward.SetSize(height, height);
        _back.SetName(BackwardText ?? "Previous page", BackwardText);
        _forward.SetName(ForwardText ?? "Next page", ForwardText);
        _back.IsEnabled = !disabled && PaginationModel.CanGoBack(Page);
        _forward.IsEnabled = !disabled && PaginationModel.CanGoForward(Page, count, unknown, IsLastPage);

        // Left group.
        _perPageText.Text = ItemsPerPageText;
        var (min, max) = PaginationModel.Range(Page, PageSize, TotalItems);
        _rangeText.Text = unknown
            ? ItemText?.Invoke((Page - 1) * PageSize + 1, Page * PageSize) ?? PaginationModel.RangeText(Page, PageSize, TotalItems, true)
            : ItemRangeText?.Invoke(min, max, TotalItems) ?? PaginationModel.RangeText(Page, PageSize, TotalItems, false);
        _sizeSelect.Size = size;
        _sizeSelect.Label = ItemsPerPageText;
        _sizeSelect.IsEnabled = !disabled && !IsPageSizeInputDisabled;

        // Right group.
        var useEntry = !unknown && PaginationModel.UsesPageInput(count);
        _pageSelect.IsVisible = !unknown && !useEntry;
        _pageEntry.IsVisible = useEntry;
        _pageText.IsVisible = unknown;
        _pageRangeText.IsVisible = !unknown;
        _pageSelect.Size = size;
        _pageSelect.Label = $"Page of {count} pages";
        _pageSelect.IsEnabled = !disabled && !IsPageInputDisabled;
        _pageEntry.IsEnabled = !disabled && !IsPageInputDisabled;
        _pageText.Text = PageText?.Invoke(Page) ?? PaginationModel.PageText(Page);
        _pageRangeText.Text = PageRangeText?.Invoke(Page, count) ?? PaginationModel.PageRangeText(count);
        SemanticProperties.SetDescription(_pageEntry, $"Page of {count} pages");

        // Below 672 only the range and the arrows remain.
        _perPageText.IsVisible = !_compact;
        _sizeSelect.IsVisible = !_compact;
        _sizeDivider.IsVisible = !_compact;
        _pageSelect.IsVisible &= !_compact;
        _pageEntry.IsVisible &= !_compact;
        _pageText.IsVisible &= !_compact;
        _pageRangeText.IsVisible &= !_compact;
        _rightDivider.IsVisible = !_compact; // the back button draws its own start border
        // Compact: the range takes the free width and the arrows sit at the end.
        _root.ColumnDefinitions[0].Width = _compact ? GridLength.Star : GridLength.Auto;
        _root.ColumnDefinitions[1].Width = _compact ? new GridLength(0) : GridLength.Star;
        _rangeText.LineBreakMode = _compact ? LineBreakMode.TailTruncation : LineBreakMode.NoWrap;

        SyncSelects(count);
        RefreshAttachment();
        RefreshVisuals();
    }

    void SyncSelects(int count)
    {
        _syncing = true;
        try
        {
            var sizeKey = string.Join(',', _sizes);
            if (_lastSizeKey != sizeKey)
            {
                _sizeSelect.ItemsSource = _sizes.Cast<object>().ToList();
                _lastSizeKey = sizeKey;
            }
            _sizeSelect.SelectedItem = PageSize;

            if (!IsPagesUnknown && !PaginationModel.UsesPageInput(count))
            {
                if (_builtPageCount != count)
                {
                    _pageSelect.ItemsSource = Enumerable.Range(1, count).Cast<object>().ToList();
                    _builtPageCount = count;
                }
                _pageSelect.SelectedItem = Page;
            }
            else
            {
                _builtPageCount = -1;
                _pageEntry.Text = Page.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    string? _lastSizeKey;
}
