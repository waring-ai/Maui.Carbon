namespace Maui.Carbon;

/// <summary>
/// Carbon's grouped bars (bar-grouped.ts): within each category, one bar per group on an inner band, bars 5px apart
/// (scaled down as categories get denser). Shared by the grouped bar and combo charts.
/// </summary>
internal static class ChartGroupedBars
{
    /// <summary>Pixels between the bars of a group (Carbon's padding).</summary>
    public const double Padding = 5;

    /// <summary>The category step the padding is normalized to (Carbon's defaultStepFactor).</summary>
    public const double StepFactor = 70;

    /// <summary>The geometry of a set of grouped bars: bar width, groups, the category step and each category's inner band.</summary>
    internal sealed class Geometry
    {
        readonly Dictionary<string, (double Width, BandScale Scale)> _byKey = new(StringComparer.Ordinal);
        readonly (double Width, BandScale Scale) _full;
        readonly bool _preserve;
        readonly Dictionary<string, HashSet<string>> _present;

        internal Geometry(double barWidth, IReadOnlyList<string> groups, double step, bool preserve, Dictionary<string, HashSet<string>> present)
        {
            BarWidth = barWidth;
            Groups = groups;
            Step = step;
            _preserve = preserve;
            _present = present;
            _full = Band(groups);
        }

        public double BarWidth { get; }

        public IReadOnlyList<string> Groups { get; }

        public double Step { get; }

        /// <summary>The inner band of a category: its total width and the band scale of its groups.</summary>
        public (double Width, BandScale Scale) ForKey(string key)
        {
            if (_preserve) return _full;
            if (_byKey.TryGetValue(key, out var hit)) return hit;
            var avail = _present.TryGetValue(key, out var set) ? Groups.Where(set.Contains).ToList() : [];
            return _byKey[key] = Band(avail);
        }

        (double Width, BandScale Scale) Band(IReadOnlyList<string> groups)
        {
            var w = BarWidth * groups.Count + Pad(Step, groups.Count);
            var scale = new BandScale(groups);
            scale.SetRange(0, w);
            return (w, scale);
        }
    }

    /// <summary>The space between the bars of a group of <paramref name="count"/>: min(5, 5 x step / 70) per gap.</summary>
    public static double Pad(double step, int count) => count <= 1 ? 0 : Math.Min(Padding, Padding * (step / StepFactor)) * (count - 1);

    /// <summary>
    /// Carbon's grouped geometry: the step is the band step (or the distance between a group's first two points on a time
    /// or linear axis); bars are <paramref name="width"/> when it fits <paramref name="maxWidth"/>, else
    /// min(maxWidth, (step - padding) / groups).
    /// </summary>
    public static Geometry Measure(ChartBuildContext ctx, CartesianLayout cs, IReadOnlyList<CdsChartPoint> rows, double? width, double maxWidth, bool preserveSpace)
    {
        var present = new HashSet<string>(rows.Select(r => r.Group ?? ""), StringComparer.Ordinal);
        var groups = ctx.ActiveGroups.Where(present.Contains).ToList();
        var n = Math.Max(1, groups.Count);
        var step = StepFactor;
        if (cs.IsBand) step = cs.Step;
        else
        {
            foreach (var g in groups)
            {
                var list = rows.Where(r => (r.Group ?? "") == g).Take(2).ToList();
                if (list.Count < 2) continue;
                var d = Math.Abs(cs.DomainPx(list[1]) - cs.DomainPx(list[0]));
                step = d > 0 && double.IsFinite(d) ? d : StepFactor;
                break;
            }
        }
        var bw = width is { } w && w <= maxWidth ? w : Math.Min(maxWidth, (step - Pad(step, n)) / n);
        var byKey = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var r in rows)
        {
            var k = cs.DomainKey(r);
            if (!byKey.TryGetValue(k, out var set)) byKey[k] = set = new HashSet<string>(StringComparer.Ordinal);
            set.Add(r.Group ?? "");
        }
        return new Geometry(bw, groups, step, preserveSpace, byKey);
    }

    /// <summary>
    /// Adds a bar and a mark per point with a value, in reading order (category by category, groups in order). Bars start
    /// at the baseline (zero, or the axis minimum above zero) and darken on hover. Returns the number of bars added; their
    /// mark ids run from <paramref name="firstId"/>.
    /// </summary>
    public static int Add(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs, IReadOnlyList<CdsChartPoint> rows, double? width, double maxWidth, bool preserveSpace, int firstId = 0)
    {
        var geo = Measure(ctx, cs, rows, width, maxWidth, preserveSpace);
        var order = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < geo.Groups.Count; i++) order[geo.Groups[i]] = i;
        var bars = new List<(CdsChartPoint P, double Center, int GroupIndex, RectF Rect)>();
        foreach (var d in rows)
        {
            if (d.Value is not { } value || double.IsNaN(value)) continue;
            var g = d.Group ?? "";
            if (!order.TryGetValue(g, out var gi)) continue;
            var center = cs.DomainPx(d);
            if (double.IsNaN(center)) continue;
            var (groupWidth, inner) = geo.ForKey(cs.DomainKey(d));
            var start = inner.Map(g);
            if (double.IsNaN(start)) continue;
            var x0 = center - groupWidth / 2 + start;
            var x1 = x0 + geo.BarWidth;
            var y0 = cs.Baseline(d.Group);
            var y1 = cs.RangePx(d.Group, value);
            bars.Add((d, center, gi, ChartBars.Rect(x0, x1, y0, y1, cs.Vertical)));
        }
        var id = firstId;
        foreach (var b in bars.OrderBy(b => b.Center).ThenBy(b => b.GroupIndex))
        {
            var d = b.P;
            var mark = id++;
            scene.Shapes.Add(new ChartRect { Rect = b.Rect, Fill = ChartPaint.OfSeries(d.Group), Group = d.Group, Mark = mark });
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = b.Rect,
                Group = d.Group,
                Rows = CartesianFrame.TooltipRows(ctx, cs, [d]),
                Description = $"{cs.DomainText(d)}, {d.Group}, {ctx.Value(d.Value)}",
            });
        }
        return id - firstId;
    }
}
