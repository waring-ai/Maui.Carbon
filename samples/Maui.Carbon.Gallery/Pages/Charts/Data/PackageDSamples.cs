namespace Maui.Carbon.Gallery.Pages.Charts.Data;

/// <summary>Sample data of the treemap, circle pack, tree, alluvial, word cloud and network diagram pages (their Carbon previews).</summary>
public static class PackageDSamples
{
    static CdsChartNode N(string name, params CdsChartNode[] children) => new(name, children);

    static CdsChartNode L(string name, double value, bool label = false) => new(name, value, label);

    // ----- treemap -----

    public static IReadOnlyList<CdsChartNode> Spend { get; } =
    [
        N("Compute", L("Virtual servers", 412, true), L("Kubernetes", 286, true), L("Functions", 64, true), L("Bare metal", 118, true)),
        N("Storage", L("Object storage", 204, true), L("Block storage", 132, true), L("File storage", 41)),
        N("Databases", L("PostgreSQL", 176, true), L("Redis", 58, true), L("MongoDB", 47)),
        N("Networking", L("Load balancers", 83, true), L("Transit gateway", 36), L("CDN", 29)),
        N("AI", L("Model inference", 154, true), L("Training", 97, true)),
    ];

    public static IReadOnlyList<CdsChartNode> SpendTopThree { get; } = Spend.Take(3).ToList();

    public static IReadOnlyList<CdsChartNode> Population { get; } =
    [
        N("Asia", L("India", 1428, true), L("China", 1425, true), L("Indonesia", 277, true), L("Pakistan", 240, true), L("Bangladesh", 173)),
        N("Africa", L("Nigeria", 224, true), L("Ethiopia", 127), L("Egypt", 113), L("DR Congo", 102)),
        N("Americas", L("United States", 340, true), L("Brazil", 216, true), L("Mexico", 128)),
        N("Europe", L("Russia", 144, true), L("Germany", 84), L("France", 65)),
        N("Oceania", L("Australia", 27), L("Papua New Guinea", 10)),
        N("Middle East", L("Iran", 89, true), L("Turkey", 85), L("Iraq", 45)),
    ];

    public static IReadOnlyList<CdsChartNode> NoNodes { get; } = [];

    // ----- circle pack -----

    public static IReadOnlyList<CdsChartNode> Revenue { get; } =
    [
        N("North America", L("United States", 3120), L("Canada", 640), L("Mexico", 410)),
        N("Europe", L("Germany", 980), L("United Kingdom", 870), L("France", 720), L("Netherlands", 310), L("Spain", 280)),
        N("Asia Pacific", L("Japan", 1150), L("Australia", 520), L("India", 470), L("Singapore", 260)),
        N("Latin America", L("Brazil", 540), L("Chile", 190), L("Colombia", 150)),
    ];

    public static IReadOnlyList<CdsChartNode> Org { get; } =
    [
        N("Engineering",
            N("Platform", L("Runtime", 18), L("Networking", 11), L("Storage", 14)),
            N("Product", L("Web", 22), L("Mobile", 15)),
            N("Data", L("Analytics", 9), L("ML", 12))),
        N("Sales",
            N("Enterprise", L("EMEA", 16), L("Americas", 21)),
            L("Commercial", 24)),
        N("Operations",
            N("Support", L("Tier 1", 19), L("Tier 2", 8)),
            L("Finance", 12), L("People", 9)),
    ];

    public static IReadOnlyList<CdsChartNode> StorageTiers { get; } =
    [
        N("Storage", L("Object", 204), L("Block", 132), L("File", 41), L("Archive", 76), L("Backup", 58)),
    ];

    // ----- tree -----

    public static IReadOnlyList<CdsChartNode> Catalog { get; } =
    [
        N("Platform",
            N("Compute", N("Virtual servers"), N("Kubernetes"), N("Functions")),
            N("Storage", N("Object"), N("Block")),
            N("Networking", N("Load balancers"), N("DNS"))),
        N("Data",
            N("Databases", N("PostgreSQL"), N("Redis")),
            N("Streaming", N("Event streams"))),
        N("AI", N("Inference"), N("Training", N("Fine-tuning"), N("Evaluation"))),
        N("Security", N("Identity"), N("Key management")),
    ];

    public static IReadOnlyList<CdsChartNode> CatalogSmall { get; } = Catalog.Skip(1).Take(2).ToList();

    // ----- alluvial -----

