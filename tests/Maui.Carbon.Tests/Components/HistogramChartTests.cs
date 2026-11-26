namespace Maui.Carbon.Tests.Components;

public class HistogramChartTests
{
    static List<CdsChartPoint> Ages() => ChartBinsTests.PreviewSamples().Ages.Select(a => new CdsChartPoint { Group = "Employees", Value = a }).ToList();

    static List<CdsChartPoint> Latency()
    {
        var values = ChartBinsTests.PreviewSamples().Latency;
        return values.Select((v, i) => new CdsChartPoint { Group = i < 260 ? "us-east" : i < 480 ? "eu-west" : "ap-south", Value = v }).ToList();
    }

    static List<ChartRect> Bars(ChartScene scene) => scene.Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0 && !r.HoverOnly).ToList();

    [Fact]
    public void Exact_extent_comes_back_after_the_axis_headroom()
    {
        var (lo, hi) = CdsHistogramChart.ExactExtent(18, 68);
        var pad = (hi - lo) * 0.1;
        Assert.Equal(18, lo - pad, 9);
        Assert.Equal(68, hi + pad, 9);
    }

    [Fact]
    public void Bins_span_the_axis_exactly_and_the_tallest_meets_the_top()
    {
        var chart = new CdsHistogramChart { Data = Ages(), Bins = 20, IsLegendEnabled = false };
        var scene = chart.RenderScene(700, 320);
        Assert.Equal("25 bins", scene.Summary);
        var bars = Bars(scene);
        Assert.Equal(24, bars.Count); // the 62-64 bin is empty
        Assert.Equal(scene.Plot.Left, bars[0].Rect.Left, 0.5);
        Assert.Equal(scene.Plot.Right, bars[^1].Rect.Right + 1, 0.5);
        var unit = scene.Plot.Width / 50; // 18 to 68
        Assert.All(bars, b => Assert.Equal(2 * unit - 1, b.Rect.Width, 0.05));
        Assert.Equal(scene.Plot.Top + 1, bars.Min(b => b.Rect.Top), 0.5);
        Assert.All(bars, b => Assert.Equal(scene.Plot.Bottom, b.Rect.Bottom, 0.5));
        Assert.Contains(scene.Texts, t => t.Text == "40");
    }

    [Fact]
    public void A_custom_bin_axis_without_zero_is_exact_too()
    {
        var chart = new CdsHistogramChart
        {
            Data = Ages(),
            Bins = 20,
            BottomAxis = new CdsChartAxis { Title = "Age (years)", IncludeZero = false },
            LeftAxis = new CdsChartAxis { Title = "Employees" },
        };
        var scene = chart.RenderScene(700, 320);
        Assert.Equal(scene.Plot.Left, Bars(scene)[0].Rect.Left, 0.5);
        Assert.Contains(scene.Texts, t => t.Text == "Age (years)");
    }

    [Fact]
    public void Bar_tooltips_show_range_group_and_count()
    {
        var chart = new CdsHistogramChart { Data = Ages(), Bins = 20, RangeLabel = "Ages", LeftAxis = new CdsChartAxis { Title = "Employees" } };
        var scene = chart.RenderScene(700, 320);
        var bar = scene.Marks.First(m => m.RulerAt is null && m.Rows[0].Value == "30 – 32");
        Assert.Equal(["Ages", "Group", "Employees"], bar.Rows.Select(r => r.Label));
        Assert.Equal(["30 – 32", "Employees", "42"], bar.Rows.Select(r => r.Value));
        Assert.Equal("30 – 32, Employees, 42", bar.Description);
    }

    [Fact]
    public void Bin_marks_list_groups_last_first_with_the_total()
    {
        var scene = new CdsHistogramChart { Data = Latency(), Bins = 15 }.RenderScene(700, 320);
        var bin = scene.Marks.First(m => m.RulerAt is not null && m.Rows[0].Value == "160 – 180");
        Assert.Equal("Range", bin.Rows[0].Label);
        Assert.Equal(["ap-south", "eu-west", "us-east", "Total"], bin.Rows.Skip(1).Select(r => r.Label));
        Assert.Equal("103", bin.Rows[^1].Value);
        Assert.True(bin.Rows[^1].Bold);
        // The ruler sits at the middle of the bin.
        var segs = Bars(scene).Where(b => b.Rect.Left >= bin.Bounds.Left - 0.5 && b.Rect.Right <= bin.Bounds.Right + 0.5).ToList();
        Assert.Equal(3, segs.Count);
        Assert.Equal(bin.Bounds.Center.X, bin.RulerAt!.Value, 0.5);
        Assert.Equal(3, scene.Shapes.OfType<ChartRect>().Count(r => r.HoverOnly && r.Mark == bin.Id));
        // Segments stack in group order with the divider gap.
        Assert.Equal(segs[0].Rect.Top - 1, segs[1].Rect.Bottom, 0.01);
    }

    [Fact]
    public void Legend_selection_rebins_the_remaining_groups()
    {
        var chart = new CdsHistogramChart { Data = Latency(), Bins = 15 };
        chart.RenderScene(700, 320);
        chart.ToggleGroup("us-east");
        var scene = chart.RenderScene(700, 320);
        Assert.All(Bars(scene), b => Assert.Equal("us-east", b.Group));
        Assert.Equal(260, scene.Marks.Where(m => m.RulerAt is null).Sum(m => int.Parse(m.Rows[2].Value!)));
    }

    [Fact]
    public void Thresholds_make_uneven_bins()
    {
        var data = ChartBinsTests.PreviewSamples().Orders.Select(v => new CdsChartPoint { Group = v > 150 ? "Wholesale" : "Retail", Value = v }).ToList();
        var chart = new CdsHistogramChart { Data = data, BinThresholds = [0, 25, 50, 75, 100, 150, 200, 300, 500], RangeLabel = "Price band" };
        var scene = chart.RenderScene(700, 320);
        Assert.Equal("8 bins", scene.Summary);
        var bars = Bars(scene);
        Assert.Equal(scene.Plot.Left, bars[0].Rect.Left, 0.5);
        var unit = scene.Plot.Width / 500;
        Assert.Equal(25 * unit - 1, bars[0].Rect.Width, 0.05);
        Assert.Contains(bars, b => Math.Abs(b.Rect.Width - (100 * unit - 1)) < 0.05);
    }

    [Fact]
    public void Ruler_off_leaves_bar_marks()
    {
        var chart = new CdsHistogramChart { Data = Ages(), IsRulerEnabled = false };
        var scene = chart.RenderScene(700, 320);
        Assert.All(scene.Marks, m => Assert.Null(m.RulerAt));
        Assert.Equal(Bars(scene).Count, scene.Marks.Count);
    }

    [Fact]
    public void Without_limit_the_bin_axis_adds_headroom()
    {
        var chart = new CdsHistogramChart { Data = Ages(), Bins = 20, LimitDomainToBins = false };
        var scene = chart.RenderScene(700, 320);
        Assert.True(Bars(scene)[0].Rect.Left > scene.Plot.Left + 10);
    }

    [Fact]
    public void Table_lists_each_range_with_a_count_per_group()
    {
        var table = new CdsHistogramChart { Data = Latency(), Bins = 15 }.RenderTable();
        Assert.Equal(["Range", "us-east", "eu-west", "ap-south"], table.Headers);
        Assert.Equal(15, table.Rows.Count);
        Assert.Equal("160 – 180", table.Rows[6][0]);
        Assert.Equal(103, table.Rows[6].Skip(1).Sum(int.Parse));
    }

    [Fact]
    public void Bins_are_coerced_to_at_least_one()
    {
        var chart = new CdsHistogramChart { Bins = 0 };
        Assert.Equal(1, chart.Bins);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsHistogramChart { IsLoading = true };
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
        var chart = new CdsHistogramChart
        {
            Data = Ages(),
            Bins = 5,
            BinThresholds = [20, 40, 60],
            LimitDomainToBins = false,
            RangeLabel = "Ages",
            BarDividerSize = 3,
            ShowTooltipTotal = false,
        };
        Assert.Equal("2 bins", chart.RenderScene(500, 300).Summary);
        var d = new CdsHistogramChart();
        Assert.Equal((10, true, "Range", 1.5), (d.Bins, d.LimitDomainToBins, d.RangeLabel, d.BarDividerSize));
    }
}
