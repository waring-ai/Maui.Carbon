namespace Maui.Carbon.Tests.Components;

public class TreemapChartTests
{
    static CdsChartNode N(string name, params CdsChartNode[] children) => new(name, children);

    static CdsChartNode L(string name, double value, bool label = false) => new(name, value, label);

    static List<CdsChartNode> Spend() =>
    [
        N("Compute", L("Virtual servers", 412, true), L("Kubernetes", 286, true), L("Functions", 64, true), L("Bare metal", 118, true)),
        N("Storage", L("Object storage", 204, true), L("Block storage", 132, true), L("File storage", 41)),
        N("Databases", L("PostgreSQL", 176, true), L("Redis", 58, true), L("MongoDB", 47)),
        N("Networking", L("Load balancers", 83, true), L("Transit gateway", 36), L("CDN", 29)),
        N("AI", L("Model inference", 154, true), L("Training", 97, true)),
    ];

    static List<ChartRect> Leaves(ChartScene scene) => scene.Shapes.OfType<ChartRect>().Where(r => !r.HoverOnly && r.Mark >= 0).ToList();

    [Fact]
    public void Every_leaf_is_a_cell_sized_by_its_value_with_one_pixel_gaps()
    {
        var chart = new CdsTreemapChart { Data = Spend() };
        var scene = chart.RenderScene(600, 400);
        var leaves = Leaves(scene);
        Assert.Equal(15, leaves.Count);
        Assert.Equal(15, scene.Marks.Count);
        Assert.Equal("15 leaves", scene.Summary);
        // Whole pixels, inside the plot, never overlapping.
        foreach (var a in leaves)
        {
            Assert.Equal(Math.Round(a.Rect.X), a.Rect.X);
            Assert.True(a.Rect.Left >= 0 && a.Rect.Top >= 0 && a.Rect.Right <= 600 && a.Rect.Bottom <= 400);
            foreach (var b in leaves)
                if (!ReferenceEquals(a, b)) Assert.False(a.Rect.IntersectsWith(b.Rect) && a.Rect.Intersect(b.Rect).Width * a.Rect.Intersect(b.Rect).Height > 0);
        }
        // Areas follow values (minus the gaps): the largest leaf is the largest cell.
        var area = leaves.Select(l => l.Rect.Width * l.Rect.Height).ToList();
        Assert.Equal(area.Max(), area[0]);
        Assert.InRange(area[0] / area.Sum(), 412.0 / 1937 * 0.9, 412.0 / 1937 * 1.1);
        // Leaves of one group share its color; the cells reach the edges (no outer padding).
        Assert.All(leaves.Take(4), l => Assert.Equal(ChartPaint.OfSeries("Compute"), l.Fill));
        Assert.Equal(0, leaves.Min(l => l.Rect.Left));
        Assert.Equal(600, leaves.Max(l => l.Rect.Right));
        Assert.Equal(400, leaves.Max(l => l.Rect.Bottom));
    }

    [Fact]
    public void Labels_sit_at_seven_by_eighteen_only_where_asked()
    {
        var chart = new CdsTreemapChart { Data = Spend() };
        var scene = chart.RenderScene(600, 400);
        var leaves = Leaves(scene);
        var label = Assert.Single(scene.Texts, t => t.Text == "Virtual servers");
        Assert.Equal(leaves[0].Rect.X + 7, label.X, 0.01);
        Assert.Equal(leaves[0].Rect.Y + 18 - 0.375f * 12, label.Y, 0.01);
        Assert.DoesNotContain(scene.Texts, t => t.Text == "File storage");
        Assert.True(label.FadesWithLegend);
    }

    [Fact]
    public void Labels_are_cut_at_the_leaf_edge_and_hidden_in_short_leaves()
    {
        Assert.Equal("Virtual servers", CdsTreemapChart.LabelText("Virtual servers", new RectF(0, 0, 200, 40)));
        var cut = CdsTreemapChart.LabelText("Virtual servers", new RectF(0, 0, 40, 40));
        Assert.NotNull(cut);
        Assert.StartsWith(cut, "Virtual servers");
        Assert.True(CdsChartTextMetrics.Width(cut, ChartFont.Tick) <= 33);
        Assert.Null(CdsTreemapChart.LabelText("Virtual servers", new RectF(0, 0, 200, 10)));
    }

