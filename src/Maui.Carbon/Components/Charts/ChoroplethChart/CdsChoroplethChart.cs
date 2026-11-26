namespace Maui.Carbon;

/// <summary>
/// A map whose regions are shaded by value (Carbon's choropleth), with a color legend and a hatch for regions without data.
/// Each <see cref="CdsChartPoint"/> is a region: <see cref="CdsChartPoint.Key"/> matches a feature id (ISO 3166 numeric in
/// world-atlas files), else its name (<c>properties.NAME</c> or <c>properties.name</c>), and <see cref="CdsChartPoint.Value"/>
/// is its value. Use it for rates and shares (percent of population), not raw totals, where large regions dominate: use a
/// <see cref="CdsProportionalSymbolMap"/> for those. The color scale is the heat map's: 11 sequential steps
/// (<see cref="CdsChart.ColorPairingOption"/> 1-4: purple, blue, cyan, teal) or 17 diverging steps when values cross 0
/// (1 red-cyan, 2 purple-teal). By default the view fits the regions that have data.
/// </summary>
public class CdsChoroplethChart : CdsThematicChart
{
    /// <summary>Bindable property for <see cref="ColorLegendTitle"/>.</summary>
    public static readonly BindableProperty ColorLegendTitleProperty = BindableProperty.Create(
        nameof(ColorLegendTitle), typeof(string), typeof(CdsChoroplethChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ColorLegendType"/>.</summary>
    public static readonly BindableProperty ColorLegendTypeProperty = BindableProperty.Create(
        nameof(ColorLegendType), typeof(CdsChartColorLegendType), typeof(CdsChoroplethChart), CdsChartColorLegendType.Linear, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ColorDomainMinimum"/>.</summary>
    public static readonly BindableProperty ColorDomainMinimumProperty = BindableProperty.Create(
        nameof(ColorDomainMinimum), typeof(double?), typeof(CdsChoroplethChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ColorDomainMaximum"/>.</summary>
    public static readonly BindableProperty ColorDomainMaximumProperty = BindableProperty.Create(
        nameof(ColorDomainMaximum), typeof(double?), typeof(CdsChoroplethChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="GradientColors"/>.</summary>
    public static readonly BindableProperty GradientColorsProperty = BindableProperty.Create(
        nameof(GradientColors), typeof(IList<Color>), typeof(CdsChoroplethChart), null, propertyChanged: Changed);

    /// <summary>The color legend's title, above the bar (Carbon's <c>choropleth.colorLegend.title</c>): name the measure and unit ("% of population").</summary>
    public string? ColorLegendTitle
    {
        get => (string?)GetValue(ColorLegendTitleProperty);
        set => SetValue(ColorLegendTitleProperty, value);
    }

    /// <summary>A gradient bar (default) or one block per step (Carbon's <c>choropleth.colorLegend.type</c>). Hide the legend with <see cref="CdsChart.IsLegendEnabled"/> <c>false</c>.</summary>
    public CdsChartColorLegendType ColorLegendType
    {
        get => (CdsChartColorLegendType)GetValue(ColorLegendTypeProperty);
        set => SetValue(ColorLegendTypeProperty, value);
    }

    /// <summary>The low end of the color domain (Carbon's <c>choropleth.colorDomain.min</c>); <c>null</c> is the data's nice extent from 0 (symmetric around 0 for mixed signs). Use it to clamp outliers.</summary>
    public double? ColorDomainMinimum
    {
        get => (double?)GetValue(ColorDomainMinimumProperty);
        set => SetValue(ColorDomainMinimumProperty, value);
    }

    /// <summary>The high end of the color domain (Carbon's <c>choropleth.colorDomain.max</c>).</summary>
    public double? ColorDomainMaximum
    {
        get => (double?)GetValue(ColorDomainMaximumProperty);
        set => SetValue(ColorDomainMaximumProperty, value);
    }

    /// <summary>Your own color steps, lowest value first (Carbon's <c>color.gradient.colors</c>), instead of the Carbon palettes.</summary>
    public IList<Color>? GradientColors
    {
        get => (IList<Color>?)GetValue(GradientColorsProperty);
        set => SetValue(GradientColorsProperty, value);
    }

    private protected override CdsMapFit DefaultFitTo => CdsMapFit.Data;

    private protected override string ChartKind => "Choropleth map";

    private protected override bool DefaultLegendEnabled => false;

    private protected override IReadOnlyList<string> LegendGroups => [];

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) => [];

    private protected override bool IsEmpty(IReadOnlyList<object> items) => items.Count == 0 || MapFeatures.Count == 0;

    /// <summary>
    /// The row of each feature (by index): a row's key matches a feature id first, else a feature name; later rows win.
    /// </summary>
    internal static Dictionary<int, CdsChartPoint> Join(IReadOnlyList<GeoFeature> features, IEnumerable<CdsChartPoint> rows)
    {
        var byId = new Dictionary<string, int>(StringComparer.Ordinal);
        var byName = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < features.Count; i++)
        {
            if (features[i].Id is { } id) byId[id] = i;
            byName[features[i].Name] = i;
        }
        var values = new Dictionary<int, CdsChartPoint>();
        foreach (var d in rows)
        {
            if (d.Key is not { } key) continue;
            if (byId.TryGetValue(key, out var i) || byName.TryGetValue(key, out i)) values[i] = d;
            else System.Diagnostics.Debug.WriteLine($"Data point {key} is missing geographical data.");
        }
        return values;
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var features = MapFeatures;
        var legendOn = IsLegendEnabled ?? true;
        var title = ColorLegendTitle;
        var legend = ChartColorLegend.Measure(ctx.Width, title, 0, heatmap: false);
        var gh = legendOn ? Math.Max(1, ctx.Height - legend.Height - ChartColorLegend.Spacer) : ctx.Height;
        var plot = new RectF(0, 0, ctx.Width, gh);
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = plot, HitMode = ChartHitMode.Contains };

        var scale = ChartColorScale.Create(ctx.Points.Where(d => d.Value.HasValue).Select(d => d.Value!.Value), ColorDomainMinimum, ColorDomainMaximum, ColorPairingOption, GradientColors);
        var values = Join(features, ctx.Points);
        var withData = new List<(GeoFeature F, CdsChartPoint D)>();
        var missing = new List<int>();
        for (var i = 0; i < features.Count; i++)
        {
            if (values.TryGetValue(i, out var d) && d.Value is { } v && !double.IsNaN(v)) withData.Add((features[i], d));
            else missing.Add(i);
        }
        var projection = Fit(plot, withData.Select(x => x.F).ToList(), null, 0);

        var regions = new List<(GeoFeature F, CdsChartPoint D, List<PointF[]> Rings)>();
        foreach (var (f, d) in withData)
        {
            var rings = GeoScene.Project(projection, f);
            if (rings.Count > 0) regions.Add((f, d, rings));
        }
        // Reading order: rows of the map from the top, then left to right.
        var band = Math.Max(24f, gh / 8);
        var ordered = regions.Select(r => (r.F, r.D, r.Rings, Anchor: GeoScene.Anchor(r.Rings)))
            .OrderBy(r => Math.Floor(r.Anchor.Y / band)).ThenBy(r => r.Anchor.X).ToList();
        var id = 0;
        var outlines = new List<ChartPath>();
        foreach (var r in ordered)
        {
            var fill = scale.Paint(r.D.Value!.Value);
            var mark = id++;
            var path = GeoScene.Path(r.Rings);
            scene.Shapes.Add(new ChartPath { Path = path, Winding = WindingMode.EvenOdd, Fill = fill, Stroke = ChartPaint.OfKey(CdsColorKeys.BorderSubtleSelected01), StrokeWidth = 1, Mark = mark, Hover = ChartHoverStyle.None, Clip = true });
            outlines.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute), StrokeWidth = 1.5f, Mark = mark, HoverOnly = true, Hover = ChartHoverStyle.None, Clip = true });
            var name = string.IsNullOrEmpty(r.F.Name) ? r.D.Key ?? "" : r.F.Name;
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = GeoScene.Bounds(r.Rings.SelectMany(x => x)),
                Polygon = GeoScene.HitPolygon(r.Rings),
                Anchor = r.Anchor,
                Rows = [new ChartTooltipRow(CdsChartTruncate.Apply(name), ctx.Value(r.D.Value), fill)],
                Description = $"{name}: {ctx.Value(r.D.Value)}",
            });
        }

