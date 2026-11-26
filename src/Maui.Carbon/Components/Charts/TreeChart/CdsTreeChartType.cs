namespace Maui.Carbon;

/// <summary>The layout of a tree chart (Carbon's <c>tree.type</c>).</summary>
public enum CdsTreeChartType
{
    /// <summary>A tidy tree: each node one level right of its parent (default).</summary>
    Tree,
    /// <summary>A dendrogram: every leaf lines up in the last column, as for clustering results.</summary>
    Dendrogram,
}
