namespace Maui.Carbon.Tests.Components;

public class TreeViewTests
{
    sealed class N : ITreeNodeData
    {
        public N(string name, bool expanded = false, bool disabled = false, params N[] children)
        {
            Name = name;
            IsExpanded = expanded;
            IsDisabled = disabled;
            Kids = children;
        }

        public string Name { get; }
        public bool IsExpanded { get; set; }
        public bool IsDisabled { get; }
        public N[] Kids { get; }
        public IReadOnlyList<ITreeNodeData> ChildNodes => Kids;
    }

    // 2026 (open)
    //   03 (open)
    //     a
    //     b
    //   02
    //   01 (disabled)
    // 2025 (closed)
    //   12
    // notes
    static N[] Tree() =>
    [
        new("2026", true, false,
            new N("03", true, false, new N("a"), new N("b")),
            new N("02"),
            new N("01", disabled: true)),
        new("2025", false, false, new N("12")),
        new("notes"),
    ];

    static string[] Names(IEnumerable<TreeRow> rows) => rows.Select(r => ((N)r.Node).Name).ToArray();

    static List<TreeRow> Rows(N[]? tree = null) => TreeViewModel.Flatten(tree ?? Tree());

    static int Index(List<TreeRow> rows, string name) => rows.FindIndex(r => ((N)r.Node).Name == name);

    // ----- flattening -----

    [Fact]
    public void Only_children_of_expanded_parents_are_visible_depth_first()
    {
        var rows = Rows();
        Assert.Equal(["2026", "03", "a", "b", "02", "01", "2025", "notes"], Names(rows));
        Assert.Equal([0, 1, 2, 2, 1, 1, 0, 0], rows.Select(r => r.Depth));
    }

    [Fact]
    public void Parent_indices_point_at_the_parent_row()
    {
        var rows = Rows();
        Assert.Equal(-1, rows[0].ParentIndex);
        Assert.Equal(0, rows[1].ParentIndex);
        Assert.Equal(1, rows[2].ParentIndex);
        Assert.Equal(1, rows[3].ParentIndex);
        Assert.Equal(0, rows[4].ParentIndex);
    }

    [Fact]
    public void Collapsing_a_parent_hides_the_whole_subtree()
    {
        var tree = Tree();
        tree[0].IsExpanded = false;
        Assert.Equal(["2026", "2025", "notes"], Names(Rows(tree)));
    }

    [Fact]
    public void An_empty_tree_flattens_to_nothing()
    {
        Assert.Empty(TreeViewModel.Flatten([]));
    }

    [Fact]
    public void Parents_and_open_state_are_reported()
    {
        var rows = Rows();
        Assert.True(rows[0].HasChildren);
        Assert.True(rows[0].IsOpen);
        Assert.True(rows[6].HasChildren);
        Assert.False(rows[6].IsOpen);
        Assert.False(rows[7].HasChildren);
    }

    // ----- keyboard -----

    [Fact]
    public void Down_and_up_move_through_visible_rows_and_skip_disabled_ones()
    {
        var rows = Rows();
        Assert.Equal(1, TreeViewModel.HandleKey(rows, 0, CdsKey.Down).FocusIndex);
        Assert.Equal(Index(rows, "2025"), TreeViewModel.HandleKey(rows, Index(rows, "02"), CdsKey.Down).FocusIndex); // skips 01
        Assert.Equal(Index(rows, "02"), TreeViewModel.HandleKey(rows, Index(rows, "2025"), CdsKey.Up).FocusIndex);
    }

    [Fact]
    public void Down_at_the_end_and_up_at_the_start_stay_put_and_are_handled()
    {
        var rows = Rows();
        var down = TreeViewModel.HandleKey(rows, rows.Count - 1, CdsKey.Down);
        Assert.True(down.Handled);
        Assert.Equal(-1, down.FocusIndex);
        var up = TreeViewModel.HandleKey(rows, 0, CdsKey.Up);
        Assert.True(up.Handled);
        Assert.Equal(-1, up.FocusIndex);
    }

    [Fact]
    public void From_no_focus_down_goes_to_the_first_and_up_to_the_last_enabled_row()
    {
        var rows = Rows();
        Assert.Equal(0, TreeViewModel.HandleKey(rows, -1, CdsKey.Down).FocusIndex);
        Assert.Equal(rows.Count - 1, TreeViewModel.HandleKey(rows, -1, CdsKey.Up).FocusIndex);
    }

