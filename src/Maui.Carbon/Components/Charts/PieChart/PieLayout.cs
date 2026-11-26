namespace Maui.Carbon;

/// <summary>One slice of a pie: its row, its place in the angular order, and its angles (radians, 0 at 12 o'clock, clockwise, pad included).</summary>
internal readonly record struct PieArc(int Index, int Order, float Start, float End)
{
    public float Mid => (Start + End) / 2;

    public float Degrees => (End - Start) * 180 / MathF.PI;
}

/// <summary>A callout of a small slice's label: from the arc out and up to a bend, then horizontally to the label.</summary>
internal readonly record struct PieCallout(PointF Start, PointF Bend, PointF End);

/// <summary>A percentage label: the point is the text's alphabetic baseline, centered (relative to the pie center).</summary>
internal sealed record PieLabel(int Arc, string Text, PointF At, PieCallout? Callout);

/// <summary>Carbon's pie model and layout (model/pie.ts, d3 <c>pie()</c>, Pie.render label transforms and callouts).</summary>
internal static class PieLayout
{
    public const float PadAngle = 0.007f;
    public const float RadiusOffset = -15;
    public const float PieInnerRadius = 2;
    public const float HoverOffset = 3;
    public const float XOffset = 30;
    public const float YOffset = 20;
    public const float YOffsetCallout = 10;
    public const float LabelDistance = 7;
    public const float CalloutMinSliceDegree = 5;
    public const float CalloutOffsetX = 15;
    public const float CalloutOffsetY = 12;
    public const float CalloutLineLength = 8;
    public const float CalloutTextMargin = 2;

    /// <summary>The rows by value, largest first, keeping data order for equal values (PieChartModel.sanitize).</summary>
    public static List<CdsChartPoint> Sorted(IEnumerable<CdsChartPoint> rows) =>
        rows.Select((p, i) => (P: p, I: i)).OrderByDescending(x => ValueOf(x.P)).ThenBy(x => x.I).Select(x => x.P).ToList();

    public static double ValueOf(CdsChartPoint p) => p.Value is { } v && !double.IsNaN(v) ? v : 0;

    /// <summary>
    /// d3's pie layout: the slices of <paramref name="values"/> (in their order) laid out clockwise from 12 o'clock in
    /// <paramref name="order"/> (input order when null), each widened by the pad angle.
    /// </summary>
    public static PieArc[] Arcs(IReadOnlyList<double> values, Comparison<int>? order = null, float padAngle = PadAngle)
    {
        var n = values.Count;
        var arcs = new PieArc[n];
        if (n == 0) return arcs;
        IEnumerable<int> index = Enumerable.Range(0, n);
        if (order is not null) index = index.OrderBy(i => i, Comparer<int>.Create(order));
        var sum = values.Where(v => v > 0).Sum();
        const float full = MathF.PI * 2;
        var pa = Math.Min(full / n, padAngle);
        var k = sum > 0 ? (full - n * pa) / sum : 0;
        var a0 = 0f;
        var i = 0;
        foreach (var j in index)
        {
            var v = values[j];
            var a1 = a0 + (float)(v > 0 ? v * k : 0) + pa;
            arcs[j] = new PieArc(j, i++, a0, a1);
            a0 = a1;
        }
        return arcs;
    }

