namespace Maui.Carbon.Tests.Components;

public class StackedAreaChartTests
{
    static List<CdsChartPoint> Energy() =>
    [
        .. new double[] { 12, 14, 19 }.Select((v, i) => new CdsChartPoint("Solar", new DateTime(2024, 1 + i, 1), v)),
        .. new double[] { 31, 29, 27 }.Select((v, i) => new CdsChartPoint("Wind", new DateTime(2024, 1 + i, 1), v)),
        .. new double[] { 18, 19 }.Select((v, i) => new CdsChartPoint("Hydro", new DateTime(2024, 1 + i, 1), v)),
    ];

    static CdsStackedAreaChart Chart(bool percentage = false) => new()
    {
        Data = Energy(),
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time },
        LeftAxis = new CdsChartAxis { Title = "Generation (TWh)", IsPercentage = percentage },
    };

    static List<ChartPath> Bands(ChartScene s) => s.Shapes.OfType<ChartPath>().Where(p => !p.Fill.IsNone && p.Group is not null).ToList();

    static int Key(CdsChartPoint p) => p.Date!.Value.Month;

    [Fact]
    public void Model_stacks_groups_in_order_with_missing_values_as_zero()
    {
        var series = StackedAreaModel.Stack(Energy(), ["Solar", "Wind", "Hydro"], p => Key(p).ToString(), p => Key(p), false);
        Assert.Equal(3, series.Count);
        var march = series.Select(s => s.Rows[2]).ToList();
        Assert.Equal((0d, 19d), (march[0].Y0, march[0].Y1));
        Assert.Equal((19d, 46d), (march[1].Y0, march[1].Y1));
        Assert.Equal((46d, 46d), (march[2].Y0, march[2].Y1));
        Assert.Null(march[2].Source);
        Assert.NotNull(march[1].Source);
    }

    [Fact]
    public void Model_orders_positions_and_computes_percentages()
    {
        var rows = new List<CdsChartPoint> { new("A", "b", 1), new("A", "a", 3), new("B", "a", 1), new("B", "b", 0) };
        var series = StackedAreaModel.Stack(rows, ["A", "B"], p => p.Key!, p => p.Key == "a" ? 0 : 1, true);
        Assert.Equal(["a", "b"], series[0].Rows.Select(r => r.Key));
        Assert.Equal(75, series[0].Rows[0].Y1, 6);
        Assert.Equal(100, series[1].Rows[0].Y1, 6);
        Assert.Equal(100, series[0].Rows[1].Y1, 6);
        Assert.Equal(series[1].Rows[1].Y0, series[1].Rows[1].Y1);
        // A position whose total is zero stacks at zero.
        var zero = StackedAreaModel.Stack([new("A", "z", 0)], ["A"], p => p.Key!, _ => 0, true);
        Assert.Equal(0, zero[0].Rows[0].Y1);
    }

    [Fact]
    public void Model_flags_groups_that_add_nothing()
    {
        var series = StackedAreaModel.Stack([new("A", "a", 2), new("B", "a", 0)], ["A", "B"], p => p.Key!, _ => 0, false);
        Assert.True(series[0].HasLine);
        Assert.False(series[1].HasLine);
    }

    [Fact]
    public void Bands_stack_from_the_bottom_at_40_percent_with_lines_on_top()
    {
        var scene = Chart().RenderScene(700, 320);
        var bands = Bands(scene);
        Assert.Equal(3, bands.Count);
        Assert.All(bands, b =>
        {
            Assert.Equal(0.4f, b.Opacity);
            Assert.True(b.Stroke.IsNone);
        });
        Assert.True(bands[0].Path.Bounds.Bottom >= bands[1].Path.Bounds.Bottom - 0.5);
        Assert.True(bands[1].Path.Bounds.Top < bands[0].Path.Bounds.Top);
        var lines = scene.Shapes.OfType<ChartPath>().Where(p => p.Fill.IsNone && p.Group is not null).ToList();
        Assert.Equal(3, lines.Count);
        Assert.True(scene.Shapes.IndexOf(lines[0]) > scene.Shapes.IndexOf(bands[^1]));
        Assert.Equal("3 areas", scene.Summary);
    }

    [Fact]
    public void Ruler_tooltip_lists_raw_values_and_the_total()
    {
        var scene = Chart().RenderScene(700, 320);
        Assert.Equal(3, scene.Marks.Count);
        var jan = scene.Marks[0];
        Assert.Equal(["x-value", "Wind", "Hydro", "Solar", "Total"], jan.Rows.Select(r => r.Label));
        Assert.Equal("61", jan.Rows[^1].Value);
        // Dots sit on each band's top edge, only where the group has a value.
        var dots = scene.Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(8, dots.Count);
        Assert.All(dots, d => Assert.True(d.HoverOnly));
        Assert.Equal(3, dots.Count(d => d.Mark == jan.Id));
        // The tooltip anchors at the top of the stack.
        Assert.Equal(dots.Where(d => d.Mark == jan.Id).Min(d => d.Center.Y), jan.AnchorPoint.Y, 0.5);
    }

    [Fact]
    public void Percentage_axis_fills_to_the_top_of_the_plot()
    {
        var scene = Chart(percentage: true).RenderScene(700, 320);
        var top = Bands(scene)[^1].Path.Bounds.Top;
        Assert.Equal(scene.Plot.Top, top, 0.5);
        Assert.Contains(scene.Texts, t => t.Text == "100");
    }

    [Fact]
    public void Labels_axis_and_legend_selection_restack()
    {
        var data = new List<CdsChartPoint> { new("Critical", "Mon", 4), new("Critical", "Tue", 6), new("High", "Mon", 12), new("High", "Tue", 15) };
        var chart = new CdsStackedAreaChart { Data = data, BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels }, Curve = CdsChartCurve.MonotoneX };
        Assert.Equal(2, Bands(chart.RenderScene(500, 300)).Count);
        chart.ToggleGroup("High");
        var scene = chart.RenderScene(500, 300);
        var band = Assert.Single(Bands(scene));
        Assert.Equal("High", band.Group);
        Assert.Equal(scene.Plot.Bottom, band.Path.Bounds.Bottom, 0.5);
    }

    [Fact]
    public void Table_gives_raw_values()
    {
        var table = Chart(percentage: true).RenderTable();
        Assert.Equal(["Group", "Date", "Generation (TWh)"], table.Headers);
        Assert.Equal(8, table.Rows.Count);
        Assert.Equal("12", table.Rows[0][2]);
    }

    [Fact]
    public void Ruler_off_gives_each_point_its_own_tooltip()
    {
        var chart = Chart();
        chart.IsRulerEnabled = false;
        var scene = chart.RenderScene(700, 320);
        Assert.Equal(8, scene.Marks.Count);
        Assert.All(scene.Marks, m => Assert.Null(m.RulerAt));
    }

    [Fact]
    public void Loading_shows_the_skeleton()
    {
        var chart = new CdsStackedAreaChart { IsLoading = true };
        var scene = chart.RenderScene(400, 300);
        Assert.True(scene.Shimmer);
        Assert.Empty(scene.Marks);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsStackedAreaChart
        {
            Data = Energy(),
            Curve = CdsChartCurve.Basis,
            ShowPoints = false,
            PointRadius = 5,
            ArePointsFilled = true,
            IsRulerEnabled = false,
            LegendOrder = ["Hydro"],
        };
        Assert.Empty(chart.RenderScene(500, 300).Shapes.OfType<ChartCircle>());
    }
}
