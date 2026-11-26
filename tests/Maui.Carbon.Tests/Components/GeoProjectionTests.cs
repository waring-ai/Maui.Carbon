namespace Maui.Carbon.Tests.Components;

public class GeoProjectionTests
{
    // A tiny quantized topology: two squares sharing an edge (arc 0), as topojson writes them.
    internal const string TwoSquares = """
    {
      "type": "Topology",
      "transform": { "scale": [1, 1], "translate": [0, 0] },
      "objects": {
        "regions": {
          "type": "GeometryCollection",
          "geometries": [
            { "type": "Polygon", "id": "1", "properties": { "name": "West" }, "arcs": [[0, 1]] },
            { "type": "Polygon", "id": 2, "properties": { "NAME": "East" }, "arcs": [[-1, 2]] }
          ]
        }
      },
      "arcs": [
        [[10, 0], [0, 10]],
        [[10, 10], [-10, 0], [0, -10], [10, 0]],
        [[10, 0], [10, 0], [0, 10], [-10, 0]]
      ]
    }
    """;

    internal static string WorldPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "samples", "Maui.Carbon.Gallery", "Resources", "Raw", "Maps", "countries-110m.json");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("countries-110m.json");
    }

    static string? _world;

    internal static string World() => _world ??= File.ReadAllText(WorldPath());

    [Fact]
    public void Topology_arcs_are_delta_decoded_and_stitched_into_rings()
    {
        var map = GeoMap.Parse(TwoSquares)!;
        Assert.True(map.IsTopology);
        Assert.Equal(3, map.Arcs.Count);
        Assert.Equal([new GeoPos(10, 0), new GeoPos(10, 10)], map.Arcs[0]);
        Assert.Equal(new GeoPos(0, 0), map.Arcs[1][2]);
        var features = map.Features(null);
        Assert.Equal(2, features.Count);
        Assert.Equal(("1", "West"), (features[0].Id, features[0].Name));
        Assert.Equal(("2", "East"), (features[1].Id, features[1].Name));
        var west = features[0].Polygons.Single().Single();
        // Arc 0 then arc 1, the shared point once: a closed square.
        Assert.Equal([new(10, 0), new(10, 10), new(0, 10), new(0, 0), new(10, 0)], west.Points);
        Assert.Equal([0, 1], west.Arcs!);
        var east = features[1].Polygons.Single().Single();
        // ~0 is arc 0 reversed.
        Assert.Equal(new GeoPos(10, 10), east.Points[0]);
        Assert.Equal(new GeoPos(10, 0), east.Points[1]);
        Assert.Equal(east.Points[0], east.Points[^1]);
    }

    [Fact]
    public void Objects_key_defaults_to_countries_then_the_first_object()
    {
        var map = GeoMap.Parse(TwoSquares)!;
        Assert.Equal("regions", map.ObjectsKeyOf(null));
        Assert.Equal("regions", map.ObjectsKeyOf("missing"));
        var world = GeoMap.Parse(World())!;
        Assert.Equal("countries", world.ObjectsKeyOf(null));
        Assert.Equal("land", world.ObjectsKeyOf("land"));
    }

    [Fact]
    public void GeoJson_feature_collections_features_and_geometries_parse()
    {
        const string fc = """
        { "type": "FeatureCollection", "features": [
          { "type": "Feature", "id": "KE", "properties": { "NAME": "Kenya" }, "geometry": { "type": "Polygon", "coordinates": [[[34, -4], [41, -4], [41, 5], [34, 5], [34, -4]]] } },
          { "type": "Feature", "properties": { "name": "Isles" }, "geometry": { "type": "MultiPolygon", "coordinates": [[[[0, 0], [1, 0], [1, 1], [0, 0]]], [[[2, 2], [3, 2], [3, 3], [2, 2]]]] } },
          { "type": "Feature", "properties": { "name": "Point" }, "geometry": { "type": "Point", "coordinates": [1, 1] } }
        ] }
        """;
        var map = GeoMap.Parse(fc)!;
        Assert.False(map.IsTopology);
        var f = map.Features(null);
        Assert.Equal(3, f.Count);
        Assert.Equal(("KE", "Kenya"), (f[0].Id, f[0].Name));
        Assert.Equal(2, f[1].Polygons.Count);
        Assert.Empty(f[2].Polygons);
        Assert.Single(GeoMap.Parse("""{ "type": "Polygon", "coordinates": [[[0,0],[1,0],[1,1],[0,0]]] }""")!.Features(null));
    }

    [Fact]
    public void Invalid_or_empty_text_has_no_map_and_parsing_is_cached()
    {
        Assert.Null(GeoMap.Parse(null));
        Assert.Null(GeoMap.Parse("  "));
        Assert.Null(GeoMap.Parse("{ not json"));
        var text = World();
        Assert.Same(GeoMap.Parse(text), GeoMap.Parse(text));
    }

    [Fact]
    public void The_bundled_world_map_reads_every_country()
    {
        var features = GeoMap.Parse(World())!.Features(null);
        Assert.Equal(177, features.Count);
        Assert.Contains(features, f => f.Name == "Kenya" && f.Id == "404");
        var russia = features.Single(f => f.Name == "Russia");
        Assert.True(russia.Polygons.Count > 5);
        Assert.All(features.SelectMany(f => f.Rings), r => Assert.Equal(r.Points[0], r.Points[^1]));
    }

    [Fact]
    public void Raw_projections_match_d3()
    {
        Assert.Equal((0d, 0d), GeoProjection.NaturalEarth1(0, 0));
        var ne = GeoProjection.NaturalEarth1(Math.PI, 0);
        Assert.Equal(Math.PI * 0.8707, ne.X, 6);
        var neY = GeoProjection.NaturalEarth1(0, Math.PI / 2).Y;
        Assert.Equal(1.4224, neY, 3);
        var ee = GeoProjection.EqualEarth(Math.PI, 0);
        Assert.Equal(Math.PI / (Math.Sqrt(3) / 2 * 1.340264), ee.X, 6);
        Assert.Equal(Math.Log(Math.Tan(Math.PI / 4 + Math.PI / 8)), GeoProjection.Mercator(0, Math.PI / 4).Y, 9);
        Assert.True(double.IsFinite(GeoProjection.Mercator(0, Math.PI / 2).Y));
        // Conic projections: symmetric about the central meridian, latitude increases y.
        var conic = GeoProjection.ConicEqualArea(0, 60);
        var a = conic(0.3, 0.8);
        var b = conic(-0.3, 0.8);
        Assert.Equal(a.X, -b.X, 9);
        Assert.Equal(a.Y, b.Y, 9);
        Assert.True(conic(0, 1).Y > conic(0, 0.5).Y);
        var eq = GeoProjection.ConicEquidistant(0, 60);
        // Equidistant along meridians: equal latitude steps are equal distances on the central meridian.
        Assert.Equal(eq(0, 0.6).Y - eq(0, 0.3).Y, eq(0, 0.3).Y - eq(0, 0).Y, 9);
    }

    [Fact]
    public void Conic_equal_area_preserves_area()
    {
        var f = GeoProjection.ConicEqualArea(20, 50);
        double Cell(double lon, double lat)
        {
            const double d = 0.01;
            var p0 = f(lon, lat);
            var p1 = f(lon + d, lat);
            var p2 = f(lon, lat + d);
            return Math.Abs((p1.X - p0.X) * (p2.Y - p0.Y) - (p1.Y - p0.Y) * (p2.X - p0.X)) / Math.Cos(lat + d / 2);
        }
        Assert.Equal(1, Cell(0, 0.2) / Cell(0.5, 1.0), 2);
    }

    [Fact]
    public void Fit_centers_the_bounds_in_the_extent()
    {
        var p = new GeoProjection(CdsGeoProjection.Equirectangular);
        p.Fit(new RectF(0, 0, 400, 200), [new GeoPos(-10, -10), new GeoPos(10, 10)]);
        var sw = p.Project(-10, -10);
        var ne = p.Project(10, 10);
        // A square box in a 2:1 extent: limited by the height, centered horizontally.
        Assert.Equal(200, sw.Y, 0.01);
        Assert.Equal(0, ne.Y, 0.01);
        Assert.Equal(100, sw.X, 0.01);
        Assert.Equal(300, ne.X, 0.01);
        var single = new GeoProjection(CdsGeoProjection.Mercator);
        single.Fit(new RectF(0, 0, 100, 100), [new GeoPos(5, 5)]);
        var c = single.Project(5, 5);
        Assert.Equal(50, c.X, 0.5);
        Assert.Equal(50, c.Y, 0.5);
    }

    [Fact]
    public void Albers_rotates_the_united_states_to_the_center()
    {
        var p = new GeoProjection(CdsGeoProjection.Albers);
        Assert.Equal(96, p.Rotation);
        Assert.Equal(new GeoPos(0, 40), p.Rotate(new GeoPos(-96, 40)));
        Assert.Equal(-164, p.Rotate(new GeoPos(100, 0)).Lon, 9);
    }

    [Fact]
    public void Rings_across_the_antimeridian_split_into_pieces_on_each_side()
    {
        var p = new GeoProjection(CdsGeoProjection.Equirectangular);
        GeoPos[] ring = [new(170, 60), new(-170, 60), new(-170, 70), new(170, 70), new(170, 60)];
        var pieces = p.CutRing(ring);
        Assert.Equal(2, pieces.Count);
        Assert.All(pieces, piece => Assert.All(piece, q => Assert.InRange(q.Lon, -180.0001, 180.0001)));
        Assert.Contains(pieces, piece => piece.All(q => q.Lon >= 170 - 1e-9));
        Assert.Contains(pieces, piece => piece.All(q => q.Lon <= -170 + 1e-9));
        // A ring that does not cross stays whole.
        Assert.Single(p.CutRing([new(0, 0), new(10, 0), new(10, 10), new(0, 0)]));
    }

    [Fact]
    public void A_ring_around_a_pole_closes_along_it()
    {
        var p = new GeoProjection(CdsGeoProjection.NaturalEarth1);
        var ring = Enumerable.Range(0, 36).Select(i => new GeoPos(-180 + i * 10, -70)).Append(new GeoPos(-180, -70)).ToList();
        var pieces = p.CutRing(ring);
        Assert.NotEmpty(pieces);
        var all = pieces.SelectMany(x => x).ToList();
        Assert.Contains(all, q => q.Lat == -90);
        Assert.Equal(360, all.Max(q => q.Lon) - all.Min(q => q.Lon), 1);
        // Antarctica on the bundled map does the same.
        var antarctica = GeoMap.Parse(World())!.Features(null).Single(f => f.Name == "Antarctica");
        var cut = antarctica.Rings.SelectMany(r => p.CutRing(r.Points)).SelectMany(x => x).ToList();
        Assert.Contains(cut, q => q.Lat == -90);
    }

    [Fact]
    public void Lines_split_at_the_antimeridian_and_long_edges_are_resampled()
    {
        var p = new GeoProjection(CdsGeoProjection.Equirectangular);
        var parts = p.CutLine([new GeoPos(170, 10), new GeoPos(-170, 20)]);
        Assert.Equal(2, parts.Count);
        Assert.Equal(180, parts[0][^1].Lon, 9);
        Assert.Equal(-180, parts[1][0].Lon, 9);
        Assert.Equal(15, parts[0][^1].Lat, 9);
        var long_ = p.CutLine([new GeoPos(0, 0), new GeoPos(20, 0)]);
        Assert.Single(long_);
        Assert.Equal(11, long_[0].Count);
    }

    [Fact]
    public void Great_circles_bow_toward_the_pole()
    {
        var london = new GeoPos(-0.45, 51.47);
        var newYork = new GeoPos(-73.78, 40.64);
        var path = GeoProjection.GreatCircle(london, newYork);
        Assert.Equal(london, path[0]);
        Assert.Equal(newYork, path[^1]);
        Assert.True(path.Count > 40);
        Assert.True(path.Max(q => q.Lat) > 52);
        // Along the equator it stays on the equator.
        Assert.All(GeoProjection.GreatCircle(new GeoPos(0, 0), new GeoPos(90, 0)), q => Assert.Equal(0, q.Lat, 6));
    }

    [Fact]
    public void Hit_polygons_combine_rings_with_even_odd()
    {
        PointF[] a = [new(0, 0), new(10, 0), new(10, 10), new(0, 10)];
        PointF[] b = [new(20, 0), new(30, 0), new(30, 10), new(20, 10)];
        PointF[] hole = [new(2, 2), new(8, 2), new(8, 8), new(2, 8)];
        var poly = GeoScene.HitPolygon([a, b, hole]);
        Assert.True(ChartGeometry.PolygonContains(poly, new PointF(1, 1)));
        Assert.True(ChartGeometry.PolygonContains(poly, new PointF(25, 5)));
        Assert.False(ChartGeometry.PolygonContains(poly, new PointF(5, 5)));
        Assert.False(ChartGeometry.PolygonContains(poly, new PointF(15, 5)));
    }

    [Fact]
    public void Stroke_hit_areas_follow_the_line()
    {
        var hit = GeoScene.StrokeHit([new(0, 0), new(100, 0), new(100, 100)], 8);
        Assert.True(ChartGeometry.PolygonContains(hit, new PointF(50, 3)));
        Assert.True(ChartGeometry.PolygonContains(hit, new PointF(97, 50)));
        Assert.False(ChartGeometry.PolygonContains(hit, new PointF(50, 6)));
        Assert.False(ChartGeometry.PolygonContains(hit, new PointF(50, 50)));
    }
}
