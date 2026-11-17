using System.Collections.Concurrent;

namespace Maui.Carbon;

/// <summary>Lookup for the bundled Carbon icons.</summary>
public static class CdsIcons
{
    static readonly Lazy<Dictionary<string, CdsIconName>> s_byStem = new(() =>
        Enum.GetValues<CdsIconName>().ToDictionary(CdsIconData.GetStem, n => n, StringComparer.Ordinal));

    static readonly ConcurrentDictionary<string, PathF> s_paths = new(StringComparer.Ordinal);

    /// <summary>The Carbon stem of an icon, e.g. <c>arrow--right</c>.</summary>
    public static string GetStem(CdsIconName icon) => CdsIconData.GetStem(icon);

    /// <summary>Finds an icon by its Carbon stem (<c>arrow--right</c>, <c>error--filled</c>).</summary>
    public static bool TryGetName(string stem, out CdsIconName icon) => s_byStem.Value.TryGetValue(stem, out icon);

    /// <summary>Every source artboard of an icon, smallest first.</summary>
    public static IReadOnlyList<CdsGlyph> GetGlyphs(CdsIconName icon) => CdsIconData.GetGlyphs(icon);

    /// <summary>
    /// The best source glyph to draw <paramref name="icon"/> at <paramref name="size"/>: an exact artboard match,
    /// else the smallest larger one, else the largest.
    /// </summary>
    public static CdsGlyph GetGlyph(CdsIconName icon, double size)
    {
        var glyphs = CdsIconData.GetGlyphs(icon);
        CdsGlyph? larger = null;
        foreach (var g in glyphs)
        {
            if (Math.Abs(g.ViewBox.Width - size) < 0.01) return g;
            if (g.ViewBox.Width > size && (larger is null || g.ViewBox.Width < larger.ViewBox.Width)) larger = g;
        }
        return larger ?? glyphs[^1];
    }

    /// <summary>Parsed (and cached) path geometry for path data.</summary>
    internal static PathF GetPath(string data) => s_paths.GetOrAdd(data, static d => PathBuilder.Build(d));
}
