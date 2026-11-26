namespace Maui.Carbon.Tests.Components;

public class LineChartTests
{
    static List<CdsChartPoint> Weekly() =>
    [
        .. new double?[] { 41200, 32100, 52300, null, 61200, 49800 }.Select((v, i) => new CdsChartPoint("Dataset 1", new DateTime(2023, 1, 1).AddDays(7 * i), v)),
        .. new double?[] { 22100, 25600, 21900, 31400, 28300, 35100 }.Select((v, i) => new CdsChartPoint("Dataset 2", new DateTime(2023, 1, 1).AddDays(7 * i), v)),
    ];

    static CdsLineChart Chart() => new()
    {
        Data = Weekly(),
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time },
        LeftAxis = new CdsChartAxis { Title = "Revenue" },
    };

    [Fact]
    public void Ruler_marks_one_column_per_date_with_every_series()
    {
        var scene = Chart().RenderScene(700, 320);
        Assert.Equal(ChartHitMode.Ruler, scene.HitMode);
        Assert.Equal(6, scene.Marks.Count);
        var third = scene.Marks[2];
        Assert.Equal("Total", third.Rows[^1].Label);
        Assert.Equal("74,200", third.Rows[^1].Value);
        Assert.Equal("Dataset 1", third.Rows[1].Label);
        var hit = scene.HitTest(new PointF(third.RulerAt!.Value + 2, 100));
        Assert.Equal(third.Id, hit!.Id);
    }

    [Fact]
    public void Null_values_break_the_line_and_hide_the_point()
    {
        var scene = Chart().RenderScene(700, 320);
        var lines = scene.Shapes.OfType<ChartPath>().Where(p => p.Group is not null).ToList();
        Assert.Equal(2, lines.Count);
        Assert.Equal(11, scene.Shapes.OfType<ChartCircle>().Count());
        Assert.Equal("2 lines", scene.Summary);
    }

    [Fact]
    public void Ruler_off_gives_each_point_its_own_tooltip()
    {
        var chart = Chart();
        chart.IsRulerEnabled = false;
        var scene = chart.RenderScene(700, 320);
        Assert.Equal(ChartHitMode.Contains, scene.HitMode);
        Assert.Equal(11, scene.Marks.Count);
        Assert.Contains(scene.Marks[0].Rows, r => r.Label == "Group");
    }

    [Fact]
    public void Time_axis_ticks_bold_new_months()
    {
        var scene = Chart().RenderScene(700, 320);
        var ticks = scene.Texts.Where(t => t.Font == ChartFont.TickPrimary).Select(t => t.Text).ToList();
        Assert.Contains("Jan 1", ticks);
        Assert.Contains("Feb 5", ticks);
    }

    [Fact]
    public void A_chart_with_only_a_time_axis_gets_a_value_axis()
    {
        var chart = new CdsLineChart { Data = Weekly(), BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time } };
        var scene = chart.RenderScene(600, 300);
        Assert.Equal(6, scene.Marks.Count);
    }

    [Fact]
    public void Step_curve_and_points_options_apply()
    {
        var chart = Chart();
        chart.Curve = CdsChartCurve.StepAfter;
        chart.ShowPoints = false;
        var scene = chart.RenderScene(600, 300);
        Assert.Empty(scene.Shapes.OfType<ChartCircle>());
    }
}
