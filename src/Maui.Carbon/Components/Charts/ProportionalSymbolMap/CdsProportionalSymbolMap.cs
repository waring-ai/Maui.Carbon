namespace Maui.Carbon;

/// <summary>
/// Circles at longitude and latitude over a muted base map, each circle's area proportional to its value, colored by group,
/// with a size key (a proportional symbol map). Data is <see cref="CdsGeoPoint"/>s. Use it for totals at places (city
/// populations, site capacity); use a <see cref="CdsChoroplethChart"/> for rates per region and a
/// <see cref="CdsConnectionMap"/> for flows. Area, not radius, follows the value: the largest value gets
/// <see cref="MaximumRadius"/>, and no circle is smaller than <see cref="MinimumRadius"/>. Circles are the group color at 20%
/// opacity with a 1px stroke, largest first so small ones stay on top.
/// </summary>
public class CdsProportionalSymbolMap : CdsThematicChart
{
    /// <summary>Bindable property for <see cref="RadiusLabel"/>.</summary>
    public static readonly BindableProperty RadiusLabelProperty = BindableProperty.Create(
        nameof(RadiusLabel), typeof(string), typeof(CdsProportionalSymbolMap), "Value", propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="LocationLabel"/>.</summary>
    public static readonly BindableProperty LocationLabelProperty = BindableProperty.Create(
        nameof(LocationLabel), typeof(string), typeof(CdsProportionalSymbolMap), "Location", propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="MinimumRadius"/>.</summary>
    public static readonly BindableProperty MinimumRadiusProperty = BindableProperty.Create(
        nameof(MinimumRadius), typeof(double?), typeof(CdsProportionalSymbolMap), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="MaximumRadius"/>.</summary>
    public static readonly BindableProperty MaximumRadiusProperty = BindableProperty.Create(
        nameof(MaximumRadius), typeof(double?), typeof(CdsProportionalSymbolMap), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="SymbolFillOpacity"/>.</summary>
    public static readonly BindableProperty SymbolFillOpacityProperty = BindableProperty.Create(
        nameof(SymbolFillOpacity), typeof(double), typeof(CdsProportionalSymbolMap), 0.2, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="IsSizeLegendEnabled"/>.</summary>
    public static readonly BindableProperty IsSizeLegendEnabledProperty = BindableProperty.Create(
        nameof(IsSizeLegendEnabled), typeof(bool), typeof(CdsProportionalSymbolMap), true, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="SizeLegendTitle"/>.</summary>
    public static readonly BindableProperty SizeLegendTitleProperty = BindableProperty.Create(
        nameof(SizeLegendTitle), typeof(string), typeof(CdsProportionalSymbolMap), null, propertyChanged: Changed);

    /// <summary>The name of the value in tooltips, the table and the size key (Carbon's <c>radiusLabel</c>). Default "Value"; give it a unit ("Population (M)").</summary>
    public string? RadiusLabel
    {
        get => (string?)GetValue(RadiusLabelProperty);
        set => SetValue(RadiusLabelProperty, value);
    }

    /// <summary>The name of a location in tooltips and the table (Carbon's <c>locationLabel</c>). Default "Location".</summary>
    public string? LocationLabel
    {
        get => (string?)GetValue(LocationLabelProperty);
        set => SetValue(LocationLabelProperty, value);
    }

    /// <summary>The smallest radius drawn (Carbon's <c>radiusRange[0]</c>); <c>null</c> (default) is min(width, height) x 3/400.</summary>
    public double? MinimumRadius
    {
        get => (double?)GetValue(MinimumRadiusProperty);
        set => SetValue(MinimumRadiusProperty, value);
    }

    /// <summary>The radius of the largest value (Carbon's <c>radiusRange[1]</c>); <c>null</c> (default) is min(width, height) x 25/400.</summary>
    public double? MaximumRadius
    {
        get => (double?)GetValue(MaximumRadiusProperty);
        set => SetValue(MaximumRadiusProperty, value);
    }

    /// <summary>The circles' fill opacity (Carbon's <c>fillOpacity</c>). Default 0.2; a hovered circle fills fully.</summary>
    public double SymbolFillOpacity
    {
        get => (double)GetValue(SymbolFillOpacityProperty);
        set => SetValue(SymbolFillOpacityProperty, value);
    }

    /// <summary>Whether the size key shows under the map (Carbon's <c>sizeLegend.enabled</c>). Default <c>true</c>: area is hard to judge without it.</summary>
    public bool IsSizeLegendEnabled
    {
        get => (bool)GetValue(IsSizeLegendEnabledProperty);
        set => SetValue(IsSizeLegendEnabledProperty, value);
    }

    /// <summary>The size key's title (Carbon's <c>sizeLegend.title</c>); default <see cref="RadiusLabel"/>.</summary>
    public string? SizeLegendTitle
    {
        get => (string?)GetValue(SizeLegendTitleProperty);
        set => SetValue(SizeLegendTitleProperty, value);
    }

    private protected override string ChartKind => "Proportional symbol map";

    private protected override bool IsEmpty(IReadOnlyList<object> items) => !items.OfType<CdsGeoPoint>().Any();

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) =>
        items.OfType<CdsGeoPoint>().Select(p => p.Group ?? "").Distinct().ToList();

    /// <summary>Rounds to <paramref name="digits"/> significant digits, so size key values read as round numbers.</summary>
    internal static double RoundSignificant(double v, int digits)
    {
        if (!(v > 0)) return 0;
        var p = Math.Pow(10, Math.Floor(Math.Log10(v)) - (digits - 1));
        return Math.Round(v / p, MidpointRounding.AwayFromZero) * p;
    }

    /// <summary>The radius of a value: a square-root scale from 0 (area proportional), at least <paramref name="min"/>.</summary>
    internal static double Radius(double? value, double maxValue, double min, double max)
    {
        if (value is not { } v || double.IsNaN(v)) return 0;
        var r = maxValue > 0 ? Math.Sqrt(Math.Abs(v) / maxValue) * max : 0;
        return Math.Max(min, r);
    }

    /// <summary>The size key: the largest value rounded, about a quarter and about a sixteenth, smallest first.</summary>
    internal static List<double> SizeKeyValues(double maxValue) =>
        new[] { RoundSignificant(maxValue, 2), RoundSignificant(maxValue / 4, 1), RoundSignificant(maxValue / 16, 1) }
            .Where(v => v > 0).Distinct().Reverse().ToList();

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var all = ctx.Items.OfType<CdsGeoPoint>().ToList();
        var e = Math.Min(ctx.Width, ctx.Height);
        var r0 = MinimumRadius ?? e * 3 / 400.0;
        var r1 = MaximumRadius ?? e * 25 / 400.0;
        var maxValue = all.Select(p => p.Value is { } v && !double.IsNaN(v) ? Math.Abs(v) : 0).DefaultIfEmpty(0).Max();
        if (maxValue <= 0) maxValue = 1;

        var legendOn = IsSizeLegendEnabled;
        var title = string.IsNullOrEmpty(SizeLegendTitle) ? RadiusLabel : SizeLegendTitle;
        var keyValues = SizeKeyValues(maxValue);
        var keyRadii = keyValues.Select(v => Radius(v, maxValue, r0, r1)).ToList();
        var rMax = keyRadii.Count > 0 ? (float)keyRadii[^1] : 0;
        var keyTop = string.IsNullOrEmpty(title) ? 0f : 20f;
        var keyHeight = keyTop + 2 * rMax + 20;
        var h = legendOn ? Math.Max(1, ctx.Height - keyHeight - ChartColorLegend.Spacer) : ctx.Height;
        var plot = new RectF(0, 0, ctx.Width, h);
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = plot, HitMode = ChartHitMode.Contains };

        var located = all.Where(p => p.HasLocation).ToList();
        var fitData = EffectiveFit == CdsMapFit.Data;
        var projection = Fit(plot, null, located.Select(p => new GeoPos(p.Longitude!.Value, p.Latitude!.Value)).ToList(), fitData ? (float)r1 + 2 : 0);
        GeoScene.BaseMap(scene, projection, MapFeatures);

        var symbols = located.Where(p => ctx.IsActive(p.Group ?? ""))
            .Select(p => (P: p, At: projection.Project(p.Longitude!.Value, p.Latitude!.Value), R: (float)Radius(p.Value, maxValue, r0, r1)))
            .Where(s => float.IsFinite(s.At.X) && float.IsFinite(s.At.Y) && s.R > 0)
            .OrderByDescending(s => s.R)
            .ToList();
        var id = 0;
        foreach (var s in symbols)
        {
            var g = s.P.Group ?? "";
            var mark = id++;
            scene.Shapes.Add(new ChartCircle
            {
                Center = s.At,
                Radius = s.R,
                Fill = ChartPaint.OfSeries(g),
                FillOpacity = (float)SymbolFillOpacity,
                HoverFillOpacity = 1,
                Stroke = ChartPaint.OfSeries(g),
                StrokeWidth = 1,
                Group = g,
                Mark = mark,
                Hover = ChartHoverStyle.None,
                Clip = true,
            });
            var rows = new List<ChartTooltipRow>();
            if (s.P.Name is not null) rows.Add(new ChartTooltipRow(LocationLabel ?? "Location", s.P.Name));
            if (s.P.Value is { } v) rows.Add(new ChartTooltipRow(RadiusLabel ?? "Value", ctx.Value(v)));
            if (s.P.Group is not null) rows.Add(new ChartTooltipRow(ctx.GroupLabel, s.P.Group, ChartPaint.OfSeries(g)));
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = new RectF(s.At.X - s.R, s.At.Y - s.R, 2 * s.R, 2 * s.R),
                Radius = Math.Max(s.R, 4),
                Anchor = s.At,
                Group = g,
                Rows = rows,
                Description = $"{s.P.Name}, {ctx.Value(s.P.Value)}" + (string.IsNullOrEmpty(s.P.Group) ? "" : $", {s.P.Group}"),
            });
        }

        if (legendOn)
        {
            var top = h + ChartColorLegend.Spacer;
            if (!string.IsNullOrEmpty(title))
                scene.Texts.Add(new ChartText { Text = title!, X = 0, Y = top - 2, Color = ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute) });
            float x = 0;
            var cy = top + keyTop + rMax;
            for (var i = 0; i < keyValues.Count; i++)
            {
                var r = (float)keyRadii[i];
                var label = ctx.Number(keyValues[i]);
                var w = Math.Max(2 * r, CdsChartTextMetrics.Width(label, ChartFont.Tick));
                var cx = x + w / 2;
                x += w + 16;
                scene.Shapes.Add(new ChartCircle { Center = new PointF(cx, cy + rMax - r), Radius = r, Stroke = ChartPaint.OfKey(CdsColorKeys.TextSecondary), StrokeWidth = 1, Hover = ChartHoverStyle.None });
                scene.Texts.Add(new ChartText { Text = label, X = cx, Y = top + keyTop + 2 * rMax + 6 - 2, Anchor = ChartTextAnchor.Middle });
            }
        }
        scene.Summary = (id == 1 ? "1 symbol" : $"{id} symbols")
            + (legendOn && keyValues.Count > 0 ? $", {(string.IsNullOrEmpty(title) ? "circle sizes" : title)}: " + string.Join(", ", keyValues.Select(v => ctx.Number(v))) : "");
        return scene;
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx) => new(
        [ctx.GroupLabel, LocationLabel ?? "Location", "Longitude", "Latitude", RadiusLabel ?? "Value"],
        ctx.Items.OfType<CdsGeoPoint>().Where(p => ctx.IsActive(p.Group ?? "")).Select(p => new[]
        {
            p.Group ?? "", p.Name ?? "", ctx.Number(p.Longitude), ctx.Number(p.Latitude), ctx.Value(p.Value),
        }).ToList());

    static void Changed(BindableObject b, object? o, object? n) => ((CdsProportionalSymbolMap)b).RequestRebuild();
}
