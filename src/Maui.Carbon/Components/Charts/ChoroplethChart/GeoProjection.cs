namespace Maui.Carbon;

/// <summary>
/// A map projection (d3-geo's raw projections: Natural Earth 1, Equal Earth, equirectangular, Mercator, conic equal-area,
/// Albers and conic equidistant) with a longitude rotation, fitted into a rectangle like d3's <c>fitExtent</c>.
/// Geometry is projected vertex by vertex in the plane, after these steps in degrees: rotate, cut at the antimeridian
/// (a ring that crosses it is unwrapped and clipped into its pieces on either side; a ring that winds around a pole is
/// closed along that pole), and resample long edges every 2 degrees so curved parallels and meridians stay curved. This is
/// simpler than d3's spherical clipping: rings are not clipped to the sphere's horizon (no projection here needs it).
/// </summary>
internal sealed class GeoProjection
{
    const double Rad = Math.PI / 180;
    const double MaxStep = 2;

    readonly Func<double, double, (double X, double Y)> _raw;

    public GeoProjection(CdsGeoProjection kind)
    {
        Kind = kind;
        (_raw, Rotation) = kind switch
        {
            CdsGeoProjection.EqualEarth => (EqualEarth, 0d),
            CdsGeoProjection.Equirectangular => ((l, p) => (l, p), 0d),
            CdsGeoProjection.Mercator => (Mercator, 0d),
            CdsGeoProjection.ConicEqualArea => (ConicEqualArea(0, 60), 0d),
            CdsGeoProjection.Albers => (ConicEqualArea(29.5, 45.5), 96d),
            CdsGeoProjection.ConicEquidistant => (ConicEquidistant(0, 60), 0d),
            _ => (NaturalEarth1, 0d),
        };
    }

    public CdsGeoProjection Kind { get; }

    /// <summary>Degrees added to every longitude before projecting (d3's <c>rotate([λ, 0])</c>; Albers centers the US with 96).</summary>
    public double Rotation { get; }

    /// <summary>The scale and translation (d3's <c>scale</c> and <c>translate</c> for unit raw coordinates).</summary>
    public double K { get; private set; } = 150;

    public double Tx { get; private set; } = 480;

    public double Ty { get; private set; } = 250;

    // ----- raw projections (radians in, unit plane out, y north-up) -----

    internal static (double X, double Y) NaturalEarth1(double l, double p)
    {
        var p2 = p * p;
        var p4 = p2 * p2;
        return (l * (0.8707 - 0.131979 * p2 + p4 * (-0.013791 + p4 * (0.003971 * p2 - 0.001529 * p4))),
            p * (1.007226 + p2 * (0.015085 + p4 * (-0.044475 + 0.028874 * p2 - 0.005916 * p4))));
    }

    internal static (double X, double Y) EqualEarth(double l, double p)
    {
        const double a1 = 1.340264, a2 = -0.081106, a3 = 0.000893, a4 = 0.003796;
        var m = Math.Sqrt(3) / 2;
        var t = Math.Asin(m * Math.Sin(p));
        var t2 = t * t;
        var t6 = t2 * t2 * t2;
        return (l * Math.Cos(t) / (m * (a1 + 3 * a2 * t2 + t6 * (7 * a3 + 9 * a4 * t2))),
            t * (a1 + a2 * t2 + t6 * (a3 + a4 * t2)));
    }

    internal static (double X, double Y) Mercator(double l, double p)
    {
        var limit = 85.0511287798 * Rad;
        p = Math.Clamp(p, -limit, limit);
        return (l, Math.Log(Math.Tan((Math.PI / 2 + p) / 2)));
    }

