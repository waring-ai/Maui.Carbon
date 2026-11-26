namespace Maui.Carbon.Tests.Components;

public class SparklineChartTests
{
    static List<CdsChartPoint> Revenue() =>
        new double[] { 420, 431, 418, 447, 452, 440, 466 }.Select((v, i) => new CdsChartPoint("Revenue", new DateTime(2024, 9, 1 + i), v)).ToList();

    [Fact]
    public void Defaults_hide_axes_grid_legend_toolbar_and_points()
    {
        var chart = new CdsSparklineChart { Data = Revenue() };
        var scene = chart.RenderScene(300, 80);
        Assert.Empty(scene.Texts);
        Assert.Equal(new RectF(0, 0, 300, 80), scene.Plot);
        Assert.False(chart.LegendShown);
        Assert.False(chart.ShowPoints);
        Assert.True(chart.IsGradientEnabled);
        Assert.Empty(scene.Shapes.OfType<ChartCircle>());
        Assert.Empty(scene.Shapes.OfType<ChartLine>());
        // The backdrop is transparent so the sparkline sits on its surface.
        Assert.True(scene.Shapes.OfType<ChartRect>().First().Fill.IsNone);
        Assert.Equal("7 values", scene.Summary);
    }

    [Fact]
    public void Area_type_draws_a_gradient_and_line_type_the_line_alone()
    {
        var chart = new CdsSparklineChart { Data = Revenue() };
        var area = chart.RenderScene(300, 80).Shapes.OfType<ChartPath>().Where(p => p.FillGradient is not null).ToList();
        Assert.Single(area);
        chart.Type = CdsSparklineType.Line;
        var line = chart.RenderScene(300, 80).Shapes.OfType<ChartPath>().ToList();
        var path = Assert.Single(line);
        Assert.True(path.Fill.IsNone);
        Assert.Equal(1.5f, path.StrokeWidth);
    }

    [Fact]
    public void Time_domain_runs_edge_to_edge()
    {
        var scene = new CdsSparklineChart { Data = Revenue(), Type = CdsSparklineType.Line }.RenderScene(300, 80);
        var b = scene.Shapes.OfType<ChartPath>().Single().Path.Bounds;
        Assert.Equal(0, b.Left, 0.5);
        Assert.Equal(300, b.Right, 0.5);
    }

    [Fact]
    public void Ruler_shows_date_and_value()
    {
        var scene = new CdsSparklineChart { Data = Revenue() }.RenderScene(300, 80);
        Assert.Equal(ChartHitMode.Ruler, scene.HitMode);
        Assert.Equal(7, scene.Marks.Count);
        Assert.Contains(scene.Marks[0].Rows, r => r.Value == "420");
        var off = new CdsSparklineChart { Data = Revenue(), IsRulerEnabled = false }.RenderScene(300, 80);
        Assert.Empty(off.Marks);
    }

    [Fact]
    public void Empty_draws_one_line_and_loading_shimmers()
    {
        var chart = new CdsSparklineChart();
        var empty = chart.RenderScene(300, 80);
        var line = Assert.Single(empty.Shapes.OfType<ChartLine>());
        Assert.Equal(40.5f, line.From.Y);
        Assert.Equal(300, line.To.X);
        Assert.Equal("No data", empty.Summary);
        Assert.Empty(empty.Marks);
        chart.IsLoading = true;
        Assert.True(chart.RenderScene(300, 80).Shimmer);
    }

    [Fact]
    public void Area_options_apply_and_visible_axes_can_be_set()
    {
        var chart = new CdsSparklineChart
        {
            Data = Revenue(),
            ShowPoints = true,
            IsGradientEnabled = false,
            Curve = CdsChartCurve.MonotoneX,
            LeftAxis = new CdsChartAxis { IncludeZero = false, IsVisible = false },
            BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time, IsVisible = false },
            ColorPairingOption = 3,
            Title = "Revenue",
        };
        var scene = chart.RenderScene(300, 80);
        Assert.Equal(7, scene.Shapes.OfType<ChartCircle>().Count());
        Assert.Single(scene.Shapes.OfType<ChartPath>(), p => !p.Fill.IsNone);
        chart.ClearValue(CdsAreaChart.IsGradientEnabledProperty);
        Assert.True(chart.IsGradientEnabled);
    }

    [Fact]
    public void Loading_shows_one_shimmering_line_without_a_backdrop()
    {
        var scene = new CdsSparklineChart { IsLoading = true }.RenderScene(300, 80);
        Assert.True(scene.Shimmer);
        var line = Assert.IsType<ChartLine>(Assert.Single(scene.Shapes));
        Assert.True(line.Shimmers);
        Assert.Equal(40.5f, line.From.Y);
    }
}
