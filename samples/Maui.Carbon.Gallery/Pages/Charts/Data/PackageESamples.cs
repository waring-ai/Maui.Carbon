using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace Maui.Carbon.Gallery.Pages.Charts.Data;

/// <summary>Sample data of the heat map, map and parallel coordinates pages (the Carbon preview examples).</summary>
public static class PackageESamples
{
    // ----- heat map -----

    static readonly string[] Days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    /// <summary>Weekdays bottom to top, so Monday is the top row.</summary>
    public static IList<string> WeekdaysBottomUp { get; } = Days.Reverse().ToList();

    public static IReadOnlyList<CdsChartPoint> Sessions { get; } = Days.SelectMany((day, di) => Enumerable.Range(0, 24).Select(hr =>
    {
        var work = di < 5 ? 1 : 0.55;
        var curve = Math.Exp(-Math.Pow((hr - 13.5) / 4.2, 2)) * 9 + Math.Exp(-Math.Pow((hr - 20.5) / 2.2, 2)) * 4 * (di < 5 ? 0.7 : 1.2) + 0.8;
        return new CdsChartPoint(day, hr.ToString("00"), Math.Round(curve * work * 10) / 10);
    })).ToList();

    static readonly string[] Months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public static IReadOnlyList<CdsChartPoint> Anomalies { get; } = Enumerable.Range(0, 10).SelectMany(yi => Months.Select((m, mi) =>
    {
        var v = -0.9 + yi * 0.21 + Math.Sin(mi * 0.9 + yi * 1.7) * 0.55 + Math.Cos(mi * 2.3 + yi) * 0.3;
        return new CdsChartPoint((2015 + yi).ToString(), m, Math.Round(v * 10) / 10);
    })).ToList();

    public static IReadOnlyList<CdsChartPoint> Incidents { get; } = new (string Service, double?[] Counts)[]
    {
        ("Auth", [1, 4, 9, 14]), ("Billing", [null, 2, 6, 11]), ("Catalog", [0, 1, 5, null]),
        ("Checkout", [2, 6, 12, 18]), ("Search", [null, 3, 8, 10]), ("Shipping", [1, null, 7, 13]),
    }.SelectMany(s => s.Counts.Select((v, i) => (s.Service, Severity: "Sev " + (i + 1), Value: v)))
        .Where(x => x.Value.HasValue).Select(x => new CdsChartPoint(x.Service, x.Severity, x.Value)).ToList();

    public static IList<string> Severities { get; } = ["Sev 1", "Sev 2", "Sev 3", "Sev 4"];

    public static IReadOnlyList<CdsChartPoint> Satisfaction { get; } =
        new[] { "North America", "Latin America", "Europe", "Middle East", "Africa", "Asia Pacific" }.SelectMany((r, ri) =>
            new[] { "Q1 2024", "Q2 2024", "Q3 2024", "Q4 2024", "Q1 2025", "Q2 2025" }.Select((q, qi) =>
                new CdsChartPoint(r, q, Math.Round(62 + ri * 3.1 + qi * 2.2 + Math.Sin(ri * 1.3 + qi) * 5)))).ToList();

    // ----- choropleth -----

    static List<CdsChartPoint> Regions(params (string Name, double Value)[] rows) => rows.Select(r => new CdsChartPoint { Key = r.Name, Value = r.Value }).ToList();

