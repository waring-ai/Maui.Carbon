namespace Maui.Carbon;

/// <summary>
/// Carbon's stacked area data (model.ts getStackedData as used by area-stacked.ts): one series per active group over every
/// domain position, missing values stacking as 0, optionally as percentages of each position's total.
/// </summary>
internal static class StackedAreaModel
{
    /// <summary>One stacked value: the domain key, a point at that position (for its pixel), the source point (or <c>null</c> when the group has no value there), and the band's bottom and top.</summary>
    internal sealed record Row(string Key, CdsChartPoint At, CdsChartPoint? Source, double Y0, double Y1);

    /// <summary>One group's band, in domain order.</summary>
    internal sealed record Series(string Group, IReadOnlyList<Row> Rows)
    {
        /// <summary>Carbon draws no line for a group that adds nothing anywhere.</summary>
        public bool HasLine => Rows.Any(r => r.Y1 != r.Y0);
    }

    /// <summary>
    /// Stacks <paramref name="rows"/> (already limited to drawable domain positions) for <paramref name="groups"/> in that
    /// order from the bottom. Positions are ordered by <paramref name="orderOf"/> (the domain pixel); the first point of a
    /// group at a position counts. Percentage stacking divides by the position's total (0 when the total is 0).
    /// </summary>
    public static IReadOnlyList<Series> Stack(IReadOnlyList<CdsChartPoint> rows, IReadOnlyList<string> groups, Func<CdsChartPoint, string> keyOf, Func<CdsChartPoint, double> orderOf, bool percentage)
    {
        var keys = new List<(string Key, CdsChartPoint At, double Order)>();
        var byKey = new Dictionary<string, Dictionary<string, CdsChartPoint>>(StringComparer.Ordinal);
        foreach (var p in rows)
        {
            var k = keyOf(p);
            if (!byKey.TryGetValue(k, out var m))
            {
                byKey[k] = m = new Dictionary<string, CdsChartPoint>(StringComparer.Ordinal);
                keys.Add((k, p, orderOf(p)));
            }
            m.TryAdd(p.Group ?? "", p);
        }
        var ordered = keys.Select((k, i) => (k, i)).OrderBy(x => x.k.Order).ThenBy(x => x.i).Select(x => x.k).ToList();
        var series = groups.Select(g => (Group: g, Rows: new List<Row>())).ToList();
        foreach (var (key, at, _) in ordered)
        {
            var m = byKey[key];
            var values = groups.Select(g => m.TryGetValue(g, out var d) && d.Value is { } v && !double.IsNaN(v) ? v : 0).ToList();
            var total = values.Sum();
            double cum = 0;
            for (var i = 0; i < groups.Count; i++)
            {
                var v = percentage ? (total != 0 ? values[i] / total * 100 : 0) : values[i];
                m.TryGetValue(groups[i], out var src);
                series[i].Rows.Add(new Row(key, at, src?.Value is null ? null : src, cum, cum + v));
                cum += v;
            }
        }
        return series.Select(s => new Series(s.Group, s.Rows)).ToList();
    }
}