    [Fact]
    public void Home_and_end_jump_to_the_first_and_last_enabled_rows()
    {
        var rows = Rows();
        Assert.Equal(0, TreeViewModel.HandleKey(rows, 5, CdsKey.Home).FocusIndex);
        Assert.Equal(rows.Count - 1, TreeViewModel.HandleKey(rows, 0, CdsKey.End).FocusIndex);
    }

    [Fact]
    public void End_skips_a_disabled_last_row()
    {
        var tree = new N[] { new("a"), new("b"), new("c", disabled: true) };
        var rows = Rows(tree);
        Assert.Equal(1, TreeViewModel.HandleKey(rows, 0, CdsKey.End).FocusIndex);
    }

    [Fact]
    public void Right_expands_a_closed_parent_first()
    {
        var rows = Rows();
        var result = TreeViewModel.HandleKey(rows, Index(rows, "2025"), CdsKey.Right);
        Assert.True(result.Handled);
        Assert.Equal(Index(rows, "2025"), result.ToggleIndex);
        Assert.True(result.Expand);
        Assert.Equal(-1, result.FocusIndex);
    }

    [Fact]
    public void Right_on_an_open_parent_moves_to_its_first_enabled_child()
    {
        var rows = Rows();
        Assert.Equal(Index(rows, "03"), TreeViewModel.HandleKey(rows, 0, CdsKey.Right).FocusIndex);
        Assert.Equal(Index(rows, "a"), TreeViewModel.HandleKey(rows, Index(rows, "03"), CdsKey.Right).FocusIndex);
    }

    [Fact]
    public void Right_on_a_leaf_is_not_handled()
    {
        var rows = Rows();
        Assert.False(TreeViewModel.HandleKey(rows, Index(rows, "notes"), CdsKey.Right).Handled);
    }

    [Fact]
    public void Left_collapses_an_open_parent_then_moves_to_the_parent()
    {
        var rows = Rows();
        var collapse = TreeViewModel.HandleKey(rows, Index(rows, "03"), CdsKey.Left);
        Assert.Equal(Index(rows, "03"), collapse.ToggleIndex);
        Assert.False(collapse.Expand);
        var toParent = TreeViewModel.HandleKey(rows, Index(rows, "a"), CdsKey.Left);
        Assert.Equal(Index(rows, "03"), toParent.FocusIndex);
        Assert.Equal(-1, toParent.ToggleIndex);
    }

    [Fact]
    public void Left_on_a_collapsed_root_is_not_handled()
    {
        var rows = Rows();
        Assert.False(TreeViewModel.HandleKey(rows, Index(rows, "notes"), CdsKey.Left).Handled);
        Assert.False(TreeViewModel.HandleKey(rows, Index(rows, "2025"), CdsKey.Left).Handled);
    }

    [Fact]
    public void Enter_toggles_a_parent_and_selects_and_space_only_selects()
    {
        var rows = Rows();
        var enterParent = TreeViewModel.HandleKey(rows, Index(rows, "2025"), CdsKey.Enter);
        Assert.True(enterParent.Select);
        Assert.Equal(Index(rows, "2025"), enterParent.ToggleIndex);
        Assert.True(enterParent.Expand);
        var enterLeaf = TreeViewModel.HandleKey(rows, Index(rows, "notes"), CdsKey.Enter);
        Assert.True(enterLeaf.Select);
        Assert.Equal(-1, enterLeaf.ToggleIndex);
        var space = TreeViewModel.HandleKey(rows, Index(rows, "2025"), CdsKey.Space);
        Assert.True(space.Select);
        Assert.Equal(-1, space.ToggleIndex);
    }

    [Fact]
    public void Disabled_rows_cannot_be_selected_from_the_keyboard()
    {
        var rows = Rows();
        var result = TreeViewModel.HandleKey(rows, Index(rows, "01"), CdsKey.Space);
        Assert.False(result.Select);
    }

    [Fact]
    public void Other_keys_and_an_empty_tree_do_nothing()
    {
        var rows = Rows();
        Assert.False(TreeViewModel.HandleKey(rows, 0, CdsKey.Tab).Handled);
        Assert.False(TreeViewModel.HandleKey(rows, 0, CdsKey.Escape).Handled);
        Assert.False(TreeViewModel.HandleKey([], 0, CdsKey.Down).Handled);
        Assert.False(TreeViewModel.HandleKey(rows, -1, CdsKey.Space).Handled);
    }

    [Fact]
    public void Right_on_a_parent_with_only_disabled_children_stays_put()
    {
        var tree = new N[] { new("p", true, false, new N("x", disabled: true)) };
        var rows = Rows(tree);
        var result = TreeViewModel.HandleKey(rows, 0, CdsKey.Right);
        Assert.True(result.Handled);
        Assert.Equal(-1, result.FocusIndex);
    }

