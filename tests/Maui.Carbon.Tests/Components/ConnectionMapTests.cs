namespace Maui.Carbon.Tests.Components;

public class ConnectionMapTests
{
    static List<CdsGeoConnection> Routes() =>
    [
        new("Americas", "London", "New York", -0.45, 51.47, -73.78, 40.64, 98),
        new("Asia", "London", "Singapore", -0.45, 51.47, 103.99, 1.36, 28),
        new("Asia", "London", "Tokyo", -0.45, 51.47, 139.78, 35.55, 21),
        new("Americas", "London", "Nowhere", 7),
    ];

    static CdsConnectionMap Chart() => new() { GeoData = GeoProjectionTests.World(), Data = Routes(), ValueLabel = "Departures per week" };

    [Fact]
    public void Ends_come_from_coordinates_or_named_nodes()
    {
        var resolved = CdsConnectionMap.Resolve(Routes(), [new CdsGeoPoint("Nowhere", 10, 10), new CdsGeoPoint("London", -0.13, 51.51)]);
        Assert.Equal(4, resolved.Count);
        Assert.Equal(new GeoPos(10, 10), resolved[3].To);
        Assert.Equal(3, CdsConnectionMap.Resolve(Routes(), []).Count);
    }

    [Fact]
    public void Line_width_grows_with_value()
    {
        Assert.Equal(6, CdsConnectionMap.LineWidth(98, 98, 1, 6), 9);
        Assert.Equal(1, CdsConnectionMap.LineWidth(null, 98, 1, 6), 9);
        Assert.Equal(1 + 5 * 49 / 98.0, CdsConnectionMap.LineWidth(49, 98, 1, 6), 9);
    }

