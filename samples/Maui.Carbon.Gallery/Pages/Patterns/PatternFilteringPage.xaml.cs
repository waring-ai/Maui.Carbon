using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Patterns.Data;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Filtering", GalleryCategory.Patterns, Order = 40, Spec = "PatternFiltering",
    Summary = "Narrowing a resource list with search and filters: a left filter panel that updates results instantly, a result count, removable applied-filter tags and Clear filters, plus a toolbar flyout that applies a batch of filters together.")]
public partial class PatternFilteringPage : GalleryPage
{
    static readonly string[] s_statuses = ["Running", "Updating", "Warning", "Failed", "Stopped"];
    static readonly string[] s_regions = ["Dallas", "Frankfurt", "London", "Tokyo", "Washington DC"];

    static IReadOnlyList<K1Cluster> Data => K1Clusters.ForFiltering;

    // Instant filters.
    readonly HashSet<string> _statuses = ["Running"];
    readonly HashSet<string> _regions = ["Frankfurt", "Dallas"];
    readonly Dictionary<string, List<CdsCheckbox>> _checks = [];
    readonly Dictionary<string, CdsButton> _clearButtons = [];
    string _version = "any";
    string _query = string.Empty;
    bool _syncing;

    // Batch filters: what is applied, and the draft in the open flyout.
    readonly HashSet<string> _appliedStatuses = ["Running"];
    readonly HashSet<string> _appliedRegions = ["Frankfurt", "London"];
    readonly HashSet<string> _draftStatuses = ["Running"];
    readonly HashSet<string> _draftRegions = ["Frankfurt", "London", "Tokyo"];
    readonly Dictionary<string, List<CdsCheckbox>> _flyoutChecks = [];
    string _batchQuery = string.Empty;

    public PatternFilteringPage()
    {
        InitializeComponent();

        CdsTokens.SetColor(FilterPanel, BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetColor(PanelRule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        CdsTokens.SetColor(BatchToolbar, BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetColor(BatchCount, BackgroundColorProperty, CdsColorKeys.Layer01);

        // Instant filters.
        K1Clusters.AddColumns(ResultsTable, created: false);
        BuildCategory(StatusHost, "status", "Status", s_statuses, _statuses, countOf: s => Data.Count(c => c.StatusText == s));
        BuildCategory(RegionHost, "region", "Region", s_regions, _regions, countOf: r => Data.Count(c => c.Region == r));
        ResultsTable.EmptyContent = BuildNoMatches();
        RefreshInstant();

        // Batch filters.
        K1Clusters.AddColumns(BatchTable, created: false);
        BuildFlyoutGroup(FlyoutStatus, "status", "Status", ["Running", "Updating", "Stopped"], _draftStatuses);
        BuildFlyoutGroup(FlyoutRegion, "region", "Region", s_regions.Take(4), _draftRegions);
        RefreshBatch();
    }

    // ----- instant filters -----

    void BuildCategory(VerticalStackLayout host, string key, string title, IEnumerable<string> options, HashSet<string> selected, Func<string, int> countOf)
    {
        var boxes = new List<CdsCheckbox>();
        foreach (var option in options)
        {
            var box = new CdsCheckbox { Label = $"{option} ({countOf(option)})", IsChecked = selected.Contains(option), StyleId = option };
            var captured = option;
            box.CheckedChanged += (_, e) =>
            {
                if (_syncing) return;
                if (e.Value) selected.Add(captured);
                else selected.Remove(captured);
                RefreshInstant();
            };
            boxes.Add(box);
            host.Add(box);
        }

        _checks[key] = boxes;

        var clear = new CdsButton { Text = $"Clear {title.ToLowerInvariant()}", Kind = CdsButtonKind.Ghost, Size = CdsSize.Sm, HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0) };
        clear.Clicked += (_, _) =>
        {
            selected.Clear();
            SyncChecks();
            RefreshInstant();
        };
        _clearButtons[key] = clear;
        host.Add(clear);
    }

    View BuildNoMatches()
    {
        var clear = new CdsButton { Text = "Clear filters", Kind = CdsButtonKind.Tertiary };
        clear.Clicked += (_, _) => ClearAll();
        return new K1EmptyState(
            null,
            "No clusters match these filters",
            $"Remove a filter or clear them all to see the {Data.Count} clusters in this account.",
            [clear],
            new Thickness(CdsSpacing.Spacing05, 0));
    }

    void SyncChecks()
    {
        _syncing = true;
        foreach (var (key, boxes) in _checks)
        {
            var selected = key == "status" ? _statuses : _regions;
            foreach (var box in boxes) box.IsChecked = selected.Contains(box.StyleId);
        }

        VersionGroup.SelectedValue = _version;
        _syncing = false;
    }

    void ClearAll()
    {
        _statuses.Clear();
        _regions.Clear();
        _version = "any";
        _query = string.Empty;
        Search.Text = string.Empty;
        SyncChecks();
        RefreshInstant();
    }

    void OnClearAllClicked(object? sender, EventArgs e) => ClearAll();

    void OnVersionChanged(object? sender, CdsChoiceChangedEventArgs<object> e)
    {
        if (_syncing) return;
        _version = e.NewValue as string ?? "any";
        RefreshInstant();
    }

    void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_syncing) return;
        _query = e.NewTextValue?.Trim() ?? string.Empty;
        RefreshInstant();
    }

