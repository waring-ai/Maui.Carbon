namespace Maui.Carbon.Tests.Components;

public class GroupedBarChartTests
{
    static readonly string[] Regions = ["North America", "Europe", "Asia Pacific", "Latin America"];

    static List<CdsChartPoint> Revenue() =>
    [
        .. new double[] { 42, 47, 51, 58 }.Select((v, i) => new CdsChartPoint("North America", "Q" + (i + 1), v)),
        .. new double[] { 35, 33, 38, 41 }.Select((v, i) => new CdsChartPoint("Europe", "Q" + (i + 1), v)),
        .. new double[] { 28, 31, 36, 44 }.Select((v, i) => new CdsChartPoint("Asia Pacific", "Q" + (i + 1), v)),
        .. new double[] { 12, 14, 13, 17 }.Select((v, i) => new CdsChartPoint("Latin America", "Q" + (i + 1), v)),
    ];

    static CdsGroupedBarChart Chart(IEnumerable<CdsChartPoint>? data = null) => new()
    {
        Data = (data ?? Revenue()).ToList(),
        LeftAxis = new CdsChartAxis { Title = "Revenue (USD M)" },
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels, Title = "Quarter" },
    };

    static List<ChartRect> Bars(ChartScene scene) => scene.Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0).ToList();

    [Fact]
    public void Padding_is_five_pixels_scaled_to_the_step()
    {
        Assert.Equal(15, ChartGroupedBars.Pad(70, 4), 6);
        Assert.Equal(7.5, ChartGroupedBars.Pad(35, 4), 6);
        Assert.Equal(15, ChartGroupedBars.Pad(700, 4), 6);
        Assert.Equal(0, ChartGroupedBars.Pad(70, 1));
    }

    [Fact]
    public void Bars_sit_side_by_side_centered_on_their_category_in_reading_order()
    {
        var scene = Chart().RenderScene(800, 320);
        var bars = Bars(scene);
        Assert.Equal(16, bars.Count);
        Assert.Equal("16 bars", scene.Summary);
        Assert.All(bars, b => Assert.Equal(16, b.Rect.Width, 0.01));
        // Marks run category by category, groups in order.
        Assert.Equal(Regions, scene.Marks.Take(4).Select(m => m.Group));
        Assert.All(scene.Marks.Take(4), m => Assert.Equal("Q1", m.Rows[0].Value));
        var q1 = bars.Take(4).Select(b => b.Rect).ToList();
        for (var i = 1; i < 4; i++)
        {
            var gap = q1[i].Left - q1[i - 1].Right;
            Assert.InRange(gap, 0, 5.01);
        }
        var label = scene.Texts.Single(t => t.Text == "Q1");
        // Carbon draws each bar at the start of its inner band, so the group sits within a few pixels of the center.
        Assert.Equal(label.X, (q1[0].Left + q1[3].Right) / 2, 3.0);
        Assert.All(bars, b => Assert.Equal(bars[0].Rect.Bottom, b.Rect.Bottom, 0.01));
        Assert.Equal(58 / 12.0, bars[12].Rect.Height / bars[3].Rect.Height, 0.05);
    }

    [Fact]
    public void Dense_categories_shrink_bars_and_padding()
    {
        var scene = Chart().RenderScene(220, 320);
        var bars = Bars(scene);
        Assert.All(bars, b => Assert.True(b.Rect.Width < 16));
        var gap = bars[1].Rect.Left - bars[0].Rect.Right;
        Assert.InRange(gap, 0, 4.9);
    }

    [Fact]
    public void Bar_width_is_used_when_it_fits_the_maximum()
    {
        var chart = Chart();
        chart.BarWidth = 8;
        Assert.All(Bars(chart.RenderScene(800, 320)), b => Assert.Equal(8, b.Rect.Width, 0.01));
        chart.BarWidth = 30;
        Assert.All(Bars(chart.RenderScene(800, 320)), b => Assert.Equal(16, b.Rect.Width, 0.01));
        chart.BarMaxWidth = 40;
        Assert.All(Bars(chart.RenderScene(800, 320)), b => Assert.Equal(30, b.Rect.Width, 0.01));
    }

    [Fact]
    public void Tooltips_show_category_value_and_group()
    {
        var mark = Chart().RenderScene(800, 320).Marks[1];
        Assert.Equal(("Quarter", "Q1"), (mark.Rows[0].Label, mark.Rows[0].Value));
        Assert.Equal(("Revenue (USD M)", "35"), (mark.Rows[1].Label, mark.Rows[1].Value));
        Assert.Equal(("Group", "Europe"), (mark.Rows[2].Label, mark.Rows[2].Value));
        Assert.Equal(ChartPaintKind.Series, mark.Rows[2].Color.Kind);
        Assert.Equal("Q1, Europe, 35", mark.Description);
    }

    [Fact]
    public void Missing_bars_keep_their_slot_unless_told_otherwise()
    {
        var data = Revenue().Where(p => !(p.Group == "Europe" && p.Key == "Q2")).ToList();
        var chart = Chart(data);
        var kept = Bars(chart.RenderScene(800, 320));
        Assert.Equal(15, kept.Count);
        var label = chart.RenderScene(800, 320).Texts.Single(t => t.Text == "Q2");
        var q2 = kept.Skip(4).Take(3).Select(b => b.Rect).ToList();
        Assert.True(q2[1].Left - q2[0].Right > 16, "Europe's slot stays empty");
        chart.PreserveSpaceForMissingBars = false;
        var closed = Bars(chart.RenderScene(800, 320)).Skip(4).Take(3).Select(b => b.Rect).ToList();
        Assert.InRange(closed[1].Left - closed[0].Right, 0, 5.01);
        Assert.Equal(label.X, (closed[0].Left + closed[2].Right) / 2, 3.0);
    }

    [Fact]
    public void Negative_values_grow_down_from_zero()
    {
        var chart = Chart([new("Hardware", "Q1", 4.2), new("Hardware", "Q2", -1.8), new("Software", "Q1", 8.1), new("Software", "Q2", -0.9)]);
        var scene = chart.RenderScene(600, 300);
        var zero = scene.Shapes.OfType<ChartLine>().Single(l => l.Stroke == ChartPaint.OfKey(CdsColorKeys.BorderStrong01) && l.From.Y == l.To.Y && l.From.Y > scene.Plot.Top + 1 && l.From.Y < scene.Plot.Bottom - 1);
        var bars = Bars(scene);
        var negative = bars[2].Rect; // marks run Q1 Hardware, Q1 Software, Q2 Hardware, Q2 Software
        Assert.Equal(zero.From.Y, bars[2].Rect.Top, 1.0);
        Assert.True(bars[0].Rect.Bottom <= zero.From.Y + 1);
        Assert.True(negative.Height > 0);
    }

    [Fact]
    public void Time_axis_groups_by_date()
    {
        var data = new[] { 310d, 284, 342 }.Select((v, i) => new CdsChartPoint("Opened", new DateTime(2024, 1 + i, 1), v))
            .Concat(new[] { 295d, 301, 318 }.Select((v, i) => new CdsChartPoint("Resolved", new DateTime(2024, 1 + i, 1), v)));
        var chart = new CdsGroupedBarChart { Data = data.ToList(), BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time } };
        var scene = chart.RenderScene(700, 300);
        var bars = Bars(scene);
        Assert.Equal(6, bars.Count);
        Assert.Equal("Opened", scene.Marks[0].Group);
        Assert.Equal("Resolved", scene.Marks[1].Group);
        Assert.True(bars[0].Rect.Right <= bars[1].Rect.Left + 0.01);
    }

    [Fact]
    public void Legend_selection_filters_groups_and_closes_the_gap()
    {
        var chart = Chart();
        chart.RenderScene(800, 320);
        chart.ToggleGroup("Europe");
        var scene = chart.RenderScene(800, 320);
        Assert.Equal(4, scene.Marks.Count);
        Assert.All(scene.Marks, m => Assert.Equal("Europe", m.Group));
        var label = scene.Texts.Single(t => t.Text == "Q1");
        Assert.Equal(label.X, Bars(scene)[0].Rect.Center.X, 1.0);
    }

    [Fact]
    public void Table_lists_group_category_and_value()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Group", "Quarter", "Revenue (USD M)"], table.Headers);
        Assert.Equal(16, table.Rows.Count);
        Assert.Equal(["North America", "Q1", "42"], table.Rows[0]);
    }

    [Fact]
    public void Horizontal_bars_stack_groups_down_the_band()
    {
        var chart = new CdsGroupedBarChart { Data = Revenue(), LeftAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels }, BottomAxis = new CdsChartAxis() };
        var bars = Bars(chart.RenderScene(600, 400));
        Assert.All(bars, b => Assert.Equal(bars[0].Rect.Height, b.Rect.Height, 0.01));
        Assert.True(bars[1].Rect.Top >= bars[0].Rect.Bottom - 0.01);
        Assert.Equal(bars[0].Rect.Left, bars[5].Rect.Left, 0.01);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsGroupedBarChart { IsLoading = true };
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
        var chart = new CdsGroupedBarChart
        {
            Title = "Revenue",
            Data = Revenue(),
            BarWidth = 10,
            BarMaxWidth = 20,
            PreserveSpaceForMissingBars = false,
            LegendPosition = CdsChartLegendPosition.Top,
            ColorPairingOption = 3,
            IsXGridEnabled = false,
            IsYGridEnabled = false,
            IsRulerEnabled = false,
            ValueFormat = "N1",
        };
        Assert.Equal(16, chart.RenderScene(500, 300).Marks.Count);
        Assert.Equal(16d, new CdsGroupedBarChart().BarMaxWidth);
        Assert.True(new CdsGroupedBarChart().PreserveSpaceForMissingBars);
        Assert.Null(new CdsGroupedBarChart().BarWidth);
    }
}
