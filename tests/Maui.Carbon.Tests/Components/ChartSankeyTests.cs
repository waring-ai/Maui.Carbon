namespace Maui.Carbon.Tests.Components;

public class ChartSankeyTests
{
    internal static (string, string?)[] Nodes =>
    [
        ("Search", "Source"), ("Referral", "Source"), ("Social", "Source"), ("Email", "Source"),
        ("Trial", "Stage"), ("Demo", "Stage"), ("Contact sales", "Stage"), ("Won", "Outcome"), ("Lost", "Outcome"), ("Open", "Outcome"),
    ];

    internal static (string, string, double, object?)[] Flows =>
    [
        ("Search", "Trial", 420, null), ("Search", "Demo", 160, null), ("Search", "Contact sales", 70, null),
        ("Referral", "Demo", 190, null), ("Referral", "Contact sales", 110, null), ("Social", "Trial", 230, null), ("Social", "Demo", 40, null),
        ("Email", "Trial", 150, null), ("Email", "Contact sales", 60, null), ("Trial", "Won", 210, null), ("Trial", "Lost", 430, null),
        ("Trial", "Open", 160, null), ("Demo", "Won", 170, null), ("Demo", "Lost", 140, null), ("Demo", "Open", 80, null),
        ("Contact sales", "Won", 120, null), ("Contact sales", "Lost", 60, null), ("Contact sales", "Open", 60, null),
    ];

    // d3-sankey 0.12.3: nodeWidth(4).nodePadding(24).nodeAlign(sankeyJustify).extent([[2, 30], [598, 400]]).
    static readonly (string Name, double X0, double Y0, double Y1)[] ExpectedNodes =
    [
        ("Search", 2, 30, 165.454545),
        ("Referral", 2, 337.482517, 400),
        ("Social", 2, 189.454545, 245.72028),
        ("Email", 2, 269.72028, 313.482517),
        ("Trial", 298, 30, 196.713287),
        ("Demo", 298, 220.713287, 301.986014),
        ("Contact sales", 298, 325.986014, 376),
        ("Won", 594, 30, 134.195804),
        ("Lost", 594, 158.195804, 289.482517),
        ("Open", 594, 313.482517, 376),
    ];

    static readonly (string Source, string Target, double Y0, double Y1, double Width)[] ExpectedLinks =
    [
        ("Search", "Trial", 73.762238, 73.762238, 87.524476),
        ("Search", "Demo", 134.195804, 237.384615, 33.342657),
        ("Search", "Contact sales", 158.160839, 333.27972, 14.587413),
        ("Referral", "Demo", 357.27972, 282.188811, 39.594406),
        ("Referral", "Contact sales", 388.538462, 364.538462, 22.923077),
        ("Social", "Trial", 213.41958, 141.48951, 47.93007),
        ("Social", "Demo", 241.552448, 258.223776, 8.335664),
        ("Email", "Trial", 285.34965, 181.083916, 31.258741),
        ("Email", "Contact sales", 307.230769, 346.825175, 12.503497),
        ("Trial", "Won", 51.881119, 51.881119, 43.762238),
        ("Trial", "Lost", 118.566434, 203, 89.608392),
        ("Trial", "Open", 180.041958, 330.153846, 33.342657),
        ("Demo", "Won", 238.426573, 91.475524, 35.426573),
        ("Demo", "Lost", 270.727273, 262.391608, 29.174825),
        ("Demo", "Open", 293.65035, 355.160839, 16.671329),
        ("Contact sales", "Won", 338.48951, 121.692308, 25.006993),
        ("Contact sales", "Lost", 357.244755, 283.230769, 12.503497),
        ("Contact sales", "Open", 369.748252, 369.748252, 12.503497),
    ];

    static ChartSankey Carbon() => new() { X0 = 2, Y0 = 30, X1 = 598, Y1 = 400, NodeWidth = 4, NodePadding = 24 };

    [Fact]
    public void Matches_d3_sankey()
    {
        var (nodes, links) = Carbon().Layout(Nodes, Flows);
        for (var i = 0; i < nodes.Count; i++)
        {
            Assert.Equal(ExpectedNodes[i].Name, nodes[i].Name);
            Assert.Equal(ExpectedNodes[i].X0, nodes[i].X0, 1e-4);
            Assert.Equal(ExpectedNodes[i].Y0, nodes[i].Y0, 1e-4);
            Assert.Equal(ExpectedNodes[i].Y1, nodes[i].Y1, 1e-4);
        }
        for (var i = 0; i < links.Count; i++)
        {
            Assert.Equal(ExpectedLinks[i].Source, links[i].Source.Name);
            Assert.Equal(ExpectedLinks[i].Target, links[i].Target.Name);
            Assert.Equal(ExpectedLinks[i].Y0, links[i].Y0, 1e-4);
            Assert.Equal(ExpectedLinks[i].Y1, links[i].Y1, 1e-4);
            Assert.Equal(ExpectedLinks[i].Width, links[i].Width, 1e-4);
        }
    }

    [Fact]
    public void Node_values_are_the_larger_of_inflow_and_outflow_and_columns_follow_depth()
    {
        var (nodes, _) = Carbon().Layout(Nodes, Flows);
        var trial = nodes.Single(n => n.Name == "Trial");
        Assert.Equal(800, trial.Value);
        Assert.Equal(1, trial.Depth);
        Assert.Equal(1, trial.Height);
        Assert.Equal(598, nodes.Single(n => n.Name == "Won").X1);
        Assert.All(nodes, n => Assert.Equal(4, n.X1 - n.X0, 9));
    }

    [Fact]
    public void Alignments_place_sinks_and_sources()
    {
        (string, string?)[] nodes = [("a", null), ("b", null), ("c", null), ("d", null)];
        (string, string, double, object?)[] links = [("a", "b", 1, null), ("b", "c", 1, null), ("a", "d", 1, null)];
        var justify = new ChartSankey { X1 = 100, NodeWidth = 4 }.Layout(nodes, links).Nodes;
        Assert.Equal(96, justify.Single(n => n.Name == "d").X0, 9);
        var left = new ChartSankey { X1 = 100, NodeWidth = 4, Align = SankeyAlign.Left }.Layout(nodes, links).Nodes;
        Assert.Equal(48, left.Single(n => n.Name == "d").X0, 9);
        var right = new ChartSankey { X1 = 100, NodeWidth = 4, Align = SankeyAlign.Right }.Layout(nodes, links).Nodes;
        Assert.Equal(96, right.Single(n => n.Name == "d").X0, 9);
        Assert.Equal(0, right.Single(n => n.Name == "a").X0, 9);
    }

    [Fact]
    public void Missing_nodes_and_cycles_throw_like_d3()
    {
        Assert.Throws<InvalidOperationException>(() => Carbon().Layout([("a", null)], [("a", "x", 1, null)]));
        Assert.Throws<InvalidOperationException>(() => Carbon().Layout([("a", null), ("b", null)], [("a", "b", 1, null), ("b", "a", 1, null)]));
    }
}
