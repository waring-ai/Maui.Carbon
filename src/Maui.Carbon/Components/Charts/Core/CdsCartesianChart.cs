namespace Maui.Carbon;

/// <summary>
/// Base class of the axis charts (bars, lines, areas, scatter and the rest): up to four <see cref="CdsChartAxis"/> axes,
/// the grid, thresholds and, for line, area and scatter charts, the ruler. Set only the axes you want drawn; with none
/// the chart draws a linear value axis on the left and a labels axis at the bottom.
/// </summary>
public abstract class CdsCartesianChart : CdsChart
{
    /// <summary>Bindable property for <see cref="LeftAxis"/>.</summary>
    public static readonly BindableProperty LeftAxisProperty = AxisProperty(nameof(LeftAxis));

    /// <summary>Bindable property for <see cref="BottomAxis"/>.</summary>
    public static readonly BindableProperty BottomAxisProperty = AxisProperty(nameof(BottomAxis));

    /// <summary>Bindable property for <see cref="RightAxis"/>.</summary>
    public static readonly BindableProperty RightAxisProperty = AxisProperty(nameof(RightAxis));

    /// <summary>Bindable property for <see cref="TopAxis"/>.</summary>
    public static readonly BindableProperty TopAxisProperty = AxisProperty(nameof(TopAxis));

