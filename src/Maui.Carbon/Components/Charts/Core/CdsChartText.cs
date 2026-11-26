namespace Maui.Carbon;

/// <summary>The type families charts use: IBM Plex Sans for titles, Plex Sans Condensed for ticks, legends and labels.</summary>
internal enum ChartFontFamily
{
    Condensed,
    CondensedSemiBold,
    Sans,
    SansSemiBold,
    /// <summary>IBM Plex Sans Light (300): the gauge value.</summary>
    SansLight,
    /// <summary>IBM Plex Sans Condensed Light (300): word cloud words larger than 32px.</summary>
    CondensedLight,
}

/// <summary>A chart font: family and size in device-independent units.</summary>
internal readonly record struct ChartFont(ChartFontFamily Family, float Size)
{
    /// <summary>Ticks, legend items, tooltip rows and data labels: 12px Condensed.</summary>
    public static readonly ChartFont Tick = new(ChartFontFamily.Condensed, 12);

    /// <summary>Emphasized time ticks (first tick, a new day, month or year).</summary>
    public static readonly ChartFont TickPrimary = new(ChartFontFamily.CondensedSemiBold, 12);

    /// <summary>Axis titles: 12px Plex Sans SemiBold.</summary>
    public static readonly ChartFont AxisTitle = new(ChartFontFamily.SansSemiBold, 12);

    /// <summary>12px Plex Sans (meter labels, captions).</summary>
    public static readonly ChartFont Sans12 = new(ChartFontFamily.Sans, 12);

    /// <summary>14px Plex Sans (meter titles and totals).</summary>
    public static readonly ChartFont Sans14 = new(ChartFontFamily.Sans, 14);

    /// <summary>The registered font alias of the family.</summary>
    public string Alias => Family switch
    {
        ChartFontFamily.CondensedSemiBold => CdsFonts.SansCondensedSemiBold,
        ChartFontFamily.Sans => CdsFonts.Sans,
        ChartFontFamily.SansSemiBold => CdsFonts.SansSemiBold,
        ChartFontFamily.SansLight => CdsFonts.SansLight,
        ChartFontFamily.CondensedLight => CdsFonts.SansCondensedLight,
        _ => CdsFonts.SansCondensed,
    };

    /// <summary>The line height used for layout: 16px at 12px (Carbon's 1rem), else 4/3 of the size.</summary>
    public float LineHeight => Size == 12 ? 16 : MathF.Ceiling(Size * 4 / 3);
}

/// <summary>
/// Measures chart text from the advance widths of the embedded IBM Plex fonts, so layout (margins, rotation, label
/// fitting) is the same on every platform and in tests. Latin text measures exactly (without kerning, which only
/// narrows it); other scripts use an estimate.
/// </summary>
internal static partial class CdsChartTextMetrics
{
    /// <summary>The width of <paramref name="text"/> set in <paramref name="font"/>.</summary>
    public static float Width(string? text, ChartFont font)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        var (table, fallback) = font.Family switch
        {
            ChartFontFamily.CondensedSemiBold => (CondensedSemiBold, CondensedSemiBoldFallback),
            ChartFontFamily.Sans => (SansRegular, SansRegularFallback),
            ChartFontFamily.SansSemiBold => (SansSemiBold, SansSemiBoldFallback),
            ChartFontFamily.SansLight => (SansLight, SansLightFallback),
            ChartFontFamily.CondensedLight => (CondensedLight, CondensedLightFallback),
            _ => (CondensedRegular, CondensedRegularFallback),
        };
        var units = 0;
        foreach (var ch in text)
        {
            var i = ch - 0x20;
            if (i >= 0 && i < table.Length && table[i] > 0) units += table[i];
            else if (ch >= 0x2E80) units += 1000;
            else if (!char.IsControl(ch)) units += fallback;
        }
        return units * font.Size / 1000f;
    }

    /// <summary>The widest of <paramref name="texts"/>.</summary>
    public static float MaxWidth(IEnumerable<string> texts, ChartFont font)
    {
        var max = 0f;
        foreach (var t in texts) max = Math.Max(max, Width(t, font));
        return max;
    }
}

/// <summary>Carbon's label truncation (<c>truncation</c>: type, threshold and number of characters).</summary>
internal static class CdsChartTruncate
{
    /// <summary>Cuts <paramref name="text"/> when it is longer than <paramref name="threshold"/> characters.</summary>
    public static string Apply(string? text, CdsChartTruncation type = CdsChartTruncation.End, int threshold = 16, int count = 14)
    {
        var s = text ?? "";
        if (type == CdsChartTruncation.None || s.Length <= threshold || count <= 0) return s;
        count = Math.Min(count, s.Length);
        return type switch
        {
            CdsChartTruncation.Front => "..." + s[^count..],
            CdsChartTruncation.Middle => s[..(count / 2)] + "..." + s[^(count / 2)..],
            _ => s[..count] + "...",
        };
    }
}
