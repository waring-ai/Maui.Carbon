namespace Maui.Carbon;

/// <summary>A node of the sankey layout.</summary>
internal sealed class SankeyNode(string name, string? category)
{
    public string Name { get; } = name;
    public string? Category { get; } = category;
    public int Index;
    public List<SankeyLink> SourceLinks = [];
    public List<SankeyLink> TargetLinks = [];
    public double Value;
    public int Depth;
    public int Height;
    public int Layer;
    public double X0;
    public double X1;
    public double Y0;
    public double Y1;
    /// <summary>The label box the alluvial chart drew for the node.</summary>
    public RectF Box;
}

/// <summary>A link of the sankey layout.</summary>
internal sealed class SankeyLink(SankeyNode source, SankeyNode target, double value)
{
    public SankeyNode Source { get; } = source;
    public SankeyNode Target { get; } = target;
    public double Value { get; } = value;
    public int Index;
    public double Width;
    /// <summary>The link's centre at the source.</summary>
    public double Y0;
    /// <summary>The link's centre at the target.</summary>
    public double Y1;
    /// <summary>The item it came from.</summary>
    public object? Tag;
}

/// <summary>How a sankey assigns nodes to columns (d3-sankey's align functions).</summary>
internal enum SankeyAlign
{
    /// <summary><c>sankeyJustify</c>: like left, but nodes without outgoing links move to the last column.</summary>
    Justify,
    /// <summary><c>sankeyLeft</c>: by depth from the sources.</summary>
    Left,
    /// <summary><c>sankeyRight</c>: by height from the sinks.</summary>
    Right,
}

/// <summary>
/// d3-sankey 0.12's layout, as Carbon's alluvial chart configures it (6 relaxation iterations, nodes sorted by breadth,
/// links sorted by the far end's breadth): columns by depth, node heights proportional to value, then iterative
/// relaxation and collision resolution.
/// </summary>
internal sealed class ChartSankey
{
    public double X0 { get; init; }
    public double Y0 { get; init; }
    public double X1 { get; init; } = 1;
    public double Y1 { get; init; } = 1;
    public double NodeWidth { get; init; } = 24;
    public double NodePadding { get; init; } = 8;
    public SankeyAlign Align { get; init; } = SankeyAlign.Justify;
    public int Iterations { get; init; } = 6;

    double _py;

