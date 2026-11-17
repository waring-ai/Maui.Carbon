using System.Globalization;

namespace Maui.Carbon.Generator;

/// <summary>Parses SVG path data (full grammar) into absolute M/L/C/Z <see cref="PathData"/>.</summary>
public static class SvgPathParser
{
    /// <summary>Parses and normalizes <paramref name="d"/>.</summary>
    public static PathData Parse(string d)
    {
        var r = new Reader(d);
        var path = new PathData();
        double cx = 0, cy = 0, sx = 0, sy = 0;   // current point, subpath start
        double lcx = 0, lcy = 0;                 // last control point (cubic or quad)
        var prev = ' ';
        var cmd = ' ';
        var needMove = false;                    // true after Z until the next drawing command

        void EnsureStart()
        {
            if (!needMove) return;
            path.MoveTo(cx, cy);
            sx = cx; sy = cy;
            needMove = false;
        }

        while (true)
        {
            r.SkipSeparators();
            if (r.End) break;
            if (char.IsLetter(r.Peek))
            {
                cmd = r.Next();
                if (prev == ' ' && cmd is not ('M' or 'm'))
                    throw new FormatException("Path data must start with a moveto.");
            }
            else if (cmd == ' ' || cmd is 'Z' or 'z')
            {
                throw new FormatException($"Unexpected number at {r.Pos} in path data '{d}'.");
            }

            var rel = char.IsLower(cmd);
            var ox = rel ? cx : 0;
            var oy = rel ? cy : 0;
            var up = char.ToUpperInvariant(cmd);
            switch (up)
            {
                case 'M':
                {
                    var x = r.Number() + ox; var y = r.Number() + oy;
                    path.MoveTo(x, y);
                    cx = sx = x; cy = sy = y;
                    needMove = false;
                    cmd = rel ? 'l' : 'L'; // implicit repeats are linetos
                    break;
                }
                case 'L':
                {
                    EnsureStart();
                    var x = r.Number() + ox; var y = r.Number() + oy;
                    path.LineTo(x, y);
                    cx = x; cy = y;
                    break;
                }
                case 'H':
                {
                    EnsureStart();
                    var x = r.Number() + ox;
                    path.LineTo(x, cy);
                    cx = x;
                    break;
                }
                case 'V':
                {
                    EnsureStart();
                    var y = r.Number() + oy;
                    path.LineTo(cx, y);
                    cy = y;
                    break;
                }
                case 'C':
                {
                    EnsureStart();
                    var x1 = r.Number() + ox; var y1 = r.Number() + oy;
                    var x2 = r.Number() + ox; var y2 = r.Number() + oy;
                    var x = r.Number() + ox; var y = r.Number() + oy;
                    path.CubicTo(x1, y1, x2, y2, x, y);
                    lcx = x2; lcy = y2; cx = x; cy = y;
                    break;
                }
                case 'S':
                {
                    EnsureStart();
                    var (x1, y1) = prev is 'C' or 'S' ? (2 * cx - lcx, 2 * cy - lcy) : (cx, cy);
                    var x2 = r.Number() + ox; var y2 = r.Number() + oy;
                    var x = r.Number() + ox; var y = r.Number() + oy;
                    path.CubicTo(x1, y1, x2, y2, x, y);
                    lcx = x2; lcy = y2; cx = x; cy = y;
                    break;
                }
                case 'Q':
                {
                    EnsureStart();
                    var qx = r.Number() + ox; var qy = r.Number() + oy;
                    var x = r.Number() + ox; var y = r.Number() + oy;
                    Quad(path, cx, cy, qx, qy, x, y);
                    lcx = qx; lcy = qy; cx = x; cy = y;
                    break;
                }
                case 'T':
                {
                    EnsureStart();
                    var (qx, qy) = prev is 'Q' or 'T' ? (2 * cx - lcx, 2 * cy - lcy) : (cx, cy);
                    var x = r.Number() + ox; var y = r.Number() + oy;
                    Quad(path, cx, cy, qx, qy, x, y);
                    lcx = qx; lcy = qy; cx = x; cy = y;
                    break;
                }
                case 'A':
                {
                    EnsureStart();
                    var rx = r.Number(); var ry = r.Number(); var rot = r.Number();
                    var large = r.Flag(); var sweep = r.Flag();
                    var x = r.Number() + ox; var y = r.Number() + oy;
                    ArcTo(path, cx, cy, rx, ry, rot, large, sweep, x, y);
                    cx = x; cy = y;
                    break;
                }
                case 'Z':
                {
                    if (!needMove && path.Segments.Count > 0)
                    {
                        path.Close();
                        cx = sx; cy = sy;
                        needMove = true;
                    }
                    break;
                }
                default:
                    throw new FormatException($"Unknown path command '{cmd}'.");
            }
            prev = up;
        }
        return path;
    }

