namespace Maui.Carbon;

/// <summary>One flow of an alluvial chart (Carbon's data row <c>{ source, target, value }</c>): a band from one node to another, as wide as its value.</summary>
public sealed class CdsAlluvialLink
{
    /// <summary>Creates an empty link (for XAML).</summary>
    public CdsAlluvialLink()
    {
    }

    /// <summary>Creates a link from <paramref name="source"/> to <paramref name="target"/> carrying <paramref name="value"/>.</summary>
    public CdsAlluvialLink(string source, string target, double value)
    {
        Source = source;
        Target = target;
        Value = value;
    }

    /// <summary>The name of the node the flow leaves (Carbon's <c>source</c>); it also picks the link's color.</summary>
    public string? Source { get; set; }

    /// <summary>The name of the node the flow enters (Carbon's <c>target</c>).</summary>
    public string? Target { get; set; }

    /// <summary>The quantity that flows (Carbon's <c>value</c>); give its unit in <see cref="CdsAlluvialChart.Units"/>.</summary>
    public double Value { get; set; }

    /// <summary>Any object of your own carried with the link; the chart never reads it.</summary>
    public object? Tag { get; set; }
}

/// <summary>A node of an alluvial chart (Carbon's <c>alluvial.nodes</c> entry): a name and the category that heads its column.</summary>
public sealed class CdsAlluvialNode
{
    /// <summary>Creates an empty node (for XAML).</summary>
    public CdsAlluvialNode()
    {
    }

    /// <summary>Creates a node; <paramref name="category"/> heads its column ("Source", "Stage").</summary>
    public CdsAlluvialNode(string name, string? category = null)
    {
        Name = name;
        Category = category;
    }

    /// <summary>The node's name; links refer to it in <see cref="CdsAlluvialLink.Source"/> and <see cref="CdsAlluvialLink.Target"/>.</summary>
    public string? Name { get; set; }

    /// <summary>The column header (Carbon's <c>category</c>); nodes of one column share it.</summary>
    public string? Category { get; set; }
}

/// <summary>How an alluvial chart places nodes in columns (Carbon's <c>alluvial.nodeAlignment</c>).</summary>
public enum CdsAlluvialNodeAlignment
{
    /// <summary>By distance from the sources, with nodes that have no outgoing flows in the last column (default; d3's justify).</summary>
    Center,
    /// <summary>By distance from the sources.</summary>
    Left,
    /// <summary>By distance from the sinks.</summary>
    Right,
}
