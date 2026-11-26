namespace Maui.Carbon.Tests.Components;

public class ChartPackTests
{
    static CdsChartNode N(string name, params CdsChartNode[] children) => new(name, children);

    static CdsChartNode L(string name, double value) => new(name, value);

    static List<CdsChartNode> Sales() =>
    [
        N("North America", L("United States", 3120), L("Canada", 640), L("Mexico", 410)),
        N("Europe", L("Germany", 980), L("United Kingdom", 870), L("France", 720), L("Netherlands", 310), L("Spain", 280)),
        N("Asia Pacific", L("Japan", 1150), L("Australia", 520), L("India", 470), L("Singapore", 260)),
        N("Latin America", L("Brazil", 540), L("Chile", 190), L("Colombia", 150)),
    ];

    // d3-hierarchy 3.1.3: pack().size([600, 400]).padding(d => (d.depth >= 1 ? 2 : 4) + 3), descendants() order.
    static readonly (string Name, double X, double Y, double R)[] Expected =
    [
        ("North America", 206.543416, 176.688872, 97.579055),
        ("Europe", 400.73015, 176.688872, 90.506732),
        ("Asia Pacific", 309.951561, 320.245198, 73.242758),
        ("Latin America", 309.04694, 64.33822, 48.40432),
        ("United States", 175.9842, 176.688872, 62.662019),
        ("Canada", 271.384345, 176.688872, 28.380306),
        ("Mexico", 249.770787, 227.756863, 22.715328),
        ("Germany", 380.96595, 171.462886, 35.118837),
        ("United Kingdom", 453.531841, 171.462886, 33.089234),
        ("France", 419.166561, 229.616849, 30.10186),
        ("Netherlands", 418.877081, 125.957249, 19.751857),
        ("Spain", 377.907787, 113.294757, 18.771808),
        ("Japan", 279.97721, 312.981817, 38.04311),
        ("Australia", 347.959802, 312.981817, 25.581662),
        ("India", 325.056701, 362.171399, 24.320697),
        ("Singapore", 323.914403, 271.405975, 18.088967),
        ("Brazil", 291.482149, 60.508138, 26.068976),
        ("Chile", 337.372305, 60.508138, 15.463361),
        ("Colombia", 323.408931, 91.026121, 13.739557),
    ];

    [Fact]
    public void Matches_d3_pack_with_carbon_padding()
    {
        var root = HierarchyNode.Build("", Sales()).Sum().SortByValueDescending();
        ChartPack.Pack(root, 600, 400, d => (d.Depth >= 1 ? 2 : 4) + 3);
        var nodes = root.Descendants().Skip(1).ToList();
        Assert.Equal(Expected.Length, nodes.Count);
        for (var i = 0; i < nodes.Count; i++)
        {
            Assert.Equal(Expected[i].Name, nodes[i].Name);
            Assert.Equal(Expected[i].X, nodes[i].X, 1e-4);
            Assert.Equal(Expected[i].Y, nodes[i].Y, 1e-4);
            Assert.Equal(Expected[i].R, nodes[i].R, 1e-4);
        }
    }

    [Fact]
    public void Children_sit_inside_parents_and_siblings_keep_their_padding()
    {
        var root = HierarchyNode.Build("", Sales()).Sum().SortByValueDescending();
        ChartPack.Pack(root, 600, 400, d => (d.Depth >= 1 ? 2 : 4) + 3);
        Assert.Equal(200, root.R, 1e-6);
        foreach (var n in root.PreOrder())
        {
            if (n.Children is not { } kids) continue;
            foreach (var c in kids)
            {
                var d = Math.Sqrt((c.X - n.X) * (c.X - n.X) + (c.Y - n.Y) * (c.Y - n.Y));
                Assert.True(d + c.R <= n.R + 1e-6);
                foreach (var o in kids)
                {
                    if (ReferenceEquals(o, c)) continue;
                    var g = Math.Sqrt((c.X - o.X) * (c.X - o.X) + (c.Y - o.Y) * (c.Y - o.Y)) - c.R - o.R;
                    // d3 scales the padding with the final radius, so gaps are near (not exactly) the padding.
                    Assert.True(g >= (n.Depth >= 1 ? 5 : 7) * 0.5);
                }
            }
        }
    }

    [Fact]
    public void Siblings_pack_tangent_and_enclose_finds_the_smallest_circle()
    {
        var circles = new[] { new PackCircle(0, 0, 10), new PackCircle(0, 0, 10) };
        Assert.Equal(20, ChartPack.PackSiblings(circles, ChartPack.Lcg()), 6);
        Assert.Equal(20, Math.Abs(circles[0].X - circles[1].X), 6);
        var one = new[] { new PackCircle(5, 5, 3) };
        Assert.Equal(3, ChartPack.PackSiblings(one, ChartPack.Lcg()));
        Assert.Equal(0, ChartPack.PackSiblings([], ChartPack.Lcg()));
        var e = ChartPack.Enclose([new PackCircle(-10, 0, 1), new PackCircle(10, 0, 1), new PackCircle(0, 3, 1)], ChartPack.Lcg());
        Assert.Equal(0, e.X, 6);
        Assert.Equal(11, e.R, 6);
        var many = Enumerable.Range(1, 30).Select(i => new PackCircle(0, 0, 1 + i % 7)).ToArray();
        var r = ChartPack.PackSiblings(many, ChartPack.Lcg());
        for (var i = 0; i < many.Length; i++)
        {
            Assert.True(Math.Sqrt(many[i].X * many[i].X + many[i].Y * many[i].Y) + many[i].R <= r + 1e-6);
            for (var j = i + 1; j < many.Length; j++)
                Assert.True(Math.Sqrt(Math.Pow(many[i].X - many[j].X, 2) + Math.Pow(many[i].Y - many[j].Y, 2)) >= many[i].R + many[j].R - 1e-6);
        }
    }

    [Fact]
    public void Lcg_matches_d3()
    {
        var r = ChartPack.Lcg();
        Assert.Equal((1664525.0 + 1013904223) / 4294967296, r(), 12);
        Assert.InRange(r(), 0, 1);
    }
}
