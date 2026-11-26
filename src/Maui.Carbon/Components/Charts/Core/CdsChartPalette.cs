namespace Maui.Carbon;

/// <summary>
/// The colors of a chart where it sits: theme tokens (with layer remapping), Carbon Charts' tokens, the categorical
/// pairing palettes and the sequential and diverging scales. Drawing code reads colors from here instead of binding them,
/// and redraws when the theme or layer changes.
/// </summary>
internal sealed class CdsChartPalette
{
    readonly Dictionary<string, Color> _keys = new(StringComparer.Ordinal);

    public CdsChartPalette(CdsThemeName theme, int level)
    {
        Theme = theme;
        Level = Math.Clamp(level, 1, 3);
    }

    public CdsThemeName Theme { get; }

    public int Level { get; }

    /// <summary>g90 and g100, which use Carbon Charts' dark palette.</summary>
    public bool IsDark => Theme is CdsThemeName.G90 or CdsThemeName.G100;

    /// <summary>A theme color key (<see cref="CdsColorKeys"/>); layer keys follow the layer level.</summary>
    public Color Key(string key)
    {
        if (_keys.TryGetValue(key, out var c)) return c;
        c = CdsPalettes.Resolve(Theme, Level, key);
        _keys[key] = c;
        return c;
    }

    /// <summary>A Carbon Charts token (grid background, zone fills, threshold red…).</summary>
    public Color Token(CdsChartToken token)
    {
        var row = Theme switch
        {
            CdsThemeName.G10 => CdsChartPaletteData.TokensG10,
            CdsThemeName.G90 => CdsChartPaletteData.TokensG90,
            CdsThemeName.G100 => CdsChartPaletteData.TokensG100,
            _ => CdsChartPaletteData.TokensWhite,
        };
        var v = row[(int)token];
        return v.Color ?? CdsPalettes.ResolveToken(Theme, Level, v.Token!);
    }

    /// <summary>
    /// The categorical color of series <paramref name="index"/> when a chart shows <paramref name="variants"/> series
    /// (Carbon's color pairing): one to five series use pairing option <paramref name="option"/> (falling back to 1 when the
    /// group has fewer options), six or more use <c>data-vis-01</c> to <c>14</c> in order.
    /// </summary>
    public Color Series(int variants, int option, int index, bool hovered)
    {
        var (colors, opt) = Pairing(variants, option);
        if (colors == 14)
        {
            var i = ((index % 14) + 14) % 14;
            return hovered
                ? (IsDark ? CdsChartPaletteData.Hovered14Dark : CdsChartPaletteData.Hovered14Light)[i]
                : Key(DataVisKeys[i]);
        }
        var table = IsDark
            ? (hovered ? CdsChartPaletteData.PairingDarkHovered : CdsChartPaletteData.PairingDark)
            : (hovered ? CdsChartPaletteData.PairingLightHovered : CdsChartPaletteData.PairingLight);
        var set = table[colors - 1][opt - 1];
        return set[((index % set.Length) + set.Length) % set.Length];
    }

    /// <summary>The palette size and option Carbon picks for a number of series and a requested option.</summary>
    public static (int Colors, int Option) Pairing(int variants, int option)
    {
        var colors = variants > 5 ? 14 : Math.Max(1, variants);
        var options = colors == 14 ? 1 : CdsChartPaletteData.PairingLight[colors - 1].Length;
        return (colors, option < 1 || option > options ? 1 : option);
    }

    /// <summary>Step <paramref name="step"/> (0 to 10, lowest value first) of sequential scale <paramref name="hue"/> (1 purple, 2 blue, 3 cyan, 4 teal).</summary>
    public Color Sequential(int hue, int step)
    {
        var scales = IsDark ? CdsChartPaletteData.SequentialDark : CdsChartPaletteData.SequentialLight;
        var s = scales[Math.Clamp(hue, 1, scales.Length) - 1];
        return s[Math.Clamp(step, 0, s.Length - 1)];
    }

    /// <summary>Step <paramref name="step"/> (0 to 16, 8 is zero) of diverging scale <paramref name="palette"/> (1 red to cyan, 2 purple to teal).</summary>
    public Color Diverging(int palette, int step)
    {
        var s = CdsChartPaletteData.Diverging[Math.Clamp(palette, 1, CdsChartPaletteData.Diverging.Length) - 1];
        return s[Math.Clamp(step, 0, s.Length - 1)];
    }

    /// <summary>The 14 categorical color keys, in Carbon's order.</summary>
    public static readonly string[] DataVisKeys =
    [
        CdsColorKeys.DataVis01, CdsColorKeys.DataVis02, CdsColorKeys.DataVis03, CdsColorKeys.DataVis04, CdsColorKeys.DataVis05,
        CdsColorKeys.DataVis06, CdsColorKeys.DataVis07, CdsColorKeys.DataVis08, CdsColorKeys.DataVis09, CdsColorKeys.DataVis10,
        CdsColorKeys.DataVis11, CdsColorKeys.DataVis12, CdsColorKeys.DataVis13, CdsColorKeys.DataVis14,
    ];
}
