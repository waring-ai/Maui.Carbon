namespace Maui.Carbon.Gallery.Pages.Patterns.Data;

/// <summary>A Kubernetes cluster row shared by the resource list and filtering patterns.</summary>
public sealed record K1Cluster(string Id, string Name, string StatusText, string Region, string Version, int Nodes, DateTime Created)
{
    /// <summary>The status indicator kind for <see cref="StatusText"/>.</summary>
    public CdsIconIndicatorKind Status => StatusText switch
    {
        "Running" => CdsIconIndicatorKind.Succeeded,
        "Updating" => CdsIconIndicatorKind.InProgress,
        "Warning" => CdsIconIndicatorKind.CautionMajor,
        "Failed" => CdsIconIndicatorKind.Failed,
        "Stopped" => CdsIconIndicatorKind.NotStarted,
        _ => CdsIconIndicatorKind.Pending,
    };

    /// <summary>The Kubernetes minor version, for example 1.31.</summary>
    public string Minor => string.Join('.', Version.Split('.').Take(2));
}

/// <summary>Sample clusters and column setup for the resource list, filtering and empty state patterns.</summary>
public static class K1Clusters
{
    static K1Cluster C(int i, string name, string status, string region, string version, int nodes, int month, int day) =>
        new($"c{i}", name, status, region, version, nodes, new DateTime(2026, month, day));

    /// <summary>The 23 clusters of the resource list; the first page shows ten.</summary>
    public static IReadOnlyList<K1Cluster> All { get; } =
    [
        C(0, "prod-eu-01", "Running", "Frankfurt", "1.31.2", 12, 11, 3),
        C(1, "prod-us-02", "Running", "Dallas", "1.31.2", 18, 10, 28),
        C(2, "staging-eu-01", "Updating", "Frankfurt", "1.30.6", 4, 10, 21),
        C(3, "payments-ap-01", "Warning", "Tokyo", "1.30.6", 6, 10, 14),
        C(4, "analytics-us-01", "Failed", "Washington DC", "1.29.10", 8, 9, 30),
        C(5, "dev-sandbox-03", "Stopped", "London", "1.31.2", 1, 9, 22),
        C(6, "ml-training-01", "Running", "Dallas", "1.31.2", 24, 9, 9),
        C(7, "edge-cache-02", "Provisioning", "London", "1.31.2", 3, 11, 16),
        C(8, "prod-eu-02", "Running", "Frankfurt", "1.31.2", 12, 8, 27),
        C(9, "batch-jobs-us-01", "Running", "Washington DC", "1.30.6", 10, 8, 12),
        C(10, "search-eu-01", "Running", "Frankfurt", "1.30.6", 6, 8, 4),
        C(11, "qa-us-01", "Stopped", "Dallas", "1.29.10", 2, 7, 29),
        C(12, "ingest-ap-02", "Running", "Tokyo", "1.31.2", 9, 7, 18),
        C(13, "reports-eu-03", "Updating", "London", "1.31.2", 4, 7, 7),
        C(14, "billing-us-01", "Running", "Dallas", "1.31.2", 5, 6, 26),
        C(15, "iot-gateway-ap-01", "Running", "Tokyo", "1.30.6", 7, 6, 15),
        C(16, "marketing-eu-01", "Running", "Frankfurt", "1.31.2", 3, 6, 2),
        C(17, "data-lake-us-02", "Warning", "Washington DC", "1.30.6", 16, 5, 20),
        C(18, "ci-runners-eu-01", "Running", "London", "1.31.2", 9, 5, 8),
        C(19, "staging-us-01", "Stopped", "Washington DC", "1.29.10", 3, 4, 24),
        C(20, "auth-eu-02", "Running", "Frankfurt", "1.31.2", 4, 4, 9),
        C(21, "media-ap-01", "Running", "Tokyo", "1.30.6", 11, 3, 18),
        C(22, "legacy-us-03", "Failed", "Dallas", "1.29.10", 2, 2, 5),
    ];

