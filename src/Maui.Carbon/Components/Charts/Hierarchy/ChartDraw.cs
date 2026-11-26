namespace Maui.Carbon;

/// <summary>Geometry and text helpers shared by the hierarchy, flow, word and diagram charts.</summary>
internal static class ChartDraw
{
    /// <summary>
    /// The y of a <see cref="ChartTextBaseline.Middle"/> label whose alphabetic baseline is at <paramref name="baseline"/>
    /// (SVG text positions are baselines). IBM Plex's line box is 1.025em above and 0.275em below the baseline, so the
    /// middle of the line sits 0.375em above it.
    /// </summary>
    public static float MiddleForBaseline(float baseline, ChartFont font) => baseline - 0.375f * font.Size;

    /// <summary>The cubic of d3 <c>linkHorizontal</c> (curveBumpX): control points halfway across at each end's height.</summary>
    public static PathF HorizontalLink(PointF s, PointF t)
    {
        var path = new PathF();
        var mx = (s.X + t.X) / 2;
        path.MoveTo(s);
        path.CurveTo(new PointF(mx, s.Y), new PointF(mx, t.Y), t);
        return path;
    }

    /// <summary>Points along a cubic Bezier, ends included.</summary>
    public static List<PointF> SampleCubic(PointF p0, PointF p1, PointF p2, PointF p3, int segments = 24)
    {
        var list = new List<PointF>(segments + 1);
        for (var i = 0; i <= segments; i++)
        {
            var t = (float)i / segments;
            var u = 1 - t;
            var a = u * u * u;
            var b = 3 * u * u * t;
            var c = 3 * u * t * t;
            var d = t * t * t;
            list.Add(new PointF(a * p0.X + b * p1.X + c * p2.X + d * p3.X, a * p0.Y + b * p1.Y + c * p2.Y + d * p3.Y));
        }
        return list;
    }

    /// <summary>
    /// The outline of a polyline stroked <paramref name="width"/> wide (mitred joins, butt caps): a hit area for edges and
    /// flows. Empty for fewer than two distinct points.
    /// </summary>
    public static PointF[] StrokePolygon(IReadOnlyList<PointF> points, float width)
    {
        var pts = new List<PointF>();
        foreach (var p in points)
            if (pts.Count == 0 || Distance(pts[^1], p) > 1e-3f) pts.Add(p);
        if (pts.Count < 2) return [];
        var h = width / 2;
        var left = new PointF[pts.Count];
        var right = new PointF[pts.Count];
        for (var i = 0; i < pts.Count; i++)
        {
            var n0 = i > 0 ? Normal(pts[i - 1], pts[i]) : Normal(pts[i], pts[i + 1]);
            var n1 = i < pts.Count - 1 ? Normal(pts[i], pts[i + 1]) : n0;
            var nx = n0.X + n1.X;
            var ny = n0.Y + n1.Y;
            var len = MathF.Sqrt(nx * nx + ny * ny);
            if (len < 1e-4f)
            {
                nx = n1.X;
                ny = n1.Y;
                len = 1;
            }
            nx /= len;
            ny /= len;
            // Miter length: h / cos(half the turn), capped so sharp turns stay bounded.
            var cos = nx * n1.X + ny * n1.Y;
            var m = h / Math.Max(0.35f, cos);
            left[i] = new PointF(pts[i].X + nx * m, pts[i].Y + ny * m);
            right[i] = new PointF(pts[i].X - nx * m, pts[i].Y - ny * m);
        }
        var poly = new List<PointF>(pts.Count * 2);
        poly.AddRange(left);
        for (var i = right.Length - 1; i >= 0; i--) poly.Add(right[i]);
        return poly.ToArray();
    }

    /// <summary>The bounding box of points.</summary>
    public static RectF Bounds(IReadOnlyList<PointF> points)
    {
        if (points.Count == 0) return RectF.Zero;
        float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
        foreach (var p in points)
        {
            x0 = Math.Min(x0, p.X);
            y0 = Math.Min(y0, p.Y);
            x1 = Math.Max(x1, p.X);
            y1 = Math.Max(y1, p.Y);
        }
        return new RectF(x0, y0, x1 - x0, y1 - y0);
    }

