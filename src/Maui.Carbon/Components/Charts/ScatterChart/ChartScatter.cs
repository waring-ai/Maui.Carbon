namespace Maui.Carbon;

/// <summary>Carbon's scatter helpers (components/graphs/scatter.ts): threshold anomalies.</summary>
internal static class ChartScatter
{
    /// <summary>
    /// Carbon's isDatapointThresholdAnomaly: a dot is an anomaly when it is at or beyond the highest threshold of the
    /// domain axis and of the range axis (whichever have thresholds). Returns a test on the dot's center in plot pixels,
    /// or <c>null</c> when neither axis has a threshold.
    /// </summary>
    public static Func<PointF, bool>? Anomaly(CartesianLayout cs)
    {
        var d = HighestThreshold(cs, cs.DomainPosition);
        var r = HighestThreshold(cs, cs.RangePosition);
        if (d is null && r is null) return null;
        var xT = cs.Vertical ? d : r;
        var yT = cs.Vertical ? r : d;
        return p =>
        {
            if (yT is { } y && xT is { } x) return p.Y <= y && p.X >= x;
            if (yT is { } y2) return p.Y <= y2;
            return p.X >= xT!.Value;
        };
    }

    /// <summary>The pixel of the highest threshold on an axis (Carbon's getHighestDomainThreshold / getHighestRangeThreshold).</summary>
    internal static double? HighestThreshold(CartesianLayout cs, AxisPosition pos)
    {
        if (!cs.Axes.TryGetValue(pos, out var a) || a.Thresholds.Count == 0) return null;
        var s = cs.Scales[pos];
        double px;
        if (s.Band is { } band)
        {
            var key = a.Thresholds.FirstOrDefault(t => t.Key is not null)?.Key;
            if (key is null) return null;
            px = band.Center(key);
        }
        else if (a.ScaleType == CdsChartScaleType.Time)
        {
            var dates = a.Thresholds.Where(t => t.Date.HasValue).Select(t => t.Date!.Value).ToList();
            if (dates.Count == 0) return null;
            px = s.Map(ChartTime.ToMs(dates.Max()));
        }
        else
        {
            var values = a.Thresholds.Where(t => t.Value.HasValue).Select(t => t.Value!.Value).ToList();
            if (values.Count == 0) return null;
            px = s.Map(values.Max());
        }
        return double.IsNaN(px) || double.IsInfinity(px) ? null : px;
    }

    /// <summary>"x, y, group" for a dot's screen-reader text.</summary>
    public static string Describe(ChartBuildContext ctx, CartesianLayout cs, CdsChartPoint d, string? extra = null)
    {
        var text = $"{cs.DomainText(d)}, {ctx.Value(d.Value)}";
        if (extra is not null) text += ", " + extra;
        if (d.Group is { Length: > 0 } g && g != cs.DomainText(d)) text += ", " + g;
        return text;
    }
}
