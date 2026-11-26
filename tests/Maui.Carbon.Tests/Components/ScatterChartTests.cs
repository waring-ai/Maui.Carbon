namespace Maui.Carbon.Tests.Components;

public class ScatterChartTests
{
    static List<CdsChartPoint> Requests() =>
    [
        new("API", 10, 150), new("API", 40, 260), new("API", 80, 410),
        new("Web", 10, 90), new("Web", 55, 300),
    ];

    static CdsScatterChart Chart() => new()
    {
        Data = Requests(),
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Linear, Title = "Size (KB)" },
        LeftAxis = new CdsChartAxis { Title = "Latency (ms)" },
    };

    [Fact]
    public void Dots_are_4px_filled_at_30_percent_and_not_clipped()
    {
        var scene = Chart().RenderScene(600, 320);
        var dots = scene.Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(5, dots.Count);
        Assert.All(dots, d =>
        {
            Assert.Equal(4, d.Radius);
            Assert.Equal(0.3f, d.FillOpacity);
            Assert.Equal(1.5f, d.StrokeWidth);
            Assert.False(d.Clip);
            Assert.False(d.HoverOnly);
        });
        Assert.Equal("5 points", scene.Summary);
    }

    [Fact]
    public void Dots_sit_at_x_and_value()
    {
        var scene = Chart().RenderScene(600, 320);
        var dots = scene.Shapes.OfType<ChartCircle>().ToList();
        // Same x, different y: the API and Web dots at 10 KB.
        Assert.Equal(dots[0].Center.X, dots[3].Center.X, 0.01);
        Assert.True(dots[0].Center.Y < dots[3].Center.Y);
        Assert.True(dots[2].Center.X > dots[1].Center.X);
        Assert.Contains(scene.Texts, t => t.Text == "Size (KB)");
    }

    [Fact]
    public void Ruler_columns_group_dots_at_the_same_x()
    {
        var scene = Chart().RenderScene(600, 320);
        Assert.Equal(ChartHitMode.Ruler, scene.HitMode);
        Assert.Equal(4, scene.Marks.Count);
        var first = scene.Marks[0];
        Assert.Equal(["Size (KB)", "API", "Web", "Total"], first.Rows.Select(r => r.Label));
        Assert.Equal(2, scene.Shapes.OfType<ChartCircle>().Count(d => d.Mark == first.Id));
    }

    [Fact]
    public void Ruler_off_gives_each_dot_its_tooltip_and_description()
    {
        var chart = Chart();
        chart.IsRulerEnabled = false;
        var scene = chart.RenderScene(600, 320);
        Assert.Equal(ChartHitMode.Contains, scene.HitMode);
        Assert.Equal(5, scene.Marks.Count);
        var m = scene.Marks[0];
        Assert.Equal(["Size (KB)", "Latency (ms)", "Group"], m.Rows.Select(r => r.Label));
        Assert.Equal("10, 150, API", m.Description);
        var dot = scene.Shapes.OfType<ChartCircle>().First();
        Assert.Same(m, scene.HitTest(dot.Center));
    }

    [Fact]
    public void Unfilled_and_per_point_fill()
    {
        var chart = Chart();
        chart.ArePointsFilled = false;
        chart.IsPointFilled = p => p.Value > 400;
        var dots = chart.RenderScene(600, 320).Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(1, dots.Count(d => d.Fill.Kind == ChartPaintKind.Series));
        Assert.Equal(4, dots.Count(d => d.Fill == ChartPaint.OfKey(CdsColorKeys.Layer01)));
    }

    [Fact]
    public void Threshold_anomalies_get_a_3px_stroke()
    {
        var chart = Chart();
        var axis = new CdsChartAxis { Title = "Latency (ms)" };
        axis.Thresholds.Add(new CdsChartThreshold { Value = 250, Label = "Alert", ColorKey = CdsColorKeys.SupportError });
        axis.Thresholds.Add(new CdsChartThreshold { Value = 100 });
        chart.LeftAxis = axis;
        var scene = chart.RenderScene(600, 320);
        var thick = scene.Shapes.OfType<ChartCircle>().Where(d => d.StrokeWidth == 3).ToList();
        Assert.Equal(3, thick.Count);
        Assert.Contains(scene.Marks, m => m.IsReference && m.Title == "Alert: 250");
    }

    [Fact]
    public void Both_axis_thresholds_must_be_passed()
    {
        var chart = Chart();
        var left = new CdsChartAxis();
        left.Thresholds.Add(new CdsChartThreshold { Value = 250 });
        var bottom = new CdsChartAxis { ScaleType = CdsChartScaleType.Linear };
        bottom.Thresholds.Add(new CdsChartThreshold { Value = 60 });
        chart.LeftAxis = left;
        chart.BottomAxis = bottom;
        Assert.Single(chart.RenderScene(600, 320).Shapes.OfType<ChartCircle>(), d => d.StrokeWidth == 3);
    }

    [Fact]
    public void Time_and_labels_domains()
    {
        var time = new CdsScatterChart
        {
            Data = new[] { new CdsChartPoint("node-a", new DateTime(2024, 6, 1, 6, 0, 0), 40), new CdsChartPoint("node-a", new DateTime(2024, 6, 2), 70) },
            BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time },
        };
        Assert.Equal(2, time.RenderScene(500, 300).Shapes.OfType<ChartCircle>().Count());
        var labels = new CdsScatterChart
        {
            Data = new[] { new CdsChartPoint("Q1", "North", 40), new CdsChartPoint("Q1", "North", 70), new CdsChartPoint("Q2", "South", 20) },
            BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels },
        };
        var scene = labels.RenderScene(500, 300);
        Assert.Equal(2, scene.Marks.Count);
        Assert.Equal(3, scene.Shapes.OfType<ChartCircle>().Count());
    }

    [Fact]
    public void Legend_selection_filters_points_and_table_lists_them()
    {
        var chart = Chart();
        chart.RenderScene(600, 320);
        chart.ToggleGroup("Web");
        Assert.Equal(2, chart.RenderScene(600, 320).Shapes.OfType<ChartCircle>().Count());
        var table = chart.RenderTable();
        Assert.Equal(["Group", "Size (KB)", "Latency (ms)"], table.Headers);
        Assert.Equal(["Web", "10", "90"], table.Rows[0]);
    }

    [Fact]
    public void Empty_and_loading_show_the_skeleton()
    {
        var chart = new CdsScatterChart { Data = Array.Empty<CdsChartPoint>() };
        Assert.Equal("No data", chart.RenderScene(400, 300).Summary);
        chart.IsLoading = true;
        Assert.True(chart.RenderScene(400, 300).Shimmer);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsScatterChart
        {
            Data = Requests(),
            ShowPoints = false,
            PointRadius = 6,
            ArePointsFilled = false,
            PointFillOpacity = 2,
            IsPointFilled = _ => true,
            IsRulerEnabled = false,
        };
        Assert.Equal(1, chart.PointFillOpacity);
        var scene = chart.RenderScene(500, 300);
        Assert.Empty(scene.Shapes.OfType<ChartCircle>());
        Assert.Empty(scene.Marks);
    }
}
