using System.Globalization;

namespace Maui.Carbon.Generator;

/// <summary>Converts SVG basic shapes to <see cref="PathData"/>.</summary>
public static class SvgShapes
{
    /// <summary>Bézier circle-arc constant.</summary>
    public const double Kappa = 0.5522847498;

    /// <summary>Four cubic arcs.</summary>
    public static PathData Ellipse(double cx, double cy, double rx, double ry)
    {
        var kx = rx * Kappa; var ky = ry * Kappa;
        return new PathData()
            .MoveTo(cx + rx, cy)
            .CubicTo(cx + rx, cy + ky, cx + kx, cy + ry, cx, cy + ry)
            .CubicTo(cx - kx, cy + ry, cx - rx, cy + ky, cx - rx, cy)
            .CubicTo(cx - rx, cy - ky, cx - kx, cy - ry, cx, cy - ry)
            .CubicTo(cx + kx, cy - ry, cx + rx, cy - ky, cx + rx, cy)
            .Close();
    }

    /// <summary>Rectangle with optional rounded corners (radii clamped to half the size).</summary>
    public static PathData Rect(double x, double y, double w, double h, double rx = 0, double ry = 0)
    {
        if (rx > 0 && ry <= 0) ry = rx;
        if (ry > 0 && rx <= 0) rx = ry;
        rx = Math.Min(rx, w / 2); ry = Math.Min(ry, h / 2);
        var p = new PathData();
        if (rx <= 0 || ry <= 0)
            return p.MoveTo(x, y).LineTo(x + w, y).LineTo(x + w, y + h).LineTo(x, y + h).Close();

        var kx = rx * Kappa; var ky = ry * Kappa;
        var r = x + w; var b = y + h;
        return p.MoveTo(x + rx, y)
            .LineTo(r - rx, y)
            .CubicTo(r - rx + kx, y, r, y + ry - ky, r, y + ry)
            .LineTo(r, b - ry)
            .CubicTo(r, b - ry + ky, r - rx + kx, b, r - rx, b)
            .LineTo(x + rx, b)
            .CubicTo(x + rx - kx, b, x, b - ry + ky, x, b - ry)
            .LineTo(x, y + ry)
            .CubicTo(x, y + ry - ky, x + rx - kx, y, x + rx, y)
            .Close();
    }

    /// <summary>Polygon (closed) or polyline (open) from a <c>points</c> attribute.</summary>
    public static PathData Poly(string points, bool close)
    {
        var nums = points.Split([' ', ',', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        var p = new PathData();
        for (var i = 0; i + 1 < nums.Length; i += 2)
        {
            if (i == 0) p.MoveTo(nums[0], nums[1]); else p.LineTo(nums[i], nums[i + 1]);
        }
        if (close && nums.Length >= 2) p.Close();
        return p;
    }
}
