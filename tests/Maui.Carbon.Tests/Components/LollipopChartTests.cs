namespace Maui.Carbon.Tests.Components;

public class LollipopChartTests
{
    static List<CdsChartPoint> Nps() =>
    [
        new("NPS", "Onboarding", 42), new("NPS", "Billing", 18), new("NPS", "Search", 35),
        new("NPS", "Mobile app", 51), new("NPS", "Support", 27), new("NPS", "Reports", 12),
    ];

    static CdsLollipopChart Chart() => new()
    {
        Data = Nps(),
        LeftAxis = new CdsChartAxis { Title = "NPS" },
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels },
    };

    [Fact]
    public void Stems_run_from_the_axis_start_to_the_edge_of_the_dot()
    {
        var scene = Chart().RenderScene(600, 300);
        Assert.Equal("6 lollipops", scene.Summary);
        var stems = scene.Shapes.OfType<ChartLine>().Where(l => l.Group == "NPS").ToList();
        var dots = scene.Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(6, stems.Count);
        Assert.Equal(6, dots.Count);
        for (var i = 0; i < 6; i++)
        {
            Assert.Equal(scene.Plot.Bottom, stems[i].From.Y, 0.01);
            Assert.Equal(dots[i].Center.Y + 4, stems[i].To.Y, 0.01);
            Assert.Equal(dots[i].Center.X, stems[i].From.X, 0.01);
            Assert.Equal(1, stems[i].StrokeWidth);
            Assert.Equal(2f, stems[i].HoverStrokeWidth);
        }
        Assert.All(dots, d =>
        {
            Assert.Equal(4, d.Radius);
            Assert.Equal(0.3f, d.FillOpacity);
            Assert.Equal(ChartPaintKind.Series, d.Fill.Kind);
            Assert.Equal(1.5f, d.StrokeWidth);
        });
    }

    [Fact]
    public void The_ruler_snaps_to_categories_and_highlights_stem_and_dot()
    {
        var scene = Chart().RenderScene(600, 300);
        Assert.Equal(ChartHitMode.Ruler, scene.HitMode);
        Assert.Equal(6, scene.Marks.Count);
        var search = scene.Marks[2];
        Assert.Equal(["x-value", "NPS", "Group"], search.Rows.Select(r => r.Label));
        Assert.Equal(["Search", "35", "NPS"], search.Rows.Select(r => r.Value));
        Assert.Equal(search.Id, scene.HitTest(new PointF(search.RulerAt!.Value + 3, scene.Plot.Top + 5))!.Id);
        Assert.Single(scene.Shapes.OfType<ChartLine>(), l => l.Mark == search.Id);
        Assert.Single(scene.Shapes.OfType<ChartCircle>(), c => c.Mark == search.Id);
    }

    [Fact]
    public void Several_groups_at_a_category_list_every_group_with_the_total()
    {
        var chart = new CdsLollipopChart { Data = new List<CdsChartPoint> { new("Web", "JS", 62.3), new("Data", "JS", 20), new("Web", "Go", 13.5) } };
        var scene = chart.RenderScene(600, 300);
        var js = scene.Marks[0];
        Assert.Equal(["x-value", "Web", "Data", "Total"], js.Rows.Select(r => r.Label));
        Assert.Equal("82.3", js.Rows[^1].Value);
    }

    [Fact]
    public void Horizontal_stems_start_at_the_left()
    {
        var chart = new CdsLollipopChart { Data = Nps(), LeftAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels }, BottomAxis = new CdsChartAxis() };
        var scene = chart.RenderScene(600, 300);
        var stem = scene.Shapes.OfType<ChartLine>().First(l => l.Group == "NPS");
        var dot = scene.Shapes.OfType<ChartCircle>().First();
        Assert.Equal(scene.Plot.Left, stem.From.X, 0.01);
        Assert.Equal(dot.Center.X - 4, stem.To.X, 0.01);
        Assert.False(scene.RulerVertical);
    }

    [Fact]
    public void Ruler_off_gives_each_dot_its_own_tooltip()
    {
        var chart = Chart();
        chart.IsRulerEnabled = false;
        var scene = chart.RenderScene(600, 300);
        Assert.Equal(ChartHitMode.Contains, scene.HitMode);
        Assert.Equal(6, scene.Marks.Count);
        Assert.All(scene.Marks, m => Assert.Null(m.RulerAt));
        Assert.Equal("Onboarding, 42, NPS", scene.Marks[0].Description);
        var dot = scene.Shapes.OfType<ChartCircle>().First();
        Assert.Equal(scene.Marks[0].Id, scene.HitTest(dot.Center)!.Id);
    }

    [Fact]
    public void Point_options_apply()
    {
        var chart = Chart();
        chart.PointRadius = 6;
        chart.ArePointsFilled = false;
        chart.PointFillOpacity = 0.5;
        var scene = chart.RenderScene(600, 300);
        var dot = scene.Shapes.OfType<ChartCircle>().First();
        Assert.Equal(6, dot.Radius);
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.Layer01), dot.Fill);
        var stem = scene.Shapes.OfType<ChartLine>().First(l => l.Group == "NPS");
        Assert.Equal(dot.Center.Y + 6, stem.To.Y, 0.01);
    }

    [Fact]
    public void Legend_selection_filters_groups()
    {
        var chart = new CdsLollipopChart { Data = new List<CdsChartPoint> { new("Web", "JS", 62.3), new("Data", "SQL", 51.5) } };
        chart.RenderScene(600, 300);
        chart.ToggleGroup("Data");
        Assert.Single(chart.RenderScene(600, 300).Shapes.OfType<ChartCircle>());
    }

    [Fact]
    public void Table_lists_every_row()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Group", "Key", "NPS"], table.Headers);
        Assert.Equal(["NPS", "Onboarding", "42"], table.Rows[0]);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsLollipopChart { IsLoading = true };
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
        var chart = new CdsLollipopChart { Data = Nps(), PointRadius = 2, ArePointsFilled = false, PointFillOpacity = 1, IsRulerEnabled = false };
        Assert.Equal(6, chart.RenderScene(500, 300).Marks.Count);
        var d = new CdsLollipopChart();
        Assert.Equal((4d, true, 0.3), (d.PointRadius, d.ArePointsFilled, d.PointFillOpacity));
    }
}