    public static IReadOnlyList<CdsChartPoint> InternetUsers { get; } = Regions(
        ("United States of America", 92), ("Canada", 93), ("Mexico", 76), ("Guatemala", 56), ("Honduras", 48), ("Nicaragua", 57), ("Costa Rica", 83), ("Panama", 68),
        ("Cuba", 71), ("Dominican Rep.", 85), ("Haiti", 39), ("Brazil", 81), ("Argentina", 88), ("Chile", 90), ("Colombia", 73), ("Peru", 75), ("Venezuela", 62),
        ("Ecuador", 72), ("Bolivia", 66), ("Paraguay", 77), ("Uruguay", 90), ("United Kingdom", 97), ("Ireland", 95), ("France", 85), ("Germany", 92), ("Spain", 95),
        ("Portugal", 85), ("Italy", 85), ("Netherlands", 93), ("Belgium", 94), ("Switzerland", 96), ("Austria", 93), ("Poland", 87), ("Czechia", 85), ("Sweden", 95),
        ("Norway", 99), ("Finland", 93), ("Denmark", 98), ("Iceland", 99), ("Greece", 83), ("Turkey", 83), ("Ukraine", 79), ("Romania", 89), ("Hungary", 89),
        ("Bulgaria", 79), ("Serbia", 83), ("Belarus", 89), ("Russia", 90), ("Kazakhstan", 90), ("China", 76), ("India", 46), ("Pakistan", 33), ("Bangladesh", 39),
        ("Indonesia", 66), ("Philippines", 73), ("Vietnam", 79), ("Thailand", 88), ("Malaysia", 97), ("Japan", 84), ("South Korea", 97), ("Mongolia", 83),
        ("Myanmar", 44), ("Nepal", 51), ("Sri Lanka", 50), ("Afghanistan", 18), ("Iran", 79), ("Iraq", 79), ("Saudi Arabia", 100), ("United Arab Emirates", 100),
        ("Israel", 90), ("Egypt", 72), ("Morocco", 88), ("Algeria", 71), ("Tunisia", 79), ("Libya", 88), ("Nigeria", 55), ("Ethiopia", 19), ("Kenya", 40),
        ("Tanzania", 32), ("Uganda", 10), ("South Africa", 72), ("Dem. Rep. Congo", 23), ("Angola", 39), ("Mozambique", 20), ("Madagascar", 20), ("Niger", 17),
        ("Chad", 12), ("Sudan", 29), ("Mali", 34), ("Ghana", 69), ("Cameroon", 45), ("Zambia", 31), ("Zimbabwe", 33), ("Namibia", 62), ("Botswana", 77),
        ("Somalia", 2), ("Australia", 96), ("New Zealand", 96), ("Papua New Guinea", 32));

    public static IReadOnlyList<CdsChartPoint> PopulationChange { get; } = Regions(
        ("Germany", 4.5), ("France", 3.6), ("United Kingdom", 5.8), ("Ireland", 15), ("Spain", 2.4), ("Portugal", -1.2), ("Italy", -2.8), ("Greece", -5.6),
        ("Poland", -1.5), ("Romania", -5), ("Bulgaria", -11), ("Hungary", -2.2), ("Czechia", 3), ("Slovakia", 0.4), ("Austria", 8.5), ("Switzerland", 10.5),
        ("Netherlands", 6.5), ("Belgium", 6.3), ("Luxembourg", 22), ("Denmark", 6), ("Sweden", 10.5), ("Norway", 8.9), ("Finland", 2.2), ("Iceland", 19),
        ("Estonia", 3.1), ("Latvia", -7.5), ("Lithuania", -3.8), ("Croatia", -9.5), ("Slovenia", 3), ("Serbia", -7), ("Bosnia and Herz.", -8), ("Albania", -3.5),
        ("Macedonia", -10), ("Montenegro", -0.5), ("Belarus", -2.5), ("Cyprus", 6.5));

    public static IReadOnlyList<CdsChartPoint> ForestArea { get; } = Regions(
        ("Brazil", 59), ("Peru", 57), ("Colombia", 53), ("Venezuela", 52), ("Bolivia", 46), ("Ecuador", 50), ("Guyana", 94), ("Suriname", 97), ("Paraguay", 40),
        ("Argentina", 10), ("Chile", 24), ("Uruguay", 12));

    public static IReadOnlyList<CdsChartPoint> ElectricityAccess { get; } = Regions(
        ("Egypt", 100), ("Algeria", 100), ("Morocco", 100), ("Tunisia", 100), ("Libya", 69), ("South Africa", 89), ("Ghana", 86), ("Kenya", 77), ("Nigeria", 59),
        ("Ethiopia", 55), ("Tanzania", 43), ("Uganda", 45), ("Dem. Rep. Congo", 21), ("Congo", 50), ("Angola", 48), ("Zambia", 47), ("Zimbabwe", 49), ("Mozambique", 32),
        ("Madagascar", 35), ("Malawi", 14), ("Niger", 19), ("Chad", 11), ("Mali", 53), ("Burkina Faso", 19), ("Sudan", 62), ("S. Sudan", 7), ("Somalia", 49),
        ("Cameroon", 65), ("Gabon", 92), ("Senegal", 70), ("Mauritania", 48), ("Namibia", 56), ("Botswana", 75), ("Côte d'Ivoire", 71), ("Guinea", 47),
        ("Sierra Leone", 29), ("Liberia", 32), ("Togo", 57), ("Benin", 42), ("Central African Rep.", 16), ("Rwanda", 49), ("Burundi", 10), ("Eritrea", 54),
        ("Lesotho", 50), ("eSwatini", 82), ("Gambia", 64), ("Guinea-Bissau", 36), ("Eq. Guinea", 67), ("Djibouti", 65));

