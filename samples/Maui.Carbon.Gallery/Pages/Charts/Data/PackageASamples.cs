namespace Maui.Carbon.Gallery.Pages.Charts.Data;

/// <summary>Sample data of the grouped and stacked bar, histogram, box plot, bullet and lollipop chart pages (the Carbon previews' data).</summary>
public static class PackageASamples
{
    static readonly string[] Quarters = ["Q1", "Q2", "Q3", "Q4"];

    // ----- grouped bars -----

    public static IReadOnlyList<CdsChartPoint> RevenueByRegion { get; } = Categories(Quarters,
        ("North America", [42, 47, 51, 58]),
        ("Europe", [35, 33, 38, 41]),
        ("Asia Pacific", [28, 31, 36, 44]),
        ("Latin America", [12, 14, 13, 17]));

    public static IReadOnlyList<CdsChartPoint> Tickets { get; } = Monthly(
        ("Opened", [310, 284, 342, 298, 365, 331]),
        ("Resolved", [295, 301, 318, 322, 340, 352]));

    public static IReadOnlyList<CdsChartPoint> MarginChange { get; } = Categories(Quarters,
        ("Hardware", [4.2, -1.8, 2.6, 3.9]),
        ("Software", [8.1, 6.4, -0.9, 7.2]),
        ("Services", [-2.3, 1.1, 3.4, 2.0]));

    /// <summary>Europe has no Q2 value: with PreserveSpaceForMissingBars off the other bars close the gap.</summary>
    public static IReadOnlyList<CdsChartPoint> RevenueWithGap { get; } =
        RevenueByRegion.Where(p => !(p.Group == "Europe" && p.Key == "Q2")).ToList();

    // ----- stacked bars -----

    public static IReadOnlyList<CdsChartPoint> UnitsByChannel { get; } = Categories(["Jan", "Feb", "Mar", "Apr", "May"],
        ("Direct", [120, 135, 98, 150, 142]),
        ("Partner", [80, 72, 95, 88, 101]),
        ("Online", [45, 60, 74, 91, 110]));

    public static IReadOnlyList<CdsChartPoint> EffortByTeam { get; } = Categories(["Platform", "Mobile", "Web", "Data", "Security"],
        ("Features", [34, 28, 40, 22, 12]),
        ("Bugs", [12, 18, 15, 9, 6]),
        ("Tech debt", [10, 6, 8, 14, 10]),
        ("Support", [6, 10, 7, 5, 14]));

    public static IReadOnlyList<CdsChartPoint> CashFlow { get; } = Monthly(
        ("Deposits", [52, 61, 48, 70, 66, 58]),
        ("Interest", [8, 8, 9, 9, 10, 10]),
        ("Withdrawals", [-38, -45, -62, -41, -50, -57]),
        ("Fees", [-4, -4, -5, -4, -6, -5]));

    // ----- histogram (the preview's seeded samples, one generator for all three) -----

    public static IReadOnlyList<CdsChartPoint> EmployeeAges { get; }

    public static IReadOnlyList<CdsChartPoint> ResponseTimes { get; }

    public static IReadOnlyList<CdsChartPoint> OrderValues { get; }

    public static IList<double> PriceBands { get; } = [0, 25, 50, 75, 100, 150, 200, 300, 500];

    // ----- box plots -----

    public static IReadOnlyList<CdsChartPoint> Salaries { get; }

    public static IReadOnlyList<CdsChartPoint> BuildTimes { get; }

    // ----- bullets -----

    public static IReadOnlyList<CdsBulletChartItem> Kpis { get; } = new[]
    {
        new CdsBulletChartItem("Revenue (USD K)", "Q3", 372, 400, 280, 360, 420),
        new CdsBulletChartItem("New customers", "Q3", 198, 240, 150, 210, 260),
        new CdsBulletChartItem("Avg. order (USD)", "Q3", 445, 420, 300, 380, 450),
        new CdsBulletChartItem("Satisfaction", "Q3", 71, 85, 60, 75, 90),
    }.Select(d => new CdsBulletChartItem(d.Title!, d.Group!, d.Value / d.Target * 100, 100, d.Ranges!.Select(r => r / d.Target!.Value * 100).ToArray())).ToList();

    public static IReadOnlyList<CdsBulletChartItem> SalesReps { get; } =
    [
        new("North", "On track", 872, 900, 540, 765, 900),
        new("South", "On track", 745, 800, 480, 680, 800),
        new("East", "Behind", 612, 1000, 600, 850, 1000),
        new("West", "Behind", 388, 700, 420, 595, 700),
    ];

