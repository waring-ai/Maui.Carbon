namespace Maui.Carbon;

/// <summary>What the <see cref="TreeViewModel"/> needs to know about a node.</summary>
internal interface ITreeNodeData
{
    /// <summary>Whether the node's children are shown.</summary>
    bool IsExpanded { get; }

    /// <summary>Whether the node is disabled (skipped by the keyboard).</summary>
    bool IsDisabled { get; }

    /// <summary>The child nodes; empty for a leaf.</summary>
    IReadOnlyList<ITreeNodeData> ChildNodes { get; }
}

/// <summary>One visible row of a flattened tree.</summary>
/// <param name="Node">The node.</param>
/// <param name="Depth">0 for a root node.</param>
/// <param name="ParentIndex">The row index of the parent, or -1 for a root node.</param>
internal readonly record struct TreeRow(ITreeNodeData Node, int Depth, int ParentIndex)
{
    /// <summary>Whether the node is a parent (has children).</summary>
    public bool HasChildren => Node.ChildNodes.Count > 0;

    /// <summary>Whether the node is a parent and expanded.</summary>
    public bool IsOpen => HasChildren && Node.IsExpanded;
}

/// <summary>What a key press does in a tree.</summary>
/// <param name="Handled">Whether the key did anything (so the host should swallow it).</param>
/// <param name="FocusIndex">The row to move focus to, or -1 to stay.</param>
/// <param name="ToggleIndex">The row whose expansion changes, or -1.</param>
/// <param name="Expand">The new expansion of <paramref name="ToggleIndex"/>.</param>
/// <param name="Select">Whether the focused row is selected (activated).</param>
internal readonly record struct TreeKeyResult(bool Handled, int FocusIndex = -1, int ToggleIndex = -1, bool Expand = false, bool Select = false)
{
    /// <summary>Nothing happens.</summary>
    public static TreeKeyResult None => default;
}

/// <summary>Flattening, keyboard movement and indentation of <see cref="CdsTreeView"/> (pure, tested).</summary>
internal static class TreeViewModel
{
    /// <summary>The visible rows in order: every root, and the children of every expanded parent, depth first.</summary>
    public static List<TreeRow> Flatten(IEnumerable<ITreeNodeData> roots)
    {
        var rows = new List<TreeRow>();
        foreach (var root in roots) Add(rows, root, 0, -1);
        return rows;
    }

    static void Add(List<TreeRow> rows, ITreeNodeData node, int depth, int parent)
    {
        var index = rows.Count;
        rows.Add(new TreeRow(node, depth, parent));
        if (node.ChildNodes.Count == 0 || !node.IsExpanded) return;
        foreach (var child in node.ChildNodes) Add(rows, child, depth + 1, index);
    }

    /// <summary>The first enabled row at or after <paramref name="from"/> going in <paramref name="step"/> direction, or -1.</summary>
    public static int NextEnabled(IReadOnlyList<TreeRow> rows, int from, int step)
    {
        for (var i = from; i >= 0 && i < rows.Count; i += step)
            if (!rows[i].Node.IsDisabled) return i;
        return -1;
    }

    /// <summary>
    /// The row that takes the tab stop: the active node's, else the first selected, else the first enabled row.
    /// </summary>
    public static int TabStopIndex(IReadOnlyList<TreeRow> rows, ITreeNodeData? active, Func<ITreeNodeData, bool> isSelected)
    {
        if (active is not null)
        {
            var i = IndexOf(rows, active);
            if (i >= 0 && !rows[i].Node.IsDisabled) return i;
        }
        for (var i = 0; i < rows.Count; i++)
            if (!rows[i].Node.IsDisabled && isSelected(rows[i].Node)) return i;
        return NextEnabled(rows, 0, 1);
    }

    /// <summary>The row index of a node, or -1 when it is not visible.</summary>
    public static int IndexOf(IReadOnlyList<TreeRow> rows, ITreeNodeData node)
    {
        for (var i = 0; i < rows.Count; i++)
            if (ReferenceEquals(rows[i].Node, node)) return i;
        return -1;
    }

