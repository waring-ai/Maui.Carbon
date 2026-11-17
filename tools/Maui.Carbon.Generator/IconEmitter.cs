using System.Text;

namespace Maui.Carbon.Generator;

/// <summary>Converts design/carbon/icons/*.svg into CdsIconName / CdsIconData.</summary>
public static partial class IconEmitter
{
    /// <summary>True: the SVG converter is implemented.</summary>
    public static bool IsAvailable => true;

    /// <summary>Returns (file name, body) pairs.</summary>
    public static IEnumerable<(string Name, string Body)> Emit(string iconsDir)
    {
        var glyphs = Directory.GetFiles(iconsDir, "*.svg")
            .Order(StringComparer.Ordinal)
            .Select(SvgIconConverter.ConvertFile)
            .ToList();
        yield return ("CdsIcons.g.cs", Render(glyphs));
    }

    /// <summary>Renders the generated source body for the given glyphs.</summary>
    public static string Render(IReadOnlyList<GlyphData> glyphs)
    {
        var byStem = glyphs
            .GroupBy(g => g.Stem, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => (Stem: g.Key, Member: MemberName(g.Key), Glyphs: g.OrderBy(x => x.ArtboardSize).ToList()))
            .ToList();

        var collisions = byStem.GroupBy(s => s.Member).FirstOrDefault(g => g.Count() > 1);
        if (collisions is not null)
            throw new InvalidOperationException(
                $"Icon names {string.Join(", ", collisions.Select(c => c.Stem))} all map to enum member '{collisions.Key}'.");
        foreach (var s in byStem)
        {
            var dup = s.Glyphs.GroupBy(g => g.ArtboardSize).FirstOrDefault(g => g.Count() > 1);
            if (dup is not null) throw new InvalidOperationException($"Icon '{s.Stem}' has two {dup.Key}px sources.");
        }

        var sb = new StringBuilder();
        sb.Append("using Microsoft.Maui.Graphics;\n\n");
        sb.Append("namespace Maui.Carbon;\n\n");
        sb.Append("/// <summary>The Carbon icons bundled with Maui.Carbon. Draw them with <c>CdsIcon</c>.</summary>\n");
        sb.Append("public enum CdsIconName\n{\n");
        foreach (var s in byStem)
        {
            sb.Append($"    /// <summary><c>{s.Stem}</c> ({string.Join(", ", s.Glyphs.Select(g => g.ArtboardSize))}).</summary>\n");
            sb.Append($"    {s.Member},\n");
        }
        sb.Append("}\n\n");

        sb.Append("internal static partial class CdsIconData\n{\n");
        sb.Append("    /// <summary>Carbon stem for each icon.</summary>\n");
        sb.Append("    internal static string GetStem(CdsIconName name) => name switch\n    {\n");
        foreach (var s in byStem)
            sb.Append($"        CdsIconName.{s.Member} => \"{s.Stem}\",\n");
        sb.Append("        _ => throw new ArgumentOutOfRangeException(nameof(name)),\n    };\n\n");

        sb.Append("    /// <summary>Every source artboard of an icon, smallest first.</summary>\n");
        sb.Append("    internal static CdsGlyph[] GetGlyphs(CdsIconName name) => name switch\n    {\n");
        foreach (var s in byStem)
        {
            sb.Append($"        CdsIconName.{s.Member} => [");
            sb.Append(string.Join(", ", s.Glyphs.Select(Glyph)));
            sb.Append("],\n");
        }
        sb.Append("        _ => throw new ArgumentOutOfRangeException(nameof(name)),\n    };\n}\n");
        return sb.ToString();
    }

    static string Glyph(GlyphData g)
    {
        var vb = $"new RectF({F(g.ViewBoxX)}, {F(g.ViewBoxY)}, {F(g.ViewBoxWidth)}, {F(g.ViewBoxHeight)})";
        return $"new(\"{g.Stem}\", {g.ArtboardSize}, {vb}, [{PathList(g.Paths)}], [{PathList(g.InnerPaths)}])";
    }

    static string PathList(IReadOnlyList<GlyphPathData> paths) =>
        string.Join(", ", paths.Select(p => p.EvenOdd ? $"new(\"{p.Data}\", true)" : $"new(\"{p.Data}\")"));

    static string F(float v) => PathData.FormatNumber(v) + "f";

    /// <summary>PascalCase enum member for a stem: <c>arrow--right</c> becomes <c>ArrowRight</c>.</summary>
    public static string MemberName(string stem)
    {
        var name = string.Concat(stem.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
        if (name.Length == 0) throw new InvalidOperationException($"Icon stem '{stem}' has no name characters.");
        return char.IsAsciiDigit(name[0]) ? "_" + name : name;
    }
}
