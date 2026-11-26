using System.Collections;
using System.Collections.Specialized;
using System.Globalization;

namespace Maui.Carbon;

/// <summary>
/// Base class of the Carbon chart controls (Carbon Charts' chart holder): a title and toolbar, a legend, the graph,
/// tooltips, "Show as table", fullscreen and exports. The graph is drawn on one canvas; its text labels are real labels.
/// Keyboard and screen-reader users reach the data through one focusable data cursor: arrow keys (or VoiceOver and
/// TalkBack's adjust gestures) step through the values and show each one's tooltip. "Show as table" gives every value.
/// Charts take Carbon Charts' data and options as <see cref="Data"/> and bindable properties; each chart documents its
/// data type. Set <c>HeightRequest</c> for the whole chart (title, legend and graph); the default is 400.
/// </summary>
public abstract partial class CdsChart : CdsControl
{
    const string ShimmerAnimation = "cds-chart-shimmer";

    readonly Grid _root = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly Grid _header = new() { ColumnSpacing = 0 };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.HeadingCompact02, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalTextAlignment = TextAlignment.Center };
    readonly HorizontalStackLayout _toolbar = new() { Spacing = 0, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
    readonly BoxView _headerSpacer = new() { Color = Microsoft.Maui.Graphics.Colors.Transparent, InputTransparent = true };
    readonly Grid _frame = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly CdsChartLegend _legend;
    readonly Grid _plot = new();
    readonly CdsChartCanvas _canvas;
    readonly CdsChartLabelLayer _labels = new();
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset) { InputTransparent = true };
    readonly CdsHitTarget _hit;
    readonly AbsoluteLayout _overlay = new() { InputTransparent = true, CascadeInputTransparent = true };
    readonly CdsChartTooltip _tooltip = new();
    readonly CdsModal _tableModal = new() { Size = CdsModalSize.Lg, HasScrollingContent = true };
    readonly CdsCoalescedAction _rebuild;
    CdsWeakSubscription<CdsChart>? _collectionSubscription;

    IReadOnlyList<object> _items = [];
    IReadOnlyList<string> _groups = [];
    IReadOnlyList<string> _selected = [];
    ChartScene _scene = ChartScene.Empty;
    ChartColors? _colors;
    int _active = -1;
    string? _legendHover;
    PointF? _pointer;
    bool _itemsDirty = true;
    bool _syncingSelection;
    float _shimmerPhase;
    CdsOverlay? _fullscreen;
    Size _lastSize;

    /// <summary>Builds the chart chrome.</summary>
    private protected CdsChart()
    {
        _canvas = new CdsChartCanvas(GetType().Name);
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        _legend = new CdsChartLegend(this);

        _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _header.Add(_title, 0);
        _header.Add(_toolbar, 1);
        _title.Margin = new Thickness(0, 0, CdsSpacing.Spacing05, 0);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level2);

        _hit = CreateHitTarget(CdsAccessibilityRole.Adjustable);
        _hit.Incremented += (_, _) => MoveCursor(1);
        _hit.Decremented += (_, _) => MoveCursor(-1);
        FocusLost += (_, _) => { if (_pointer is null) ClearActive(); };

        _plot.Add(_canvas);
        _plot.Add(_labels);
        _plot.Add(_ring);
        _plot.Add(_hit);
        _plot.SizeChanged += (_, _) => RequestRebuild();

        var pointer = new PointerGestureRecognizer();
        pointer.PointerMoved += (_, e) => OnPointer(e.GetPosition(_plot), touch: false);
        pointer.PointerPressed += (_, e) => OnPointer(e.GetPosition(_plot), touch: true);
        pointer.PointerExited += (_, _) =>
        {
            _pointer = null;
            ClearActive();
        };
        _plot.GestureRecognizers.Add(pointer);

        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        _root.Add(_header, 0, 0);
        _root.Add(_headerSpacer, 0, 1);
        _root.Add(_frame, 0, 2);
        _root.Add(_tableModal, 0, 0);
        _overlay.Add(_tooltip);
        _root.Add(_overlay, 0, 0);
        Grid.SetRowSpan(_overlay, 3);

        _tableModal.RequestSubmit += async (_, _) => await ExportAsync(CdsChartExportFormat.Csv);
        _tableModal.RequestClose += (_, _) => _tableModal.IsOpen = false;

        SetChrome(_root);
        Loaded += (_, _) =>
        {
            SubscribeCollection();
            RequestRebuild();
            UpdateShimmer();
        };
        Unloaded += (_, _) =>
        {
            UnsubscribeCollection();
            CdsLoop.Stop(this, ShimmerAnimation);
        };
        BuildFrame();
        BuildToolbar();
    }

    /// <summary>Raised when a legend click changes <see cref="SelectedGroups"/>.</summary>
    public event EventHandler<CdsChartSelectedGroupsChangedEventArgs>? SelectedGroupsChanged;

    /// <summary>Raised before an export is shared; set <see cref="CdsChartExportingEventArgs.Handled"/> to save the file yourself.</summary>
    public event EventHandler<CdsChartExportingEventArgs>? Exporting;

    /// <summary>Whether the chart fills the page ("Make fullscreen"); Escape or "Exit fullscreen" returns it.</summary>
    public bool IsFullscreen => _fullscreen is not null;

    // ----- what each chart provides -----

    /// <summary>Builds the scene for the plot size in <paramref name="ctx"/>.</summary>
    private protected abstract ChartScene BuildScene(ChartBuildContext ctx);

    /// <summary>The "Show as table" and CSV content.</summary>
    private protected virtual ChartTable BuildTable(ChartBuildContext ctx) => new(
        [ctx.GroupLabel, "Key", "Value"],
        ctx.ActivePoints.Select(p => new[] { p.Group ?? "", p.Key ?? (p.Date is { } d ? ctx.Date(d) : ctx.Number(p.X)), ctx.Value(p.Value) }).ToList());

    /// <summary>The groups (series) of the data, in order; colors follow this order.</summary>
    private protected virtual IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items)
    {
        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
            if (item is CdsChartPoint p && seen.Add(p.Group ?? "")) list.Add(p.Group ?? "");
        return list;
    }

    /// <summary>The height when no height is requested and the parent does not constrain it. Default 400.</summary>
    private protected virtual double DefaultHeight => 400;

    /// <summary>Whether the legend shows when <see cref="IsLegendEnabled"/> is not set.</summary>
    private protected virtual bool DefaultLegendEnabled => true;

    /// <summary>Whether the toolbar shows when <see cref="IsToolbarEnabled"/> is not set.</summary>
    private protected virtual bool DefaultToolbarEnabled => true;

    /// <summary>The groups the legend lists (default every group).</summary>
    private protected virtual IReadOnlyList<string> LegendGroups => _groups;

    /// <summary>Additional legend items (radius, areas, quartiles).</summary>
    private protected virtual IReadOnlyList<ChartLegendExtra> LegendExtras => [];

    /// <summary>The loading and empty skeleton.</summary>
    private protected virtual ChartSkeleton Skeleton => ChartSkeleton.Grid;

    /// <summary>
    /// The loading (<paramref name="shimmer"/>) or empty scene; default <see cref="Skeleton"/>'s. A chart with its own
    /// skeleton shape (the meter's empty bar) builds it here and marks it <see cref="ChartShape.Shimmers"/>.
    /// </summary>
    private protected virtual ChartScene BuildSkeleton(float width, float height, bool shimmer) => ChartSkeletons.Build(Skeleton, width, height, shimmer);

    /// <summary>What the chart is, for screen readers ("Bar chart").</summary>
    private protected virtual string ChartKind => "Chart";

    /// <summary>Whether the data is empty (Carbon shows the skeleton then).</summary>
    private protected virtual bool IsEmpty(IReadOnlyList<object> items) => items.Count == 0;

    /// <summary>Requests a rebuild of the scene after the current changes (chart properties call it).</summary>
    private protected void RequestRebuild() => _rebuild.Request();

    /// <summary>The isolated groups (empty: all).</summary>
    private protected IReadOnlyList<string> Selected => _selected;

    /// <summary>The current scene (for tests).</summary>
    internal ChartScene Scene => _scene;

    /// <summary>The data cursor's mark, or -1 (for tests).</summary>
    internal int ActiveMark => _active;

    /// <summary>The legend (for tests).</summary>
    internal CdsChartLegend Legend => _legend;

    /// <summary>The plot hit target (for tests).</summary>
    internal CdsHitTarget PlotTarget => _hit;

    internal bool LegendShown => (IsLegendEnabled ?? DefaultLegendEnabled) && LegendGroups.Count + LegendExtras.Count > 0;

    // ----- building -----

    /// <summary>Builds the scene for a plot size without laying out (tests and exports).</summary>
    internal ChartScene RenderScene(float width, float height)
    {
        SnapshotItems();
        var colors = ResolveColors();
        if (width <= 0 || height <= 0) return ChartScene.Empty;
        if (IsLoading || IsEmpty(_items)) return BuildSkeleton(width, height, IsLoading);
        return BuildScene(Context(width, height, colors));
    }

    /// <summary>The table content (for tests and the table modal).</summary>
    internal ChartTable RenderTable()
    {
        SnapshotItems();
        return BuildTable(Context(Math.Max(1, (float)_plot.Width), Math.Max(1, (float)_plot.Height), ResolveColors()));
    }

    ChartBuildContext Context(float width, float height, ChartColors colors) => new(width, height, _items, _groups, _selected, colors, Culture ?? CultureInfo.CurrentCulture)
    {
        ValueFormat = ValueFormat,
        ValueFormatter = ValueFormatter,
        ShowTotal = ShowTooltipTotal,
        TotalLabel = string.IsNullOrEmpty(TooltipTotalLabel) ? "Total" : TooltipTotalLabel!,
    };

    ChartColors ResolveColors()
    {
        _colors = new ChartColors(new CdsChartPalette(ContextTheme, ContextLayerLevel), _groups, ColorPairingOption, ColorScale);
        return _colors;
    }

    void SnapshotItems()
    {
        if (!_itemsDirty) return;
        _itemsDirty = false;
        var list = new List<object>();
        if (Data is { } data)
            foreach (var item in data)
                if (item is not null) list.Add(item);
        _items = list;
        _groups = GroupsOf(_items);
        var kept = _selected.Where(_groups.Contains).ToList();
        if (kept.Count != _selected.Count) SetSelected(kept, raise: false);
    }

    void Rebuild()
    {
        var w = (float)_plot.Width;
        var h = (float)_plot.Height;
        try
        {
            _scene = RenderScene(w, h);
        }
        catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
        {
            // A chart must not take the app down over data it cannot draw: show the empty skeleton and report it.
            System.Diagnostics.Trace.TraceError($"{GetType().Name} could not draw its data: {ex}");
            _scene = BuildSkeleton(w, h, shimmer: false);
        }
        var colors = _colors!;
        _canvas.Show(_scene, colors);
        _labels.Show(_scene.Texts, colors, _legendHover);
        UpdateLegend();
        if (_active >= 0 && _scene.FindMark(_active) is null) ClearActive();
        else if (_active >= 0) ShowActive();
        UpdatePlotSemantics();
        UpdateShimmer();
    }

    void UpdateLegend()
    {
        var shown = LegendShown;
        _legend.IsVisible = shown;
        if (!shown || _colors is null) return;
        var groups = ChartLegendModel.Ordered(LegendGroups, LegendOrder);
        var hasDeactivated = _selected.Count > 0 && _selected.Count < _groups.Count;
        _legend.Update(groups, g => _selected.Count == 0 || _selected.Contains(g), hasDeactivated, IsLegendClickable, _colors, LegendExtras);
    }

    /// <inheritdoc/>
    protected override void OnContextChanged()
    {
        base.OnContextChanged();
        if (_scene == ChartScene.Empty) return;
        var colors = ResolveColors();
        _canvas.Show(_scene, colors);
        _labels.Show(_scene.Texts, colors, _legendHover);
        UpdateLegend();
    }

    void OnDataChanged(IEnumerable? oldData, IEnumerable? newData)
    {
        UnsubscribeCollection();
        if (IsLoaded) SubscribeCollection();
        _itemsDirty = true;
        RequestRebuild();
    }

    void SubscribeCollection()
    {
        UnsubscribeCollection();
        if (Data is not INotifyCollectionChanged incc) return;
        NotifyCollectionChangedEventHandler? adapter = null;
        _collectionSubscription = new CdsWeakSubscription<CdsChart>(this, static c =>
        {
            c._itemsDirty = true;
            c.RequestRebuild();
        }, h =>
        {
            adapter = (s, e) => h(s, e);
            incc.CollectionChanged += adapter;
        }, _ =>
        {
            if (adapter is not null) incc.CollectionChanged -= adapter;
        });
    }

    void UnsubscribeCollection()
    {
        _collectionSubscription?.Dispose();
        _collectionSubscription = null;
    }

    // ----- legend -----

    void OnSelectedGroupsChanged(IReadOnlyList<string>? groups)
    {
        if (_syncingSelection) return;
        _selected = groups?.ToList() ?? [];
        RequestRebuild();
    }

    void SetSelected(IReadOnlyList<string> next, bool raise)
    {
        _selected = next;
        _syncingSelection = true;
        try
        {
            SelectedGroups = next.Count == 0 ? null : next;
        }
        finally
        {
            _syncingSelection = false;
        }
        if (raise) SelectedGroupsChanged?.Invoke(this, new CdsChartSelectedGroupsChangedEventArgs(next));
    }

    /// <summary>Applies a legend click (Carbon's toggle semantics).</summary>
    internal void ToggleGroup(string group)
    {
        SetSelected(ChartLegendModel.Toggle(_selected, _groups, group), raise: true);
        RequestRebuild();
    }

    /// <summary>Fades the other series while a legend item is hovered or focused.</summary>
    internal void HoverLegend(string? group, string source)
    {
        if (group is null && _legendHover != source) return;
        if (_legendHover == group) return;
        _legendHover = group;
        _canvas.SetState(new ChartDrawState(_active, _legendHover, _shimmerPhase));
        if (_colors is not null && _scene.Texts.Any(t => t.FadesWithLegend)) _labels.Show(_scene.Texts, _colors, _legendHover);
    }

    // ----- data cursor, hover and tooltip -----

    void OnPointer(Point? position, bool touch)
    {
        if (position is not { } p) return;
        _pointer = new PointF((float)p.X, (float)p.Y);
        var mark = _scene.HitTest(_pointer.Value);
        if (mark is null)
        {
            if (!touch) ClearActive();
            return;
        }
        SetActive(mark.Id, _pointer);
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        // Enter, Space or a tap without a position: start the data cursor at the first value.
        if (_active < 0 && _scene.Marks.FirstOrDefault(m => !m.IsReference) is { } first) SetActive(first.Id, null);
    }

    void MoveCursor(int delta)
    {
        var marks = _scene.Marks.Where(m => !m.IsReference).ToList();
        if (marks.Count == 0) return;
        var i = marks.FindIndex(m => m.Id == _active);
        var next = i < 0 ? (delta > 0 ? 0 : marks.Count - 1) : Math.Clamp(i + delta, 0, marks.Count - 1);
        SetActive(marks[next].Id, null);
    }

    void SetActive(int id, PointF? at)
    {
        _active = id;
        _canvas.SetState(new ChartDrawState(_active, _legendHover, _shimmerPhase));
        ShowActive(at);
        UpdatePlotSemantics();
    }

    internal void ClearActive()
    {
        if (_active < 0 && !_tooltip.IsVisible) return;
        _active = -1;
        _canvas.SetState(new ChartDrawState(-1, _legendHover, _shimmerPhase));
        _tooltip.Hide();
        UpdatePlotSemantics();
    }

    void ShowActive(PointF? at = null)
    {
        if (_scene.FindMark(_active) is not { } mark || _colors is null) return;
        if (!IsTooltipEnabled || (mark.Rows.Count == 0 && string.IsNullOrEmpty(mark.Title)))
        {
            _tooltip.Hide();
            return;
        }
        _tooltip.Show(mark, _colors);
        PlaceTooltip(at ?? _pointer ?? mark.AnchorPoint);
    }

    void PlaceTooltip(PointF plotPoint)
    {
        var origin = OffsetInRoot(_plot);
        var px = origin.X + plotPoint.X;
        var py = origin.Y + plotPoint.Y;
        var size = ((IView)_tooltip).Measure(270, double.PositiveInfinity);
        var w = Math.Min(270, size.Width);
        var h = size.Height;
        const double gap = 10;
        var x = px + gap;
        if (x + w > _root.Width && px - gap - w >= 0) x = px - gap - w;
        var y = Math.Clamp(py - h / 2, 0, Math.Max(0, _root.Height - h));
        AbsoluteLayout.SetLayoutBounds(_tooltip, new Rect(x, y, w, h));
    }

    Point OffsetInRoot(VisualElement view)
    {
        double x = 0, y = 0;
        for (Element? e = view; e is VisualElement ve && !ReferenceEquals(ve, _root); e = e.Parent)
        {
            x += ve.X;
            y += ve.Y;
        }
        return new Point(x, y);
    }

    void UpdatePlotSemantics()
    {
        var name = string.IsNullOrWhiteSpace(Title) ? ChartKind : $"{Title}, {ChartKind.ToLowerInvariant()}";
        if (!string.IsNullOrEmpty(_scene.Summary)) name += $", {_scene.Summary}";
        SemanticProperties.SetDescription(_hit, name);
        SemanticProperties.SetHint(_hit, _scene.Marks.Count > 0 ? "Use the arrow keys to move between values. Show as table lists every value." : null);
        var marks = _scene.Marks.Where(m => !m.IsReference).ToList();
        var i = marks.FindIndex(m => m.Id == _active);
        _hit.AccessibleValue = i >= 0 ? $"{marks[i].SpokenText}, {i + 1} of {marks.Count}" : null;
    }

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected internal override bool WantsEscapeKey => _active >= 0;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (!_hit.IsFocused) return false;
        switch (e.Key)
        {
            case CdsKey.Right:
            case CdsKey.Down:
                MoveCursor(1);
                return true;
            case CdsKey.Left:
            case CdsKey.Up:
                MoveCursor(-1);
                return true;
            case CdsKey.Home:
                MoveCursor(-_scene.Marks.Count);
                return true;
            case CdsKey.End:
                MoveCursor(_scene.Marks.Count);
                return true;
            case CdsKey.Escape when _active >= 0:
                ClearActive();
                return true;
        }
        return false;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    // ----- layout -----

    void OnChromeChanged()
    {
        BuildToolbar();
        UpdatePlotSemantics();
    }

    void OnFrameChanged()
    {
        BuildFrame();
        RequestRebuild();
    }

    void BuildFrame()
    {
        _frame.Clear();
        _frame.RowDefinitions.Clear();
        _frame.ColumnDefinitions.Clear();
        var legend = IsLegendEnabled ?? DefaultLegendEnabled;
        var position = LegendPosition;
        var vertical = position is CdsChartLegendPosition.Left or CdsChartLegendPosition.Right;
        _legend.SetOrientation(vertical, LegendAlignment);
        if (!legend)
        {
            _frame.Add(_plot);
            return;
        }
        var gap = CdsSpacing.Spacing06;
        switch (position)
        {
            case CdsChartLegendPosition.Top:
                _frame.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _frame.RowDefinitions.Add(new RowDefinition(new GridLength(gap)));
                _frame.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                _frame.Add(_legend, 0, 0);
                _frame.Add(_plot, 0, 2);
                break;
            case CdsChartLegendPosition.Left:
                _frame.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _frame.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(gap)));
                _frame.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                _frame.Add(_legend, 0, 0);
                _frame.Add(_plot, 2, 0);
                break;
            case CdsChartLegendPosition.Right:
                _frame.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                _frame.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(gap)));
                _frame.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                _frame.Add(_plot, 0, 0);
                _frame.Add(_legend, 2, 0);
                break;
            default:
                _frame.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                _frame.RowDefinitions.Add(new RowDefinition(new GridLength(gap)));
                _frame.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _frame.Add(_plot, 0, 0);
                _frame.Add(_legend, 0, 2);
                break;
        }
        _legend.VerticalOptions = vertical ? LayoutOptions.Start : LayoutOptions.Fill;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        if (_fullscreen is not null) return _lastSize;
        if (double.IsInfinity(heightConstraint) && HeightRequest < 0) heightConstraint = DefaultHeight;
        var size = base.MeasureOverride(widthConstraint, heightConstraint);
        if (HeightRequest < 0 && !double.IsInfinity(heightConstraint)) size = new Size(size.Width, Math.Max(size.Height, heightConstraint));
        if (double.IsInfinity(widthConstraint) && WidthRequest < 0) size = new Size(Math.Max(size.Width, 320), size.Height);
        return size;
    }

    /// <inheritdoc/>
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        if (_fullscreen is null) _lastSize = new Size(width, height);
    }

    // ----- shimmer -----

    void UpdateShimmer()
    {
        if (_scene.Shimmer && IsLoaded && !CdsMotion.IsReducedMotion)
        {
            CdsLoop.Start(this, ShimmerAnimation, phase =>
            {
                _shimmerPhase = (float)phase;
                _canvas.SetState(new ChartDrawState(_active, _legendHover, _shimmerPhase));
            }, 0.5, 2000);
        }
        else
        {
            CdsLoop.Stop(this, ShimmerAnimation);
            _shimmerPhase = 0.5f;
        }
    }
}
