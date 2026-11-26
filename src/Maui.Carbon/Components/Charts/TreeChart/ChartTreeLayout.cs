namespace Maui.Carbon;

/// <summary>
/// d3-hierarchy's tidy tree (Reingold-Tilford with Buchheim's linear-time improvements) and cluster (dendrogram) layouts,
/// with d3's default separation (1 between siblings, 2 between cousins). They write <see cref="HierarchyNode.X"/> along the
/// breadth (0 to <c>dx</c>) and <see cref="HierarchyNode.Y"/> along the depth (0 to <c>dy</c>).
/// </summary>
internal static class ChartTreeLayout
{
    static double Separation(HierarchyNode a, HierarchyNode b) => ReferenceEquals(a.Parent, b.Parent) ? 1 : 2;

    sealed class TreeNode(HierarchyNode? node, int i)
    {
        public readonly HierarchyNode? Node = node;
        public TreeNode? Parent;
        public List<TreeNode>? Children;
        public TreeNode? A;
        public TreeNode Ancestor = null!;
        public double Z;
        public double M;
        public double C;
        public double S;
        public TreeNode? T;
        public readonly int I = i;
    }

    /// <summary>d3 <c>tree().size([dx, dy])</c>.</summary>
    public static void Tree(HierarchyNode root, double dx, double dy)
    {
        var t = TreeRoot(root);
        foreach (var v in PostOrder(t)) FirstWalk(v);
        t.Parent!.M = -t.Z;
        foreach (var v in PreOrder(t)) SecondWalk(v);

        HierarchyNode left = root, right = root, bottom = root;
        foreach (var node in root.PreOrder())
        {
            if (node.X < left.X) left = node;
            if (node.X > right.X) right = node;
            if (node.Depth > bottom.Depth) bottom = node;
        }
        var s = ReferenceEquals(left, right) ? 1 : Separation(left, right) / 2;
        var tx = s - left.X;
        var kx = dx / (right.X + s + tx);
        var ky = dy / (bottom.Depth == 0 ? 1 : bottom.Depth);
        foreach (var node in root.PreOrder())
        {
            node.X = (node.X + tx) * kx;
            node.Y = node.Depth * ky;
        }
    }

    /// <summary>d3 <c>cluster().size([dx, dy])</c>: every leaf at the same depth, parents centred over their children.</summary>
    public static void Cluster(HierarchyNode root, double dx, double dy)
    {
        HierarchyNode? previous = null;
        double x = 0;
        foreach (var node in root.PostOrder())
        {
            if (node.Children is { } children)
            {
                node.X = children.Average(c => c.X);
                node.Y = 1 + children.Max(c => c.Y);
            }
            else
            {
                node.X = previous is not null ? x += Separation(node, previous) : 0;
                node.Y = 0;
                previous = node;
            }
        }
        var leftLeaf = root;
        while (leftLeaf.Children is { } lc) leftLeaf = lc[0];
        var rightLeaf = root;
        while (rightLeaf.Children is { } rc) rightLeaf = rc[^1];
        var x0 = leftLeaf.X - Separation(leftLeaf, rightLeaf) / 2;
        var x1 = rightLeaf.X + Separation(rightLeaf, leftLeaf) / 2;
        var rootY = root.Y;
        foreach (var node in root.PostOrder())
        {
            node.X = (node.X - x0) / (x1 - x0) * dx;
            node.Y = (1 - (rootY != 0 ? node.Y / rootY : 1)) * dy;
        }
    }

