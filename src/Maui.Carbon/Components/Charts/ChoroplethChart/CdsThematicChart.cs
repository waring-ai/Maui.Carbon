namespace Maui.Carbon;

/// <summary>
/// Base class of the map charts (Carbon's thematic charts): the map geometry you supply as TopoJSON or GeoJSON text, its
/// projection and what the view is fitted to. The chart never fetches geometry: load it yourself (for example from your app
/// package) and set <see cref="GeoData"/>, with <see cref="CdsChart.IsLoading"/> on until it is read. Everything outside
/// the plot is clipped.
/// </summary>
public abstract class CdsThematicChart : CdsChart
{
    /// <summary>Bindable property for <see cref="GeoData"/>.</summary>
    public static readonly BindableProperty GeoDataProperty = BindableProperty.Create(
        nameof(GeoData), typeof(string), typeof(CdsThematicChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ObjectsKey"/>.</summary>
    public static readonly BindableProperty ObjectsKeyProperty = BindableProperty.Create(
        nameof(ObjectsKey), typeof(string), typeof(CdsThematicChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="Projection"/>.</summary>
    public static readonly BindableProperty ProjectionProperty = BindableProperty.Create(
        nameof(Projection), typeof(CdsGeoProjection), typeof(CdsThematicChart), CdsGeoProjection.NaturalEarth1, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="FitTo"/>.</summary>
    public static readonly BindableProperty FitToProperty = BindableProperty.Create(
        nameof(FitTo), typeof(CdsMapFit?), typeof(CdsThematicChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="FitBounds"/>.</summary>
    public static readonly BindableProperty FitBoundsProperty = BindableProperty.Create(
        nameof(FitBounds), typeof(CdsGeoBounds), typeof(CdsThematicChart), null, propertyChanged: Changed);

    private protected CdsThematicChart()
    {
    }

    /// <summary>
    /// The map geometry (Carbon's <c>geoData</c>): TopoJSON (object <see cref="ObjectsKey"/>) or GeoJSON text (a
    /// FeatureCollection, a Feature or a geometry). Regions are matched by feature id or by <c>properties.NAME</c> or
    /// <c>properties.name</c>. Polygons are drawn; points and lines in the geometry are ignored. Charts that share one string
    /// parse it once.
    /// </summary>
    public string? GeoData
    {
        get => (string?)GetValue(GeoDataProperty);
        set => SetValue(GeoDataProperty, value);
    }

    /// <summary>The TopoJSON object to draw (Carbon's <c>thematic.objectsKey</c>); default <c>countries</c> when present, else the first object.</summary>
    public string? ObjectsKey
    {
        get => (string?)GetValue(ObjectsKeyProperty);
        set => SetValue(ObjectsKeyProperty, value);
    }

    /// <summary>The projection (Carbon's <c>thematic.projection</c>). Default <see cref="CdsGeoProjection.NaturalEarth1"/>; use a conic projection for one mid-latitude continent.</summary>
    public CdsGeoProjection Projection
    {
        get => (CdsGeoProjection)GetValue(ProjectionProperty);
        set => SetValue(ProjectionProperty, value);
    }

    /// <summary>What the view fits (Carbon's <c>thematic.fitTo</c>): <c>null</c> follows the chart (the regions with data for a choropleth, the whole map for the symbol and connection maps).</summary>
    public CdsMapFit? FitTo
    {
        get => (CdsMapFit?)GetValue(FitToProperty);
        set => SetValue(FitToProperty, value);
    }

    /// <summary>A fixed view in degrees (Carbon's <c>fitTo: [[west, south], [east, north]]</c>); wins over <see cref="FitTo"/>.</summary>
    public CdsGeoBounds? FitBounds
    {
        get => (CdsGeoBounds?)GetValue(FitBoundsProperty);
        set => SetValue(FitBoundsProperty, value);
    }

    private protected virtual CdsMapFit DefaultFitTo => CdsMapFit.Map;

    /// <summary>The fit in effect.</summary>
    private protected CdsMapFit EffectiveFit => FitTo ?? DefaultFitTo;

    private protected override ChartSkeleton Skeleton => ChartSkeleton.Lines;

    /// <summary>The parsed geometry, or null.</summary>
    internal GeoMap? Map => GeoMap.Parse(GeoData);

    /// <summary>The regions of the base map.</summary>
    internal IReadOnlyList<GeoFeature> MapFeatures => Map?.Features(ObjectsKey) ?? [];

    /// <summary>
    /// A projection fitted into <paramref name="extent"/>: to <see cref="FitBounds"/>, else to the data (<paramref name="dataFeatures"/> or <paramref name="dataPoints"/>)
    /// (when fitting the data and there is some), else to the base map's regions, else to the data.
    /// </summary>
    internal GeoProjection Fit(RectF extent, IReadOnlyList<GeoFeature>? dataFeatures, IReadOnlyList<GeoPos>? dataPoints, float padding)
    {
        var projection = new GeoProjection(Projection);
        var inner = extent.Width > 2 * padding + 1 && extent.Height > 2 * padding + 1 ? extent.Inflate(-padding, -padding) : extent;
        IEnumerable<GeoPos> target;
        var features = MapFeatures;
        if (FitBounds is { } box) target = box.FitPoints().Select(projection.Rotate);
        else if (EffectiveFit == CdsMapFit.Data && dataFeatures is { Count: > 0 }) target = GeoScene.Positions(projection, dataFeatures);
        else if (EffectiveFit == CdsMapFit.Data && dataPoints is { Count: > 0 }) target = dataPoints.Select(projection.Rotate);
        else if (features.Count > 0) target = GeoScene.Positions(projection, features);
        else if (dataFeatures is { Count: > 0 }) target = GeoScene.Positions(projection, dataFeatures);
        else target = (dataPoints ?? []).Select(projection.Rotate);
        projection.Fit(inner, target);
        return projection;
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsThematicChart)b).RequestRebuild();
}

/// <summary>Map scene building shared by the map charts: projected regions, hit areas, the muted base map and line hit areas.</summary>
internal static class GeoScene
{
    /// <summary>Every rotated, cut position of the features (what a fit measures).</summary>
    public static IEnumerable<GeoPos> Positions(GeoProjection projection, IEnumerable<GeoFeature> features)
    {
        foreach (var f in features)
            foreach (var ring in f.Rings)
                foreach (var piece in projection.CutRing(ring.Points))
                    foreach (var p in piece)
                        yield return p;
    }

    /// <summary>The projected rings of a feature (every polygon, hole and antimeridian piece).</summary>
    public static List<PointF[]> Project(GeoProjection projection, GeoFeature feature)
    {
        var rings = new List<PointF[]>();
        foreach (var ring in feature.Rings)
            foreach (var piece in projection.CutRing(ring.Points))
            {
                var pts = piece.Select(projection.ProjectRotated).Where(p => float.IsFinite(p.X) && float.IsFinite(p.Y)).ToArray();
                if (pts.Length >= 3) rings.Add(pts);
            }
        return rings;
    }

    /// <summary>The rings as one path of closed subpaths (filled even-odd, so holes stay open).</summary>
    public static PathF Path(IEnumerable<PointF[]> rings)
    {
        var path = new PathF();
        foreach (var ring in rings)
        {
            path.MoveTo(ring[0]);
            for (var i = 1; i < ring.Length; i++) path.LineTo(ring[i]);
            path.Close();
        }
        return path;
    }

    /// <summary>
    /// One polygon that hit-tests like all the rings together (even-odd): each ring is reached from the first ring's start
    /// and left back to it along the same bridge, so the bridges cancel out.
    /// </summary>
    public static PointF[] HitPolygon(IReadOnlyList<PointF[]> rings)
    {
        if (rings.Count == 0) return [];
        if (rings.Count == 1) return rings[0];
        var anchor = rings[0][0];
        var all = new List<PointF>();
        foreach (var ring in rings)
        {
            all.Add(anchor);
            all.AddRange(ring);
            all.Add(ring[0]);
            all.Add(anchor);
        }
        return all.ToArray();
    }

    /// <summary>The bounding box of rings.</summary>
    public static RectF Bounds(IEnumerable<PointF> points)
    {
        float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
        foreach (var p in points)
        {
            x0 = Math.Min(x0, p.X);
            y0 = Math.Min(y0, p.Y);
            x1 = Math.Max(x1, p.X);
            y1 = Math.Max(y1, p.Y);
        }
        return x0 > x1 ? RectF.Zero : new RectF(x0, y0, x1 - x0, y1 - y0);
    }

    /// <summary>The centroid of the largest ring (where a keyboard tooltip points).</summary>
    public static PointF Anchor(IReadOnlyList<PointF[]> rings)
    {
        PointF[]? best = null;
        double bestArea = -1;
        foreach (var r in rings)
        {
            double a = 0;
            for (int i = 0, j = r.Length - 1; i < r.Length; j = i++) a += r[j].X * r[i].Y - r[i].X * r[j].Y;
            if (Math.Abs(a) > bestArea)
            {
                bestArea = Math.Abs(a);
                best = r;
            }
        }
        if (best is null) return PointF.Zero;
        double cx = 0, cy = 0, area = 0;
        for (int i = 0, j = best.Length - 1; i < best.Length; j = i++)
        {
            var f = best[j].X * best[i].Y - best[i].X * best[j].Y;
            cx += (best[j].X + best[i].X) * f;
            cy += (best[j].Y + best[i].Y) * f;
            area += f;
        }
        if (Math.Abs(area) < 1e-6) return new PointF(best.Average(p => p.X), best.Average(p => p.Y));
        return new PointF((float)(cx / (3 * area)), (float)(cy / (3 * area)));
    }

    /// <summary>The muted base map: land in <c>layer-accent-01</c>, borders 0.5px in <c>background</c>.</summary>
    public static void BaseMap(ChartScene scene, GeoProjection projection, IReadOnlyList<GeoFeature> features)
    {
        foreach (var f in features)
        {
            var rings = Project(projection, f);
            if (rings.Count == 0) continue;
            scene.Shapes.Add(new ChartPath
            {
                Path = Path(rings),
                Winding = WindingMode.EvenOdd,
                Fill = ChartPaint.OfKey(CdsColorKeys.LayerAccent01),
                Stroke = ChartPaint.OfKey(CdsColorKeys.Background),
                StrokeWidth = 0.5f,
                Hover = ChartHoverStyle.None,
                Clip = true,
            });
        }
    }

    /// <summary>A path through points.</summary>
    public static PathF Polyline(IReadOnlyList<PointF> points)
    {
        var path = new PathF();
        if (points.Count == 0) return path;
        path.MoveTo(points[0]);
        for (var i = 1; i < points.Count; i++) path.LineTo(points[i]);
        return path;
    }

    /// <summary>
    /// The hit area of a stroked polyline: the outline of the line widened to <paramref name="width"/> (mitered joins,
    /// limited at sharp turns), as one polygon.
    /// </summary>
    public static PointF[] StrokeHit(IReadOnlyList<PointF> points, float width)
    {
        var pts = new List<PointF>();
        foreach (var p in points)
            if (pts.Count == 0 || Math.Abs(pts[^1].X - p.X) + Math.Abs(pts[^1].Y - p.Y) > 0.01f) pts.Add(p);
        var h = width / 2;
        if (pts.Count == 1) return [new(pts[0].X - h, pts[0].Y - h), new(pts[0].X + h, pts[0].Y - h), new(pts[0].X + h, pts[0].Y + h), new(pts[0].X - h, pts[0].Y + h)];
        if (pts.Count == 0) return [];
        var left = new List<PointF>();
        var right = new List<PointF>();
        PointF Normal(PointF a, PointF b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var len = MathF.Sqrt(dx * dx + dy * dy);
            return len < 1e-6f ? new PointF(0, 0) : new PointF(-dy / len, dx / len);
        }
        for (var i = 0; i < pts.Count; i++)
        {
            PointF n;
            float scale = 1;
            if (i == 0) n = Normal(pts[0], pts[1]);
            else if (i == pts.Count - 1) n = Normal(pts[i - 1], pts[i]);
            else
            {
                var n0 = Normal(pts[i - 1], pts[i]);
                var n1 = Normal(pts[i], pts[i + 1]);
                n = new PointF(n0.X + n1.X, n0.Y + n1.Y);
                var len = MathF.Sqrt(n.X * n.X + n.Y * n.Y);
                if (len < 1e-3f) n = n0;
                else
                {
                    n = new PointF(n.X / len, n.Y / len);
                    var cos = n.X * n0.X + n.Y * n0.Y;
                    scale = Math.Min(2, 1 / Math.Max(0.1f, cos));
                }
            }
            left.Add(new PointF(pts[i].X + n.X * h * scale, pts[i].Y + n.Y * h * scale));
            right.Add(new PointF(pts[i].X - n.X * h * scale, pts[i].Y - n.Y * h * scale));
        }
        right.Reverse();
        left.AddRange(right);
        return left.ToArray();
    }
}
