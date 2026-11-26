namespace Maui.Carbon;

/// <summary>The shape of a gauge (Carbon's <c>gauge.type</c>).</summary>
public enum CdsGaugeChartType
{
    /// <summary>A half circle with the numbers on its baseline (default).</summary>
    Semi,
    /// <summary>A full ring with the numbers in its middle.</summary>
    Full,
}

/// <summary>Which way a gauge's delta arrow points (Carbon's <c>gauge.deltaArrow.direction</c>).</summary>
public enum CdsGaugeDeltaArrowDirection
{
    /// <summary>Up for a positive delta, down otherwise (default).</summary>
    Auto,
    /// <summary>Always up.</summary>
    Up,
    /// <summary>Always down.</summary>
    Down,
}

/// <summary>
/// A status of a chart value (Carbon's <c>status</c>: <c>'success'</c>, <c>'warning'</c>, <c>'danger'</c>), colored
/// <c>support-success</c>, <c>support-warning</c> and <c>support-error</c>. Use status colors only for status, and always
/// with text or an icon.
/// </summary>
public enum CdsChartStatus
{
    /// <summary>Good (<c>support-success</c>).</summary>
    Success,
    /// <summary>Needs attention (<c>support-warning</c>).</summary>
    Warning,
    /// <summary>Critical (<c>support-error</c>).</summary>
    Danger,
}

/// <summary>The color key of a <see cref="CdsChartStatus"/>.</summary>
internal static class ChartStatusColors
{
    public static string Key(CdsChartStatus status) => status switch
    {
        CdsChartStatus.Danger => CdsColorKeys.SupportError,
        CdsChartStatus.Warning => CdsColorKeys.SupportWarning,
        _ => CdsColorKeys.SupportSuccess,
    };

    /// <summary>Carbon's status name ("danger").</summary>
    public static string Name(CdsChartStatus status) => status switch
    {
        CdsChartStatus.Danger => "danger",
        CdsChartStatus.Warning => "warning",
        _ => "success",
    };
}
