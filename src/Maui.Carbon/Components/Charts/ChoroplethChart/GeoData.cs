using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Maui.Carbon;

/// <summary>A longitude and latitude in degrees.</summary>
internal readonly record struct GeoPos(double Lon, double Lat);

/// <summary>A closed ring of a polygon, and for TopoJSON the arcs it was stitched from (negative: reversed, <c>~i</c>).</summary>
internal sealed record GeoRing(GeoPos[] Points, int[]? Arcs = null);

/// <summary>A region of a map: its id, display name and polygons (each an outer ring and its holes).</summary>
internal sealed class GeoFeature
{
    public string? Id { get; init; }

    /// <summary><c>properties.NAME</c>, else <c>properties.name</c>, else the id (Carbon's demos and world-atlas).</summary>
    public string Name { get; init; } = "";

    public List<List<GeoRing>> Polygons { get; } = [];

    public IEnumerable<GeoRing> Rings => Polygons.SelectMany(p => p);
}

/// <summary>
/// Map geometry parsed from TopoJSON (topojson-client's <c>feature</c>: quantized, delta-encoded arcs stitched into rings) or
/// GeoJSON (a FeatureCollection, a Feature, a geometry or an array of features), with <see cref="JsonDocument"/> only
/// (no reflection, trimmable). Polygons are kept; points and lines in the geometry are ignored. Parsing is cached per
/// string instance, so charts sharing one map text parse it once.
/// </summary>
internal sealed class GeoMap
{
    static readonly ConditionalWeakTable<string, GeoMap> Cache = new();

    readonly Dictionary<string, IReadOnlyList<GeoFeature>> _features = new(StringComparer.Ordinal);
    readonly Dictionary<string, JsonElement> _objects = new(StringComparer.Ordinal);
    readonly List<string> _objectKeys = [];
    readonly IReadOnlyList<GeoFeature> _geoJson = [];

