namespace Maui.Carbon;

/// <summary>What a network diagram node is drawn as (Carbon's diagram building blocks).</summary>
public enum CdsNetworkNodeKind
{
    /// <summary>A card node (Carbon's <c>CardNode</c>): <c>layer-01</c> with a 4px left border, a title, subtitle and label (default).</summary>
    Card,
    /// <summary>A shape node (Carbon's <c>ShapeNode</c>): a 48px circle or square holding an icon, with its text below.</summary>
    Shape,
}

/// <summary>The outline of a shape node (Carbon's <c>ShapeNode shape</c>).</summary>
public enum CdsNetworkNodeShape
{
    /// <summary>A circle (default).</summary>
    Circle,
    /// <summary>A square.</summary>
    Square,
    /// <summary>A square with 8px rounded corners.</summary>
    RoundedSquare,
}

/// <summary>How an edge runs between its nodes (Carbon's path builders).</summary>
public enum CdsNetworkEdgePath
{
    /// <summary>A straight line from outline to outline (<c>buildStraightPathString</c>; default).</summary>
    Straight,
    /// <summary>Horizontal, vertical, horizontal between the facing sides (<c>buildElbowPathString</c>).</summary>
    Elbow,
    /// <summary>A cubic curve between the facing sides (<c>buildBezierPathString</c>).</summary>
    Bezier,
}

/// <summary>The line style of an edge (Carbon's <c>Edge variant</c>).</summary>
public enum CdsNetworkEdgeVariant
{
    /// <summary>A solid 1px line.</summary>
    Default,
    /// <summary>Dashes of 2, gaps of 4.</summary>
    DashSm,
    /// <summary>Dashes of 4, gaps of 4.</summary>
    DashMd,
    /// <summary>Dashes of 8, gaps of 4.</summary>
    DashLg,
    /// <summary>Dashes of 16, gaps of 4.</summary>
    DashXl,
    /// <summary>A line in a 6px <c>layer-accent-01</c> casing.</summary>
    Tunnel,
    /// <summary>A double line: a <c>background</c> core in a <c>border-inverse</c> casing.</summary>
    Double,
}

/// <summary>A marker at an edge's end (Carbon's marker definitions), filled <c>border-inverse</c>.</summary>
public enum CdsNetworkMarker
{
    /// <summary>No marker.</summary>
    None,
    /// <summary>An arrow pointing back along the edge (7.5 by 8).</summary>
    ArrowLeft,
    /// <summary>An arrow pointing along the edge (7.5 by 8).</summary>
    ArrowRight,
    /// <summary>A circle (10 by 10).</summary>
    Circle,
    /// <summary>A diamond (10 by 10).</summary>
    Diamond,
    /// <summary>A square (8 by 9).</summary>
    Square,
    /// <summary>A bar across the edge (2 by 8).</summary>
    Tee,
}

/// <summary>
/// A node of a <see cref="CdsNetworkDiagram"/>, placed by the app: <see cref="X"/> and <see cref="Y"/> are its centre.
/// Status colors on cards (<see cref="Color"/>) need a text label too.
/// </summary>
public sealed class CdsNetworkNode
{
    /// <summary>Creates an empty node (for XAML).</summary>
    public CdsNetworkNode()
    {
    }

    /// <summary>Creates a card node centred at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public CdsNetworkNode(string id, double x, double y, string title, string? subtitle = null)
    {
        Id = id;
        X = x;
        Y = y;
        Title = title;
        Subtitle = subtitle;
    }

    /// <summary>The id edges refer to.</summary>
    public string? Id { get; set; }

    /// <summary>The horizontal centre, from the diagram's left edge.</summary>
    public double X { get; set; }

    /// <summary>The vertical centre, from the diagram's top edge.</summary>
    public double Y { get; set; }

    /// <summary>A card (default) or a shape node.</summary>
    public CdsNetworkNodeKind Kind { get; set; }

    /// <summary>The title: productive-heading-01 on cards, body-compact-01 below shapes.</summary>
    public string? Title { get; set; }