    public static IList<CdsAlluvialNode> LeadNodes { get; } =
    [
        new("Search", "Source"), new("Referral", "Source"), new("Social", "Source"), new("Email", "Source"),
        new("Trial", "Stage"), new("Demo", "Stage"), new("Contact sales", "Stage"),
        new("Won", "Outcome"), new("Lost", "Outcome"), new("Open", "Outcome"),
    ];

    public static IReadOnlyList<CdsAlluvialLink> Leads { get; } =
    [
        new("Search", "Trial", 420), new("Search", "Demo", 160), new("Search", "Contact sales", 70),
        new("Referral", "Demo", 190), new("Referral", "Contact sales", 110),
        new("Social", "Trial", 230), new("Social", "Demo", 40),
        new("Email", "Trial", 150), new("Email", "Contact sales", 60),
        new("Trial", "Won", 210), new("Trial", "Lost", 430), new("Trial", "Open", 160),
        new("Demo", "Won", 170), new("Demo", "Lost", 140), new("Demo", "Open", 80),
        new("Contact sales", "Won", 120), new("Contact sales", "Lost", 60), new("Contact sales", "Open", 60),
    ];

    public static IList<CdsAlluvialNode> EnergyNodes { get; } =
        [new("Solar"), new("Wind"), new("Gas"), new("Grid"), new("Homes"), new("Industry")];

    public static IReadOnlyList<CdsAlluvialLink> Energy { get; } =
    [
        new("Solar", "Grid", 34), new("Wind", "Grid", 52), new("Gas", "Grid", 61), new("Grid", "Homes", 68), new("Grid", "Industry", 79),
    ];

    /// <summary>Carbon's preview pins every node to a color for the gradient links.</summary>
    public static IDictionary<string, string> EnergyColors { get; } = new Dictionary<string, string>
    {
        ["Solar"] = CdsColorKeys.DataVis02, ["Wind"] = CdsColorKeys.DataVis11, ["Gas"] = CdsColorKeys.DataVis01,
        ["Grid"] = CdsColorKeys.TextHelper, ["Homes"] = CdsColorKeys.DataVis09, ["Industry"] = CdsColorKeys.DataVis05,
    };

    public static IReadOnlyList<CdsAlluvialLink> NoLinks { get; } = [];

    // ----- word cloud -----

    public static IReadOnlyList<CdsChartPoint> Tickets { get; } = Words(
        ("Account", [("login", 96), ("password", 81), ("MFA", 44), ("locked", 38), ("SSO", 27), ("profile", 14)]),
        ("Billing", [("invoice", 72), ("refund", 58), ("card", 41), ("upgrade", 35), ("tax", 19), ("receipt", 16)]),
        ("Performance", [("timeout", 88), ("slow", 64), ("latency", 47), ("outage", 33), ("500 error", 29), ("cache", 12)]),
        ("Features", [("export", 53), ("dark mode", 37), ("API", 49), ("webhook", 24), ("dashboard", 31), ("import", 18)]));

    public static IReadOnlyList<CdsChartPoint> Skills { get; } = Words(
        ("Skills", [("React", 62), ("TypeScript", 58), ("Python", 55), ("SQL", 49), ("Go", 31), ("Kubernetes", 36), ("Terraform", 22),
            ("GraphQL", 18), ("Rust", 14), ("Swift", 17), ("Kotlin", 15), ("C#", 26), ("Java", 33), ("Figma", 21), ("Accessibility", 24)]));

    static List<CdsChartPoint> Words(params (string Group, (string Word, double Value)[] Words)[] groups) =>
        groups.SelectMany(g => g.Words.Select(w => new CdsChartPoint(g.Group, w.Word, w.Value))).ToList();

    // ----- network diagram -----

    public static IReadOnlyList<CdsNetworkNode> PipelineNodes { get; } =
    [
        new("repo", 128, 60, "Repository", "main branch") { Color = CdsColorKeys.SupportInfo },
        new("build", 408, 60, "Build", "4 min 12 s"),
        new("test", 408, 180, "Test suite", "1,284 passed") { Label = "Stacked: 3 shards", IsStacked = true, Color = CdsColorKeys.SupportSuccess },
        new("stage", 688, 60, "Staging", "v2.14.0") { IsClickable = true },
        new("prod", 688, 180, "Production", "Awaiting approval") { Color = CdsColorKeys.SupportWarning },
    ];