    static void Quad(PathData p, double x0, double y0, double qx, double qy, double x, double y) =>
        p.CubicTo(x0 + 2.0 / 3 * (qx - x0), y0 + 2.0 / 3 * (qy - y0),
                  x + 2.0 / 3 * (qx - x), y + 2.0 / 3 * (qy - y), x, y);

    /// <summary>Appends an SVG elliptical arc as cubic Béziers (≤ 90° each), or a line for degenerate radii.</summary>
    public static void ArcTo(PathData p, double x0, double y0, double rx, double ry, double rotationDegrees,
        bool largeArc, bool sweep, double x1, double y1)
    {
        if (x0 == x1 && y0 == y1) return;
        rx = Math.Abs(rx); ry = Math.Abs(ry);
        if (rx == 0 || ry == 0) { p.LineTo(x1, y1); return; }

        var phi = rotationDegrees * Math.PI / 180;
        var (sinPhi, cosPhi) = Math.SinCos(phi);
        var dx = (x0 - x1) / 2; var dy = (y0 - y1) / 2;
        var x1p = cosPhi * dx + sinPhi * dy;
        var y1p = -sinPhi * dx + cosPhi * dy;

        var lambda = x1p * x1p / (rx * rx) + y1p * y1p / (ry * ry);
        if (lambda > 1) { var s = Math.Sqrt(lambda); rx *= s; ry *= s; }

        var rx2 = rx * rx; var ry2 = ry * ry;
        var num = rx2 * ry2 - rx2 * y1p * y1p - ry2 * x1p * x1p;
        var den = rx2 * y1p * y1p + ry2 * x1p * x1p;
        var coef = (largeArc == sweep ? -1 : 1) * Math.Sqrt(Math.Max(0, num / den));
        var cxp = coef * rx * y1p / ry;
        var cyp = -coef * ry * x1p / rx;
        var cx = cosPhi * cxp - sinPhi * cyp + (x0 + x1) / 2;
        var cy = sinPhi * cxp + cosPhi * cyp + (y0 + y1) / 2;

        var theta1 = Math.Atan2((y1p - cyp) / ry, (x1p - cxp) / rx);
        var theta2 = Math.Atan2((-y1p - cyp) / ry, (-x1p - cxp) / rx);
        var delta = theta2 - theta1;
        if (sweep && delta < 0) delta += 2 * Math.PI;
        else if (!sweep && delta > 0) delta -= 2 * Math.PI;

        var n = Math.Max(1, (int)Math.Ceiling(Math.Abs(delta) / (Math.PI / 2) - 1e-9));
        var step = delta / n;
        var t = 4.0 / 3 * Math.Tan(step / 4);

        (double, double) Map(double u, double v) =>
            (cx + rx * cosPhi * u - ry * sinPhi * v, cy + rx * sinPhi * u + ry * cosPhi * v);

        for (var i = 0; i < n; i++)
        {
            var a1 = theta1 + i * step;
            var a2 = a1 + step;
            var (s1, c1) = Math.SinCos(a1);
            var (s2, c2) = Math.SinCos(a2);
            var (ax, ay) = Map(c1 - t * s1, s1 + t * c1);
            var (bx, by) = Map(c2 + t * s2, s2 - t * c2);
            var (ex, ey) = i == n - 1 ? (x1, y1) : Map(c2, s2);
            p.CubicTo(ax, ay, bx, by, ex, ey);
        }
    }

    sealed class Reader(string s)
    {
        public int Pos;
        public bool End => Pos >= s.Length;
        public char Peek => s[Pos];
        public char Next() => s[Pos++];

        public void SkipSeparators()
        {
            while (Pos < s.Length && (char.IsWhiteSpace(s[Pos]) || s[Pos] == ',')) Pos++;
        }

        public bool Flag()
        {
            SkipSeparators();
            if (End || (s[Pos] != '0' && s[Pos] != '1'))
                throw new FormatException($"Expected arc flag at {Pos} in '{s}'.");
            return s[Pos++] == '1';
        }

        public double Number()
        {
            SkipSeparators();
            var start = Pos;
            if (Pos < s.Length && (s[Pos] == '+' || s[Pos] == '-')) Pos++;
            var digits = 0;
            while (Pos < s.Length && char.IsAsciiDigit(s[Pos])) { Pos++; digits++; }
            if (Pos < s.Length && s[Pos] == '.')
            {
                Pos++;
                while (Pos < s.Length && char.IsAsciiDigit(s[Pos])) { Pos++; digits++; }
            }
            if (digits == 0) throw new FormatException($"Expected a number at {start} in '{s}'.");
            if (Pos < s.Length && (s[Pos] == 'e' || s[Pos] == 'E'))
            {
                var save = Pos++;
                if (Pos < s.Length && (s[Pos] == '+' || s[Pos] == '-')) Pos++;
                var expDigits = 0;
                while (Pos < s.Length && char.IsAsciiDigit(s[Pos])) { Pos++; expDigits++; }
                if (expDigits == 0) Pos = save;
            }
            return double.Parse(s.AsSpan(start, Pos - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }
}
