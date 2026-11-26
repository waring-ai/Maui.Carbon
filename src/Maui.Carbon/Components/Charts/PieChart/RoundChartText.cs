namespace Maui.Carbon;

/// <summary>The SVG <c>dominant-baseline</c> a Carbon chart text is positioned with.</summary>
internal enum SvgBaseline
{
    /// <summary>The point is on the alphabetic baseline (SVG's default).</summary>
    Alphabetic,
    /// <summary>The point is at the hanging baseline (the top of the capitals, roughly).</summary>
    Hanging,
    /// <summary>The point is half the x-height above the baseline.</summary>
    Middle,
    /// <summary>The point is the middle of the em box.</summary>
    Central,
}

/// <summary>
/// Text helpers of the round charts: places Carbon's SVG text (positioned by its baseline) as a scene label (positioned by
/// its line box), and truncates titles to a width as Carbon's meter title does.
/// </summary>
internal static class RoundChartText
{
    // IBM Plex: ascent 1.025 em, descent 0.275 em, x-height 0.516 em; browsers put the hanging baseline at 0.8 ascent.
    const float CenterToBaseline = 0.375f;
    const float Hanging = 0.82f;
    const float HalfXHeight = 0.258f;

    /// <summary>A label whose SVG text would be drawn at (<paramref name="x"/>, <paramref name="y"/>) with <paramref name="baseline"/>.</summary>
    public static ChartText At(string text, float x, float y, ChartFont font, ChartTextAnchor anchor = ChartTextAnchor.Start,
        SvgBaseline baseline = SvgBaseline.Alphabetic, ChartPaint? color = null, string? group = null)
    {
        var s = font.Size;
        var center = baseline switch
        {
            SvgBaseline.Alphabetic => y - CenterToBaseline * s,
            SvgBaseline.Hanging => y + (Hanging - CenterToBaseline) * s,
            SvgBaseline.Middle => y + (HalfXHeight - CenterToBaseline) * s,
            _ => y,
        };
        return new ChartText
        {
            Text = text,
            X = x,
            Y = center,
            Font = font,
            Anchor = anchor,
            Baseline = ChartTextBaseline.Middle,
            Color = color ?? ChartPaint.OfKey(CdsColorKeys.TextSecondary),
            Group = group,
        };
    }

    /// <summary>Carbon's title truncation: the longest start of <paramref name="text"/> that fits with "..." (the whole text when it fits).</summary>
    public static string Fit(string? text, float maxWidth, ChartFont font)
    {
        var s = text ?? "";
        if (maxWidth <= 0 || CdsChartTextMetrics.Width(s, font) <= maxWidth) return s;
        int lo = 0, hi = s.Length;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) / 2;
            if (CdsChartTextMetrics.Width(s[..mid] + "...", font) <= maxWidth) lo = mid;
            else hi = mid - 1;
        }
        return s[..lo] + "...";
    }
}
