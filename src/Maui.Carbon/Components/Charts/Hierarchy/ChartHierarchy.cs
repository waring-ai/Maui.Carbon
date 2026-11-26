namespace Maui.Carbon;

/// <summary>
/// A laid-out hierarchy node (d3-hierarchy's <c>Node</c>): the data node, its parent and children, depth, summed value and
/// the positions the layouts write (x0..y1 for treemaps, x, y and r for packs and trees).
/// </summary>
internal sealed class HierarchyNode
{
    public HierarchyNode(CdsChartNode? data, string name)
    {
        Data = data;
        Name = name;
    }

    /// <summary>The data node; null for the synthetic root.</summary>
    public CdsChartNode? Data { get; }

    public string Name { get; }

    public HierarchyNode? Parent { get; private set; }

    /// <summary>The children, or null for a leaf (d3: an empty children array is a leaf).</summary>
    public List<HierarchyNode>? Children { get; private set; }

    public int Depth { get; private set; }

    /// <summary>Pre-order index in the whole hierarchy (stable for the same data).</summary>
    public int Index { get; set; }

    /// <summary>The top-level (depth 1) ancestor's group name, or a group assigned by the chart.</summary>
    public string Group { get; set; } = "";

    public double Value { get; set; }

    public double X0 { get; set; }

    public double Y0 { get; set; }

    public double X1 { get; set; }

    public double Y1 { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double R { get; set; }

    public bool HasChildren => Children is { Count: > 0 };

    /// <summary>d3 <c>hierarchy({ name, children })</c>.</summary>
    public static HierarchyNode Build(string rootName, IEnumerable<CdsChartNode> children)
    {
        var root = new HierarchyNode(null, rootName);
        root.Attach(children);
        var i = 0;
        foreach (var n in root.PreOrder()) n.Index = i++;
        return root;
    }

    void Attach(IEnumerable<CdsChartNode> children)
    {
        var list = new List<HierarchyNode>();
        foreach (var c in children)
        {
            if (c is null) continue;
            var node = new HierarchyNode(c, c.Name ?? "") { Parent = this, Depth = Depth + 1 };
            if (c.Children is { Count: > 0 } cc) node.Attach(cc);
            list.Add(node);
        }
        Children = list.Count > 0 ? list : null;
    }

    /// <summary>d3 <c>sum(d => d.value)</c>: own value plus every descendant's.</summary>
    public HierarchyNode Sum()
    {
        foreach (var n in PostOrder())
        {
            var v = n.Data?.Value is { } d && !double.IsNaN(d) ? d : 0;
            if (n.Children is { } ch) foreach (var c in ch) v += c.Value;
            n.Value = v;
        }
        return this;
    }

    /// <summary>d3 <c>sort((a, b) => b.value - a.value)</c>, stable like Array.prototype.sort.</summary>
    public HierarchyNode SortByValueDescending()
    {
        foreach (var n in PreOrder())
            if (n.Children is { Count: > 1 } ch)
                n.Children = ch.Select((c, i) => (c, i)).OrderByDescending(x => x.c.Value).ThenBy(x => x.i).Select(x => x.c).ToList();
        return this;
    }

    /// <summary>Pre-order, children left to right (d3 <c>eachBefore</c>).</summary>
    public IEnumerable<HierarchyNode> PreOrder()
    {
        var stack = new Stack<HierarchyNode>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            yield return n;
            if (n.Children is { } ch)
                for (var i = ch.Count - 1; i >= 0; i--) stack.Push(ch[i]);
        }
    }

    /// <summary>Post-order, children left to right (d3 <c>eachAfter</c>).</summary>
    public IEnumerable<HierarchyNode> PostOrder()
    {
        var list = PreOrderReversedChildren().ToList();
        for (var i = list.Count - 1; i >= 0; i--) yield return list[i];
    }

    IEnumerable<HierarchyNode> PreOrderReversedChildren()
    {
        var stack = new Stack<HierarchyNode>();
        stack.Push(this);
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            yield return n;
            if (n.Children is { } ch) foreach (var c in ch) stack.Push(c);
        }
    }

    /// <summary>Breadth-first, like d3 <c>descendants()</c>.</summary>
    public List<HierarchyNode> Descendants()
    {
        var list = new List<HierarchyNode> { this };
        for (var i = 0; i < list.Count; i++)
            if (list[i].Children is { } ch) list.AddRange(ch);
        return list;
    }

    /// <summary>The leaves in pre-order (d3 <c>leaves()</c>).</summary>
    public IEnumerable<HierarchyNode> Leaves() => PreOrder().Where(n => n.Children is null);

    /// <summary>The depth-1 ancestor (the legend group of a treemap leaf).</summary>
    public HierarchyNode TopParent()
    {
        var p = this;
        while (p.Depth > 1 && p.Parent is not null) p = p.Parent;
        return p;
    }
}
