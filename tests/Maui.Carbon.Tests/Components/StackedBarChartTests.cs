namespace Maui.Carbon.Tests.Components;

public class StackedBarChartTests
{
    static List<CdsChartPoint> Units() =>
    [
        .. new double[] { 120, 135, 98, 150, 142 }.Select((v, i) => new CdsChartPoint("Direct", Months[i], v)),
        .. new double[] { 80, 72, 95, 88, 101 }.Select((v, i) => new CdsChartPoint("Partner", Months[i], v)),
        .. new double[] { 45, 60, 74, 91, 110 }.Select((v, i) => new CdsChartPoint("Online", Months[i], v)),
    ];

    static readonly string[] Months = ["Jan", "Feb", "Mar", "Apr", "May"];

    static CdsStackedBarChart Chart(bool percentage = false) => new()
    {
        Data = Units(),
        LeftAxis = new CdsChartAxis { Title = "Units", IsPercentage = percentage },
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels },
    };

    static List<ChartRect> Segments(ChartScene scene) => scene.Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0 && !r.HoverOnly).ToList();

    [Fact]
    public void Divider_takes_one_pixel_off_the_far_end_of_long_segments()
    {
        Assert.Equal(51, ChartStackedBars.DividedEnd(100, 50, 0, 10, true, 1.5, 3));
        Assert.Equal(149, ChartStackedBars.DividedEnd(100, 150, 0, 10, false, 1.5, 3));
        Assert.Equal(101, ChartStackedBars.DividedEnd(100, 101, 0, 1, true, 1.5, 3));
        // Negative segments: 1px, or 2px where they end at zero; none for a single group.
        Assert.Equal(101, ChartStackedBars.DividedEnd(150, 100, -10, -5, true, 1.5, 2));
        Assert.Equal(102, ChartStackedBars.DividedEnd(150, 100, -10, 0, true, 1.5, 2));
        Assert.Equal(100, ChartStackedBars.DividedEnd(150, 100, -10, 0, true, 1.5, 1));
    }

    [Fact]
    public void Segments_stack_in_group_order_with_divider_gaps()
    {
        var scene = Chart().RenderScene(600, 320);
        Assert.Equal("5 stacked bars", scene.Summary);
        var segs = Segments(scene);
        Assert.Equal(15, segs.Count);
        Assert.All(segs, s => Assert.Equal(16, s.Rect.Width, 0.01));
        var (direct, partner, online) = (segs[0].Rect, segs[1].Rect, segs[2].Rect);
        Assert.Equal("Direct", segs[0].Group);
        Assert.Equal(direct.Left, partner.Left, 0.01);
        Assert.Equal(direct.Top - 1, partner.Bottom, 0.01);
        Assert.Equal(partner.Top - 1, online.Bottom, 0.01);
        Assert.Equal(120 / 80.0, (direct.Height + 1) / (partner.Height + 1), 0.02);
    }

    [Fact]
    public void A_category_mark_lists_every_group_largest_first_with_the_total()
    {
        var scene = Chart().RenderScene(600, 320);
        Assert.Equal(20, scene.Marks.Count);
        var jan = scene.Marks[0];
        Assert.NotNull(jan.RulerAt);
        Assert.Equal(["x-value", "Direct", "Partner", "Online", "Total"], jan.Rows.Select(r => r.Label));
        Assert.Equal(["Jan", "120", "80", "45", "245"], jan.Rows.Select(r => r.Value));
        Assert.True(jan.Rows[^1].Bold);
        Assert.Equal("Jan, Direct 120, Partner 80, Online 45, Total 245", jan.Description);
        // The stack darkens with its category: one hover copy per segment.
        Assert.Equal(15, scene.Shapes.OfType<ChartRect>().Count(r => r.HoverOnly));
        Assert.Equal(3, scene.Shapes.OfType<ChartRect>().Count(r => r.HoverOnly && r.Mark == jan.Id));
    }

    [Fact]
    public void Hovering_a_segment_shows_it_and_the_rest_of_the_category_shows_the_stack()
    {
        var scene = Chart().RenderScene(600, 320);
        var direct = scene.Marks[1];
        Assert.Equal("Direct", direct.Group);
        Assert.Equal(direct.Id, scene.HitTest(direct.Bounds.Center)!.Id);
        Assert.Equal(["x-value", "Units", "Group"], direct.Rows.Select(r => r.Label));
        var above = new PointF(direct.Bounds.Center.X, scene.Plot.Top + 2);
        Assert.Equal(scene.Marks[0].Id, scene.HitTest(above)!.Id);
    }

    [Fact]
    public void Ruler_off_leaves_only_segment_marks()
    {
        var chart = Chart();
        chart.IsRulerEnabled = false;
        var scene = chart.RenderScene(600, 320);
        Assert.Equal(15, scene.Marks.Count);
        Assert.DoesNotContain(scene.Shapes, s => s.HoverOnly);
    }

    [Fact]
    public void Percentage_stacks_fill_the_axis_and_tooltips_keep_values()
    {
        var scene = Chart(percentage: true).RenderScene(600, 320);
        var segs = Segments(scene);
        foreach (var top in segs.Where((_, i) => i % 3 == 2)) Assert.Equal(scene.Plot.Top + 1, top.Rect.Top, 0.5);
        Assert.Contains(scene.Texts, t => t.Text == "100");
        Assert.Equal("120", scene.Marks[1].Rows[1].Value);
    }

    [Fact]
    public void Negative_values_stack_down_from_zero()
    {
        var data = new List<CdsChartPoint>
        {
            new("Deposits", "Jan", 52), new("Interest", "Jan", 8), new("Withdrawals", "Jan", -38), new("Fees", "Jan", -4),
        };
        var scene = new CdsStackedBarChart { Data = data }.RenderScene(400, 300);
        var segs = Segments(scene);
        var deposits = segs.Single(s => s.Group == "Deposits").Rect;
        var withdrawals = segs.Single(s => s.Group == "Withdrawals").Rect;
        var fees = segs.Single(s => s.Group == "Fees").Rect;
        Assert.True(withdrawals.Top >= deposits.Bottom - 0.01);
        Assert.True(fees.Top >= withdrawals.Bottom - 0.01);
        Assert.Equal(38 / 4.0, (withdrawals.Height + 1) / (fees.Height + 1), 0.6);
    }

    [Fact]
    public void Value_axis_covers_the_stacked_sums()
    {
        var scene = Chart().RenderScene(600, 320);
        // May stacks to 353, the tallest: the axis adds 10% headroom above it.
        var mayTop = Segments(scene).Where(r => r.Group == "Online").Min(r => r.Rect.Top);
        var expected = scene.Plot.Top + scene.Plot.Height * (1 - 353 / 388.3) + 1;
        Assert.Equal(expected, mayTop, 1.0);
    }

    [Fact]
    public void Legend_selection_restacks_the_remaining_groups()
    {
        var chart = Chart();
        chart.RenderScene(600, 320);
        chart.ToggleGroup("Online");
        var scene = chart.RenderScene(600, 320);
        var segs = Segments(scene);
        Assert.Equal(5, segs.Count);
        Assert.All(segs, s => Assert.Equal("Online", s.Group));
        Assert.Equal(scene.Plot.Bottom, segs[0].Rect.Bottom, 1.0);
    }

    [Fact]
    public void Time_axis_stacks_by_date()
    {
        var data = new[] { 52d, 61, 48 }.Select((v, i) => new CdsChartPoint("Deposits", new DateTime(2024, 1 + i, 1), v))
            .Concat(new[] { 8d, 8, 9 }.Select((v, i) => new CdsChartPoint("Interest", new DateTime(2024, 1 + i, 1), v))).ToList();
        var scene = new CdsStackedBarChart { Data = data, BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time } }.RenderScene(600, 300);
        Assert.Equal(3 + 6, scene.Marks.Count);
        Assert.Equal("Mar 1, 2024", scene.Marks[6].Rows[0].Value);
    }

    [Fact]
    public void Table_lists_every_row()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Group", "Key", "Units"], table.Headers);
        Assert.Equal(15, table.Rows.Count);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsStackedBarChart { IsLoading = true };
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
        var chart = new CdsStackedBarChart
        {
            Data = Units(),
            BarWidth = 12,
            BarMaxWidth = 24,
            BarDividerSize = 3,
            IsRulerEnabled = false,
            ShowTooltipTotal = false,
            TooltipTotalLabel = "Sum",
        };
        Assert.All(Segments(chart.RenderScene(500, 300)), s => Assert.Equal(12, s.Rect.Width, 0.01));
        Assert.Equal(1.5, new CdsStackedBarChart().BarDividerSize);
        Assert.Equal(16, new CdsStackedBarChart().BarMaxWidth);
    }
}
