namespace Maui.Carbon;

/// <summary>
/// A located value of a <see cref="CdsProportionalSymbolMap"/> (a city, a site), or a named location in
/// <see cref="CdsConnectionMap.Nodes"/>: its name, longitude and latitude in degrees, value and group.
/// </summary>
public sealed class CdsGeoPoint
{
    /// <summary>Creates an empty point (for XAML).</summary>
    public CdsGeoPoint()
    {
    }

    /// <summary>Creates a named location (a connection map node).</summary>
    public CdsGeoPoint(string name, double longitude, double latitude)
    {
        Name = name;
        Longitude = longitude;
        Latitude = latitude;
    }

    /// <summary>Creates a located value in a group.</summary>
    public CdsGeoPoint(string group, string name, double longitude, double latitude, double? value)
    {
        Group = group;
        Name = name;
        Longitude = longitude;
        Latitude = latitude;
        Value = value;
    }

    /// <summary>The series (Carbon's <c>group</c>): its color and legend item.</summary>
    public string? Group { get; set; }

    /// <summary>The location's name (Carbon's <c>labelMapsTo</c>, default <c>name</c>), shown in the tooltip and table.</summary>
    public string? Name { get; set; }

    /// <summary>The longitude in degrees, east positive (Carbon's <c>longitudeMapsTo</c>).</summary>
    public double? Longitude { get; set; }

    /// <summary>The latitude in degrees, north positive (Carbon's <c>latitudeMapsTo</c>).</summary>
    public double? Latitude { get; set; }

    /// <summary>The value the circle's area is proportional to (Carbon's <c>radiusMapsTo</c>).</summary>
    public double? Value { get; set; }

    /// <summary>Any object of your own carried with the point; the chart never reads it.</summary>
    public object? Tag { get; set; }

    internal bool HasLocation => Longitude is { } lon && Latitude is { } lat && double.IsFinite(lon) && double.IsFinite(lat);
}
