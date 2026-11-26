namespace Maui.Carbon;

/// <summary>Carbon's radar geometry (radar.ts, services/angle-utils.ts, configuration-non-customizable.ts <c>radar</c>).</summary>
internal static class RadarLayout
{
    public const float FillOpacity = 0.3f;
    public const float FadedOpacity = 0.1f;
    public const float XLabelPadding = 10;
    public const float YLabelPadding = 8;
    public const int YTicks = 4;
    public const float MinRange = 10;
    public const float AxisRectHeight = 50;
    public const float DotRadius = 5;
    public const float LabelHeight = 16;

    /// <summary>
    /// angle-utils radialLabelPlacement: the text anchor and SVG baseline of a label at <paramref name="angle"/> (radians,
    /// 0 at 3 o'clock, clockwise) so it sits outside the circle.
    /// </summary>
    public static (ChartTextAnchor Anchor, SvgBaseline Baseline) LabelPlacement(double angle)
    {
        var d = ((angle * 180 / Math.PI) % 360 + 360) % 360;
        static bool In(double x, double a, double b) => x >= a && x <= b;
        if (In(d, 0, 10)) return (ChartTextAnchor.Start, SvgBaseline.Middle);
        if (In(d, 10, 80)) return (ChartTextAnchor.Start, SvgBaseline.Hanging);
        if (In(d, 80, 100)) return (ChartTextAnchor.Middle, SvgBaseline.Hanging);
        if (In(d, 100, 170)) return (ChartTextAnchor.End, SvgBaseline.Hanging);
        if (In(d, 170, 190)) return (ChartTextAnchor.End, SvgBaseline.Middle);
        if (In(d, 190, 260)) return (ChartTextAnchor.End, SvgBaseline.Alphabetic);
        if (In(d, 260, 280)) return (ChartTextAnchor.Middle, SvgBaseline.Alphabetic);
        return (ChartTextAnchor.Start, SvgBaseline.Alphabetic);
    }

    /// <summary>A point at <paramref name="radius"/> and <paramref name="angle"/> (radians, 0 at 3 o'clock, clockwise) from <paramref name="center"/>.</summary>
    public static PointF Polar(double angle, double radius, PointF center) =>
        new((float)(radius * Math.Cos(angle) + center.X), (float)(radius * Math.Sin(angle) + center.Y));

    /// <summary>The angle of spoke <paramref name="index"/> of <paramref name="count"/>: the first at 12 o'clock, then clockwise.</summary>
    public static double Angle(int index, int count) => -Math.PI / 2 + index * (2 * Math.PI / Math.Max(1, count));

    /// <summary>
    /// The value scale: from 0 (or the minimum when negative) to the maximum (raised to <paramref name="maxValue"/> when
    /// that is larger), made nice for about 4 ticks; it maps to <see cref="MinRange"/> to the radius.
    /// </summary>
    public static (double Min, double Max, double[] Ticks) Scale(IReadOnlyList<double> values, double? maxValue)
    {
        var min = values.Count > 0 ? values.Min() : 0;
        var max = values.Count > 0 ? values.Max() : 0;
        if (maxValue is { } m && m >= max) max = m;
        var start = min >= 0 ? 0 : min;
        var (a, b) = ChartTicks.Nice(start, max, YTicks);
        return (a, b, ChartTicks.Ticks(a, b, YTicks));
    }

    public static double Map(double value, double min, double max, double radius) =>
        max == min ? MinRange : MinRange + (value - min) / (max - min) * (radius - MinRange);

    /// <summary>The hit area of a spoke: Carbon's 50px-high rectangle from the center to the outer ring, rotated to the spoke.</summary>
    public static PointF[] SpokeArea(PointF center, double angle, double length)
    {
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        var h = AxisRectHeight / 2;
        PointF P(double x, double y) => new((float)(center.X + x * cos - y * sin), (float)(center.Y + x * sin + y * cos));
        return [P(0, -h), P(length, -h), P(length, h), P(0, h)];
    }
}
