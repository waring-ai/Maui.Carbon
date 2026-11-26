namespace Maui.Carbon.Tests.Components;

public class BoxplotChartTests
{
    static List<CdsChartPoint> Data() =>
    [
        .. Enumerable.Range(1, 10).Select(v => new CdsChartPoint { Group = "Design", Value = v }),
        new CdsChartPoint { Group = "Design", Value = 100 },
        .. new double[] { 20, 22, 24, 26, 28, 30 }.Select(v => new CdsChartPoint { Group = "Engineering", Value = v }),
    ];

    static CdsBoxplotChart Chart() => new()
    {
        Data = Data(),
        LeftAxis = new CdsChartAxis { Title = "Salary" },
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels },
    };

    [Theory]
    [InlineData(0.25, 1.75)]
    [InlineData(0.5, 2.5)]
    [InlineData(0.75, 3.25)]
    [InlineData(0, 1)]
    [InlineData(1, 4)]
    public void Quantile_is_d3s_r7(double p, double expected)
    {
        Assert.Equal(expected, ChartBoxplotStats.Quantile([1, 2, 3, 4], p), 9);
    }

    [Fact]
    public void Quantile_of_one_or_no_values()
    {
        Assert.Equal(7, ChartBoxplotStats.Quantile([7], 0.5));
        Assert.True(double.IsNaN(ChartBoxplotStats.Quantile([], 0.5)));
    }

    [Fact]
    public void Statistics_fence_outliers_at_one_and_a_half_iqr()
    {
        var s = ChartBoxplotStats.Compute("Design", [10, 3, 1, 2, 100, 4, 5, 6, 7, 8, 9, double.NaN])!;
        Assert.Equal(3.5, s.Q1, 9);
        Assert.Equal(6, s.Median, 9);
        Assert.Equal(8.5, s.Q3, 9);
        Assert.Equal(5, s.Iqr, 9);
        Assert.Equal([100d], s.Outliers);
        Assert.Equal(1, s.WhiskerMin);
        Assert.Equal(10, s.WhiskerMax);
        Assert.Null(ChartBoxplotStats.Compute("Empty", []));
    }

    [Fact]
    public void Whiskers_fall_back_to_the_quartiles_when_every_value_is_an_outlier()
    {
        // Two clusters: the fences are tight around nothing.
        var s = ChartBoxplotStats.Compute("g", [0, 0, 0, 0, 10])!;
        Assert.Equal([10d], s.Outliers);
        Assert.Equal(0, s.WhiskerMin);
        Assert.Equal(0, s.WhiskerMax);
    }

    [Fact]
    public void Boxes_whiskers_and_outliers_follow_carbons_geometry()
    {
        var scene = Chart().RenderScene(600, 320);
        Assert.Equal("2 boxes", scene.Summary);
        Assert.Equal(3, scene.Marks.Count); // Design box, its outlier, Engineering box
        var boxes = scene.Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0).ToList();
        Assert.Equal(2, boxes.Count);
        Assert.All(boxes, b =>
        {
            Assert.Equal(16, b.Rect.Width, 0.01);
            Assert.Equal(0.3f, b.FillOpacity);
            Assert.Equal(0.5f, b.HoverFillOpacity);
            Assert.Equal(1, b.StrokeWidth);
        });
        var outlier = Assert.Single(scene.Shapes.OfType<ChartCircle>());
        Assert.Equal(4, outlier.Radius);
        Assert.Equal(0.3f, outlier.FillOpacity);
        Assert.Equal(1f, outlier.HoverFillOpacity);
        var lines = scene.Shapes.OfType<ChartLine>().Where(l => l.Group == "Design").ToList();
        Assert.Equal(5, lines.Count);
        Assert.Equal(2, lines.Count(l => l.StrokeWidth == 1));
        var caps = lines.Where(l => l.StrokeWidth == 2).Select(l => Math.Abs(l.To.X - l.From.X)).Order().ToList();
        Assert.Equal([8f, 8f, 16f], caps.Select(c => MathF.Round(c, 2)));
        var box = scene.Marks[0];
        Assert.True(box.Bounds.Height > boxes[0].Rect.Height, "the hover area spans the whiskers");
    }

    [Fact]
    public void Box_tooltips_list_the_statistics_and_outliers_their_value()
    {
        var scene = Chart().RenderScene(600, 320);
        var box = scene.Marks[0];
        Assert.Equal(["Group", "Minimum", "Q1", "Median", "Q3", "Maximum", "IQR"], box.Rows.Select(r => r.Label));
        Assert.Equal(["Design", "1", "3.5", "6", "8.5", "10", "5"], box.Rows.Select(r => r.Value));
        Assert.StartsWith("Design: median 6", box.Description);
        Assert.EndsWith("1 outlier", box.Description);
        var outlier = scene.Marks[1];
        Assert.Equal(["Group", "Outlier"], outlier.Rows.Select(r => r.Label));
        Assert.Equal("100", outlier.Rows[1].Value);
        Assert.Equal(outlier.Id, scene.HitTest(outlier.AnchorPoint)!.Id);
    }

    [Fact]
    public void Box_plots_are_monochrome_and_have_no_legend()
    {
        var chart = Chart();
        chart.RenderScene(600, 320);
        Assert.False(chart.LegendShown);
        chart.IsLegendEnabled = true;
        Assert.False(chart.LegendShown);
        var palette = new CdsChartPalette(CdsThemeName.White, 1);
        var colors = new ChartColors(palette, [""], 2, null);
        Assert.Equal(palette.Series(1, 2, 0, false), colors.Series("Design"));
        Assert.Equal(colors.Series("Design"), colors.Series("Engineering"));
    }

    [Fact]
    public void Labels_on_the_left_make_horizontal_boxes()
    {
        var chart = new CdsBoxplotChart { Data = Data(), LeftAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels }, BottomAxis = new CdsChartAxis() };
        var boxes = chart.RenderScene(600, 320).Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0).ToList();
        Assert.All(boxes, b => Assert.Equal(16, b.Rect.Height, 0.01));
    }

    [Fact]
    public void Narrow_plots_shrink_boxes_to_half_the_category()
    {
        var boxes = Chart().RenderScene(80, 320).Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0).ToList();
        Assert.All(boxes, b => Assert.True(b.Rect.Width < 16));
    }

    [Fact]
    public void Table_gives_every_statistic_per_group()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Group", "Minimum", "Q1", "Median", "Q3", "Maximum", "IQR", "Outlier(s)"], table.Headers);
        Assert.Equal(["Design", "1", "3.5", "6", "8.5", "10", "5", "100"], table.Rows[0]);
        Assert.Equal("–", table.Rows[1][7]);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsBoxplotChart { IsLoading = true };
        var loading = chart.RenderScene(400, 300);
        Assert.True(loading.Shimmer);
        Assert.Empty(loading.Marks);
        chart.IsLoading = false;
        var empty = chart.RenderScene(400, 300);
        Assert.False(empty.Shimmer);
        Assert.Equal("No data", empty.Summary);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsBoxplotChart
        {
            Data = Data(),
            ColorPairingOption = 4,
            ColorScale = new Dictionary<string, string> { ["Design"] = CdsColorKeys.DataVis03 },
            ValueFormat = "N1",
            SelectedGroups = ["Design"],
        };
        Assert.Equal("2 boxes", chart.RenderScene(500, 300).Summary);
    }
}
