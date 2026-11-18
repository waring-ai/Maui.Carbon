using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Data.Data;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Data table", GalleryCategory.Data, Order = 50, Spec = "DataTable",
    Summary = "Shows structured rows and columns that people scan, sort, search, select and act on.")]
public partial class DataTablePage : GalleryPage
{
    public DataTablePage()
    {
        InitializeComponent();

        // Resource table.
        HDataSamples.Bind(MainTable);
        HDataSamples.AddRowActions(MainTable);
        MainTable.ItemsSource = HDataSamples.Clusters(8);

        // Batch bar: two rows selected.
        HDataSamples.AddClusterColumns(BatchTable);
        BatchTable.ItemsSource = HDataSamples.Clusters(5);
        BatchTable.SelectedRows = new List<object> { HDataSamples.AllClusters[0], HDataSamples.AllClusters[2] };

        // Expandable rows with the first one open.
        HDataSamples.Bind(ExpandTable);
        ExpandTable.ExpandedRowTemplate = HDataSamples.ExpandedTemplate();
        ExpandTable.ItemsSource = HDataSamples.Clusters(4);
        ExpandTable.ExpandedRows = new List<object> { HDataSamples.AllClusters[0] };

        // Sorted and zebra.
        HDataSamples.AddClusterColumns(ZebraTable);
        ZebraTable.ItemsSource = HDataSamples.Clusters(6);
        ZebraTable.SelectedRows = new List<object> { HDataSamples.AllClusters[4] };

        // Sizes.
        foreach (var size in new[] { CdsSize.Xs, CdsSize.Sm, CdsSize.Md, CdsSize.Lg, CdsSize.Xl })
        {
            var table = new CdsDataTable
            {
                Title = $"Size {size.ToString().ToLowerInvariant()}",
                Size = size,
                IsSearchable = true,
                IsSortable = true,
                ItemsSource = HDataSamples.Clusters(3),
            };
            HDataSamples.AddClusterColumns(table, region: false);
            table.Headers[1].IsSortable = false;
            SizesHost.Add(table);
        }

        // Radio selection.
        HDataSamples.AddClusterColumns(RadioTable);
        RadioTable.ItemsSource = HDataSamples.Clusters(8);
        RadioTable.IsRowDisabled = r => ((HCluster)r).StatusText == "Pending";
        RadioTable.SelectedRows = new List<object> { HDataSamples.AllClusters[6] };

        // Sticky header: more rows than fit.
        HDataSamples.AddClusterColumns(StickyTable);
        StickyTable.ItemsSource = HDataSamples.AllClusters;

        // Large data set: virtualized by the sticky header.
        HDataSamples.AddClusterColumns(LargeTable);
        HDataSamples.AddRowActions(LargeTable);
        LargeTable.ExpandedRowTemplate = HDataSamples.ExpandedTemplate();
        LargeTable.ItemsSource = HDataSamples.Generated(10_000);

        // Empty states.
        HDataSamples.AddClusterColumns(EmptyTable);
        EmptyTable.ItemsSource = Array.Empty<HCluster>();
        HDataSamples.AddClusterColumns(NoMatchTable);
        NoMatchTable.ItemsSource = HDataSamples.Clusters(4);

        // Paged table.
        HDataSamples.AddClusterColumns(PagedTable);
        ShowPage();

        Strip.Template = new DataTemplate(() => HDataSamples.CompactTable());
    }

    void ShowPage() =>
        PagedTable.ItemsSource = HDataSamples.AllClusters.Skip(Pager.FirstItemIndex).Take(Pager.PageSize).ToList();

    void OnPageChanged(object? sender, CdsPaginationChangedEventArgs e) => ShowPage();

    void OnMainSelectionChanged(object? sender, CdsDataTableRowsEventArgs e) => UpdateMainStatus();

    void OnMainSortChanged(object? sender, CdsDataTableSortEventArgs e) => UpdateMainStatus();

    void OnMainSearchChanged(object? sender, TextChangedEventArgs e) => UpdateMainStatus();

    void UpdateMainStatus()
    {
        var sort = MainTable.SortKey is null ? "Not sorted" : $"Sorted by {MainTable.SortKey}, {MainTable.SortDirection.ToString().ToLowerInvariant()}";
        var selected = MainTable.SelectedCount == 0 ? "Nothing selected" : $"{MainTable.SelectedCount} selected";
        var search = string.IsNullOrEmpty(MainTable.SearchText) ? string.Empty : $" Filter: \"{MainTable.SearchText}\" ({MainTable.VisibleRowCount} rows).";
        MainStatus.Text = $"{sort}. {selected}.{search}";
    }

    void OnBatchRestart(object? sender, CdsDataTableRowsEventArgs e) =>
        MainStatus.Text = $"Restarting {string.Join(", ", e.Rows.Cast<HCluster>().Select(c => c.Name))}";

    void OnBatchDelete(object? sender, CdsDataTableRowsEventArgs e) =>
        MainStatus.Text = $"Would delete {e.Rows.Count} cluster(s). Confirm in a modal first.";

    void OnRadioSelectionChanged(object? sender, CdsDataTableRowsEventArgs e) =>
        RadioStatus.Text = e.Rows.Count == 0 ? "Nothing chosen" : $"Chosen: {((HCluster)e.Rows[0]).Name}";

    void OnPrimitiveHeaderClicked(object? sender, EventArgs e)
    {
        if (sender is not CdsTableHeader header) return;
        header.SortDirection = header.SortDirection == CdsSortDirection.Ascending ? CdsSortDirection.Descending : CdsSortDirection.Ascending;
    }
}