    public static IList<string> PlanTitles { get; } = ["Below plan", "On plan", "Quota met"];

    // ----- lollipops -----

    public static IReadOnlyList<CdsChartPoint> Nps { get; } =
    [
        new("NPS", "Onboarding", 42), new("NPS", "Billing", 18), new("NPS", "Search", 35),
        new("NPS", "Mobile app", 51), new("NPS", "Support", 27), new("NPS", "Reports", 12),
    ];

    public static IReadOnlyList<CdsChartPoint> Languages { get; } =
    [
        new("Web", "JavaScript", 62.3), new("Web", "HTML/CSS", 52.9), new("General purpose", "Python", 51.0),
        new("Data", "SQL", 51.5), new("Web", "TypeScript", 38.5), new("General purpose", "Java", 30.3),
        new("General purpose", "C#", 27.1), new("General purpose", "Go", 13.5),
    ];

    public static IReadOnlyList<CdsChartPoint> Empty { get; } = [];

    static PackageASamples()
    {
        var r = new Seeded(7);
        EmployeeAges = Enumerable.Range(0, 400).Select(_ => new CdsChartPoint { Group = "Employees", Value = JsRound(Math.Max(19, Math.Min(66, r.Normal(38, 9)))) }).ToList();
        var latency = new List<CdsChartPoint>();
        foreach (var (g, m, s, n) in new[] { ("us-east", 140.0, 28.0, 260), ("eu-west", 175.0, 35.0, 220), ("ap-south", 230.0, 45.0, 160) })
            for (var i = 0; i < n; i++) latency.Add(new CdsChartPoint { Group = g, Value = Math.Max(40, r.Normal(m, s)) });
        ResponseTimes = latency;
        var orders = new List<CdsChartPoint>();
        for (var i = 0; i < 300; i++)
        {
            var v = Math.Exp(r.Normal(3.6, 0.7));
            orders.Add(new CdsChartPoint { Group = v > 150 ? "Wholesale" : "Retail", Value = Math.Min(499, v) });
        }
        OrderValues = orders;

        var b = new Seeded(11);
        var salaries = new List<CdsChartPoint>();
        foreach (var (g, m, s) in new[] { ("Design", 78.0, 9.0), ("Engineering", 96.0, 14.0), ("Marketing", 71.0, 11.0), ("Sales", 68.0, 16.0), ("Support", 54.0, 7.0) })
            for (var i = 0; i < 40; i++) salaries.Add(new CdsChartPoint { Group = g, Value = JsRound(b.Normal(m, s) * 10) / 10 });
        salaries.AddRange([new() { Group = "Engineering", Value = 162 }, new() { Group = "Sales", Value = 131 }, new() { Group = "Support", Value = 88 }, new() { Group = "Design", Value = 41 }]);
        Salaries = salaries;
        var builds = new List<CdsChartPoint>();
        foreach (var (g, m, s) in new[] { ("main", 6.2, 0.8), ("release", 7.4, 1.1), ("feature", 5.1, 1.6), ("hotfix", 3.8, 0.6) })
            for (var i = 0; i < 30; i++) builds.Add(new CdsChartPoint { Group = g, Value = JsRound(Math.Max(1, b.Normal(m, s)) * 10) / 10 });
        builds.AddRange([new() { Group = "feature", Value = 14.2 }, new() { Group = "main", Value = 11.6 }]);
        BuildTimes = builds;
    }

    static double JsRound(double v) => Math.Floor(v + 0.5);

    /// <summary>The previews' Park-Miller generator and Box-Muller normal samples, so the gallery shows the same data.</summary>
    sealed class Seeded(long seed)
    {
        long _seed = seed;

        double Next()
        {
            _seed = _seed * 16807 % 2147483647;
            return (_seed - 1) / 2147483646.0;
        }

        public double Normal(double mean, double sd)
        {
            double u = 0, v = 0;
            while (u == 0) u = Next();
            while (v == 0) v = Next();
            return mean + sd * Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
        }
    }

    static List<CdsChartPoint> Categories(string[] keys, params (string Group, double[] Values)[] series) =>
        series.SelectMany(s => s.Values.Select((v, i) => new CdsChartPoint(s.Group, keys[i], v))).ToList();

    static List<CdsChartPoint> Monthly(params (string Group, double[] Values)[] series) =>
        series.SelectMany(s => s.Values.Select((v, i) => new CdsChartPoint(s.Group, new DateTime(2024, 1 + i, 1), v))).ToList();
}
