namespace Maui.Carbon;

/// <summary>
/// One connection of a <see cref="CdsConnectionMap"/>: a source and a target, located by their own coordinates
/// (<see cref="FromLongitude"/>…) or by name in <see cref="CdsConnectionMap.Nodes"/>, with a value and a group.
/// </summary>
public sealed class CdsGeoConnection
{
    /// <summary>Creates an empty connection (for XAML).</summary>
    public CdsGeoConnection()
    {
    }

    /// <summary>Creates a connection between two named nodes (listed in <see cref="CdsConnectionMap.Nodes"/>).</summary>
    public CdsGeoConnection(string group, string source, string target, double? value)
    {
        Group = group;
        Source = source;
        Target = target;
        Value = value;
    }

    /// <summary>Creates a connection between two coordinates (longitude, latitude in degrees).</summary>
    public CdsGeoConnection(string group, string source, string target, double fromLongitude, double fromLatitude, double toLongitude, double toLatitude, double? value)
        : this(group, source, target, value)
    {
        FromLongitude = fromLongitude;
        FromLatitude = fromLatitude;
        ToLongitude = toLongitude;
        ToLatitude = toLatitude;
    }

    /// <summary>The series (Carbon's <c>group</c>): its color and legend item.</summary>
    public string? Group { get; set; }

    /// <summary>The source's name (Carbon's <c>sourceMapsTo</c>); also how it is found in <see cref="CdsConnectionMap.Nodes"/>.</summary>
    public string? Source { get; set; }

    /// <summary>The target's name (Carbon's <c>targetMapsTo</c>).</summary>
    public string? Target { get; set; }

    /// <summary>The source's longitude (Carbon's <c>from[0]</c>); with <see cref="FromLatitude"/> it wins over the named node.</summary>
    public double? FromLongitude { get; set; }

    /// <summary>The source's latitude (Carbon's <c>from[1]</c>).</summary>
    public double? FromLatitude { get; set; }

    /// <summary>The target's longitude (Carbon's <c>to[0]</c>).</summary>
    public double? ToLongitude { get; set; }

    /// <summary>The target's latitude (Carbon's <c>to[1]</c>).</summary>
    public double? ToLatitude { get; set; }

    /// <summary>The value that sets the line width (Carbon's <c>valueMapsTo</c>): the largest value draws the widest line.</summary>
    public double? Value { get; set; }

    /// <summary>Any object of your own carried with the connection; the chart never reads it.</summary>
    public object? Tag { get; set; }
}
