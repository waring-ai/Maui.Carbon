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
/// <see cref="CdsDataTable"/> when it should. Hand-built rows are all real views: keep to about 200 and put a
/// <see cref="CdsPagination"/> under longer data (a <see cref="CdsDataTable"/> with a sticky header only builds the rows in view).
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
    readonly HashSet<CdsTableRow> _attached = [];
    readonly Dictionary<CdsTableRow, RowVisual> _visualByRow = [];
    readonly Dictionary<CdsTableCell, CdsText> _cellLabels = [];
    readonly HashSet<BindableObject> _forget = [];
    readonly Stack<RowVisual> _parked = new();
    readonly List<GridLength> _columns = [];
    double _rowHeight;
    double _padTop;
    double _padBottom;
    bool _topAligned;
    int _nextGridRow;
    bool _built;

    // Virtualized body (a row source with a sticky header): a pool of row slots positioned in an absolute layout.
    const int Overscan = 6;
    readonly AbsoluteLayout _virtualBody = new();
    string? _accessibleHint;
    readonly Dictionary<View, RowVisual> _slotByRoot = [];
    readonly Dictionary<object, double> _childHeights = [];
    ICdsTableRowSource? _source;
    ICdsTableRowSource? _poolSource;
    double[] _offsets = [0];
    double _childEstimate;
    bool _binding;
    bool _relayoutQueued;
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
        CdsTokens.SetColor(_virtualBody, BackgroundColorProperty, CdsTableTokens.Background);
        // The sticky header and its body are one table surface: nothing of the page shows between or beside them.
        CdsTokens.SetColor(_stickyRoot, BackgroundColorProperty, CdsTableTokens.Background);
        _scroll.Scrolled += (_, _) => { if (IsVirtual) UpdateWindow(); };
        _scroll.SizeChanged += (_, _) => { if (IsVirtual) UpdateWindow(); };
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

    /// <summary>The body row count (with a row source, the source's count, whether or not the rows are realized).</summary>
    internal int BodyRowCount => IsVirtual ? _source!.Count : _rowVisuals.Count;

    /// <summary>The body rows that have views (all of them, unless the table is virtualized).</summary>
    internal int RealizedRowCount => _rowVisuals.Count(v => v.Index >= 0);

    /// <summary>The realized rows by data index (for tests).</summary>
    internal IReadOnlyList<(int Index, CdsTableRow Row)> RealizedRows =>
        _rowVisuals.Where(v => v.Index >= 0).OrderBy(v => v.Index).Select(v => (v.Index, v.Source)).ToList();

    /// <summary>
    /// Rows supplied on demand. With <see cref="StickyHeader"/> on, only the rows in (and near) view are realized, in a
    /// pool of rows the source re-binds as the body scrolls; <see cref="Rows"/> is then ignored.
    /// </summary>
    internal ICdsTableRowSource? RowSource
    {
        get => _source;
        set
        {
            if (ReferenceEquals(_source, value)) return;
            _source = value;
            _childHeights.Clear();
            Rebuild();
        }
    }

    /// <summary>Whether the body is virtualized: a row source and a sticky header (the table owns the scrolling).</summary>
    internal bool IsVirtual => _source is not null && StickyHeader;

    /// <summary>The source's rows changed (count, order or content): re-binds the realized rows.</summary>
    internal void InvalidateRows()
    {
        if (!IsVirtual || !_built) return;
        if (_dirty) return; // a pending rebuild re-binds everything
        var count = _source!.Count;
        _binding = true;
        try
        {
            foreach (var v in _rowVisuals)
            {
                if (v.Index < 0) continue;
                if (v.Index >= count) Unbind(v);
                else Bind(v, v.Index);
            }
        }
        finally
        {
            _binding = false;
        }
        ComputeOffsets();
        UpdateWindow(force: true);
    }

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
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // Reset (Clear) carries no old items: detach what was attached, by our own record of it.
            foreach (var r in _attached) DetachRow(r);
            _attached.Clear();
            foreach (var r in _rows)
                if (_attached.Add(r)) AttachRow(r);
        }
        else
        {
            if (e.OldItems is not null)
                foreach (CdsTableRow r in e.OldItems)
                    if (_attached.Remove(r)) DetachRow(r);
            if (e.NewItems is not null)
                foreach (CdsTableRow r in e.NewItems)
                    if (_attached.Add(r)) AttachRow(r);
        }

        if (CanUpdateIncrementally(e))
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                for (var i = e.NewStartingIndex; i < _rows.Count; i++) AppendRowVisual(_rows[i], i);
            }
            else
            {
                while (_rowVisuals.Count > _rows.Count) ParkLastRowVisual();
            }
            UpdateAccessibility();
            RequestVisuals();
            return;
        }
        Rebuild();
    }

    /// <summary>Rows added or removed at the end of a built, non-virtual table only add or remove their own views.</summary>
    bool CanUpdateIncrementally(NotifyCollectionChangedEventArgs e)
    {
        if (!_built || _dirty || _building || IsVirtual) return false;
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is { } added)
        {
            if (e.NewStartingIndex != _rowVisuals.Count || e.NewStartingIndex + added.Count != _rows.Count) return false;
            foreach (CdsTableRow r in added)
                if (r.CellList.Count > _columns.Count) return false; // the row adds columns
            return _columns.Count > 0;
        }
        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is { } removed)
            return e.OldStartingIndex == _rows.Count && _rowVisuals.Count == _rows.Count + removed.Count;
        return false;
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
        cell.TextChanged += OnCellTextChanged;
        cell.KeepVisibleChanged += OnKeepVisibleChanged;
    }

    void DetachCell(CdsTableCell cell)
    {
        cell.Changed -= OnPartChanged;
        cell.TextChanged -= OnCellTextChanged;
        cell.KeepVisibleChanged -= OnKeepVisibleChanged;
    }

    void OnCellTextChanged(object? sender, EventArgs e)
    {
        // A text cell's label is updated in place; a cell drawn without one (a body, or not built yet) rebuilds.
        if (sender is CdsTableCell cell && _cellLabels.TryGetValue(cell, out var label))
        {
            SetCellText(label, cell.Text);
            return;
        }
        if (_building || _binding) return;
        Rebuild();
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
        if (sender is CdsTableRow row && _visualByRow.TryGetValue(row, out var visual))
        {
            if (row.IsExpandable && visual.ChildHost is null) { Rebuild(); return; }
            if (visual.ChildHost is not null && !ReferenceEquals(visual.ChildHost.Children.OfType<View>().FirstOrDefault(), row.ExpandedContent))
                SetChildContent(visual, row);
            if (visual.Root is not null && visual.Index >= 0 && visual.Expanded != row.IsExpanded)
            {
                // An expanded or collapsed row moves the rows under it.
                visual.Expanded = row.IsExpanded;
                if (!_binding)
                {
                    ComputeOffsets();
                    UpdateWindow(force: true);
                    return;
                }
            }
        }
        if (_binding) return;
        RequestVisuals();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        if (_building) return;
        if (_forget.Count > 0)
        {
            ForgetColors(_forget);
            _forget.Clear();
        }
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

        foreach (var v in _rowVisuals)
        {
            if (v.Index < 0) continue;
            var selected = v.Source.IsSelected;
            var hover = _hoverRow == v.Index;
            var stripe = zebra && v.Index % 2 == 1;
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

    /// <summary>
    /// Extra information about the table for assistive technology (the data table's description). Kept as a group hint on
    /// the body: a Hint on the body (like a Description) would make it one VoiceOver element and hide every row.
    /// </summary>
    internal string? AccessibleHint
    {
        get => _accessibleHint;
        set
        {
            if (_accessibleHint == value) return;
            _accessibleHint = value;
            UpdateAccessibility();
        }
    }

    void UpdateAccessibility()
    {
        // Group semantics, not a Description: the rows, their checkboxes and expand buttons stay reachable.
        CdsSemantics.SetGroup(_bodyGrid, Label, hint: _accessibleHint);
        // Only the rows in view exist, so say how many there are.
        var count = IsVirtual ? (_source!.Count == 1 ? "1 row" : $"{_source.Count} rows") : null;
        var hint = string.IsNullOrEmpty(_accessibleHint) ? count : count is null ? _accessibleHint : $"{_accessibleHint}. {count}";
        CdsSemantics.SetGroup(_virtualBody, Label, hint: hint);
    }

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
        var virtualized = IsVirtual;
        var header = sticky ? _headerGrid : _bodyGrid;
        ReleaseSlots();
        foreach (var grid in new[] { _bodyGrid, _headerGrid })
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
        }
        _virtualBody.Children.Clear();
        _headers.Clear();
        _rowVisuals.Clear();
        _visualByRow.Clear();
        _cellLabels.Clear();
        _columns.Clear();
        _forget.Clear();
        _parked.Clear();
        ForgetColors();
        _hoverRow = -1;
        _hoverHeader = -1;

        var head = Head?.HeaderList;
        var rows = _rows;
        var rowColumns = virtualized || rows.Count == 0 ? 0 : rows.Max(r => r.CellList.Count);
        var columnCount = Math.Max(head?.Count ?? 0, rowColumns);
        _minimumWidth = 0;
        var widths = new GridLength[columnCount];
        var flush = new bool[columnCount];
        for (var c = 0; c < columnCount; c++)
        {
            var width = head is not null && c < head.Count ? head[c].Width : GridLength.Star;
            if (sticky && width.IsAuto) width = GridLength.Star;
            widths[c] = width;
            flush[c] = head is not null && c < head.Count && head[c].IsFlush;
            _minimumWidth += width.IsAbsolute ? width.Value : CdsTableTokens.MinimumFlexibleColumnWidth;
        }
        foreach (var width in FillColumns(widths, flush))
        {
            _columns.Add(width);
            _bodyGrid.ColumnDefinitions.Add(new ColumnDefinition(width));
            if (sticky) _headerGrid.ColumnDefinitions.Add(new ColumnDefinition(width));
        }
        _rowHeight = CdsTableTokens.RowHeight(Size);
        (_padTop, _padBottom) = CdsTableTokens.CellPaddingY(Size);
        _topAligned = Size >= CdsSize.Xl;
        if (_childEstimate <= 0) _childEstimate = _rowHeight * 3;

        // Header row.
        _nextGridRow = 0;
        if (head is { Count: > 0 })
        {
            header.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (var c = 0; c < columnCount; c++)
                BuildHeaderCell(header, c < head.Count ? head[c] : null, c, _rowHeight, _padTop, _padBottom, _topAligned);
            if (!sticky) _nextGridRow = 1;
        }

        if (!virtualized)
        {
            // One change to the row definitions, not one per row (each change makes the grid walk all of them).
            var gridRows = _nextGridRow + rows.Sum(r => r.IsExpandable ? 2 : 1);
            if (gridRows > _bodyGrid.RowDefinitions.Count)
                _bodyGrid.RowDefinitions = new RowDefinitionCollection(Enumerable.Range(0, gridRows).Select(_ => new RowDefinition(GridLength.Auto)).ToArray());
            for (var r = 0; r < rows.Count; r++) AppendRowVisual(rows[r], r);
        }

        // The table scrolls sideways when its columns need more than the available width (Carbon's overflow-x: auto);
        // otherwise its content is exactly as wide as the table, so star columns share the space.
        if (sticky)
        {
            _scroll.Content = virtualized ? _virtualBody : _bodyGrid;
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
        _built = true;

        if (virtualized)
        {
            ComputeOffsets();
            UpdateWindow(force: true, applyVisuals: false);
        }
    }

    /// <summary>Adds a body row (and its child row, when expandable) at the end of the shared body grid.</summary>
    void AppendRowVisual(CdsTableRow source, int index)
    {
        if (_parked.Count > 0)
        {
            var parked = _parked.Peek();
            if (ReferenceEquals(parked.Source, source) && parked.FirstGridRow == _nextGridRow && parked.Matches(source))
            {
                _parked.Pop();
                Revive(parked, index);
                return;
            }
            DiscardParked();
        }
        var visual = new RowVisual { Source = source, Index = index, FirstGridRow = _nextGridRow, GridRows = source.IsExpandable ? 2 : 1 };
        while (_bodyGrid.RowDefinitions.Count < _nextGridRow + visual.GridRows) _bodyGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _nextGridRow += visual.GridRows;
        BuildRowVisual(_bodyGrid, visual.FirstGridRow, visual, fixedHeight: false);
        visual.Snapshot();
        _rowVisuals.Add(visual);
        _visualByRow[source] = visual;
    }

    /// <summary>
    /// Takes the last body row out of the table by hiding its views, not removing them: MAUI removes a child with a linear
    /// search of the parent's children, so emptying a large grid row by row is quadratic. The row's views come back if
    /// the same row is added at the same place again (the data table re-adds the rows it took off when a search widens).
    /// </summary>
    void ParkLastRowVisual()
    {
        var visual = _rowVisuals[^1];
        _rowVisuals.RemoveAt(_rowVisuals.Count - 1);
        _visualByRow.Remove(visual.Source);
        foreach (var view in visual.Views) view.IsVisible = false;
        _nextGridRow -= visual.GridRows;
        _parked.Push(visual);
        if (_hoverRow >= _rowVisuals.Count) _hoverRow = -1;
    }

    void Revive(RowVisual visual, int index)
    {
        visual.Index = index;
        _nextGridRow += visual.GridRows;
        foreach (var view in visual.Views)
        {
            // The child row's views are shown by ApplyVisuals when the row is expanded.
            var child = view == visual.ChildHost || view == visual.ChildFill || view == visual.ChildRule;
            view.IsVisible = !child;
        }
        // The cells were detached while parked: catch up with their text.
        foreach (var cell in visual.TextCells)
            if (_cellLabels.TryGetValue(cell, out var label)) SetCellText(label, cell.Text);
        if (visual.ChildHost is not null) SetChildContent(visual, visual.Source);
        _rowVisuals.Add(visual);
        _visualByRow[visual.Source] = visual;
    }

    /// <summary>Removes the parked rows' views (a different row was added where they would come back).</summary>
    void DiscardParked()
    {
        while (_parked.Count > 0)
        {
            var visual = _parked.Pop();
            foreach (var cell in visual.TextCells) _cellLabels.Remove(cell);
            foreach (var view in visual.Views) _bodyGrid.Remove(view);
            _forget.UnionWith(visual.Views);
            _forget.UnionWith(visual.Labels);
        }
    }

    /// <summary>
    /// Builds a row's views into <paramref name="grid"/> at <paramref name="gridRow"/>: [fill] [rule] [cell hosts]; an
    /// expandable row adds a child row [fill] [rule] [content] under it.
    /// </summary>
    void BuildRowVisual(Grid grid, int gridRow, RowVisual visual, bool fixedHeight)
    {
        var source = visual.Source;
        var head = Head?.HeaderList;
        var columnCount = _columns.Count;
        var span = Math.Max(1, columnCount);
        var height = _rowHeight;

        void Add(View view, int column, int row, int columnSpan = 1)
        {
            grid.Add(view, column, row);
            if (columnSpan > 1) Grid.SetColumnSpan(view, columnSpan);
            visual.Views.Add(view);
        }

        visual.Fill = new BoxView { MinimumHeightRequest = height };
        Hide(visual.Fill);
        Add(visual.Fill, 0, gridRow, span);
        WireRowHover(visual.Fill, visual);

        visual.Rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        Hide(visual.Rule);
        Add(visual.Rule, 0, gridRow, span);

        for (var c = 0; c < columnCount; c++)
        {
            var cell = c < source.CellList.Count ? source.CellList[c] : null;
            var headerAlign = head is not null && c < head.Count ? head[c].Align : CdsTableAlignment.Start;
            var columnName = head is not null && c < head.Count && !head[c].IsFlush ? head[c].Text : null;
            var align = cell?.Align ?? headerAlign;
            var flush = cell?.IsFlush == true;
            var host = new Grid { BackgroundColor = Colors.Transparent, MinimumHeightRequest = height };
            View? content = null;
            if (cell?.Body is { } body)
            {
                content = body;
                body.VerticalOptions = flush ? body.VerticalOptions : _topAligned ? LayoutOptions.Start : LayoutOptions.Center;
                if (!flush)
                {
                    body.HorizontalOptions = align == CdsTableAlignment.End ? LayoutOptions.End : LayoutOptions.Start;
                    host.Padding = new Thickness(CdsTableTokens.CellPaddingX, _padTop, CdsTableTokens.CellPaddingX, _padBottom);
                }
                if (cell.RevealOnHover) visual.RevealViews.Add((cell, body));
                // A custom cell is a group named by its column, so a screen reader entering it hears the header first.
                CdsSemantics.SetGroupName(host, columnName);
            }
            else if (cell is not null)
            {
                // Every text cell gets a label, even an empty one, so a later text change updates it in place.
                var label = new CdsText(cell.Text ?? string.Empty, CdsTypeStyle.BodyCompact01, CdsTextTone.Secondary)
                {
                    HorizontalTextAlignment = align == CdsTableAlignment.End ? TextAlignment.End : TextAlignment.Start,
                    VerticalOptions = _topAligned ? LayoutOptions.Start : LayoutOptions.Center,
                    InputTransparent = true,
                };
                if (fixedHeight) label.LineBreakMode = LineBreakMode.TailTruncation;
                label.SetValue(ColumnNameProperty, columnName);
                SetCellText(label, cell.Text);
                visual.Labels.Add(label);
                visual.TextCells.Add(cell);
                _cellLabels[cell] = label;
                content = label;
                host.Padding = new Thickness(CdsTableTokens.CellPaddingX, _padTop, CdsTableTokens.CellPaddingX, _padBottom);
            }
            if (content is not null)
            {
                // A view that already lives elsewhere (rebuilt rows reuse cell views) is detached first.
                if (content.Parent is Layout old) old.Remove(content);
                host.Add(content);
            }
            WireRowHover(host, visual);
            Add(host, c, gridRow);
        }

        if (source.IsExpandable)
        {
            var childRow = gridRow + 1;
            visual.ChildFill = new BoxView { IsVisible = false };
            Hide(visual.ChildFill);
            Add(visual.ChildFill, 0, childRow, span);
            visual.ChildRule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
            Hide(visual.ChildRule);
            Add(visual.ChildRule, 0, childRow, span);
            visual.ChildHost = new Grid
            {
                IsVisible = false,
                Padding = new Thickness(ChildInset, CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing06),
            };
            Add(visual.ChildHost, 0, childRow, span);
            SetChildContent(visual, source);
            WireRowHover(visual.ChildFill, visual);
        }
    }

    // ----- virtualized body -----

    /// <summary>Hands every pooled row back to the source that made it.</summary>
    void ReleaseSlots()
    {
        foreach (var v in _rowVisuals)
        {
            if (v.Root is null) continue;
            DetachRow(v.Source);
            _poolSource?.ReleaseRow(v.Source);
        }
        _slotByRoot.Clear();
        _poolSource = null;
    }

    /// <summary>The top of every row (and the body height), from the row height and the expanded rows' child heights.</summary>
    void ComputeOffsets()
    {
        if (_source is null) return;
        var count = _source.Count;
        if (_offsets.Length != count + 1) _offsets = new double[count + 1];
        _offsets[0] = 0;
        for (var i = 0; i < count; i++)
            _offsets[i + 1] = _offsets[i] + _rowHeight + (_source.IsExpanded(i) ? ChildHeight(i) : 0);
        var total = _offsets[count];
        if (Math.Abs(_virtualBody.HeightRequest - total) > 0.5) _virtualBody.HeightRequest = total;
    }

    double ChildHeight(int index) => _childHeights.TryGetValue(_source!.KeyOf(index), out var h) ? h : _childEstimate;

    /// <summary>The row at vertical position <paramref name="y"/> of the body.</summary>
    internal static int IndexAt(double[] offsets, int count, double y)
    {
        if (count == 0) return -1;
        int lo = 0, hi = count - 1;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) / 2;
            if (offsets[mid] <= y) lo = mid;
            else hi = mid - 1;
        }
        return lo;
    }

    /// <summary>The data rows to realize for a viewport: those it shows, plus <see cref="Overscan"/> on each side.</summary>
    internal static (int First, int Last) RowWindow(double[] offsets, int count, double top, double viewport, int overscan)
    {
        if (count == 0) return (0, -1);
        var first = Math.Max(0, IndexAt(offsets, count, top) - overscan);
        var last = Math.Min(count - 1, IndexAt(offsets, count, top + Math.Max(viewport, 1)) + overscan);
        return (first, last);
    }

    /// <summary>Realizes the rows in view: frees the slots that scrolled away, binds free (or new) slots to the rows that came in.</summary>
    void UpdateWindow(bool force = false, bool applyVisuals = true, double? top = null)
    {
        if (!IsVirtual || !_built || _building && !force) return;
        var source = _source!;
        var count = source.Count;
        var viewport = _scroll.Height > 0 ? _scroll.Height : MaximumBodyHeight;
        var (first, last) = RowWindow(_offsets, count, Math.Max(0, top ?? _scroll.ScrollY), viewport, Overscan);
        var changed = false;

        var bound = new Dictionary<int, RowVisual>();
        var free = new Stack<RowVisual>();
        foreach (var v in _rowVisuals)
        {
            if (v.Index >= count || v.Index >= 0 && (v.Index < first || v.Index > last) && !IsBusy(v))
            {
                Unbind(v);
                changed = true;
            }
            if (v.Index >= 0) bound[v.Index] = v;
            else free.Push(v);
        }

        _binding = true;
        try
        {
            for (var i = first; i <= last; i++)
            {
                if (bound.ContainsKey(i)) continue;
                var slot = free.Count > 0 ? free.Pop() : CreateSlot();
                Bind(slot, i);
                Order(slot);
                bound[i] = slot;
                changed = true;
            }
        }
        finally
        {
            _binding = false;
        }

        foreach (var (index, v) in bound) Place(v, _offsets[index]);
        if (_hoverRow >= 0 && !bound.ContainsKey(_hoverRow)) _hoverRow = -1;
        UpdateAccessibility();
        if (changed && applyVisuals) RequestVisuals();
    }

    /// <summary>Realizes the rows for a body scrolled to <paramref name="top"/>, as scrolling there would (tests have no native scroll view).</summary>
    internal void RealizeAt(double top) => UpdateWindow(force: true, top: top);

    /// <summary>The top of data row <paramref name="index"/> in the virtualized body.</summary>
    internal double RowTop(int index) => _offsets[index];

    /// <summary>A row with focus, or an open or focused menu, is kept while it is out of view (recycling it would move focus).</summary>
    static bool IsBusy(RowVisual v)
    {
        foreach (var cell in v.Source.CellList)
            if (cell.KeepVisible) return true;
        return v.Root!.GetVisualTreeDescendants().OfType<VisualElement>().Any(e => e.IsFocused);
    }

    RowVisual CreateSlot()
    {
        var source = _source!;
        _poolSource = source;
        var row = source.CreateRow();
        AttachRow(row);
        var root = new Grid { RowSpacing = 0, ColumnSpacing = 0 };
        foreach (var width in _columns) root.ColumnDefinitions.Add(new ColumnDefinition(width));
        root.RowDefinitions.Add(new RowDefinition(new GridLength(_rowHeight)));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        var visual = new RowVisual { Source = row, Index = -1, Root = root, GridRows = 2 };
        BuildRowVisual(root, 0, visual, fixedHeight: true);
        AbsoluteLayout.SetLayoutFlags(root, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.WidthProportional);
        root.SizeChanged += (_, _) => OnSlotSized(visual);
        _virtualBody.Add(root);
        _rowVisuals.Add(visual);
        _visualByRow[row] = visual;
        _slotByRoot[root] = visual;
        return visual;
    }

    void Bind(RowVisual slot, int index)
    {
        var wasBinding = _binding;
        _binding = true;
        try
        {
            slot.Index = index;
            _source!.BindRow(slot.Source, index);
            slot.Expanded = slot.Source.IsExpanded;
        }
        finally
        {
            _binding = wasBinding;
        }
        if (!slot.Root!.IsVisible) slot.Root.IsVisible = true;
    }

    static void Unbind(RowVisual slot)
    {
        slot.Index = -1;
        slot.Top = double.NaN;
        if (slot.Root!.IsVisible) slot.Root.IsVisible = false;
    }

    static void Place(RowVisual slot, double top)
    {
        if (slot.Top == top) return;
        slot.Top = top;
        AbsoluteLayout.SetLayoutBounds(slot.Root!, new Rect(0, top, 1, AbsoluteLayout.AutoSize));
    }

    /// <summary>Keeps the bound slots in row order among the body's children, so Tab and screen readers go top to bottom.</summary>
    void Order(RowVisual slot)
    {
        var children = _virtualBody.Children;
        var at = children.IndexOf(slot.Root!);
        RowVisual? Bound(int k) => _slotByRoot.TryGetValue((View)children[k], out var v) && v.Index >= 0 && v != slot ? v : null;
        int? Before()
        {
            for (var k = at - 1; k >= 0; k--) if (Bound(k) is { } v) return v.Index;
            return null;
        }
        int? After()
        {
            for (var k = at + 1; k < children.Count; k++) if (Bound(k) is { } v) return v.Index;
            return null;
        }
        if ((Before() is not { } b || b < slot.Index) && (After() is not { } a || a > slot.Index)) return;

        _virtualBody.Children.Remove(slot.Root!);
        var target = children.Count;
        for (var k = 0; k < children.Count; k++)
        {
            if (Bound(k) is { } v && v.Index > slot.Index)
            {
                target = k;
                break;
            }
        }
        _virtualBody.Children.Insert(target, slot.Root!);
    }

    /// <summary>A realized expanded row reports its child row's real height; the rows under it move when it differs from the estimate.</summary>
    void OnSlotSized(RowVisual slot)
    {
        if (!IsVirtual || slot.Index < 0 || slot.Root!.Height <= 0) return;
        if (!_source!.IsExpanded(slot.Index) || !slot.Source.IsExpanded) return;
        var child = Math.Max(0, slot.Root.Height - _rowHeight);
        var key = _source.KeyOf(slot.Index);
        var assumed = _childHeights.TryGetValue(key, out var known) ? known : _childEstimate;
        _childHeights[key] = child;
        if (Math.Abs(assumed - child) <= 0.5) return;
        _childEstimate = child;
        QueueRelayout();
    }

    void QueueRelayout()
    {
        if (_relayoutQueued) return;
        _relayoutQueued = true;
        void Run()
        {
            _relayoutQueued = false;
            if (!IsVirtual) return;
            ComputeOffsets();
            UpdateWindow(force: true);
        }
        if (Dispatcher is { } dispatcher) dispatcher.Dispatch(Run);
        else Run();
    }

    /// <summary>
    /// The columns as laid out. Carbon's table is always as wide as its container (`width: 100%`), so when no column is a
    /// star column the spare width goes to the content columns: the last `Auto` column takes it, or else the fixed-width
    /// columns share it in proportion to their widths, as a browser's table layout does. Flush tool columns (selection,
    /// expand, row menu) keep their width. The fixed widths stay the minimum: below it the table scrolls.
    /// </summary>
    internal static GridLength[] FillColumns(IReadOnlyList<GridLength> widths, IReadOnlyList<bool> flush)
    {
        var result = widths.ToArray();
        if (result.Any(w => w.IsStar)) return result;
        for (var c = result.Length - 1; c >= 0; c--)
        {
            if (!result[c].IsAuto || flush[c]) continue;
            result[c] = GridLength.Star;
            return result;
        }
        for (var c = 0; c < result.Length; c++)
            if (result[c].IsAbsolute && !flush[c] && result[c].Value > 0) result[c] = new GridLength(result[c].Value, GridUnitType.Star);
        return result;
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

        // The Auto header row ignores the fill's minimum, so the host carries the size's height (as body cells do).
        var host = new Grid { BackgroundColor = Colors.Transparent, MinimumHeightRequest = height };
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
        if (header.Body is not { } body) return;
        // A control in the header (the select-all checkbox) names its own hit target: a Description on the control would
        // make it one VoiceOver element and hide the hit target with its role and checked state.
        if (body is CdsControl { AccessibilityTarget: { } target })
        {
            if (string.IsNullOrEmpty(SemanticProperties.GetDescription(target))) SemanticProperties.SetDescription(target, header.Description ?? header.Text);
            return;
        }
        SemanticProperties.SetDescription(body, header.Description ?? header.Text);
    }

    void WireRowHover(View view, RowVisual visual)
    {
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) =>
        {
            _hoverVersion++;
            if (_hoverRow == visual.Index) return;
            _hoverRow = visual.Index;
            RequestVisuals();
        };
        pointer.PointerExited += (_, _) =>
        {
            // Moving between cells fires an exit then an enter: only clear when no enter follows.
            var version = ++_hoverVersion;
            var row = visual.Index;
            Dispatcher.Dispatch(() =>
            {
                if (version != _hoverVersion || _hoverRow != row) return;
                _hoverRow = -1;
                RequestVisuals();
            });
        };
        view.GestureRecognizers.Add(pointer);
    }

    // The column header of a text cell's label: the cell is read as "<header>: <value>" (the web's scope="col").
    static readonly BindableProperty ColumnNameProperty = BindableProperty.CreateAttached(
        "CdsColumnName", typeof(string), typeof(CdsTable), null);

    /// <summary>Shows a text cell's text and names it with its column header.</summary>
    static void SetCellText(CdsText label, string? text)
    {
        text ??= string.Empty;
        if (label.Text != text) label.Text = text;
        var name = CellName((string?)label.GetValue(ColumnNameProperty), text);
        if (name is null) label.ClearValue(SemanticProperties.DescriptionProperty);
        else SemanticProperties.SetDescription(label, name);
    }

    /// <summary>
    /// The accessible name of a text cell: "&lt;header&gt;: &lt;value&gt;", so a cell reached on its own still says which column it
    /// is in. <c>null</c> (the label reads its own text) when the column has no header text or the cell is empty.
    /// </summary>
    internal static string? CellName(string? header, string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(header)) return null;
        return $"{header}: {text}";
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
        /// <summary>The data row shown; -1 for a pooled slot that shows nothing.</summary>
        public int Index;
        /// <summary>A virtualized slot's own grid (null for rows in the shared body grid).</summary>
        public Grid? Root;
        public double Top = double.NaN;
        public bool Expanded;
        public int FirstGridRow;
        public int GridRows;
        public readonly List<View> Views = [];
        int _cellCount;
        View?[] _bodies = [];
        bool _expandable;

        /// <summary>Remembers the row's structure, to tell whether parked views still fit it.</summary>
        public void Snapshot()
        {
            _cellCount = Source.CellList.Count;
            _bodies = Source.CellList.Select(c => c.Body).ToArray();
            _expandable = Source.IsExpandable;
        }

        public bool Matches(CdsTableRow row)
        {
            if (row.CellList.Count != _cellCount || row.IsExpandable != _expandable) return false;
            for (var i = 0; i < _cellCount; i++)
                if (!ReferenceEquals(row.CellList[i].Body, _bodies[i])) return false;
            return true;
        }
        public readonly List<CdsTableCell> TextCells = [];
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