    public static IList<CdsNetworkEdge> PipelineEdges { get; } =
    [
        new("repo", "build", CdsNetworkEdgePath.Elbow, CdsNetworkMarker.ArrowRight),
        new("build", "test", CdsNetworkEdgePath.Elbow, CdsNetworkMarker.ArrowRight),
        new("build", "stage", CdsNetworkEdgePath.Elbow, CdsNetworkMarker.ArrowRight),
        new("stage", "prod", CdsNetworkEdgePath.Elbow, CdsNetworkMarker.ArrowRight) { Variant = CdsNetworkEdgeVariant.DashMd, Label = "Manual approval" },
    ];

    static CdsNetworkNode Shape(string id, double x, double y, string title, CdsIconName icon, CdsNetworkNodeShape shape = CdsNetworkNodeShape.Circle, string? subtitle = null) =>
        new() { Id = id, X = x, Y = y, Title = title, Subtitle = subtitle, Icon = icon, Kind = CdsNetworkNodeKind.Shape, Shape = shape };

    public static IReadOnlyList<CdsNetworkNode> TopologyNodes { get; } =
    [
        Shape("user", 60, 120, "Users", CdsIconName.User),
        Shape("lb", 230, 120, "Load balancer", CdsIconName.ArrowsVertical, CdsNetworkNodeShape.RoundedSquare),
        Shape("vm1", 400, 50, "App server 1", CdsIconName.VirtualMachine, CdsNetworkNodeShape.Square, "10.0.1.4"),
        Shape("vm2", 400, 200, "App server 2", CdsIconName.VirtualMachine, CdsNetworkNodeShape.Square, "10.0.1.5"),
        Shape("db", 560, 120, "Database", CdsIconName.Catalog),
    ];

    public static IList<CdsNetworkEdge> TopologyEdges { get; } =
    [
        new("user", "lb", CdsNetworkEdgePath.Straight, CdsNetworkMarker.ArrowRight),
        new("lb", "vm1", CdsNetworkEdgePath.Bezier, CdsNetworkMarker.ArrowRight),
        new("lb", "vm2", CdsNetworkEdgePath.Bezier, CdsNetworkMarker.ArrowRight),
        new("vm1", "db") { MarkerStart = CdsNetworkMarker.Circle, MarkerEnd = CdsNetworkMarker.Diamond, Variant = CdsNetworkEdgeVariant.Tunnel, Label = "Primary connection" },
        new("vm2", "db") { MarkerStart = CdsNetworkMarker.Circle, MarkerEnd = CdsNetworkMarker.Diamond, Variant = CdsNetworkEdgeVariant.Double, Label = "Replica connection" },
    ];

    static IList<CdsNetworkEdge> Free(CdsNetworkEdgeVariant variant = CdsNetworkEdgeVariant.Default, CdsNetworkMarker marker = CdsNetworkMarker.None) =>
        [new() { Start = new Point(8, 12), End = new Point(marker == CdsNetworkMarker.None ? 220 : 118, 12), Variant = variant, MarkerEnd = marker }];

    public static IList<CdsNetworkEdge> EdgeDefault { get; } = Free();
    public static IList<CdsNetworkEdge> EdgeDashSm { get; } = Free(CdsNetworkEdgeVariant.DashSm);
    public static IList<CdsNetworkEdge> EdgeDashMd { get; } = Free(CdsNetworkEdgeVariant.DashMd);
    public static IList<CdsNetworkEdge> EdgeDashLg { get; } = Free(CdsNetworkEdgeVariant.DashLg);
    public static IList<CdsNetworkEdge> EdgeDashXl { get; } = Free(CdsNetworkEdgeVariant.DashXl);
    public static IList<CdsNetworkEdge> EdgeTunnel { get; } = Free(CdsNetworkEdgeVariant.Tunnel);
    public static IList<CdsNetworkEdge> EdgeDouble { get; } = Free(CdsNetworkEdgeVariant.Double);
    public static IList<CdsNetworkEdge> MarkerArrowRight { get; } = Free(marker: CdsNetworkMarker.ArrowRight);
    public static IList<CdsNetworkEdge> MarkerArrowLeft { get; } = Free(marker: CdsNetworkMarker.ArrowLeft);
    public static IList<CdsNetworkEdge> MarkerCircle { get; } = Free(marker: CdsNetworkMarker.Circle);
    public static IList<CdsNetworkEdge> MarkerDiamond { get; } = Free(marker: CdsNetworkMarker.Diamond);
    public static IList<CdsNetworkEdge> MarkerSquare { get; } = Free(marker: CdsNetworkMarker.Square);
    public static IList<CdsNetworkEdge> MarkerTee { get; } = Free(marker: CdsNetworkMarker.Tee);

    public static IReadOnlyList<CdsNetworkNode> NoNetworkNodes { get; } = [];
}
