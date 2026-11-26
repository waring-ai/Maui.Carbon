namespace Maui.Carbon.Tests.Components;

public class AreaChartTests
{
    static List<CdsChartPoint> Traffic() =>
    [
        .. new double?[] { 42, 48, 45, null, 61, 58 }.Select((v, i) => new CdsChartPoint("EMEA", new DateTime(2024, 1 + i, 1), v)),
        .. new double?[] { 28, 31, 35, 33, 39, 44 }.Select((v, i) => new CdsChartPoint("Americas", new DateTime(2024, 1 + i, 1), v)),
    ];

    static CdsAreaChart Chart() => new()
    {
        Data = Traffic(),
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time },
        LeftAxis = new CdsChartAxis { Title = "Traffic (TB)" },
    };

    static List<ChartPath> Areas(ChartScene s) => s.Shapes.OfType<ChartPath>().Where(p => (!p.Fill.IsNone || p.FillGradient is not null) && p.Group is not null).ToList();

    [Fact]
    public void Each_group_draws_a_line_and_a_40_percent_area_with_a_1px_edge()
    {
        var scene = Chart().RenderScene(700, 320);
        var areas = Areas(scene);
        Assert.Equal(2, areas.Count);
        Assert.All(areas, a =>
        {
            Assert.Equal(0.4f, a.Opacity);
            Assert.Equal(1, a.StrokeWidth);
            Assert.Equal(0.05f, a.Opacity * a.LegendFade, 3);
        });
        var lines = scene.Shapes.OfType<ChartPath>().Where(p => p.Fill.IsNone && p.Group is not null).ToList();
        Assert.Equal(2, lines.Count);
        Assert.All(lines, l => Assert.Equal(1.5f, l.StrokeWidth));
        // Lines first, areas over them (Carbon's order).
        Assert.True(scene.Shapes.IndexOf(lines[0]) < scene.Shapes.IndexOf(areas[0]));
        Assert.Equal("2 areas", scene.Summary);
    }

    [Fact]
    public void Areas_fill_down_to_zero_and_the_time_axis_runs_edge_to_edge()
    {
        var chart = Chart();
        var scene = chart.RenderScene(700, 320);
        var cs = chart.Layout(new ChartBuildContext(700, 320, Traffic().Cast<object>().ToList(), ["EMEA", "Americas"], [], new ChartColors(new CdsChartPalette(CdsThemeName.White, 1), ["EMEA", "Americas"], 1, null), System.Globalization.CultureInfo.InvariantCulture));
        var americas = Areas(scene)[1].Path.Bounds;
        Assert.Equal(cs.RangePx("Americas", 0), americas.Bottom, 0.5);
        Assert.Equal(scene.Plot.Left, americas.Left, 0.5);
        Assert.Equal(scene.Plot.Right, americas.Right, 0.5);
    }

    [Fact]
    public void A_null_value_splits_the_area()
    {
        var scene = Chart().RenderScene(700, 320);
        var emea = Areas(scene)[0].Path;
        Assert.Equal(2, emea.SubPathCount);
    }

    [Fact]
    public void Include_zero_off_grows_areas_from_the_axis_minimum()
    {
        var chart = Chart();
        chart.LeftAxis = new CdsChartAxis { IncludeZero = false };
        var scene = chart.RenderScene(700, 320);
        Assert.Equal(scene.Plot.Bottom, Areas(scene)[1].Path.Bounds.Bottom, 0.5);
    }

    [Fact]
    public void Ruler_lists_every_group_with_a_total_and_points_appear_only_under_it()
    {
        var scene = Chart().RenderScene(700, 320);
        Assert.Equal(ChartHitMode.Ruler, scene.HitMode);
        Assert.Equal(6, scene.Marks.Count);
        var first = scene.Marks[0];
        Assert.Equal(["x-value", "EMEA", "Americas", "Total"], first.Rows.Select(r => r.Label));
        Assert.Equal("70", first.Rows[^1].Value);
        var dots = scene.Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(11, dots.Count);
        Assert.All(dots, d =>
        {
            Assert.True(d.HoverOnly);
            Assert.Equal(3, d.Radius);
        });
        Assert.Equal(2, dots.Count(d => d.Mark == first.Id));
    }

    [Fact]
    public void Ruler_off_gives_each_point_its_own_tooltip()
    {
        var chart = Chart();
        chart.IsRulerEnabled = false;
        var scene = chart.RenderScene(700, 320);
        Assert.Equal(11, scene.Marks.Count);
        Assert.Equal("Group", scene.Marks[0].Rows[^1].Label);
    }

    static List<CdsChartPoint> Forecast() =>
        new[] { (310, 295, 325), (332, 312, 350), (348, 324, 371), (341, 312, 369) }
            .Select((t, i) => new CdsChartPoint("Orders", new DateTime(2024, 1, 1).AddDays(7 * i), t.Item1) { Low = t.Item2, High = t.Item3 }).ToList();

    [Fact]
    public void Bounds_shade_a_dashed_band_and_tooltips_keep_the_value()
    {
        var chart = new CdsAreaChart { Data = Forecast(), BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time }, LeftAxis = new CdsChartAxis { IncludeZero = false } };
        var scene = chart.RenderScene(600, 300);
        var band = Assert.Single(Areas(scene));
        Assert.Equal([2f, 2f], band.Dash!);
        Assert.Equal(0.7f, band.StrokeWidth);
        Assert.Equal(0.4f, band.FillOpacity);
        Assert.Equal(1f, band.Opacity);
        var mark = scene.Marks[2];
        Assert.Contains(mark.Rows, r => r.Value == "348");
        Assert.DoesNotContain(mark.Rows, r => r.Label == "Start");
        Assert.Contains("324 to 371", mark.Description);
        // The value domain covers the bounds: the band's top is the highest upper bound.
        Assert.True(band.Path.Bounds.Top > scene.Plot.Top);
    }

    [Fact]
    public void Bounds_add_table_columns()
    {
        var chart = new CdsAreaChart { Data = Forecast(), BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time } };
        var table = chart.RenderTable();
        Assert.Equal(["Group", "Date", "Value", "Upper bound", "Lower bound"], table.Headers);
        Assert.Equal("310", table.Rows[0][2]);
        Assert.Equal("325", table.Rows[0][3]);
        Assert.Equal("295", table.Rows[0][4]);
    }

    [Fact]
    public void Bounds_with_several_groups_draw_plain_areas()
    {
        var data = Forecast().Concat(Forecast().Select(p => new CdsChartPoint("Returns", p.Date!.Value, p.Value / 3))).ToList();
        var chart = new CdsAreaChart { Data = data, BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time } };
        var areas = Areas(chart.RenderScene(600, 300));
        Assert.Equal(2, areas.Count);
        Assert.All(areas, a => Assert.Null(a.Dash));
    }

    [Fact]
    public void Gradient_is_one_area_fading_from_60_percent_over_its_height()
    {
        var data = new double[] { 18, 19, 21, 24, 22 }.Select((v, i) => new CdsChartPoint("Users", new DateTime(2024, 5, 1 + i), v)).ToList();
        var chart = new CdsAreaChart { Data = data, IsGradientEnabled = true, Curve = CdsChartCurve.MonotoneX, BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time } };
        var scene = chart.RenderScene(600, 300);
        var area = Assert.Single(Areas(scene));
        Assert.True(area.Stroke.IsNone);
        Assert.True(area.Fill.IsNone);
        // A continuous vertical gradient (no stacked layers, so no banding): 60% at the top to 0 at 80% of the height.
        var g = area.FillGradient!;
        Assert.Equal(g.Start.X, g.End.X);
        Assert.Equal(area.Path.Bounds.Top, g.Start.Y, 1);
        Assert.Equal(area.Path.Bounds.Bottom, g.End.Y, 1);
        Assert.Equal([(0f, 0.6f), (0.8f, 0f)], g.Stops.Select(s => (s.Offset, s.Opacity)));
        Assert.All(g.Stops, s => Assert.Equal(ChartPaint.OfSeries("Users"), s.Color));
        // Two series turn the gradient off.
        data.Add(new CdsChartPoint("Bots", new DateTime(2024, 5, 1), 3));
        chart.Data = data.ToList();
        Assert.All(Areas(chart.RenderScene(600, 300)), a => Assert.Null(a.FillGradient));
    }

    [Fact]
    public void Gradient_across_zero_fades_both_ways()
    {
        var data = new double[] { 0.4, -0.8, 0.6, -0.3 }.Select((v, i) => new CdsChartPoint("Anomaly", new DateTime(2009 + i, 1, 1), v)).ToList();
        var chart = new CdsAreaChart { Data = data, IsGradientEnabled = true, BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time } };
        var stops = Assert.Single(Areas(chart.RenderScene(600, 300))).FillGradient!.Stops;
        Assert.Equal([0.6f, 0f, 0.6f], stops.Select(s => s.Opacity));
    }

    [Fact]
    public void Gradient_stops_follow_carbon()
    {
        Assert.Equal([(0d, 0.6f), (0.8d, 0f)], ChartAreas.GradientStops(0, 100).Select(s => (s.Offset, (float)s.Opacity)));
        var crossing = ChartAreas.GradientStops(-1, 3);
        Assert.Equal(3, crossing.Length);
        Assert.Equal(0.75, crossing[1].Offset, 4);
    }


    [Fact]
    public void Flatten_passes_through_the_data_points()
    {
        List<PointF> pts = [new(0, 10), new(50, 40), new(100, 20)];
        var flat = ChartAreas.Flatten(pts, CdsChartCurve.MonotoneX);
        Assert.True(flat.Count > 3);
        Assert.Contains(flat, p => Math.Abs(p.X - 50) < 0.01 && Math.Abs(p.Y - 40) < 0.01);
        Assert.Equal(new PointF(100, 20), flat[^1]);
    }

    [Fact]
    public void Threshold_anomalies_get_a_3px_stroke()
    {
        var axis = new CdsChartAxis();
        axis.Thresholds.Add(new CdsChartThreshold { Value = 55 });
        var chart = Chart();
        chart.LeftAxis = axis;
        var dots = chart.RenderScene(700, 320).Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(2, dots.Count(d => d.StrokeWidth == 3));
    }

    [Fact]
    public void Legend_selection_filters_areas()
    {
        var chart = Chart();
        chart.RenderScene(700, 320);
        chart.ToggleGroup("Americas");
        var scene = chart.RenderScene(700, 320);
        Assert.Single(Areas(scene));
        Assert.Equal("1 area", scene.Summary);
        Assert.Equal(6, chart.RenderTable().Rows.Count);
    }

    [Fact]
    public void Loading_and_empty_show_the_grid_skeleton()
    {
        var chart = new CdsAreaChart { IsLoading = true };
        Assert.True(chart.RenderScene(400, 300).Shimmer);
        chart.IsLoading = false;
        var empty = chart.RenderScene(400, 300);
        Assert.Equal("No data", empty.Summary);
        Assert.True(empty.Shapes.OfType<ChartLine>().Count() > 6);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsAreaChart
        {
            Title = "Traffic",
            Data = Traffic(),
            Curve = CdsChartCurve.Natural,
            ShowPoints = false,
            PointRadius = 4,
            ArePointsFilled = true,
            IsGradientEnabled = true,
            IsRulerEnabled = false,
            TimeEdgeSpace = 1,
            IsXGridEnabled = false,
            ValueFormat = "N1",
        };
        Assert.True(chart.ShowPoints == false && chart.IsGradientEnabled);
        Assert.NotNull(chart.RenderScene(500, 300));
        Assert.False(new CdsAreaChart().IsGradientEnabled);
        Assert.True(new CdsAreaChart().ShowPoints);
    }

    [Fact]
    public void Sparse_time_ticks_stay_flat_when_they_fit()
    {
        var data = Enumerable.Range(0, 12).Select(m => new CdsChartPoint("Traffic", new DateTime(2024, 1 + m, 1), 40 + m)).ToList();
        var chart = new CdsAreaChart { Data = data, BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Time } };
        // A small chart takes the fewer, rotated-axis ticks (quarters); they fit flat, so they do not rotate.
        var narrow = chart.RenderScene(245, 220);
        Assert.All(narrow.Texts, t => Assert.Equal(0, t.Rotation));
        Assert.True(narrow.Texts.Count(t => t.Y > narrow.Plot.Bottom) <= 5);
    }
}
