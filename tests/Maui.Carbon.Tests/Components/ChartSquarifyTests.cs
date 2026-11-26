namespace Maui.Carbon.Tests.Components;

public class ChartSquarifyTests
{
    static CdsChartNode N(string name, params CdsChartNode[] children) => new(name, children);

    static CdsChartNode L(string name, double value) => new(name, value);

    static List<CdsChartNode> Spend() =>
    [
        N("Compute", L("Virtual servers", 412), L("Kubernetes", 286), L("Functions", 64), L("Bare metal", 118)),
        N("Storage", L("Object storage", 204), L("Block storage", 132), L("File storage", 41)),
        N("Databases", L("PostgreSQL", 176), L("Redis", 58), L("MongoDB", 47)),
        N("Networking", L("Load balancers", 83), L("Transit gateway", 36), L("CDN", 29)),
        N("AI", L("Model inference", 154), L("Training", 97)),
    ];

    // d3-hierarchy 3.1.3: treemap().size([600, 400]).paddingInner(1).paddingOuter(0).round(true), sorted by value.
    static readonly (string Name, double X0, double Y0, double X1, double Y1)[] Expected =
    [
        ("Virtual servers", 0, 0, 308, 165),
        ("Kubernetes", 0, 166, 308, 280),
        ("Bare metal", 309, 0, 389, 181),
        ("Functions", 309, 182, 389, 280),
        ("Object storage", 0, 281, 210, 400),
        ("Block storage", 211, 281, 347, 400),
        ("File storage", 348, 281, 389, 400),
        ("PostgreSQL", 390, 0, 500, 195),
        ("Redis", 390, 196, 451, 313),
        ("MongoDB", 452, 196, 500, 313),
        ("Model inference", 501, 0, 600, 191),
        ("Training", 501, 192, 600, 313),
        ("Load balancers", 390, 314, 507, 400),
        ("Transit gateway", 508, 314, 600, 361),
        ("CDN", 508, 362, 600, 400),
    ];

    [Fact]
    public void Matches_d3_treemap_with_inner_padding_and_rounding()
    {
        var root = HierarchyNode.Build("r", Spend()).Sum().SortByValueDescending();
        ChartSquarify.Treemap(root, 600, 400);
        var leaves = root.Leaves().ToList();
        Assert.Equal(Expected.Length, leaves.Count);
        for (var i = 0; i < leaves.Count; i++)
        {
            Assert.Equal(Expected[i].Name, leaves[i].Name);
            Assert.Equal(Expected[i].X0, leaves[i].X0);
            Assert.Equal(Expected[i].Y0, leaves[i].Y0);
            Assert.Equal(Expected[i].X1, leaves[i].X1);
            Assert.Equal(Expected[i].Y1, leaves[i].Y1);
        }
    }

    [Fact]
    public void Sum_adds_own_values_and_sort_is_stable()
    {
        var root = HierarchyNode.Build("r", [N("a", L("x", 1), L("y", 2)), new CdsChartNode("b", 4), N("c", L("z", 3)), new CdsChartNode("d", 3)]).Sum().SortByValueDescending();
        Assert.Equal(13, root.Value);
        Assert.Equal(["b", "a", "c", "d"], root.Children!.Select(c => c.Name));
        var parent = new CdsChartNode("p", 5) { Children = [L("q", 1)] };
        Assert.Equal(6, HierarchyNode.Build("r", [parent]).Sum().Children![0].Value);
        Assert.Equal(["y", "x"], root.Children![1].Children!.Select(c => c.Name));
    }

    [Fact]
    public void Squarify_keeps_rows_near_the_golden_ratio_and_fills_the_box()
    {
        var root = HierarchyNode.Build("r", Enumerable.Range(1, 12).Select(i => L("n" + i, 13 - i))).Sum().SortByValueDescending();
        ChartSquarify.Treemap(root, 300, 200, paddingInner: 0, round: false);
        var leaves = root.Leaves().ToList();
        Assert.Equal(300 * 200, leaves.Sum(l => (l.X1 - l.X0) * (l.Y1 - l.Y0)), 3);
        foreach (var l in leaves)
            Assert.Equal(l.Value / root.Value * 60000, (l.X1 - l.X0) * (l.Y1 - l.Y0), 3);
        // Squarified cells stay far from slivers.
        Assert.All(leaves, l => Assert.InRange(Math.Max((l.X1 - l.X0) / (l.Y1 - l.Y0), (l.Y1 - l.Y0) / (l.X1 - l.X0)), 1, 4));
    }

    [Fact]
    public void Zero_values_and_empty_boxes_do_not_break_the_layout()
    {
        var root = HierarchyNode.Build("r", [L("a", 0), L("b", 5), L("c", 0)]).Sum().SortByValueDescending();
        ChartSquarify.Treemap(root, 100, 50);
        Assert.Equal(100, root.Children![0].X1 - root.Children[0].X0);
        var tiny = HierarchyNode.Build("r", [L("a", 1), L("b", 1)]).Sum();
        ChartSquarify.Treemap(tiny, 0, 0);
        Assert.All(tiny.Leaves(), l => Assert.True(l.X1 >= l.X0 && l.Y1 >= l.Y0));
        Assert.Equal(3, ChartSquarify.JsRound(2.5));
        Assert.Equal(-2, ChartSquarify.JsRound(-2.5));
    }
}
