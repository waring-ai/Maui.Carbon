namespace Maui.Carbon.Gallery.Pages.Charts.Data;

/// <summary>Sample data of the area, stacked area, sparkline, scatter and bubble chart pages (the design previews' data).</summary>
public static class PackageBSamples
{
    // ----- area -----

    public static IReadOnlyList<CdsChartPoint> Traffic { get; } = Monthly(
        ("EMEA", [42, 48, 45, 57, 61, 58, 66, 72, 69, 75, 81, 78]),
        ("Americas", [28, 31, 35, 33, 39, 44, 41, 47, 52, 50, 55, 59]));

    public static IReadOnlyList<CdsChartPoint> ActiveUsers { get; } =
        new double[] { 18200, 19400, 18800, 21300, 23900, 22700, 25100, 26800, 25400, 28900, 30200, 29600, 31800, 33400 }
            .Select((v, i) => new CdsChartPoint("Active users", new DateTime(2024, 5, 1 + i), v)).ToList();

    public static IReadOnlyList<CdsChartPoint> OrderForecast { get; } =
        new (double Value, double Lower, double Upper)[]
        {
            (310, 295, 325), (332, 312, 350), (348, 324, 371), (341, 312, 369), (365, 331, 398),
            (382, 342, 421), (376, 331, 420), (401, 349, 452), (418, 359, 476), (409, 344, 474),
        }.Select((t, i) => new CdsChartPoint("Orders", new DateTime(2024, 1, 1).AddDays(i * 7), t.Value) { Low = t.Lower, High = t.Upper }).ToList();

    public static IReadOnlyList<CdsChartPoint> TemperatureAnomaly { get; } =
        new[] { 0.4, 0.9, 0.6, -0.3, -0.8, -0.5, 0.2, 0.7, 1.2, 0.9, 0.3, -0.2, -0.6, 0.1, 0.8, 1.1 }
            .Select((v, i) => new CdsChartPoint("Anomaly", new DateTime(2009 + i, 1, 1), v)).ToList();

    // ----- stacked area -----

    public static IReadOnlyList<CdsChartPoint> Generation { get; } = Monthly(
        ("Solar", [12, 14, 19, 24, 29, 33, 35, 32, 26, 19, 14, 11]),
        ("Wind", [31, 29, 27, 22, 19, 17, 16, 18, 23, 28, 32, 34]),
        ("Hydro", [18, 19, 22, 24, 23, 21, 19, 17, 16, 17, 18, 19]),
        ("Gas", [39, 37, 31, 27, 24, 23, 25, 27, 30, 34, 37, 38]));

    public static IReadOnlyList<CdsChartPoint> Tickets { get; } = Days(
        ("Critical", [4, 6, 5, 9, 7, 3, 2]),
        ("High", [12, 15, 11, 18, 16, 9, 7]),
        ("Medium", [24, 22, 27, 31, 26, 18, 15]),
        ("Low", [31, 35, 29, 38, 33, 21, 19]));

    // ----- sparkline (seeded like the preview) -----

    public static IReadOnlyList<CdsChartPoint> Revenue { get; }

    public static IReadOnlyList<CdsChartPoint> Errors { get; }

    public static IReadOnlyList<CdsChartPoint> SignUps { get; }

    // ----- scatter -----

    public static IReadOnlyList<CdsChartPoint> Requests { get; }

    public static IReadOnlyList<CdsChartPoint> CpuLoad { get; }

    public static IReadOnlyList<CdsChartPoint> Deals { get; }

    // ----- bubble -----