    // ----- tab stop and metrics -----

    [Fact]
    public void The_tab_stop_is_the_active_node_else_the_first_selected_else_the_first_enabled()
    {
        var tree = Tree();
        var rows = Rows(tree);
        var active = tree[0].Kids[1]; // 02
        Assert.Equal(Index(rows, "02"), TreeViewModel.TabStopIndex(rows, active, _ => false));
        Assert.Equal(Index(rows, "notes"), TreeViewModel.TabStopIndex(rows, null, n => ReferenceEquals(n, tree[2])));
        Assert.Equal(0, TreeViewModel.TabStopIndex(rows, null, _ => false));
        // A disabled active node cannot hold the tab stop.
        Assert.Equal(0, TreeViewModel.TabStopIndex(rows, tree[0].Kids[2], _ => false));
        Assert.Equal(-1, TreeViewModel.TabStopIndex([], null, _ => false));
    }

    [Fact]
    public void IndexOf_finds_visible_nodes_only()
    {
        var tree = Tree();
        var rows = Rows(tree);
        Assert.Equal(0, TreeViewModel.IndexOf(rows, tree[0]));
        Assert.Equal(-1, TreeViewModel.IndexOf(rows, tree[1].Kids[0])); // hidden inside a closed parent
    }

    [Theory]
    [InlineData(0, false, 16)]
    [InlineData(1, false, 32)]
    [InlineData(2, false, 48)]
    [InlineData(0, true, 16)]
    [InlineData(1, true, 40)]
    [InlineData(2, true, 64)]
    public void Toggle_x_grows_16_per_level_and_24_with_icons(int depth, bool icons, double x) =>
        Assert.Equal(x, TreeViewModel.ToggleX(depth, icons));

    [Theory]
    [InlineData(0, false, 44)]
    [InlineData(1, false, 60)]
    [InlineData(0, true, 68)]
    [InlineData(1, true, 92)]
    public void Text_starts_after_the_toggle_slot_and_the_icon(int depth, bool icons, double x) =>
        Assert.Equal(x, TreeViewModel.TextX(depth, icons));

    [Fact]
    public void Row_heights()
    {
        Assert.Equal(24, TreeViewModel.RowHeight(CdsTreeViewSize.Xs));
        Assert.Equal(32, TreeViewModel.RowHeight(CdsTreeViewSize.Sm));
    }

    // ----- tokens -----