    internal static Func<double, double, (double X, double Y)> ConicEqualArea(double parallel0, double parallel1)
    {
        var y0 = parallel0 * Rad;
        var y1 = parallel1 * Rad;
        var sy0 = Math.Sin(y0);
        var n = (sy0 + Math.Sin(y1)) / 2;
        if (Math.Abs(n) < 1e-6)
        {
            var cy0 = Math.Cos(y0);
            return (l, p) => (l * cy0, Math.Sin(p) / cy0);
        }
        var c = 1 + sy0 * (2 * n - sy0);
        var r0 = Math.Sqrt(c) / n;
        return (l, p) =>
        {
            var r = Math.Sqrt(Math.Max(0, c - 2 * n * Math.Sin(p))) / n;
            var a = l * n;
            return (r * Math.Sin(a), r0 - r * Math.Cos(a));
        };
    }

    internal static Func<double, double, (double X, double Y)> ConicEquidistant(double parallel0, double parallel1)
    {
        var y0 = parallel0 * Rad;
        var y1 = parallel1 * Rad;
        var cy0 = Math.Cos(y0);
        var n = y0 == y1 ? Math.Sin(y0) : (cy0 - Math.Cos(y1)) / (y1 - y0);
        if (Math.Abs(n) < 1e-6) return (l, p) => (l, p);
        var g = cy0 / n + y0;
        return (l, p) =>
        {
            var gy = g - p;
            var a = n * l;
            return (gy * Math.Sin(a), g - gy * Math.Cos(a));
        };
    }

    // ----- projecting -----

    /// <summary>Unit-plane coordinates of a rotated position, y down (before scale and translation).</summary>
    (double X, double Y) Plane(GeoPos rotated)
    {
        var (x, y) = _raw(rotated.Lon * Rad, Math.Clamp(rotated.Lat, -90, 90) * Rad);
        return (x, -y);
    }

    /// <summary>Rotates a position (longitude into [-180, 180]).</summary>
    public GeoPos Rotate(GeoPos p) => Rotation == 0 ? p : new GeoPos(Wrap(p.Lon + Rotation), p.Lat);

    /// <summary>The pixel of an already rotated position.</summary>
    public PointF ProjectRotated(GeoPos p)
    {
        var (x, y) = Plane(p);
        return new PointF((float)(x * K + Tx), (float)(y * K + Ty));
    }

    /// <summary>The pixel of a longitude and latitude.</summary>
    public PointF Project(double lon, double lat) => ProjectRotated(Rotate(new GeoPos(lon, lat)));

    /// <summary>
    /// Fits the projection into <paramref name="extent"/> around rotated positions (d3 <c>fitExtent</c>): the largest scale
    /// that fits their bounds, centered.
    /// </summary>
    public void Fit(RectF extent, IEnumerable<GeoPos> rotated)
    {
        double x0 = double.MaxValue, y0 = double.MaxValue, x1 = double.MinValue, y1 = double.MinValue;
        foreach (var p in rotated)
        {
            var (x, y) = Plane(p);
            if (double.IsNaN(x) || double.IsNaN(y) || double.IsInfinity(x) || double.IsInfinity(y)) continue;
            x0 = Math.Min(x0, x);
            y0 = Math.Min(y0, y);
            x1 = Math.Max(x1, x);
            y1 = Math.Max(y1, y);
        }
        if (x0 > x1)
        {
            K = 150;
            Tx = extent.Center.X;
            Ty = extent.Center.Y;
            return;
        }
        // A single point (or a line) has no area to fit: give it a 10 degree neighborhood.
        const double min = 10 * Rad;
        if (x1 - x0 < 1e-9 && y1 - y0 < 1e-9)
        {
            x0 -= min / 2;
            x1 += min / 2;
            y0 -= min / 2;
            y1 += min / 2;
        }
        var w = extent.Width;
        var h = extent.Height;
        var kx = x1 - x0 > 1e-12 ? w / (x1 - x0) : double.MaxValue;
        var ky = y1 - y0 > 1e-12 ? h / (y1 - y0) : double.MaxValue;
        K = Math.Min(kx, ky);
        Tx = extent.Left + (w - K * (x1 + x0)) / 2;
        Ty = extent.Top + (h - K * (y1 + y0)) / 2;
    }

