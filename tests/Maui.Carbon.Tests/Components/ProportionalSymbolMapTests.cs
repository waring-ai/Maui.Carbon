namespace Maui.Carbon.Tests.Components;

public class ProportionalSymbolMapTests
{
    static List<CdsGeoPoint> Cities() =>
    [
        new("Asia", "Tokyo", 139.69, 35.69, 37.2), new("Asia", "Delhi", 77.21, 28.61, 32.9),
        new("Americas", "São Paulo", -46.63, -23.55, 22.6), new("Europe", "London", -0.13, 51.51, 9.6),
        new("Oceania", "Sydney", 151.21, -33.87, 5.1), new() { Group = "Europe", Name = "Nowhere", Value = 3 },
    ];

    static CdsProportionalSymbolMap Chart() => new() { GeoData = GeoProjectionTests.World(), Data = Cities(), RadiusLabel = "Population (M)", LocationLabel = "Urban area" };

    [Fact]
    public void Area_is_proportional_to_value()
    {
        Assert.Equal(25, CdsProportionalSymbolMap.Radius(100, 100, 3, 25), 6);
        Assert.Equal(12.5, CdsProportionalSymbolMap.Radius(25, 100, 3, 25), 6);
        Assert.Equal(3, CdsProportionalSymbolMap.Radius(0.01, 100, 3, 25), 6);
        Assert.Equal(0, CdsProportionalSymbolMap.Radius(null, 100, 3, 25));
        Assert.Equal(12.5, CdsProportionalSymbolMap.Radius(-25, 100, 3, 25), 6);
    }

    [Theory]
    [InlineData(37.2, new[] { 2.0, 9.0, 37.0 })]
    [InlineData(1100, new[] { 70.0, 300.0, 1100.0 })]
    [InlineData(1, new[] { 0.06, 0.3, 1.0 })]
    public void Size_key_rounds_the_largest_a_quarter_and_a_sixteenth(double max, double[] expected)
    {
        var values = CdsProportionalSymbolMap.SizeKeyValues(max);
        Assert.Equal(expected.Length, values.Count);
        for (var i = 0; i < expected.Length; i++) Assert.Equal(expected[i], values[i], 9);
    }

    [Fact]
    public void Circles_are_drawn_largest_first_with_default_radii()
    {
        var scene = Chart().RenderScene(800, 400);
        var circles = scene.Shapes.OfType<ChartCircle>().Where(c => c.Mark >= 0).ToList();
        Assert.Equal(5, circles.Count);
        Assert.Equal(circles.OrderByDescending(c => c.Radius).Select(c => c.Radius), circles.Select(c => c.Radius));
        // min(800, 400) x 25 / 400 for the largest value.
        Assert.Equal(25, circles[0].Radius, 0.01);
        Assert.Equal(0.2f, circles[0].FillOpacity);
        Assert.Equal(1f, circles[0].HoverFillOpacity);
        Assert.Equal(ChartPaint.OfSeries("Asia"), circles[0].Fill);
        Assert.Equal("5 symbols", scene.Summary!.Split(',')[0]);
        // The base map is muted land.
        Assert.Contains(scene.Shapes.OfType<ChartPath>(), s => s.Fill == ChartPaint.OfKey(CdsColorKeys.LayerAccent01) && s.StrokeWidth == 0.5f);
    }

    [Fact]
    public void Tooltip_shows_location_value_and_group()
    {
        var scene = Chart().RenderScene(800, 400);
        var tokyo = scene.Marks[0];
        Assert.Equal(["Urban area", "Population (M)", "Group"], tokyo.Rows.Select(r => r.Label));
        Assert.Equal(["Tokyo", "37.2", "Asia"], tokyo.Rows.Select(r => r.Value));
        Assert.Equal("Tokyo, 37.2, Asia", tokyo.Description);
        Assert.Equal(25, tokyo.Radius!.Value, 0.01);
        // Tokyo is east of London on the map.
        var london = scene.Marks.Single(m => m.Rows[0].Value == "London");
        Assert.True(tokyo.AnchorPoint.X > london.AnchorPoint.X);
        Assert.True(london.AnchorPoint.Y < tokyo.AnchorPoint.Y);
    }

    [Fact]
    public void Size_key_sits_under_the_map()
    {
        var chart = Chart();
        var scene = chart.RenderScene(800, 400);
        Assert.Contains(scene.Texts, t => t.Text == "Population (M)");
        Assert.Contains(scene.Texts, t => t.Text == "37");
        var keyCircles = scene.Shapes.OfType<ChartCircle>().Where(c => c.Mark < 0).ToList();
        Assert.Equal(3, keyCircles.Count);
        Assert.All(keyCircles, c => Assert.True(c.Fill.IsNone));
        Assert.True(keyCircles.All(c => c.Center.Y > scene.Plot.Bottom));
        // Circles stand on one baseline.
        Assert.Single(keyCircles.Select(c => MathF.Round(c.Center.Y + c.Radius, 2)).Distinct());
        chart.IsSizeLegendEnabled = false;
        Assert.Equal(400, chart.RenderScene(800, 400).Plot.Height);
    }

    [Fact]
    public void Legend_selection_filters_symbols()
    {
        var chart = Chart();
        chart.RenderScene(800, 400);
        chart.ToggleGroup("Europe");
        var scene = chart.RenderScene(800, 400);
        Assert.Single(scene.Marks);
        Assert.Equal("London", scene.Marks[0].Rows[0].Value);
        Assert.Equal(["Europe", "Nowhere", "", "", "3"], chart.RenderTable().Rows[1]);
    }

    [Fact]
    public void Fit_to_data_zooms_to_the_points()
    {
        var chart = Chart();
        chart.FitTo = CdsMapFit.Data;
        chart.IsSizeLegendEnabled = false;
        var scene = chart.RenderScene(800, 400);
        var xs = scene.Marks.Select(m => m.AnchorPoint.X).ToList();
        var ys = scene.Marks.Select(m => m.AnchorPoint.Y).ToList();
        // Padded by the largest radius plus 2.
        Assert.True(Math.Abs(ys.Min() - 27) < 1 || Math.Abs(xs.Min() - 27) < 1);
    }

    [Fact]
    public void Works_without_a_base_map_and_shows_the_skeleton_without_data()
    {
        var chart = new CdsProportionalSymbolMap { Data = Cities() };
        Assert.Equal(5, chart.RenderScene(600, 400).Marks.Count);
        Assert.Equal("No data", new CdsProportionalSymbolMap().RenderScene(600, 400).Summary);
        Assert.True(new CdsProportionalSymbolMap { IsLoading = true }.RenderScene(600, 400).Shimmer);
    }

    [Fact]
    public void Table_lists_group_location_coordinates_and_value()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Group", "Urban area", "Longitude", "Latitude", "Population (M)"], table.Headers);
        Assert.Equal(["Asia", "Tokyo", "139.69", "35.69", "37.2"], table.Rows[0]);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsProportionalSymbolMap
        {
            GeoData = GeoProjectionTests.World(),
            Data = Cities(),
            Projection = CdsGeoProjection.ConicEqualArea,
            FitBounds = new CdsGeoBounds(-11, 36, 30, 62),
            RadiusLabel = "Capacity (MW)",
            LocationLabel = "City",
            MinimumRadius = 2,
            MaximumRadius = 30,
            SymbolFillOpacity = 0.3,
            IsSizeLegendEnabled = true,
            SizeLegendTitle = "Capacity",
            IsLegendEnabled = false,
        };
        Assert.NotNull(chart.RenderScene(500, 300));
    }
}