        // Regions without data read as one striped shape: background, a 45 degree hatch, and only their outer border.
        if (missing.Count > 0)
        {
            var missingRings = missing.SelectMany(i => GeoScene.Project(projection, features[i])).ToList();
            if (missingRings.Count > 0)
            {
                scene.Shapes.Add(new ChartPath { Path = GeoScene.Path(missingRings), Winding = WindingMode.EvenOdd, Fill = ChartPaint.OfKey(CdsColorKeys.Background), Hover = ChartHoverStyle.None, Clip = true });
                scene.Shapes.Add(new ChartPath { Path = ChartHatch.Lines(missingRings, 5), Stroke = ChartPaint.OfKey(CdsColorKeys.BorderStrong01), StrokeWidth = 0.5f, Hover = ChartHoverStyle.None, Clip = true });
                scene.Shapes.Add(new ChartPath { Path = MissingBorder(projection, features, missing, missingRings), Stroke = ChartPaint.OfKey(CdsColorKeys.BorderSubtleSelected01), StrokeWidth = 1, Hover = ChartHoverStyle.None, Clip = true });
            }
        }
        scene.Shapes.AddRange(outlines);

        if (legendOn) ChartColorLegend.Draw(scene, ctx, scale, legend, ColorLegendType, title, heatmap: false, gh + ChartColorLegend.Spacer);
        scene.Summary = (id == 1 ? "1 region" : $"{id} regions") + (legendOn ? ", " + ChartColorLegend.Describe(ctx, scale, title) : "");
        return scene;
    }

    /// <summary>
    /// The border of the merged regions without data (topojson's <c>merge</c>): for TopoJSON, the arcs used by exactly one of
    /// their rings; for GeoJSON, every ring.
    /// </summary>
    PathF MissingBorder(GeoProjection projection, IReadOnlyList<GeoFeature> features, List<int> missing, List<PointF[]> missingRings)
    {
        var path = new PathF();
        var map = Map;
        var rings = missing.SelectMany(i => features[i].Rings).ToList();
        if (map is { IsTopology: true } && rings.All(r => r.Arcs is not null))
        {
            var uses = new Dictionary<int, int>();
            foreach (var r in rings)
                foreach (var a in r.Arcs!)
                {
                    var k = a < 0 ? ~a : a;
                    uses[k] = uses.GetValueOrDefault(k) + 1;
                }
            foreach (var (k, count) in uses)
            {
                if (count != 1 || k >= map.Arcs.Count) continue;
                foreach (var part in projection.CutLine(map.Arcs[k]))
                {
                    var pts = part.Select(projection.ProjectRotated).ToArray();
                    path.MoveTo(pts[0]);
                    for (var i = 1; i < pts.Length; i++) path.LineTo(pts[i]);
                }
            }
            return path;
        }
        foreach (var ring in missingRings)
        {
            path.MoveTo(ring[0]);
            for (var i = 1; i < ring.Length; i++) path.LineTo(ring[i]);
            path.Close();
        }
        return path;
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        const string dash = "–";
        var features = MapFeatures;
        var matched = Join(features, ctx.Points);
        var idOf = new Dictionary<CdsChartPoint, GeoFeature>(ReferenceEqualityComparer.Instance);
        foreach (var (i, d) in matched) idOf[d] = features[i];
        return new ChartTable(
            ["Country ID", "Country Name", "Value"],
            ctx.Points.Select(d =>
            {
                var f = idOf.GetValueOrDefault(d);
                var byId = f is not null && f.Id == d.Key;
                return new[]
                {
                    (byId ? d.Key : f?.Id) ?? dash,
                    (byId ? f!.Name : d.Key) ?? dash,
                    d.Value is { } v && !double.IsNaN(v) ? ctx.Value(v) : dash,
                };
            }).ToList());
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsChoroplethChart)b).RequestRebuild();
}
