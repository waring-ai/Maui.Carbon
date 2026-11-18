using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// The issue list: scope (all / mine / watching), search, filters, sort and paging over every issue, plus bulk
/// actions on the selected rows with a confirmed, undoable delete.
/// </summary>
public sealed partial class IssuesViewModel : PageViewModel
{
    readonly IIssueService _issues;
    readonly IPeopleService _people;
    readonly IProjectService _projects;
    readonly INavigator _navigator;
    readonly IUserFeedback _feedback;
    readonly IClock _clock;
    readonly HashSet<IssueStatus> _statuses = [];
    readonly HashSet<IssuePriority> _priorities = [];
    readonly HashSet<IssueType> _types = [];
    readonly HashSet<string> _labels = [];
    bool _loaded;

    public IssuesViewModel(IIssueService issues, IPeopleService people, IProjectService projects, INavigator navigator, IUserFeedback feedback, IClock clock)
    {
        _issues = issues;
        _people = people;
        _projects = projects;
        _navigator = navigator;
        _feedback = feedback;
        _clock = clock;
        Title = "Issues";
        AssigneeOptions = [new AssigneeOption(null, "Anyone"), new AssigneeOption(IssueFilter.Unassigned, "Unassigned"), .. people.All.Select(p => new AssigneeOption(p.Id, p.Name))];
        AssigneeFilter = AssigneeOptions[0];
        ProjectOptions = [new ProjectOption(null, "All projects"), .. projects.All.Where(p => !p.IsArchived).Select(p => new ProjectOption(p.Id, p.Name))];
        ProjectFilter = ProjectOptions[0];
    }

    // ---- what the table shows ----