    GeoMap(JsonDocument document)
    {
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Object && Str(root, "type") == "Topology" && root.TryGetProperty("objects", out var objects) && objects.ValueKind == JsonValueKind.Object)
        {
            IsTopology = true;
            foreach (var o in objects.EnumerateObject())
            {
                _objects[o.Name] = o.Value;
                _objectKeys.Add(o.Name);
            }
            Arcs = DecodeArcs(root);
        }
        else
        {
            _geoJson = ParseGeoJson(root);
        }
    }

    public bool IsTopology { get; }

    /// <summary>The decoded arcs of a topology, in degrees.</summary>
    public IReadOnlyList<GeoPos[]> Arcs { get; } = [];

    /// <summary>The map of <paramref name="json"/>, or null when it is empty or not valid JSON.</summary>
    public static GeoMap? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        if (Cache.TryGetValue(json, out var cached)) return cached;
        try
        {
            var map = new GeoMap(JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true }));
            Cache.AddOrUpdate(json, map);
            return map;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
        {
            System.Diagnostics.Trace.TraceError($"Map geometry could not be read: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// The TopoJSON object used: <paramref name="objectsKey"/> when the topology has it, else <c>countries</c>, else the first
    /// object (Carbon's <c>thematic.objectsKey</c>).
    /// </summary>
    public string? ObjectsKeyOf(string? objectsKey)
    {
        if (!IsTopology || _objectKeys.Count == 0) return null;
        if (objectsKey is not null && _objects.ContainsKey(objectsKey)) return objectsKey;
        return _objects.ContainsKey("countries") ? "countries" : _objectKeys[0];
    }

    /// <summary>The regions of the map (for TopoJSON, of the object <see cref="ObjectsKeyOf"/> picks).</summary>
    public IReadOnlyList<GeoFeature> Features(string? objectsKey)
    {
        if (!IsTopology) return _geoJson;
        var key = ObjectsKeyOf(objectsKey);
        if (key is null) return [];
        lock (_features)
        {
            if (_features.TryGetValue(key, out var list)) return list;
            var o = _objects[key];
            var result = new List<GeoFeature>();
            if (Str(o, "type") == "GeometryCollection" && o.TryGetProperty("geometries", out var geoms) && geoms.ValueKind == JsonValueKind.Array)
                foreach (var g in geoms.EnumerateArray()) result.Add(TopoFeature(g));
            else result.Add(TopoFeature(o));
            _features[key] = result;
            return result;
        }
    }

    // ----- TopoJSON -----

    static IReadOnlyList<GeoPos[]> DecodeArcs(JsonElement root)
    {
        double sx = 1, sy = 1, tx = 0, ty = 0;
        var quantized = false;
        if (root.TryGetProperty("transform", out var t) && t.ValueKind == JsonValueKind.Object)
        {
            quantized = true;
            if (t.TryGetProperty("scale", out var s) && s.GetArrayLength() >= 2)
            {
                sx = s[0].GetDouble();
                sy = s[1].GetDouble();
            }
            if (t.TryGetProperty("translate", out var tr) && tr.GetArrayLength() >= 2)
            {
                tx = tr[0].GetDouble();
                ty = tr[1].GetDouble();
            }
        }
        var arcs = new List<GeoPos[]>();
        if (!root.TryGetProperty("arcs", out var list) || list.ValueKind != JsonValueKind.Array) return arcs;
        foreach (var arc in list.EnumerateArray())
        {
            var points = new GeoPos[arc.GetArrayLength()];
            double x = 0, y = 0;
            var i = 0;
            foreach (var p in arc.EnumerateArray())
            {
                double px = p[0].GetDouble(), py = p[1].GetDouble();
                if (quantized)
                {
                    x += px;
                    y += py;
                    points[i++] = new GeoPos(x * sx + tx, y * sy + ty);
                }
                else points[i++] = new GeoPos(px, py);
            }
            arcs.Add(points);
        }
        return arcs;
    }

    GeoFeature TopoFeature(JsonElement g)
    {
        var f = new GeoFeature { Id = IdOf(g), Name = NameOf(g) };
        AddTopoGeometry(f, g);
        return f;
    }

    void AddTopoGeometry(GeoFeature f, JsonElement g)
    {
        switch (Str(g, "type"))
        {
            case "Polygon":
                if (g.TryGetProperty("arcs", out var pa)) f.Polygons.Add(TopoPolygon(pa));
                break;
            case "MultiPolygon":
                if (g.TryGetProperty("arcs", out var ma))
                    foreach (var poly in ma.EnumerateArray()) f.Polygons.Add(TopoPolygon(poly));
                break;
            case "GeometryCollection":
                if (g.TryGetProperty("geometries", out var gs))
                    foreach (var child in gs.EnumerateArray()) AddTopoGeometry(f, child);
                break;
        }
    }

    List<GeoRing> TopoPolygon(JsonElement rings)
    {
        var polygon = new List<GeoRing>();
        foreach (var ring in rings.EnumerateArray())
        {
            var indices = ring.EnumerateArray().Select(a => a.GetInt32()).ToArray();
            polygon.Add(new GeoRing(Stitch(Arcs, indices), indices));
        }
        return polygon;
    }

    /// <summary>topojson-client's ring: each arc's points (reversed for <c>~i</c>), dropping the point shared with the previous arc.</summary>
    internal static GeoPos[] Stitch(IReadOnlyList<GeoPos[]> arcs, IReadOnlyList<int> indices)
    {
        var points = new List<GeoPos>();
        foreach (var i in indices)
        {
            var k = i < 0 ? ~i : i;
            if (k < 0 || k >= arcs.Count) continue;
            if (points.Count > 0) points.RemoveAt(points.Count - 1);
            var a = arcs[k];
            if (i < 0) for (var j = a.Length - 1; j >= 0; j--) points.Add(a[j]);
            else points.AddRange(a);
        }
        if (points.Count > 0 && points.Count < 4) points.Add(points[0]);
        return points.ToArray();
    }

    // ----- GeoJSON -----

    static IReadOnlyList<GeoFeature> ParseGeoJson(JsonElement root)
    {
        var list = new List<GeoFeature>();
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var f in root.EnumerateArray()) AddGeoJson(list, f);
            return list;
        }
        AddGeoJson(list, root);
        return list;
    }

    static void AddGeoJson(List<GeoFeature> list, JsonElement e)
    {
        if (e.ValueKind != JsonValueKind.Object) return;
        switch (Str(e, "type"))
        {
            case "FeatureCollection":
                if (e.TryGetProperty("features", out var fs) && fs.ValueKind == JsonValueKind.Array)
                    foreach (var f in fs.EnumerateArray()) AddGeoJson(list, f);
                break;
            case "Feature":
            {
                var feature = new GeoFeature { Id = IdOf(e), Name = NameOf(e) };
                if (e.TryGetProperty("geometry", out var geometry)) AddGeoJsonGeometry(feature, geometry);
                list.Add(feature);
                break;
            }
            case "Polygon" or "MultiPolygon" or "GeometryCollection":
            {
                var feature = new GeoFeature { Id = IdOf(e), Name = NameOf(e) };
                AddGeoJsonGeometry(feature, e);
                list.Add(feature);
                break;
            }
        }
    }

    static void AddGeoJsonGeometry(GeoFeature f, JsonElement g)
    {
        if (g.ValueKind != JsonValueKind.Object) return;
        switch (Str(g, "type"))
        {
            case "Polygon":
                if (g.TryGetProperty("coordinates", out var pc)) f.Polygons.Add(GeoJsonPolygon(pc));
                break;
            case "MultiPolygon":
                if (g.TryGetProperty("coordinates", out var mc))
                    foreach (var poly in mc.EnumerateArray()) f.Polygons.Add(GeoJsonPolygon(poly));
                break;
            case "GeometryCollection":
                if (g.TryGetProperty("geometries", out var gs))
                    foreach (var child in gs.EnumerateArray()) AddGeoJsonGeometry(f, child);
                break;
        }
    }

    static List<GeoRing> GeoJsonPolygon(JsonElement rings) =>
        rings.EnumerateArray().Select(r => new GeoRing(r.EnumerateArray().Select(p => new GeoPos(p[0].GetDouble(), p[1].GetDouble())).ToArray())).ToList();

    // ----- helpers -----

    static string? Str(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    static string? IdOf(JsonElement e)
    {
        if (e.ValueKind != JsonValueKind.Object || !e.TryGetProperty("id", out var id)) return null;
        return id.ValueKind switch
        {
            JsonValueKind.String => id.GetString(),
            JsonValueKind.Number => id.TryGetInt64(out var l) ? l.ToString(CultureInfo.InvariantCulture) : id.GetDouble().ToString(CultureInfo.InvariantCulture),
            _ => null,
        };
    }

    static string NameOf(JsonElement e)
    {
        if (e.ValueKind == JsonValueKind.Object && e.TryGetProperty("properties", out var p) && p.ValueKind == JsonValueKind.Object)
        {
            if (p.TryGetProperty("NAME", out var upper) && upper.ValueKind == JsonValueKind.String) return upper.GetString() ?? "";
            if (p.TryGetProperty("name", out var lower) && lower.ValueKind == JsonValueKind.String) return lower.GetString() ?? "";
        }
        return IdOf(e) ?? "";
    }
}