    static PointF Normal(PointF a, PointF b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var l = MathF.Sqrt(dx * dx + dy * dy);
        return l < 1e-6f ? new PointF(0, -1) : new PointF(-dy / l, dx / l);
    }

    static float Distance(PointF a, PointF b) => MathF.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    /// <summary>WCAG relative luminance of a color.</summary>
    public static double Luminance(Color c)
    {
        static double Lin(float v) => v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        return 0.2126 * Lin(c.Red) + 0.7152 * Lin(c.Green) + 0.0722 * Lin(c.Blue);
    }

    /// <summary>
    /// Carbon's treemap <c>textFillColor</c>: white text on fills whose Carbon shade is above 50, black otherwise. Carbon's
    /// palette gives every hue the same luminance per grade (about 0.27 at 50, 0.16 at 60), so a luminance below 0.21 is
    /// "darker than grade 50" for every Carbon color; any other color falls back to Carbon's lightness rule (below 50%).
    /// </summary>
    public static bool NeedsLightText(Color fill, bool isCarbonColor = true)
    {
        if (isCarbonColor) return Luminance(fill) < 0.21;
        var max = Math.Max(fill.Red, Math.Max(fill.Green, fill.Blue));
        var min = Math.Min(fill.Red, Math.Min(fill.Green, fill.Blue));
        return (max + min) / 2 < 0.5;
    }

    /// <summary>The black and white text of data labels on colored fills (Carbon's literal black and white).</summary>
    public static ChartPaint ContrastText(ChartColors colors, Color fill) => NeedsLightText(fill)
        ? ChartPaint.OfKey(CdsColorKeys.TextOnColor)
        : ChartPaint.OfToken(colors.Palette.IsDark ? CdsChartToken.Layer01Absolute : CdsChartToken.LayerInverseAbsolute);

    /// <summary>A Carbon icon as a filled path, <paramref name="size"/> square with its top-left at <paramref name="origin"/>.</summary>
    public static List<ChartPath> Icon(CdsIconName icon, PointF origin, float size, ChartPaint fill, int mark)
    {
        var glyph = CdsIcons.GetGlyph(icon, size);
        var vb = glyph.ViewBox;
        var k = size / Math.Max(1, vb.Width);
        var m = System.Numerics.Matrix3x2.CreateTranslation(-vb.X, -vb.Y) * System.Numerics.Matrix3x2.CreateScale(k) * System.Numerics.Matrix3x2.CreateTranslation(origin.X, origin.Y);
        var list = new List<ChartPath>();
        foreach (var p in glyph.Paths)
        {
            var path = PathBuilder.Build(p.Data);
            path.Transform(m);
            list.Add(new ChartPath { Path = path, Fill = fill, Winding = p.EvenOdd ? WindingMode.EvenOdd : WindingMode.NonZero, Mark = mark, Hover = ChartHoverStyle.None });
        }
        return list;
    }
}

/// <summary>Seeded randomness for layouts that use it, so a chart is the same on every render (mulberry32, FNV-1a).</summary>
internal static class ChartRandom
{
    /// <summary>A mulberry32 generator: numbers in [0, 1).</summary>
    public static Func<double> Seeded(uint seed)
    {
        var a = seed;
        return () =>
        {
            unchecked
            {
                a += 0x6d2b79f5;
                var t = a;
                t = (t ^ (t >> 15)) * (t | 1);
                t ^= t + (t ^ (t >> 7)) * (t | 61);
                return (t ^ (t >> 14)) / 4294967296.0;
            }
        };
    }

    /// <summary>FNV-1a hash of a string (UTF-16 code units).</summary>
    public static uint Hash(string s)
    {
        unchecked
        {
            var x = 2166136261u;
            foreach (var ch in s)
            {
                x ^= ch;
                x *= 16777619u;
            }
            return x;
        }
    }
}