    [Fact]
    public void Label_text_contrasts_with_the_fill()
    {
        var palette = new CdsChartPalette(CdsThemeName.White, 1);
        var colors = new ChartColors(palette, ["a"], 1, null);
        // Purple 70 (data-vis-01) takes white text; yellow 50 (data-vis-10) black, as Carbon's shade rule.
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.TextOnColor), ChartDraw.ContrastText(colors, palette.Key(CdsColorKeys.DataVis01)));
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute), ChartDraw.ContrastText(colors, palette.Key(CdsColorKeys.DataVis10)));
        Assert.True(ChartDraw.NeedsLightText(palette.Key(CdsColorKeys.DataVis08)));
        Assert.False(ChartDraw.NeedsLightText(palette.Key(CdsColorKeys.DataVis11)));
        var dark = new ChartColors(new CdsChartPalette(CdsThemeName.G100, 1), ["a"], 1, null);
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.Layer01Absolute), ChartDraw.ContrastText(dark, palette.Key(CdsColorKeys.DataVis10)));
        // Non-Carbon colors use Carbon's lightness rule.
        Assert.True(ChartDraw.NeedsLightText(new Color(0.2f, 0.2f, 0.2f), isCarbonColor: false));
    }

    [Fact]
    public void Tooltip_shows_the_group_in_bold_then_the_leaf_and_hover_darkens_it()
    {
        var chart = new CdsTreemapChart { Data = Spend() };
        var scene = chart.RenderScene(600, 400);
        var mark = scene.Marks[0];
        Assert.Equal(2, mark.Rows.Count);
        Assert.Equal(new ChartTooltipRow("Compute", null, ChartPaint.OfSeries("Compute"), true), mark.Rows[0]);
        Assert.Equal(new ChartTooltipRow("Virtual servers", "412"), mark.Rows[1]);
        Assert.Equal("Virtual servers: 412, Compute", mark.SpokenText);
        var overlay = Assert.Single(scene.Shapes.OfType<ChartRect>(), r => r.HoverOnly && r.Mark == mark.Id);
        Assert.Equal(0.221f, overlay.FillOpacity, 0.001f);
    }

    [Fact]
    public void Legend_isolates_groups()
    {
        var chart = new CdsTreemapChart { Data = Spend() };
        chart.RenderScene(600, 400);
        chart.ToggleGroup("AI");
        var scene = chart.RenderScene(600, 400);
        Assert.Equal(2, scene.Marks.Count);
        Assert.Equal(0, Leaves(scene).Min(l => l.Rect.Left));
        Assert.Equal(600, Leaves(scene).Max(l => l.Rect.Right));
    }

    [Fact]
    public void Table_lists_child_group_and_value()
    {
        var data = Spend();
        data.Add(new CdsChartNode("Support", 12));
        var chart = new CdsTreemapChart { Data = data };
        var table = chart.RenderTable();
        Assert.Equal(["Child", "Group", "Value"], table.Headers);
        Assert.Equal(["Virtual servers", "Compute", "412"], table.Rows[0]);
        Assert.Equal(["–", "Support", "12"], table.Rows[^1]);
        Assert.Equal(16, table.Rows.Count);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsTreemapChart { IsLoading = true };
        Assert.True(chart.RenderScene(400, 300).Shimmer);
        chart.IsLoading = false;
        chart.Data = new List<CdsChartNode>();
        Assert.Equal("No data", chart.RenderScene(400, 300).Summary);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsTreemapChart
        {
            Title = "Spend",
            Data = Spend(),
            LegendPosition = CdsChartLegendPosition.Right,
            ColorPairingOption = 2,
            ColorScale = new Dictionary<string, string> { ["AI"] = CdsColorKeys.DataVis05 },
            ValueFormat = "N0",
            IsToolbarEnabled = false,
            SelectedGroups = ["Compute"],
        };
        Assert.Equal(4, chart.RenderScene(500, 300).Marks.Count);
    }
}
