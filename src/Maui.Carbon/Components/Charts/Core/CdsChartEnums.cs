namespace Maui.Carbon;

/// <summary>Where a chart's legend sits relative to the graph (Carbon's <c>legend.position</c>).</summary>
public enum CdsChartLegendPosition
{
    /// <summary>Below the graph (default).</summary>
    Bottom,
    /// <summary>Above the graph; use where space is scarce, such as dashboards.</summary>
    Top,
    /// <summary>Left of the graph, one item per row.</summary>
    Left,
    /// <summary>Right of the graph, one item per row.</summary>
    Right,
}

/// <summary>How a horizontal legend aligns its items (Carbon's <c>legend.alignment</c>).</summary>
public enum CdsChartLegendAlignment
{
    /// <summary>Start of the row (default).</summary>
    Start,
    /// <summary>Centered.</summary>
    Center,
    /// <summary>End of the row.</summary>
    End,
}

/// <summary>The scale of a chart axis (Carbon's <c>scaleType</c>).</summary>
public enum CdsChartScaleType
{
    /// <summary>Continuous numbers. On the domain axis it reads <see cref="CdsChartPoint.X"/>; on the range axis <see cref="CdsChartPoint.Value"/>.</summary>
    Linear,
    /// <summary>Continuous numbers on a base-10 logarithmic scale.</summary>
    Log,
    /// <summary>Categories in data order; reads <see cref="CdsChartPoint.Key"/>.</summary>
    Labels,
    /// <summary>Dates and times; reads <see cref="CdsChartPoint.Date"/>.</summary>
    Time,
}

/// <summary>When horizontal tick labels rotate 45 degrees (Carbon's <c>ticks.rotation</c>).</summary>
public enum CdsChartTickRotation
{
    /// <summary>Rotate when the labels would collide (default).</summary>
    Auto,
    /// <summary>Always rotate.</summary>
    Always,
    /// <summary>Never rotate.</summary>
    Never,
}

/// <summary>Where long labels are cut (Carbon's <c>truncation.type</c>).</summary>
public enum CdsChartTruncation
{
    /// <summary>Keep the start, then "..." (default): "Qty in sto...".</summary>
    End,
    /// <summary>Keep the start and the end around "...": "Qty i...tock".</summary>
    Middle,
    /// <summary>Keep the end: "...in stock".</summary>
    Front,
    /// <summary>Never truncate.</summary>
    None,
}

/// <summary>How lines and areas join their points (the d3 curve names Carbon accepts in <c>curve</c>).</summary>
public enum CdsChartCurve
{
    /// <summary>Straight segments (default).</summary>
    Linear,
    /// <summary>Smooth, monotone in x: never overshoots the data. The usual choice for smooth time series.</summary>
    MonotoneX,
    /// <summary>Smooth, monotone in y.</summary>
    MonotoneY,
    /// <summary>A natural cubic spline through every point.</summary>
    Natural,
    /// <summary>A Catmull-Rom spline through every point.</summary>
    CatmullRom,
    /// <summary>A cardinal spline through every point.</summary>
    Cardinal,
    /// <summary>A B-spline that approaches but does not pass through the points.</summary>
    Basis,
    /// <summary>Steps that change halfway between points.</summary>
    Step,
    /// <summary>Steps that change at each point (the value holds until the next point): step charts.</summary>
    StepAfter,
    /// <summary>Steps that change just before each point.</summary>
    StepBefore,
}

/// <summary>The controls of a chart's toolbar (Carbon's <c>toolbar.controls</c>).</summary>
[Flags]
public enum CdsChartToolbarControls
{
    /// <summary>No controls.</summary>
    None = 0,
    /// <summary>"Show as table": the data in a data table, in a modal with "Download as CSV".</summary>
    ShowAsTable = 1,
    /// <summary>"Make fullscreen": the chart fills the page until Escape or "Exit fullscreen".</summary>
    Fullscreen = 2,
    /// <summary>"Export to CSV".</summary>
    ExportCsv = 4,
    /// <summary>"Export to PNG".</summary>
    ExportPng = 8,
    /// <summary>"Export to JPG".</summary>
    ExportJpg = 16,
    /// <summary>Every control (default).</summary>
    All = ShowAsTable | Fullscreen | ExportCsv | ExportPng | ExportJpg,
}

/// <summary>The file format of a chart export.</summary>
public enum CdsChartExportFormat
{
    /// <summary>Comma-separated values of the table data.</summary>
    Csv,
    /// <summary>A PNG image of the chart.</summary>
    Png,
    /// <summary>A JPEG image of the chart.</summary>
    Jpg,
}

/// <summary>A time axis tick interval (Carbon's <c>timeScale.timeInterval</c>); <see cref="Auto"/> picks one from the data.</summary>
public enum CdsChartTimeInterval
{
    /// <summary>Pick from the tick spacing (default).</summary>
    Auto,
    /// <summary>15 seconds.</summary>
    FifteenSeconds,
    /// <summary>A minute.</summary>
    Minute,
    /// <summary>30 minutes.</summary>
    ThirtyMinutes,
    /// <summary>An hour.</summary>
    Hourly,
    /// <summary>A day.</summary>
    Daily,
    /// <summary>A week.</summary>
    Weekly,
    /// <summary>A month.</summary>
    Monthly,
    /// <summary>A quarter.</summary>
    Quarterly,
    /// <summary>A year.</summary>
    Yearly,
}
