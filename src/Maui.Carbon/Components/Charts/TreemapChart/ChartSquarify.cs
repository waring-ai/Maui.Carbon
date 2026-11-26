namespace Maui.Carbon;

/// <summary>
/// d3-hierarchy's treemap layout with the squarified tiling (golden-ratio aspect target), inner padding and pixel
/// rounding, as Carbon's treemap uses it: <c>treemap().size([w, h]).paddingInner(1).paddingOuter(0).round(true)</c>.
/// </summary>
internal static class ChartSquarify
{
    /// <summary>The golden ratio: squarify's target aspect ratio.</summary>
    public static readonly double Phi = (1 + Math.Sqrt(5)) / 2;

    /// <summary>Lays out a summed (and sorted) hierarchy into a <paramref name="width"/> by <paramref name="height"/> box.</summary>
    public static void Treemap(HierarchyNode root, double width, double height, double paddingInner = 1, bool round = true)
    {
        root.X0 = root.Y0 = 0;
        root.X1 = width;
        root.Y1 = height;
        var padding = new Dictionary<int, double> { [0] = 0 };
        foreach (var node in root.PreOrder())
        {
            var p = padding.TryGetValue(node.Depth, out var pp) ? pp : 0;
            double x0 = node.X0 + p, y0 = node.Y0 + p, x1 = node.X1 - p, y1 = node.Y1 - p;
            if (x1 < x0) x0 = x1 = (x0 + x1) / 2;
            if (y1 < y0) y0 = y1 = (y0 + y1) / 2;
            node.X0 = x0;
            node.Y0 = y0;
            node.X1 = x1;
            node.Y1 = y1;
            if (node.Children is null) continue;
            p = padding[node.Depth + 1] = paddingInner / 2;
            x0 -= p;
            y0 -= p;
            x1 += p;
            y1 += p;
            if (x1 < x0) x0 = x1 = (x0 + x1) / 2;
            if (y1 < y0) y0 = y1 = (y0 + y1) / 2;
            Squarify(node, x0, y0, x1, y1, Phi);
        }
        if (round)
            foreach (var node in root.PreOrder())
            {
                node.X0 = JsRound(node.X0);
                node.Y0 = JsRound(node.Y0);
                node.X1 = JsRound(node.X1);
                node.Y1 = JsRound(node.Y1);
            }
    }

    /// <summary>JavaScript's Math.round: halves round up.</summary>
    public static double JsRound(double v) => Math.Floor(v + 0.5);

    /// <summary>d3 <c>squarifyRatio</c>: rows of children whose worst aspect ratio is as close to <paramref name="ratio"/> as possible.</summary>
    public static void Squarify(HierarchyNode parent, double x0, double y0, double x1, double y1, double ratio)
    {
        var nodes = parent.Children!;
        int i0 = 0, i1 = 0, n = nodes.Count;
        var value = parent.Value;
        while (i0 < n)
        {
            var dx = x1 - x0;
            var dy = y1 - y0;
            double sumValue;
            // Find the next non-empty node.
            do sumValue = nodes[i1++].Value; while (sumValue == 0 && i1 < n);
            var minValue = sumValue;
            var maxValue = sumValue;
            var alpha = Math.Max(dy / dx, dx / dy) / (value * ratio);
            var beta = sumValue * sumValue * alpha;
            var minRatio = Math.Max(maxValue / beta, beta / minValue);
            // Keep adding nodes while the aspect ratio maintains or improves.
            for (; i1 < n; ++i1)
            {
                var nodeValue = nodes[i1].Value;
                sumValue += nodeValue;
                if (nodeValue < minValue) minValue = nodeValue;
                if (nodeValue > maxValue) maxValue = nodeValue;
                beta = sumValue * sumValue * alpha;
                var newRatio = Math.Max(maxValue / beta, beta / minValue);
                if (newRatio > minRatio)
                {
                    sumValue -= nodeValue;
                    break;
                }
                minRatio = newRatio;
            }
            var row = nodes.GetRange(i0, i1 - i0);
            if (dx < dy)
            {
                var ny = dy != 0 ? y0 + dy * sumValue / value : y1;
                Dice(row, sumValue, x0, y0, x1, ny);
                if (dy != 0) y0 = ny;
            }
            else
            {
                var nx = dx != 0 ? x0 + dx * sumValue / value : x1;
                Slice(row, sumValue, x0, y0, nx, y1);
                if (dx != 0) x0 = nx;
            }
            value -= sumValue;
            i0 = i1;
        }
    }

    /// <summary>d3 <c>treemapDice</c>: side by side horizontally.</summary>
    static void Dice(List<HierarchyNode> nodes, double total, double x0, double y0, double x1, double y1)
    {
        var k = total != 0 ? (x1 - x0) / total : 0;
        foreach (var node in nodes)
        {
            node.Y0 = y0;
            node.Y1 = y1;
            node.X0 = x0;
            node.X1 = x0 += node.Value * k;
        }
    }

    /// <summary>d3 <c>treemapSlice</c>: stacked vertically.</summary>
    static void Slice(List<HierarchyNode> nodes, double total, double x0, double y0, double x1, double y1)
    {
        var k = total != 0 ? (y1 - y0) / total : 0;
        foreach (var node in nodes)
        {
            node.X0 = x0;
            node.X1 = x1;
            node.Y0 = y0;
            node.Y1 = y0 += node.Value * k;
        }
    }
}
