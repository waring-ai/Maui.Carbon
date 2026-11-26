namespace Maui.Carbon;

/// <summary>
/// One row of a <see cref="CdsBulletChart"/> (Carbon's bullet data): a measure's title, its group, the value, the target
/// and the starts of its performance areas. The chart does not watch an item's properties: replace the item, or the
/// collection, to change the chart.
/// </summary>
public sealed class CdsBulletChartItem
{
    /// <summary>Creates an empty item (for XAML).</summary>
    public CdsBulletChartItem()
    {
    }

    /// <summary>Creates an item: the measure's title, its group, its value, the target and the starts of the poor, satisfactory and great areas.</summary>
    public CdsBulletChartItem(string title, string group, double? value, double? target, params double[] ranges)
    {
        Title = title;
        Group = group;
        Value = value;
        Target = target;
        Ranges = ranges.Length > 0 ? ranges : null;
    }

    /// <summary>The measure (Carbon's <c>title</c>): the row's label on the labels axis ("Revenue (USD K)", "North").</summary>
    public string? Title { get; set; }

    /// <summary>The series (Carbon's <c>group</c>): its legend item and the bar color ("On track", "Q3").</summary>
    public string? Group { get; set; }

    /// <summary>The measured value (Carbon's <c>value</c>), drawn as an 8px bar from zero. <c>null</c> draws no bar.</summary>
    public double? Value { get; set; }

    /// <summary>The target (Carbon's <c>marker</c>), drawn as a 24px line with quartile ticks at 25, 50 and 75% of it.</summary>
    public double? Target { get; set; }

    /// <summary>
    /// The starts of the performance areas in ascending order (Carbon's <c>ranges</c>), usually three: poor, satisfactory
    /// and great. The first area always spans the axis; ranges beyond the axis are not drawn.
    /// </summary>
    public IList<double>? Ranges { get; set; }

    /// <summary>Any object of your own carried with the item; the chart never reads it.</summary>
    public object? Tag { get; set; }
}