    /// <summary>
    /// What a key does with focus on row <paramref name="current"/> (-1 for none): Up and Down move through the enabled
    /// visible rows, Home and End jump to the first and last, Right expands a closed parent and then moves to its first
    /// child, Left collapses an open parent and otherwise moves to the parent, Enter toggles a parent and selects, Space selects.
    /// </summary>
    public static TreeKeyResult HandleKey(IReadOnlyList<TreeRow> rows, int current, CdsKey key)
    {
        if (rows.Count == 0) return TreeKeyResult.None;
        var hasCurrent = current >= 0 && current < rows.Count;
        switch (key)
        {
            case CdsKey.Down:
            {
                var next = NextEnabled(rows, hasCurrent ? current + 1 : 0, 1);
                return new TreeKeyResult(true, next);
            }
            case CdsKey.Up:
            {
                var next = NextEnabled(rows, hasCurrent ? current - 1 : rows.Count - 1, -1);
                return new TreeKeyResult(true, next);
            }
            case CdsKey.Home:
                return new TreeKeyResult(true, NextEnabled(rows, 0, 1));
            case CdsKey.End:
                return new TreeKeyResult(true, NextEnabled(rows, rows.Count - 1, -1));
        }
        if (!hasCurrent) return TreeKeyResult.None;
        var row = rows[current];
        switch (key)
        {
            case CdsKey.Right:
                if (!row.HasChildren) return TreeKeyResult.None;
                if (!row.Node.IsExpanded) return new TreeKeyResult(true, ToggleIndex: current, Expand: true);
                // Open parent: first enabled child (the rows right after it, one level deeper).
                for (var i = current + 1; i < rows.Count && rows[i].Depth > row.Depth; i++)
                    if (rows[i].ParentIndex == current && !rows[i].Node.IsDisabled) return new TreeKeyResult(true, i);
                return new TreeKeyResult(true);
            case CdsKey.Left:
                if (row.IsOpen) return new TreeKeyResult(true, ToggleIndex: current, Expand: false);
                if (row.ParentIndex >= 0 && !rows[row.ParentIndex].Node.IsDisabled) return new TreeKeyResult(true, row.ParentIndex);
                return TreeKeyResult.None;
            case CdsKey.Enter:
                return row.HasChildren && !row.Node.IsDisabled
                    ? new TreeKeyResult(true, ToggleIndex: current, Expand: !row.Node.IsExpanded, Select: true)
                    : new TreeKeyResult(true, Select: !row.Node.IsDisabled);
            case CdsKey.Space:
                return new TreeKeyResult(true, Select: !row.Node.IsDisabled);
            default:
                return TreeKeyResult.None;
        }
    }

    /// <summary>The row height: xs 24, sm 32.</summary>
    public static double RowHeight(CdsTreeViewSize size) => size == CdsTreeViewSize.Xs ? CdsSizing.XSmall : CdsSizing.Small;

    /// <summary>
    /// The x position of a node's toggle chevron: 16 to start, plus 16 per level (24 with icons: Carbon indents nested
    /// nodes by 8 more when every node has an icon).
    /// </summary>
    public static double ToggleX(int depth, bool withIcons) => CdsSpacing.Spacing05 + depth * (CdsSpacing.Spacing05 + (withIcons ? CdsSpacing.Spacing03 : 0));

    /// <summary>The width of the toggle slot (the chevron's 16 plus 4 either side, 24 in all).</summary>
    public const double ToggleSlot = 24;

    /// <summary>The start of the label text for a node: the toggle slot, a 4px gap, the icon column when used, and 4px of text padding.</summary>
    public static double TextX(int depth, bool withIcons)
    {
        // Spacer before the slot is ToggleX - 4; the slot is 24; then 4 gap (+ 16 icon + 8 margin) and 4 text padding.
        var x = ToggleX(depth, withIcons) - 4 + ToggleSlot + 4;
        if (withIcons) x += 16 + CdsSpacing.Spacing03;
        return x + 4;
    }
}
