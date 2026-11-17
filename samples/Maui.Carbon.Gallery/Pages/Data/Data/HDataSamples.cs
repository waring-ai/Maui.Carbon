namespace Maui.Carbon.Gallery.Pages.Data.Data;

/// <summary>A Kubernetes cluster row of the data table samples.</summary>
public sealed record HCluster(string Id, string Name, string Region, CdsIconIndicatorKind Status, string StatusText, int Nodes, string Version, string Created)
{
    /// <summary>The text of an expanded row.</summary>
    public string Details => $"Kubernetes {Version} in {Region}. Created {Created}. {Nodes} worker nodes.";
}

/// <summary>Sample data and column setup shared by the data table, pagination and skeleton pages.</summary>
public static class HDataSamples
{
    static readonly HCluster[] s_clusters =
    [
        new("c01", "prod-eu-de-1", "eu-de", CdsIconIndicatorKind.Succeeded, "Running", 12, "1.29.4", "Mar 4, 2026"),
        new("c02", "prod-us-east-1", "us-east", CdsIconIndicatorKind.Succeeded, "Running", 24, "1.29.4", "Feb 11, 2026"),
        new("c03", "staging-eu-gb", "eu-gb", CdsIconIndicatorKind.InProgress, "Updating", 6, "1.30.1", "Jan 27, 2026"),
        new("c04", "dev-sandbox", "us-south", CdsIconIndicatorKind.NotStarted, "Stopped", 2, "1.28.9", "Jan 9, 2026"),
        new("c05", "analytics-jp-tok", "jp-tok", CdsIconIndicatorKind.CautionMinor, "Degraded", 18, "1.29.2", "Dec 15, 2025"),
        new("c06", "edge-au-syd", "au-syd", CdsIconIndicatorKind.Failed, "Failed", 4, "1.28.9", "Nov 30, 2025"),
        new("c07", "ci-runners", "eu-de", CdsIconIndicatorKind.Succeeded, "Running", 9, "1.30.1", "Nov 2, 2025"),
        new("c08", "ml-training", "us-east", CdsIconIndicatorKind.Pending, "Pending", 32, "1.30.1", "Oct 19, 2025"),
        new("c09", "billing-api", "eu-gb", CdsIconIndicatorKind.Succeeded, "Running", 5, "1.29.4", "Sep 8, 2025"),
        new("c10", "data-lake-eu", "eu-de", CdsIconIndicatorKind.Succeeded, "Running", 15, "1.29.2", "Aug 21, 2025"),
        new("c11", "qa-regression", "us-south", CdsIconIndicatorKind.NotStarted, "Stopped", 3, "1.28.9", "Aug 1, 2025"),
        new("c12", "search-index", "jp-tok", CdsIconIndicatorKind.CautionMajor, "Needs attention", 10, "1.29.2", "Jul 14, 2025"),
        new("c13", "marketing-site", "au-syd", CdsIconIndicatorKind.Succeeded, "Running", 3, "1.30.1", "Jun 30, 2025"),
        new("c14", "iot-gateway", "eu-gb", CdsIconIndicatorKind.Succeeded, "Running", 7, "1.29.4", "Jun 2, 2025"),
    ];

    /// <summary>The first <paramref name="count"/> sample clusters.</summary>
    public static IReadOnlyList<HCluster> Clusters(int count = 8) => s_clusters.Take(Math.Clamp(count, 0, s_clusters.Length)).ToList();

    /// <summary>All sample clusters.</summary>
    public static IReadOnlyList<HCluster> AllClusters => s_clusters;

    /// <summary>
    /// Connects the XAML-declared columns (<c>name</c>, <c>region</c>, <c>status</c>, <c>nodes</c>, <c>version</c>) to
    /// <see cref="HCluster"/>: value selectors, the status indicator template and the row key.
    /// </summary>
    public static void Bind(CdsDataTable table)
    {
        table.RowKeySelector = r => ((HCluster)r).Id;
        foreach (var header in table.Headers)
        {
            switch (header.Key)
            {
                case "name": header.ValueSelector = r => ((HCluster)r).Name; break;
                case "region": header.ValueSelector = r => ((HCluster)r).Region; break;
                case "status":
                    header.ValueSelector = r => ((HCluster)r).StatusText;
                    header.CellTemplate = StatusTemplate();
                    break;
                case "nodes": header.ValueSelector = r => ((HCluster)r).Nodes; break;
                case "version": header.ValueSelector = r => ((HCluster)r).Version; break;
            }
        }
    }

    /// <summary>Adds the cluster columns (name, region, status, nodes) in code and binds them.</summary>
    public static void AddClusterColumns(CdsDataTable table, bool region = true)
    {
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", Width = new GridLength(200) });
        if (region) table.Headers.Add(new CdsDataTableHeader { Key = "region", Header = "Region", Width = new GridLength(120) });
        table.Headers.Add(new CdsDataTableHeader { Key = "status", Header = "Status", Width = new GridLength(140) });
        table.Headers.Add(new CdsDataTableHeader { Key = "nodes", Header = "Nodes", Width = new GridLength(90), Align = CdsTableAlignment.End });
        Bind(table);
    }

    /// <summary>The cell template of the status column: a status indicator with its label.</summary>
    public static DataTemplate StatusTemplate() => new(() =>
    {
        var indicator = new CdsIconIndicator { Size = 16 };
        indicator.SetBinding(CdsIconIndicator.KindProperty, static (HCluster c) => c.Status, mode: BindingMode.OneTime);
        indicator.SetBinding(CdsIconIndicator.LabelProperty, static (HCluster c) => c.StatusText, mode: BindingMode.OneTime);
        return indicator;
    });

    /// <summary>The template of an expanded row.</summary>
    public static DataTemplate ExpandedTemplate() => new(() =>
    {
        var text = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, Tone = CdsTextTone.Secondary };
        text.SetBinding(Label.TextProperty, static (HCluster c) => c.Details, mode: BindingMode.OneTime);
        return text;
    });

    /// <summary>Adds the usual row menu: view details, restart and a destructive delete.</summary>
    public static void AddRowActions(CdsDataTable table)
    {
        table.RowActions.Add(new CdsDataTableRowAction { Label = "View details" });
        table.RowActions.Add(new CdsDataTableRowAction { Label = "Restart" });
        table.RowActions.Add(new CdsDataTableRowAction { Label = "Delete", Kind = CdsMenuItemKind.Danger });
    }

    /// <summary>A compact table (name, status, nodes) for the theme strips.</summary>
    public static CdsDataTable CompactTable(bool selectFirst = true)
    {
        var table = new CdsDataTable
        {
            Size = CdsSize.Md,
            Selection = CdsDataTableSelection.Multi,
            IsSortable = true,
            SortKey = "name",
            SortDirection = CdsSortDirection.Ascending,
            ItemsSource = Clusters(4),
            Title = "Clusters",
        };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", Width = GridLength.Star });
        table.Headers.Add(new CdsDataTableHeader { Key = "status", Header = "Status", Width = new GridLength(120) });
        table.Headers.Add(new CdsDataTableHeader { Key = "nodes", Header = "Nodes", Width = new GridLength(70), Align = CdsTableAlignment.End });
        Bind(table);
        table.Headers[1].IsSortable = false;
        table.UseZebraStyles = true;
        table.BatchActions.Add(new CdsDataTableBatchAction { Label = "Restart", Icon = CdsIconName.Restart });
        if (selectFirst) table.SelectedRows = new List<object> { s_clusters[1] };
        return table;
    }
}
