namespace Maui.Carbon;

/// <summary>
/// One node of a hierarchy chart (treemap, circle pack and tree charts): a name, an optional value and child nodes. The
/// top-level nodes you put in <see cref="CdsChart.Data"/> are the groups (legend items and colors); leaves carry the values
/// and parents sum their children. The chart does not watch a node's properties: replace the data to change the chart.
/// </summary>
[ContentProperty(nameof(Children))]
public sealed class CdsChartNode
{
    /// <summary>Creates an empty node (for XAML).</summary>
    public CdsChartNode()
    {
    }

    /// <summary>Creates a leaf with a value; <paramref name="showLabel"/> draws its name in a treemap.</summary>
    public CdsChartNode(string name, double? value, bool showLabel = false)
    {
        Name = name;
        Value = value;
        ShowLabel = showLabel;
    }

    /// <summary>Creates a parent of <paramref name="children"/>.</summary>
    public CdsChartNode(string name, params CdsChartNode[] children)
    {
        Name = name;
        Children = children.ToList();
    }

    /// <summary>The node's name (Carbon's <c>name</c>): the legend item of a top-level node, the label of a leaf.</summary>
    public string? Name { get; set; }

    /// <summary>The node's own value (Carbon's <c>value</c>); a parent's size is this plus the sum of its children.</summary>
    public double? Value { get; set; }

    /// <summary>Whether a treemap draws this leaf's name (Carbon's <c>showLabel</c>). Label only leaves large enough to hold it.</summary>
    public bool ShowLabel { get; set; }

    /// <summary>The child nodes (Carbon's <c>children</c>); empty for a leaf.</summary>
    public IList<CdsChartNode> Children { get; set; } = new List<CdsChartNode>();

    /// <summary>What a screen reader speaks for the node instead of its name and value.</summary>
    public string? Description { get; set; }

    /// <summary>Any object of your own carried with the node; the chart never reads it.</summary>
    public object? Tag { get; set; }
}

/// <summary>The node a hierarchy chart reported (a click on a circle or a tree node).</summary>
public sealed class CdsChartNodeEventArgs(CdsChartNode node) : EventArgs
{
    /// <summary>The node that was clicked.</summary>
    public CdsChartNode Node { get; } = node;
}
