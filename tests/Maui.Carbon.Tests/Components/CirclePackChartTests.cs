namespace Maui.Carbon.Tests.Components;

public class CirclePackChartTests
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

    static List<CdsChartNode> Org() =>
    [
        N("Engineering", N("Platform", L("Runtime", 18), L("Networking", 11), L("Storage", 14)), N("Product", L("Web", 22), L("Mobile", 15)), N("Data", L("Analytics", 9), L("ML", 12))),
        N("Sales", N("Enterprise", L("EMEA", 16), L("Americas", 21)), L("Commercial", 24)),
        N("Operations", N("Support", L("Tier 1", 19), L("Tier 2", 8)), L("Finance", 12), L("People", 9)),
    ];

    static List<ChartCircle> Circles(ChartScene s) => s.Shapes.OfType<ChartCircle>().Where(c => !c.HoverOnly).ToList();

    [Fact]
    public void Two_levels_draw_groups_and_their_children_at_thirty_percent()
    {
        var chart = new CdsCirclePackChart { Data = Sales() };
        var scene = chart.RenderScene(600, 400);
        var circles = Circles(scene);
        Assert.Equal(19, circles.Count);
        Assert.Equal(19, scene.Marks.Count);
        Assert.Equal("19 circles", scene.Summary);
        Assert.All(circles, c =>
        {
            Assert.Equal(0.3f, c.FillOpacity);
            Assert.Equal(1f, c.HoverFillOpacity);
            Assert.Equal(1.5f, c.StrokeWidth);
            Assert.Equal(c.Fill, c.Stroke);
        });
        // The biggest group fills the smaller side (d3 pack scales the root to min(w, h) / 2).
        var na = circles.First(c => c.Group == "North America");
        Assert.Equal(97.579, na.Radius, 0.01);
        Assert.Equal(ChartPaint.OfSeries("North America"), na.Fill);
        // Marks in reading order: a group, then its children.
        Assert.Equal("North America: 4,170", scene.Marks[0].SpokenText);
        Assert.Equal(97.579f, scene.Marks[0].Radius!.Value, 0.01f);
        Assert.StartsWith("United States", scene.Marks[1].SpokenText);
    }

    [Fact]
    public void Tooltip_lists_children_and_a_bold_total_and_hover_outlines_children_in_white()
    {
        var chart = new CdsCirclePackChart { Data = Sales() };
        var scene = chart.RenderScene(600, 400);
        var group = scene.Marks[0];
        Assert.Equal(new ChartTooltipRow("North America", null, ChartPaint.OfSeries("North America")), group.Rows[0]);
        Assert.Equal(new ChartTooltipRow("United States", "3,120"), group.Rows[1]);
        Assert.Equal(new ChartTooltipRow("Total", "4,170", default, true), group.Rows[^1]);
        var outlines = scene.Shapes.OfType<ChartCircle>().Where(c => c.HoverOnly && c.Mark == group.Id).ToList();
        Assert.Equal(3, outlines.Count);
        Assert.All(outlines, o => Assert.Equal(ChartPaint.OfKey(CdsColorKeys.TextOnColor), o.Stroke));
        var leaf = scene.Marks[1];
        Assert.Equal(new ChartTooltipRow("United States", "3,120", ChartPaint.OfSeries("North America")), Assert.Single(leaf.Rows));
        // The inner circle is hit before its parent.
        Assert.Equal(leaf.Id, scene.HitTest(leaf.AnchorPoint)!.Id);
    }

    [Fact]
    public void Hierarchy_level_limits_the_depth_drawn()
    {
        var chart = new CdsCirclePackChart { Data = Org() };
        Assert.Equal(11, chart.RenderScene(600, 400).Marks.Count);
        chart.HierarchyLevel = 3;
        Assert.Equal(22, chart.RenderScene(600, 400).Marks.Count);
        chart.HierarchyLevel = 1;
        Assert.Equal(3, chart.RenderScene(600, 400).Marks.Count);
        chart.HierarchyLevel = 9;
        Assert.Equal(3, chart.HierarchyLevel);
        Assert.Equal(3, CdsCirclePackChart.DataDepth(Org()));
        Assert.Equal(2, CdsCirclePackChart.DataDepth(Sales()));
    }

    [Fact]
    public void Zoom_scales_a_department_by_two_around_the_centre_and_disables_the_rest()
    {
        var chart = new CdsCirclePackChart { Data = Org(), IsZoomEnabled = true, HierarchyLevel = 3 };
        CdsChartNode? clicked = null;
        chart.CircleClicked += (_, e) => clicked = e.Node;
        var scene = chart.RenderScene(600, 400);
        Assert.Equal(11, scene.Marks.Count);
        var platform = scene.Marks.First(m => m.SpokenText.StartsWith("Platform"));
        Assert.Contains("press Enter to zoom in", platform.SpokenText);
        var before = platform.Radius!.Value;
        chart.ActivateMark(platform.Id);
        Assert.Equal("Platform", clicked?.Name);
        Assert.Equal("Platform", chart.Focal?.Name);
        scene = chart.RenderScene(600, 400);
        Assert.Equal(22, scene.Marks.Count);
        var zoomed = scene.Marks.First(m => m.SpokenText.StartsWith("Platform"));
        Assert.Equal(before * 2, zoomed.Radius!.Value, 0.01f);
        Assert.Equal(300, zoomed.AnchorPoint.X, 0.01f);
        Assert.Equal(200, zoomed.AnchorPoint.Y, 0.01f);
        var disabled = Circles(scene).Where(c => c.Fill == ChartPaint.OfKey(CdsColorKeys.IconDisabled)).ToList();
        Assert.Equal(22 - 4, disabled.Count);
        Assert.All(Circles(scene), c => Assert.True(c.Clip));
        // Outside the focus there is no tooltip.
        Assert.Empty(scene.Marks.First(m => m.SpokenText.StartsWith("Sales")).Rows);
        chart.ActivateMark(zoomed.Id);
        Assert.Null(chart.Focal);
        Assert.Equal(11, chart.RenderScene(600, 400).Marks.Count);
    }

    [Fact]
    public void One_top_level_node_draws_its_children_in_one_color()
    {
        var chart = new CdsCirclePackChart { Data = new[] { N("Storage", L("Object", 204), L("Block", 132), L("File", 41)) } };
        var scene = chart.RenderScene(600, 400);
        var circles = Circles(scene);
        Assert.Equal(3, circles.Count);
        Assert.All(circles, c =>
        {
            Assert.Equal(ChartPaint.OfSeries("Storage"), c.Fill);
            Assert.Null(c.Group);
        });
    }

    [Fact]
    public void Legend_isolates_groups_and_the_table_lists_child_parent_value()
    {
        var chart = new CdsCirclePackChart { Data = Sales() };
        chart.RenderScene(600, 400);
        chart.ToggleGroup("Europe");
        Assert.Equal(6, chart.RenderScene(600, 400).Marks.Count);
        chart.ToggleGroup("Europe");
        var table = new CdsCirclePackChart { Data = Org() }.RenderTable();
        Assert.Equal(["Child", "Parent", "Value"], table.Headers);
        Assert.Equal(["Runtime", "Platform", "18"], table.Rows[0]);
        Assert.Contains(table.Rows, r => r.SequenceEqual(["Platform", "Engineering", "43"]));
        Assert.Contains(table.Rows, r => r.SequenceEqual(["–", "Engineering", "101"]));
    }

    [Fact]
    public void Loading_uses_the_pie_skeleton_and_every_property_is_settable()
    {
        var chart = new CdsCirclePackChart { IsLoading = true };
        var loading = chart.RenderScene(400, 300);
        Assert.True(loading.Shimmer);
        Assert.Contains(loading.Shapes, s => s is ChartPath);
        chart = new CdsCirclePackChart
        {
            Data = Org(), IsLoading = false, HierarchyLevel = 3, MainGroupPadding = 8, ChildrenPadding = 4, IsZoomEnabled = true,
            Title = "Headcount", LegendPosition = CdsChartLegendPosition.Top, TooltipTotalLabel = "Sum", ValueFormat = "N0",
        };
        var scene = chart.RenderScene(500, 300);
        Assert.Equal(new ChartTooltipRow("Sum", "101", default, true), scene.Marks[0].Rows[^1]);
    }

    [Fact]
    public void Zoom_on_three_level_data_adds_a_click_to_zoom_legend_item()
    {
        var item = Assert.Single(CdsCirclePackChart.ZoomLegend(true, Org()));
        Assert.Equal(ChartLegendExtraKind.Zoom, item.Kind);
        Assert.Equal("Click to zoom", item.Name);
        Assert.Empty(CdsCirclePackChart.ZoomLegend(false, Org()));
        Assert.Empty(CdsCirclePackChart.ZoomLegend(true, Sales()));
    }
}
