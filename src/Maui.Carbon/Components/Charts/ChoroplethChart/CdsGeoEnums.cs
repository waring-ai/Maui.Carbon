namespace Maui.Carbon;

/// <summary>The map projection of a map chart (Carbon's <c>thematic.projection</c>, d3-geo's projections).</summary>
public enum CdsGeoProjection
{
    /// <summary>Natural Earth 1 (<c>geoNaturalEarth1</c>, default): a compromise projection for world maps.</summary>
    NaturalEarth1,
    /// <summary>Equal Earth (<c>geoEqualEarth</c>): equal-area, for world maps where sizes compare.</summary>
    EqualEarth,
    /// <summary>Equirectangular (<c>geoEquirectangular</c>): longitude and latitude as x and y.</summary>
    Equirectangular,
    /// <summary>Mercator (<c>geoMercator</c>): conformal; exaggerates high latitudes, best for regions near the equator.</summary>
    Mercator,
    /// <summary>Conic equal-area (<c>geoConicEqualArea</c>, standard parallels 0 and 60): continents in the mid latitudes, such as Europe.</summary>
    ConicEqualArea,
    /// <summary>Albers (<c>geoAlbers</c>: conic equal-area, parallels 29.5 and 45.5, rotated to center the United States).</summary>
    Albers,
    /// <summary>Conic equidistant (<c>geoConicEquidistant</c>, standard parallels 0 and 60).</summary>
    ConicEquidistant,
}

/// <summary>What a map's projection is fitted to (Carbon's <c>thematic.fitTo</c>).</summary>
public enum CdsMapFit
{
    /// <summary>The regions or locations that have data: a dataset for one continent zooms to it.</summary>
    Data,
    /// <summary>Every region of the base map.</summary>
    Map,
}

/// <summary>
/// A box in degrees that a map's view is fitted to (Carbon's <c>thematic.fitTo: [[west, south], [east, north]]</c>).
/// The chart does not watch its properties: assign a new box to change the view.
/// </summary>
public sealed class CdsGeoBounds
{
    /// <summary>Creates an empty box (for XAML).</summary>
    public CdsGeoBounds()
    {
    }

    /// <summary>Creates a box from its west, south, east and north edges in degrees.</summary>
    public CdsGeoBounds(double west, double south, double east, double north)
    {
        West = west;
        South = south;
        East = east;
        North = north;
    }

    /// <summary>The western edge (longitude, degrees).</summary>
    public double West { get; set; }

    /// <summary>The southern edge (latitude, degrees).</summary>
    public double South { get; set; }

    /// <summary>The eastern edge (longitude, degrees).</summary>
    public double East { get; set; }

    /// <summary>The northern edge (latitude, degrees).</summary>
    public double North { get; set; }

    /// <summary>The corners and edge midpoints, as Carbon fits the box (a MultiPoint).</summary>
    internal IEnumerable<GeoPos> FitPoints()
    {
        double w = West, s = South, e = East, n = North;
        return [new(w, s), new(e, s), new(e, n), new(w, n), new((w + e) / 2, s), new((w + e) / 2, n), new(w, (s + n) / 2), new(e, (s + n) / 2)];
    }
}
