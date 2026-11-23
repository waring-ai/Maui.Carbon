using System.Globalization;
using System.Text;

namespace Maui.Carbon.Generator;

/// <summary>One absolute path command: <c>M</c>/<c>L</c> (1 point), <c>C</c> (3 points) or <c>Z</c> (none).</summary>
public readonly record struct PathSegment(char Op, double[] Coords);

/// <summary>A path made only of absolute M, L, C and Z commands.</summary>
public sealed class PathData
{
    readonly List<PathSegment> _segments = [];

    /// <summary>The commands in order.</summary>
    public IReadOnlyList<PathSegment> Segments => _segments;

    /// <summary>Starts a subpath.</summary>
    public PathData MoveTo(double x, double y) => Add('M', x, y);

    /// <summary>Straight line.</summary>
    public PathData LineTo(double x, double y) => Add('L', x, y);

    /// <summary>Cubic Bézier.</summary>
    public PathData CubicTo(double x1, double y1, double x2, double y2, double x, double y) => Add('C', x1, y1, x2, y2, x, y);

    /// <summary>Closes the subpath.</summary>
    public PathData Close() => Add('Z');

    PathData Add(char op, params double[] coords)
    {
        _segments.Add(new PathSegment(op, coords));
        return this;
    }

    /// <summary>True when there is nothing to draw (no line or curve).</summary>
    public bool IsEmpty => _segments.All(s => s.Op is 'Z' or 'M');

    /// <summary>Every point, including Bézier control points.</summary>
    public IEnumerable<(double X, double Y)> Points()
    {
        foreach (var s in _segments)
            for (var i = 0; i + 1 < s.Coords.Length; i += 2)
                yield return (s.Coords[i], s.Coords[i + 1]);
    }

    /// <summary>Bounding box of all points (control points included), or null when empty.</summary>
    public (double MinX, double MinY, double MaxX, double MaxY)? Bounds()
    {
        var any = false;
        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
        foreach (var (x, y) in Points())
        {
            any = true;
            minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
        }
        return any ? (minX, minY, maxX, maxY) : null;
    }

    /// <summary>Returns a copy with every point transformed.</summary>
    public PathData Transform(Affine m)
    {
        var result = new PathData();
        foreach (var s in _segments)
        {
            var c = new double[s.Coords.Length];
            for (var i = 0; i + 1 < c.Length; i += 2)
                (c[i], c[i + 1]) = m.Apply(s.Coords[i], s.Coords[i + 1]);
            result._segments.Add(new PathSegment(s.Op, c));
        }
        return result;
    }

    /// <summary>Formats a number: invariant culture, at most 3 decimals, no trailing zeros, no exponent.</summary>
    /// <remarks>
    /// Rounds the shortest round-trip decimal form of the value, not the double itself. The SVG sources are written
    /// in decimal, and a midpoint such as 11.6665 has no exact double: the nearest one sits just below it. .NET 10's
    /// Math.Round and (decimal) conversion were inexact enough to round it up (11.667); .NET 11 made both exact, so
    /// they round it down (11.666). The round-trip string is "11.6665" on every runtime, so this answers the same.
    /// </remarks>
    public static string FormatNumber(double v)
    {
        var written = decimal.Parse(v.ToString("R", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture);
        var r = (double)Math.Round(written, 3, MidpointRounding.AwayFromZero);
        if (r == 0) r = 0; // drops negative zero
        return r.ToString("0.###", CultureInfo.InvariantCulture);
    }

    /// <summary>Serializes as <c>M 1 2 L 3 4 ... Z</c>.</summary>
    public string ToDataString()
    {
        var sb = new StringBuilder();
        foreach (var s in _segments)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(s.Op);
            foreach (var v in s.Coords) sb.Append(' ').Append(FormatNumber(v));
        }
        return sb.ToString();
    }

    /// <inheritdoc />
    public override string ToString() => ToDataString();
}
