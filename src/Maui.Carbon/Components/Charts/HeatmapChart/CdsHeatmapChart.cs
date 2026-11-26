namespace Maui.Carbon;

/// <summary>
/// A grid of cells on two category axes, each colored by its value (Carbon's heat map): weekday by hour, service by
/// severity, region by quarter. Each <see cref="CdsChartPoint"/> is one cell: <see cref="CdsChartPoint.Key"/> is its column
/// (the bottom axis), <see cref="CdsChartPoint.Group"/> its row (the left axis, first row at the bottom) and
/// <see cref="CdsChartPoint.Value"/> its value; a missing pair or a <c>null</c> value is a striped null-state cell.
/// Colors come from a quantize scale over the value domain: 11 sequential steps (<see cref="CdsChart.ColorPairingOption"/>
/// 1 purple, 2 blue, 3 cyan, 4 teal) or, when the domain crosses 0, 17 diverging steps (1 red-cyan, 2 purple-teal).
/// Color is the only encoding of the value: keep the color legend on with a unit in its title, and keep "Show as table".
/// </summary>
public class CdsHeatmapChart : CdsChart
{
    /// <summary>Bindable property for <see cref="BottomAxis"/>.</summary>
    public static readonly BindableProperty BottomAxisProperty = AxisProperty(nameof(BottomAxis));

    /// <summary>Bindable property for <see cref="LeftAxis"/>.</summary>
    public static readonly BindableProperty LeftAxisProperty = AxisProperty(nameof(LeftAxis));