    [Fact]
    public void Lines_and_locations_are_marks()
    {
        var scene = Chart().RenderScene(800, 400);
        // 3 located lines, then London and 3 destinations.
        Assert.Equal(7, scene.Marks.Count);
        Assert.Equal("3 connections, 4 locations", scene.Summary);
        var ny = scene.Marks[0];
        Assert.Equal("London → New York", ny.Rows[0].Label);
        Assert.Equal("98", ny.Rows[0].Value);
        Assert.Equal(ChartPaint.OfSeries("Americas"), ny.Rows[0].Color);
        Assert.Equal(("Group", "Americas"), (ny.Rows[1].Label, ny.Rows[1].Value));
        Assert.Equal("London to New York: 98, Americas", ny.Description);
        var line = scene.Shapes.OfType<ChartPath>().Single(s => s.Mark == ny.Id && !s.HoverOnly);
        Assert.Equal(6, line.StrokeWidth, 0.01);
        Assert.Equal(0.8f, line.Opacity);
        Assert.Equal(LineCap.Round, line.Cap);
        Assert.True(ny.Contains(ny.AnchorPoint));
        var london = scene.Marks.Single(m => m.Rows.Count > 0 && m.Rows[0].Label == "London");
        Assert.Equal(["London", "Connections", "Total"], london.Rows.Select(r => r.Label));
        Assert.Equal("3", london.Rows[1].Value);
        Assert.Equal("147", london.Rows[2].Value);
        // Hovering London raises its three lines.
        Assert.Equal(3, scene.Shapes.OfType<ChartPath>().Count(s => s.Mark == london.Id && s.HoverOnly));
        var dot = scene.Shapes.OfType<ChartCircle>().Single(c => c.Mark == london.Id);
        Assert.Equal(3, dot.Radius);
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.Layer01), dot.Fill);
        Assert.Equal(1.5f, dot.StrokeWidth);
    }

    [Fact]
    public void Great_circles_curve_and_straight_lines_do_not()
    {
        var gc = Chart().RenderScene(800, 400);
        var straight = Chart();
        straight.LineType = CdsConnectionLineType.Straight;
        var st = straight.RenderScene(800, 400);
        var gcLine = gc.Shapes.OfType<ChartPath>().Single(s => s.Mark == 0 && !s.HoverOnly);
        var stLine = st.Shapes.OfType<ChartPath>().Single(s => s.Mark == 0 && !s.HoverOnly);
        Assert.Equal(2, stLine.Path.Count);
        Assert.True(gcLine.Path.Count > 20);
        // London to New York bows north: above the straight chord.
        Assert.True(gc.Marks[0].Bounds.Top < st.Marks[0].Bounds.Top - 1);
        var arc = Chart();
        arc.LineType = CdsConnectionLineType.Arc;
        Assert.Equal(25, arc.RenderScene(800, 400).Shapes.OfType<ChartPath>().Single(s => s.Mark == 0 && !s.HoverOnly).Path.Count);
    }

    [Fact]
    public void A_route_across_the_pacific_breaks_at_the_antimeridian()
    {
        var chart = new CdsConnectionMap { Data = new List<CdsGeoConnection> { new("Pacific", "Tokyo", "Los Angeles", 139.78, 35.55, -118.41, 33.94, 10) } };
        var scene = chart.RenderScene(800, 400);
        var line = scene.Shapes.OfType<ChartPath>().Single(s => s.Mark == 0 && !s.HoverOnly);
        var moves = 0;
        for (var i = 0; i < line.Path.OperationCount; i++) if (line.Path.GetSegmentType(i) == PathOperation.Move) moves++;
        Assert.Equal(2, moves);
    }

    [Fact]
    public void Nodes_labels_and_legend_selection()
    {
        var chart = new CdsConnectionMap
        {
            GeoData = GeoProjectionTests.World(),
            Data = new List<CdsGeoConnection> { new("Primary", "Frankfurt", "Paris", 400), new("Backup", "London", "Paris", 100) },
            ArePointLabelsEnabled = true,
            FitBounds = new CdsGeoBounds(-11, 38, 26, 61),
            Projection = CdsGeoProjection.ConicEqualArea,
        };
        chart.Nodes.Add(new CdsGeoPoint("Frankfurt", 8.68, 50.11));
        chart.Nodes.Add(new CdsGeoPoint("Paris", 2.35, 48.86));
        chart.Nodes.Add(new CdsGeoPoint("London", -0.13, 51.51));
        var scene = chart.RenderScene(600, 400);
        Assert.Equal("2 connections, 3 locations", scene.Summary);
        Assert.Contains(scene.Texts, t => t.Text == "Frankfurt" && t.Color == ChartPaint.OfKey(CdsColorKeys.TextPrimary));
        // Carbon's background halo follows the glyphs: background copies of the name, no plate over the routes.
        Assert.Equal(8, scene.Texts.Count(t => t.Text == "Frankfurt" && t.Color == ChartPaint.OfKey(CdsColorKeys.Background)));
        Assert.DoesNotContain(scene.Shapes.OfType<ChartRect>(), r => r.Fill == ChartPaint.OfKey(CdsColorKeys.Background));
        chart.ToggleGroup("Backup");
        var filtered = chart.RenderScene(600, 400);
        Assert.Equal("1 connection, 2 locations", filtered.Summary);
        Assert.Single(chart.RenderTable().Rows);
    }

    [Fact]
    public void Points_can_be_turned_off()
    {
        var chart = Chart();
        chart.ArePointsEnabled = false;
        var scene = chart.RenderScene(800, 400);
        Assert.Equal(3, scene.Marks.Count);
        Assert.Empty(scene.Shapes.OfType<ChartCircle>());
    }

    [Fact]
    public void Table_lists_group_source_target_and_value()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Group", "Source", "Target", "Departures per week"], table.Headers);
        Assert.Equal(["Americas", "London", "New York", "98"], table.Rows[0]);
        Assert.Equal(4, table.Rows.Count);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        Assert.True(new CdsConnectionMap { IsLoading = true }.RenderScene(400, 300).Shimmer);
        Assert.Equal("No data", new CdsConnectionMap { GeoData = GeoProjectionTests.World() }.RenderScene(400, 300).Summary);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsConnectionMap
        {
            Data = Routes(),
            GeoData = GeoProjectionTests.World(),
            LineType = CdsConnectionLineType.Arc,
            MinimumLineWidth = 1,
            MaximumLineWidth = 5,
            ArePointsEnabled = true,
            PointRadius = 4,
            ArePointLabelsEnabled = true,
            ValueLabel = "Gbps",
            FitTo = CdsMapFit.Data,
            Projection = CdsGeoProjection.Mercator,
        };
        Assert.NotEmpty(chart.RenderScene(500, 300).Marks);
    }
}
