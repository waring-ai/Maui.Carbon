namespace Maui.Carbon;

/// <summary>
/// A status range of a meter (Carbon's <c>meter.status.ranges</c> entry <c>{ range: [From, To], status }</c>): when the
/// meter's value (or a proportional meter's total) falls within From to To, inclusive, the bar takes the status color and
/// the status icon shows. The first matching range wins.
/// </summary>
public sealed class CdsMeterStatusRange
{
    /// <summary>Creates an empty range (for XAML).</summary>
    public CdsMeterStatusRange()
    {
    }

    /// <summary>Creates a range from <paramref name="from"/> to <paramref name="to"/> with a status.</summary>
    public CdsMeterStatusRange(double from, double to, CdsChartStatus status)
    {
        From = from;
        To = to;
        Status = status;
    }

    /// <summary>The start of the range, inclusive.</summary>
    public double From { get; set; }

    /// <summary>The end of the range, inclusive.</summary>
    public double To { get; set; }

    /// <summary>The status of values in the range.</summary>
    public CdsChartStatus Status { get; set; }

    /// <summary>Whether <paramref name="value"/> is within the range.</summary>
    public bool Contains(double value) => From <= value && value <= To;
}
