namespace Maui.Carbon.Tests.Components;

public class HeatmapChartTests
{
    // 3 columns x 2 rows; (Sev 3, Billing) missing.
    static List<CdsChartPoint> Incidents() =>
    [
        new("Auth", "Sev 1", 1), new("Auth", "Sev 2", 4), new("Auth", "Sev 3", 9),
        new("Billing", "Sev 1", 2), new("Billing", "Sev 2", 6),
    ];

    static CdsHeatmapChart Chart(IEnumerable<CdsChartPoint>? data = null) => new()
    {
        Data = (data ?? Incidents()).ToList(),
        BottomAxis = new CdsChartAxis { Title = "Severity" },
        LeftAxis = new CdsChartAxis { Title = "Service" },
        ColorLegendTitle = "Open incidents",
    };

    static List<ChartRect> Cells(ChartScene scene) => scene.Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0 && !r.HoverOnly).ToList();

    [Fact]
    public void Cells_fill_the_plot_edge_to_edge_with_the_first_row_at_the_bottom()
    {
        var scene = Chart().RenderScene(600, 400);
        var cells = Cells(scene);
        Assert.Equal(5, cells.Count);
        var p = scene.Plot;
        Assert.All(cells, c => Assert.Equal(p.Width / 3, c.Rect.Width, 0.01));
        Assert.All(cells, c => Assert.Equal(p.Height / 2, c.Rect.Height, 0.01));
        // Reading order: the top row (Billing, the second row) first.
        var first = scene.Marks[0];
        Assert.Equal("Sev 1, Billing, 2", first.Description);
        Assert.Equal(p.Top, first.Bounds.Top, 0.01);
        Assert.Equal(p.Left, first.Bounds.Left, 0.01);
        var auth3 = scene.Marks.Single(m => m.Description == "Sev 3, Auth, 9");
        Assert.Equal(p.Bottom, auth3.Bounds.Bottom, 0.01);
        Assert.Equal(p.Right, auth3.Bounds.Right, 0.01);
        Assert.StartsWith("5 cells, 1 without data", scene.Summary);
    }

    [Fact]
    public void Values_take_quantized_sequential_steps()
    {
        var scene = Chart().RenderScene(600, 400);
        // Domain 0..9 (nice extent from 0): 9 takes the darkest step, 1 the second.
        Assert.Equal(ChartPaint.OfSequential(1, 10), Cells(scene).Single(c => scene.FindMark(c.Mark)!.Description!.EndsWith(", 9")).Fill);
        Assert.Equal(ChartPaint.OfSequential(1, 1), Cells(scene).Single(c => scene.FindMark(c.Mark)!.Description!.EndsWith(", 1")).Fill);
        var chart = Chart();
        chart.ColorPairingOption = 4;
        var teal = chart.RenderScene(600, 400);
        Assert.All(Cells(teal), c => Assert.Equal(ChartPaintKind.Sequential, c.Fill.Kind));
        Assert.All(Cells(teal), c => Assert.Equal(4, c.Fill.A));
    }

    [Fact]
    public void Mixed_signs_switch_to_the_diverging_palette()
    {
        var chart = Chart([new("2015", "Jan", -1.6), new("2015", "Feb", 1.2), new("2016", "Jan", 0)]);
        chart.ColorLegendType = CdsChartColorLegendType.Quantize;
        var scene = chart.RenderScene(600, 400);
        Assert.All(Cells(scene), c => Assert.Equal(ChartPaintKind.Diverging, c.Fill.Kind));
        // -1.6..1.6: zero is the middle step.
        var zero = Cells(scene).Single(c => scene.FindMark(c.Mark)!.Description!.EndsWith(", 0"));
        Assert.Equal(8, zero.Fill.B);
        // Quantized legend without the white midpoint: 16 blocks.
        var blocks = scene.Shapes.OfType<ChartRect>().Where(r => r.Rect.Height == 8 && r.Fill.Kind == ChartPaintKind.Diverging).ToList();
        Assert.Equal(16, blocks.Count);
        Assert.DoesNotContain(blocks, b => b.Fill.B == 8);
        Assert.Contains(scene.Texts, t => t.Text == "-1.6");
        Assert.Contains(scene.Texts, t => t.Text == "1.6");
    }

    [Fact]
    public void Color_domain_overrides_the_extent()
    {
        var chart = Chart([new("NA", "Q1", 62), new("NA", "Q2", 75), new("EU", "Q1", 99)]);
        chart.ColorDomainMinimum = 50;
        chart.ColorDomainMaximum = 100;
        var scene = chart.RenderScene(600, 400);
        Assert.Equal(ChartPaint.OfSequential(1, 2), Cells(scene).Single(c => scene.FindMark(c.Mark)!.Description!.EndsWith(", 62")).Fill);
        Assert.Contains(scene.Texts, t => t.Text == "50");
        Assert.Contains(scene.Texts, t => t.Text == "75");
        Assert.Contains(scene.Texts, t => t.Text == "100");
    }

    [Fact]
    public void Cell_tooltip_shows_column_row_and_value_with_its_color()
    {
        var scene = Chart().RenderScene(600, 400);
        var m = scene.Marks.Single(x => x.Description == "Sev 2, Auth, 4");
        Assert.Equal(3, m.Rows.Count);
        Assert.Equal(("Severity", "Sev 2"), (m.Rows[0].Label, m.Rows[0].Value));
        Assert.Equal(("Service", "Auth"), (m.Rows[1].Label, m.Rows[1].Value));
        Assert.Equal(("Total", "4"), (m.Rows[2].Label, m.Rows[2].Value));
        Assert.Equal(ChartPaintKind.Sequential, m.Rows[2].Color.Kind);
        // Hover outline: white plus shadow, only while active.
        var outline = scene.Shapes.OfType<ChartRect>().Where(s => s.Mark == m.Id && s.HoverOnly).ToList();
        Assert.Equal(2, outline.Count);
        Assert.Contains(outline, o => o.Stroke == ChartPaint.OfKey(CdsColorKeys.TextOnColor));
    }

    [Fact]
    public void Axis_labels_summarize_their_row_or_column()
    {
        var scene = Chart().RenderScene(600, 400);
        var sev2 = scene.Marks.Single(m => m.Rows.Count > 0 && m.Rows[0].Bold && m.Rows[0].Value == "Sev 2");
        Assert.Equal(["Min", "Max", "Average"], sev2.Rows.Skip(1).Select(r => r.Label));
        Assert.Equal(["4", "6", "5"], sev2.Rows.Skip(1).Select(r => r.Value));
        var sev3 = scene.Marks.Single(m => m.Rows.Count > 0 && m.Rows[0].Bold && m.Rows[0].Value == "Sev 3");
        Assert.Equal("9", sev3.Rows[3].Value);
        var auth = scene.Marks.Single(m => m.Rows.Count > 0 && m.Rows[0].Bold && m.Rows[0].Value == "Auth");
        Assert.Equal("Service", auth.Rows[0].Label);
        Assert.True(auth.Bounds.Right <= scene.Plot.Left);
        // 5 cells + 3 column labels + 2 row labels.
        Assert.Equal(10, scene.Marks.Count);
    }

    [Fact]
    public void Null_cells_are_hatched_and_have_no_mark()
    {
        var scene = Chart().RenderScene(600, 400);
        var hatch = Assert.Single(scene.Shapes.OfType<ChartPath>(), s => s.Stroke == ChartPaint.OfKey(CdsColorKeys.BorderStrong01));
        Assert.Equal(0.5f, hatch.StrokeWidth);
        Assert.True(hatch.Path.Count > 10);
        Assert.DoesNotContain(scene.Marks, m => m.Description?.StartsWith("Sev 3, Billing") == true);
    }

    [Fact]
    public void Dividers_follow_cell_size()
    {
        Assert.All(Cells(Chart().RenderScene(600, 400)), c => Assert.Equal(ChartPaint.OfKey(CdsColorKeys.Background), c.Stroke));
        var many = Enumerable.Range(0, 60).Select(i => new CdsChartPoint("Row", "C" + i, i)).ToList();
        Assert.All(Cells(Chart(many).RenderScene(600, 400)), c => Assert.True(c.Stroke.IsNone));
        var off = Chart();
        off.Divider = CdsHeatmapDivider.Off;
        Assert.All(Cells(off.RenderScene(600, 400)), c => Assert.True(c.Stroke.IsNone));
    }

    [Fact]
    public void Color_legend_sits_under_the_plot_and_hides_with_the_legend()
    {
        var chart = Chart();
        var scene = chart.RenderScene(600, 400);
        Assert.Contains(scene.Texts, t => t.Text == "Open incidents");
        var bar = scene.Shapes.OfType<ChartRect>().Where(r => r.Rect.Height == 8).ToList();
        Assert.NotEmpty(bar);
        Assert.Equal(scene.Plot.Left, bar.Min(r => r.Rect.Left), 0.5);
        Assert.Equal(300, bar.Max(r => r.Rect.Right) - bar.Min(r => r.Rect.Left), 0.5);
        Assert.True(bar.Min(r => r.Rect.Top) > scene.Plot.Bottom + 15);
        chart.IsLegendEnabled = false;
        var off = chart.RenderScene(600, 400);
        Assert.DoesNotContain(off.Texts, t => t.Text == "Open incidents");
        Assert.True(off.Plot.Height > scene.Plot.Height);
    }

    [Fact]
    public void Axis_categories_fix_the_order()
    {
        var chart = Chart();
        chart.BottomAxis = new CdsChartAxis { Title = "Severity", Categories = ["Sev 3", "Sev 2", "Sev 1"] };
        var scene = chart.RenderScene(600, 400);
        var sev3 = scene.Texts.Single(t => t.Text == "Sev 3");
        var sev1 = scene.Texts.Single(t => t.Text == "Sev 1");
        Assert.True(sev3.X < sev1.X);
    }

    [Fact]
    public void Table_lists_column_row_and_value()
    {
        var chart = Chart([new("Auth", "Sev 1", 1), new("Auth", "Sev 2", null)]);
        var table = chart.RenderTable();
        Assert.Equal(["Severity", "Service", "Value"], table.Headers);
        Assert.Equal(["Sev 1", "Auth", "1"], table.Rows[0]);
        Assert.Equal(["Sev 2", "Auth", "–"], table.Rows[1]);
    }

    [Fact]
    public void Loading_and_empty_show_the_grid_skeleton()
    {
        var chart = new CdsHeatmapChart { IsLoading = true };
        Assert.True(chart.RenderScene(400, 300).Shimmer);
        chart.IsLoading = false;
        var empty = chart.RenderScene(400, 300);
        Assert.Equal("No data", empty.Summary);
        Assert.Empty(empty.Marks);
        Assert.False(chart.LegendShown);
    }

    [Fact]
    public void Custom_gradient_colors_replace_the_palette()
    {
        var chart = Chart();
        chart.GradientColors = [Colors.White, Colors.Gray, Colors.Black];
        var scene = chart.RenderScene(600, 400);
        Assert.All(Cells(scene), c => Assert.Equal(ChartPaintKind.Color, c.Fill.Kind));
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsHeatmapChart
        {
            Title = "Sessions",
            Data = Incidents(),
            BottomAxis = new CdsChartAxis { Title = "Hour", TickRotation = CdsChartTickRotation.Always },
            LeftAxis = new CdsChartAxis { Title = "Day", IsVisible = false },
            ColorLegendTitle = "Sessions (k)",
            ColorLegendType = CdsChartColorLegendType.Quantize,
            ColorDomainMinimum = 0,
            ColorDomainMaximum = 20,
            Divider = CdsHeatmapDivider.On,
            GradientColors = null,
            ColorPairingOption = 2,
            IsLegendEnabled = false,
            IsLoading = false,
            ValueFormat = "N1",
        };
        Assert.NotEmpty(chart.RenderScene(500, 300).Marks);
        Assert.NotNull(chart.RenderScene(1, 1));
    }
}

