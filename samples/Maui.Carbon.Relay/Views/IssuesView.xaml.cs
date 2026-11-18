using System.ComponentModel;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Presentation;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The issue list. Bindings carry the values; this code-behind adapts the table's events (selection, sort, paging,
/// row and batch actions) and builds the filter controls whose items come from enums.
/// </summary>
public partial class IssuesView : ContentView
{
    readonly Dictionary<IssueStatus, CdsCheckbox> _statusBoxes = [];
    readonly Dictionary<IssueType, CdsSelectableTag> _typeTags = [];
    IssuesViewModel? _vm;
    bool _syncing;

    public IssuesView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Scroller, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
        IssueColumns.Add(Table, OpenRow);
        Table.RowActionsProvider = RowActions;

        foreach (var status in Enum.GetValues<IssueStatus>())
        {
            var box = new CdsCheckbox { Label = status.Text() };
            box.CheckedChanged += (_, e) =>
            {
                if (!_syncing) _vm?.ToggleStatus(status, e.Value);
            };
            _statusBoxes[status] = box;
            StatusGroup.Items.Add(box);
        }
        foreach (var type in Enum.GetValues<IssueType>())
        {
            var tag = new CdsSelectableTag { Text = type.Text(), Icon = CarbonMap.Icon(type), Margin = new Thickness(0, 0, 8, 8) };
            tag.SelectedChanged += (_, e) =>
            {
                if (!_syncing) _vm?.ToggleType(type, e.Value);
            };
            _typeTags[type] = tag;
            TypeTags.Add(tag);
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_vm is not null)
        {
            _vm.PropertyChanged -= OnViewModelChanged;
            _vm.AppliedFilters.CollectionChanged -= OnFiltersChanged;
            _vm.FiltersReset -= OnFiltersReset;
        }
        _vm = BindingContext as IssuesViewModel;
        if (_vm is null) return;
        _vm.PropertyChanged += OnViewModelChanged;
        _vm.AppliedFilters.CollectionChanged += OnFiltersChanged;
        _vm.FiltersReset += OnFiltersReset;
        SyncFilterControls();
    }

    // ---- view model → controls the bindings can't reach ----

    void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IssuesViewModel.Selected)) PushSelection();
    }

    /// <summary>Mirrors the view model's selection into the table (rows are rebuilt on every refresh).</summary>
    void PushSelection()
    {
        if (_vm is null) return;
        var current = Table.SelectedRows?.Cast<IssueRow>().Select(r => r.Key).ToHashSet() ?? [];
        if (current.SetEquals(_vm.Selected.Select(r => r.Key))) return;
        _syncing = true;
        try
        {
            Table.SelectedRows = _vm.Selected.Cast<object>().ToList();
        }
        finally
        {
            _syncing = false;
        }
    }

    void OnFiltersChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => SyncFilterControls();

    void OnFiltersReset(object? sender, EventArgs e) => SyncFilterControls();

    void SyncFilterControls()
    {
        if (_vm is null) return;
        _syncing = true;
        try
        {
            foreach (var (status, box) in _statusBoxes) box.IsChecked = _vm.SelectedStatuses.Contains(status);
            foreach (var (type, tag) in _typeTags) tag.IsSelected = _vm.SelectedTypes.Contains(type);
            if (!SameItems(PriorityFilter.SelectedItems, _vm.SelectedPriorities.Cast<object>()))
                PriorityFilter.SelectedItems = _vm.SelectedPriorities.Cast<object>().ToList();
            if (!SameItems(LabelFilter.SelectedItems, _vm.SelectedLabels))
                LabelFilter.SelectedItems = _vm.SelectedLabels.Cast<object>().ToList();
        }
        finally
        {
            _syncing = false;
        }
    }

    static bool SameItems(System.Collections.IList? a, IEnumerable<object> b) =>
        (a?.Cast<object>() ?? []).ToHashSet().SetEquals(b);

    // ---- controls → view model ----

    void OnSelectionChanged(object? sender, CdsDataTableRowsEventArgs e)
    {
        if (_syncing || _vm is null) return;
        _vm.Selected = e.Rows.Cast<IssueRow>().ToList();
    }

    void OnSortChanged(object? sender, CdsDataTableSortEventArgs e) =>
        _vm?.ApplySort(e.Direction == CdsSortDirection.None ? null : e.Key, e.Direction == CdsSortDirection.Descending);

    void OnPageChanged(object? sender, CdsPaginationChangedEventArgs e)
    {
        if (_vm is not null && (e.Page != _vm.Page || e.PageSize != _vm.PageSize)) _vm.ChangePage(e.Page, e.PageSize);
    }

    void OnPrioritiesChanged(object? sender, CdsListBoxSelectionChangedEventArgs e)
    {
        if (!_syncing) _vm?.SetPriorities(e.SelectedItems.OfType<IssuePriority>());
    }

    void OnLabelsChanged(object? sender, CdsListBoxSelectionChangedEventArgs e)
    {
        if (!_syncing) _vm?.SetLabels(e.SelectedItems.OfType<string>());
    }

    void OnChipDismissed(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: FilterChip chip }) chip.Remove();
    }

    void OnUndoClosed(object? sender, CdsNotificationCloseEventArgs e) => _vm?.DismissUndoCommand.Execute(null);

    void OnExportCsv(object? sender, EventArgs e) => _vm?.ExportCommand.Execute("csv");

    void OnExportJson(object? sender, EventArgs e) => _vm?.ExportCommand.Execute("json");

    void OnBatchAssign(object? sender, CdsDataTableRowsEventArgs e) => _vm?.AssignSelectedToMeCommand.Execute(null);

    void OnBatchStart(object? sender, CdsDataTableRowsEventArgs e) => _vm?.MoveSelectedCommand.Execute(IssueStatus.InProgress);

    void OnBatchDone(object? sender, CdsDataTableRowsEventArgs e) => _vm?.MoveSelectedCommand.Execute(IssueStatus.Done);

    void OnBatchDelete(object? sender, CdsDataTableRowsEventArgs e) => _vm?.RequestDeleteSelectedCommand.Execute(null);

    void OnOpenExpanded(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: IssueRow row }) OpenRow(row);
    }

    void OpenRow(IssueRow row) => _vm?.OpenIssueCommand.Execute(row);

    /// <summary>The row menu: unavailable actions are disabled, not hidden; the destructive one is last.</summary>
    IEnumerable<CdsDataTableRowAction> RowActions(object row)
    {
        var issue = (IssueRow)row;
        var mine = issue.Issue.AssigneeId == Maui.Carbon.Relay.Services.MockDataSeed.MeId;
        var watching = issue.Issue.WatcherIds.Contains(Maui.Carbon.Relay.Services.MockDataSeed.MeId);
        var open = new CdsDataTableRowAction { Label = "Open issue" };
        var assign = new CdsDataTableRowAction { Label = "Assign to me", IsEnabled = !mine };
        var watch = new CdsDataTableRowAction { Label = watching ? "Stop watching" : "Watch" };
        var delete = new CdsDataTableRowAction { Label = "Delete issue", Kind = CdsMenuItemKind.Danger, Icon = CdsIconName.TrashCan };
        open.Invoked += (_, _) => OpenRow(issue);
        assign.Invoked += (_, _) => _vm?.AssignToMeCommand.Execute(issue);
        watch.Invoked += (_, _) => _vm?.ToggleWatchCommand.Execute(issue);
        delete.Invoked += (_, _) => _vm?.RequestDeleteOneCommand.Execute(issue);
        return [open, assign, watch, delete];
    }
}