    /// <summary>Carbon's convertValueToPercentage: the share of <paramref name="total"/> in percent, one decimal.</summary>
    public static double Percent(double value, double total, bool exact = false)
    {
        var pct = total != 0 ? value / total * 100 : 0;
        return exact ? pct : Math.Round(pct, 1, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// The outer radius (Pie.computeRadius): half the smaller side of the graph, 15px less when labels are drawn.
    /// </summary>
    public static float Radius(float width, float height, bool labels) =>
        Math.Max(0, Math.Min(width, height) / 2 + (labels ? RadiusOffset : 0));

    /// <summary>
    /// Label positions: 7px outside the arc at each slice's middle angle, pushed out by half the text width (and 10px lower
    /// on the bottom half). The two smallest slices (last in value order), when narrower than 5 degrees, get a callout:
    /// the slice last in angular order to the right, the other to the left.
    /// </summary>
    public static List<PieLabel> Labels(IReadOnlyList<PieArc> arcs, IReadOnlyList<string> texts, float radius, ChartFont font)
    {
        var n = arcs.Count;
        var labels = new List<PieLabel>(n);
        var r = radius + LabelDistance;
        for (var i = 0; i < n; i++)
        {
            var a = arcs[i];
            var theta = a.Mid;
            var deg = theta * 180 / MathF.PI;
            var offX = CdsChartTextMetrics.Width(texts[i], font) / 2;
            var offY = deg > 90 && deg < 270 ? 10f : 0f;
            var xPos = (offX + r) * MathF.Sin(theta);
            var yPos = (offY + r) * -MathF.Cos(theta);
            PieCallout? callout = null;
            var x = xPos;
            var y = yPos;
            if (i >= n - 2 && a.Degrees < CalloutMinSliceDegree)
            {
                var right = a.Order == n - 1;
                x = right ? xPos + CalloutOffsetX + CalloutTextMargin + offX : xPos - CalloutOffsetX - offX - CalloutTextMargin;
                y = yPos - CalloutOffsetY;
                var sy = yPos + offY;
                var ey = yPos - CalloutOffsetY + offY;
                var ex = right ? xPos + CalloutOffsetX : xPos - CalloutOffsetX;
                var ix = right ? ex - CalloutLineLength : ex + CalloutLineLength;
                callout = new PieCallout(new PointF(xPos, sy), new PointF(ix, ey), new PointF(ex, ey));
            }
            labels.Add(new PieLabel(i, texts[i], new PointF(x, y), callout));
        }
        return Separate(labels, font);
    }

    /// <summary>
    /// Moves plain labels off the callouts and off the labels of larger slices (Carbon lets them collide): the callouts
    /// keep their place, then each label in value order moves to the nearest free spot, at most 40px sideways (away
    /// from what it touches first) and 12px outward.
    /// </summary>
    internal static List<PieLabel> Separate(List<PieLabel> labels, ChartFont font)
    {
        const float Gap = 2;
        RectF Box(PieLabel l)
        {
            var w = CdsChartTextMetrics.Width(l.Text, font);
            return new RectF(l.At.X - w / 2 - Gap, l.At.Y - 0.75f * font.Size, w + 2 * Gap, 0.95f * font.Size);
        }
        static bool Crosses(RectF box, PointF a, PointF b)
        {
            for (var s = 0; s <= 16; s++)
            {
                var t = s / 16f;
                if (box.Contains(new PointF(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t))) return true;
            }
            return false;
        }
        var placed = labels.Where(l => l.Callout is not null).Select(Box).ToList();
        var lines = labels.Where(l => l.Callout is not null).SelectMany(l => new[] { (l.Callout!.Value.Start, l.Callout.Value.Bend), (l.Callout.Value.Bend, l.Callout.Value.End) }).ToList();
        var result = new List<PieLabel>(labels);
        for (var i = 0; i < result.Count; i++)
        {
            var l = result[i];
            if (l.Callout is not null) continue;
            var box = Box(l);
            bool Free(RectF b) => !placed.Any(o => o.IntersectsWith(b)) && !lines.Any(s => Crosses(b, s.Item1, s.Item2));
            if (!Free(box))
            {
                // Away from a callout first (they keep their place), else away from the larger slice's label.
                float dir = 0;
                foreach (var (a, b) in lines)
                    if (dir == 0 && Crosses(box, a, b)) dir = l.At.X >= (a.X + b.X) / 2 ? 1 : -1;
                foreach (var other in placed)
                    if (dir == 0 && other.IntersectsWith(box)) dir = l.At.X >= other.Center.X ? 1 : -1;
                // The cheapest free spot: sideways (preferably away), and up to 12px outward from the pie.
                var outward = l.At.Y < 0 ? -1 : 1;
                (float Dx, float Dy)? best = null;
                var bestCost = float.MaxValue;
                for (var dy = 0; dy <= 12; dy += 2)
                    for (var dx = -40; dx <= 40; dx++)
                    {
                        var cost = Math.Abs(dx) + 1.5f * dy + (dx != 0 && Math.Sign(dx) != dir ? 20 : 0);
                        if (cost >= bestCost || !Free(box.Offset(dx, outward * dy))) continue;
                        best = (dx, outward * dy);
                        bestCost = cost;
                    }
                if (best is { } spot)
                {
                    l = l with { At = new PointF(l.At.X + spot.Dx, l.At.Y + spot.Dy) };
                    box = box.Offset(spot.Dx, spot.Dy);
                }
                result[i] = l;
            }
            placed.Add(box);
        }
        return result;
    }
}
