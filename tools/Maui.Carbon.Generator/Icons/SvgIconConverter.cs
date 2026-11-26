using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Maui.Carbon.Generator;

/// <summary>A converted path.</summary>
public readonly record struct GlyphPathData(string Data, bool EvenOdd);

/// <summary>One converted SVG file.</summary>
public sealed record GlyphData(
    string Stem,
    int ArtboardSize,
    float ViewBoxX,
    float ViewBoxY,
    float ViewBoxWidth,
    float ViewBoxHeight,
    IReadOnlyList<GlyphPathData> Paths,
    IReadOnlyList<GlyphPathData> InnerPaths);

/// <summary>Converts a Carbon SVG into normalized glyph path data.</summary>
public static partial class SvgIconConverter
{
    const double CropTolerance = 0.01;

    static readonly HashSet<string> Skipped = ["defs", "clipPath", "mask", "title", "desc", "style", "metadata", "symbol", "pattern", "linearGradient", "radialGradient"];

    /// <summary>Splits <c>arrow--right-16</c> into its stem and size.</summary>
    public static (string Stem, int Size) ParseFileName(string fileNameOrPath)
    {
        var name = Path.GetFileNameWithoutExtension(fileNameOrPath);
        var m = FileNameRegex().Match(name);
        if (!m.Success) throw new FormatException($"'{name}' does not look like <stem>-<size>.");
        return (m.Groups[1].Value, int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    /// <summary>Converts an SVG file.</summary>
    public static GlyphData ConvertFile(string path) => Convert(File.ReadAllText(path), Path.GetFileName(path));

    /// <summary>Converts SVG text; <paramref name="fileName"/> (<c>stem-size.svg</c>) supplies the name and artboard size.</summary>
    public static GlyphData Convert(string svg, string fileName)
    {
        var (stem, size) = ParseFileName(fileName);
        try
        {
            var root = XDocument.Parse(svg).Root ?? throw new FormatException("No root element.");
            if (root.Name.LocalName != "svg") throw new FormatException("Root element is not <svg>.");

            var artboard = ReadArtboard(root);
            if (artboard != size)
                throw new FormatException($"Artboard is {artboard} but the file name says {size}.");

            var classes = ReadClassRules(root);
            var paths = new List<(PathData Path, bool EvenOdd)>();
            var inner = new List<(PathData Path, bool EvenOdd)>();
            Visit(root, Affine.Identity, new Dictionary<string, string>(), classes, size, paths, inner);

            if (paths.Count == 0) throw new FormatException("No drawable paths left after dropping transparent shapes.");

            var (vx, vy, vs) = ViewBox(stem, size, paths.Select(p => p.Path));
            return new GlyphData(stem, size, vx, vy, vs, vs,
                paths.Select(p => new GlyphPathData(p.Path.ToDataString(), p.EvenOdd)).ToList(),
                inner.Select(p => new GlyphPathData(p.Path.ToDataString(), p.EvenOdd)).ToList());
        }
        catch (Exception e) when (e is FormatException or System.Xml.XmlException)
        {
            throw new FormatException($"{fileName}: {e.Message}", e);
        }
    }

    // Carbon's 16px versions of these are the 32px artwork at half scale (close: an 8px cross; checkbox: a 12px box), so
    // they keep the full 32 box (design/carbon/components/Icon/README.md; the reference bundle.js crops neither).
    static readonly HashSet<string> FullBox = ["close", "checkbox", "checkbox--checked", "checkbox--checked--filled", "checkbox--indeterminate"];

    // Carbon's 16px add is a 10px cross: drawn from the centre 26x26 (the reference bundle.js CROP 'add-32'). subtract is
    // its pair in steppers and gets the same box, so the two bars match.
    static readonly HashSet<string> Centre26 = ["add", "subtract"];

    /// <summary>
    /// The region of the artboard a glyph is drawn from. 32px artboards: <c>close</c> and the checkbox glyphs keep the full
    /// box, <c>add</c> and <c>subtract</c> use the centre 26x26, and any other artwork that fits the centre 24x24 is cropped
    /// to it; the rest keep the full box so nothing is clipped. Smaller artboards are never cropped.
    /// </summary>
    static (float X, float Y, float Size) ViewBox(string stem, int size, IEnumerable<PathData> paths)
    {
        if (size != 32 || FullBox.Contains(stem)) return (0, 0, size);
        if (Centre26.Contains(stem)) return (3, 3, 26);
        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
        foreach (var p in paths)
        {
            if (p.Bounds() is not { } b) continue;
            minX = Math.Min(minX, b.MinX); minY = Math.Min(minY, b.MinY);
            maxX = Math.Max(maxX, b.MaxX); maxY = Math.Max(maxY, b.MaxY);
        }
        var fits = minX >= 4 - CropTolerance && minY >= 4 - CropTolerance
                   && maxX <= 28 + CropTolerance && maxY <= 28 + CropTolerance;
        return fits ? (4, 4, 24) : (0, 0, 32);
    }

    static int ReadArtboard(XElement svg)
    {
        double w, h;
        if (svg.Attribute("viewBox") is { } vb)
        {
            var parts = vb.Value.Split([' ', ',', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            if (parts.Length != 4) throw new FormatException($"Bad viewBox '{vb.Value}'.");
            if (parts[0] != 0 || parts[1] != 0) throw new FormatException($"viewBox '{vb.Value}' does not start at the origin.");
            (w, h) = (parts[2], parts[3]);
        }
        else
        {
            w = Length(svg.Attribute("width")?.Value) ?? throw new FormatException("No viewBox or width.");
            h = Length(svg.Attribute("height")?.Value) ?? w;
        }
        if (w != h || w != Math.Floor(w)) throw new FormatException($"Artboard {w}x{h} is not a square integer size.");
        return (int)w;
    }

    static double? Length(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        s = s.Trim();
        if (s.EndsWith("px", StringComparison.Ordinal)) s = s[..^2];
        return double.Parse(s, CultureInfo.InvariantCulture);
    }

    static Dictionary<string, Dictionary<string, string>> ReadClassRules(XElement root)
    {
        var rules = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        foreach (var style in root.Descendants().Where(e => e.Name.LocalName == "style"))
        {
            var css = Regex.Replace(style.Value, @"/\*.*?\*/", "", RegexOptions.Singleline);
            foreach (Match m in RuleRegex().Matches(css))
            {
                var decls = ParseDeclarations(m.Groups[2].Value);
                foreach (var sel in m.Groups[1].Value.Split(','))
                {
                    var s = sel.Trim();
                    if (!s.StartsWith('.') || s.IndexOfAny([' ', '.', '#', ':', '[', '>'], 1) >= 0) continue;
                    var cls = s[1..];
                    if (!rules.TryGetValue(cls, out var existing)) rules[cls] = existing = [];
                    foreach (var (k, v) in decls) existing[k] = v;
                }
            }
        }
        return rules;
    }

    static Dictionary<string, string> ParseDeclarations(string text)
    {
        var d = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var part in text.Split(';'))
        {
            var i = part.IndexOf(':');
            if (i <= 0) continue;
            d[part[..i].Trim()] = part[(i + 1)..].Trim();
        }
        return d;
    }

    static bool IsTransparentRectangle(XElement e) =>
        (e.Attribute("id")?.Value is { } id && (id.StartsWith("_Transparent_Rectangle", StringComparison.OrdinalIgnoreCase)
                                               || id.Contains("Transparent_Rectangle", StringComparison.OrdinalIgnoreCase)))
        || (e.Attribute("data-name")?.Value is { } dn && dn.Contains("Transparent Rectangle", StringComparison.OrdinalIgnoreCase));

    static void Visit(XElement e, Affine parent, Dictionary<string, string> inherited,
        Dictionary<string, Dictionary<string, string>> classes, int size,
        List<(PathData, bool)> paths, List<(PathData, bool)> inner)
    {
        var name = e.Name.LocalName;
        if (Skipped.Contains(name)) return;
        if (IsTransparentRectangle(e)) return;

        var m = parent.Then(Affine.Parse(e.Attribute("transform")?.Value));

        // Inherited presentation properties: attribute < class < style attribute.
        var props = new Dictionary<string, string>(inherited);
        foreach (var key in new[] { "fill", "fill-rule" })
            if (e.Attribute(key) is { } a) props[key] = a.Value.Trim();
        if (e.Attribute("class") is { } cls)
            foreach (var c in cls.Value.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries))
                if (classes.TryGetValue(c, out var rule))
                    foreach (var key in new[] { "fill", "fill-rule" })
                        if (rule.TryGetValue(key, out var v)) props[key] = v;
        if (e.Attribute("style") is { } st)
        {
            var decls = ParseDeclarations(st.Value);
            foreach (var key in new[] { "fill", "fill-rule" })
                if (decls.TryGetValue(key, out var v)) props[key] = v;
        }

        if (name is "svg" or "g" or "a" or "switch")
        {
            foreach (var child in e.Elements()) Visit(child, m, props, classes, size, paths, inner);
            return;
        }

        var isInner = e.Attribute("data-icon-path")?.Value == "inner-path" || e.Attribute("id")?.Value == "inner-path";
        if (!isInner && props.TryGetValue("fill", out var fill) && fill.Equals("none", StringComparison.OrdinalIgnoreCase))
            return;

        PathData? geometry = name switch
        {
            "path" => SvgPathParser.Parse(e.Attribute("d")?.Value ?? ""),
            "polygon" => SvgShapes.Poly(e.Attribute("points")?.Value ?? "", close: true),
            "polyline" => SvgShapes.Poly(e.Attribute("points")?.Value ?? "", close: false),
            "circle" => Num(e, "r") is var r && r > 0 ? SvgShapes.Ellipse(Num(e, "cx"), Num(e, "cy"), r, r) : null,
            "ellipse" => Num(e, "rx") is var rx && Num(e, "ry") is var ry && rx > 0 && ry > 0
                ? SvgShapes.Ellipse(Num(e, "cx"), Num(e, "cy"), rx, ry) : null,
            "rect" => RectGeometry(e, size, isInner),
            _ => null, // line and anything else: Carbon fills, it never strokes
        };
        if (geometry is null || geometry.IsEmpty) return;

        var evenOdd = props.TryGetValue("fill-rule", out var fr) && fr.Equals("evenodd", StringComparison.OrdinalIgnoreCase);
        (isInner ? inner : paths).Add((geometry.Transform(m), evenOdd));
    }

    static PathData? RectGeometry(XElement e, int size, bool isInner)
    {
        var x = Num(e, "x"); var y = Num(e, "y"); var w = Num(e, "width"); var h = Num(e, "height");
        if (w <= 0 || h <= 0) return null;
        if (!isInner && Math.Abs(x) < 1e-9 && Math.Abs(y) < 1e-9 && Math.Abs(w - size) < 1e-9 && Math.Abs(h - size) < 1e-9)
            return null; // full-artboard rectangle: Carbon's invisible bounding box
        var rx = Num(e, "rx"); var ry = Num(e, "ry");
        return SvgShapes.Rect(x, y, w, h, rx, ry);
    }

    static double Num(XElement e, string attr) => Length(e.Attribute(attr)?.Value) ?? 0;

    [GeneratedRegex(@"^(.+)-(\d+)$")]
    private static partial Regex FileNameRegex();

    [GeneratedRegex(@"([^{}]+)\{([^}]*)\}")]
    private static partial Regex RuleRegex();
}
