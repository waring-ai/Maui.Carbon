using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// The static table primitive of the data table family, for hand-built tables: a <see cref="Head"/> of
/// <see cref="CdsTableHeader"/>s and <see cref="Rows"/> of <see cref="CdsTableRow"/>s of <see cref="CdsTableCell"/>s, all
/// sharing the column widths. Headers sit on <c>layer-accent-01</c> in 600 weight <c>text-primary</c>; body rows are
/// <c>layer-01</c> with <c>text-secondary</c> cells (<c>text-primary</c> when hovered or selected), <c>border-subtle-01</c>
/// dividers, <c>layer-hover-01</c> on hover and <c>layer-selected-01</c> when selected. Rows are 24, 32, 40, 48 or 64 high
/// (<see cref="Size"/>); <see cref="UseZebraStyles"/> stripes every second row; <see cref="StickyHeader"/> pins the header
/// above a body that scrolls inside a 300 high area. The table does not sort, filter or select by itself: use
/// <see cref="CdsDataTable"/> when it should. Keep to about 200 rows and put a <see cref="CdsPagination"/> under longer data.
/// </summary>
[ContentProperty(nameof(Rows))]
public class CdsTable : CdsControl
{
    /// <summary>Bindable property for <see cref="Head"/>.</summary>
    public static readonly BindableProperty HeadProperty = BindableProperty.Create(
        nameof(Head), typeof(CdsTableHead), typeof(CdsTable), null,
        propertyChanged: static (b, o, n) => ((CdsTable)b).OnHeadChanged((CdsTableHead?)o, (CdsTableHead?)n));

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsTable), CdsSize.Lg,
        propertyChanged: static (b, _, _) => ((CdsTable)b).Rebuild());

    /// <summary>Bindable property for <see cref="UseZebraStyles"/>.</summary>
    public static readonly BindableProperty UseZebraStylesProperty = BindableProperty.Create(
        nameof(UseZebraStyles), typeof(bool), typeof(CdsTable), false,
        propertyChanged: static (b, _, _) => ((CdsTable)b).RequestVisuals());

    /// <summary>Bindable property for <see cref="IsSortable"/>.</summary>
    public static readonly BindableProperty IsSortableProperty = BindableProperty.Create(
        nameof(IsSortable), typeof(bool), typeof(CdsTable), false,
        propertyChanged: static (b, _, _) => ((CdsTable)b).Rebuild());

    /// <summary>Bindable property for <see cref="StickyHeader"/>.</summary>
    public static readonly BindableProperty StickyHeaderProperty = BindableProperty.Create(
        nameof(StickyHeader), typeof(bool), typeof(CdsTable), false,
        propertyChanged: static (b, _, _) => ((CdsTable)b).Rebuild());

    /// <summary>Bindable property for <see cref="MaximumBodyHeight"/>.</summary>
    public static readonly BindableProperty MaximumBodyHeightProperty = BindableProperty.Create(
        nameof(MaximumBodyHeight), typeof(double), typeof(CdsTable), CdsTableTokens.StickyBodyHeight,
        propertyChanged: static (b, _, _) => ((CdsTable)b).Rebuild());

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsTable), null,
        propertyChanged: static (b, _, _) => ((CdsTable)b).UpdateAccessibility());

    /// <summary>Bindable property for <see cref="ChildInset"/>.</summary>
    public static readonly BindableProperty ChildInsetProperty = BindableProperty.Create(
        nameof(ChildInset), typeof(double), typeof(CdsTable), 56d,
        propertyChanged: static (b, _, _) => ((CdsTable)b).Rebuild());

    readonly ObservableCollection<CdsTableRow> _rows = [];
    readonly Grid _bodyGrid = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly Grid _headerGrid = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly Grid _stickyRoot = new() { RowSpacing = 0 };
    readonly ScrollView _scroll = new() { Orientation = ScrollOrientation.Vertical };
    readonly ScrollView _hscroll = new() { Orientation = ScrollOrientation.Horizontal, HorizontalScrollBarVisibility = ScrollBarVisibility.Default };
    double _minimumWidth;
    readonly List<HeaderVisual> _headers = [];
    readonly List<RowVisual> _rowVisuals = [];
    int _suspend;
    bool _visualsDirty;
    bool _dirty;
    int _hoverRow = -1;
    int _hoverHeader = -1;
    int _hoverVersion;
    bool _building;

    /// <summary>Creates an empty table.</summary>
    public CdsTable()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        CdsTokens.SetColor(_bodyGrid, BackgroundColorProperty, CdsTableTokens.Background);
        _stickyRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _stickyRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _stickyRoot.Add(_headerGrid, 0, 0);
        _stickyRoot.Add(_scroll, 0, 1);
        _rows.CollectionChanged += OnRowsChanged;
        _hscroll.SizeChanged += (_, _) => SyncContentWidth();
        Rebuild();
    }

    /// <summary>Raised when a sortable header is activated (also raised on the header itself).</summary>
    public event EventHandler<CdsTableHeaderEventArgs>? HeaderClicked;

    /// <summary>The header row: one <see cref="CdsTableHeader"/> per column. It also defines the column widths and alignment.</summary>
    public CdsTableHead? Head
    {
        get => (CdsTableHead?)GetValue(HeadProperty);
        set => SetValue(HeadProperty, value);
    }

    /// <summary>The body rows (the content property).</summary>
    public IList<CdsTableRow> Rows => _rows;

    /// <summary>Row height: xs 24, sm 32, md 40, lg 48 (default), xl 64. Match the toolbar and the pagination under it.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Stripes every second row with <c>layer-accent-01</c>. A parent and its child row share a stripe.</summary>
    public bool UseZebraStyles
    {
        get => (bool)GetValue(UseZebraStylesProperty);
        set => SetValue(UseZebraStylesProperty, value);
    }

    /// <summary>Makes every header a sort button unless its <see cref="CdsTableHeader.IsSortable"/> is <c>false</c>.</summary>
    public bool IsSortable
    {
        get => (bool)GetValue(IsSortableProperty);
        set => SetValue(IsSortableProperty, value);
    }

    /// <summary>
    /// Pins the header while the body scrolls inside an area of at most <see cref="MaximumBodyHeight"/>. Header and body then
    /// are separate grids, so give columns fixed or star widths (an <c>Auto</c> column becomes a star column).
    /// </summary>
    public bool StickyHeader
    {
        get => (bool)GetValue(StickyHeaderProperty);
        set => SetValue(StickyHeaderProperty, value);
    }

    /// <summary>The height of the scrolling body when <see cref="StickyHeader"/> is on. Default 300.</summary>
    public double MaximumBodyHeight
    {
        get => (double)GetValue(MaximumBodyHeightProperty);
        set => SetValue(MaximumBodyHeightProperty, value);
    }

    /// <summary>The accessible name of the table (<c>aria-label</c>). Name it after what the rows are.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The start inset of an expanded child row's content: 56, or 88 next to a selection column.</summary>
    public double ChildInset
    {
        get => (double)GetValue(ChildInsetProperty);
        set => SetValue(ChildInsetProperty, value);
    }

    /// <summary>Runs <paramref name="change"/> and rebuilds once afterwards instead of once per change.</summary>
    internal void BatchUpdate(Action change)
    {
        _suspend++;
        try
        {
            change();
        }
        finally
        {
            _suspend--;
            if (_suspend == 0 && _dirty) Rebuild();
            else if (_suspend == 0 && _visualsDirty) RequestVisuals();
        }
    }

    /// <summary>The header cell host of a column (for tests and the data table).</summary>
    internal int HeaderCount => _headers.Count;

    /// <summary>The rendered body row count.</summary>
    internal int BodyRowCount => _rowVisuals.Count;

    void OnHeadChanged(CdsTableHead? oldHead, CdsTableHead? newHead)
    {
        if (oldHead is not null)
        {
            oldHead.Changed -= OnHeadStructureChanged;
            foreach (var h in oldHead.HeaderList) Detach(h);
        }
        if (newHead is not null)
        {
            newHead.Changed += OnHeadStructureChanged;
            foreach (var h in newHead.HeaderList) Attach(h);
        }
        Rebuild();
    }

    void OnHeadStructureChanged(object? sender, EventArgs e)
    {
        if (Head is { } head)
        {
            foreach (var h in head.HeaderList)
            {
                h.Changed -= OnPartChanged;
                h.VisualChanged -= OnHeaderVisualChanged;
                Attach(h);
            }
        }
        Rebuild();
    }

    void Attach(CdsTableHeader h)
    {
        h.Changed += OnPartChanged;
        h.VisualChanged += OnHeaderVisualChanged;
    }

    void Detach(CdsTableHeader h)
    {
        h.Changed -= OnPartChanged;
        h.VisualChanged -= OnHeaderVisualChanged;
    }

    void OnRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsTableRow r in e.OldItems) DetachRow(r);
        if (e.NewItems is not null)
            foreach (CdsTableRow r in e.NewItems) AttachRow(r);
        Rebuild();
    }

    void AttachRow(CdsTableRow row)
    {
        row.Changed += OnPartChanged;
        row.VisualChanged += OnRowVisualChanged;
        foreach (var cell in row.CellList) AttachCell(cell);
        row.CellList.CollectionChanged += OnCellsChanged;
    }

    void DetachRow(CdsTableRow row)
    {
        row.Changed -= OnPartChanged;
        row.VisualChanged -= OnRowVisualChanged;
        foreach (var cell in row.CellList) DetachCell(cell);
        row.CellList.CollectionChanged -= OnCellsChanged;
    }

    void AttachCell(CdsTableCell cell)
    {
        cell.Changed += OnPartChanged;
        cell.KeepVisibleChanged += OnKeepVisibleChanged;
    }

    void DetachCell(CdsTableCell cell)
    {
        cell.Changed -= OnPartChanged;
        cell.KeepVisibleChanged -= OnKeepVisibleChanged;
    }

    void OnCellsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsTableCell c in e.OldItems) DetachCell(c);
        if (e.NewItems is not null)
            foreach (CdsTableCell c in e.NewItems) AttachCell(c);
    }

    void OnPartChanged(object? sender, EventArgs e) => Rebuild();

    void OnKeepVisibleChanged(object? sender, EventArgs e) => RequestVisuals();

    void OnHeaderVisualChanged(object? sender, EventArgs e) => RequestVisuals();

    void OnRowVisualChanged(object? sender, EventArgs e)
    {
        if (sender is CdsTableRow row)
        {
            var visual = _rowVisuals.FirstOrDefault(v => ReferenceEquals(v.Source, row));
            if (visual is not null)
            {
                if (row.IsExpandable && visual.ChildHost is null) { Rebuild(); return; }
                if (visual.ChildHost is not null && !ReferenceEquals(visual.ChildHost.Children.OfType<View>().FirstOrDefault(), row.ExpandedContent))
                    SetChildContent(visual, row);
            }
        }
        RequestVisuals();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        if (_building) return;
        var zebra = UseZebraStyles;
        for (var c = 0; c < _headers.Count; c++)
        {
            var h = _headers[c];
            var active = h.Source?.SortDirection is CdsSortDirection.Ascending or CdsSortDirection.Descending;
            var hover = _hoverHeader == c;
            var focus = h.Hit is { IsFocused: true };
            SetColor(h.Fill, BoxView.ColorProperty, CdsTableTokens.HeaderFill(h.Sortable, hover || h.Pressed, active));
            if (h.Label is not null) SetColor(h.Label, CdsText.TextColorProperty, CdsTableTokens.HeaderText);
            if (h.Ring is not null) h.Ring.IsActive = focus && h.Hit!.IsFocusFromKeyboard;
            if (h.Icon is not null && h.Source is not null)
            {
                SetColor(h.Icon, CdsIcon.ColorProperty, CdsTableTokens.SortIcon);
                var direction = h.Source.SortDirection;
                var icon = direction == CdsSortDirection.None ? CdsIconName.ArrowsVertical : CdsIconName.ArrowUp;
                if (h.Icon.Icon != icon) h.Icon.Icon = icon;
                var rotation = direction == CdsSortDirection.Descending ? 180d : 0d;
                if (Math.Abs(h.Icon.Rotation - rotation) > 0.1) Rotate(h.Icon, rotation);
                h.Icon.Opacity = direction != CdsSortDirection.None || hover || focus ? 1 : 0;
            }
            if (h.Hit is not null && h.Source is not null)
            {
                SemanticProperties.SetHint(h.Hit, h.Source.SortDirection switch
                {
                    CdsSortDirection.Ascending => "Sorted ascending",
                    CdsSortDirection.Descending => "Sorted descending",
                    _ => "Not sorted",
                });
            }
        }

        for (var i = 0; i < _rowVisuals.Count; i++)
        {
            var v = _rowVisuals[i];
            var selected = v.Source.IsSelected;
            var hover = _hoverRow == i;
            var stripe = zebra && i % 2 == 1;
            var fill = CdsTableTokens.RowFill(selected, hover, stripe);
            SetColor(v.Fill, BoxView.ColorProperty, fill);
            SetColor(v.Rule, BoxView.ColorProperty, CdsTableTokens.RowRule(selected, hover, zebra, stripe));
            var active = selected || hover;
            foreach (var label in v.Labels) SetColor(label, CdsText.TextColorProperty, CdsTableTokens.CellText(active));

            var expanded = v.ChildHost is not null && v.Source.IsExpanded && v.Source.ExpandedContent is not null;
            if (v.ChildHost is not null)
            {
                v.ChildHost.IsVisible = expanded;
                v.ChildFill!.IsVisible = expanded;
                v.ChildRule!.IsVisible = expanded;
                SetColor(v.ChildFill, BoxView.ColorProperty, fill);
                SetColor(v.ChildRule, BoxView.ColorProperty, CdsTableTokens.RowRule(false, hover, zebra, stripe));
            }
            foreach (var (cell, view) in v.RevealViews)
                view.Opacity = CdsTableTokens.IsOverflowRevealed(true, s_touch, hover, cell.KeepVisible) ? 1 : 0;
        }
    }

    static readonly bool s_touch = CdsTableTokens.IsTouchPlatform;

    void Rotate(CdsIcon icon, double target)
    {
        var from = icon.Rotation;
        _ = CdsMotion.AnimateAsync(icon, "cds-sort-rotate", v => icon.Rotation = v, from, target, CdsDuration.Fast02, CdsEasing.StandardProductive);
    }

    /// <summary>Re-applies the visuals, or remembers to when a batch update is running.</summary>
    void RequestVisuals()
    {
        if (_suspend > 0)
        {
            _visualsDirty = true;
            return;
        }
        _visualsDirty = false;
        RefreshVisuals();
    }

    void UpdateAccessibility() => SemanticProperties.SetDescription(_bodyGrid, Label);

    /// <summary>Rebuilds the whole grid from the head and rows.</summary>
    void Rebuild()
    {
        if (_suspend > 0)
        {
            _dirty = true;
            return;
        }
        _dirty = false;
        _building = true;
        var focusedHeader = _headers.FindIndex(h => h.Hit is { IsFocused: true });
        try
        {
            BuildGrids();
        }
        finally
        {
            _building = false;
        }
        UpdateAccessibility();
        RequestVisuals();
        if (focusedHeader >= 0 && focusedHeader < _headers.Count && _headers[focusedHeader].Hit is { } hit)
            Dispatcher.Dispatch(() => hit.Focus());
    }

    void BuildGrids()
    {
        var sticky = StickyHeader;
        var header = sticky ? _headerGrid : _bodyGrid;
        foreach (var grid in new[] { _bodyGrid, _headerGrid })
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
        }
        _headers.Clear();
        _rowVisuals.Clear();
        _hoverRow = -1;
        _hoverHeader = -1;

        var head = Head?.HeaderList;
        var rows = _rows;
        var columnCount = Math.Max(head?.Count ?? 0, rows.Count == 0 ? 0 : rows.Max(r => r.CellList.Count));
        _minimumWidth = 0;
        for (var c = 0; c < columnCount; c++)
        {
            var width = head is not null && c < head.Count ? head[c].Width : GridLength.Star;
            if (sticky && width.IsAuto) width = GridLength.Star;
            _bodyGrid.ColumnDefinitions.Add(new ColumnDefinition(width));
            if (sticky) _headerGrid.ColumnDefinitions.Add(new ColumnDefinition(width));
            _minimumWidth += width.IsAbsolute ? width.Value : CdsTableTokens.MinimumFlexibleColumnWidth;
        }
        var span = Math.Max(1, columnCount);
        var height = CdsTableTokens.RowHeight(Size);
        var (padTop, padBottom) = CdsTableTokens.CellPaddingY(Size);
        var topAligned = Size >= CdsSize.Xl;

        // Header row.
        var bodyRow = 0;
        if (head is { Count: > 0 })
        {
            header.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (var c = 0; c < columnCount; c++)
                BuildHeaderCell(header, c < head.Count ? head[c] : null, c, height, padTop, padBottom, topAligned);
            if (!sticky) bodyRow = 1;
        }

        // Body rows: [fill] [rule] [cell hosts]; an expandable row adds a child row [fill] [rule] [content].
        for (var r = 0; r < rows.Count; r++)
        {
            var source = rows[r];
            var visual = new RowVisual { Source = source, Index = r };
            _rowVisuals.Add(visual);

            _bodyGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var gridRow = bodyRow++;

            visual.Fill = new BoxView { MinimumHeightRequest = height };
            Hide(visual.Fill);
            _bodyGrid.Add(visual.Fill, 0, gridRow);
            Grid.SetColumnSpan(visual.Fill, span);
            WireRowHover(visual.Fill, r);

            visual.Rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
            Hide(visual.Rule);
            _bodyGrid.Add(visual.Rule, 0, gridRow);
            Grid.SetColumnSpan(visual.Rule, span);

            for (var c = 0; c < columnCount; c++)
            {
                var cell = c < source.CellList.Count ? source.CellList[c] : null;
                var headerAlign = head is not null && c < head.Count ? head[c].Align : CdsTableAlignment.Start;
                var align = cell?.Align ?? headerAlign;
                var flush = cell?.IsFlush == true;
                var host = new Grid { BackgroundColor = Colors.Transparent, MinimumHeightRequest = height };
                View? content = null;
                if (cell?.Body is { } body)
                {
                    content = body;
                    body.VerticalOptions = flush ? body.VerticalOptions : topAligned ? LayoutOptions.Start : LayoutOptions.Center;
                    if (!flush)
                    {
                        body.HorizontalOptions = align == CdsTableAlignment.End ? LayoutOptions.End : LayoutOptions.Start;
                        host.Padding = new Thickness(CdsTableTokens.CellPaddingX, padTop, CdsTableTokens.CellPaddingX, padBottom);
                    }
                    if (cell.RevealOnHover) visual.RevealViews.Add((cell, body));
                }
                else if (!string.IsNullOrEmpty(cell?.Text))
                {
                    var label = new CdsText(cell!.Text!, CdsTypeStyle.BodyCompact01, CdsTextTone.Secondary)
                    {
                        HorizontalTextAlignment = align == CdsTableAlignment.End ? TextAlignment.End : TextAlignment.Start,
                        VerticalOptions = topAligned ? LayoutOptions.Start : LayoutOptions.Center,
                        InputTransparent = true,
                    };
                    visual.Labels.Add(label);
                    content = label;
                    host.Padding = new Thickness(CdsTableTokens.CellPaddingX, padTop, CdsTableTokens.CellPaddingX, padBottom);
                }
                if (content is not null)
                {
                    // A view that already lives elsewhere (rebuilt rows reuse cell views) is detached first.
                    if (content.Parent is Layout old) old.Remove(content);
                    host.Add(content);
                }
                WireRowHover(host, r);
                _bodyGrid.Add(host, c, gridRow);
            }

            if (source.IsExpandable)
            {
                _bodyGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                var childRow = bodyRow++;
                visual.ChildFill = new BoxView { IsVisible = false };
                Hide(visual.ChildFill);
                _bodyGrid.Add(visual.ChildFill, 0, childRow);
                Grid.SetColumnSpan(visual.ChildFill, span);
                visual.ChildRule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
                Hide(visual.ChildRule);
                _bodyGrid.Add(visual.ChildRule, 0, childRow);
                Grid.SetColumnSpan(visual.ChildRule, span);
                visual.ChildHost = new Grid
                {
                    IsVisible = false,
                    Padding = new Thickness(ChildInset, CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing06),
                };
                _bodyGrid.Add(visual.ChildHost, 0, childRow);
                Grid.SetColumnSpan(visual.ChildHost, span);
                SetChildContent(visual, source);
                WireRowHover(visual.ChildFill, r);
            }
        }

        // The table scrolls sideways when its columns need more than the available width (Carbon's overflow-x: auto);
        // otherwise its content is exactly as wide as the table, so star columns share the space.
        if (sticky)
        {
            _scroll.Content = _bodyGrid;
            _scroll.MaximumHeightRequest = MaximumBodyHeight;
            _hscroll.Content = _stickyRoot;
        }
        else
        {
            _scroll.Content = null;
            _hscroll.Content = _bodyGrid;
        }
        if (Content != _hscroll) SetChrome(_hscroll);
        SyncContentWidth();
    }

    /// <summary>The content width: the table's width, or the columns' minimum when that is wider (it then scrolls).</summary>
    internal static double ContentWidth(double viewport, double minimum) =>
        viewport > 0 ? Math.Max(viewport, minimum) : minimum > 0 ? minimum : -1;

    void SyncContentWidth()
    {
        if (_hscroll.Content is not View content) return;
        var width = ContentWidth(_hscroll.Width, _minimumWidth);
        if (Math.Abs(content.WidthRequest - width) > 0.5) content.WidthRequest = width;
    }

    static void Hide(View view)
    {
        AutomationProperties.SetIsInAccessibleTree(view, false);
    }

    void SetChildContent(RowVisual visual, CdsTableRow source)
    {
        var host = visual.ChildHost!;
        host.Children.Clear();
        if (source.ExpandedContent is { } content)
        {
            if (content.Parent is Layout old) old.Remove(content);
            host.Add(content);
        }
    }

    void BuildHeaderCell(Grid grid, CdsTableHeader? header, int column, double height, double padTop, double padBottom, bool topAligned)
    {
        var visual = new HeaderVisual { Source = header };
        _headers.Add(visual);
        var sortable = header is not null && header.Body is null && (header.IsSortable ?? IsSortable);
        visual.Sortable = sortable;
        var align = header?.Align ?? CdsTableAlignment.Start;

        visual.Fill = new BoxView { MinimumHeightRequest = height };
        Hide(visual.Fill);
        grid.Add(visual.Fill, column, 0);

        var host = new Grid { BackgroundColor = Colors.Transparent };
        grid.Add(host, column, 0);

        if (header?.Body is { } body)
        {
            if (body.Parent is Layout old) old.Remove(body);
            body.VerticalOptions = LayoutOptions.Center;
            if (!header.IsFlush) host.Padding = new Thickness(CdsTableTokens.CellPaddingX, 0);
            host.Add(body);
            SetHeaderName(header);
        }
        else if (!sortable)
        {
            visual.Label = new CdsText(header?.Text ?? string.Empty, CdsTypeStyle.HeadingCompact01)
            {
                HorizontalTextAlignment = align == CdsTableAlignment.End ? TextAlignment.End : TextAlignment.Start,
                VerticalOptions = topAligned ? LayoutOptions.Start : LayoutOptions.Center,
                Padding = new Thickness(CdsTableTokens.CellPaddingX, padTop, CdsTableTokens.CellPaddingX, padBottom),
                InputTransparent = true,
            };
            host.Add(visual.Label);
        }
        else
        {
            visual.Label = new CdsText(header!.Text ?? string.Empty, CdsTypeStyle.HeadingCompact01)
            {
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                InputTransparent = true,
            };
            visual.Icon = new CdsIcon(CdsIconName.ArrowsVertical, 16)
            {
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(CdsSpacing.Spacing03 + 2, 0),
                InputTransparent = true,
                Opacity = 0,
            };
            var layout = new Grid { ColumnSpacing = 0, InputTransparent = true };
            if (align == CdsTableAlignment.End)
            {
                // Label on the end edge, the arrow before it.
                layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                layout.Add(visual.Icon, 1);
                layout.Add(visual.Label, 2);
                layout.Padding = new Thickness(0, 0, CdsTableTokens.CellPaddingX, 0);
            }
            else
            {
                layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                layout.Add(visual.Label, 0);
                layout.Add(visual.Icon, 1);
                layout.Padding = new Thickness(CdsTableTokens.CellPaddingX, 0, 0, 0);
            }
            host.Add(layout);
            AutomationProperties.SetExcludedWithChildren(layout, true);

            visual.Ring = new CdsFocusRing(CdsFocusRingStyle.Inset);
            host.Add(visual.Ring);
            visual.Hit = new CdsHitTarget { Role = CdsAccessibilityRole.Button };
            SemanticProperties.SetDescription(visual.Hit, header.Description ?? $"Sort by {header.Text}");
            var index = _headers.Count - 1;
            visual.Hit.Clicked += (_, _) =>
            {
                header.RaiseClicked();
                HeaderClicked?.Invoke(this, new CdsTableHeaderEventArgs(header));
            };
            visual.Hit.Pressed += (_, _) => { visual.Pressed = true; RequestVisuals(); };
            visual.Hit.Released += (_, _) => { visual.Pressed = false; RequestVisuals(); };
            visual.Hit.Focused += (_, _) => RequestVisuals();
            visual.Hit.Unfocused += (_, _) => RequestVisuals();
            host.Add(visual.Hit);
            var pointer = new PointerGestureRecognizer();
            pointer.PointerEntered += (_, _) => { _hoverHeader = index; RequestVisuals(); };
            pointer.PointerExited += (_, _) => { if (_hoverHeader == index) _hoverHeader = -1; RequestVisuals(); };
            host.GestureRecognizers.Add(pointer);
        }
        if (header is not null && header.Body is null && !sortable) SetHeaderName(header);
    }

    static void SetHeaderName(CdsTableHeader header)
    {
        // Non-sortable headers are plain text; a hidden-text header carries its name for screen readers.
        if (header.Body is { } body) SemanticProperties.SetDescription(body, header.Description ?? header.Text);
    }

    void WireRowHover(View view, int row)
    {
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) =>
        {
            _hoverVersion++;
            if (_hoverRow == row) return;
            _hoverRow = row;
            RequestVisuals();
        };
        pointer.PointerExited += (_, _) =>
        {
            // Moving between cells fires an exit then an enter: only clear when no enter follows.
            var version = ++_hoverVersion;
            Dispatcher.Dispatch(() =>
            {
                if (version != _hoverVersion || _hoverRow != row) return;
                _hoverRow = -1;
                RequestVisuals();
            });
        };
        view.GestureRecognizers.Add(pointer);
    }

    sealed class HeaderVisual
    {
        public CdsTableHeader? Source;
        public BoxView Fill = null!;
        public CdsText? Label;
        public CdsIcon? Icon;
        public CdsFocusRing? Ring;
        public CdsHitTarget? Hit;
        public bool Sortable;
        public bool Pressed;
    }

    sealed class RowVisual
    {
        public CdsTableRow Source = null!;
        public int Index;
        public BoxView Fill = null!;
        public BoxView Rule = null!;
        public BoxView? ChildFill;
        public BoxView? ChildRule;
        public Grid? ChildHost;
        public readonly List<CdsText> Labels = [];
        public readonly List<(CdsTableCell Cell, View View)> RevealViews = [];
    }
}

/// <summary>Event data of <see cref="CdsTable.HeaderClicked"/>.</summary>
public sealed class CdsTableHeaderEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsTableHeaderEventArgs(CdsTableHeader header) => Header = header;

    /// <summary>The header that was activated.</summary>
    public CdsTableHeader Header { get; }
}