    /// <summary>The 14 clusters of the filtering pattern.</summary>
    public static IReadOnlyList<K1Cluster> ForFiltering { get; } =
    [
        C(0, "prod-eu-01", "Running", "Frankfurt", "1.31.2", 12, 11, 3),
        C(1, "prod-us-02", "Running", "Dallas", "1.31.2", 18, 10, 28),
        C(2, "staging-eu-01", "Updating", "Frankfurt", "1.30.6", 4, 10, 21),
        C(3, "payments-ap-01", "Warning", "Tokyo", "1.30.6", 6, 10, 14),
        C(4, "analytics-us-01", "Failed", "Washington DC", "1.29.10", 8, 9, 30),
        C(5, "dev-sandbox-03", "Stopped", "London", "1.31.2", 1, 9, 22),
        C(6, "ml-training-01", "Running", "Dallas", "1.31.2", 24, 9, 9),
        C(7, "edge-cache-02", "Running", "London", "1.31.2", 3, 9, 2),
        C(8, "prod-eu-02", "Running", "Frankfurt", "1.31.2", 12, 8, 27),
        C(9, "batch-jobs-us-01", "Running", "Washington DC", "1.30.6", 10, 8, 12),
        C(10, "search-eu-01", "Running", "Frankfurt", "1.30.6", 6, 8, 4),
        C(11, "qa-us-01", "Stopped", "Dallas", "1.29.10", 2, 7, 29),
        C(12, "ingest-ap-02", "Running", "Tokyo", "1.31.2", 9, 7, 18),
        C(13, "reports-eu-03", "Updating", "London", "1.31.2", 4, 7, 7),
    ];

    /// <summary>
    /// Adds the cluster columns to <paramref name="table"/> and binds them: a name link, a status indicator, region,
    /// version, right-aligned worker nodes and (optionally) the created date.
    /// </summary>
    public static void AddColumns(CdsDataTable table, bool created = true)
    {
        table.RowKeySelector = r => ((K1Cluster)r).Id;
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", Width = GridLength.Star, ValueSelector = r => ((K1Cluster)r).Name, CellTemplate = NameTemplate() });
        table.Headers.Add(new CdsDataTableHeader { Key = "status", Header = "Status", Width = new GridLength(140), ValueSelector = r => ((K1Cluster)r).StatusText, CellTemplate = StatusTemplate() });
        table.Headers.Add(new CdsDataTableHeader { Key = "region", Header = "Region", Width = new GridLength(140), ValueSelector = r => ((K1Cluster)r).Region });
        table.Headers.Add(new CdsDataTableHeader { Key = "version", Header = "Version", Width = new GridLength(100), ValueSelector = r => ((K1Cluster)r).Version });
        table.Headers.Add(new CdsDataTableHeader { Key = "nodes", Header = "Worker nodes", Width = new GridLength(130), Align = CdsTableAlignment.End, ValueSelector = r => ((K1Cluster)r).Nodes });
        if (created)
            table.Headers.Add(new CdsDataTableHeader { Key = "created", Header = "Created", Width = GridLength.Star, ValueSelector = r => ((K1Cluster)r).Created, StringFormat = "MMM d, yyyy" });
    }

    /// <summary>A status indicator (icon plus word, never color alone).</summary>
    public static DataTemplate StatusTemplate() => new(() =>
    {
        var indicator = new CdsIconIndicator { Size = 16 };
        indicator.SetBinding(CdsIconIndicator.KindProperty, static (K1Cluster c) => c.Status, mode: BindingMode.OneTime);
        indicator.SetBinding(CdsIconIndicator.LabelProperty, static (K1Cluster c) => c.StatusText, mode: BindingMode.OneTime);
        return indicator;
    });

    /// <summary>The name as a link to the detail page.</summary>
    public static DataTemplate NameTemplate() => new(() =>
    {
        var link = new CdsLink { VerticalOptions = LayoutOptions.Center };
        link.SetBinding(CdsLink.TextProperty, static (K1Cluster c) => c.Name, mode: BindingMode.OneTime);
        return link;
    });
}