    void RefreshInstant()
    {
        var rows = Data.Where(c =>
                (_statuses.Count == 0 || _statuses.Contains(c.StatusText))
                && (_regions.Count == 0 || _regions.Contains(c.Region))
                && (_version == "any" || c.Minor == _version)
                && (_query.Length == 0 || c.Name.Contains(_query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        ResultsTable.ItemsSource = rows;

        StatusItem.Title = _statuses.Count > 0 ? $"Status ({_statuses.Count})" : "Status";
        RegionItem.Title = _regions.Count > 0 ? $"Region ({_regions.Count})" : "Region";
        VersionItem.Title = _version != "any" ? "Kubernetes version (1)" : "Kubernetes version";
        _clearButtons["status"].IsVisible = _statuses.Count > 0;
        _clearButtons["region"].IsVisible = _regions.Count > 0;
        ClearAllButton.IsEnabled = _statuses.Count > 0 || _regions.Count > 0 || _version != "any";

        // The count is always shown, including zero; then one tag per applied filter and Clear filters.
        AppliedRow.Clear();
        AppliedRow.Add(new CdsText($"Showing {rows.Count} of {Data.Count} clusters", CdsTypeStyle.BodyCompact01, CdsTextTone.Secondary)
        {
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, CdsSpacing.Spacing05, 0),
        });

        var tags = 0;
        foreach (var status in s_statuses.Where(_statuses.Contains)) { AddTag($"Status: {status}", () => { _statuses.Remove(status); SyncChecks(); RefreshInstant(); }); tags++; }
        foreach (var region in s_regions.Where(_regions.Contains)) { AddTag($"Region: {region}", () => { _regions.Remove(region); SyncChecks(); RefreshInstant(); }); tags++; }
        if (_version != "any") { AddTag($"Version: {_version}", () => { _version = "any"; SyncChecks(); RefreshInstant(); }); tags++; }

        if (tags > 0 || _query.Length > 0)
        {
            var clear = new CdsButton { Text = "Clear filters", Kind = CdsButtonKind.Ghost, Size = CdsSize.Sm };
            clear.Clicked += (_, _) => ClearAll();
            AppliedRow.Add(clear);
        }
    }

    void AddTag(string text, Action remove)
    {
        var tag = new CdsDismissibleTag { Text = text, DismissLabel = $"Remove filter {text}", Margin = new Thickness(0, 0, CdsSpacing.Spacing03, 0) };
        tag.Dismissed += (_, _) => remove();
        AppliedRow.Add(tag);
    }

    // ----- batch filters -----

    void BuildFlyoutGroup(VerticalStackLayout host, string key, string title, IEnumerable<string> options, HashSet<string> draft)
    {
        host.Add(new CdsText(title, CdsTypeStyle.HeadingCompact01) { Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) });
        var boxes = new List<CdsCheckbox>();
        foreach (var option in options)
        {
            var box = new CdsCheckbox { Label = option, IsChecked = draft.Contains(option) };
            var captured = option;
            box.CheckedChanged += (_, e) =>
            {
                if (_syncing) return;
                if (e.Value) draft.Add(captured);
                else draft.Remove(captured);
            };
            boxes.Add(box);
            host.Add(box);
        }

        _flyoutChecks[key] = boxes;
    }

    void SyncFlyout()
    {
        _syncing = true;
        foreach (var box in _flyoutChecks["status"]) box.IsChecked = _draftStatuses.Contains(box.Label ?? string.Empty);
        foreach (var box in _flyoutChecks["region"]) box.IsChecked = _draftRegions.Contains(box.Label ?? string.Empty);
        _syncing = false;
    }

    void RefreshBatch()
    {
        var rows = Data.Where(c =>
                (_appliedStatuses.Count == 0 || _appliedStatuses.Contains(c.StatusText))
                && (_appliedRegions.Count == 0 || _appliedRegions.Contains(c.Region))
                && (_batchQuery.Length == 0 || c.Name.Contains(_batchQuery, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        BatchTable.ItemsSource = rows;
        BatchCount.Text = $"Showing {rows.Count} of {Data.Count} clusters";

        var n = _appliedStatuses.Count + _appliedRegions.Count;
        BatchTag.IsVisible = n > 0;
        BatchTag.Text = n == 1 ? "1 filter" : $"{n} filters";
        BatchTag.DismissLabel = n == 1 ? "Clear 1 filter" : $"Clear {n} filters";
    }

    void OnBatchSearchChanged(object? sender, TextChangedEventArgs e)
    {
        _batchQuery = e.NewTextValue?.Trim() ?? string.Empty;
        RefreshBatch();
    }

    void OnFilterClicked(object? sender, EventArgs e)
    {
        FlyoutPopover.IsOpen = !FlyoutPopover.IsOpen;
        FilterButton.IsSelected = FlyoutPopover.IsOpen;
    }

    void OnFlyoutClosed(object? sender, EventArgs e) => FilterButton.IsSelected = false;

    void OnResetClicked(object? sender, EventArgs e)
    {
        _draftStatuses.Clear();
        _draftRegions.Clear();
        SyncFlyout();
    }

    void OnApplyClicked(object? sender, EventArgs e)
    {
        _appliedStatuses.Clear();
        _appliedStatuses.UnionWith(_draftStatuses);
        _appliedRegions.Clear();
        _appliedRegions.UnionWith(_draftRegions);
        FlyoutPopover.IsOpen = false;
        RefreshBatch();
    }

    void OnBatchTagDismissed(object? sender, EventArgs e)
    {
        _appliedStatuses.Clear();
        _appliedRegions.Clear();
        _draftStatuses.Clear();
        _draftRegions.Clear();
        SyncFlyout();
        RefreshBatch();
    }
}
