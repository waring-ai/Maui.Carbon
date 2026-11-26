namespace Maui.Carbon;

/// <summary>
/// One data row of an axis, pie, radar, heat map, word cloud or map chart (Carbon's data objects). Which fields a chart
/// reads depends on its axes: a <see cref="CdsChartScaleType.Labels"/> axis reads <see cref="Key"/>, a
/// <see cref="CdsChartScaleType.Time"/> axis <see cref="Date"/>, a linear domain axis <see cref="X"/>, and the range
/// (value) axis <see cref="Value"/>. Each chart's README lists the fields it uses. The chart does not watch a point's
/// properties: replace the point, or the collection, to change the chart.
/// </summary>
public sealed class CdsChartPoint
{
    /// <summary>Creates an empty point (for XAML).</summary>
    public CdsChartPoint()
    {
    }

    /// <summary>Creates a categorical point: a group, a label and a value.</summary>
    public CdsChartPoint(string group, string key, double? value)
    {
        Group = group;
        Key = key;
        Value = value;
    }

    /// <summary>Creates a time-series point: a group, a date and a value.</summary>
    public CdsChartPoint(string group, DateTime date, double? value)
    {
        Group = group;
        Date = date;
        Value = value;
    }

    /// <summary>Creates a numeric point (scatter, bubble): a group, an x and a y value.</summary>
    public CdsChartPoint(string group, double x, double? value)
    {
        Group = group;
        X = x;
        Value = value;
    }

    /// <summary>The series (Carbon's <c>group</c>): one legend item and one color. Points without a group join a single unnamed series.</summary>
    public string? Group { get; set; }

    /// <summary>The category on a labels axis (Carbon's <c>key</c>), the slice name of a pie, the word of a word cloud, or the region of a map.</summary>
    public string? Key { get; set; }

    /// <summary>The date on a time axis (Carbon's <c>date</c>), in local time.</summary>
    public DateTime? Date { get; set; }

    /// <summary>The number on a linear domain axis (the x of a scatter or bubble chart).</summary>
    public double? X { get; set; }

    /// <summary>The measured value (Carbon's <c>value</c>). <c>null</c> leaves a gap: a line breaks there and no bar is drawn.</summary>
    public double? Value { get; set; }

    /// <summary>The start of a range (floating bars, bounds): with <see cref="High"/>, the bar spans Low to High instead of 0 to Value.</summary>
    public double? Low { get; set; }

    /// <summary>The end of a range (floating bars, bounds).</summary>
    public double? High { get; set; }

    /// <summary>The third measure of a bubble chart, mapped to the bubble's area (Carbon's <c>radius</c>).</summary>
    public double? Size { get; set; }

    /// <summary>Any object of your own (a view model row) carried with the point; the chart never reads it.</summary>
    public object? Tag { get; set; }

    /// <summary>True when the point has a range (<see cref="Low"/> and <see cref="High"/>).</summary>
    public bool HasRange => Low.HasValue && High.HasValue;
}