    // ----- geometry in degrees -----

    internal static double Wrap(double lon)
    {
        var l = (lon + 180) % 360;
        if (l < 0) l += 360;
        return l - 180;
    }

    /// <summary>
    /// A ring rotated, cut at the antimeridian and resampled: one ring, or the pieces on either side of the antimeridian.
    /// </summary>
    public List<List<GeoPos>> CutRing(IReadOnlyList<GeoPos> ring)
    {
        var result = new List<List<GeoPos>>();
        var n = ring.Count;
        if (n > 1 && ring[0] == ring[n - 1]) n--;
        if (n < 3) return result;
        var u = new List<GeoPos>(n + 4);
        var prev = Rotate(ring[0]);
        u.Add(prev);
        var lon = prev.Lon;
        for (var i = 1; i < n; i++)
        {
            var r = Rotate(ring[i]);
            var d = r.Lon - prev.Lon;
            if (d > 180) d -= 360;
            else if (d < -180) d += 360;
            lon += d;
            u.Add(new GeoPos(lon, r.Lat));
            prev = r;
        }
        // Back to the start: a ring that winds once around a pole ends 360 degrees away from where it began.
        var close = Rotate(ring[0]).Lon - prev.Lon;
        if (close > 180) close -= 360;
        else if (close < -180) close += 360;
        var net = lon + close - u[0].Lon;
        if (Math.Abs(net) > 180)
        {
            var pole = u.Average(p => p.Lat) < 0 ? -90 : 90;
            var end = lon + close;
            u.Add(new GeoPos(end, u[0].Lat));
            u.Add(new GeoPos(end, pole));
            u.Add(new GeoPos(u[0].Lon, pole));
        }
        var lo = u.Min(p => p.Lon);
        var hi = u.Max(p => p.Lon);
        if (lo >= -180 - 1e-9 && hi <= 180 + 1e-9)
        {
            result.Add(Resample(u, closed: true));
            return result;
        }
        for (var shift = -360 * (int)Math.Ceiling((hi - 180) / 360); shift <= 360 * (int)Math.Ceiling((-180 - lo) / 360); shift += 360)
        {
            var shifted = u.Select(p => new GeoPos(p.Lon + shift, p.Lat)).ToList();
            var clipped = ClipX(ClipX(shifted, -180, keepAbove: true), 180, keepAbove: false);
            if (clipped.Count >= 3 && Math.Abs(Area(clipped)) > 1e-9) result.Add(Resample(clipped, closed: true));
        }
        return result;
    }

    /// <summary>A line (great-circle points, a border arc) rotated, split where it crosses the antimeridian, and resampled.</summary>
    public List<List<GeoPos>> CutLine(IReadOnlyList<GeoPos> line)
    {
        var parts = new List<List<GeoPos>>();
        if (line.Count == 0) return parts;
        var current = new List<GeoPos> { Rotate(line[0]) };
        for (var i = 1; i < line.Count; i++)
        {
            var a = current[^1];
            var b = Rotate(line[i]);
            var d = b.Lon - a.Lon;
            if (Math.Abs(d) > 180)
            {
                var edge = d > 0 ? -180 : 180;
                var bu = b.Lon + (d > 0 ? -360 : 360);
                var t = (edge - a.Lon) / (bu - a.Lon);
                var lat = a.Lat + (b.Lat - a.Lat) * (double.IsNaN(t) ? 0 : t);
                current.Add(new GeoPos(edge, lat));
                parts.Add(Resample(current, closed: false));
                current = [new GeoPos(-edge, lat)];
            }
            current.Add(b);
        }
        parts.Add(Resample(current, closed: false));
        return parts.Where(p => p.Count >= 2).ToList();
    }

