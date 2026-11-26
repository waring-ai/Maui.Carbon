namespace Maui.Carbon;

/// <summary>Bar geometry shared by the bar charts (Carbon's bar.ts).</summary>
internal static class ChartBars
{
    /// <summary>Carbon's getBarWidth: the set width, else min(max width, half the band step), or a share of the axis for continuous domains.</summary>
    public static double Width(CartesianLayout cs, double? width, double maxWidth, int count, double spacingFactor = 0.25)
    {
        if (width is { } w) return w;
        if (cs.IsBand) return Math.Min(maxWidth, cs.Step / 2);
        var len = cs.Vertical ? cs.Plot.Width : cs.Plot.Height;
        return Math.Min(maxWidth, len * spacingFactor / Math.Max(1, count));
    }

    /// <summary>A bar from domain pixels x0..x1 and range pixels y0..y1 (flipped for horizontal charts).</summary>
    public static RectF Rect(double x0, double x1, double y0, double y1, bool vertical)
    {
        var (a0, a1) = (Math.Min(x0, x1), Math.Max(x0, x1));
        var (b0, b1) = (Math.Min(y0, y1), Math.Max(y0, y1));
        return vertical
            ? new RectF((float)a0, (float)b0, (float)(a1 - a0), (float)(b1 - b0))
            : new RectF((float)b0, (float)a0, (float)(b1 - b0), (float)(a1 - a0));
    }

    /// <summary>Carbon keeps a non-zero bar at least 2px long.</summary>
    public static double MinLength(double y0, double y1, double value, bool vertical)
    {
        var diff = Math.Abs(y1 - y0);
        if (diff != 0 && diff < 2) return (value > 0 && vertical) || (value < 0 && !vertical) ? y0 - 2 : y0 + 2;
        return y1;
    }
}

/// <summary>Carbon's scatter dots (scatter, line, area, lollipop and bubble charts).</summary>
internal static class ChartDots
{
    /// <summary>
    /// A dot: filled dots fill with the series color at <paramref name="fillOpacity"/>, unfilled ones with <c>layer-01</c>;
    /// both stroke 1.5px in the series color. While its mark is active the dot fills solid.
    /// </summary>
    public static ChartCircle Dot(PointF center, string? group, float radius, bool filled, float fillOpacity, int mark)
    {
        return new ChartCircle
        {
            Center = center,
            Radius = radius,
            Group = group,
            Mark = mark,
            Fill = filled ? ChartPaint.OfSeries(group) : ChartPaint.OfKey(CdsColorKeys.Layer01),
            FillOpacity = filled ? fillOpacity : 1,
            HoverFill = ChartPaint.OfSeries(group),
            HoverFillOpacity = 1,
            Hover = ChartHoverStyle.None,
            Stroke = ChartPaint.OfSeries(group),
            StrokeWidth = 1.5f,
        };
    }
}
