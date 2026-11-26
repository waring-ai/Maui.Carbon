namespace Maui.Carbon.Tests.Components;

public class ChartTreeLayoutTests
{
    static CdsChartNode N(string name, params CdsChartNode[] children) => new(name, children);

    internal static List<CdsChartNode> Catalog() =>
    [
        N("Platform", N("Compute", N("Virtual servers"), N("Kubernetes"), N("Functions")), N("Storage", N("Object"), N("Block")), N("Networking", N("Load balancers"), N("DNS"))),
        N("Data", N("Databases", N("PostgreSQL"), N("Redis")), N("Streaming", N("Event streams"))),
        N("AI", N("Inference"), N("Training", N("Fine-tuning"), N("Evaluation"))),
        N("Security", N("Identity"), N("Key management")),
    ];

    // d3-hierarchy 3.1.3: tree().size([400, 300]) and cluster().size([400, 300]), pre-order.
    static readonly (string Name, double X, double Y)[] Tree =
    [
        ("Cloud", 235.714286, 0),
        ("Platform", 100, 100),
        ("Compute", 38.095238, 200),
        ("Virtual servers", 19.047619, 300),
        ("Kubernetes", 38.095238, 300),
        ("Functions", 57.142857, 300),
        ("Storage", 104.761905, 200),
        ("Object", 95.238095, 300),
        ("Block", 114.285714, 300),
        ("Networking", 161.904762, 200),
        ("Load balancers", 152.380952, 300),
        ("DNS", 171.428571, 300),
        ("Data", 242.857143, 100),
        ("Databases", 219.047619, 200),
        ("PostgreSQL", 209.52381, 300),
        ("Redis", 228.571429, 300),
        ("Streaming", 266.666667, 200),
        ("Event streams", 266.666667, 300),
        ("AI", 314.285714, 100),
        ("Inference", 304.761905, 200),
        ("Training", 323.809524, 200),
        ("Fine-tuning", 314.285714, 300),
        ("Evaluation", 333.333333, 300),
        ("Security", 371.428571, 100),
        ("Identity", 361.904762, 200),
        ("Key management", 380.952381, 200),
    ];

    static readonly (string Name, double X, double Y)[] Cluster =
    [
        ("Cloud", 247.101449, 0),
        ("Platform", 92.753623, 100),
        ("Compute", 34.782609, 200),
        ("Virtual servers", 17.391304, 300),
        ("Kubernetes", 34.782609, 300),
        ("Functions", 52.173913, 300),
        ("Storage", 95.652174, 200),
        ("Object", 86.956522, 300),
        ("Block", 104.347826, 300),
        ("Networking", 147.826087, 200),
        ("Load balancers", 139.130435, 300),
        ("DNS", 156.521739, 300),
        ("Data", 221.73913, 100),
        ("Databases", 200, 200),
        ("PostgreSQL", 191.304348, 300),
        ("Redis", 208.695652, 300),
        ("Streaming", 243.478261, 200),
        ("Event streams", 243.478261, 300),
        ("AI", 300, 100),
        ("Inference", 278.26087, 300),
        ("Training", 321.73913, 200),
        ("Fine-tuning", 313.043478, 300),
        ("Evaluation", 330.434783, 300),
        ("Security", 373.913043, 200),
        ("Identity", 365.217391, 300),
        ("Key management", 382.608696, 300),
    ];

    [Fact]
    public void Tree_matches_d3_tidy_tree()
    {
        var root = HierarchyNode.Build("Cloud", Catalog());
        ChartTreeLayout.Tree(root, 400, 300);
        var nodes = root.PreOrder().ToList();
        Assert.Equal(Tree.Length, nodes.Count);
        for (var i = 0; i < nodes.Count; i++)
        {
            Assert.Equal(Tree[i].Name, nodes[i].Name);
            Assert.Equal(Tree[i].X, nodes[i].X, 1e-4);
            Assert.Equal(Tree[i].Y, nodes[i].Y, 1e-4);
        }
    }

    [Fact]
    public void Cluster_matches_d3_dendrogram()
    {
        var root = HierarchyNode.Build("Cloud", Catalog());
        ChartTreeLayout.Cluster(root, 400, 300);
        var nodes = root.PreOrder().ToList();
        for (var i = 0; i < nodes.Count; i++)
        {
            Assert.Equal(Cluster[i].Name, nodes[i].Name);
            Assert.Equal(Cluster[i].X, nodes[i].X, 1e-4);
            Assert.Equal(Cluster[i].Y, nodes[i].Y, 1e-4);
        }
        Assert.All(root.Leaves(), l => Assert.Equal(300, l.Y, 1e-9));
    }

    [Fact]
    public void Single_nodes_and_chains_are_centred()
    {
        var root = HierarchyNode.Build("r", []);
        ChartTreeLayout.Tree(root, 100, 50);
        Assert.Equal(50, root.X, 6);
        Assert.Equal(0, root.Y);
        var chain = HierarchyNode.Build("r", [N("a", N("b"))]);
        ChartTreeLayout.Tree(chain, 100, 50);
        Assert.All(chain.PreOrder(), n => Assert.Equal(50, n.X, 6));
        Assert.Equal(50, chain.Leaves().Single().Y, 6);
    }

    [Fact]
    public void Siblings_never_overlap_and_cousins_are_spaced_wider()
    {
        var root = HierarchyNode.Build("Cloud", Catalog());
        ChartTreeLayout.Tree(root, 400, 300);
        foreach (var level in root.PreOrder().GroupBy(n => n.Depth))
        {
            var xs = level.Select(n => n.X).ToList();
            for (var i = 1; i < xs.Count; i++) Assert.True(xs[i] > xs[i - 1]);
        }
    }
}
