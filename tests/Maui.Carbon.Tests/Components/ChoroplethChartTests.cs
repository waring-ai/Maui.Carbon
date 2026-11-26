namespace Maui.Carbon.Tests.Components;

public class ChoroplethChartTests
{
    static List<CdsChartPoint> Africa() =>
    [
        new() { Key = "Kenya", Value = 40 }, new() { Key = "Ghana", Value = 69 }, new() { Key = "South Africa", Value = 72 },
        new() { Key = "818", Value = 100 }, // Egypt, by its ISO numeric id
        new() { Key = "Atlantis", Value = 50 },
    ];

    static CdsChoroplethChart Chart() => new() { GeoData = GeoProjectionTests.World(), Data = Africa(), ColorLegendTitle = "% of population" };

    [Fact]
    public void Rows_match_regions_by_id_or_name()
    {
        var features = GeoMap.Parse(GeoProjectionTests.World())!.Features(null);
        var joined = CdsChoroplethChart.Join(features, Africa());
        Assert.Equal(4, joined.Count);
        Assert.Contains(joined, kv => features[kv.Key].Name == "Egypt" && kv.Value.Value == 100);
        Assert.Contains(joined, kv => features[kv.Key].Name == "Kenya");
    }

    [Fact]
    public void Regions_with_data_are_marks_colored_on_the_scale()
    {
        var scene = Chart().RenderScene(600, 400);
        Assert.Equal(4, scene.Marks.Count);
        var kenya = scene.Marks.Single(m => m.Description == "Kenya: 40");
        Assert.Equal("Kenya", kenya.Rows[0].Label);
        Assert.Equal("40", kenya.Rows[0].Value);
        // Domain 0..100 (nice, from 0): 40 is step 4.
        Assert.Equal(ChartPaint.OfSequential(1, 4), kenya.Rows[0].Color);
        Assert.NotNull(kenya.Polygon);
        Assert.True(kenya.Contains(kenya.AnchorPoint));
        Assert.StartsWith("4 regions", scene.Summary);
        var region = scene.Shapes.OfType<ChartPath>().Single(s => s.Mark == kenya.Id && !s.HoverOnly);
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.BorderSubtleSelected01), region.Stroke);
        var outline = scene.Shapes.OfType<ChartPath>().Single(s => s.Mark == kenya.Id && s.HoverOnly);
        Assert.Equal(1.5f, outline.StrokeWidth);
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute), outline.Stroke);
    }

    [Fact]
    public void The_view_fits_the_regions_with_data()
    {
        var scene = Chart().RenderScene(600, 400);
        var bounds = scene.Marks.Select(m => m.Bounds).ToList();
        var x0 = bounds.Min(b => b.Left);
        var x1 = bounds.Max(b => b.Right);
        var y0 = bounds.Min(b => b.Top);
        var y1 = bounds.Max(b => b.Bottom);
        // The data fills the map area in at least one direction, centered in the other.
        Assert.True(Math.Abs(y0) < 1 || Math.Abs(x0) < 1);
        Assert.True(Math.Abs(y1 - scene.Plot.Bottom) < 1 || Math.Abs(x1 - 600) < 1);
        var whole = Chart();
        whole.FitTo = CdsMapFit.Map;
        var world = whole.RenderScene(600, 400);
        Assert.True(world.Marks.Single(m => m.Description == "Kenya: 40").Bounds.Width < scene.Marks.Single(m => m.Description == "Kenya: 40").Bounds.Width);
        var boxed = Chart();
        boxed.FitBounds = new CdsGeoBounds(-20, -40, 55, 40);
        Assert.NotEqual(world.Marks[0].Bounds, boxed.RenderScene(600, 400).Marks[0].Bounds);
    }

    [Fact]
    public void Regions_without_data_merge_into_one_hatched_shape()
    {
        var scene = Chart().RenderScene(600, 400);
        var background = scene.Shapes.OfType<ChartPath>().Single(s => s.Fill == ChartPaint.OfKey(CdsColorKeys.Background));
        Assert.True(background.Mark < 0);
        var hatch = scene.Shapes.OfType<ChartPath>().Single(s => s.Stroke == ChartPaint.OfKey(CdsColorKeys.BorderStrong01));
        Assert.Equal(0.5f, hatch.StrokeWidth);
        Assert.True(hatch.Path.Count > 100);
        Assert.Contains(scene.Shapes.OfType<ChartPath>(), s => s.Mark < 0 && s.Fill.IsNone && s.Stroke == ChartPaint.OfKey(CdsColorKeys.BorderSubtleSelected01));
    }

    [Fact]
    public void Color_legend_sits_under_the_map()
    {
        var chart = Chart();
        var scene = chart.RenderScene(600, 400);
        Assert.Contains(scene.Texts, t => t.Text == "% of population");
        Assert.Contains(scene.Texts, t => t.Text == "100");
        Assert.True(scene.Plot.Bottom <= 400 - 43 - 15 + 0.01);
        chart.IsLegendEnabled = false;
        Assert.Equal(400, chart.RenderScene(600, 400).Plot.Height);
    }

    [Fact]
    public void Diverging_values_use_the_diverging_palette()
    {
        var chart = new CdsChoroplethChart
        {
            GeoData = GeoProjectionTests.World(),
            Data = new List<CdsChartPoint> { new() { Key = "Germany", Value = 4.5 }, new() { Key = "Bulgaria", Value = -11 } },
            Projection = CdsGeoProjection.ConicEqualArea,
            ColorPairingOption = 2,
        };
        var scene = chart.RenderScene(600, 400);
        Assert.All(scene.Marks, m => Assert.Equal(ChartPaintKind.Diverging, m.Rows[0].Color.Kind));
        Assert.All(scene.Marks, m => Assert.Equal(2, m.Rows[0].Color.A));
    }

    [Fact]
    public void Table_lists_country_id_name_and_value()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Country ID", "Country Name", "Value"], table.Headers);
        Assert.Contains(table.Rows, r => r.SequenceEqual(["404", "Kenya", "40"]));
        Assert.Contains(table.Rows, r => r.SequenceEqual(["818", "Egypt", "100"]));
        Assert.Contains(table.Rows, r => r.SequenceEqual(["–", "Atlantis", "50"]));
    }

    [Fact]
    public void Without_geometry_or_data_it_shows_the_skeleton()
    {
        var noMap = new CdsChoroplethChart { Data = Africa() };
        Assert.Equal("No data", noMap.RenderScene(400, 300).Summary);
        var loading = new CdsChoroplethChart { IsLoading = true };
        Assert.True(loading.RenderScene(400, 300).Shimmer);
        var bad = new CdsChoroplethChart { Data = Africa(), GeoData = "{ nope" };
        Assert.Empty(bad.RenderScene(400, 300).Marks);
    }

    [Fact]
    public void Every_projection_draws()
    {
        foreach (var p in Enum.GetValues<CdsGeoProjection>())
        {
            var chart = Chart();
            chart.Projection = p;
            chart.FitTo = CdsMapFit.Map;
            var scene = chart.RenderScene(600, 400);
            Assert.Equal(4, scene.Marks.Count);
            Assert.All(scene.Marks, m => Assert.True(float.IsFinite(m.Bounds.Width) && m.Bounds.Width > 0));
        }
    }

    [Fact]
    public void GeoJson_maps_stroke_each_missing_region()
    {
        const string geo = """
        { "type": "FeatureCollection", "features": [
          { "type": "Feature", "id": "A", "properties": { "name": "Alpha" }, "geometry": { "type": "Polygon", "coordinates": [[[0, 0], [10, 0], [10, 10], [0, 10], [0, 0]]] } },
          { "type": "Feature", "id": "B", "properties": { "name": "Beta" }, "geometry": { "type": "Polygon", "coordinates": [[[10, 0], [20, 0], [20, 10], [10, 10], [10, 0]]] } }
        ] }
        """;
        var chart = new CdsChoroplethChart { GeoData = geo, Data = new List<CdsChartPoint> { new() { Key = "A", Value = 3 } }, FitTo = CdsMapFit.Map, IsLegendEnabled = false, Projection = CdsGeoProjection.Equirectangular };
        var scene = chart.RenderScene(400, 200);
        var alpha = Assert.Single(scene.Marks);
        Assert.Equal("Alpha: 3", alpha.Description);
        Assert.Equal(0, alpha.Bounds.Left, 0.5);
        Assert.Equal(200, alpha.Bounds.Width, 0.5);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsChoroplethChart
        {
            Title = "Internet users",
            GeoData = GeoProjectionTests.World(),
            ObjectsKey = "countries",
            Data = Africa(),
            Projection = CdsGeoProjection.EqualEarth,
            FitTo = CdsMapFit.Map,
            FitBounds = new CdsGeoBounds { West = -20, South = -40, East = 55, North = 40 },
            ColorLegendTitle = "%",
            ColorLegendType = CdsChartColorLegendType.Quantize,
            ColorDomainMinimum = 0,
            ColorDomainMaximum = 100,
            GradientColors = [Colors.White, Colors.Black],
            ColorPairingOption = 3,
            IsLegendEnabled = true,
        };
        Assert.NotEmpty(chart.RenderScene(500, 300).Marks);
    }
}