    /// <summary>Points along the great circle from <paramref name="a"/> to <paramref name="b"/> (d3 <c>geoInterpolate</c>), about one per degree.</summary>
    public static List<GeoPos> GreatCircle(GeoPos a, GeoPos b)
    {
        double x0 = a.Lon * Rad, y0 = a.Lat * Rad, x1 = b.Lon * Rad, y1 = b.Lat * Rad;
        double cy0 = Math.Cos(y0), sy0 = Math.Sin(y0), cy1 = Math.Cos(y1), sy1 = Math.Sin(y1);
        double kx0 = cy0 * Math.Cos(x0), ky0 = cy0 * Math.Sin(x0), kx1 = cy1 * Math.Cos(x1), ky1 = cy1 * Math.Sin(x1);
        static double Haversin(double x) => (x = Math.Sin(x / 2)) * x;
        var d = 2 * Math.Asin(Math.Sqrt(Math.Min(1, Haversin(y1 - y0) + cy0 * cy1 * Haversin(x1 - x0))));
        if (d < 1e-9) return [a, b];
        var k = Math.Sin(d);
        var n = Math.Max(2, (int)Math.Ceiling(d / Rad));
        var points = new List<GeoPos>(n + 1);
        for (var i = 0; i <= n; i++)
        {
            var t = d * i / n;
            var bb = Math.Sin(t) / k;
            var aa = Math.Sin(d - t) / k;
            var x = aa * kx0 + bb * kx1;
            var y = aa * ky0 + bb * ky1;
            var z = aa * sy0 + bb * sy1;
            points.Add(new GeoPos(Math.Atan2(y, x) / Rad, Math.Atan2(z, Math.Sqrt(x * x + y * y)) / Rad));
        }
        points[0] = a;
        points[^1] = b;
        return points;
    }

    static List<GeoPos> Resample(List<GeoPos> points, bool closed)
    {
        var result = new List<GeoPos>(points.Count);
        var count = closed ? points.Count : points.Count - 1;
        for (var i = 0; i < count; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % points.Count];
            result.Add(a);
            var span = Math.Max(Math.Abs(b.Lon - a.Lon), Math.Abs(b.Lat - a.Lat));
            if (span <= MaxStep) continue;
            var steps = (int)Math.Ceiling(span / MaxStep);
            for (var s = 1; s < steps; s++)
                result.Add(new GeoPos(a.Lon + (b.Lon - a.Lon) * s / steps, a.Lat + (b.Lat - a.Lat) * s / steps));
        }
        if (!closed && points.Count > 0) result.Add(points[^1]);
        return result;
    }

    /// <summary>Sutherland-Hodgman clipping of a closed ring to the half-plane east (or west) of the meridian <paramref name="edge"/>.</summary>
    internal static List<GeoPos> ClipX(List<GeoPos> ring, double edge, bool keepAbove)
    {
        var output = new List<GeoPos>();
        if (ring.Count == 0) return output;
        bool Inside(GeoPos p) => keepAbove ? p.Lon >= edge : p.Lon <= edge;
        GeoPos Cross(GeoPos a, GeoPos b)
        {
            var t = (edge - a.Lon) / (b.Lon - a.Lon);
            return new GeoPos(edge, a.Lat + (b.Lat - a.Lat) * t);
        }
        var prev = ring[^1];
        foreach (var cur in ring)
        {
            var curIn = Inside(cur);
            var prevIn = Inside(prev);
            if (curIn)
            {
                if (!prevIn) output.Add(Cross(prev, cur));
                output.Add(cur);
            }
            else if (prevIn) output.Add(Cross(prev, cur));
            prev = cur;
        }
        return output;
    }

    /// <summary>The signed area of a ring in its own units (shoelace).</summary>
    internal static double Area(IReadOnlyList<GeoPos> ring)
    {
        double a = 0;
        for (int i = 0, j = ring.Count - 1; i < ring.Count; j = i++) a += (ring[j].Lon * ring[i].Lat) - (ring[i].Lon * ring[j].Lat);
        return a / 2;
    }
}