    public static CdsGeoBounds Europe { get; } = new(-24, 35, 42, 70);

    // ----- proportional symbol map -----

    public static IReadOnlyList<CdsGeoPoint> UrbanAreas { get; } = new (string Name, string Group, double Lon, double Lat, double Value)[]
    {
        ("Tokyo", "Asia", 139.69, 35.69, 37.2), ("Delhi", "Asia", 77.21, 28.61, 32.9), ("Shanghai", "Asia", 121.47, 31.23, 29.2), ("Dhaka", "Asia", 90.41, 23.81, 23.2),
        ("São Paulo", "Americas", -46.63, -23.55, 22.6), ("Mexico City", "Americas", -99.13, 19.43, 22.3), ("Cairo", "Africa", 31.24, 30.04, 22.2),
        ("Beijing", "Asia", 116.41, 39.9, 21.8), ("Mumbai", "Asia", 72.88, 19.08, 21.3), ("Osaka", "Asia", 135.5, 34.69, 19), ("New York", "Americas", -74, 40.71, 18.9),
        ("Karachi", "Asia", 67.01, 24.86, 17.2), ("Kinshasa", "Africa", 15.27, -4.44, 16.3), ("Lagos", "Africa", 3.38, 6.52, 15.9), ("Istanbul", "Europe", 28.98, 41.01, 15.8),
        ("Buenos Aires", "Americas", -58.38, -34.6, 15.5), ("Kolkata", "Asia", 88.36, 22.57, 15.3), ("Manila", "Asia", 120.98, 14.6, 14.7), ("Lahore", "Asia", 74.36, 31.52, 13.9),
        ("Rio de Janeiro", "Americas", -43.17, -22.91, 13.7), ("Moscow", "Europe", 37.62, 55.76, 12.7), ("Los Angeles", "Americas", -118.24, 34.05, 12.5),
        ("Bogotá", "Americas", -74.07, 4.71, 11.5), ("Paris", "Europe", 2.35, 48.86, 11.2), ("Jakarta", "Asia", 106.85, -6.21, 11.2), ("Lima", "Americas", -77.04, -12.05, 11.2),
        ("London", "Europe", -0.13, 51.51, 9.6), ("Johannesburg", "Africa", 28.05, -26.2, 6.4), ("Sydney", "Oceania", 151.21, -33.87, 5.1), ("Melbourne", "Oceania", 144.96, -37.81, 5.0),
    }.Select(c => new CdsGeoPoint(c.Group, c.Name, c.Lon, c.Lat, c.Value)).ToList();

    public static IReadOnlyList<CdsGeoPoint> DataCenterCapacity { get; } = new (string Name, double Lon, double Lat, double Value)[]
    {
        ("London", -0.13, 51.51, 1100), ("Dublin", -6.26, 53.35, 900), ("Frankfurt", 8.68, 50.11, 800), ("Amsterdam", 4.9, 52.37, 650), ("Paris", 2.35, 48.86, 500),
        ("Madrid", -3.7, 40.42, 200), ("Milan", 9.19, 45.46, 180), ("Berlin", 13.4, 52.52, 160), ("Stockholm", 18.07, 59.33, 150), ("Zurich", 8.54, 47.38, 140),
        ("Warsaw", 21.01, 52.23, 120), ("Copenhagen", 12.57, 55.68, 110), ("Helsinki", 24.94, 60.17, 100), ("Oslo", 10.75, 59.91, 90), ("Brussels", 4.35, 50.85, 90),
        ("Munich", 11.58, 48.14, 90), ("Vienna", 16.37, 48.21, 80), ("Marseille", 5.37, 43.3, 70), ("Lisbon", -9.14, 38.72, 60),
    }.Select(c => new CdsGeoPoint("Capacity", c.Name, c.Lon, c.Lat, c.Value)).ToList();