    static TreeNode TreeRoot(HierarchyNode root)
    {
        var tree = new TreeNode(root, 0);
        tree.Ancestor = tree;
        var stack = new Stack<TreeNode>();
        stack.Push(tree);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node.Node!.Children is not { } children) continue;
            node.Children = new List<TreeNode>(children.Count);
            for (var i = 0; i < children.Count; i++)
            {
                var child = new TreeNode(children[i], i) { Parent = node };
                child.Ancestor = child;
                node.Children.Add(child);
            }
            for (var i = children.Count - 1; i >= 0; i--) stack.Push(node.Children[i]);
        }
        var holder = new TreeNode(null, 0) { Children = [tree] };
        holder.Ancestor = holder;
        tree.Parent = holder;
        return tree;
    }

    static IEnumerable<TreeNode> PreOrder(TreeNode root)
    {
        var stack = new Stack<TreeNode>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            yield return n;
            if (n.Children is { } ch)
                for (var i = ch.Count - 1; i >= 0; i--) stack.Push(ch[i]);
        }
    }

    static List<TreeNode> PostOrder(TreeNode root)
    {
        var list = new List<TreeNode>();
        var stack = new Stack<TreeNode>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            list.Add(n);
            if (n.Children is { } ch) foreach (var c in ch) stack.Push(c);
        }
        list.Reverse();
        return list;
    }

    static TreeNode? NextLeft(TreeNode v) => v.Children is { } c ? c[0] : v.T;

    static TreeNode? NextRight(TreeNode v) => v.Children is { } c ? c[^1] : v.T;

    static void MoveSubtree(TreeNode wm, TreeNode wp, double shift)
    {
        var change = shift / (wp.I - wm.I);
        wp.C -= change;
        wp.S += shift;
        wm.C += change;
        wp.Z += shift;
        wp.M += shift;
    }

    static void ExecuteShifts(TreeNode v)
    {
        double shift = 0, change = 0;
        var children = v.Children!;
        for (var i = children.Count - 1; i >= 0; i--)
        {
            var w = children[i];
            w.Z += shift;
            w.M += shift;
            shift += w.S + (change += w.C);
        }
    }

    static TreeNode NextAncestor(TreeNode vim, TreeNode v, TreeNode ancestor) =>
        ReferenceEquals(vim.Ancestor.Parent, v.Parent) ? vim.Ancestor : ancestor;

    static void FirstWalk(TreeNode v)
    {
        var children = v.Children;
        var siblings = v.Parent!.Children!;
        var w = v.I > 0 ? siblings[v.I - 1] : null;
        if (children is not null)
        {
            ExecuteShifts(v);
            var midpoint = (children[0].Z + children[^1].Z) / 2;
            if (w is not null)
            {
                v.Z = w.Z + Separation(v.Node!, w.Node!);
                v.M = v.Z - midpoint;
            }
            else
            {
                v.Z = midpoint;
            }
        }
        else if (w is not null)
        {
            v.Z = w.Z + Separation(v.Node!, w.Node!);
        }
        v.Parent.A = Apportion(v, w, v.Parent.A ?? siblings[0]);
    }

    static void SecondWalk(TreeNode v)
    {
        v.Node!.X = v.Z + v.Parent!.M;
        v.M += v.Parent.M;
    }

    static TreeNode Apportion(TreeNode v, TreeNode? w, TreeNode ancestor)
    {
        if (w is null) return ancestor;
        TreeNode? vip = v, vop = v, vim = w, vom = v.Parent!.Children![0];
        double sip = vip.M, sop = vop.M, sim = vim.M, som = vom.M;
        while (true)
        {
            vim = NextRight(vim!);
            vip = NextLeft(vip!);
            if (vim is null || vip is null) break;
            vom = NextLeft(vom!);
            vop = NextRight(vop!);
            vop!.Ancestor = v;
            var shift = vim.Z + sim - vip.Z - sip + Separation(vim.Node!, vip.Node!);
            if (shift > 0)
            {
                MoveSubtree(NextAncestor(vim, v, ancestor), v, shift);
                sip += shift;
                sop += shift;
            }
            sim += vim.M;
            sip += vip.M;
            som += vom!.M;
            sop += vop.M;
        }
        if (vim is not null && NextRight(vop!) is null)
        {
            vop!.T = vim;
            vop.M += sim - sop;
        }
        if (vip is not null && NextLeft(vom!) is null)
        {
            vom!.T = vip;
            vom.M += sip - som;
            ancestor = v;
        }
        return ancestor;
    }
}
