namespace Maui.Carbon.Tests.Components;

public class BulletChartTests
{
    static List<CdsBulletChartItem> Reps() =>
    [
        new("North", "On track", 872, 900, 540, 765, 900),
        new("South", "On track", 745, 800, 480, 680, 800),
        new("East", "Behind", 612, 1000, 600, 850, 1000),
        new("West", "Behind", 388, 700, 420, 595, 700),
    ];

    static CdsBulletChart Chart() => new() { Data = Reps() };

    static List<ChartRect> Bars(ChartScene scene) => scene.Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0).ToList();

    [Theory]
    [InlineData(500, 0)]
    [InlineData(765, 1)]
    [InlineData(872, 1)]
    [InlineData(900, 2)]
    [InlineData(1200, 2)]
    public void Performance_index_finds_the_area(double value, int index)
    {
        Assert.Equal(index, CdsBulletChart.PerformanceIndex([540, 765, 900], value));
    }

    [Fact]
    public void Performance_index_without_ranges_is_the_first_area()
    {
        Assert.Equal(0, CdsBulletChart.PerformanceIndex(null, 10));
    }

    [Fact]
    public void Rows_draw_areas_a_bar_a_target_and_quartiles()
    {
        var scene = Chart().RenderScene(600, 300);
        Assert.Equal("4 bars", scene.Summary);
        var bars = Bars(scene);
        Assert.Equal(4, bars.Count);
        Assert.All(bars, b => Assert.Equal(8, b.Rect.Height, 0.01));
        Assert.All(bars, b => Assert.Equal(scene.Plot.Left + 1, b.Rect.Left, 0.5));
        var zones = scene.Shapes.OfType<ChartRect>().Where(r => r.Fill.Kind == ChartPaintKind.Token && r.Stroke == ChartPaint.OfToken(CdsChartToken.ZoneStroke)).ToList();
        Assert.Equal(12, zones.Count);
        Assert.All(zones, z => Assert.Equal(16, z.Rect.Height, 0.01));
        Assert.Equal(4, zones.Count(z => z.Fill == ChartPaint.OfToken(CdsChartToken.ZoneFill01)));
        Assert.Equal(4, zones.Count(z => z.Fill == ChartPaint.OfToken(CdsChartToken.ZoneFill03)));
        var first = zones[0].Rect;
        Assert.Equal(scene.Plot.Left + 1, first.Left, 0.5);
        Assert.Equal(scene.Plot.Right - 2, first.Right, 0.5);
        var strokes = scene.Shapes.OfType<ChartLine>().Where(l => l.StrokeWidth == 1.5f).ToList();
        Assert.Equal(4 + 12, strokes.Count);
        var target = strokes[0];
        Assert.Equal(24, Math.Abs(target.To.Y - target.From.Y), 0.01);
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute), target.Stroke);
    }

    [Fact]
    public void Quartiles_switch_color_over_the_bar_and_lengthen_on_a_range()
    {
        var chart = new CdsBulletChart { Data = new[] { new CdsBulletChartItem("West", "Behind", 388, 700, 420, 595, 700) } };
        var ticks = chart.RenderScene(600, 200).Shapes.OfType<ChartLine>().Where(l => l.StrokeWidth == 1.5f).Skip(1).ToList();
        Assert.Equal(3, ticks.Count);
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.Layer01Absolute), ticks[0].Stroke); // 175 is under the bar
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.Layer01Absolute), ticks[1].Stroke); // 350
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute), ticks[2].Stroke); // 525
        Assert.All(ticks, t => Assert.Equal(4, Math.Abs(t.To.Y - t.From.Y), 0.01));
        var onRange = new CdsBulletChart { Data = new[] { new CdsBulletChartItem("A", "g", 10, 100, 25, 50, 75) } };
        var long_ = onRange.RenderScene(600, 200).Shapes.OfType<ChartLine>().Where(l => l.StrokeWidth == 1.5f).Skip(1).ToList();
        Assert.All(long_, t => Assert.Equal(8, Math.Abs(t.To.Y - t.From.Y), 0.01));
    }

    [Fact]
    public void Tooltips_show_value_target_percentage_and_performance()
    {
        var mark = Chart().RenderScene(600, 300).Marks[0];
        Assert.Equal(["Group", "Value", "Target", "Percentage", "Performance"], mark.Rows.Select(r => r.Label));
        Assert.Equal(["On track", "872", "900", "96%", "Satisfactory"], mark.Rows.Select(r => r.Value));
        Assert.Equal("North: 872, target 900, 96% of target, Satisfactory, On track", mark.Description);
    }

    [Fact]
    public void Rows_without_a_target_drop_target_and_percentage()
    {
        var chart = new CdsBulletChart { Data = new[] { new CdsBulletChartItem { Title = "Uptime", Group = "SLA", Value = 99 } } };
        var scene = chart.RenderScene(600, 200);
        Assert.Equal(["Group", "Value", "Performance"], scene.Marks[0].Rows.Select(r => r.Label));
        Assert.Single(scene.Shapes.OfType<ChartRect>(), r => r.Fill == ChartPaint.OfToken(CdsChartToken.ZoneFill01));
    }

    [Fact]
    public void Value_axis_reaches_the_target()
    {
        var chart = new CdsBulletChart { Data = new[] { new CdsBulletChartItem("A", "g", 50, 200) } };
        var scene = chart.RenderScene(600, 200);
        Assert.Contains(scene.Texts, t => t.Text == "200");
    }

    [Fact]
    public void Grid_is_off_by_default()
    {
        var scene = Chart().RenderScene(600, 300);
        Assert.DoesNotContain(scene.Shapes.OfType<ChartLine>(), l => l.Stroke == ChartPaint.OfKey(CdsColorKeys.LayerAccent01));
    }

    [Fact]
    public void Legend_lists_groups_then_areas_and_quartiles()
    {
        var chart = Chart();
        chart.RenderScene(600, 300);
        Assert.True(chart.LegendShown);
        chart.AreaLegendTitles = ["Below plan", "On plan", "Quota met"];
        chart.QuartileLegendTitle = "Quartiles of quota";
        chart.PerformanceAreaTitles = ["Below plan", "On plan", "Quota met"];
        var scene = chart.RenderScene(600, 300);
        Assert.Equal("On plan", scene.Marks[0].Rows[^1].Value);
    }

    [Fact]
    public void Legend_selection_filters_rows()
    {
        var chart = Chart();
        chart.RenderScene(600, 300);
        chart.ToggleGroup("Behind");
        var scene = chart.RenderScene(600, 300);
        Assert.Equal(2, scene.Marks.Count);
        Assert.DoesNotContain(scene.Texts, t => t.Text == "North");
    }

    [Fact]
    public void Table_lists_title_group_value_target_percentage_and_performance()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Title", "Group", "Value", "Target", "Percentage", "Performance"], table.Headers);
        Assert.Equal(["West", "Behind", "388", "700", "55%", "Poor"], table.Rows[3]);
    }

    [Fact]
    public void Item_constructors_set_every_field()
    {
        var item = new CdsBulletChartItem("North", "On track", 872, 900, 540, 765, 900) { Tag = 1 };
        Assert.Equal(("North", "On track", 872d, 900d), (item.Title, item.Group, item.Value!.Value, item.Target!.Value));
        Assert.Equal([540d, 765, 900], item.Ranges!);
        Assert.Null(new CdsBulletChartItem("a", "b", 1, 2).Ranges);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsBulletChart { IsLoading = true };
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
        var chart = new CdsBulletChart
        {
            Data = Reps(),
            PerformanceAreaTitles = null,
            AreaLegendTitles = [],
            QuartileLegendTitle = null,
            IsXGridEnabled = true,
            IsYGridEnabled = true,
        };
        Assert.Equal(4, chart.RenderScene(500, 300).Marks.Count);
        var d = new CdsBulletChart();
        Assert.Equal(["Poor", "Satisfactory", "Great"], d.PerformanceAreaTitles!);
        Assert.Equal(["Poor area", "Satisfactory area", "Great area"], d.AreaLegendTitles!);
        Assert.Equal("Quartiles", d.QuartileLegendTitle);
    }
}
