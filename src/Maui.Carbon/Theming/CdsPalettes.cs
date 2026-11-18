namespace Maui.Carbon;

/// <summary>Builds theme and layer resource dictionaries from the generated palette data.</summary>
internal static class CdsPalettes
{
    /// <summary>Token bases re-mapped by <see cref="CdsLayer"/> (from Carbon's <c>_layer-sets.scss</c>).</summary>
    internal static readonly string[] LayerSetBases =
    [
        "layer", "layer-hover", "layer-active", "layer-selected", "layer-selected-hover",
        "layer-accent", "layer-accent-hover", "layer-accent-active", "layer-background",
        "field", "field-hover", "border-subtle", "border-subtle-selected", "border-strong", "border-tile",
    ];

    static readonly Dictionary<string, int> IndexByToken = CdsColorKeys.TokenNames
        .Select((name, i) => (name, i))
        .ToDictionary(x => x.name, x => x.i, StringComparer.Ordinal);

    static readonly Dictionary<string, int> IndexByKey = CdsColorKeys.All
        .Select((key, i) => (key, i))
        .ToDictionary(x => x.key, x => x.i, StringComparer.Ordinal);

    /// <summary>The value of a color key in a theme (ignores layering).</summary>
    internal static Color GetColor(CdsThemeName theme, string key) => CdsPaletteData.Get(theme)[IndexByKey[key]];

    /// <summary>The value of a token name (<c>layer-02</c>) in a theme.</summary>
    internal static Color GetTokenColor(CdsThemeName theme, string token) => CdsPaletteData.Get(theme)[IndexByToken[token]];

    /// <summary>True when <paramref name="key"/> is a Carbon color key.</summary>
    internal static bool IsColorKey(string key) => IndexByKey.ContainsKey(key);

    /// <summary>A new dictionary holding every color of <paramref name="theme"/> at layer level one, plus the context keys.</summary>
    internal static ResourceDictionary CreateThemeDictionary(CdsThemeName theme)
    {
        var rd = new ResourceDictionary();
        FillTheme(rd, theme);
        return rd;
    }

    internal static void FillTheme(ResourceDictionary rd, CdsThemeName theme)
    {
        var colors = CdsPaletteData.Get(theme);
        var keys = CdsColorKeys.All;
        for (var i = 0; i < keys.Count; i++) rd.Add(keys[i], colors[i]);
        rd.Add(CdsContextKeys.ThemeId, theme);
        rd.Add(CdsContextKeys.LayerLevel, 1);
        rd.Add(CdsContextKeys.Transparent, Colors.Transparent);
    }

    /// <summary>
    /// A new dictionary that re-maps the <c>-01</c> layer keys to <paramref name="level"/>'s values in
    /// <paramref name="theme"/>, plus <see cref="CdsContextKeys.LayerLevel"/>.
    /// </summary>
    internal static ResourceDictionary CreateLayerDictionary(CdsThemeName theme, int level)
    {
        level = Math.Clamp(level, 1, 3);
        var rd = new ResourceDictionary();
        FillLayer(rd, theme, level);
        rd.Add(CdsContextKeys.LayerLevel, level);
        return rd;
    }

    internal static void FillLayer(ResourceDictionary rd, CdsThemeName theme, int level)
    {
        foreach (var b in LayerSetBases)
            rd[CdsColorKeys.All[IndexByToken[$"{b}-01"]]] = GetTokenColor(theme, $"{b}-0{level}");
    }

    /// <summary>A dictionary for an overlay that mirrors an anchor's theme and layer level.</summary>
    internal static ResourceDictionary CreateContextDictionary(CdsThemeName theme, int level)
    {
        var rd = new ResourceDictionary();
        FillTheme(rd, theme);
        FillLayer(rd, theme, Math.Clamp(level, 1, 3));
        rd[CdsContextKeys.LayerLevel] = Math.Clamp(level, 1, 3);
        return rd;
    }
}
