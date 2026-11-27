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
    ScrollView? _pageScroll;
    readonly List<IDisposable> _pageSubscriptions = [];
    bool _pageWindowQueued;
    bool _warmQueued;
    bool _warming;
    int _hoverRow = -1;
    int _hoverHeader = -1;
    int _hoverVersion;
    bool _building;

    // AI presence (Carbon's AI row, AI column and whole-table treatment). Nothing below exists until something in the table is AI.
    Brush? _rowTint;
    Brush? _rowHoverTint;
    Brush? _tableTint;
    bool _tintsBound;
    bool _aiEnabled;
    bool _showDecoratorColumn;
    bool _decoratorColumn;
    int _firstBodyRow;
    CdsTableHeader? _decoratorHeader;
    Grid? _aiOverlay;

    /// <summary>Creates an empty table.</summary>
    public CdsTable()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        CdsTokens.SetColor(_bodyGrid, BackgroundColorProperty, CdsTableTokens.Background);
        CdsTokens.SetColor(_virtualBody, BackgroundColorProperty, CdsTableTokens.Background);
        // The sticky header and its body are one table surface: nothing of the page shows between or beside them.
        CdsTokens.SetColor(_stickyRoot, BackgroundColorProperty, CdsTableTokens.Background);
        _scroll.Scrolled += (_, _) => { if (IsVirtual && StickyHeader) UpdateWindow(); };
        _scroll.SizeChanged += (_, _) => { if (IsVirtual && StickyHeader) UpdateWindow(); };
        _stickyRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _stickyRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _stickyRoot.Add(_headerGrid, 0, 0);
        _stickyRoot.Add(_scroll, 0, 1);
        _rows.CollectionChanged += OnRowsChanged;
        _hscroll.SizeChanged += (_, _) => SyncContentWidth();
        // Row hover in the shared body grid: one recognizer that finds the row under the pointer, not one on every cell of
        // every row (each is a native gesture recognizer, and a table of 25 rows had 300 of them).
        var hover = new PointerGestureRecognizer();
        hover.PointerEntered += OnBodyPointer;
        hover.PointerMoved += OnBodyPointer;
        hover.PointerExited += (_, _) => SetHoverRow(-1);
        _bodyGrid.GestureRecognizers.Add(hover);
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

    /// <summary>
    /// Whether the table has Carbon's whole-table AI treatment (<c>cds--data-table-container--ai-enabled</c>): an <c>ai-aura-start</c>
    /// glow rising from the bottom of the body. The data table draws the gradient border around itself.
    /// </summary>
    internal bool IsAiEnabled
    {
        get => _aiEnabled;
        set
        {
            if (_aiEnabled == value) return;
            _aiEnabled = value;
            if (_built) PlaceAiOverlay();
        }
    }

    /// <summary>The table has a decorator column even when no row of its own has a decorator yet (rows supplied by a row source).</summary>
    internal bool ShowDecoratorColumn
    {
        get => _showDecoratorColumn;
        set
        {
            if (_showDecoratorColumn == value) return;
            _showDecoratorColumn = value;
            Rebuild();
        }
    }

    /// <summary>The table has the 16 wide column of its rows' <see cref="CdsTableRow.Decorator"/>s, before its first column.</summary>
    internal bool HasDecoratorColumn => _decoratorColumn;

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

    RowVisual? VisualAt(int index) => _rowVisuals.FirstOrDefault(v => v.Index == index);

    /// <summary>The AI tint over a realized row (for tests); null until the row first shows AI.</summary>
    internal Grid? AiLayerOf(int index) => VisualAt(index)?.AiLayer;

    /// <summary>The AI tint over the child row of a realized row (for tests).</summary>
    internal Grid? ChildAiOf(int index) => VisualAt(index)?.ChildAi;

    /// <summary>The fill of a realized row (for tests).</summary>
    internal BoxView? FillOf(int index) => VisualAt(index)?.Fill;

    /// <summary>The rule under a realized row (for tests).</summary>
    internal BoxView? RuleOf(int index) => VisualAt(index)?.Rule;

    /// <summary>The cell hosts of a realized row that carry an AI column's gradient (for tests).</summary>
    internal IReadOnlyList<Grid> AiHostsOf(int index) => VisualAt(index)?.AiHosts ?? [];

    /// <summary>The content host of a realized row's child row (for tests).</summary>
    internal Grid? ChildHostOf(int index) => VisualAt(index)?.ChildHost;

    /// <summary>The AI tint in a column's header cell and its top line (for tests).</summary>
    internal (Grid? Layer, BoxView? Line) HeaderAiOf(int column) => (_headers[column].AiLayer, _headers[column].AiLine);

    /// <summary>The whole-table tint, while it is placed over the body (for tests).</summary>
    internal Grid? AiOverlay => _aiOverlay?.Parent is null ? null : _aiOverlay;

    /// <summary>Puts the pointer on a row (for tests, which have no pointer).</summary>
    internal void HoverRow(int index) => SetHoverRow(index);

    /// <summary>The realized rows by data index (for tests).</summary>
    internal IReadOnlyList<(int Index, CdsTableRow Row)> RealizedRows =>
        _rowVisuals.Where(v => v.Index >= 0).OrderBy(v => v.Index).Select(v => (v.Index, v.Source)).ToList();

    /// <summary>
    /// Rows supplied on demand. With <see cref="StickyHeader"/> on, or a <see cref="PageScroll"/>, only the rows in (and
    /// near) view are realized, in a pool of rows the source re-binds as the body scrolls; <see cref="Rows"/> is then ignored.
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

    /// <summary>
    /// The vertical scroll view the table scrolls in when it has no sticky header (the page's). With a row source, the
    /// body is then virtualized against that scroll view's viewport: the header and a body as tall as all the rows, of
    /// which only those in (and near) view have views. Set by the data table, which finds it among its ancestors.
    /// </summary>
    internal ScrollView? PageScroll
    {
        get => _pageScroll;
        set
        {
            if (ReferenceEquals(_pageScroll, value)) return;
            foreach (var subscription in _pageSubscriptions) subscription.Dispose();
            _pageSubscriptions.Clear();
            _pageScroll = value;
            if (value is not null) WatchPageScroll(value);
            _childHeights.Clear();
            Rebuild();
        }
    }

    /// <summary>
    /// Whether the body is virtualized: a row source, and the table owns the scrolling (a sticky header) or knows the page's
    /// scroll view.
    /// </summary>
    internal bool IsVirtual => _source is not null && (StickyHeader || _pageScroll is not null);

    /// <summary>Virtualized against the page's scroll view (no sticky header).</summary>
    bool IsPageVirtual => IsVirtual && !StickyHeader;

    void WatchPageScroll(ScrollView page)
    {
        // Weakly: the scroll view can outlive the table (a table moved out of it), and must not keep it alive.
        EventHandler<ScrolledEventArgs>? scrolled = null;
        _pageSubscriptions.Add(new CdsWeakSubscription<CdsTable>(this, static t => t.OnPageScrolled(),
            h => { scrolled = (s, e) => h(s, e); page.Scrolled += scrolled; }, _ => page.Scrolled -= scrolled));
        _pageSubscriptions.Add(new CdsWeakSubscription<CdsTable>(this, static t => t.QueuePageWindow(),
            h => page.SizeChanged += h, h => page.SizeChanged -= h));
        // Content above the table growing or shrinking moves the body inside the scroll view.
        if (page.Content is { } content)
            _pageSubscriptions.Add(new CdsWeakSubscription<CdsTable>(this, static t => t.QueuePageWindow(),
                h => content.SizeChanged += h, h => content.SizeChanged -= h));
    }

    void QueueWarm()
    {
        if (_warmQueued) return;
        _warmQueued = true;
        Dispatcher.Dispatch(() =>
        {
            _warmQueued = false;
            _warming = true;
            try
            {
                UpdateWindow();
            }
            finally
            {
                _warming = false;
            }
        });
    }

    void OnPageScrolled()
    {
        if (IsPageVirtual) UpdateWindow();
    }

    void QueuePageWindow()
    {
        if (!IsPageVirtual || _pageWindowQueued) return;
        _pageWindowQueued = true;
        void Run()
        {
            _pageWindowQueued = false;
            UpdateWindow();
        }
        if (Handler is not null && Dispatcher is { } dispatcher) dispatcher.Dispatch(Run);
        else Run();
    }

    /// <summary>
    /// The part of the body in view, as (top, height) in body coordinates: the inner scroll view's with a sticky header,
    /// else the page scroll view's viewport less the body's offset in its content (the top is negative while the body
    /// starts below the fold).
    /// </summary>
    (double Top, double Height) Viewport()
    {
        if (!IsPageVirtual) return (_scroll.ScrollY, _scroll.Height > 0 ? _scroll.Height : MaximumBodyHeight);
        var page = _pageScroll!;
        var offset = 0d;
        for (Element? e = _virtualBody; e is not null && !ReferenceEquals(e, page); e = e.Parent)
            if (e is VisualElement v) offset += v.Y;
        // Before the first layout nothing has a size: assume the window (or a laptop screen) is the viewport.
        var height = page.Height > 0 ? page.Height : page.Window?.Height is > 0 and var window ? window : 900;
        return (page.ScrollY - offset, height);
    }

    /// <summary>The visible span of a body from a viewport (top may be negative or past the end); at least 1 high, from 0.</summary>
    internal static (double Top, double Height) VisibleSpan(double top, double viewport)
    {
        var from = Math.Max(0, top);
        return (from, Math.Max(1, top + viewport - from));
    }

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
        // Re-bound rows may have become AI rows, or stopped being one.
        if (_decoratorColumn) RequestVisuals();
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
            if (_aiEnabled) PlaceAiOverlay();
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
            {
                if (r.CellList.Count + (_decoratorColumn ? 1 : 0) > _columns.Count) return false; // the row adds columns
                if (r.Decorator is not null && !_decoratorColumn) return false; // the row adds the decorator column
            }
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
            if (row.IsExpandable && !visual.HasChildRow) { Rebuild(); return; }
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
            if (h.AiLine is not null) SetColor(h.AiLine, BoxView.ColorProperty, CdsAiTokens.BorderStrong);
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
            // An AI row (a shown AI label in its decorator) ignores the zebra stripe and has its own fills under the AI tint.
            var ai = v.Source.IsAi;
            var stripe = zebra && v.Index % 2 == 1 && !ai;
            var fill = ai ? CdsTableTokens.AiRowFill(selected, hover) : CdsTableTokens.RowFill(selected, hover, stripe);
            SetColor(v.Fill, BoxView.ColorProperty, fill);
            SetColor(v.Rule, BoxView.ColorProperty, CdsTableTokens.RowRule(selected, hover, zebra, stripe, ai));
            UpdateRowAi(v, ai, hover);
            var active = selected || hover;
            foreach (var label in v.Labels) SetColor(label, CdsText.TextColorProperty, CdsTableTokens.CellText(active));

            var expanded = v.HasChildRow && v.Source.IsExpanded && v.Source.ExpandedContent is not null;
            if (expanded) EnsureChildRow(v);
            if (v.ChildHost is not null)
            {
                v.ChildHost.IsVisible = expanded;
                v.ChildFill!.IsVisible = expanded;
                v.ChildRule!.IsVisible = expanded;
                SetColor(v.ChildFill, BoxView.ColorProperty, fill);
                SetColor(v.ChildRule, BoxView.ColorProperty, CdsTableTokens.RowRule(false, hover, zebra, stripe, ai));
            }
            UpdateChildAi(v, ai && expanded, hover);
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
        // The header is a grid of its own above the body when the table scrolls its body or virtualizes against the page.
        var separate = sticky || virtualized;
        var header = separate ? _headerGrid : _bodyGrid;
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
        // Rows with a decorator (Carbon's TableDecoratorRow) get a 16 wide column before the first one.
        _decoratorColumn = _showDecoratorColumn || rows.Any(r => r.Decorator is not null);
        _decoratorHeader = _decoratorColumn ? CreateDecoratorHeader() : null;
        var rowColumns = virtualized || rows.Count == 0 ? 0 : rows.Max(r => r.CellList.Count);
        var columnCount = Math.Max(head?.Count ?? 0, rowColumns) + (_decoratorColumn ? 1 : 0);
        _minimumWidth = 0;
        var widths = new GridLength[columnCount];
        var flush = new bool[columnCount];
        for (var c = 0; c < columnCount; c++)
        {
            var width = HeadAt(c) is { } columnHead ? columnHead.Width : GridLength.Star;
            if (separate && width.IsAuto) width = GridLength.Star;
            widths[c] = width;
            flush[c] = HeadAt(c)?.IsFlush == true;
            _minimumWidth += width.IsAbsolute ? width.Value : CdsTableTokens.MinimumFlexibleColumnWidth;
        }
        foreach (var width in FillColumns(widths, flush))
        {
            _columns.Add(width);
            _bodyGrid.ColumnDefinitions.Add(new ColumnDefinition(width));
            if (separate) _headerGrid.ColumnDefinitions.Add(new ColumnDefinition(width));
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
                BuildHeaderCell(header, HeadAt(c), c, _rowHeight, _padTop, _padBottom, _topAligned);
            if (!separate) _nextGridRow = 1;
        }
        _firstBodyRow = _nextGridRow;

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
            SetBody(_scroll);
            _scroll.Content = virtualized ? _virtualBody : _bodyGrid;
            _scroll.MaximumHeightRequest = MaximumBodyHeight;
            _hscroll.Content = _stickyRoot;
        }
        else if (virtualized)
        {
            // The page scrolls: the body is as tall as all its rows, under the header.
            _scroll.Content = null;
            SetBody(_virtualBody);
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
        PlaceAiOverlay();
    }

    /// <summary>The header of a column (the decorator column's own header first, when there is one); `null` past the head.</summary>
    CdsTableHeader? HeadAt(int column)
    {
        if (_decoratorColumn)
        {
            if (column == 0) return _decoratorHeader;
            column--;
        }
        var head = Head?.HeaderList;
        return head is not null && column < head.Count ? head[column] : null;
    }

    /// <summary>The cell of a row in a column (the decorator column has none: the row's decorator is drawn there).</summary>
    CdsTableCell? CellAt(CdsTableRow row, int column)
    {
        if (_decoratorColumn) column--;
        return column >= 0 && column < row.CellList.Count ? row.CellList[column] : null;
    }

    static CdsTableHeader CreateDecoratorHeader() => new()
    {
        Width = new GridLength(CdsTableTokens.DecoratorColumnWidth),
        IsSortable = false,
        IsFlush = true,
        Description = "AI",
        Body = new BoxView { WidthRequest = 1, HeightRequest = 1, Color = Colors.Transparent, InputTransparent = true },
    };

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
            var child = view == visual.ChildHost || view == visual.ChildFill || view == visual.ChildRule || view == visual.ChildAi;
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

        visual.Rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        Hide(visual.Rule);
        Add(visual.Rule, 0, gridRow, span);

        string? ColumnName(int c) => HeadAt(c) is { IsFlush: false } named ? named.Text : null;
        // A flush control in a tool column (selection, expand, row menu) needs no cell host: it has no padding and no
        // column name to group under, so it goes straight into the row. One host stays, to carry the row's height.
        bool Bare(int c) => CellAt(source, c) is { Body: not null, IsFlush: true } && ColumnName(c) is null;
        var anyHosted = false;
        for (var c = 0; c < columnCount && !anyHosted; c++) anyHosted = !Bare(c);

        for (var c = 0; c < columnCount; c++)
        {
            if (c == 0 && _decoratorColumn)
            {
                // The row's AI label, in the decorator column. A decorator that is not an AI label draws nothing, as in Carbon.
                var decoratorHost = new Grid { BackgroundColor = Colors.Transparent, MinimumHeightRequest = height };
                if (source.Decorator is CdsAILabel label)
                {
                    if (label.Parent is Layout labelParent) labelParent.Remove(label);
                    label.HorizontalOptions = LayoutOptions.Start;
                    // Carbon centers it (translateY(-50%)), and in xl rows (top aligned) sets it at translateY(1px) of the cell's text line.
                    label.VerticalOptions = _topAligned ? LayoutOptions.Start : LayoutOptions.Center;
                    label.Margin = _topAligned ? new Thickness(0, _padTop + 1, 0, 0) : new Thickness(0);
                    decoratorHost.Add(label);
                }
                Add(decoratorHost, 0, gridRow);
                continue;
            }
            var cell = CellAt(source, c);
            var headerAlign = HeadAt(c)?.Align ?? CdsTableAlignment.Start;
            var columnName = ColumnName(c);
            var align = cell?.Align ?? headerAlign;
            var flush = cell?.IsFlush == true;
            if (anyHosted && Bare(c))
            {
                var tool = cell!.Body!;
                if (cell.RevealOnHover) visual.RevealViews.Add((cell, tool));
                if (tool.Parent is Layout previous) previous.Remove(tool);
                Add(tool, c, gridRow);
                continue;
            }
            var host = new Grid { BackgroundColor = Colors.Transparent, MinimumHeightRequest = height };
            if (HeadAt(c) is { IsAi: true })
            {
                // Carbon: td.cds--table-cell--column-slug gets the AI gradient.
                EnsureTints();
                host.Background = _rowTint;
                visual.AiHosts.Add(host);
            }
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
            Add(host, c, gridRow);
        }

        // The child row's views wait for the row's first expansion (EnsureChildRow): most rows are never expanded.
        visual.HasChildRow = source.IsExpandable;
        visual.ChildGrid = grid;
        visual.ChildGridRow = gridRow + 1;
    }

    /// <summary>
    /// Creates an expandable row's child row (fill, rule, content host) the first time it is shown, after the row's own views
    /// so the order of the grid's children (Tab and screen reader order) stays row by row.
    /// </summary>
    void EnsureChildRow(RowVisual visual)
    {
        if (visual.ChildHost is not null || !visual.HasChildRow || visual.ChildGrid is not { } grid) return;
        var span = Math.Max(1, _columns.Count);
        var at = grid.Children.IndexOf(visual.Views[^1]) + 1;
        void Insert(View view)
        {
            Grid.SetRow(view, visual.ChildGridRow);
            Grid.SetColumn(view, 0);
            Grid.SetColumnSpan(view, span);
            grid.Children.Insert(at++, view);
            visual.Views.Add(view);
        }
        visual.ChildFill = new BoxView { IsVisible = false };
        Hide(visual.ChildFill);
        Insert(visual.ChildFill);
        visual.ChildRule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
        Hide(visual.ChildRule);
        Insert(visual.ChildRule);
        visual.ChildHost = new Grid
        {
            IsVisible = false,
            Padding = new Thickness(CdsTableTokens.ChildContentInset(ChildInset, _decoratorColumn), CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing06),
        };
        Insert(visual.ChildHost);
        SetChildContent(visual, visual.Source);
    }

    void OnBodyPointer(object? sender, PointerEventArgs e)
    {
        if (IsVirtual || e.GetPosition(_bodyGrid) is not { } point) return;
        SetHoverRow(RowAt(point.Y));
    }

    void SetHoverRow(int row)
    {
        if (_hoverRow == row) return;
        _hoverRow = row;
        RequestVisuals();
    }

    /// <summary>The data row of the shared body grid at <paramref name="y"/> (its own row or its child row), or -1.</summary>
    int RowAt(double y) => RowAt(RowSpans(), y);

    IEnumerable<(int Index, double Top, double Bottom)> RowSpans()
    {
        foreach (var v in _rowVisuals)
        {
            if (v.Index < 0 || v.Fill.Height <= 0) continue;
            var bottom = v.ChildFill is { IsVisible: true } child ? child.Y + child.Height : v.Fill.Y + v.Fill.Height;
            yield return (v.Index, v.Fill.Y, bottom);
        }
    }

    /// <summary>The row whose span holds <paramref name="y"/>; the spans run top to bottom. -1 above, between or below them.</summary>
    internal static int RowAt(IEnumerable<(int Index, double Top, double Bottom)> spans, double y)
    {
        foreach (var (index, top, bottom) in spans)
        {
            if (y < top) return -1;
            if (y < bottom) return index;
        }
        return -1;
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
        var view = Viewport();
        var (spanTop, spanHeight) = VisibleSpan(top ?? view.Top, view.Height);
        var (first, last) = RowWindow(_offsets, count, spanTop, spanHeight, Overscan);
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

        // The rows in view first; overscan rows (realized ahead of scrolling) take free slots, but new slots for them are
        // made on the next turn of the run loop, so a first build only pays for what shows.
        var (seenFirst, seenLast) = RowWindow(_offsets, count, spanTop, spanHeight, 0);
        var deferred = false;
        _binding = true;
        try
        {
            void Realize(int i, bool inView)
            {
                if (bound.ContainsKey(i)) return;
                if (free.Count == 0 && !inView && !_warming && Handler is not null)
                {
                    deferred = true;
                    return;
                }
                RowVisual slot;
                if (free.Count > 0)
                {
                    slot = free.Pop();
                    Bind(slot, i);
                }
                else
                {
                    slot = CreateSlot(i);
                }
                Order(slot);
                bound[i] = slot;
                changed = true;
            }
            for (var i = seenFirst; i <= seenLast; i++) Realize(i, inView: true);
            for (var i = first; i <= last; i++)
                if (i < seenFirst || i > seenLast) Realize(i, inView: false);
        }
        finally
        {
            _binding = false;
        }
        if (deferred) QueueWarm();

        foreach (var (index, v) in bound) Place(v, _offsets[index]);
        if (_hoverRow >= 0 && !bound.ContainsKey(_hoverRow)) _hoverRow = -1;
        UpdateAccessibility();
        // New slots were added after the whole-table tint: keep it on top.
        if (changed && _aiEnabled) PlaceAiOverlay();
        if (changed && applyVisuals) RequestVisuals();
    }

    /// <summary>Puts the body under the sticky root's header: the inner scroll view (sticky header) or the bare virtual body.</summary>
    void SetBody(View body)
    {
        View? current = null;
        foreach (var child in _stickyRoot.Children)
            if (child is View v && Grid.GetRow(v) == 1) current = v;
        if (ReferenceEquals(current, body)) return;
        if (current is not null) _stickyRoot.Remove(current);
        if (body.Parent is Layout old) old.Remove(body);
        _stickyRoot.Add(body, 0, 1);
    }

    /// <summary>
    /// Realizes the rows for a body whose top <paramref name="top"/> is at the top of the viewport (scrolled there, or for a
    /// page-virtualized body, that far into the body), as scrolling would (tests have no native scroll view).
    /// </summary>
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

    /// <summary>
    /// A new pooled slot showing data row <paramref name="index"/>. It is bound before it joins the body, so its cell
    /// templates never see the table's own binding context (the page's view model) in between.
    /// </summary>
    RowVisual CreateSlot(int index)
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
        WireRowHover(root, visual);
        AbsoluteLayout.SetLayoutFlags(root, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.WidthProportional);
        root.SizeChanged += (_, _) => OnSlotSized(visual);
        _rowVisuals.Add(visual);
        _visualByRow[row] = visual;
        _slotByRoot[root] = visual;
        Bind(visual, index);
        _virtualBody.Add(root);
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

        // An AI column (an AI label in its decorator): the AI gradient over the header fill, and the 1px ai-border-strong top line.
        if (header is { IsAi: true })
        {
            EnsureTints();
            var layer = new Grid { InputTransparent = true, Background = _rowTint };
            Hide(layer);
            AutomationProperties.SetExcludedWithChildren(layer, true);
            var line = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
            Hide(line);
            layer.Add(line);
            grid.Add(layer, column, 0);
            visual.AiLayer = layer;
            visual.AiLine = line;
        }

        // The Auto header row ignores the fill's minimum, so the host carries the size's height (as body cells do).
        var host = new Grid { BackgroundColor = Colors.Transparent, MinimumHeightRequest = height };
        // The decorator sits in a column of its own at the end of the cell (Carbon: margin-inline-start: auto).
        var decorator = header?.Decorator;
        if (decorator is not null)
        {
            host.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            host.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }
        var endPadding = decorator is null ? CdsTableTokens.CellPaddingX : 0;
        grid.Add(host, column, 0);

        if (header?.Body is { } body)
        {
            if (body.Parent is Layout old) old.Remove(body);
            body.VerticalOptions = LayoutOptions.Center;
            if (!header.IsFlush) host.Padding = new Thickness(CdsTableTokens.CellPaddingX, 0, endPadding, 0);
            host.Add(body);
            SetHeaderName(header);
        }
        else if (!sortable)
        {
            visual.Label = new CdsText(header?.Text ?? string.Empty, CdsTypeStyle.HeadingCompact01)
            {
                HorizontalTextAlignment = align == CdsTableAlignment.End ? TextAlignment.End : TextAlignment.Start,
                VerticalOptions = topAligned ? LayoutOptions.Start : LayoutOptions.Center,
                Padding = new Thickness(CdsTableTokens.CellPaddingX, padTop, endPadding, padBottom),
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
                layout.Padding = new Thickness(0, 0, endPadding, 0);
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
        if (decorator is not null)
        {
            // Above the hit target: the label is its own button. Carbon centers it in the header (and sets it 8 from the end of a sort header).
            if (decorator.Parent is Layout decoratorParent) decoratorParent.Remove(decorator);
            decorator.HorizontalOptions = LayoutOptions.End;
            decorator.VerticalOptions = topAligned ? LayoutOptions.Start : LayoutOptions.Center;
            decorator.Margin = new Thickness(0, topAligned ? padTop + 1 : 0, sortable ? CdsSpacing.Spacing03 : CdsTableTokens.CellPaddingX, 0);
            host.Add(decorator, 1, 0);
        }
        if (header is not null && header.Body is null && !sortable) SetHeaderName(header);
    }

    // ----- AI presence -----

    /// <summary>
    /// Binds the aura tokens once, when the table first shows AI (a column, a row, or the whole table): the tint brushes are rebuilt
    /// from the resolved colors whenever the theme changes, and handed to every view that shows one.
    /// </summary>
    void EnsureTints()
    {
        if (_tintsBound) return;
        _tintsBound = true;
        CdsColorProxies.Bind(this, OnTintColors, CdsAiTokens.AuraSmall, CdsAiTokens.AuraHoverStart, CdsAiTokens.Aura);
    }

    void OnTintColors(Color?[] colors)
    {
        _rowTint = colors[0] is { } small ? CdsAiTableGradient.Row(small) : null;
        _rowHoverTint = colors[1] is { } hover ? CdsAiTableGradient.RowHover(hover) : null;
        _tableTint = colors[2] is { } aura ? CdsAiTableGradient.FullTable(aura) : null;
        foreach (var h in _headers)
            if (h.AiLayer is not null) h.AiLayer.Background = _rowTint;
        foreach (var v in _rowVisuals)
        {
            foreach (var host in v.AiHosts) host.Background = _rowTint;
            if (v.AiLayer is not null) v.AiLayer.Background = v.AiHover ? _rowHoverTint : _rowTint;
            if (v.ChildAi is not null) v.ChildAi.Background = v.AiHover ? _rowHoverTint : _rowTint;
        }
        if (_aiOverlay is not null) _aiOverlay.Background = _tableTint;
    }

    /// <summary>
    /// Shows or hides an AI row's tint (and its 1px start edge): made on the first AI row a slot shows, then only toggled, so a pooled slot
    /// re-bound from an AI row to another row clears it. The row's own fill is the token color the state picks.
    /// </summary>
    void UpdateRowAi(RowVisual v, bool ai, bool hover)
    {
        if (!ai)
        {
            if (v.AiLayer is { IsVisible: true } shown) shown.IsVisible = false;
            return;
        }
        EnsureTints();
        if (v.AiLayer is null) CreateRowAiLayer(v);
        var layer = v.AiLayer!;
        if (!layer.IsVisible) layer.IsVisible = true;
        v.AiHover = hover;
        layer.Background = hover ? _rowHoverTint : _rowTint;
        SetColor(v.AiEdge!, BoxView.ColorProperty, CdsAiTokens.BorderStrong);
    }

    void CreateRowAiLayer(RowVisual v)
    {
        var grid = v.ChildGrid!;
        var layer = new Grid { IsVisible = false, InputTransparent = true };
        Hide(layer);
        AutomationProperties.SetExcludedWithChildren(layer, true);
        // Carbon: box-shadow: inset 1px 0 ai-border-strong on the row.
        var edge = new BoxView { WidthRequest = 1, MinimumHeightRequest = _rowHeight, HorizontalOptions = LayoutOptions.Start, InputTransparent = true };
        Hide(edge);
        layer.Add(edge);
        Grid.SetRow(layer, Grid.GetRow(v.Fill));
        Grid.SetColumn(layer, 0);
        Grid.SetColumnSpan(layer, Math.Max(1, _columns.Count));
        grid.Children.Insert(grid.Children.IndexOf(v.Fill) + 1, layer);
        v.Views.Add(layer);
        v.AiLayer = layer;
        v.AiEdge = edge;
    }

    /// <summary>The AI tint over the child row of an expanded AI row (Carbon: <c>tr.ai-label-row + .expandable-row</c>).</summary>
    void UpdateChildAi(RowVisual v, bool ai, bool hover)
    {
        if (!ai)
        {
            if (v.ChildAi is { IsVisible: true } shown) shown.IsVisible = false;
            return;
        }
        if (v.ChildFill is null || v.ChildGrid is not { } grid) return;
        EnsureTints();
        if (v.ChildAi is null)
        {
            var layer = new Grid { InputTransparent = true };
            Hide(layer);
            AutomationProperties.SetExcludedWithChildren(layer, true);
            Grid.SetRow(layer, v.ChildGridRow);
            Grid.SetColumn(layer, 0);
            Grid.SetColumnSpan(layer, Math.Max(1, _columns.Count));
            grid.Children.Insert(grid.Children.IndexOf(v.ChildFill) + 1, layer);
            v.Views.Add(layer);
            v.ChildAi = layer;
        }
        if (!v.ChildAi.IsVisible) v.ChildAi.IsVisible = true;
        v.ChildAi.Background = hover ? _rowHoverTint : _rowTint;
    }

    /// <summary>
    /// Puts the whole-table tint over the body rows (Carbon's <c>tbody::before</c>), or takes it away: after the rows of the shared grid
    /// (spanning them) or last in the virtualized body, so it stays on top of rows added later. It takes no input.
    /// </summary>
    void PlaceAiOverlay()
    {
        var virtualized = IsVirtual;
        if (!_aiEnabled || !virtualized && _nextGridRow <= _firstBodyRow)
        {
            if (_aiOverlay?.Parent is Layout current) current.Remove(_aiOverlay);
            return;
        }
        EnsureTints();
        if (_aiOverlay is null)
        {
            _aiOverlay = new Grid { InputTransparent = true };
            Hide(_aiOverlay);
            AutomationProperties.SetExcludedWithChildren(_aiOverlay, true);
        }
        var overlay = _aiOverlay;
        overlay.Background = _tableTint;
        if (virtualized)
        {
            var body = _virtualBody;
            if (!ReferenceEquals(overlay.Parent, body) || !ReferenceEquals(body.Children[body.Children.Count - 1], overlay))
            {
                if (overlay.Parent is Layout old) old.Remove(overlay);
                body.Add(overlay);
            }
            AbsoluteLayout.SetLayoutFlags(overlay, Microsoft.Maui.Layouts.AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(overlay, new Rect(0, 0, 1, 1));
            return;
        }
        Grid.SetRow(overlay, _firstBodyRow);
        Grid.SetRowSpan(overlay, _nextGridRow - _firstBodyRow);
        Grid.SetColumn(overlay, 0);
        Grid.SetColumnSpan(overlay, Math.Max(1, _columns.Count));
        var grid = _bodyGrid;
        if (!ReferenceEquals(overlay.Parent, grid) || !ReferenceEquals(grid.Children[grid.Children.Count - 1], overlay))
        {
            if (overlay.Parent is Layout old) old.Remove(overlay);
            grid.Add(overlay);
        }
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
        /// <summary>The AI gradient over the fill of an AI column's header, and its top line.</summary>
        public Grid? AiLayer;
        public BoxView? AiLine;
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
        View? _decorator;

        /// <summary>Remembers the row's structure, to tell whether parked views still fit it.</summary>
        public void Snapshot()
        {
            _cellCount = Source.CellList.Count;
            _bodies = Source.CellList.Select(c => c.Body).ToArray();
            _expandable = Source.IsExpandable;
            _decorator = Source.Decorator;
        }

        public bool Matches(CdsTableRow row)
        {
            if (row.CellList.Count != _cellCount || row.IsExpandable != _expandable || !ReferenceEquals(row.Decorator, _decorator)) return false;
            for (var i = 0; i < _cellCount; i++)
                if (!ReferenceEquals(row.CellList[i].Body, _bodies[i])) return false;
            return true;
        }
        public readonly List<CdsTableCell> TextCells = [];
        public BoxView Fill = null!;
        public BoxView Rule = null!;
        /// <summary>The AI tint over an AI row's fill, with its 1px start edge; made on the first AI row this slot shows.</summary>
        public Grid? AiLayer;
        public BoxView? AiEdge;
        /// <summary>The AI tint over the child row of an expanded AI row.</summary>
        public Grid? ChildAi;
        public bool AiHover;
        /// <summary>The cell hosts of the AI columns, which carry the column's gradient.</summary>
        public readonly List<Grid> AiHosts = [];
        /// <summary>The row is expandable: its child row's views are made on first expansion, in <see cref="ChildGrid"/>.</summary>
        public bool HasChildRow;
        public Grid? ChildGrid;
        public int ChildGridRow;
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
