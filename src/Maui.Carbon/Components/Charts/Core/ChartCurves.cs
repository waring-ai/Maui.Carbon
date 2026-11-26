namespace Maui.Carbon;

/// <summary>d3-shape's curves for lines and areas (Carbon's <c>curve</c> option).</summary>
internal static class ChartCurves
{
    /// <summary>Appends a curve through <paramref name="points"/> to <paramref name="path"/>; <paramref name="move"/> starts a new subpath.</summary>
    public static void Append(PathF path, IReadOnlyList<PointF> points, CdsChartCurve curve, bool move = true)
    {
        var n = points.Count;
        if (n == 0) return;
        if (move) path.MoveTo(points[0]);
        else path.LineTo(points[0]);
        if (n == 1) return;
        switch (curve)
        {
            case CdsChartCurve.Step: Step(path, points, 0.5f); break;
            case CdsChartCurve.StepAfter: Step(path, points, 1); break;
            case CdsChartCurve.StepBefore: Step(path, points, 0); break;
            case CdsChartCurve.MonotoneX: Monotone(path, points, false); break;
            case CdsChartCurve.MonotoneY: Monotone(path, points, true); break;
            case CdsChartCurve.Natural: Natural(path, points); break;
            case CdsChartCurve.Cardinal:
            case CdsChartCurve.CatmullRom: Cardinal(path, points, 0); break;
            case CdsChartCurve.Basis: Basis(path, points); break;
            default:
                for (var i = 1; i < n; i++) path.LineTo(points[i]);
                break;
        }
    }

    /// <summary>A path through the points, split into segments where <paramref name="points"/> has <c>null</c> gaps.</summary>
    public static PathF Line(IReadOnlyList<PointF?> points, CdsChartCurve curve)
    {
        var path = new PathF();
        foreach (var segment in Segments(points)) Append(path, segment, curve);
        return path;
    }

    /// <summary>The runs of consecutive defined points.</summary>
    public static List<List<PointF>> Segments(IReadOnlyList<PointF?> points)
    {
        var list = new List<List<PointF>>();
        List<PointF>? current = null;
        foreach (var p in points)
        {
            if (p is { } q && !float.IsNaN(q.X) && !float.IsNaN(q.Y))
            {
                current ??= [];
                current.Add(q);
            }
            else if (current is not null)
            {
                list.Add(current);
                current = null;
            }
        }
        if (current is not null) list.Add(current);
        return list;
    }

    static void Step(PathF path, IReadOnlyList<PointF> p, float t)
    {
        for (var i = 1; i < p.Count; i++)
        {
            var (x0, y0) = (p[i - 1].X, p[i - 1].Y);
            var (x, y) = (p[i].X, p[i].Y);
            if (t <= 0)
            {
                path.LineTo(x0, y);
                path.LineTo(x, y);
            }
            else
            {
                var x1 = x0 * (1 - t) + x * t;
                path.LineTo(x1, y0);
                path.LineTo(x1, y);
                if (t < 1) path.LineTo(x, y);
            }
        }
    }

    static void Monotone(PathF path, IReadOnlyList<PointF> src, bool swap)
    {
        var p = swap ? src.Select(q => new PointF(q.Y, q.X)).ToList() : src.ToList();
        var n = p.Count;
        if (n == 2)
        {
            path.LineTo(src[1]);
            return;
        }
        var t = new float[n];
        static float Sign(float x) => x < 0 ? -1 : 1;
        for (var k = 1; k < n - 1; k++)
        {
            var h0 = p[k].X - p[k - 1].X;
            var h1 = p[k + 1].X - p[k].X;
            var s0 = h0 != 0 ? (p[k].Y - p[k - 1].Y) / h0 : 0;
            var s1 = h1 != 0 ? (p[k + 1].Y - p[k].Y) / h1 : 0;
            var pp = h0 + h1 != 0 ? (s0 * h1 + s1 * h0) / (h0 + h1) : 0;
            var v = (Sign(s0) + Sign(s1)) * MathF.Min(MathF.Min(MathF.Abs(s0), MathF.Abs(s1)), 0.5f * MathF.Abs(pp));
            t[k] = float.IsNaN(v) ? 0 : v;
        }
        float Slope2(int a, int b, float tb)
        {
            var h = p[b].X - p[a].X;
            return h != 0 ? (3 * (p[b].Y - p[a].Y) / h - tb) / 2 : tb;
        }
        t[0] = Slope2(0, 1, t[1]);
        t[n - 1] = Slope2(n - 2, n - 1, t[n - 2]);
        for (var k = 0; k < n - 1; k++)
        {
            var dx = (p[k + 1].X - p[k].X) / 3;
            var c1 = new PointF(p[k].X + dx, p[k].Y + dx * t[k]);
            var c2 = new PointF(p[k + 1].X - dx, p[k + 1].Y - dx * t[k + 1]);
            if (swap)
            {
                c1 = new PointF(c1.Y, c1.X);
                c2 = new PointF(c2.Y, c2.X);
            }
            path.CurveTo(c1, c2, src[k + 1]);
        }
    }