    public static IReadOnlyList<CdsChartPoint> Products { get; } =
        new (string Group, string Name, double Growth, double Margin, double Revenue)[]
        {
            ("Storage", "Object store", 18.2, 31.5, 142), ("Storage", "Block volumes", 6.4, 24.1, 96), ("Storage", "Backup", 11.8, 38.2, 64), ("Storage", "Archive", 3.1, 44.6, 38),
            ("Compute", "Virtual servers", 9.7, 18.4, 248), ("Compute", "Bare metal", 4.2, 12.9, 121), ("Compute", "Kubernetes", 26.4, 22.7, 187), ("Compute", "Functions", 34.8, 29.3, 52),
            ("Data", "Databases", 15.3, 35.8, 176), ("Data", "Streaming", 22.9, 27.4, 71), ("Data", "Warehouse", 19.6, 41.2, 133), ("Data", "Data lake", 12.1, 19.6, 58),
        }.Select(p => new CdsChartPoint(p.Group, p.Growth, p.Margin) { Key = p.Name, Size = p.Revenue }).ToList();

    public static IReadOnlyList<CdsChartPoint> Sprints { get; } =
        new (string Group, (double Score, double Size)[] Values)[]
        {
            ("Platform", [(62, 14), (71, 9), (55, 21)]),
            ("Mobile", [(48, 6), (66, 12), (40, 4)]),
            ("Data", [(77, 18), (59, 7), (69, 11)]),
        }.SelectMany(s => s.Values.Select((v, i) => new CdsChartPoint(s.Group, $"Sprint {i + 1}", v.Score) { Size = v.Size })).ToList();

    public static IReadOnlyList<CdsChartPoint> Empty { get; } = [];

    static PackageBSamples()
    {
        var spark = new Lcg(11);
        List<CdsChartPoint> Series(string group, double start, double drift, double noise) =>
            Enumerable.Range(0, 30).Select(i => new CdsChartPoint(group, new DateTime(2024, 9, 1).AddDays(i), JsRound(start + drift * i + (spark.Next() - 0.5) * noise))).ToList();
        Revenue = Series("Revenue", 420, 6, 90);
        Errors = Series("Errors", 80, -1.4, 40);
        SignUps = Series("Sign-ups", 1200, -4, 260);

        var rnd = new Lcg(7);
        var requests = new List<CdsChartPoint>();
        foreach (var (group, start, k) in new[] { ("API", 120.0, 0.9), ("Web", 60.0, 1.6), ("Batch", 300.0, 0.5) })
            for (var i = 0; i < 18; i++)
            {
                var size = JsRound(5 + rnd.Next() * 95);
                var spread = 2 + rnd.Next() * 2;
                var latency = JsRound(start + size * k * spread + rnd.Next() * 40);
                requests.Add(new CdsChartPoint(group, size, latency));
            }
        Requests = requests;
        var cpu = new List<CdsChartPoint>();
        string[] nodes = ["node-a", "node-b"];
        for (var g = 0; g < nodes.Length; g++)
            for (var i = 0; i < 28; i++)
                cpu.Add(new CdsChartPoint(nodes[g], new DateTime(2024, 6, 1 + i / 4, i % 4 * 6, 0, 0), JsRound(38 + g * 9 + rnd.Next() * 30 + (i % 9 == 4 ? 28 : 0))));
        CpuLoad = cpu;
        var deals = new List<CdsChartPoint>();
        foreach (var group in new[] { "Q1", "Q2" })
            foreach (var region in new[] { "North", "South", "East", "West", "Central" })
                for (var i = 0; i < 4; i++)
                    deals.Add(new CdsChartPoint(group, region, JsRound(20 + rnd.Next() * 80)));
        Deals = deals;
    }

    static double JsRound(double v) => Math.Floor(v + 0.5);

    static List<CdsChartPoint> Monthly(params (string Group, double[] Values)[] series) =>
        series.SelectMany(s => s.Values.Select((v, i) => new CdsChartPoint(s.Group, new DateTime(2024, 1 + i, 1), v))).ToList();

    static List<CdsChartPoint> Days(params (string Group, double[] Values)[] series)
    {
        string[] days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        return series.SelectMany(s => s.Values.Select((v, i) => new CdsChartPoint(s.Group, days[i], v))).ToList();
    }

    /// <summary>The previews' Park-Miller generator, so the gallery shows the same points.</summary>
    sealed class Lcg(long seed)
    {
        long _s = seed;

        public double Next()
        {
            _s = _s * 16807 % 2147483647;
            return _s / 2147483647.0;
        }
    }
}