public class ChartColorScaleTests
{
    [Theory]
    [InlineData(new[] { 0.8, 9.6 }, 0, 10)]
    [InlineData(new[] { 3.0, 18.0 }, 0, 18)]
    [InlineData(new[] { -1.6, 1.2 }, -1.6, 1.6)]
    [InlineData(new[] { 5.0 }, 0, 5)]
    [InlineData(new[] { -0.4, 3.0 }, -3, 3)]
    [InlineData(new double[0], 0, 1)]
    [InlineData(new[] { 0.0, 0.0 }, 0, 1)]
    [InlineData(new[] { -5.0, -1.0 }, -5, -1)]
    public void Domain_is_nice_from_zero_or_symmetric(double[] values, double lo, double hi)
    {
        var (a, b) = ChartColorScale.Domain(values, null, null);
        Assert.Equal(lo, a, 6);
        Assert.Equal(hi, b, 6);
    }

    [Fact]
    public void Custom_ends_override_the_domain()
    {
        Assert.Equal((50d, 100d), ChartColorScale.Domain([62, 99], 50, 100));
        Assert.Equal((0d, 20d), ChartColorScale.Domain([3, 9], null, 20));
    }

    [Fact]
    public void Quantize_steps_split_the_domain_evenly_and_clamp()
    {
        var s = ChartColorScale.Create([0, 11], null, null, 1, null);
        Assert.Equal(11, s.Count);
        Assert.False(s.Diverging);
        Assert.Equal(0, s.Step(0));
        Assert.Equal(0, s.Step(-5));
        Assert.Equal(1, s.Step(1.0001));
        Assert.Equal(10, s.Step(11));
        Assert.Equal(10, s.Step(500));
        var d = ChartColorScale.Create([-17, 17], null, null, 2, null);
        Assert.True(d.Diverging);
        Assert.Equal(17, d.Count);
        Assert.Equal(8, d.Step(0));
        Assert.Equal((-20d, 20d), (d.D0, d.D1));
        Assert.Equal(ChartPaint.OfDiverging(2, 16), d.Paint(20));
    }

