using System.Globalization;
using System.Text.RegularExpressions;

namespace Maui.Carbon.Generator;

/// <summary>A 2D affine transform in SVG matrix order: x' = A·x + C·y + E, y' = B·x + D·y + F.</summary>
public readonly record struct Affine(double A, double B, double C, double D, double E, double F)
{
    /// <summary>The identity transform.</summary>
    public static Affine Identity { get; } = new(1, 0, 0, 1, 0, 0);

    /// <summary>Translation.</summary>
    public static Affine Translate(double tx, double ty) => new(1, 0, 0, 1, tx, ty);

    /// <summary>Scale.</summary>
    public static Affine Scale(double sx, double sy) => new(sx, 0, 0, sy, 0, 0);

    /// <summary>Rotation by <paramref name="degrees"/> about the origin.</summary>
    public static Affine Rotate(double degrees)
    {
        var (s, c) = Math.SinCos(degrees * Math.PI / 180);
        return new(c, s, -s, c, 0, 0);
    }

    /// <summary>Returns the transform that applies <paramref name="inner"/> first, then this one.</summary>
    public Affine Then(Affine inner) => new(
        A * inner.A + C * inner.B,
        B * inner.A + D * inner.B,
        A * inner.C + C * inner.D,
        B * inner.C + D * inner.D,
        A * inner.E + C * inner.F + E,
        B * inner.E + D * inner.F + F);

    /// <summary>Transforms a point.</summary>
    public (double X, double Y) Apply(double x, double y) => (A * x + C * y + E, B * x + D * y + F);

    /// <summary>Parses an SVG <c>transform</c> attribute (matrix, translate, scale, rotate, skewX, skewY; several allowed).</summary>
    public static Affine Parse(string? text)
    {
        var result = Identity;
        if (string.IsNullOrWhiteSpace(text)) return result;
        var matches = Regex.Matches(text, @"\s*([A-Za-z]+)\s*\(([^)]*)\)\s*,?");
        if (matches.Sum(m => m.Length) != text.Length)
            throw new FormatException($"Could not parse transform '{text}'.");
        foreach (Match m in matches)
        {
            var args = Regex.Split(m.Groups[2].Value.Trim(), @"[\s,]+")
                .Where(s => s.Length > 0)
                .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                .ToArray();
            var t = m.Groups[1].Value switch
            {
                "matrix" when args.Length == 6 => new Affine(args[0], args[1], args[2], args[3], args[4], args[5]),
                "translate" when args.Length is 1 or 2 => Translate(args[0], args.Length == 2 ? args[1] : 0),
                "scale" when args.Length is 1 or 2 => Scale(args[0], args.Length == 2 ? args[1] : args[0]),
                "rotate" when args.Length == 1 => Rotate(args[0]),
                "rotate" when args.Length == 3 => Translate(args[1], args[2]).Then(Rotate(args[0])).Then(Translate(-args[1], -args[2])),
                "skewX" when args.Length == 1 => new Affine(1, 0, Math.Tan(args[0] * Math.PI / 180), 1, 0, 0),
                "skewY" when args.Length == 1 => new Affine(1, Math.Tan(args[0] * Math.PI / 180), 0, 1, 0, 0),
                _ => throw new FormatException($"Unsupported transform '{m.Value.Trim()}'."),
            };
            result = result.Then(t);
        }
        return result;
    }
}
