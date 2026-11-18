using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Patterns.Data;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Resource list", GalleryCategory.Patterns, Order = 20, Spec = "PatternResourceList",
    Summary = "A resource list page: UI shell header and side nav, page header with breadcrumb and description, a searchable, sortable, selectable data table with batch actions, a row overflow menu and pagination.")]
public partial class PatternResourceListPage : GalleryPage
{
    const string LatestVersion = "1.31.2";

    public PatternResourceListPage()
    {
        InitializeComponent();

        CdsTokens.SetBrush(ShellFrame, Border.StrokeProperty, CdsColorKeys.BorderSubtle01);

        K1Clusters.AddColumns(MainTable);
        MainTable.RowActionsProvider = RowActions;
        Refresh();
        // Two clusters are selected when the page opens, so the batch bar shows.
        MainTable.SelectedRows = new List<object> { K1Clusters.All[4], K1Clusters.All[5] };
        UpdateStatus();

        K1Clusters.AddColumns(SearchTable);
        SearchTable.RowActionsProvider = RowActions;
        SearchTable.ItemsSource = K1Clusters.All.Take(10).ToList();
    }

    /// <summary>The row menu: unavailable items are disabled, not removed; the destructive item is last.</summary>
    IEnumerable<CdsDataTableRowAction> RowActions(object row)
    {
        var cluster = (K1Cluster)row;
        var view = new CdsDataTableRowAction { Label = "View details" };
        var add = new CdsDataTableRowAction { Label = "Add nodes" };
        var update = new CdsDataTableRowAction { Label = "Update version", IsEnabled = cluster.Version != LatestVersion };
        var delete = new CdsDataTableRowAction { Label = "Delete cluster", Kind = CdsMenuItemKind.Danger, Icon = CdsIconName.TrashCan };
        view.Invoked += (_, _) => Status.Text = $"Opening {cluster.Name}.";
        add.Invoked += (_, _) => Status.Text = $"Adding nodes to {cluster.Name}.";
        update.Invoked += (_, _) => Status.Text = $"Updating {cluster.Name} to {LatestVersion}.";
        delete.Invoked += (_, _) => Status.Text = $"Would delete {cluster.Name}. Confirm in a modal first.";
        return [view, add, update, delete];
    }

    /// <summary>Searches, sorts and pages the whole list, so the table shows the right slice and the pager the right range.</summary>
    bool _refreshing;

    void Refresh()
    {
        if (_refreshing) return;
        _refreshing = true;
        try
        {
            RefreshCore();
        }
        finally
        {
            _refreshing = false;
        }
    }

    void RefreshCore()
    {
        IEnumerable<K1Cluster> rows = K1Clusters.All;
        var query = MainTable.SearchText?.Trim();
        if (!string.IsNullOrEmpty(query))
            rows = rows.Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        if (MainTable.SortKey is { } key && MainTable.SortDirection != CdsSortDirection.None)
        {
            Func<K1Cluster, object> selector = key switch
            {
                "status" => c => c.StatusText,
                "region" => c => c.Region,
                "version" => c => c.Version,
                "nodes" => c => c.Nodes,
                "created" => c => c.Created,
                _ => c => c.Name,
            };
            rows = MainTable.SortDirection == CdsSortDirection.Ascending ? rows.OrderBy(selector) : rows.OrderByDescending(selector);
        }

        var list = rows.ToList();
        Pager.TotalItems = list.Count;
        if (Pager.Page > Pager.PageCount) Pager.Page = Math.Max(1, Pager.PageCount);
        MainTable.ItemsSource = list.Skip(Pager.FirstItemIndex).Take(Pager.PageSize).ToList();
    }

    void UpdateStatus()
    {
        Status.Text = MainTable.SelectedCount switch
        {
            0 => "Nothing selected.",
            1 => "1 cluster selected.",
            var n => $"{n} clusters selected.",
        };
    }

    void OnSelectionChanged(object? sender, CdsDataTableRowsEventArgs e) => UpdateStatus();

    void OnSortChanged(object? sender, CdsDataTableSortEventArgs e) => Refresh();

    void OnSearchChanged(object? sender, TextChangedEventArgs e) => Refresh();

    void OnPageChanged(object? sender, CdsPaginationChangedEventArgs e) => Refresh();

    void OnBatchStop(object? sender, CdsDataTableRowsEventArgs e) => Status.Text = $"Stopping {Names(e)}.";

    void OnBatchExport(object? sender, CdsDataTableRowsEventArgs e) => Status.Text = $"Exporting {Names(e)}.";

    void OnBatchDelete(object? sender, CdsDataTableRowsEventArgs e) => Status.Text = $"Would delete {e.Rows.Count} clusters ({Names(e)}). Confirm in a modal first.";

    void OnExportClicked(object? sender, EventArgs e) => Status.Text = "Exporting the cluster list.";

    void OnCreateClicked(object? sender, EventArgs e) => Status.Text = "Opening the create cluster form.";

    static string Names(CdsDataTableRowsEventArgs e) => string.Join(", ", e.Rows.Cast<K1Cluster>().Select(c => c.Name));
}