    [Fact]
    public void Row_fill_follows_selected_hover_and_disabled()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsTreeViewTokens.Fill(false, false, false));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsTreeViewTokens.Fill(false, true, false));
        Assert.Equal(CdsColorKeys.LayerSelected01, CdsTreeViewTokens.Fill(true, false, false));
        Assert.Equal(CdsColorKeys.LayerSelectedHover01, CdsTreeViewTokens.Fill(true, true, false));
        Assert.Equal(CdsColorKeys.Field01, CdsTreeViewTokens.Fill(true, true, true));
    }

    [Fact]
    public void Text_and_icon_colors()
    {
        Assert.Equal(CdsColorKeys.TextSecondary, CdsTreeViewTokens.Text(false, false, false));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsTreeViewTokens.Text(true, false, false));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsTreeViewTokens.Text(false, true, false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsTreeViewTokens.Text(true, true, true));
        Assert.Equal(CdsColorKeys.IconSecondary, CdsTreeViewTokens.Icon(false, false, false));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsTreeViewTokens.Icon(true, false, false));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsTreeViewTokens.Icon(false, true, true));
        Assert.Equal(CdsColorKeys.Interactive, CdsTreeViewTokens.ActiveBar);
        Assert.Equal(4, CdsTreeViewTokens.ActiveBarWidth);
    }

    [Fact]
    public void The_chevron_points_right_collapsed_and_down_expanded()
    {
        Assert.Equal(-90, CdsTreeViewTokens.ChevronRotation(false));
        Assert.Equal(0, CdsTreeViewTokens.ChevronRotation(true));
    }

    // ----- the controls -----

    static CdsTreeView CreateTree()
    {
        var tree = new CdsTreeView { Label = "Folders in logs-archive" };
        tree.Nodes.Add(new CdsTreeNode("2026", CdsIconName.Folder)
        {
            Id = "y2026",
            IsExpanded = true,
            Children =
            {
                new CdsTreeNode("03 (March)", CdsIconName.Folder) { Id = "m03" },
                new CdsTreeNode("02 (February)", CdsIconName.Folder) { Id = "m02", IsDisabled = true },
            },
        });
        tree.Nodes.Add(new CdsTreeNode("notes.txt", CdsIconName.Document) { Id = "notes" });
        return tree;
    }

    [Fact]
    public void A_tree_can_be_created_and_configured_without_a_platform()
    {
        var tree = CreateTree();
        tree.Size = CdsTreeViewSize.Xs;
        tree.HideLabel = true;
        tree.IsMultiselect = true;
        tree.ActiveNode = tree.FindNode("m03");
        tree.SelectedNode = tree.FindNode("m03");
        Assert.Equal(CdsTreeViewSize.Xs, tree.Size);
        Assert.Same(tree.FindNode("m03"), tree.SelectedNodes.Single());
        Assert.Null(tree.FindNode("nope"));
    }

    [Fact]
    public void Selecting_nodes_raises_events_and_activates_the_first()
    {
        var tree = CreateTree();
        var selections = 0;
        var activations = 0;
        tree.SelectionChanged += (_, _) => selections++;
        tree.NodeActivated += (_, _) => activations++;
        tree.Select(tree.FindNode("m03")!);
        Assert.Equal(1, selections);
        Assert.Equal(1, activations);
        Assert.Same(tree.FindNode("m03"), tree.ActiveNode);
        Assert.Same(tree.FindNode("m03"), tree.SelectedNode);
        tree.Select(tree.FindNode("m03")!);
        Assert.Equal(1, selections); // unchanged
    }

    [Fact]
    public void Disabled_nodes_cannot_be_selected_and_single_select_keeps_one()
    {
        var tree = CreateTree();
        tree.Select(tree.FindNode("m02")!);
        Assert.Empty(tree.SelectedNodes);
        tree.Select(tree.FindNode("m03")!, tree.FindNode("notes")!);
        Assert.Single(tree.SelectedNodes);
    }

    [Fact]
    public void Multiselect_keeps_several_nodes()
    {
        var tree = CreateTree();
        tree.IsMultiselect = true;
        tree.Select(tree.FindNode("m03")!, tree.FindNode("notes")!);
        Assert.Equal(2, tree.SelectedNodes.Count);
        tree.IsMultiselect = false;
        Assert.Single(tree.SelectedNodes);
    }

    [Fact]
    public void Expanding_a_node_raises_the_toggled_events_up_the_tree()
    {
        var tree = CreateTree();
        var toggled = new List<string?>();
        tree.NodeToggled += (_, e) => toggled.Add(e.Node.Id);
        var nodeEvents = 0;
        tree.FindNode("y2026")!.Toggled += (_, _) => nodeEvents++;
        tree.FindNode("y2026")!.IsExpanded = false;
        tree.FindNode("y2026")!.IsExpanded = true;
        Assert.Equal(["y2026", "y2026"], toggled);
        Assert.Equal(2, nodeEvents);
    }

    [Fact]
    public void ExpandAll_and_CollapseAll_reach_nested_parents()
    {
        var tree = CreateTree();
        tree.FindNode("y2026")!.Children.Add(new CdsTreeNode("x") { Id = "x", Children = { new CdsTreeNode("y") { Id = "y" } } });
        tree.ExpandAll();
        Assert.True(tree.FindNode("x")!.IsExpanded);
        tree.CollapseAll();
        Assert.False(tree.FindNode("y2026")!.IsExpanded);
        Assert.False(tree.FindNode("x")!.IsExpanded);
    }

    [Fact]
    public void A_node_with_children_is_a_parent_and_knows_its_parent()
    {
        var tree = CreateTree();
        var root = tree.FindNode("y2026")!;
        Assert.True(root.IsParent);
        Assert.False(tree.FindNode("notes")!.IsParent);
        Assert.Same(root, tree.FindNode("m03")!.ParentNode);
        Assert.Null(root.ParentNode);
        root.Children.Clear();
        Assert.False(root.IsParent);
    }

    [Fact]
    public void Removing_a_selected_node_drops_it_from_the_selection()
    {
        var tree = CreateTree();
        tree.Select(tree.FindNode("notes")!);
        tree.Nodes.Remove(tree.FindNode("notes")!);
        Assert.Empty(tree.SelectedNodes);
    }

    [Fact]
    public void The_tree_wants_navigation_keys()
    {
        var tree = CreateTree();
        Assert.True(tree.WantsNavigationKeys);
        Assert.False(tree.OnKeyDown(new CdsKeyEventArgs(CdsKey.Tab)));
    }
}
