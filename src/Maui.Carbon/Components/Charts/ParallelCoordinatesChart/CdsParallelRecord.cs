using System.Globalization;

namespace Maui.Carbon;

/// <summary>
/// One record (one line) of a <see cref="CdsParallelCoordinatesChart"/>: its name, group and a value per dimension, keyed
/// by <see cref="CdsParallelDimension.Key"/>. Numbers get a linear axis, strings a labels axis.
/// </summary>
public sealed class CdsParallelRecord
{
    /// <summary>Creates an empty record (for XAML).</summary>
    public CdsParallelRecord()
    {
    }

    /// <summary>Creates a record with its values (numbers or strings) by dimension key.</summary>
    public CdsParallelRecord(string name, string group, IDictionary<string, object?> values)
    {
        Name = name;
        Group = group;
        foreach (var (k, v) in values) Values[k] = v;
    }

    /// <summary>Creates a record with its values as (key, value) pairs.</summary>
    public CdsParallelRecord(string name, string group, params (string Key, object? Value)[] values)
    {
        Name = name;
        Group = group;
        foreach (var (k, v) in values) Values[k] = v;
    }

    /// <summary>The record's name (Carbon's <c>nameMapsTo</c>), first in its tooltip.</summary>
    public string? Name { get; set; }

    /// <summary>The series (Carbon's <c>group</c>): the line color and legend item.</summary>
    public string? Group { get; set; }

    /// <summary>The values by dimension key: numbers (any numeric type) or strings. A missing or <c>null</c> value breaks the line at that axis.</summary>
    public IDictionary<string, object?> Values { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);

    /// <summary>Any object of your own carried with the record; the chart never reads it.</summary>
    public object? Tag { get; set; }

    internal object? this[string key] => Values.TryGetValue(key, out var v) ? v : null;

    /// <summary>A value as a number, when it is one.</summary>
    internal static bool TryNumber(object? value, out double number)
    {
        switch (value)
        {
            case double d:
                number = d;
                return !double.IsNaN(d);
            case float or int or long or short or byte or decimal or uint or ulong or ushort or sbyte:
                number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return true;
            default:
                number = double.NaN;
                return false;
        }
    }
}

/// <summary>The scale of a parallel coordinates axis.</summary>
public enum CdsParallelScaleType
{
    /// <summary>Pick from the data: linear when every value is a number, else labels (default).</summary>
    Auto,
    /// <summary>A nice linear scale over the values (not forced to zero).</summary>
    Linear,
    /// <summary>Evenly spaced categories, in <see cref="CdsParallelDimension.Categories"/> or data order.</summary>
    Labels,
}

/// <summary>
/// One axis of a <see cref="CdsParallelCoordinatesChart"/> (Carbon's <c>parallelCoordinates.dimensions</c> entry): the key of
/// the values it reads, its title and scale. Name every axis with its unit. The chart does not watch its properties:
/// replace it in the collection to change it.
/// </summary>
public sealed class CdsParallelDimension
{
    /// <summary>Creates an empty dimension (for XAML).</summary>
    public CdsParallelDimension()
    {
    }

    /// <summary>Creates a dimension for a key, with a title.</summary>
    public CdsParallelDimension(string key, string? title = null)
    {
        Key = key;
        Title = title;
    }

    /// <summary>The key of the values (<see cref="CdsParallelRecord.Values"/>).</summary>
    public string Key { get; set; } = "";

    /// <summary>The axis title, semibold above the axis; default the key.</summary>
    public string? Title { get; set; }

    /// <summary>The scale; default <see cref="CdsParallelScaleType.Auto"/>.</summary>
    public CdsParallelScaleType ScaleType { get; set; }

    /// <summary>A fixed low end of a linear axis (Carbon's <c>domain[0]</c>); with <see cref="Maximum"/>, the axis is not niced.</summary>
    public double? Minimum { get; set; }

    /// <summary>A fixed high end of a linear axis (Carbon's <c>domain[1]</c>).</summary>
    public double? Maximum { get; set; }

    /// <summary>The categories of a labels axis, bottom to top (Carbon's labels <c>domain</c>); default data order.</summary>
    public IList<string>? Categories { get; set; }

    /// <summary>Formats the axis's tick labels and tooltip values (Carbon's <c>formatter</c>); default locale numbers.</summary>
    public Func<object, string>? Formatter { get; set; }

    internal string DisplayTitle => string.IsNullOrEmpty(Title) ? Key : Title!;
}

/// <summary>
/// A filter on one axis of a <see cref="CdsParallelCoordinatesChart"/> (Carbon's <c>parallelCoordinates.filters</c>, the
/// brushed range): records outside every filter turn <c>layer-accent-01</c> and lose their tooltip.
/// </summary>
public sealed class CdsParallelFilter
{
    /// <summary>Creates an empty filter (for XAML).</summary>
    public CdsParallelFilter()
    {
    }

    /// <summary>Creates a range filter on a linear axis.</summary>
    public CdsParallelFilter(string key, double minimum, double maximum)
    {
        Key = key;
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>Creates a filter on a labels axis that keeps the listed categories.</summary>
    public CdsParallelFilter(string key, IEnumerable<string> categories)
    {
        Key = key;
        Categories = categories.ToList();
    }

    /// <summary>The dimension key it filters.</summary>
    public string Key { get; set; } = "";

    /// <summary>The low end of the kept range (linear axes).</summary>
    public double? Minimum { get; set; }

    /// <summary>The high end of the kept range (linear axes).</summary>
    public double? Maximum { get; set; }

    /// <summary>The kept categories (labels axes).</summary>
    public IList<string>? Categories { get; set; }
}