    /// <summary>The subtitle (body-compact-01).</summary>
    public string? Subtitle { get; set; }

    /// <summary>A card's label: label-01 in <c>text-secondary</c>, 24px below the subtitle (Carbon's <c>CardNodeLabel</c>).</summary>
    public string? Label { get; set; }

    /// <summary>A shape node's description below its subtitle: label-01 in <c>text-secondary</c>.</summary>
    public string? Description { get; set; }

    /// <summary>A shape node's outline. Default <see cref="CdsNetworkNodeShape.Circle"/>.</summary>
    public CdsNetworkNodeShape Shape { get; set; }

    /// <summary>The 20px icon centred in a shape node, or at the far side of a card.</summary>
    public CdsIconName? Icon { get; set; }

    /// <summary>A shape node's size (Carbon's <c>size</c>). Default 48.</summary>
    public double Size { get; set; } = 48;

    /// <summary>A card's width. Default 216.</summary>
    public double Width { get; set; } = 216;

    /// <summary>A card's height; <c>null</c> (default) fits its text: 16px padding and 18px per title or subtitle, 40 for a label, at least 64.</summary>
    public double? Height { get; set; }

    /// <summary>A card's left border color as a color key (<see cref="CdsColorKeys.SupportSuccess"/>…); default <c>border-inverse</c>.</summary>
    public string? Color { get; set; }

    /// <summary>Whether a card shows two offset outlines behind it, for a group of nodes (Carbon's <c>stacked</c>).</summary>
    public bool IsStacked { get; set; }

    /// <summary>Whether the node is a button (Carbon's <c>onClick</c>/<c>href</c>): it hovers and raises <see cref="CdsNetworkDiagram.NodeClicked"/>.</summary>
    public bool IsClickable { get; set; }

    /// <summary>Any object of your own carried with the node; the diagram never reads it.</summary>
    public object? Tag { get; set; }
}

/// <summary>An edge of a <see cref="CdsNetworkDiagram"/>: a connection between two nodes (or two free points).</summary>
public sealed class CdsNetworkEdge
{
    /// <summary>Creates an empty edge (for XAML).</summary>
    public CdsNetworkEdge()
    {
    }

    /// <summary>Creates an edge from node <paramref name="source"/> to node <paramref name="target"/>.</summary>
    public CdsNetworkEdge(string source, string target, CdsNetworkEdgePath path = CdsNetworkEdgePath.Straight, CdsNetworkMarker markerEnd = CdsNetworkMarker.None)
    {
        Source = source;
        Target = target;
        Path = path;
        MarkerEnd = markerEnd;
    }

    /// <summary>The id of the node it leaves.</summary>
    public string? Source { get; set; }

    /// <summary>The id of the node it enters.</summary>
    public string? Target { get; set; }

    /// <summary>The start point when there is no <see cref="Source"/> node (Carbon's <c>Edge source</c> coordinates).</summary>
    public Point? Start { get; set; }

    /// <summary>The end point when there is no <see cref="Target"/> node.</summary>
    public Point? End { get; set; }

    /// <summary>How the edge runs. Default <see cref="CdsNetworkEdgePath.Straight"/>.</summary>
    public CdsNetworkEdgePath Path { get; set; }

    /// <summary>Where an elbow bends, as a share of the way across (Carbon's <c>percent</c>). Default 0.5.</summary>
    public double BendPercent { get; set; } = 0.5;

    /// <summary>The line style. Default <see cref="CdsNetworkEdgeVariant.Default"/>.</summary>
    public CdsNetworkEdgeVariant Variant { get; set; }

    /// <summary>The marker at the start.</summary>
    public CdsNetworkMarker MarkerStart { get; set; }

    /// <summary>The marker at the end; its tip touches the target node.</summary>
    public CdsNetworkMarker MarkerEnd { get; set; }

    /// <summary>The line color as a color key; default <c>border-strong-01</c>.</summary>
    public string? Color { get; set; }

    /// <summary>What the edge means ("calls", "replicates to"): its tooltip and part of its spoken name.</summary>
    public string? Label { get; set; }
}