    static void Natural(PathF path, IReadOnlyList<PointF> p)
    {
        var n = p.Count;
        if (n == 2)
        {
            path.LineTo(p[1]);
            return;
        }
        var (ax, bx) = Control(p.Select(q => q.X).ToArray());
        var (ay, by) = Control(p.Select(q => q.Y).ToArray());
        for (var i = 0; i < n - 1; i++) path.CurveTo(ax[i], ay[i], bx[i], by[i], p[i + 1].X, p[i + 1].Y);
    }

    static (float[] A, float[] B) Control(float[] x)
    {
        var n = x.Length - 1;
        var a = new float[n];
        var b = new float[n];
        var r = new float[n];
        a[0] = 0;
        b[0] = 2;
        r[0] = x[0] + 2 * x[1];
        for (var i = 1; i < n - 1; i++)
        {
            a[i] = 1;
            b[i] = 4;
            r[i] = 4 * x[i] + 2 * x[i + 1];
        }
        a[n - 1] = 2;
        b[n - 1] = 7;
        r[n - 1] = 8 * x[n - 1] + x[n];
        for (var i = 1; i < n; i++)
        {
            var m = a[i] / b[i - 1];
            b[i] -= m;
            r[i] -= m * r[i - 1];
        }
        a[n - 1] = r[n - 1] / b[n - 1];
        for (var i = n - 2; i >= 0; i--) a[i] = (r[i] - a[i + 1]) / b[i];
        b[n - 1] = (x[n] + a[n - 1]) / 2;
        for (var i = 0; i < n - 1; i++) b[i] = 2 * x[i + 1] - a[i + 1];
        return (a, b);
    }

    static void Cardinal(PathF path, IReadOnlyList<PointF> p, float tension)
    {
        var k = (1 - tension) / 6;
        var n = p.Count;
        for (var i = 0; i < n - 1; i++)
        {
            var p0 = p[Math.Max(0, i - 1)];
            var p1 = p[i];
            var p2 = p[i + 1];
            var p3 = p[Math.Min(n - 1, i + 2)];
            path.CurveTo(p1.X + k * (p2.X - p0.X), p1.Y + k * (p2.Y - p0.Y), p2.X - k * (p3.X - p1.X), p2.Y - k * (p3.Y - p1.Y), p2.X, p2.Y);
        }
    }

    static void Basis(PathF path, IReadOnlyList<PointF> p)
    {
        var n = p.Count;
        if (n == 2)
        {
            path.LineTo(p[1]);
            return;
        }
        path.LineTo((5 * p[0].X + p[1].X) / 6, (5 * p[0].Y + p[1].Y) / 6);
        for (var i = 2; i < n; i++) BasisPoint(path, p[i - 2], p[i - 1], p[i]);
        BasisPoint(path, p[n - 2], p[n - 1], p[n - 1]);
        path.LineTo(p[n - 1]);
    }

    static void BasisPoint(PathF path, PointF a, PointF b, PointF c) =>
        path.CurveTo((2 * a.X + b.X) / 3, (2 * a.Y + b.Y) / 3, (a.X + 2 * b.X) / 3, (a.Y + 2 * b.Y) / 3, (a.X + 4 * b.X + c.X) / 6, (a.Y + 4 * b.Y + c.Y) / 6);
}
