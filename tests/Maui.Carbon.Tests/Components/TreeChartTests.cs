namespace Maui.Carbon.Tests.Components;

public class TreeChartTests
{
    static CdsTreeChart Chart() => new() { Data = ChartTreeLayoutTests.Catalog(), RootTitle = "Cloud" };

    [Fact]
    public void Draws_every_node_with_links_labels_and_carbon_styles()
    {
        var scene = Chart().RenderScene(600, 400);
        Assert.Equal(26, scene.Marks.Count);
        Assert.Equal("26 nodes", scene.Summary);
        var links = scene.Shapes.OfType<ChartPath>().ToList();
        Assert.Equal(25, links.Count);
        Assert.All(links, l =>
        {
            Assert.Equal(ChartPaint.OfKey(CdsColorKeys.BorderStrong01), l.Stroke);
            Assert.Equal(0.4f, l.StrokeOpacity);
            Assert.Equal(1.5f, l.StrokeWidth);
        });
        var dots = scene.Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(26, dots.Count);
        Assert.All(dots, d => Assert.Equal(2.5f, d.Radius));
        Assert.Equal(11, dots.Count(d => d.Fill == ChartPaint.OfKey(CdsColorKeys.TextSecondary)));
        // Each label has a 4-copy text-inverse halo.
        Assert.Equal(26 * 5, scene.Texts.Count);
        var root = scene.Texts.Last(t => t.Text == "Cloud");
        Assert.Equal(ChartTextAnchor.End, root.Anchor);
        Assert.Equal(CdsChartTextMetrics.Width("Cloud", ChartFont.Tick), root.X, 0.01);
        var leaf = scene.Texts.Last(t => t.Text == "DNS");
        Assert.Equal(ChartTextAnchor.Start, leaf.Anchor);
        Assert.True(scene.Texts.Where(t => t.Anchor == ChartTextAnchor.Start).All(t => t.X + t.Width <= 600 + 2));
        Assert.All(scene.Marks, m => Assert.Empty(m.Rows));
    }

    [Fact]
    public void Dendrogram_lines_every_leaf_up()
    {
        var chart = Chart();
        chart.TreeType = CdsTreeChartType.Dendrogram;
        var scene = chart.RenderScene(600, 400);
        var leaves = scene.Shapes.OfType<ChartCircle>().Where(c => c.Fill == ChartPaint.OfKey(CdsColorKeys.BorderStrong01)).Select(c => c.Center.X).Distinct().ToList();
        Assert.Single(leaves);
    }

    [Fact]
    public void Clicking_a_parent_collapses_and_expands_it_and_recentres_the_rows()
    {
        var chart = Chart();
        CdsChartNode? clicked = null;
        chart.NodeClicked += (_, e) => clicked = e.Node;
        var scene = chart.RenderScene(600, 400);
        var platform = scene.Marks.First(m => m.SpokenText.StartsWith("Platform"));
        Assert.EndsWith("expanded", platform.SpokenText);
        chart.ActivateMark(platform.Id);
        Assert.Equal("Platform", clicked?.Name);
        scene = chart.RenderScene(600, 400);
        Assert.Equal(16, scene.Marks.Count);
        Assert.EndsWith("collapsed", scene.Marks.First(m => m.Id == platform.Id).SpokenText);
        var ys = scene.Marks.Select(m => m.AnchorPoint.Y).ToList();
        Assert.Equal(400 - ys.Max(), ys.Min(), 0.5);
        // The root is not clickable.
        chart.ActivateMark(scene.Marks[0].Id);
        Assert.Equal(16, chart.RenderScene(600, 400).Marks.Count);
        chart.ActivateMark(platform.Id);
        Assert.Equal(26, chart.RenderScene(600, 400).Marks.Count);
    }

    [Fact]
    public void Layout_takes_label_widths_off_the_width()
    {
        Assert.Equal("Virtual servers", CdsTreeChart.LongestLabel(ChartTreeLayoutTests.Catalog()));
        var (root, margin) = CdsTreeChart.Layout(ChartTreeLayoutTests.Catalog(), 600, 400, CdsTreeChartType.Tree, "Cloud");
        Assert.Equal(CdsChartTextMetrics.Width("Cloud", ChartFont.Tick) + 6, margin, 0.01);
        var extent = 600 - CdsChartTextMetrics.Width("Virtual servers", ChartFont.Tick) - 3 * 6 - CdsChartTextMetrics.Width("Cloud", ChartFont.Tick);
        Assert.Equal(extent, root.PreOrder().Max(n => n.Y), 0.01);
    }

    [Fact]
    public void Table_lists_child_and_parent_and_loading_shows_the_skeleton()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Child", "Parent"], table.Headers);
        Assert.Equal(25, table.Rows.Count);
        Assert.Equal(["Virtual servers", "Compute"], table.Rows[0]);
        Assert.Contains(table.Rows, r => r.SequenceEqual(["Platform", "–"]));
        var loading = new CdsTreeChart { IsLoading = true };
        Assert.True(loading.RenderScene(300, 200).Shimmer);
        Assert.False(loading.LegendShown);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsTreeChart { Data = ChartTreeLayoutTests.Catalog(), TreeType = CdsTreeChartType.Dendrogram, RootTitle = null, Title = "Catalog", IsToolbarEnabled = false };
        Assert.Equal(26, chart.RenderScene(500, 300).Marks.Count);
        chart.RootTitle = "";
        Assert.Equal(26, chart.RenderScene(500, 300).Marks.Count);
    }
}