    /// <summary>Bindable property for <see cref="IsXGridEnabled"/>.</summary>
    public static readonly BindableProperty IsXGridEnabledProperty = BindableProperty.Create(
        nameof(IsXGridEnabled), typeof(bool?), typeof(CdsCartesianChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="IsYGridEnabled"/>.</summary>
    public static readonly BindableProperty IsYGridEnabledProperty = BindableProperty.Create(
        nameof(IsYGridEnabled), typeof(bool?), typeof(CdsCartesianChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="IsRulerEnabled"/>.</summary>
    public static readonly BindableProperty IsRulerEnabledProperty = BindableProperty.Create(
        nameof(IsRulerEnabled), typeof(bool), typeof(CdsCartesianChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="TimeInterval"/>.</summary>
    public static readonly BindableProperty TimeIntervalProperty = BindableProperty.Create(
        nameof(TimeInterval), typeof(CdsChartTimeInterval), typeof(CdsCartesianChart), CdsChartTimeInterval.Auto, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="TimeEdgeSpace"/>.</summary>
    public static readonly BindableProperty TimeEdgeSpaceProperty = BindableProperty.Create(
        nameof(TimeEdgeSpace), typeof(int?), typeof(CdsCartesianChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ShowDayName"/>.</summary>
    public static readonly BindableProperty ShowDayNameProperty = BindableProperty.Create(
        nameof(ShowDayName), typeof(bool), typeof(CdsCartesianChart), false, propertyChanged: Changed);

    private protected CdsCartesianChart()
    {
    }

    /// <summary>The left axis (Carbon's <c>axes.left</c>).</summary>
    public CdsChartAxis? LeftAxis
    {
        get => (CdsChartAxis?)GetValue(LeftAxisProperty);
        set => SetValue(LeftAxisProperty, value);
    }

    /// <summary>The bottom axis (Carbon's <c>axes.bottom</c>).</summary>
    public CdsChartAxis? BottomAxis
    {
        get => (CdsChartAxis?)GetValue(BottomAxisProperty);
        set => SetValue(BottomAxisProperty, value);
    }

    /// <summary>The right axis: a secondary value axis for <see cref="CdsChartAxis.CorrespondingDatasets"/>, or the main one with <see cref="CdsChartAxis.IsMain"/>.</summary>
    public CdsChartAxis? RightAxis
    {
        get => (CdsChartAxis?)GetValue(RightAxisProperty);
        set => SetValue(RightAxisProperty, value);
    }

    /// <summary>The top axis.</summary>
    public CdsChartAxis? TopAxis
    {
        get => (CdsChartAxis?)GetValue(TopAxisProperty);
        set => SetValue(TopAxisProperty, value);
    }

    /// <summary>Vertical grid lines (Carbon's <c>grid.x.enabled</c>); <c>null</c> follows the chart (on, except bullet charts).</summary>
    public bool? IsXGridEnabled
    {
        get => (bool?)GetValue(IsXGridEnabledProperty);
        set => SetValue(IsXGridEnabledProperty, value);
    }

    /// <summary>Horizontal grid lines (Carbon's <c>grid.y.enabled</c>); <c>null</c> follows the chart.</summary>
    public bool? IsYGridEnabled
    {
        get => (bool?)GetValue(IsYGridEnabledProperty);
        set => SetValue(IsYGridEnabledProperty, value);
    }

    /// <summary>
    /// The ruler of line, area, scatter and stacked charts: hovering anywhere shows a dashed line and every series at that
    /// position. Off, each mark shows its own tooltip. Default <c>true</c>.
    /// </summary>
    public bool IsRulerEnabled
    {
        get => (bool)GetValue(IsRulerEnabledProperty);
        set => SetValue(IsRulerEnabledProperty, value);
    }

    /// <summary>The time axis tick interval; default <see cref="CdsChartTimeInterval.Auto"/> picks it from the data.</summary>
    public CdsChartTimeInterval TimeInterval
    {
        get => (CdsChartTimeInterval)GetValue(TimeIntervalProperty);
        set => SetValue(TimeIntervalProperty, value);
    }

    /// <summary>Padding on each end of a time axis, in time units (Carbon's <c>timeScale.addSpaceOnEdges</c>); <c>null</c> follows the chart (1 for bars and lines, 0 for areas).</summary>
    public int? TimeEdgeSpace
    {
        get => (int?)GetValue(TimeEdgeSpaceProperty);
        set => SetValue(TimeEdgeSpaceProperty, value);
    }

    /// <summary>Daily time ticks show the day name ("Mon").</summary>
    public bool ShowDayName
    {
        get => (bool)GetValue(ShowDayNameProperty);
        set => SetValue(ShowDayNameProperty, value);
    }

    // ----- what each axis chart provides -----

    /// <summary>Adds the chart's marks (bars, lines, dots) to the scene; the frame draws the grid before and the axes after.</summary>
    private protected abstract void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs);

    /// <summary>The axes drawn when none is set: left linear values, bottom labels.</summary>
    private protected virtual IReadOnlyDictionary<AxisPosition, AxisSpec> FallbackAxes => new Dictionary<AxisPosition, AxisSpec>
    {
        [AxisPosition.Left] = new() { Position = AxisPosition.Left, ScaleType = CdsChartScaleType.Linear },
        [AxisPosition.Bottom] = new() { Position = AxisPosition.Bottom, ScaleType = CdsChartScaleType.Labels },
    };

    /// <summary>Whether the chart uses the ruler (line, area, scatter, stacked area).</summary>
    private protected virtual bool UsesRuler => false;

    private protected virtual int DefaultTimeEdgeSpace => 1;

    private protected virtual bool DefaultXGrid => true;

    private protected virtual bool DefaultYGrid => true;

    /// <summary>Layout options of the chart (stacking, custom extents).</summary>
    private protected virtual CartesianOptions Options(ChartBuildContext ctx) => new();

    /// <summary>Whether the plot clips marks (bars and lines never draw outside the axes).</summary>
    private protected virtual bool ClipMarks => true;

    /// <summary>Builds this chart's marks into another chart's frame (the combo chart draws each graph type with its chart).</summary>
    internal void BuildMarksInto(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs) => BuildMarks(scene, ctx, cs);

    /// <summary>The configured axes (or the fallback).</summary>
    internal IReadOnlyDictionary<AxisPosition, AxisSpec> ResolvedAxes()
    {
        var axes = new Dictionary<AxisPosition, AxisSpec>();
        if (LeftAxis is { } l) axes[AxisPosition.Left] = AxisSpec.From(AxisPosition.Left, l);
        if (BottomAxis is { } b) axes[AxisPosition.Bottom] = AxisSpec.From(AxisPosition.Bottom, b);
        if (RightAxis is { } r) axes[AxisPosition.Right] = AxisSpec.From(AxisPosition.Right, r);
        if (TopAxis is { } t) axes[AxisPosition.Top] = AxisSpec.From(AxisPosition.Top, t);
        if (axes.Count == 0) return FallbackAxes;
        // A chart needs one vertical and one horizontal axis: fill a missing side from the fallback (or its opposite).
        var fallback = FallbackAxes;
        if (!axes.ContainsKey(AxisPosition.Left) && !axes.ContainsKey(AxisPosition.Right))
        {
            var horizontalIsDomain = axes.Values.Any(a => !a.IsVertical && a.ScaleType is CdsChartScaleType.Labels or CdsChartScaleType.Time);
            axes[AxisPosition.Left] = horizontalIsDomain || !fallback.TryGetValue(AxisPosition.Left, out var fl)
                ? new AxisSpec { Position = AxisPosition.Left, ScaleType = CdsChartScaleType.Linear }
                : fl;
        }
        if (!axes.ContainsKey(AxisPosition.Bottom) && !axes.ContainsKey(AxisPosition.Top))
        {
            var verticalIsDomain = axes.Values.Any(a => a.IsVertical && a.ScaleType is CdsChartScaleType.Labels or CdsChartScaleType.Time);
            axes[AxisPosition.Bottom] = verticalIsDomain
                ? new AxisSpec { Position = AxisPosition.Bottom, ScaleType = CdsChartScaleType.Linear }
                : fallback.TryGetValue(AxisPosition.Bottom, out var fb) ? fb : new AxisSpec { Position = AxisPosition.Bottom, ScaleType = CdsChartScaleType.Labels };
        }
        return axes;
    }

    internal CartesianLayout Layout(ChartBuildContext ctx)
    {
        var o = Options(ctx);
        var options = new CartesianOptions
        {
            Stacked = o.Stacked,
            RangeValues = o.RangeValues,
            DomainCategories = o.DomainCategories,
            Rows = o.Rows,
            TimeEdgeSpace = TimeEdgeSpace ?? DefaultTimeEdgeSpace,
            TimeInterval = TimeInterval,
            ShowDayName = ShowDayName,
        };
        return new CartesianLayout(ctx, ResolvedAxes(), options);
    }

    private protected sealed override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var cs = Layout(ctx);
        var ruler = UsesRuler && IsRulerEnabled;
        var scene = new ChartScene
        {
            Width = ctx.Width,
            Height = ctx.Height,
            Plot = cs.Plot,
            HitMode = ruler ? ChartHitMode.Ruler : ChartHitMode.Contains,
            RulerVertical = cs.Vertical,
        };
        CartesianFrame.Grid(scene, cs, IsXGridEnabled ?? DefaultXGrid, IsYGridEnabled ?? DefaultYGrid);
        var first = scene.Shapes.Count;
        BuildMarks(scene, ctx, cs);
        if (ClipMarks) for (var i = first; i < scene.Shapes.Count; i++) scene.Shapes[i].Clip = true;
        CartesianFrame.ZeroLine(scene, cs);
        CartesianFrame.Axes(scene, cs);
        CartesianFrame.Thresholds(scene, ctx, cs);
        return scene;
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx) => CartesianFrame.Table(ctx, Layout(ctx));

    private protected override string ChartKind => "Chart";

    static BindableProperty AxisProperty(string name) => BindableProperty.Create(
        name, typeof(CdsChartAxis), typeof(CdsCartesianChart), null,
        propertyChanged: static (b, o, n) => ((CdsCartesianChart)b).OnAxisChanged((CdsChartAxis?)o, (CdsChartAxis?)n));

    void OnAxisChanged(CdsChartAxis? oldAxis, CdsChartAxis? newAxis)
    {
        if (oldAxis is not null) oldAxis.Changed -= OnAxisSettingChanged;
        if (newAxis is not null)
        {
            newAxis.Changed += OnAxisSettingChanged;
            SetInheritedBindingContext(newAxis, BindingContext);
        }
        RequestRebuild();
    }

    void OnAxisSettingChanged(object? sender, EventArgs e) => RequestRebuild();

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        foreach (var axis in new[] { LeftAxis, BottomAxis, RightAxis, TopAxis })
            if (axis is not null) SetInheritedBindingContext(axis, BindingContext);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsCartesianChart)b).RequestRebuild();
}

/// <summary>The parts of an axis chart around its marks: grid, zero line, axes, thresholds, tooltips and the table.</summary>
internal static class CartesianFrame
{
    /// <summary>Carbon's grid: the <c>grid-bg</c> backdrop (outlined when lines show) and <c>layer-accent-01</c> lines.</summary>
    public static void Grid(ChartScene scene, CartesianLayout cs, bool xOn, bool yOn)
    {
        var p = cs.Plot;
        var accent = ChartPaint.OfKey(CdsColorKeys.LayerAccent01);
        scene.Shapes.Add(new ChartRect
        {
            Rect = p,
            Fill = ChartPaint.OfToken(CdsChartToken.GridBg),
            Stroke = xOn || yOn ? accent : ChartPaint.None,
            StrokeWidth = 1,
        });
        AxisPosition? xPos = cs.Axes.ContainsKey(AxisPosition.Bottom) ? AxisPosition.Bottom : cs.Axes.ContainsKey(AxisPosition.Top) ? AxisPosition.Top : null;
        AxisPosition? yPos = cs.Axes.ContainsKey(AxisPosition.Left)
            ? (cs.Axes[AxisPosition.Left].IsMain || !cs.Axes.ContainsKey(AxisPosition.Right) || !cs.Axes[AxisPosition.Right].IsMain ? AxisPosition.Left : AxisPosition.Right)
            : cs.Axes.ContainsKey(AxisPosition.Right) ? AxisPosition.Right : null;
        IEnumerable<double> Lines(AxisPosition pos, int n)
        {
            var s = cs.Scales[pos];
            if (s.Band is { } band) return band.Domain.Select(band.Center);
            return s.Continuous!.Ticks(n).Select(s.Map);
        }
        if (xOn && xPos is { } xp)
            foreach (var x in Lines(xp, 15))
                if (x > p.Left + 0.5 && x < p.Right - 0.5)
                {
                    var sx = MathF.Round((float)x) + 0.5f;
                    scene.Shapes.Add(new ChartLine { From = new(sx, p.Top), To = new(sx, p.Bottom), Stroke = accent });
                }
        if (yOn && yPos is { } yp)
            foreach (var y in Lines(yp, 5))
                if (y > p.Top + 0.5 && y < p.Bottom - 0.5)
                {
                    var sy = MathF.Round((float)y) + 0.5f;
                    scene.Shapes.Add(new ChartLine { From = new(p.Left, sy), To = new(p.Right, sy), Stroke = accent });
                }
    }

    /// <summary>The zero line, when a linear range axis crosses zero.</summary>
    public static void ZeroLine(ChartScene scene, CartesianLayout cs)
    {
        var s = cs.RangeScale;
        if (s.Spec.ScaleType != CdsChartScaleType.Linear) return;
        var (min, max) = s.Domain;
        if (!(min < 0 && max > 0)) return;
        var v = MathF.Round((float)s.Map(0)) + 0.5f;
        var p = cs.Plot;
        var stroke = ChartPaint.OfKey(CdsColorKeys.BorderStrong01);
        scene.Shapes.Add(cs.Vertical
            ? new ChartLine { From = new(p.Left, v), To = new(p.Right, v), Stroke = stroke }
            : new ChartLine { From = new(v, p.Top), To = new(v, p.Bottom), Stroke = stroke });
    }

    /// <summary>Axis lines (<c>border-strong-01</c>), tick labels (12px Condensed, <c>text-secondary</c>; primary time ticks bold) and titles.</summary>
    public static void Axes(ChartScene scene, CartesianLayout cs)
    {
        var p = cs.Plot;
        const float pad = 9;
        var domainStroke = ChartPaint.OfKey(CdsColorKeys.BorderStrong01);
        foreach (var (pos, a) in cs.Axes)
        {
            if (!a.IsVisible) continue;
            switch (pos)
            {
                case AxisPosition.Left: scene.Shapes.Add(new ChartLine { From = new(p.Left + 0.5f, p.Top), To = new(p.Left + 0.5f, p.Bottom), Stroke = domainStroke }); break;
                case AxisPosition.Right: scene.Shapes.Add(new ChartLine { From = new(p.Right - 0.5f, p.Top), To = new(p.Right - 0.5f, p.Bottom), Stroke = domainStroke }); break;
                case AxisPosition.Bottom: scene.Shapes.Add(new ChartLine { From = new(p.Left, p.Bottom - 0.5f), To = new(p.Right, p.Bottom - 0.5f), Stroke = domainStroke }); break;
                default: scene.Shapes.Add(new ChartLine { From = new(p.Left, p.Top + 0.5f), To = new(p.Right, p.Top + 0.5f), Stroke = domainStroke }); break;
            }
            var rotated = cs.Rotated.TryGetValue(pos, out var r) && r;
            foreach (var t in cs.Ticks[pos])
            {
                if (double.IsNaN(t.Pixel)) continue;
                var at = (float)t.Pixel;
                var font = t.Primary ? ChartFont.TickPrimary : ChartFont.Tick;
                scene.Texts.Add(pos switch
                {
                    AxisPosition.Left => new ChartText { Text = t.Label, X = p.Left - pad, Y = at, Anchor = ChartTextAnchor.End, Baseline = ChartTextBaseline.Middle, Font = font },
                    AxisPosition.Right => new ChartText { Text = t.Label, X = p.Right + pad, Y = at, Anchor = ChartTextAnchor.Start, Baseline = ChartTextBaseline.Middle, Font = font },
                    AxisPosition.Bottom when rotated => new ChartText { Text = t.Label, X = at, Y = p.Bottom + pad, Anchor = ChartTextAnchor.End, Baseline = ChartTextBaseline.Middle, Rotation = -45, Font = font },
                    AxisPosition.Bottom => new ChartText { Text = t.Label, X = at, Y = p.Bottom + pad - 2, Anchor = ChartTextAnchor.Middle, Baseline = ChartTextBaseline.Top, Font = font },
                    _ when rotated => new ChartText { Text = t.Label, X = at, Y = p.Top - pad, Anchor = ChartTextAnchor.Start, Baseline = ChartTextBaseline.Middle, Rotation = -45, Font = font },
                    _ => new ChartText { Text = t.Label, X = at, Y = p.Top - pad + 2, Anchor = ChartTextAnchor.Middle, Baseline = ChartTextBaseline.Bottom, Font = font },
                });
            }
            if (string.IsNullOrEmpty(a.Title)) continue;
            var title = ChartPaint.OfKey(CdsColorKeys.TextPrimary);
            scene.Texts.Add(pos switch
            {
                AxisPosition.Left => new ChartText { Text = a.Title!, X = 8, Y = (p.Top + p.Bottom) / 2, Anchor = ChartTextAnchor.Middle, Baseline = ChartTextBaseline.Middle, Rotation = -90, Font = ChartFont.AxisTitle, Color = title },
                AxisPosition.Right => new ChartText { Text = a.Title!, X = scene.Width - 8, Y = (p.Top + p.Bottom) / 2, Anchor = ChartTextAnchor.Middle, Baseline = ChartTextBaseline.Middle, Rotation = 90, Font = ChartFont.AxisTitle, Color = title },
                AxisPosition.Bottom => new ChartText { Text = a.Title!, X = (p.Left + p.Right) / 2, Y = scene.Height, Anchor = ChartTextAnchor.Middle, Baseline = ChartTextBaseline.Bottom, Font = ChartFont.AxisTitle, Color = title },
                _ => new ChartText { Text = a.Title!, X = (p.Left + p.Right) / 2, Y = 0, Anchor = ChartTextAnchor.Middle, Baseline = ChartTextBaseline.Top, Font = ChartFont.AxisTitle, Color = title },
            });
        }
    }

    /// <summary>Threshold lines: dashed 4, threshold red by default, 2px with "Label: value" on hover.</summary>
    public static void Thresholds(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var id = 900_000;
        foreach (var (pos, a) in cs.Axes)
        {
            var s = cs.Scales[pos];
            foreach (var t in a.Thresholds)
            {
                double px;
                string valueText;
                if (s.Band is { } band)
                {
                    px = band.Center(t.Key);
                    valueText = t.Key ?? "";
                }
                else if (a.ScaleType == CdsChartScaleType.Time)
                {
                    if (t.Date is not { } d) continue;
                    px = s.Map(ChartTime.ToMs(d));
                    valueText = string.IsNullOrEmpty(t.ValueFormat) ? ctx.Date(d) : d.ToString(t.ValueFormat, ctx.Culture);
                }
                else
                {
                    if (t.Value is not { } v) continue;
                    px = s.Map(v);
                    valueText = CdsChartFormat.Number(v, ctx.Culture, t.ValueFormat);
                }
                if (double.IsNaN(px)) continue;
                var p = cs.Plot;
                var x = (float)px;
                var markId = id++;
                var line = a.IsVertical
                    ? new ChartLine { From = new(p.Left, x), To = new(p.Right, x) }
                    : new ChartLine { From = new(x, p.Top), To = new(x, p.Bottom) };
                line.Stroke = t.ColorKey is { } key && CdsPalettes.IsColorKey(key) ? ChartPaint.OfKey(key) : ChartPaint.OfToken(CdsChartToken.Threshold);
                line.Dash = [4, 4];
                line.Mark = markId;
                line.HoverStrokeWidth = 2;
                line.Hover = ChartHoverStyle.None;
                scene.Shapes.Add(line);
                scene.Marks.Add(new ChartMark
                {
                    Id = markId,
                    Bounds = a.IsVertical ? new RectF(p.Left, x - 10, p.Width, 20) : new RectF(x - 10, p.Top, 20, p.Height),
                    Title = (string.IsNullOrEmpty(t.Label) ? "Threshold" : t.Label) + ": " + valueText,
                    IsReference = true,
                });
            }
        }
    }

    /// <summary>
    /// Carbon's axis tooltip: one datum shows the domain, its value (or start and end) and its group; several (the ruler)
    /// list every group, largest first, then the total.
    /// </summary>
    public static List<ChartTooltipRow> TooltipRows(ChartBuildContext ctx, CartesianLayout cs, IReadOnlyList<CdsChartPoint> points, IEnumerable<ChartTooltipRow>? extra = null)
    {
        var rows = new List<ChartTooltipRow>();
        if (points.Count == 0) return rows;
        rows.Add(new ChartTooltipRow(Truncate(cs.DomainLabel), cs.DomainText(points[0])));
        if (points.Count == 1)
        {
            var d = points[0];
            if (d.HasRange)
            {
                rows.Add(new ChartTooltipRow("Start", ctx.Value(d.Low)));
                rows.Add(new ChartTooltipRow("End", ctx.Value(d.High)));
            }
            else rows.Add(new ChartTooltipRow(Truncate(cs.RangeLabel(d.Group)), ctx.Value(d.Value)));
            if (extra is not null) rows.AddRange(extra);
            rows.Add(new ChartTooltipRow(ctx.GroupLabel, Truncate(d.Group ?? ""), ChartPaint.OfSeries(d.Group)));
            return rows;
        }
        foreach (var d in points.OrderByDescending(p => p.HasRange ? p.High : p.Value ?? double.MinValue))
            rows.Add(new ChartTooltipRow(Truncate(d.Group ?? ""), d.HasRange ? ctx.Value(d.Low) + " - " + ctx.Value(d.High) : ctx.Value(d.Value), ChartPaint.OfSeries(d.Group)));
        if (!cs.IsDual && ctx.ShowTotal)
            rows.Add(new ChartTooltipRow(ctx.TotalLabel, ctx.Value(points.Sum(p => p.Value ?? 0)), Bold: true));
        return rows;
    }

    static string Truncate(string s) => CdsChartTruncate.Apply(s);

    /// <summary>Carbon's axis-chart table: Group, the domain, then each range axis.</summary>
    public static ChartTable Table(ChartBuildContext ctx, CartesianLayout cs)
    {
        var headers = new List<string> { ctx.GroupLabel, cs.Axes[cs.DomainPosition].Title ?? DomainField(cs.DomainType) };
        var ranges = cs.Axes.Keys.Where(k => k != cs.DomainPosition).ToList();
        foreach (var r in ranges) headers.Add(cs.Axes[r].Title ?? (cs.Axes[r].ScaleType == CdsChartScaleType.Labels ? "Key" : "Value"));
        var rows = ctx.ActivePoints.Select(p =>
        {
            var row = new List<string> { p.Group ?? "", cs.DomainText(p) };
            foreach (var r in ranges)
            {
                var onThisAxis = cs.RangePositionOf(p.Group) == r || !cs.IsDual;
                row.Add(!onThisAxis ? "" : p.HasRange ? ctx.Value(p.Low) + " - " + ctx.Value(p.High) : ctx.Value(p.Value));
            }
            return row.ToArray();
        }).ToList();
        return new ChartTable(headers, rows);
    }

    static string DomainField(CdsChartScaleType t) => t switch
    {
        CdsChartScaleType.Labels => "Key",
        CdsChartScaleType.Time => "Date",
        _ => "Value",
    };

    /// <summary>
    /// Ruler marks: one per domain position, listing every series there, with the ruler line and shared mark id for the
    /// dots drawn at it. Returns the mark id of each domain key.
    /// </summary>
    public static Dictionary<string, int> RulerMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs, IReadOnlyList<CdsChartPoint> rows, int firstId = 0,
        Func<IReadOnlyList<CdsChartPoint>, List<ChartTooltipRow>>? rowsFor = null)
    {
        var ids = new Dictionary<string, int>();
        var columns = rows.Where(p => p.Value.HasValue || p.HasRange)
            .GroupBy(cs.DomainKey)
            .Select(g => (Key: g.Key, Points: g.ToList(), Px: cs.DomainPx(g.First())))
            .Where(c => !double.IsNaN(c.Px))
            .OrderBy(c => c.Px)
            .ToList();
        var id = firstId;
        foreach (var c in columns)
        {
            var px = (float)c.Px;
            var p = cs.Plot;
            ids[c.Key] = id;
            var anchor = cs.XY(px, c.Points.Select(pt => cs.RangePx(pt.Group, pt.Value ?? pt.High ?? 0)).DefaultIfEmpty(cs.Vertical ? p.Center.Y : p.Center.X).Min());
            scene.Marks.Add(new ChartMark
            {
                Id = id++,
                Bounds = cs.Vertical ? new RectF(px - 4, p.Top, 8, p.Height) : new RectF(p.Left, px - 4, p.Width, 8),
                RulerAt = px,
                Anchor = anchor,
                Rows = rowsFor?.Invoke(c.Points) ?? TooltipRows(ctx, cs, c.Points),
            });
        }
        return ids;
    }
}
