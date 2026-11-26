namespace Maui.Carbon.Gallery.Pages.Charts.Data;

/// <summary>Sample data of the simple bar and line chart pages (Carbon Charts' demo data).</summary>
public static class ChartSamples
{
    public static IReadOnlyList<CdsChartPoint> Sales { get; } =
    [
        new("Qty", "Qty", 65000), new("More", "More", 29123), new("Sold", "Sold", 35213),
        new("Restocking", "Restocking", 51213), new("Misc", "Misc", 16932),
    ];

    public static IReadOnlyList<CdsChartPoint> Temperatures { get; } =
    [
        new() { Group = "London", Low = 12, High = 31 }, new() { Group = "New York", Low = 4, High = 27 },
        new() { Group = "Paris", Low = 9, High = 25 }, new() { Group = "Tokyo", Low = 2, High = 29 },
        new() { Group = "Toronto", Low = -12, High = 23 },
    ];

    public static IReadOnlyList<CdsChartPoint> SubscriberChange { get; } =
    [
        new("Jan", "Jan", 4200), new("Feb", "Feb", -2300), new("Mar", "Mar", 3100),
        new("Apr", "Apr", -800), new("May", "May", 5600), new("Jun", "Jun", 1200),
    ];

    public static IDictionary<string, string> OneColor { get; } = new Dictionary<string, string>
    {
        ["Jan"] = CdsColorKeys.DataVis01, ["Feb"] = CdsColorKeys.DataVis01, ["Mar"] = CdsColorKeys.DataVis01,
        ["Apr"] = CdsColorKeys.DataVis01, ["May"] = CdsColorKeys.DataVis01, ["Jun"] = CdsColorKeys.DataVis01,
    };

    public static IReadOnlyList<CdsChartPoint> WeeklyRevenue { get; } = Weekly(
        ("Dataset 1", [41200, 32100, 52300, 38900, 61200, 49800]),
        ("Dataset 2", [22100, 25600, 21900, 31400, 28300, 35100]),
        ("Dataset 3", [11300, 18900, 16100, 13500, 22400, 19800]),
        ("Dataset 4", [5400, 8100, 12300, 9900, 7400, 14100]));

    public static IReadOnlyList<CdsChartPoint> StockByDataset { get; } = Categories(
        ["Qty", "More", "Sold", "Restocking", "Misc"],
        ("Dataset 1", [34200, 23500, 53100, 42300, 12300]),
        ("Dataset 2", [34200, 53100, 42300, 21400, 0]));

    public static IReadOnlyList<CdsChartPoint> OnCall { get; } =
        new double[] { 2, 3, 3, 1, 4, 2, 5, 3 }.Select((v, i) => new CdsChartPoint("On call", new DateTime(2023, 5, 1 + i), v)).ToList();

    public static IReadOnlyList<CdsChartPoint> Empty { get; } = [];

    static List<CdsChartPoint> Weekly(params (string Group, double[] Values)[] series) =>
        series.SelectMany(s => s.Values.Select((v, i) => new CdsChartPoint(s.Group, new DateTime(2023, 1, 1).AddDays(i * 7), v))).ToList();

    static List<CdsChartPoint> Categories(string[] keys, params (string Group, double[] Values)[] series) =>
        series.SelectMany(s => s.Values.Select((v, i) => new CdsChartPoint(s.Group, keys[i], v))).ToList();
}
