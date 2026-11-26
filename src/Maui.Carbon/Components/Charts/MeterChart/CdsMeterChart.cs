namespace Maui.Carbon;

/// <summary>
/// A thin bar showing one percentage with a title, status and peak, or a proportional bar showing parts of a total with a
/// breakdown and legend (Carbon's meter chart). Single meter: one <see cref="CdsChartPoint"/> whose
/// <see cref="CdsChartPoint.Group"/> is the title above the bar and whose <see cref="CdsChartPoint.Value"/> is a percentage
/// (0-100). Proportional meter (<see cref="IsProportional"/>): one point per part, filling <see cref="Total"/>. Use it for
/// bounded values (memory used, quota); values without a meaningful maximum belong in a bar chart, steps in a progress
/// indicator. The chart is only as tall as it needs unless you set <c>HeightRequest</c>; the toolbar is off by default.
/// </summary>
public class CdsMeterChart : CdsChart
{
    /// <summary>Bindable property for <see cref="IsProportional"/>.</summary>
    public static readonly BindableProperty IsProportionalProperty = BindableProperty.Create(
        nameof(IsProportional), typeof(bool), typeof(CdsMeterChart), false, propertyChanged: static (b, _, _) => ((CdsMeterChart)b).OnProportionalChanged());

    /// <summary>Bindable property for <see cref="Total"/>.</summary>
    public static readonly BindableProperty TotalProperty = BindableProperty.Create(
        nameof(Total), typeof(double?), typeof(CdsMeterChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Unit"/>.</summary>
    public static readonly BindableProperty UnitProperty = BindableProperty.Create(
        nameof(Unit), typeof(string), typeof(CdsMeterChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="TotalFormatter"/>.</summary>
    public static readonly BindableProperty TotalFormatterProperty = BindableProperty.Create(
        nameof(TotalFormatter), typeof(Func<double, string>), typeof(CdsMeterChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="BreakdownFormatter"/>.</summary>
    public static readonly BindableProperty BreakdownFormatterProperty = BindableProperty.Create(
        nameof(BreakdownFormatter), typeof(Func<double, double, string>), typeof(CdsMeterChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="StatusRanges"/>.</summary>
    public static readonly BindableProperty StatusRangesProperty = BindableProperty.Create(
        nameof(StatusRanges), typeof(IList<CdsMeterStatusRange>), typeof(CdsMeterChart), null, propertyChanged: Changed,
        defaultValueCreator: static b =>
        {
            // The chart's own list: it lives as long as the chart, so the chart can watch it.
            var list = new CdsItemCollection<CdsMeterStatusRange>();
            list.CollectionChanged += (_, _) => ((CdsMeterChart)b).RequestRebuild();
            return list;
        });

    /// <summary>Bindable property for <see cref="Peak"/>.</summary>
    public static readonly BindableProperty PeakProperty = BindableProperty.Create(
        nameof(Peak), typeof(double?), typeof(CdsMeterChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="BarHeight"/>.</summary>
    public static readonly BindableProperty BarHeightProperty = BindableProperty.Create(
        nameof(BarHeight), typeof(double?), typeof(CdsMeterChart), null, propertyChanged: FrameChanged);

    /// <summary>Bindable property for <see cref="ShowLabels"/>.</summary>
    public static readonly BindableProperty ShowLabelsProperty = BindableProperty.Create(
        nameof(ShowLabels), typeof(bool), typeof(CdsMeterChart), true, propertyChanged: FrameChanged);

    /// <summary>Bindable property for <see cref="ShowPercentage"/>.</summary>
    public static readonly BindableProperty ShowPercentageProperty = BindableProperty.Create(
        nameof(ShowPercentage), typeof(bool), typeof(CdsMeterChart), true, propertyChanged: Changed);

    /// <summary>Creates a meter chart (Carbon's defaults: no toolbar, no legend unless proportional, legend items not clickable).</summary>
    public CdsMeterChart()
    {
        IsLegendClickable = false;
    }

    /// <summary>
    /// Whether the meter shows parts of a total (Carbon's <c>meter.proportional</c>): a 16px bar with one segment per point in
    /// pairing colors, a breakdown ("2,742 MB used (1,258 MB available)") and the total above it, and the legend on.
    /// Default <c>false</c>: a single percentage.
    /// </summary>
    public bool IsProportional
    {
        get => (bool)GetValue(IsProportionalProperty);
        set => SetValue(IsProportionalProperty, value);
    }

    /// <summary>The capacity a proportional meter fills (Carbon's <c>meter.proportional.total</c>); <c>null</c> (default) is the sum of the values.</summary>
    public double? Total
    {
        get => (double?)GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    /// <summary>The unit of a proportional meter's values, used in the breakdown and total text (Carbon's <c>meter.proportional.unit</c>), like "MB".</summary>
    public string? Unit
    {
        get => (string?)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    /// <summary>Formats a proportional meter's total text (Carbon's <c>totalFormatter</c>); default "4,000 MB total".</summary>
    public Func<double, string>? TotalFormatter
    {
        get => (Func<double, string>?)GetValue(TotalFormatterProperty);
        set => SetValue(TotalFormatterProperty, value);
    }

    /// <summary>
    /// Formats a proportional meter's breakdown (Carbon's <c>breakdownFormatter</c>) from the sum of the values and the
    /// total; default "2,742 MB used (1,258 MB available)". Shown only when <see cref="Total"/> is set and differs from the sum.
    /// </summary>
    public Func<double, double, string>? BreakdownFormatter
    {
        get => (Func<double, double, string>?)GetValue(BreakdownFormatterProperty);
        set => SetValue(BreakdownFormatterProperty, value);
    }

    /// <summary>
    /// Status ranges (Carbon's <c>meter.status.ranges</c>): the first range holding the value (a proportional meter's sum)
    /// colors a single meter's bar (<c>support-success</c>, <c>support-warning</c> outlined in <c>alert-stroke</c>,
    /// <c>support-error</c>) and shows a 16px status icon at the right. Use status colors only for status, always with the
    /// icon and the value text. Add ranges in XAML or replace the list (the chart watches only its own list).
    /// </summary>
    public IList<CdsMeterStatusRange> StatusRanges
    {
        get => (IList<CdsMeterStatusRange>)GetValue(StatusRangesProperty);
        set => SetValue(StatusRangesProperty, value);
    }

    /// <summary>A peak marker (Carbon's <c>meter.peak</c>): a 2px <c>border-inverse</c> line at this value, clamped to the scale. Default none.</summary>
    public double? Peak
    {
        get => (double?)GetValue(PeakProperty);
        set => SetValue(PeakProperty, value);
    }

    /// <summary>The bar's height (Carbon's <c>meter.height</c>); <c>null</c> (default) is 8px, 16px for a proportional meter.</summary>
    public double? BarHeight
    {
        get => (double?)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    /// <summary>Whether the title row (title and percentage, or breakdown and total) shows above the bar (Carbon's <c>meter.showLabels</c>). Default <c>true</c>.</summary>
    public bool ShowLabels
    {
        get => (bool)GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    /// <summary>Whether a single meter shows its percentage after the title (Carbon's <c>meter.statusBar.percentageIndicator.enabled</c>). Default <c>true</c>.</summary>
    public bool ShowPercentage
    {
        get => (bool)GetValue(ShowPercentageProperty);
        set => SetValue(ShowPercentageProperty, value);
    }

    private protected override string ChartKind => "Meter chart";

    private protected override bool DefaultLegendEnabled => IsProportional;

    private protected override bool DefaultToolbarEnabled => false;

    private protected override double DefaultHeight => MeterLayout.GraphHeight(IsProportional, ShowLabels, BarHeight);

    /// <summary>Carbon has no meter skeleton: loading shimmers the meter's own empty bar.</summary>
    private protected override ChartScene BuildSkeleton(float width, float height, bool shimmer)
    {
        var scene = new ChartScene { Width = width, Height = height, Plot = new RectF(0, 0, width, height), Shimmer = shimmer, Summary = shimmer ? "Loading" : "No data" };
        AddEmptyBar(scene, width);
        return scene;
    }

    /// <summary>Carbon's empty meter: the bar in the skeleton color, below the title row's place.</summary>
    void AddEmptyBar(ChartScene scene, float width)
    {
        var bh = MeterLayout.BarHeight(IsProportional, BarHeight);
        var y = ShowLabels ? (IsProportional ? MeterLayout.TitleRowProportional : MeterLayout.TitleRowSingle) + MeterLayout.TitleSpacer : 0;
        scene.Shapes.Add(new ChartRect { Rect = new RectF(0, y, width, bh), Fill = ChartPaint.OfKey(CdsColorKeys.LayerAccent01), Shimmers = true });
    }

    private protected override bool IsEmpty(IReadOnlyList<object> items) => false;

    /// <summary>Measures to the content (Carbon's <c>height: auto</c>): header, title row, bar and legend, unless a height is requested.</summary>
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        if (HeightRequest >= 0 || IsFullscreen) return base.MeasureOverride(widthConstraint, heightConstraint);
        return base.MeasureOverride(widthConstraint, Math.Min(heightConstraint, ContentHeight(widthConstraint)));
    }

    /// <summary>The height the meter needs at <paramref name="width"/>.</summary>
    internal double ContentHeight(double width)
    {
        double height = MeterLayout.GraphHeight(IsProportional, ShowLabels, BarHeight);
        var toolbar = (IsToolbarEnabled ?? false) && ToolbarControls != CdsChartToolbarControls.None;
        var title = !string.IsNullOrEmpty(Title);
        if (toolbar || title) height += Math.Max(title ? 22 : 0, toolbar ? 32 : 0) + (toolbar ? CdsSpacing.Spacing05 : CdsSpacing.Spacing06);
        var groups = Data?.OfType<CdsChartPoint>().Select(p => p.Group ?? "").Distinct().ToList() ?? [];
        if ((IsLegendEnabled ?? IsProportional) && groups.Count > 0)
        {
            const double row = 24;
            if (LegendPosition is CdsChartLegendPosition.Left or CdsChartLegendPosition.Right)
                return Math.Max(height, groups.Count * row);
            var rows = 1;
            double x = 0;
            var available = double.IsInfinity(width) || width <= 0 ? double.MaxValue : width;
            foreach (var g in groups)
            {
                var w = 13 + 4 + CdsChartTextMetrics.Width(g, ChartFont.Tick) + 12;
                if (x > 0 && x + w > available)
                {
                    rows++;
                    x = 0;
                }
                x += w;
            }
            height += CdsSpacing.Spacing06 + rows * row;
        }
        return height;
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var width = ctx.Width;
        var scene = new ChartScene { Width = width, Height = ctx.Height, Plot = new RectF(0, 0, width, ctx.Height) };
        var proportional = IsProportional;
        var all = ctx.ActivePoints;
        var rows = proportional ? all : all.Take(1).ToList();
        var sum = all.Sum(p => p.Value is { } v && !double.IsNaN(v) ? v : 0);
        var bh = MeterLayout.BarHeight(proportional, BarHeight);
        var y = ShowLabels ? (proportional ? MeterLayout.TitleRowProportional : MeterLayout.TitleRowSingle) + MeterLayout.TitleSpacer : 0;

        if (rows.Count == 0)
        {
            AddEmptyBar(scene, width);
            scene.Summary = "No data";
            return scene;
        }

        var status = MeterLayout.Status(sum, proportional, StatusRanges);
        if (ShowLabels) AddTitle(scene, ctx, width, status, sum);

        var max = MeterLayout.Max(proportional, Total, sum);
        float Scale(double v) => max != 0 ? (float)(v / max * width) : 0;
        scene.Shapes.Add(new ChartRect { Rect = new RectF(0, y, width, bh), Fill = ChartPaint.OfKey(CdsColorKeys.Layer01) });
        scene.Shapes.Add(new ChartLine
        {
            From = new PointF(width - 0.5f, y),
            To = new PointF(width - 0.5f, y + bh),
            Stroke = ChartPaint.OfToken(CdsChartToken.MeterRangeIndicator),
        });

        var statusColored = status is not null && ColorScale is not { Count: > 0 } && !proportional;
        var values = rows.Select(p => p.Value is { } v && !double.IsNaN(v) ? v : 0).ToList();
        var bounds = MeterLayout.Stacked(values, max, width);
        for (var i = 0; i < rows.Count; i++)
        {
            var p = rows[i];
            var g = p.Group ?? "";
            var w = values[i] > max ? Scale(max) : Math.Max(bounds[i].Width, 2);
            var rect = new RectF(bounds[i].X, y, w, bh);
            var shape = new ChartRect { Rect = rect, Group = proportional ? g : null, Mark = i };
            if (statusColored)
            {
                shape.Fill = ChartPaint.OfKey(ChartStatusColors.Key(status!.Value));
                shape.Hover = ChartHoverStyle.None;
                if (status == CdsChartStatus.Warning)
                {
                    shape.Stroke = ChartPaint.OfToken(CdsChartToken.AlertStroke);
                    shape.Rect = new RectF(rect.X + 0.5f, rect.Y + 0.5f, Math.Max(0, rect.Width - 1), Math.Max(0, rect.Height - 1));
                }
            }
            else
            {
                shape.Fill = ChartPaint.OfSeries(g);
                // Only proportional segments react to hover in Carbon.
                shape.Hover = proportional ? ChartHoverStyle.Darken : ChartHoverStyle.None;
            }
            scene.Shapes.Add(shape);
            var valueText = proportional ? ctx.Value(p.Value) + (string.IsNullOrEmpty(Unit) ? "" : " " + Unit) : ctx.Number(p.Value ?? 0) + "%";
            var description = proportional
                ? $"{g}, {valueText}, {ctx.Number(Math.Round(values[i] / max * 100, 2))}% of total"
                : $"{g}, {valueText}" + (status is { } s ? $", {ChartStatusColors.Name(s)}" : "");
            scene.Marks.Add(new ChartMark
            {
                Id = i,
                Bounds = new RectF(rect.X, y - 4, Math.Max(rect.Width, 4), bh + 8),
                Group = proportional ? g : null,
                Rows = [new ChartTooltipRow(CdsChartTruncate.Apply(g), proportional ? ctx.Value(p.Value) : valueText)],
                Description = description,
            });
        }

        if (Peak is { } peakValue && !double.IsNaN(peakValue))
        {
            var peak = Math.Clamp(peakValue, 0, Math.Max(0, max));
            var x = Scale(peak);
            var id = rows.Count;
            scene.Shapes.Add(new ChartLine
            {
                From = new PointF(x, y),
                To = new PointF(x, y + bh),
                Stroke = ChartPaint.OfKey(CdsColorKeys.BorderInverse),
                StrokeWidth = 2,
                Mark = id,
                Hover = ChartHoverStyle.None,
            });
            var peakText = ctx.Number(peak) + (proportional ? (string.IsNullOrEmpty(Unit) ? "" : " " + Unit) : "%");
            scene.Marks.Add(new ChartMark
            {
                Id = id,
                Bounds = new RectF(x - 4, y - 4, 8, bh + 8),
                Rows = [new ChartTooltipRow("Peak", peakText)],
                Description = "Peak, " + peakText,
            });
        }

        scene.Summary = proportional
            ? (rows.Count == 1 ? "1 part" : $"{rows.Count} parts") + $", {ctx.Number(sum)} of {ctx.Number(max)}" + (string.IsNullOrEmpty(Unit) ? "" : " " + Unit)
            : ctx.Number(values[0]) + "%" + (status is { } st ? ", " + ChartStatusColors.Name(st) : "");
        return scene;
    }

    void AddTitle(ChartScene scene, ChartBuildContext ctx, float width, CdsChartStatus? status, double sum)
    {
        var primary = ChartPaint.OfKey(CdsColorKeys.TextPrimary);
        var statusW = status is null ? 0 : MeterLayout.StatusIndicatorSize + MeterLayout.StatusPaddingLeft;
        if (IsProportional)
        {
            var small = ChartFont.Tick;
            var unit = Unit ?? "";
            var total = Total;
            var totalValue = total ?? sum;
            var totalText = TotalFormatter is { } tf ? tf(totalValue) ?? "" : $"{ctx.Number(totalValue)} {unit} total".Replace("  ", " ");
            var totalX = status is null ? width : width - MeterLayout.TotalPaddingRight;
            scene.Texts.Add(RoundChartText.At(totalText, totalX, 12, small, ChartTextAnchor.End, color: primary));
            if (total is { } t && sum != t)
            {
                var breakdown = BreakdownFormatter is { } bf
                    ? bf(sum, t) ?? ""
                    : $"{ctx.Number(sum)} {unit} used ({ctx.Number(t - sum)} {unit} available)".Replace("  ", " ");
                var maxW = width - CdsChartTextMetrics.Width(totalText, small) - MeterLayout.TotalPaddingLeft - (status is null ? 0 : MeterLayout.TotalPaddingRight);
                scene.Texts.Add(RoundChartText.At(RoundChartText.Fit(breakdown, maxW, small), 0, 12, small, color: primary));
            }
        }
        else
        {
            var d = ctx.ActivePoints[0];
            var titleFont = new ChartFont(ChartFontFamily.SansSemiBold, 16);
            var pctFont = new ChartFont(ChartFontFamily.Sans, 16);
            var pct = ctx.Number(d.Value ?? 0) + "%";
            var pctW = ShowPercentage ? CdsChartTextMetrics.Width(pct, pctFont) : 0;
            var shown = RoundChartText.Fit(d.Group ?? "", width - pctW - MeterLayout.PercentPaddingLeft - statusW, titleFont);
            scene.Texts.Add(RoundChartText.At(shown, 0, 16, titleFont, color: primary));
            if (ShowPercentage)
                scene.Texts.Add(RoundChartText.At(pct, CdsChartTextMetrics.Width(shown, titleFont) + MeterLayout.PercentPaddingLeft, 16, pctFont, color: primary));
        }
        if (status is { } s)
        {
            var size = MeterLayout.StatusIndicatorSize;
            scene.Shapes.Add(new ChartCircle { Center = new PointF(width - size / 2, size / 2), Radius = size / 2, Fill = ChartPaint.OfKey(ChartStatusColors.Key(s)) });
            // The glyph is layer-01-absolute, except on warning yellow, where Carbon draws it black in every theme.
            var glyph = s == CdsChartStatus.Warning
                ? ChartPaint.OfColor(ChartColorMath.Darken(ctx.Colors.Palette.Key(CdsColorKeys.SupportWarning), 1))
                : ChartPaint.OfToken(CdsChartToken.Layer01Absolute);
            scene.Shapes.Add(new ChartPath { Path = MeterLayout.StatusGlyph(s, new PointF(width - size, 0)), Fill = glyph });
        }
    }

    /// <summary>
    /// Carbon's meter table: a single meter lists its group, value and (with a matching range) status; a proportional one
    /// every part with its percentage of the total.
    /// </summary>
    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var all = ctx.ActivePoints;
        var sum = all.Sum(p => p.Value is { } v && !double.IsNaN(v) ? v : 0);
        string N(double? v) => v is null ? "–" : ctx.Value(v);
        if (!IsProportional)
        {
            var status = MeterLayout.Status(sum, false, StatusRanges);
            var headers = new List<string> { ctx.GroupLabel, "Value" };
            if (status is not null) headers.Add("Status");
            var rows = all.Take(1).Select(p =>
            {
                var r = new List<string> { p.Group ?? "", N(p.Value) };
                if (status is { } s) r.Add(ChartStatusColors.Name(s));
                return r.ToArray();
            }).ToList();
            return new ChartTable(headers, rows);
        }
        var max = MeterLayout.Max(true, Total, sum);
        return new ChartTable([ctx.GroupLabel, "Value", "Percentage of total"],
            all.Select(p => new[] { p.Group ?? "", N(p.Value), ctx.Number(max != 0 ? Math.Round((p.Value ?? 0) / max * 100, 2) : 0) + " %" }).ToList());
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsMeterChart)b).RequestRebuild();

    void OnProportionalChanged()
    {
        // The legend's default follows IsProportional, but the base lays out the legend only when a legend property
        // changes: flip IsLegendEnabled and restore it so the frame is rebuilt.
        var wasSet = IsSet(IsLegendEnabledProperty);
        var value = IsLegendEnabled;
        IsLegendEnabled = !(value ?? !DefaultLegendEnabled);
        if (wasSet) IsLegendEnabled = value;
        else ClearValue(IsLegendEnabledProperty);
        InvalidateMeasure();
        RequestRebuild();
    }

    static void FrameChanged(BindableObject b, object? o, object? n)
    {
        var c = (CdsMeterChart)b;
        c.InvalidateMeasure();
        c.RequestRebuild();
    }
}
