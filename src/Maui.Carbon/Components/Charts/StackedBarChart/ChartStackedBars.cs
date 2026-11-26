namespace Maui.Carbon;

/// <summary>
/// Carbon's stacked bars (bar-stacked.ts and ruler-stacked.ts): one bar per category with each group's segment stacked in
/// group order (positives up, negatives down), a divider gap between segments, and a category mark that draws the ruler
/// and lists every group. Shared by the stacked bar and combo charts.
/// </summary>
internal static class ChartStackedBars
{
    /// <summary>
    /// Carbon's divider: a segment longer than <paramref name="dividerSize"/> loses 1px at its far end (2px for a negative
    /// segment ending at zero), so the plot background shows between segments.
    /// </summary>
    public static double DividedEnd(double y0, double y1, double v0, double v1, bool vertical, double dividerSize, int groupCount)
    {
        var length = Math.Abs(y1 - y0);
        if (length > 0 && length > dividerSize)
        {
            var negative = v0 < 0 && v1 <= 0;
            if (negative && groupCount > 1) return vertical ? y1 + (v1 == 0 ? 2 : 1) : y1 - 1;
            if (!negative) return vertical ? y1 + 1 : y1 - 1;
        }
        return y1;
    }

    /// <summary>
    /// Half the hit width of a category (Carbon's ruler-stacked threshold): half the band step, else half the smallest gap
    /// between positions (at least 8), or 40 for a single position.
    /// </summary>
    public static double Threshold(CartesianLayout cs, IReadOnlyList<double> sortedPositions)
    {
        if (cs.IsBand) return cs.Step / 2;
        var step = double.MaxValue;
        for (var i = 1; i < sortedPositions.Count; i++) step = Math.Min(step, sortedPositions[i] - sortedPositions[i - 1]);
        return step == double.MaxValue ? 40 : Math.Max(step / 2, 8);
    }

    /// <summary>
    /// Adds the stacks in reading order: per category (along the domain) its category mark (when <paramref name="ruler"/>),
    /// then its segments bottom-up. Hovering a segment darkens it and shows its own tooltip; hovering the rest of the
    /// category draws the ruler, darkens the whole stack and lists every group there (largest first) with the total.
    /// Returns the number of stacks; mark ids run from <paramref name="firstId"/> (read the next free id from the scene).
    /// </summary>
    public static int Add(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs, IReadOnlyList<CdsChartPoint> rows, double? barWidth, double maxWidth,
        double dividerSize, bool ruler, int firstId = 0, IReadOnlyList<string>? groups = null)
    {
        var percentage = cs.Axes.TryGetValue(cs.RangePosition, out var rangeAxis) && rangeAxis.IsPercentage;
        var stacks = cs.Stack(rows, groups ?? ctx.ActiveGroups, percentage);
        var bw = ChartBars.Width(cs, barWidth, maxWidth, stacks.Count);
        var groupCount = ctx.ActiveGroups.Count;
        var scale = cs.RangeScale;
        var columns = new List<(string Key, double Px, Dictionary<string, (double Y0, double Y1, CdsChartPoint Point)> Column)>();
        foreach (var (key, column) in stacks)
        {
            var first = column.Values.Select(c => c.Point).FirstOrDefault();
            if (first is null) continue;
            var px = cs.DomainPx(first);
            if (double.IsNaN(px)) continue;
            columns.Add((key, px, column));
        }
        columns.Sort((a, b) => a.Px.CompareTo(b.Px));
        var threshold = Threshold(cs, columns.Select(c => c.Px).ToList());
        var p = cs.Plot;
        var id = firstId;
        foreach (var (key, px, column) in columns)
        {
            var colId = -1;
            if (ruler)
            {
                var valid = column.Values.Select(c => c.Point).Where(pt => pt.Value.HasValue).Reverse().ToList();
                if (valid.Count > 0)
                {
                    colId = id++;
                    var top = column.Values.Select(c => c.Y1).DefaultIfEmpty(0).Max();
                    var t = (float)threshold;
                    scene.Marks.Add(new ChartMark
                    {
                        Id = colId,
                        Bounds = cs.Vertical ? new RectF((float)px - t, p.Top, 2 * t, p.Height) : new RectF(p.Left, (float)px - t, p.Width, 2 * t),
                        RulerAt = (float)px,
                        Anchor = cs.XY(px, scale.Map(top)),
                        Rows = CartesianFrame.TooltipRows(ctx, cs, valid),
                        Description = cs.DomainText(valid[0]) + ", " + string.Join(", ", valid.AsEnumerable().Reverse().Select(v => $"{v.Group} {ctx.Value(v.Value)}"))
                            + (ctx.ShowTotal && valid.Count > 1 ? $", {ctx.TotalLabel} {ctx.Value(valid.Sum(v => v.Value ?? 0))}" : ""),
                    });
                }
            }
            var x0 = px - bw / 2;
            var x1 = x0 + bw;
            foreach (var (group, (v0, v1, point)) in column)
            {
                if (v0 == v1) continue;
                var y0 = scale.Map(v0);
                var y1 = DividedEnd(y0, scale.Map(v1), v0, v1, cs.Vertical, dividerSize, groupCount);
                var rect = ChartBars.Rect(x0, x1, y0, y1, cs.Vertical);
                var mark = id++;
                scene.Shapes.Add(new ChartRect { Rect = rect, Fill = ChartPaint.OfSeries(group), Group = group, Mark = mark });
                if (colId >= 0) scene.Shapes.Add(new ChartRect { Rect = rect, Fill = ChartPaint.OfSeries(group), Group = group, Mark = colId, HoverOnly = true });
                scene.Marks.Add(new ChartMark
                {
                    Id = mark,
                    Bounds = rect,
                    Group = group,
                    Rows = CartesianFrame.TooltipRows(ctx, cs, [point]),
                    Description = $"{cs.DomainText(point)}, {group}, {ctx.Value(point.Value)}",
                });
            }
        }
        return columns.Count;
    }
}