    [Fact]
    public void Out_of_range_options_fall_back_to_one()
    {
        Assert.Equal(1, ChartColorScale.Create([1, 2], null, null, 7, null).Option);
        Assert.Equal(1, ChartColorScale.Create([-1, 2], null, null, 3, null).Option);
        Assert.Equal(4, ChartColorScale.Create([1, 2], null, null, 4, null).Option);
    }

    [Fact]
    public void Legend_title_goes_beside_the_bar_when_it_fits()
    {
        var beside = ChartColorLegend.Measure(600, "°C", 60, heatmap: true);
        Assert.Equal(0, beside.Y);
        Assert.Equal(60, beside.X);
        Assert.True(beside.TitleX > 0 && beside.TitleX < 60);
        var above = ChartColorLegend.Measure(600, "A rather long legend title", 60, heatmap: true);
        Assert.Equal(16, above.Y);
        Assert.Equal(60, above.TitleX);
        var map = ChartColorLegend.Measure(600, "% of land", 0, heatmap: false);
        Assert.Equal((0f, 16f, 300f), (map.X, map.Y, map.BarWidth));
        Assert.Equal(43, map.Height);
        var narrow = ChartColorLegend.Measure(200, null, 0, heatmap: false);
        Assert.Equal(200, narrow.BarWidth);
        Assert.False(narrow.UseDefault);
        Assert.Equal(27, narrow.Height);
    }

