namespace Maui.Carbon.Gallery.Pages.Charts.Data;

/// <summary>Sample data of the pie, donut, gauge, meter and radar chart pages (mirrors their Carbon previews).</summary>
public static class PackageCSamples
{
    public static IReadOnlyList<CdsChartPoint> Tickets { get; } = Parts(
        ("Resolved", 412), ("In progress", 138), ("Waiting on customer", 76), ("Escalated", 9), ("Reopened", 4));

    public static IReadOnlyList<CdsChartPoint> Browsers { get; } = Parts(
        ("Chrome", 61.2), ("Safari", 19.8), ("Edge", 9.5), ("Firefox", 7.6), ("Opera", 1.1), ("Other", 0.8));

    public static IReadOnlyList<CdsChartPoint> Energy { get; } = Parts(("Solar", 2310), ("Wind", 1840), ("Hydro", 1220), ("Gas", 960));

    public static IReadOnlyList<CdsChartPoint> Regions { get; } = Parts(("Americas", 41.2), ("EMEA", 33.5), ("Asia Pacific", 25.3));

    public static IReadOnlyList<CdsChartPoint> Storage { get; } = Parts(
        ("Documents", 182), ("Photos", 420), ("Video", 615), ("Backups", 240), ("Other", 38));

    public static IReadOnlyList<CdsChartPoint> Leads { get; } = Parts(
        ("Organic search", 5120), ("Paid search", 2870), ("Referral", 1430), ("Email", 980), ("Social", 640));

    public static IReadOnlyList<CdsChartPoint> Budget { get; } = Parts(("Salaries", 1.82), ("Cloud", 0.64), ("Facilities", 0.31), ("Travel", 0.08));

    public static IReadOnlyList<CdsChartPoint> Fleet { get; } = Parts(("Available", 118), ("In service", 74), ("Maintenance", 12));

    public static IReadOnlyList<CdsChartPoint> MemoryA { get; } = Parts(("Memory used", 46));

    public static IReadOnlyList<CdsChartPoint> MemoryB { get; } = Parts(("Memory used", 78));

    public static IReadOnlyList<CdsChartPoint> MemoryC { get; } = Parts(("Memory used", 96));

    /// <summary>Carbon's preview ranges: success to 60, warning to 85, danger to 100.</summary>
    public static IList<CdsMeterStatusRange> ClusterStatus { get; } =
    [
        new(0, 60, CdsChartStatus.Success), new(60, 85, CdsChartStatus.Warning), new(85, 100, CdsChartStatus.Danger),
    ];

    public static IReadOnlyList<CdsChartPoint> Mailbox { get; } = Parts(
        ("Emails", 202), ("Photos", 654), ("Video", 1480), ("Documents", 310), ("Other", 96));

    public static IReadOnlyList<CdsChartPoint> CloudSpend { get; } = Parts(("Compute", 12.4), ("Storage", 6.1), ("Network", 2.3));

    public static IReadOnlyList<CdsChartPoint> Skills { get; } = Spokes(
        ["Frontend", "Backend", "Testing", "DevOps", "Design", "Security"],
        ("Team A", [72, 64, 88, 55, 80, 61]), ("Team B", [58, 82, 69, 77, 52, 74]));

    public static IReadOnlyList<CdsChartPoint> Weather { get; } = Spokes(
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
        ("Lisbon", [15, 16, 18, 20, 23, 27, 29, 29, 27, 23, 18, 16]), ("Oslo", [0, 1, 5, 11, 17, 21, 23, 21, 16, 10, 4, 1]));

    public static IReadOnlyList<CdsChartPoint> PhoneScores { get; } = Spokes(
        ["Price", "Battery life", "Camera", "Display", "Performance"], ("Phone X", [6.5, 8.2, 9.1, 7.4, 8.8]));

    public static IReadOnlyList<CdsChartPoint> RegionalSales { get; } = Spokes(
        ["North", "East", "South", "West", "Central"],
        ("2024", [120, 98, 86, 99, 85]), ("2025", [110, 130, 70, 105, 96]), ("2026", [95, 105, 120, 80, 110]));

    public static IReadOnlyList<CdsChartPoint> Empty { get; } = [];

    static List<CdsChartPoint> Parts(params (string Group, double Value)[] parts) =>
        parts.Select(p => new CdsChartPoint { Group = p.Group, Value = p.Value }).ToList();

    static List<CdsChartPoint> Spokes(string[] keys, params (string Group, double[] Values)[] series) =>
        series.SelectMany(s => s.Values.Select((v, i) => new CdsChartPoint(s.Group, keys[i], v))).ToList();
}
