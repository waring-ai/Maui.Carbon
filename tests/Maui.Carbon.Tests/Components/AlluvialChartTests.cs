namespace Maui.Carbon.Tests.Components;

public class AlluvialChartTests
{
    static List<CdsAlluvialLink> Leads() => ChartSankeyTests.Flows.Select(f => new CdsAlluvialLink(f.Item1, f.Item2, f.Item3)).ToList();

    static List<CdsAlluvialNode> LeadNodes() => ChartSankeyTests.Nodes.Select(n => new CdsAlluvialNode(n.Item1, n.Item2)).ToList();

    static CdsAlluvialChart Chart() => new() { Data = Leads(), Nodes = LeadNodes(), Units = "leads" };

    [Fact]
    public void Nodes_links_and_headers_follow_carbon_geometry()
    {
        var scene = Chart().RenderScene(600, 400);
        Assert.Equal(28, scene.Marks.Count);
        Assert.Equal("18 flows", scene.Summary);
        var nodeRects = scene.Shapes.OfType<ChartRect>().Where(r => !r.HoverOnly && r.Rect.Width == 4).ToList();
        Assert.Equal(10, nodeRects.Count);
        Assert.Equal(2, nodeRects.Min(r => r.Rect.Left));
        Assert.Equal(598, nodeRects.Max(r => r.Rect.Right));
        Assert.True(nodeRects.Min(r => r.Rect.Top) >= 30 - 0.01);
        Assert.All(nodeRects, r => Assert.Equal(ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute), r.Fill));
        Assert.Equal(["Source", "Stage", "Outcome"], scene.Texts.Where(t => t.Font.Size == 14).Select(t => t.Text));
        // A header left of its column where it fits: the first one starts at the column.
        Assert.Equal(2, scene.Texts.First(t => t.Text == "Source").X);
        var links = scene.Shapes.OfType<ChartPath>().Where(p => !p.HoverOnly).ToList();
        Assert.Equal(18, links.Count);
        Assert.All(links, l => Assert.Equal(0.8f, l.StrokeOpacity));
        Assert.Equal(ChartPaint.OfSeries("Search"), links[0].Stroke);
        Assert.Equal(87.524f, links[0].StrokeWidth, 0.01f);
    }

    [Fact]
    public void Node_labels_read_name_and_value_left_of_the_node_where_there_is_room()
    {
        var scene = Chart().RenderScene(600, 400);
        var search = scene.Texts.Single(t => t.Text == "Search (650)");
        Assert.True(search.X > 6);
        var won = scene.Texts.Single(t => t.Text == "Won (500)");
        Assert.True(won.X < 594);
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.Layer01Absolute), won.Color);
        Assert.Contains(scene.Shapes.OfType<ChartRect>(), r => r.Rect.Height == 18 && Math.Abs(r.Rect.Right - 590) < 0.01);
    }

    [Fact]
    public void Link_tooltips_name_the_target_and_value_with_units()
    {
        var scene = Chart().RenderScene(600, 400);
        var link = scene.Marks[0];
        Assert.Equal(new ChartTooltipRow("Trial →", "420 leads", ChartPaint.OfSeries("Search")), Assert.Single(link.Rows));
        Assert.Equal("Search → Trial (420 leads)", link.SpokenText);
        Assert.True(link.Polygon!.Length > 4);
        var trial = scene.Marks.Single(m => m.SpokenText.StartsWith("Trial (800"));
        Assert.Equal("Trial (800 leads), Stage", trial.SpokenText);
        // Hovering a node raises every path through it (6 links: 3 in, 3 out).
        Assert.Equal(6, scene.Shapes.Count(s => s.HoverOnly && s.Mark == trial.Id && s is ChartPath));
        Assert.Contains(scene.Shapes.OfType<ChartRect>(), r => r.HoverOnly && r.Mark == trial.Id && r.Rect.Width == 8);
        Assert.Equal(1f, scene.Shapes.OfType<ChartPath>().First(p => p.HoverOnly && p.Mark == link.Id).StrokeOpacity);
        // A label is hit before the links under it.
        Assert.Equal(trial.Id, scene.HitTest(trial.AnchorPoint)!.Id);
    }

    [Fact]
    public void Monochrome_and_gradient_links()
    {
        var mono = Chart();
        mono.IsMonochrome = true;
        Assert.All(mono.RenderScene(600, 400).Shapes.OfType<ChartPath>(), p => Assert.Equal(ChartPaint.OfSeries("Search"), p.Stroke));
        var gradient = new CdsAlluvialChart
        {
            Data = new[] { new CdsAlluvialLink("Solar", "Grid", 34), new CdsAlluvialLink("Grid", "Homes", 68) },
            IsGradientEnabled = true,
            ColorScale = new Dictionary<string, string> { ["Solar"] = CdsColorKeys.DataVis02, ["Grid"] = CdsColorKeys.DataVis05, ["Homes"] = CdsColorKeys.DataVis09 },
        };
        var scene = gradient.RenderScene(600, 400);
        // Each link is one band filled with a gradient across the graph, so it has no seams.
        var links = scene.Shapes.OfType<ChartPath>().Where(p => !p.HoverOnly).ToList();
        Assert.Equal(2, links.Count);
        var solar = links[0].FillGradient!;
        Assert.Equal(0, solar.Start.X);
        Assert.Equal(600, solar.End.X);
        Assert.Equal(solar.Start.Y, solar.End.Y);
        Assert.Equal([ChartPaint.OfSeries("Solar"), ChartPaint.OfSeries("Grid")], solar.Stops.Select(s => s.Color));
        Assert.Equal(ChartPaint.OfSeries("Homes"), links[1].FillGradient!.Stops[^1].Color);
    }

    [Fact]
    public void Nodes_come_from_the_links_when_not_given_and_bad_graphs_draw_nothing()
    {
        var chart = new CdsAlluvialChart { Data = Leads() };
        var scene = chart.RenderScene(600, 400);
        Assert.Equal(28, scene.Marks.Count);
        Assert.DoesNotContain(scene.Texts, t => t.Font.Size == 14);
        var bad = new CdsAlluvialChart { Data = new[] { new CdsAlluvialLink("a", "b", 1) }, Nodes = [new CdsAlluvialNode("a")] };
        Assert.Empty(bad.RenderScene(600, 400).Marks);
        var cycle = new CdsAlluvialChart { Data = new[] { new CdsAlluvialLink("a", "b", 1), new CdsAlluvialLink("b", "a", 1) } };
        Assert.Empty(cycle.RenderScene(600, 400).Marks);
    }

    [Fact]
    public void Node_padding_is_at_least_twenty_four()
    {
        var chart = Chart();
        chart.NodePadding = 4;
        var nodes = chart.RenderScene(600, 400).Shapes.OfType<ChartRect>().Where(r => !r.HoverOnly && r.Rect.Width == 4 && r.Rect.X < 10).OrderBy(r => r.Rect.Y).ToList();
        for (var i = 1; i < nodes.Count; i++) Assert.True(nodes[i].Rect.Top - nodes[i - 1].Rect.Bottom >= 24 - 0.01);
        chart.NodeAlignment = CdsAlluvialNodeAlignment.Left;
        Assert.Equal(28, chart.RenderScene(600, 400).Marks.Count);
    }

    [Fact]
    public void Table_lists_source_target_value_sorted_by_source()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Source", "Target", "Value"], table.Headers);
        Assert.Equal(["Contact sales", "Won", "120"], table.Rows[0]);
        Assert.Equal(["Trial", "Open", "160"], table.Rows[^1]);
    }

    [Fact]
    public void Loading_empty_and_every_property()
    {
        var chart = new CdsAlluvialChart { IsLoading = true };
        Assert.True(chart.RenderScene(400, 300).Shimmer);
        Assert.False(chart.LegendShown);
        chart = new CdsAlluvialChart { Data = Leads(), Nodes = LeadNodes(), Units = "TWh", NodeAlignment = CdsAlluvialNodeAlignment.Right, NodePadding = 30, IsMonochrome = true, IsGradientEnabled = true, Title = "Leads" };
        Assert.Equal(28, chart.RenderScene(500, 400).Marks.Count);
    }
}
