namespace Maui.Carbon;

/// <summary>
/// One percentage on a semicircular or circular arc, with a large value and an optional change (delta) with an arrow
/// (Carbon's gauge chart). Use it for a single KPI that is naturally a percentage of a whole (utilization, attainment)
/// shown large on a dashboard; in lists or tight spaces use a meter. Set <see cref="Value"/> (clamped to 0-100 on the arc)
/// and optionally <see cref="Delta"/>; these properties replace Carbon's <c>value</c> and <c>delta</c> data rows (a
/// <see cref="CdsChartPoint"/> with group "value" or "delta" in <see cref="CdsChart.Data"/> is still read when the property
/// is not set). Put the unit and what the percentage measures in the title. The legend is off by default.
/// </summary>
public class CdsGaugeChart : CdsChart
{
    const string ValueGroup = "value";
    const string DeltaGroup = "delta";
    const float NumberSpacing = 10;

    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double?), typeof(CdsGaugeChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Delta"/>.</summary>
    public static readonly BindableProperty DeltaProperty = BindableProperty.Create(
        nameof(Delta), typeof(double?), typeof(CdsGaugeChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsGaugeChartType), typeof(CdsGaugeChart), CdsGaugeChartType.Semi,
        propertyChanged: static (b, _, _) =>
        {
            var c = (CdsGaugeChart)b;
            c.InvalidateMeasure();
            c.RequestRebuild();
        });

    /// <summary>Bindable property for <see cref="Status"/>.</summary>
    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status), typeof(CdsChartStatus?), typeof(CdsGaugeChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Alignment"/>.</summary>
    public static readonly BindableProperty AlignmentProperty = BindableProperty.Create(
        nameof(Alignment), typeof(CdsChartAlignment), typeof(CdsGaugeChart), CdsChartAlignment.Left, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ArcWidth"/>.</summary>
    public static readonly BindableProperty ArcWidthProperty = BindableProperty.Create(
        nameof(ArcWidth), typeof(double), typeof(CdsGaugeChart), 16d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ShowPercentageSymbol"/>.</summary>
    public static readonly BindableProperty ShowPercentageSymbolProperty = BindableProperty.Create(
        nameof(ShowPercentageSymbol), typeof(bool), typeof(CdsGaugeChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="NumberFormatter"/>.</summary>
    public static readonly BindableProperty NumberFormatterProperty = BindableProperty.Create(
        nameof(NumberFormatter), typeof(Func<double, string>), typeof(CdsGaugeChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="IsDeltaArrowEnabled"/>.</summary>
    public static readonly BindableProperty IsDeltaArrowEnabledProperty = BindableProperty.Create(
        nameof(IsDeltaArrowEnabled), typeof(bool), typeof(CdsGaugeChart), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="DeltaArrowDirection"/>.</summary>
    public static readonly BindableProperty DeltaArrowDirectionProperty = BindableProperty.Create(
        nameof(DeltaArrowDirection), typeof(CdsGaugeDeltaArrowDirection), typeof(CdsGaugeChart), CdsGaugeDeltaArrowDirection.Auto, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="DeltaArrowSize"/>.</summary>
    public static readonly BindableProperty DeltaArrowSizeProperty = BindableProperty.Create(
        nameof(DeltaArrowSize), typeof(double?), typeof(CdsGaugeChart), null, propertyChanged: Changed);

    /// <summary>
    /// The percentage shown (Carbon's <c>value</c> row): the arc fills to it, clamped to 0-100; the number shows two decimals
    /// only when it has a fraction. <c>null</c> (default) reads a "value" point from <see cref="CdsChart.Data"/>, and shows
    /// the empty skeleton when there is none.
    /// </summary>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// The change since the last period (Carbon's <c>delta</c> row), shown under the value with an arrow; <c>null</c> or 0
    /// shows none. Its sign repeats the arrow's direction, so the change never depends on color.
    /// </summary>
    public double? Delta
    {
        get => (double?)GetValue(DeltaProperty);
        set => SetValue(DeltaProperty, value);
    }

    /// <summary>A half circle (default, 250px tall by default) or a full ring (300px) (Carbon's <c>gauge.type</c>).</summary>
    public CdsGaugeChartType Type
    {
        get => (CdsGaugeChartType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>
    /// Colors the delta arrow (Carbon's <c>gauge.status</c>): <c>support-error</c>, <c>support-warning</c> or
    /// <c>support-success</c>; <c>null</c> (default) leaves it <c>text-secondary</c>. Use status colors only for status.
    /// </summary>
    public CdsChartStatus? Status
    {
        get => (CdsChartStatus?)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Where the gauge sits across the graph (Carbon's <c>gauge.alignment</c>). Default <see cref="CdsChartAlignment.Left"/>.</summary>
    public CdsChartAlignment Alignment
    {
        get => (CdsChartAlignment)GetValue(AlignmentProperty);
        set => SetValue(AlignmentProperty, value);
    }

    /// <summary>The thickness of the arc (Carbon's <c>gauge.arcWidth</c>). Default 16.</summary>
    public double ArcWidth
    {
        get => (double)GetValue(ArcWidthProperty);
        set => SetValue(ArcWidthProperty, value);
    }

    /// <summary>Whether the value and delta end with a "%" (Carbon's <c>gauge.showPercentageSymbol</c>). Default <c>true</c>.</summary>
    public bool ShowPercentageSymbol
    {
        get => (bool)GetValue(ShowPercentageSymbolProperty);
        set => SetValue(ShowPercentageSymbolProperty, value);
    }

    /// <summary>
    /// Formats the value and the delta (Carbon's <c>gauge.numberFormatter</c>); receives the number rounded to two decimals.
    /// Default: locale formatted.
    /// </summary>
    public Func<double, string>? NumberFormatter
    {
        get => (Func<double, string>?)GetValue(NumberFormatterProperty);
        set => SetValue(NumberFormatterProperty, value);
    }

    /// <summary>Whether the delta shows its arrow (Carbon's <c>gauge.deltaArrow.enabled</c>). Default <c>true</c>.</summary>
    public bool IsDeltaArrowEnabled
    {
        get => (bool)GetValue(IsDeltaArrowEnabledProperty);
        set => SetValue(IsDeltaArrowEnabledProperty, value);
    }

    /// <summary>Which way the delta arrow points (Carbon's <c>gauge.deltaArrow.direction</c>); default follows the delta's sign.</summary>
    public CdsGaugeDeltaArrowDirection DeltaArrowDirection
    {
        get => (CdsGaugeDeltaArrowDirection)GetValue(DeltaArrowDirectionProperty);
        set => SetValue(DeltaArrowDirectionProperty, value);
    }

    /// <summary>The delta arrow's size (Carbon's <c>gauge.deltaArrow.size</c>); <c>null</c> (default) is the radius / 8.</summary>
    public double? DeltaArrowSize
    {
        get => (double?)GetValue(DeltaArrowSizeProperty);
        set => SetValue(DeltaArrowSizeProperty, value);
    }

    private protected override string ChartKind => "Gauge chart";

    private protected override ChartSkeleton Skeleton => ChartSkeleton.Donut;

    private protected override bool DefaultLegendEnabled => false;

    private protected override double DefaultHeight => Type == CdsGaugeChartType.Full ? 300 : 250;

    /// <summary>One series, "value" (its pairing color fills the arc; <c>ColorScale["value"]</c> overrides it).</summary>
    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) => [ValueGroup];

    private protected override bool IsEmpty(IReadOnlyList<object> items) => ValueOf(items) is null;

    double? ValueOf(IReadOnlyList<object> items) => Value ?? RowValue(items, ValueGroup);

    double? DeltaOf(IReadOnlyList<object> items) => Delta ?? RowValue(items, DeltaGroup);

    static double? RowValue(IReadOnlyList<object> items, string group) =>
        items.OfType<CdsChartPoint>().FirstOrDefault(p => p.Group == group)?.Value is { } v && !double.IsNaN(v) ? v : null;

    /// <summary>Carbon's gauge number: rounded to two decimals (none when whole), then the formatter or the locale.</summary>
    string Number(ChartBuildContext ctx, double value)
    {
        var n = GaugeGeometry.Round(value);
        return NumberFormatter is { } f ? f(n) ?? "" : ctx.Number(n);
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = new RectF(0, 0, ctx.Width, ctx.Height) };
        var value = ValueOf(ctx.Items) ?? 0;
        var delta = DeltaOf(ctx.Items);
        var full = Type == CdsGaugeChartType.Full;
        var g = GaugeGeometry.Layout(ctx.Width, ctx.Height, full, (float)ArcWidth, value, Alignment);
        var center = g.Center;

        // The rest of the arc (layer-01), then the value (the series color).
        scene.Shapes.Add(new ChartPath
        {
            Path = ChartGeometry.ArcPath(center, g.Inner, g.Radius, g.Current, g.End),
            Fill = ChartPaint.OfKey(CdsColorKeys.Layer01),
        });
        if (g.Current > g.Start)
            scene.Shapes.Add(new ChartPath
            {
                Path = ChartGeometry.ArcPath(center, g.Inner, g.Radius, g.Start, g.Current),
                Fill = ChartPaint.OfSeries(ValueGroup),
                Group = ValueGroup,
                Mark = 0,
                Hover = ChartHoverStyle.None,
            });

        // The numbers (drawValueNumber / drawDelta).
        var valueFs = g.Radius / 2.5f;
        var deltaFs = g.Radius / 8;
        var hasDelta = delta is { } dv && dv != 0;
        var numbersY = full && !hasDelta ? deltaFs : !full && hasDelta ? -(deltaFs + NumberSpacing) : 0;
        var valueText = Number(ctx, value);
        // Carbon sets the value in Plex Sans Light (gauge.scss: font-weight 300).
        var valueFont = new ChartFont(ChartFontFamily.SansLight, Math.Max(1, valueFs));
        var symbol = ShowPercentageSymbol ? "%" : "";
        var symbolFont = new ChartFont(ChartFontFamily.Condensed, Math.Max(1, valueFs / 2));
        var symbolW = CdsChartTextMetrics.Width(symbol, symbolFont);
        var symbolH = symbol.Length > 0 ? symbolFont.Size * 1.3f : 0;
        var valueW = CdsChartTextMetrics.Width(valueText, valueFont);
        var vx = center.X - symbolW / 2;
        var vy = center.Y + numbersY;
        scene.Texts.Add(RoundChartText.At(valueText, vx, vy, valueFont, ChartTextAnchor.Middle));
        if (symbol.Length > 0) scene.Texts.Add(RoundChartText.At(symbol, vx + valueW / 2, vy - symbolH / 2, symbolFont));

        string? deltaText = null;
        if (hasDelta)
        {
            var d = delta!.Value;
            deltaText = Number(ctx, d) + symbol;
            var deltaFont = new ChartFont(ChartFontFamily.Condensed, Math.Max(1, deltaFs));
            var deltaW = CdsChartTextMetrics.Width(deltaText, deltaFont);
            var dy = vy + deltaFs + NumberSpacing;
            scene.Texts.Add(RoundChartText.At(deltaText, center.X, dy, deltaFont, ChartTextAnchor.Middle));
            if (IsDeltaArrowEnabled)
            {
                var size = (float)(DeltaArrowSize ?? g.Radius / 8);
                var up = DeltaArrowDirection switch
                {
                    CdsGaugeDeltaArrowDirection.Up => true,
                    CdsGaugeDeltaArrowDirection.Down => false,
                    _ => d > 0,
                };
                var origin = new PointF(center.X - size - deltaW / 2, dy - size / 2 - deltaFs * 0.35f);
                scene.Shapes.Add(new ChartPath
                {
                    Path = GaugeGeometry.Arrow(origin, size, up),
                    Fill = ChartPaint.OfKey(Status is { } s ? ChartStatusColors.Key(s) : CdsColorKeys.TextSecondary),
                });
            }
        }

        var shown = valueText + symbol;
        var rows = new List<ChartTooltipRow> { new("Value", shown, ChartPaint.OfSeries(ValueGroup)) };
        if (deltaText is not null) rows.Add(new ChartTooltipRow("Delta", deltaText));
        scene.Marks.Add(new ChartMark
        {
            Id = 0,
            Bounds = new RectF(center.X - g.Radius, center.Y - g.Radius, g.Radius * 2, full ? g.Radius * 2 : g.Radius),
            Polygon = ChartGeometry.ArcPolygon(center, g.Inner, g.Radius, g.Start, g.End),
            Anchor = ChartGeometry.Polar(center, (g.Inner + g.Radius) / 2, (g.Start + g.Current) / 2),
            Group = ValueGroup,
            Rows = rows,
            Description = deltaText is null ? shown : $"{shown}, change {deltaText}" + (Status is { } st ? $", {ChartStatusColors.Name(st)}" : ""),
        });
        scene.Summary = deltaText is null ? shown : $"{shown}, change {deltaText}";
        return scene;
    }

    /// <summary>Carbon's gauge table: Group and Value, the value row then the delta row.</summary>
    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var rows = new List<string[]> { new[] { ValueGroup, ValueOf(ctx.Items) is { } v ? ctx.Value(v) : "–" } };
        if (DeltaOf(ctx.Items) is { } d) rows.Add([DeltaGroup, ctx.Value(d)]);
        return new ChartTable([ctx.GroupLabel, "Value"], rows);
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsGaugeChart)b).RequestRebuild();
}

/// <summary>Carbon's gauge geometry (gauge.ts): radius, arc angles (radians, 0 at 12 o'clock, clockwise) and center.</summary>
internal readonly record struct GaugeLayout(PointF Center, float Radius, float Inner, float Start, float Current, float End);

internal static class GaugeGeometry
{
    // The delta arrows' points on a 16-unit artboard (Carbon's ARROW_UP and ARROW_DOWN).
    static readonly PointF[] ArrowUp = [new(4, 10), new(8, 6), new(12, 10)];
    static readonly PointF[] ArrowDown = [new(12, 6), new(8, 10), new(4, 6)];

    /// <summary>
    /// A semi gauge has radius min(width / 2, height) and spans 9 to 3 o'clock; a full one min(width / 2, height / 2) and
    /// starts at 12 o'clock. The center is one radius from the top and from the left (or centered, or one radius from the right).
    /// </summary>
    public static GaugeLayout Layout(float width, float height, bool full, float arcWidth, double value, CdsChartAlignment alignment)
    {
        var radius = Math.Max(0, full ? Math.Min(width / 2, height / 2) : Math.Min(width / 2, height));
        var inner = Math.Max(0, radius - arcWidth);
        var ratio = (float)(Math.Clamp(double.IsNaN(value) ? 0 : value, 0, 100) / 100);
        var size = (full ? 1f : 0.5f) * MathF.PI * 2;
        var start = full ? 0 : -size / 2;
        var tx = alignment switch
        {
            CdsChartAlignment.Center => width / 2,
            CdsChartAlignment.Right => width - radius,
            _ => radius,
        };
        return new GaugeLayout(new PointF(tx, radius), radius, inner, start, start + ratio * size, start + size);
    }

    /// <summary>Two decimals when the number has a fraction at that precision, else none (Carbon's gauge number).</summary>
    public static double Round(double value)
    {
        var two = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        return two % 1 != 0 ? two : Math.Round(value, 0, MidpointRounding.AwayFromZero);
    }

    /// <summary>The delta arrow: a triangle on a 16-unit artboard scaled to <paramref name="size"/> at <paramref name="origin"/>.</summary>
    public static PathF Arrow(PointF origin, float size, bool up)
    {
        var s = size / 16;
        var path = new PathF();
        var points = up ? ArrowUp : ArrowDown;
        path.MoveTo(origin.X + points[0].X * s, origin.Y + points[0].Y * s);
        for (var i = 1; i < points.Length; i++) path.LineTo(origin.X + points[i].X * s, origin.Y + points[i].Y * s);
        path.Close();
        return path;
    }
}