    public static CdsGeoBounds WesternEurope { get; } = new(-11, 36, 30, 62);

    // ----- connection map -----

    public static IReadOnlyList<CdsGeoConnection> HeathrowRoutes { get; } = new (string Target, string Group, double Lon, double Lat, double Value)[]
    {
        ("New York", "Americas", -73.78, 40.64, 98), ("Toronto", "Americas", -79.62, 43.68, 35), ("Chicago", "Americas", -87.9, 41.98, 35),
        ("Los Angeles", "Americas", -118.41, 33.94, 28), ("Seattle", "Americas", -122.31, 47.45, 14), ("Mexico City", "Americas", -99.07, 19.44, 7),
        ("São Paulo", "Americas", -46.47, -23.43, 7), ("Dubai", "Middle East", 55.36, 25.25, 70), ("Doha", "Middle East", 51.61, 25.27, 56),
        ("Delhi", "Asia", 77.1, 28.56, 35), ("Mumbai", "Asia", 72.87, 19.09, 21), ("Hong Kong", "Asia", 113.92, 22.31, 35), ("Singapore", "Asia", 103.99, 1.36, 28),
        ("Tokyo", "Asia", 139.78, 35.55, 21), ("Bangkok", "Asia", 100.75, 13.69, 14), ("Beijing", "Asia", 116.6, 40.08, 14), ("Johannesburg", "Africa", 28.24, -26.13, 14),
        ("Lagos", "Africa", 3.32, 6.58, 14), ("Nairobi", "Africa", 36.93, -1.32, 7), ("Cape Town", "Africa", 18.6, -33.97, 7),
    }.Select(r => new CdsGeoConnection(r.Group, "London", r.Target, -0.45, 51.47, r.Lon, r.Lat, r.Value)).ToList();

    public static IReadOnlyList<CdsGeoPoint> DataCenters { get; } = new (string Name, double Lon, double Lat)[]
    {
        ("Dublin", -6.26, 53.35), ("London", -0.13, 51.51), ("Paris", 2.35, 48.86), ("Frankfurt", 8.68, 50.11), ("Amsterdam", 4.9, 52.37),
        ("Stockholm", 18.07, 59.33), ("Madrid", -3.7, 40.42), ("Milan", 9.19, 45.46), ("Warsaw", 21.01, 52.23),
    }.Select(n => new CdsGeoPoint(n.Name, n.Lon, n.Lat)).ToList();

    public static IReadOnlyList<CdsGeoConnection> ReplicationLinks { get; } = new (string Group, string Source, string Target, double Value)[]
    {
        ("Primary", "Frankfurt", "Amsterdam", 400), ("Primary", "Frankfurt", "Paris", 400), ("Primary", "London", "Amsterdam", 300), ("Primary", "London", "Dublin", 200),
        ("Primary", "Paris", "Madrid", 100), ("Primary", "Frankfurt", "Milan", 100), ("Primary", "Frankfurt", "Warsaw", 100), ("Primary", "Amsterdam", "Stockholm", 100),
        ("Backup", "Dublin", "Frankfurt", 100), ("Backup", "Madrid", "Milan", 40), ("Backup", "Stockholm", "Warsaw", 40), ("Backup", "London", "Paris", 100),
    }.Select(l => new CdsGeoConnection(l.Group, l.Source, l.Target, l.Value)).ToList();

    public static CdsGeoBounds DataCenterRegion { get; } = new(-11, 38, 26, 61);

    // ----- parallel coordinates -----

