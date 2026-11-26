using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class ChartCoreTests
{
    static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");

    [Fact]
    public void Linear_ticks_match_d3()
    {
        Assert.Equal([0, 10000, 20000, 30000, 40000, 50000, 60000, 70000], ChartTicks.Ticks(0, 71500, 7));
        Assert.Equal([0, 0.2, 0.4, 0.6, 0.8, 1], ChartTicks.Ticks(0, 1, 5));
        Assert.Equal([-20, -10, 0, 10, 20, 30], ChartTicks.Ticks(-23.5, 34.1, 5));
        Assert.Empty(ChartTicks.Ticks(0, 1, 0));
        Assert.Equal([5], ChartTicks.Ticks(5, 5, 3));
    }

    [Fact]
    public void Linear_nice_extends_to_round_values()
    {
        var (a, b) = ChartTicks.Nice(3.2, 97.5, 10);
        Assert.Equal(0, a);
        Assert.Equal(100, b);
    }

    [Fact]
    public void Time_ticks_pick_d3_intervals()
    {
        var start = ChartTime.ToMs(new DateTime(2023, 1, 1));
        var weekly = ChartTimeTicks.Ticks(start, ChartTime.ToMs(new DateTime(2023, 2, 5)), 7);
        Assert.All(weekly, d => Assert.Equal(DayOfWeek.Sunday, d.DayOfWeek));
        var monthly = ChartTimeTicks.Ticks(start, ChartTime.ToMs(new DateTime(2023, 12, 31)), 7);
        Assert.All(monthly, d => Assert.Equal(1, d.Day));
        Assert.Equal([1, 4, 7, 10], monthly.Select(d => d.Month));
        var hourly = ChartTimeTicks.Ticks(start, ChartTime.ToMs(new DateTime(2023, 1, 1, 12, 0, 0)), 6);
        Assert.Equal([0, 3, 6, 9, 12], hourly.Select(d => d.Hour));
    }

    [Fact]
    public void Band_scale_rounds_and_reverses_vertical_ranges()
    {
        var band = new BandScale(["a", "b", "c", "d"]);
        band.SetRange(0, 400);
        Assert.Equal(100, band.Step);
        Assert.Equal(0, band.Map("a"));
        Assert.Equal(350, band.Center("d"));
        Assert.True(double.IsNaN(band.Map("x")));
        band.SetRange(400, 0);
        Assert.Equal(300, band.Map("a"));
    }

    [Fact]
    public void Pairing_follows_carbon_rules()
    {
        Assert.Equal((1, 4), CdsChartPalette.Pairing(1, 4));
        Assert.Equal((3, 1), CdsChartPalette.Pairing(3, 9));
        Assert.Equal((4, 3), CdsChartPalette.Pairing(4, 3));
        Assert.Equal((14, 1), CdsChartPalette.Pairing(6, 2));
        Assert.Equal((1, 1), CdsChartPalette.Pairing(0, 1));
    }

    [Fact]
    public void Palette_uses_dark_pairing_and_flips_sequential_scales_on_dark_themes()
    {
        var light = new CdsChartPalette(CdsThemeName.White, 1);
        var dark = new CdsChartPalette(CdsThemeName.G100, 1);
        Assert.NotEqual(light.Series(2, 1, 0, false), dark.Series(2, 1, 0, false));
        Assert.NotEqual(light.Series(2, 1, 0, false), light.Series(2, 1, 0, true));
        Assert.Equal(light.Sequential(1, 0), dark.Sequential(1, 10));
        Assert.Equal(light.Diverging(1, 3), dark.Diverging(1, 3));
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.White, CdsColorKeys.DataVis05), light.Series(9, 1, 4, false));
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.G10, CdsColorKeys.Layer01), new CdsChartPalette(CdsThemeName.G10, 1).Token(CdsChartToken.GridBg));
    }

    [Fact]
    public void Layer_keys_resolve_at_the_layer_level()
    {
        Assert.Equal(CdsPalettes.GetTokenColor(CdsThemeName.White, "layer-02"), CdsPalettes.Resolve(CdsThemeName.White, 2, CdsColorKeys.Layer01));
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.White, CdsColorKeys.TextPrimary), CdsPalettes.Resolve(CdsThemeName.White, 3, CdsColorKeys.TextPrimary));
    }

    [Fact]
    public void Legend_toggle_isolates_adds_and_resets()
    {
        string[] all = ["A", "B", "C"];
        Assert.Equal(["B"], ChartLegendModel.Toggle([], all, "B"));
        Assert.Equal(["A", "B"], ChartLegendModel.Toggle(["B"], all, "A"));
        Assert.Equal(["A"], ChartLegendModel.Toggle(["A", "B"], all, "B"));
        Assert.Empty(ChartLegendModel.Toggle(["A"], all, "A"));
        Assert.Empty(ChartLegendModel.Toggle(["A", "B"], all, "C"));
        Assert.Equal(["C", "A", "B"], ChartLegendModel.Ordered(all, ["C"]));
    }

    [Fact]
    public void Truncation_and_formatting_follow_carbon()
    {
        Assert.Equal("Qty in stock now...", CdsChartTruncate.Apply("Qty in stock now and later", CdsChartTruncation.End, 16, 16));
        Assert.Equal("Short", CdsChartTruncate.Apply("Short"));
        Assert.Equal("...and later", CdsChartTruncate.Apply("Qty in stock now and later", CdsChartTruncation.Front, 16, 9));
        Assert.Equal("65,000", CdsChartFormat.Number(65000, En));
        Assert.Equal("0.125", CdsChartFormat.Number(0.125, En));
        Assert.Equal("$65,000", CdsChartFormat.Number(65000, En, "C0"));
        Assert.Equal("Jan 5, 2024", CdsChartFormat.Date(new DateTime(2024, 1, 5), En));
    }

    [Fact]
    public void Text_metrics_measure_condensed_narrower_than_sans()
    {
        var condensed = CdsChartTextMetrics.Width("Restocking", ChartFont.Tick);
        var sans = CdsChartTextMetrics.Width("Restocking", ChartFont.Sans12);
        Assert.InRange(condensed, 40, 70);
        Assert.True(condensed < sans);
        Assert.Equal(0, CdsChartTextMetrics.Width("", ChartFont.Tick));
    }

    [Fact]
    public void Csv_quotes_fields()
    {
        var table = new ChartTable(["Group", "Value"], [["A, B", "1"], ["Say \"hi\"", "2"]]);
        Assert.Equal("Group,Value\n\"A, B\",1\n\"Say \"\"hi\"\"\",2", table.ToCsv());
    }

    [Fact]
    public void Scene_hit_test_finds_marks_and_references_first()
    {
        var scene = new ChartScene { Width = 100, Height = 100, Plot = new RectF(0, 0, 100, 100) };
        scene.Marks.Add(new ChartMark { Id = 1, Bounds = new RectF(0, 0, 50, 50) });
        scene.Marks.Add(new ChartMark { Id = 2, Bounds = new RectF(40, 40, 50, 50) });
        scene.Marks.Add(new ChartMark { Id = 3, Bounds = new RectF(0, 20, 100, 10), IsReference = true });
        Assert.Equal(2, scene.HitTest(new PointF(45, 45))!.Id);
        Assert.Equal(3, scene.HitTest(new PointF(10, 25))!.Id);
        Assert.Null(scene.HitTest(new PointF(95, 5)));
    }

    [Fact]
    public void Arc_polygon_contains_its_middle()
    {
        var poly = ChartGeometry.ArcPolygon(new PointF(50, 50), 20, 40, 0, MathF.PI / 2);
        Assert.True(ChartGeometry.PolygonContains(poly, ChartGeometry.Polar(new PointF(50, 50), 30, MathF.PI / 4)));
        Assert.False(ChartGeometry.PolygonContains(poly, new PointF(50, 50)));
    }

    [Fact]
    public void Curves_produce_paths_for_every_kind()
    {
        PointF?[] points = [new(0, 10), new(10, 0), null, new(30, 20), new(40, 5), new(50, 15)];
        foreach (var curve in Enum.GetValues<CdsChartCurve>())
        {
            var path = ChartCurves.Line(points, curve);
            Assert.True(path.Count > 0, curve.ToString());
        }
        Assert.Equal(2, ChartCurves.Segments(points).Count);
    }

    [Fact]
    public void Skeletons_shimmer_their_shapes_and_never_the_backdrop()
    {
        var grid = ChartSkeletons.Build(ChartSkeleton.Grid, 300, 200, shimmer: true);
        Assert.True(grid.Shimmer);
        Assert.False(Assert.Single(grid.Shapes.OfType<ChartRect>()).Shimmers);
        Assert.Equal(20, grid.Shapes.OfType<ChartLine>().Count());
        Assert.All(grid.Shapes.OfType<ChartLine>(), l => Assert.True(l.Shimmers));
        foreach (var kind in new[] { ChartSkeleton.Pie, ChartSkeleton.Donut })
        {
            var shape = Assert.Single(ChartSkeletons.Build(kind, 300, 200, shimmer: true).Shapes);
            Assert.True(shape.Shimmers);
            Assert.Equal(ChartPaint.OfKey(CdsColorKeys.LayerAccent01), shape.Fill);
        }
        // The sparkline's: one middle line on a transparent backdrop.
        var line = Assert.IsType<ChartLine>(Assert.Single(ChartSkeletons.Build(ChartSkeleton.Line, 300, 80, shimmer: true).Shapes));
        Assert.True(line.Shimmers);
        Assert.Equal(40.5f, line.From.Y);
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.LayerAccent01), line.Stroke);
    }

    [Fact]
    public void Text_halo_draws_background_copies_around_the_text_first()
    {
        var texts = new List<ChartText>();
        var label = new ChartText { Text = "Paris", X = 10, Y = 20, Color = ChartPaint.OfKey(CdsColorKeys.TextPrimary) };
        ChartTextHalo.Add(texts, label, ChartPaint.OfKey(CdsColorKeys.Background));
        Assert.Equal(9, texts.Count);
        Assert.Same(label, texts[^1]);
        Assert.All(texts.Take(8), t => Assert.Equal(ChartPaint.OfKey(CdsColorKeys.Background), t.Color));
        Assert.All(texts.Take(8), t => Assert.Equal(1.5, Math.Sqrt((t.X - 10) * (t.X - 10) + (t.Y - 20) * (t.Y - 20)), 3));
    }

    [Fact]
    public void Light_chart_fonts_measure_narrower_than_regular()
    {
        var light = new ChartFont(ChartFontFamily.CondensedLight, 40);
        Assert.Equal(CdsFonts.SansCondensedLight, light.Alias);
        Assert.Equal(CdsFonts.SansLight, new ChartFont(ChartFontFamily.SansLight, 40).Alias);
        Assert.InRange(CdsChartTextMetrics.Width("timeout", light), 1, CdsChartTextMetrics.Width("timeout", new ChartFont(ChartFontFamily.Condensed, 40)) - 0.01);
        Assert.Contains(CdsFonts.Files, f => f.Alias == CdsFonts.SansCondensedLight);
    }
}