    /// <summary>Bindable property for <see cref="ColorLegendTitle"/>.</summary>
    public static readonly BindableProperty ColorLegendTitleProperty = BindableProperty.Create(
        nameof(ColorLegendTitle), typeof(string), typeof(CdsHeatmapChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ColorLegendType"/>.</summary>
    public static readonly BindableProperty ColorLegendTypeProperty = BindableProperty.Create(
        nameof(ColorLegendType), typeof(CdsChartColorLegendType), typeof(CdsHeatmapChart), CdsChartColorLegendType.Linear, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ColorDomainMinimum"/>.</summary>
    public static readonly BindableProperty ColorDomainMinimumProperty = BindableProperty.Create(
        nameof(ColorDomainMinimum), typeof(double?), typeof(CdsHeatmapChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ColorDomainMaximum"/>.</summary>
    public static readonly BindableProperty ColorDomainMaximumProperty = BindableProperty.Create(
        nameof(ColorDomainMaximum), typeof(double?), typeof(CdsHeatmapChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Divider"/>.</summary>
    public static readonly BindableProperty DividerProperty = BindableProperty.Create(
        nameof(Divider), typeof(CdsHeatmapDivider), typeof(CdsHeatmapChart), CdsHeatmapDivider.Auto, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="GradientColors"/>.</summary>
    public static readonly BindableProperty GradientColorsProperty = BindableProperty.Create(
        nameof(GradientColors), typeof(IList<Color>), typeof(CdsHeatmapChart), null, propertyChanged: Changed);

    /// <summary>The column axis (Carbon's <c>axes.bottom</c>, a labels axis): its title names the dimension; <see cref="CdsChartAxis.Categories"/> fixes the column order (default: data order).</summary>
    public CdsChartAxis? BottomAxis
    {
        get => (CdsChartAxis?)GetValue(BottomAxisProperty);
        set => SetValue(BottomAxisProperty, value);
    }

    /// <summary>The row axis (Carbon's <c>axes.left</c>, a labels axis); its first category is the bottom row. <see cref="CdsChartAxis.Categories"/> fixes the order.</summary>
    public CdsChartAxis? LeftAxis
    {
        get => (CdsChartAxis?)GetValue(LeftAxisProperty);
        set => SetValue(LeftAxisProperty, value);
    }

    /// <summary>The color legend's title (Carbon's <c>heatmap.colorLegend.title</c>): name the measure and its unit. It sits left of the bar when it fits beside the row labels, otherwise above it.</summary>
    public string? ColorLegendTitle
    {
        get => (string?)GetValue(ColorLegendTitleProperty);
        set => SetValue(ColorLegendTitleProperty, value);
    }

    /// <summary>A gradient bar (default) or one block per color step (Carbon's <c>heatmap.colorLegend.type</c>). Hide the legend with <see cref="CdsChart.IsLegendEnabled"/> <c>false</c>.</summary>
    public CdsChartColorLegendType ColorLegendType
    {
        get => (CdsChartColorLegendType)GetValue(ColorLegendTypeProperty);
        set => SetValue(ColorLegendTypeProperty, value);
    }

    /// <summary>The low end of the color domain (Carbon's <c>heatmap.colorDomain.min</c>); <c>null</c> (default) is the data's nice extent from 0, or symmetric around 0 for mixed signs. Values outside take the end colors.</summary>
    public double? ColorDomainMinimum
    {
        get => (double?)GetValue(ColorDomainMinimumProperty);
        set => SetValue(ColorDomainMinimumProperty, value);
    }

    /// <summary>The high end of the color domain (Carbon's <c>heatmap.colorDomain.max</c>); give both ends for a fixed range such as a 50 to 100 score.</summary>
    public double? ColorDomainMaximum
    {
        get => (double?)GetValue(ColorDomainMaximumProperty);
        set => SetValue(ColorDomainMaximumProperty, value);
    }

    /// <summary>1px <c>background</c> dividers between cells (Carbon's <c>heatmap.divider.state</c>): automatic when cells are at least 16 x 16px.</summary>
    public CdsHeatmapDivider Divider
    {
        get => (CdsHeatmapDivider)GetValue(DividerProperty);
        set => SetValue(DividerProperty, value);
    }

    /// <summary>Your own color steps, lowest value first (Carbon's <c>color.gradient.colors</c>), instead of the Carbon palettes. Keep them perceptually ordered.</summary>
    public IList<Color>? GradientColors
    {
        get => (IList<Color>?)GetValue(GradientColorsProperty);
        set => SetValue(GradientColorsProperty, value);
    }

    private protected override string ChartKind => "Heat map";

    private protected override bool DefaultLegendEnabled => false;

    private protected override IReadOnlyList<string> LegendGroups => [];

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) => [];

    private protected override ChartSkeleton Skeleton => ChartSkeleton.Grid;

    internal static IReadOnlyList<string> Columns(IEnumerable<CdsChartPoint> points, CdsChartAxis? axis) =>
        axis?.Categories?.ToList() ?? points.Select(p => p.Key).Where(k => k is not null).Distinct().ToList()!;

    internal static IReadOnlyList<string> Rows(IEnumerable<CdsChartPoint> points, CdsChartAxis? axis) =>
        axis?.Categories?.ToList() ?? points.Select(p => p.Group).Where(k => k is not null).Distinct().ToList()!;

    Dictionary<AxisPosition, AxisSpec> Axes(ChartBuildContext ctx) => new()
    {
        [AxisPosition.Bottom] = LabelsSpec(AxisPosition.Bottom, BottomAxis, Columns(ctx.Points, BottomAxis)),
        [AxisPosition.Left] = LabelsSpec(AxisPosition.Left, LeftAxis, Rows(ctx.Points, LeftAxis)),
    };

    static AxisSpec LabelsSpec(AxisPosition pos, CdsChartAxis? axis, IReadOnlyList<string> categories) => new()
    {
        Position = pos,
        ScaleType = CdsChartScaleType.Labels,
        IncludeZero = false,
        Categories = categories,
        Title = axis?.Title,
        IsVisible = axis?.IsVisible ?? true,
        TickFormatter = axis?.TickFormatter,
        TickRotation = axis?.TickRotation ?? CdsChartTickRotation.Auto,
        Truncation = axis?.Truncation ?? CdsChartTruncation.End,
        TruncationThreshold = axis?.TruncationThreshold ?? 16,
        TruncationLength = axis?.TruncationLength ?? 14,
    };

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var legendOn = IsLegendEnabled ?? true;
        var title = ColorLegendTitle;
        var axes = Axes(ctx);
        var cs0 = new CartesianLayout(ctx, axes, new CartesianOptions());
        var legend = ChartColorLegend.Measure(ctx.Width, title, cs0.Plot.Left, heatmap: true);
        var gh = legendOn ? Math.Max(1, ctx.Height - legend.Height - ChartColorLegend.Spacer) : ctx.Height;
        var cs = legendOn ? new CartesianLayout(ChartOutline.WithHeight(ctx, gh), axes, new CartesianOptions()) : cs0;
        var p = cs.Plot;
        var columns = axes[AxisPosition.Bottom].Categories!;
        var rows = axes[AxisPosition.Left].Categories!;
        var nx = Math.Max(1, columns.Count);
        var ny = Math.Max(1, rows.Count);
        var bw = p.Width / nx;
        var bh = p.Height / ny;

        // Cells fill the plot exactly (unrounded bands), so the ticks move to the unrounded band centers too.
        cs.Ticks[AxisPosition.Bottom] = cs.Ticks[AxisPosition.Bottom].Select((t, i) => t with { Pixel = p.Left + bw * (i + 0.5) }).ToList();
        cs.Ticks[AxisPosition.Left] = cs.Ticks[AxisPosition.Left].Select((t, i) => t with { Pixel = p.Bottom - bh * (i + 0.5) }).ToList();

        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = p, HitMode = ChartHitMode.Contains };
        scene.Shapes.Add(new ChartRect { Rect = p, Fill = ChartPaint.OfToken(CdsChartToken.GridBg) });

        var scale = ChartColorScale.Create(ctx.Points.Where(d => d.Value.HasValue).Select(d => d.Value!.Value), ColorDomainMinimum, ColorDomainMaximum, ColorPairingOption, GradientColors);
        var byCell = new Dictionary<(string, string), CdsChartPoint>();
        foreach (var d in ctx.Points)
            if (d.Key is not null && d.Group is not null) byCell[(d.Key, d.Group)] = d;
        var divider = Divider == CdsHeatmapDivider.On || (Divider == CdsHeatmapDivider.Auto && bw >= 16 && bh >= 16);
        var colIndex = columns.Select((c, i) => (c, i)).GroupBy(x => x.c).ToDictionary(g => g.Key, g => g.First().i);
        var rowIndex = rows.Select((r, i) => (r, i)).GroupBy(x => x.r).ToDictionary(g => g.Key, g => g.First().i);

        var nulls = new List<IReadOnlyList<PointF>>();
        var highlights = new List<(RectF Rect, int Mark)>();
        var id = 0;
        var cells = 0;
        RectF CellRect(int col, int row) => new(p.Left + bw * col, p.Bottom - bh * (row + 1), bw, bh);
        // Reading order: the top row first, left to right.
        for (var row = rows.Count - 1; row >= 0; row--)
        {
            for (var col = 0; col < columns.Count; col++)
            {
                var rect = CellRect(col, row);
                if (!byCell.TryGetValue((columns[col], rows[row]), out var d) || d.Value is not { } v || double.IsNaN(v))
                {
                    nulls.Add(ChartHatch.Rect(rect));
                    continue;
                }
                var fill = scale.Paint(v);
                var mark = id++;
                cells++;
                scene.Shapes.Add(new ChartRect
                {
                    Rect = rect,
                    Fill = fill,
                    Stroke = divider ? ChartPaint.OfKey(CdsColorKeys.Background) : ChartPaint.None,
                    StrokeWidth = 1,
                    Mark = mark,
                    Hover = ChartHoverStyle.None,
                });
                highlights.Add((rect, mark));
                scene.Marks.Add(new ChartMark
                {
                    Id = mark,
                    Bounds = rect,
                    Rows =
                    [
                        new ChartTooltipRow(cs.DomainLabel, columns[col]),
                        new ChartTooltipRow(cs.RangeLabel(null), rows[row]),
                        new ChartTooltipRow(ctx.TotalLabel, ctx.Value(v), fill),
                    ],
                    Description = $"{columns[col]}, {rows[row]}, {ctx.Value(v)}",
                });
            }
        }
        if (nulls.Count > 0)
            scene.Shapes.Add(new ChartPath { Path = ChartHatch.Lines(nulls, 3, 0.25f), Stroke = ChartPaint.OfKey(CdsColorKeys.BorderStrong01), StrokeWidth = 0.5f, Hover = ChartHoverStyle.None });

        // The frame (axis lines, labels and titles) of the plot above the legend.
        var frame = new ChartScene { Width = ctx.Width, Height = gh, Plot = p };
        CartesianFrame.Axes(frame, cs);
        scene.Shapes.AddRange(frame.Shapes);
        scene.Texts.AddRange(frame.Texts);

        foreach (var (rect, mark) in highlights) ChartOutline.Add(scene, rect, divider ? 2 : 1, mark);

        // Axis labels: hovering one outlines its row or column and gives its minimum, maximum and average.
        id = AxisMarks(scene, ctx, cs, AxisPosition.Bottom, columns, byCell, rows, id, (i) => new RectF(p.Left + bw * i, p.Top, bw, p.Height));
        AxisMarks(scene, ctx, cs, AxisPosition.Left, rows, byCell, columns, id, (i) => new RectF(p.Left, p.Bottom - bh * (i + 1), p.Width, bh));

        if (legendOn)
        {
            ChartColorLegend.Draw(scene, ctx, scale, legend, ColorLegendType, title, heatmap: true, gh + ChartColorLegend.Spacer);
        }
        scene.Summary = (cells == 1 ? "1 cell" : $"{cells} cells") + (nulls.Count > 0 ? $", {nulls.Count} without data" : "")
            + (legendOn ? ", " + ChartColorLegend.Describe(ctx, scale, title) : "");
        return scene;
    }

    static int AxisMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs, AxisPosition pos, IReadOnlyList<string> labels,
        Dictionary<(string, string), CdsChartPoint> byCell, IReadOnlyList<string> others, int id, Func<int, RectF> band)
    {
        var spec = cs.Axes[pos];
        if (!spec.IsVisible) return id;
        var ticks = cs.Ticks[pos];
        var p = cs.Plot;
        var rotated = cs.Rotated.TryGetValue(pos, out var r) && r;
        var isDomain = pos == AxisPosition.Bottom;
        var order = isDomain ? Enumerable.Range(0, labels.Count) : Enumerable.Range(0, labels.Count).Reverse();
        foreach (var i in order)
        {
            if (i >= ticks.Count) continue;
            var t = ticks[i];
            var w = CdsChartTextMetrics.Width(t.Label, ChartFont.Tick);
            var at = (float)t.Pixel;
            RectF bounds;
            PointF[]? polygon = null;
            if (!isDomain) bounds = new RectF(p.Left - 9 - w - 4, at - 8, w + 8, 16);
            else if (!rotated) bounds = new RectF(at - w / 2 - 4, p.Bottom + 7, w + 8, 16);
            else
            {
                var anchor = new PointF(at, p.Bottom + 9);
                var dir = new PointF(0.7071f, -0.7071f);
                var nrm = new PointF(0.7071f, 0.7071f);
                var start = new PointF(anchor.X - dir.X * (w + 4), anchor.Y - dir.Y * (w + 4));
                polygon =
                [
                    new(start.X - nrm.X * 8, start.Y - nrm.Y * 8), new(anchor.X - nrm.X * 8, anchor.Y - nrm.Y * 8),
                    new(anchor.X + nrm.X * 8, anchor.Y + nrm.Y * 8), new(start.X + nrm.X * 8, start.Y + nrm.Y * 8),
                ];
                var xs = polygon.Select(q => q.X).ToArray();
                var ys = polygon.Select(q => q.Y).ToArray();
                bounds = new RectF(xs.Min(), ys.Min(), xs.Max() - xs.Min(), ys.Max() - ys.Min());
            }
            var values = others.Select(o => byCell.TryGetValue(isDomain ? (labels[i], o) : (o, labels[i]), out var d) ? d.Value : null)
                .Where(v => v is { } x && !double.IsNaN(x)).Select(v => v!.Value).ToList();
            var mark = id++;
            ChartOutline.Add(scene, band(i), 2, mark);
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = bounds,
                Polygon = polygon,
                Anchor = new PointF(bounds.Center.X, bounds.Center.Y),
                Rows =
                [
                    new ChartTooltipRow(isDomain ? cs.DomainLabel : cs.RangeLabel(null), labels[i], Bold: true),
                    new ChartTooltipRow("Min", values.Count > 0 ? ctx.Value(values.Min()) : "-"),
                    new ChartTooltipRow("Max", values.Count > 0 ? ctx.Value(values.Max()) : "-"),
                    new ChartTooltipRow("Average", values.Count > 0 ? ctx.Value(values.Average()) : "-"),
                ],
            });
        }
        return id;
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        const string dash = "–";
        return new ChartTable(
            [BottomAxis?.Title ?? "Key", LeftAxis?.Title ?? "Group", "Value"],
            ctx.Points.Select(d => new[] { d.Key ?? dash, d.Group ?? dash, d.Value is { } v && !double.IsNaN(v) ? ctx.Value(v) : dash }).ToList());
    }

    static BindableProperty AxisProperty(string name) => BindableProperty.Create(
        name, typeof(CdsChartAxis), typeof(CdsHeatmapChart), null,
        propertyChanged: static (b, o, n) => ((CdsHeatmapChart)b).OnAxisChanged((CdsChartAxis?)o, (CdsChartAxis?)n));

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
        foreach (var axis in new[] { BottomAxis, LeftAxis })
            if (axis is not null) SetInheritedBindingContext(axis, BindingContext);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsHeatmapChart)b).RequestRebuild();
}