    public static IReadOnlyList<CdsParallelRecord> Cars { get; } = new (string Name, string Group, double? Mpg, int Cylinders, int Hp, int Weight, double Accel)[]
    {
        ("Civic", "Japan", 35, 4, 158, 1300, 8.2), ("Corolla", "Japan", 33, 4, 169, 1390, 9.1), ("Camry", "Japan", 28, 4, 203, 1590, 8.1),
        ("Accord", "Japan", 30, 4, 192, 1480, 7.6), ("RAV4", "Japan", 27, 4, 203, 1640, 8.0), ("CX-5", "Japan", 26, 4, 187, 1610, 8.6),
        ("Outback", "Japan", 26, 4, 182, 1720, 8.9), ("Golf", "Europe", 34, 4, 148, 1340, 8.5), ("Passat", "Europe", 29, 4, 174, 1520, 8.1),
        ("3 Series", "Europe", 28, 4, 255, 1570, 5.6), ("A4", "Europe", 29, 4, 201, 1600, 6.2), ("C-Class", "Europe", 27, 4, 255, 1690, 6.0),
        ("XC60", "Europe", 25, 4, 247, 1900, 6.4), ("911 Carrera", "Europe", 20, 6, 379, 1500, 4.0), ("Mustang GT", "USA", 18, 8, 480, 1750, 4.2),
        ("Camaro SS", "USA", 19, 8, 455, 1720, 4.0), ("F-150", "USA", 20, 6, 400, 2100, 6.1), ("Silverado", "USA", 17, 8, 355, 2250, 6.6),
        ("Explorer", "USA", 21, 4, 300, 2000, 6.3), ("Wrangler", "USA", 19, 6, 285, 2000, 7.0), ("Malibu", "USA", 29, 4, 160, 1460, 8.6),
        ("Charger", "USA", 23, 6, 292, 1800, 6.2), ("Tahoe", "USA", 17, 8, 355, 2550, 7.2), ("Model 3", "USA", null, 0, 283, 1760, 5.8),
    }.Select(c => new CdsParallelRecord(c.Name, c.Group, ("mpg", c.Mpg), ("cylinders", c.Cylinders), ("horsepower", c.Hp), ("weight", c.Weight), ("accel", c.Accel))).ToList();

    public static IReadOnlyList<CdsParallelRecord> Services { get; } = new (string Name, string Group, int Latency, double Errors, int Throughput, int Cpu, int Cost)[]
    {
        ("Auth", "Tier 1", 95, 0.2, 2400, 62, 3100), ("Checkout", "Tier 1", 140, 0.4, 1300, 71, 4200), ("Payments", "Tier 1", 160, 0.3, 900, 58, 3800),
        ("Search", "Tier 1", 120, 0.6, 2100, 78, 4600), ("Catalog", "Tier 2", 110, 0.5, 1700, 44, 1900), ("Pricing", "Tier 2", 175, 0.9, 1100, 52, 1600),
        ("Inventory", "Tier 2", 210, 1.1, 800, 66, 2300), ("Profile", "Tier 2", 85, 0.3, 1500, 35, 1200), ("Shipping", "Tier 3", 230, 1.4, 450, 41, 900),
        ("Reviews", "Tier 3", 190, 0.8, 600, 29, 700), ("Notifications", "Tier 3", 260, 1.9, 1200, 48, 1100), ("Recommendations", "Tier 3", 245, 0.7, 950, 83, 2900),
    }.Select(s => new CdsParallelRecord(s.Name, s.Group, ("latency", s.Latency), ("errors", s.Errors), ("throughput", s.Throughput), ("cpu", s.Cpu), ("cost", s.Cost))).ToList();

    public static IReadOnlyList<CdsParallelRecord> NoRecords { get; } = [];
}

/// <summary>
/// The world map the map pages share: Natural Earth 1:110m countries from the app package, read once, without Antarctica
/// (as the Carbon previews do). Charts bind <see cref="Countries"/> and <see cref="IsLoading"/>.
/// </summary>
public sealed class MapAssets : INotifyPropertyChanged
{
    Task? _load;

    public static MapAssets Shared { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The countries as TopoJSON text, or null until read.</summary>
    public string? Countries { get; private set; }

    public bool IsLoading => Countries is null;

    public Task LoadAsync() => _load ??= LoadCoreAsync();

    async Task LoadCoreAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("Maps/countries-110m.json");
            using var reader = new StreamReader(stream);
            var text = await reader.ReadToEndAsync();
            Countries = await Task.Run(() => WithoutAntarctica(text));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Could not read the world map: {ex.Message}");
            Countries = "";
        }
        OnPropertyChanged(nameof(Countries));
        OnPropertyChanged(nameof(IsLoading));
    }

    static string WithoutAntarctica(string text)
    {
        if (JsonNode.Parse(text) is not JsonObject root || root["objects"]?["countries"]?["geometries"] is not JsonArray geometries) return text;
        for (var i = geometries.Count - 1; i >= 0; i--)
            if ((string?)geometries[i]?["properties"]?["name"] == "Antarctica") geometries.RemoveAt(i);
        return root.ToJsonString();
    }

    void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