    /// <summary>Lays out the graph; throws for a link to an unknown node or a cycle (as d3-sankey does).</summary>
    public (List<SankeyNode> Nodes, List<SankeyLink> Links) Layout(IEnumerable<(string Name, string? Category)> nodes, IEnumerable<(string Source, string Target, double Value, object? Tag)> links)
    {
        var nodeList = nodes.Select(n => new SankeyNode(n.Name, n.Category)).ToList();
        var byId = new Dictionary<string, SankeyNode>(StringComparer.Ordinal);
        for (var i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].Index = i;
            byId.TryAdd(nodeList[i].Name, nodeList[i]);
        }
        var linkList = new List<SankeyLink>();
        foreach (var (s, t, v, tag) in links)
        {
            if (!byId.TryGetValue(s, out var source)) throw new InvalidOperationException("missing: " + s);
            if (!byId.TryGetValue(t, out var target)) throw new InvalidOperationException("missing: " + t);
            var link = new SankeyLink(source, target, v) { Index = linkList.Count, Tag = tag };
            source.SourceLinks.Add(link);
            target.TargetLinks.Add(link);
            linkList.Add(link);
        }
        foreach (var node in nodeList)
            node.Value = Math.Max(node.SourceLinks.Sum(l => l.Value), node.TargetLinks.Sum(l => l.Value));
        ComputeDepths(nodeList);
        ComputeHeights(nodeList);
        ComputeBreadths(nodeList);
        ComputeLinkBreadths(nodeList);
        return (nodeList, linkList);
    }

    static void ComputeDepths(List<SankeyNode> nodes)
    {
        var current = new List<SankeyNode>(nodes);
        var x = 0;
        while (current.Count > 0)
        {
            var next = new List<SankeyNode>();
            var seen = new HashSet<SankeyNode>();
            foreach (var node in current)
            {
                node.Depth = x;
                foreach (var l in node.SourceLinks) if (seen.Add(l.Target)) next.Add(l.Target);
            }
            if (++x > nodes.Count) throw new InvalidOperationException("circular link");
            current = next;
        }
    }

    static void ComputeHeights(List<SankeyNode> nodes)
    {
        var current = new List<SankeyNode>(nodes);
        var x = 0;
        while (current.Count > 0)
        {
            var next = new List<SankeyNode>();
            var seen = new HashSet<SankeyNode>();
            foreach (var node in current)
            {
                node.Height = x;
                foreach (var l in node.TargetLinks) if (seen.Add(l.Source)) next.Add(l.Source);
            }
            if (++x > nodes.Count) throw new InvalidOperationException("circular link");
            current = next;
        }
    }

    int AlignOf(SankeyNode node, int n) => Align switch
    {
        SankeyAlign.Left => node.Depth,
        SankeyAlign.Right => n - 1 - node.Height,
        _ => node.SourceLinks.Count > 0 ? node.Depth : n - 1,
    };

    List<List<SankeyNode>> ComputeLayers(List<SankeyNode> nodes)
    {
        var x = (nodes.Count == 0 ? 0 : nodes.Max(d => d.Depth)) + 1;
        var kx = x > 1 ? (X1 - X0 - NodeWidth) / (x - 1) : 0;
        var columns = new List<List<SankeyNode>>();
        for (var i = 0; i < x; i++) columns.Add([]);
        foreach (var node in nodes)
        {
            var i = Math.Max(0, Math.Min(x - 1, AlignOf(node, x)));
            node.Layer = i;
            node.X0 = X0 + i * kx;
            node.X1 = node.X0 + NodeWidth;
            columns[i].Add(node);
        }
        columns.RemoveAll(c => c.Count == 0);
        return columns;
    }

    void ComputeBreadths(List<SankeyNode> nodes)
    {
        var columns = ComputeLayers(nodes);
        if (columns.Count == 0) return;
        var maxLen = columns.Max(c => c.Count);
        _py = Math.Min(NodePadding, (Y1 - Y0) / (maxLen - 1));
        InitializeBreadths(columns);
        for (var i = 0; i < Iterations; ++i)
        {
            var alpha = Math.Pow(0.99, i);
            var beta = Math.Max(1 - alpha, (i + 1.0) / Iterations);
            RelaxRightToLeft(columns, alpha, beta);
            RelaxLeftToRight(columns, alpha, beta);
        }
    }

    void InitializeBreadths(List<List<SankeyNode>> columns)
    {
        // d3's min skips the NaN and infinite ratios of columns without value.
        var ratios = columns.Select(c => (Y1 - Y0 - (c.Count - 1) * _py) / c.Sum(n => n.Value)).Where(double.IsFinite).ToList();
        var ky = ratios.Count > 0 ? ratios.Min() : 0;
        foreach (var nodes in columns)
        {
            var y = Y0;
            foreach (var node in nodes)
            {
                node.Y0 = y;
                node.Y1 = y + node.Value * ky;
                y = node.Y1 + _py;
                foreach (var link in node.SourceLinks) link.Width = link.Value * ky;
            }
            y = (Y1 - y + _py) / (nodes.Count + 1);
            for (var i = 0; i < nodes.Count; ++i)
            {
                nodes[i].Y0 += y * (i + 1);
                nodes[i].Y1 += y * (i + 1);
            }
            ReorderLinks(nodes);
        }
    }

    void RelaxLeftToRight(List<List<SankeyNode>> columns, double alpha, double beta)
    {
        for (var i = 1; i < columns.Count; ++i)
        {
            var column = columns[i];
            foreach (var target in column)
            {
                double y = 0, w = 0;
                foreach (var l in target.TargetLinks)
                {
                    var v = l.Value * (target.Layer - l.Source.Layer);
                    y += TargetTop(l.Source, target) * v;
                    w += v;
                }
                if (!(w > 0)) continue;
                var dy = (y / w - target.Y0) * alpha;
                target.Y0 += dy;
                target.Y1 += dy;
                ReorderNodeLinks(target);
            }
            StableSort(column, (a, b) => a.Y0.CompareTo(b.Y0));
            ResolveCollisions(column, beta);
        }
    }

    void RelaxRightToLeft(List<List<SankeyNode>> columns, double alpha, double beta)
    {
        for (var i = columns.Count - 2; i >= 0; --i)
        {
            var column = columns[i];
            foreach (var source in column)
            {
                double y = 0, w = 0;
                foreach (var l in source.SourceLinks)
                {
                    var v = l.Value * (l.Target.Layer - source.Layer);
                    y += SourceTop(source, l.Target) * v;
                    w += v;
                }
                if (!(w > 0)) continue;
                var dy = (y / w - source.Y0) * alpha;
                source.Y0 += dy;
                source.Y1 += dy;
                ReorderNodeLinks(source);
            }
            StableSort(column, (a, b) => a.Y0.CompareTo(b.Y0));
            ResolveCollisions(column, beta);
        }
    }

    void ResolveCollisions(List<SankeyNode> nodes, double alpha)
    {
        var i = nodes.Count >> 1;
        var subject = nodes[i];
        BottomToTop(nodes, subject.Y0 - _py, i - 1, alpha);
        TopToBottom(nodes, subject.Y1 + _py, i + 1, alpha);
        BottomToTop(nodes, Y1, nodes.Count - 1, alpha);
        TopToBottom(nodes, Y0, 0, alpha);
    }

    void TopToBottom(List<SankeyNode> nodes, double y, int i, double alpha)
    {
        for (; i < nodes.Count; ++i)
        {
            var node = nodes[i];
            var dy = (y - node.Y0) * alpha;
            if (dy > 1e-6)
            {
                node.Y0 += dy;
                node.Y1 += dy;
            }
            y = node.Y1 + _py;
        }
    }

    void BottomToTop(List<SankeyNode> nodes, double y, int i, double alpha)
    {
        for (; i >= 0; --i)
        {
            var node = nodes[i];
            var dy = (node.Y1 - y) * alpha;
            if (dy > 1e-6)
            {
                node.Y0 -= dy;
                node.Y1 -= dy;
            }
            y = node.Y0 - _py;
        }
    }

    static int ByTargetBreadth(SankeyLink a, SankeyLink b) { var c = a.Target.Y0.CompareTo(b.Target.Y0); return c != 0 ? c : a.Index - b.Index; }

    static int BySourceBreadth(SankeyLink a, SankeyLink b) { var c = a.Source.Y0.CompareTo(b.Source.Y0); return c != 0 ? c : a.Index - b.Index; }

    static void ReorderNodeLinks(SankeyNode node)
    {
        foreach (var l in node.TargetLinks) StableSort(l.Source.SourceLinks, ByTargetBreadth);
        foreach (var l in node.SourceLinks) StableSort(l.Target.TargetLinks, BySourceBreadth);
    }

    static void ReorderLinks(List<SankeyNode> nodes)
    {
        foreach (var node in nodes)
        {
            StableSort(node.SourceLinks, ByTargetBreadth);
            StableSort(node.TargetLinks, BySourceBreadth);
        }
    }

    double TargetTop(SankeyNode source, SankeyNode target)
    {
        var y = source.Y0 - (source.SourceLinks.Count - 1) * _py / 2;
        foreach (var l in source.SourceLinks)
        {
            if (l.Target == target) break;
            y += l.Width + _py;
        }
        foreach (var l in target.TargetLinks)
        {
            if (l.Source == source) break;
            y -= l.Width;
        }
        return y;
    }

    double SourceTop(SankeyNode source, SankeyNode target)
    {
        var y = target.Y0 - (target.TargetLinks.Count - 1) * _py / 2;
        foreach (var l in target.TargetLinks)
        {
            if (l.Source == source) break;
            y += l.Width + _py;
        }
        foreach (var l in source.SourceLinks)
        {
            if (l.Target == target) break;
            y -= l.Width;
        }
        return y;
    }

    static void ComputeLinkBreadths(List<SankeyNode> nodes)
    {
        foreach (var node in nodes)
        {
            var y0 = node.Y0;
            var y1 = y0;
            foreach (var l in node.SourceLinks)
            {
                l.Y0 = y0 + l.Width / 2;
                y0 += l.Width;
            }
            foreach (var l in node.TargetLinks)
            {
                l.Y1 = y1 + l.Width / 2;
                y1 += l.Width;
            }
        }
    }

    /// <summary>A stable in-place sort (JavaScript's Array.prototype.sort is stable; List.Sort is not).</summary>
    static void StableSort<T>(List<T> list, Comparison<T> compare)
    {
        if (list.Count < 2) return;
        var sorted = list.Select((x, i) => (x, i)).OrderBy(p => p.x, Comparer<T>.Create(compare)).ThenBy(p => p.i).Select(p => p.x).ToList();
        for (var i = 0; i < sorted.Count; i++) list[i] = sorted[i];
    }
}
