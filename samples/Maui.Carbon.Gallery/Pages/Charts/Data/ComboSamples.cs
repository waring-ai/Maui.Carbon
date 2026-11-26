namespace Maui.Carbon.Gallery.Pages.Charts.Data;

/// <summary>Sample data of the combo chart page (the design system preview's data).</summary>
public static class ComboSamples
{
    static readonly string[] Days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
    static readonly string[] Quarters = ["Q1", "Q2", "Q3", "Q4"];
    static readonly string[] Months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun"];

    public static IReadOnlyList<CdsChartPoint> Visitors { get; } =
    [
        .. new double[] { 42, 51, 48, 60, 71, 95, 88 }.Select((v, i) => new CdsChartPoint("Visitors (thousands)", Days[i], v)),
        .. new double[] { 15, 17, 16, 21, 24, 27, 26 }.Select((v, i) => new CdsChartPoint("Temperature (°C)", Days[i], v)),
    ];

    public static IList<string> TemperatureDataset { get; } = ["Temperature (°C)"];

    public static IList<string> VisitorsDataset { get; } = ["Visitors (thousands)"];

    public static IReadOnlyList<CdsChartPoint> Revenue { get; } =
    [
        .. Series("Cloud", Quarters, [42, 48, 55, 61]),
        .. Series("Software", Quarters, [31, 30, 33, 35]),
        .. Series("Consulting", Quarters, [18, 21, 19, 24]),
        .. Series("Target", Quarters, [90, 95, 104, 112]),
    ];

    public static IList<string> LinesOfBusiness { get; } = ["Cloud", "Software", "Consulting"];

    public static IList<string> Target { get; } = ["Target"];

    public static IReadOnlyList<CdsChartPoint> Tickets { get; } =
    [
        .. Series("Opened", Months, [120, 135, 128, 160, 150, 142]),
        .. Series("Resolved", Months, [110, 128, 131, 149, 155, 147]),
        .. Series("Backlog", Months, [40, 47, 44, 55, 50, 45]),
    ];

    public static IList<string> OpenedResolved { get; } = ["Opened", "Resolved"];

    public static IList<string> Backlog { get; } = ["Backlog"];

    static IEnumerable<CdsChartPoint> Series(string group, string[] keys, double[] values) =>
        values.Select((v, i) => new CdsChartPoint(group, keys[i], v));
}