    [ObservableProperty]
    public partial IReadOnlyList<IssueRow> Rows { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty), nameof(CountText))]
    public partial int TotalCount { get; set; }

    public bool IsEmpty => !IsLoading && TotalCount == 0;

    public string CountText => TotalCount == 1 ? "1 issue" : $"{TotalCount} issues";

    [ObservableProperty]
    public partial int Page { get; set; } = 1;

    [ObservableProperty]
    public partial int PageSize { get; set; } = 25;

    // ---- scope, search, sort ----

    /// <summary>The content switcher index: 0 all, 1 assigned to me, 2 watching.</summary>
    [ObservableProperty]
    public partial int ScopeIndex { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = "";

    public IssueSort Sort { get; private set; } = IssueSort.Default;

    // ---- filters ----

    public IReadOnlyList<IssueStatus> AllStatuses { get; } = Enum.GetValues<IssueStatus>();

    public IReadOnlyList<IssuePriority> AllPriorities { get; } = Enum.GetValues<IssuePriority>().Reverse().ToList();

    public IReadOnlyList<IssueType> AllTypes { get; } = Enum.GetValues<IssueType>();

    public IReadOnlyList<string> AllLabels => _issues.Labels;

    public IReadOnlyList<AssigneeOption> AssigneeOptions { get; }

    public IReadOnlyList<ProjectOption> ProjectOptions { get; }

    [ObservableProperty]
    public partial AssigneeOption AssigneeFilter { get; set; }

    [ObservableProperty]
    public partial ProjectOption ProjectFilter { get; set; }

    [ObservableProperty]
    public partial bool OpenOnly { get; set; }

    [ObservableProperty]
    public partial bool IsFilterPanelOpen { get; set; }

    public ObservableCollection<FilterChip> AppliedFilters { get; } = [];

    [ObservableProperty]
    public partial bool HasAppliedFilters { get; set; }

    public IReadOnlySet<IssueStatus> SelectedStatuses => _statuses;

    public IReadOnlySet<IssuePriority> SelectedPriorities => _priorities;

    public IReadOnlySet<IssueType> SelectedTypes => _types;

    public IReadOnlySet<string> SelectedLabels => _labels;

    // ---- selection and bulk actions ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionText))]
    [NotifyCanExecuteChangedFor(nameof(AssignSelectedToMeCommand), nameof(MoveSelectedCommand), nameof(RequestDeleteSelectedCommand))]
    public partial IReadOnlyList<IssueRow> Selected { get; set; } = [];

    public string SelectionText => Selected.Count == 1 ? "1 issue selected" : $"{Selected.Count} issues selected";

    [ObservableProperty]
    public partial bool IsDeleteConfirmOpen { get; set; }

    [ObservableProperty]
    public partial string DeleteConfirmText { get; set; } = "";

    /// <summary>Issues waiting to be deleted once the modal is confirmed.</summary>
    IReadOnlyList<Issue> _pendingDelete = [];

    /// <summary>The last deleted issues, kept for undo (an actionable notification shows while set).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUndo))]
    public partial IReadOnlyList<Issue>? LastDeleted { get; set; }

    public bool HasUndo => LastDeleted is { Count: > 0 };

    [ObservableProperty]
    public partial string UndoText { get; set; } = "";

    // ---- lifecycle ----

    protected override async Task OnActivatedAsync(object? parameter, CancellationToken ct)
    {
        _issues.Changed += OnIssuesChanged;
        if (parameter is IssueScope scope) ScopeIndex = (int)scope;
        if (!_loaded)
        {
            IsLoading = true;
            OnPropertyChanged(nameof(IsEmpty));
            await _issues.ListAsync(ct);
            _loaded = true;
            IsLoading = false;
            OnPropertyChanged(nameof(IsEmpty));
        }
        Refresh();
    }

    protected override void OnDeactivated()
    {
        _issues.Changed -= OnIssuesChanged;
        IsFilterPanelOpen = false;
    }

    void OnIssuesChanged(object? sender, IssuesChangedEventArgs e) => Refresh();

    partial void OnScopeIndexChanged(int value) => ResetPageAndRefresh();

    partial void OnSearchTextChanged(string value) => ResetPageAndRefresh();

    partial void OnAssigneeFilterChanged(AssigneeOption value) => ResetPageAndRefresh();

    partial void OnProjectFilterChanged(ProjectOption value) => ResetPageAndRefresh();

    partial void OnOpenOnlyChanged(bool value) => ResetPageAndRefresh();

    // ---- view entry points (called from the view's control events) ----

    public void ApplySort(string? key, bool descending)
    {
        Sort = key is null ? IssueSort.Default : new IssueSort(key, descending);
        Refresh();
    }

    public void ChangePage(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
        Refresh();
    }

    public void SetStatuses(IEnumerable<IssueStatus> statuses) => Replace(_statuses, statuses);

    public void SetPriorities(IEnumerable<IssuePriority> priorities) => Replace(_priorities, priorities);

    public void SetTypes(IEnumerable<IssueType> types) => Replace(_types, types);

    public void SetLabels(IEnumerable<string> labels) => Replace(_labels, labels);

    public void ToggleStatus(IssueStatus status, bool on)
    {
        if (on ? _statuses.Add(status) : _statuses.Remove(status)) ResetPageAndRefresh();
    }

    public void ToggleType(IssueType type, bool on)
    {
        if (on ? _types.Add(type) : _types.Remove(type)) ResetPageAndRefresh();
    }

    void Replace<T>(HashSet<T> set, IEnumerable<T> values)
    {
        var next = values.ToHashSet();
        if (set.SetEquals(next)) return;
        set.Clear();
        set.UnionWith(next);
        ResetPageAndRefresh();
    }

    // ---- commands ----

    [RelayCommand]
    void ToggleFilterPanel() => IsFilterPanelOpen = !IsFilterPanelOpen;

    [RelayCommand]
    void ClearFilters()
    {
        _statuses.Clear();
        _priorities.Clear();
        _types.Clear();
        _labels.Clear();
        AssigneeFilter = AssigneeOptions[0];
        ProjectFilter = ProjectOptions[0];
        OpenOnly = false;
        SearchText = "";
        FiltersReset?.Invoke(this, EventArgs.Empty);
        ResetPageAndRefresh();
    }

    /// <summary>Raised when filters are cleared from code, so the view can reset multi-select controls it owns.</summary>
    public event EventHandler? FiltersReset;

    [RelayCommand]
    Task OpenIssueAsync(IssueRow row) => _navigator.NavigateAsync(Routes.IssueDetail, row.Key);

    [RelayCommand]
    Task CreateIssueAsync() => _navigator.NavigateAsync(Routes.CreateIssue);

    bool HasSelection => Selected.Count > 0;

    [RelayCommand(CanExecute = nameof(HasSelection))]
    async Task AssignSelectedToMeAsync()
    {
        var list = Selected.Select(r => r.Issue).ToList();
        await _issues.AssignAsync(list, _people.Me.Id);
        _feedback.Toast(FeedbackKind.Success, list.Count == 1 ? $"{list[0].Key} assigned to you" : $"{list.Count} issues assigned to you");
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    async Task MoveSelectedAsync(IssueStatus status)
    {
        var list = Selected.Select(r => r.Issue).ToList();
        await _issues.SetStatusAsync(list, status);
        _feedback.Toast(FeedbackKind.Success, list.Count == 1 ? $"{list[0].Key} moved to {status.Text()}" : $"{list.Count} issues moved to {status.Text()}");
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    void RequestDeleteSelected() => RequestDelete(Selected.Select(r => r.Issue).ToList());

    [RelayCommand]
    void RequestDeleteOne(IssueRow row) => RequestDelete([row.Issue]);

    void RequestDelete(IReadOnlyList<Issue> issues)
    {
        _pendingDelete = issues;
        DeleteConfirmText = issues.Count == 1
            ? $"{issues[0].Key} \"{issues[0].Title}\" and its {issues[0].Comments.Count} comments will be deleted. You can undo this right after."
            : $"{issues.Count} issues and their comments will be deleted. You can undo this right after.";
        IsDeleteConfirmOpen = true;
    }

    [RelayCommand]
    async Task ConfirmDeleteAsync()
    {
        IsDeleteConfirmOpen = false;
        var list = _pendingDelete;
        _pendingDelete = [];
        if (list.Count == 0) return;
        await _issues.DeleteAsync(list);
        Selected = [];
        LastDeleted = list;
        UndoText = list.Count == 1 ? $"{list[0].Key} was deleted." : $"{list.Count} issues were deleted.";
    }

    [RelayCommand]
    void CancelDelete()
    {
        IsDeleteConfirmOpen = false;
        _pendingDelete = [];
    }

    [RelayCommand]
    async Task UndoDeleteAsync()
    {
        if (LastDeleted is not { } list) return;
        LastDeleted = null;
        await _issues.RestoreAsync(list);
        _feedback.Toast(FeedbackKind.Info, list.Count == 1 ? $"{list[0].Key} restored" : $"{list.Count} issues restored");
    }

    [RelayCommand]
    void DismissUndo() => LastDeleted = null;

    [RelayCommand]
    async Task AssignToMeAsync(IssueRow row)
    {
        await _issues.AssignAsync([row.Issue], _people.Me.Id);
        _feedback.Toast(FeedbackKind.Success, $"{row.Key} assigned to you");
    }

    [RelayCommand]
    async Task ToggleWatchAsync(IssueRow row)
    {
        await _issues.ToggleWatchAsync(row.Issue);
        var watching = row.Issue.WatcherIds.Contains(_people.Me.Id);
        _feedback.Toast(FeedbackKind.Info, watching ? $"Watching {row.Key}" : $"Stopped watching {row.Key}");
    }

    [RelayCommand]
    async Task ExportAsync(string format)
    {
        var rows = Filtered().ToList();
        var text = await _issues.ExportAsync(rows, format);
        _feedback.Toast(FeedbackKind.Success, $"Exported {rows.Count} issues as {format.ToUpperInvariant()}", $"{text.Length / 1024.0:0.0} KB ready to download");
    }

    // ---- query ----

    void ResetPageAndRefresh()
    {
        Page = 1;
        Refresh();
    }

    IssueFilter CurrentFilter() => new()
    {
        Scope = (IssueScope)ScopeIndex,
        SearchText = SearchText,
        ProjectId = ProjectFilter.Id,
        AssigneeId = AssigneeFilter.Id,
        Statuses = _statuses,
        Priorities = _priorities,
        Types = _types,
        Labels = _labels,
        OpenOnly = OpenOnly,
    };

    IEnumerable<Issue> Filtered() =>
        IssueQuery.Sort(IssueQuery.Filter(_issues.All, CurrentFilter(), _people.Me.Id), Sort, _people.NameOf);

    void Refresh()
    {
        if (!_loaded) return;
        var all = Filtered().ToList();
        var page = IssueQuery.Page(all, Page, PageSize);
        var now = _clock.Now;
        TotalCount = page.TotalCount;
        if (Page != page.Page) Page = page.Page;
        Rows = page.Items.Select(i => IssueRow.From(i, _people, _projects, now)).ToList();
        // Keep selected rows that still exist (the table matches them by key).
        var keys = Rows.Select(r => r.Key).ToHashSet();
        Selected = Selected.Where(r => keys.Contains(r.Key)).Select(r => Rows.First(x => x.Key == r.Key)).ToList();
        RebuildChips();
    }

    void RebuildChips()
    {
        AppliedFilters.Clear();
        foreach (var s in _statuses.Order()) AppliedFilters.Add(new FilterChip($"Status: {s.Text()}", () => ToggleStatus(s, false)));
        foreach (var p in _priorities.OrderDescending()) AppliedFilters.Add(new FilterChip($"Priority: {p.Text()}", () => { _priorities.Remove(p); ResetPageAndRefresh(); }));
        foreach (var t in _types.Order()) AppliedFilters.Add(new FilterChip($"Type: {t.Text()}", () => ToggleType(t, false)));
        foreach (var l in _labels.Order(StringComparer.Ordinal)) AppliedFilters.Add(new FilterChip($"Label: {l}", () => { _labels.Remove(l); FiltersReset?.Invoke(this, EventArgs.Empty); ResetPageAndRefresh(); }));
        if (AssigneeFilter.Id is not null) AppliedFilters.Add(new FilterChip($"Assignee: {AssigneeFilter.Name}", () => AssigneeFilter = AssigneeOptions[0]));
        if (ProjectFilter.Id is not null) AppliedFilters.Add(new FilterChip($"Project: {ProjectFilter.Name}", () => ProjectFilter = ProjectOptions[0]));
        if (OpenOnly) AppliedFilters.Add(new FilterChip("Open issues only", () => OpenOnly = false));
        HasAppliedFilters = AppliedFilters.Count > 0;
    }
}

/// <summary>An entry in the assignee filter (null id = anyone).</summary>
public sealed record AssigneeOption(string? Id, string Name)
{
    public override string ToString() => Name;
}

/// <summary>An entry in the project filter (null id = all projects).</summary>
public sealed record ProjectOption(string? Id, string Name)
{
    public override string ToString() => Name;
}
