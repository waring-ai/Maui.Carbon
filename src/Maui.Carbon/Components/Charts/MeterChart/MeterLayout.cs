namespace Maui.Carbon;

/// <summary>Carbon's meter model and fixed sizes (model/meter.ts, configuration-non-customizable.ts <c>meter</c>, title-meter.ts).</summary>
internal static class MeterLayout
{
    public const float TitleRowSingle = 21;
    public const float TitleRowProportional = 16;
    public const float TitleSpacer = 8;
    public const float BarHeightSingle = 8;
    public const float BarHeightProportional = 16;
    public const float DividerWidth = 2;
    public const float PercentPaddingLeft = 5;
    public const float StatusIndicatorSize = 16;
    public const float StatusPaddingLeft = 15;
    public const float TotalPaddingLeft = 36;
    public const float TotalPaddingRight = 24;

    /// <summary>The first status range holding the total (a single meter's total is capped at 100), or null.</summary>
    public static CdsChartStatus? Status(double total, bool proportional, IEnumerable<CdsMeterStatusRange>? ranges)
    {
        if (ranges is null) return null;
        var bounded = proportional ? total : Math.Min(total, 100);
        foreach (var r in ranges)
            if (r is not null && r.Contains(bounded)) return r.Status;
        return null;
    }

    /// <summary>The domain maximum: 100 for a single meter; the total (or the sum of the values) for a proportional one.</summary>
    public static double Max(bool proportional, double? total, double sum) =>
        !proportional ? 100 : total is { } t && t != 0 ? t : sum;

    /// <summary>Meter.getStackedBounds: each segment 2px (the divider) narrower than its value's width, after the previous ones.</summary>
    public static List<(float X, float Width)> Stacked(IReadOnlyList<double> values, double max, float width)
    {
        var list = new List<(float, float)>(values.Count);
        var prev = 0f;
        for (var i = 0; i < values.Count; i++)
        {
            var w = max != 0 ? (float)(values[i] / max * width) : 0;
            var x = i == 0 ? 0 : prev;
            prev = i == 0 ? w : prev + w;
            list.Add((x, Math.Abs(w - DividerWidth)));
        }
        return list;
    }

    /// <summary>The height of the meter graph: the title row and 8px when labels show, then the bar.</summary>
    public static float GraphHeight(bool proportional, bool labels, double? barHeight) =>
        (labels ? (proportional ? TitleRowProportional : TitleRowSingle) + TitleSpacer : 0) + BarHeight(proportional, barHeight);

    public static float BarHeight(bool proportional, double? barHeight) =>
        barHeight is { } h && h > 0 ? (float)h : proportional ? BarHeightProportional : BarHeightSingle;

    /// <summary>The status icon (Carbon's status indicator): a 16px circle with a check, a slash or "!", its top left at <paramref name="origin"/>.</summary>
    public static PathF StatusGlyph(CdsChartStatus status, PointF origin)
    {
        var path = new PathF();
        PointF P(float x, float y) => new(origin.X + x, origin.Y + y);
        switch (status)
        {
            case CdsChartStatus.Success:
                path.MoveTo(P(6.875f, 11.3125f));
                path.LineTo(P(3.75f, 8.1875f));
                path.LineTo(P(4.74375f, 7.25f));
                path.LineTo(P(6.875f, 9.34375f));
                path.LineTo(P(11.50625f, 4.75f));
                path.LineTo(P(12.5f, 5.7375f));
                path.Close();
                break;
            case CdsChartStatus.Danger:
                path.MoveTo(P(10.7f, 11.5f));
                path.LineTo(P(4.5f, 5.3f));
                path.LineTo(P(5.3f, 4.5f));
                path.LineTo(P(11.5f, 10.7f));
                path.Close();
                break;
            default:
                path.AppendCircle(origin.X + 7.9375f, origin.Y + 12.0625f, 0.9375f);
                path.AppendRectangle(origin.X + 7.3125f, origin.Y + 3, 1.25f, 6.875f);
                break;
        }
        return path;
    }
}