    [Fact]
    public void Hatch_lines_stay_inside_the_polygon()
    {
        var square = ChartHatch.Rect(new RectF(10, 10, 40, 40));
        var path = ChartHatch.Lines([square], 5);
        Assert.True(path.Count >= 20);
        for (var i = 0; i < path.Count; i++)
        {
            var p = path[i];
            Assert.InRange(p.X, 9.99f, 50.01f);
            Assert.InRange(p.Y, 9.99f, 50.01f);
        }
        // Every segment is a "/" diagonal: x + y is constant along it.
        for (var i = 0; i + 1 < path.Count; i += 2)
            Assert.Equal(path[i].X + path[i].Y, path[i + 1].X + path[i + 1].Y, 0.01);
        // A hole (even-odd) gets no hatch.
        var hole = ChartHatch.Rect(new RectF(20, 20, 20, 20));
        var holed = ChartHatch.Lines([square, hole], 5);
        for (var i = 0; i + 1 < holed.Count; i += 2)
        {
            var mid = new PointF((holed[i].X + holed[i + 1].X) / 2, (holed[i].Y + holed[i + 1].Y) / 2);
            Assert.False(mid.X > 20.5f && mid.X < 39.5f && mid.Y > 20.5f && mid.Y < 39.5f);
        }
    }
}
