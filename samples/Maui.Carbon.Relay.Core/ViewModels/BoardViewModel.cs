using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// The board: a Kanban of the current project's active sprint, one column per <see cref="BoardColumns.Statuses"/>.
/// Quick filters narrow the cards, columns compare their load with the WIP limit, cards move from their menu, and a
/// select mode moves several cards at once. Refreshes when the project, the issues or the sprints change.
/// </summary>
public sealed partial class BoardViewModel : PageViewModel
{
    readonly IIssueService _issues;
    readonly ISprintService _sprints;
    readonly IWorkspace _workspace;
    readonly IPeopleService _people;
    readonly IProjectService _projects;
    readonly INavigator _navigator;
    readonly IUserFeedback _feedback;
    readonly IClock _clock;
    readonly HashSet<string> _selectedKeys = new(StringComparer.Ordinal);
    bool _loaded;
    bool _refreshing;

    public BoardViewModel(
        IIssueService issues,
        ISprintService sprints,
        IWorkspace workspace,
        IPeopleService people,
        IProjectService projects,
        INavigator navigator,
        IUserFeedback feedback,
        IClock clock)
    {
        _issues = issues;
        _sprints = sprints;
        _workspace = workspace;
        _people = people;
        _projects = projects;
        _navigator = navigator;
        _feedback = feedback;
        _clock = clock;
        Title = "Board";
        Columns = BoardColumns.Statuses.Select(s => new BoardColumn(s)).ToList();
        Filters =
        [
            new BoardFilter(BoardFilterKind.Bugs, "Bugs only"),
            new BoardFilter(BoardFilterKind.Mine, "Assigned to me"),
            new BoardFilter(BoardFilterKind.HighPriority, "High priority"),
        ];
        BulkTargets = BoardColumns.Statuses.Select(s => new BoardBulkTarget(s, $"Move to {s.Text()}")).ToList();
    }

    // ---- what the board shows ----

    public IReadOnlyList<BoardColumn> Columns { get; }

    public IReadOnlyList<BoardFilter> Filters { get; }

    /// <summary>The bulk "Move selected to…" menu entries.</summary>
    public IReadOnlyList<BoardBulkTarget> BulkTargets { get; }

    [ObservableProperty]
    public partial string ProjectName { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty), nameof(ShowBoard))]
    public partial bool HasActiveSprint { get; set; }

    [ObservableProperty]
    public partial string SprintName { get; set; } = "";

    [ObservableProperty]
    public partial string SprintGoal { get; set; } = "";

    /// <summary>"Nov 6 – Nov 20 · 2 days left".</summary>
    [ObservableProperty]
    public partial string SprintText { get; set; } = "";

    /// <summary>The page header text under the title.</summary>
    [ObservableProperty]
    public partial string HeaderText { get; set; } = "";

    /// <summary>No active sprint: the empty state shows instead of the board.</summary>
    public bool IsEmpty => !IsLoading && _loaded && !HasActiveSprint;

    public bool ShowBoard => !IsLoading && HasActiveSprint;

    [ObservableProperty]
    public partial string EmptyText { get; set; } = "";

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty]
    public partial int ShownCount { get; set; }

    /// <summary>"Showing 5 of 23 issues" while a filter is on, else "23 issues".</summary>
    [ObservableProperty]
    public partial string SummaryText { get; set; } = "";

    [ObservableProperty]
    public partial bool HasActiveFilters { get; set; }

    // ---- blocked ----

    /// <summary>Blocked issues in the sprint (the badge on the "Blocked" icon button).</summary>
    [ObservableProperty]
    public partial int BlockedCount { get; set; }

    /// <summary>Shows only blocked issues (toggled by the "Blocked" icon button).</summary>
    [ObservableProperty]
    public partial bool ShowBlockedOnly { get; set; }

    public string BlockedLabel => BlockedCount == 1 ? "1 blocked issue" : $"{BlockedCount} blocked issues";

    partial void OnBlockedCountChanged(int value) => OnPropertyChanged(nameof(BlockedLabel));

    partial void OnShowBlockedOnlyChanged(bool value) => Refresh();

    // ---- select mode ----

    [ObservableProperty]
    public partial bool IsSelectMode { get; set; }

    partial void OnIsSelectModeChanged(bool value)
    {
        if (!value) _selectedKeys.Clear();
        Refresh();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionText), nameof(HasSelection))]
    [NotifyCanExecuteChangedFor(nameof(MoveSelectedCommand), nameof(ClearSelectionCommand))]
    public partial IReadOnlyList<BoardCard> Selected { get; set; } = [];

    public bool HasSelection => Selected.Count > 0;

    public string SelectionText => Selected.Count switch
    {
        0 => "Select cards to move them together",
        1 => "1 card selected",
        var n => $"{n} cards selected",
    };

    // ---- lifecycle ----

    protected override async Task OnActivatedAsync(object? parameter, CancellationToken ct)
    {
        _issues.Changed += OnIssuesChanged;
        _sprints.Changed += OnDataChanged;
        _workspace.CurrentProjectChanged += OnProjectChanged;
        if (!_loaded)
        {
            IsLoading = true;
            NotifyStateShape();
            try
            {
                await _issues.ListAsync(ct);
                _loaded = true;
            }
            finally
            {
                IsLoading = false;
                NotifyStateShape();
            }
        }
        Refresh();
    }

    protected override void OnDeactivated()
    {
        _issues.Changed -= OnIssuesChanged;
        _sprints.Changed -= OnDataChanged;
        _workspace.CurrentProjectChanged -= OnProjectChanged;
    }

    void OnIssuesChanged(object? sender, IssuesChangedEventArgs e) => Refresh();

    void OnDataChanged(object? sender, EventArgs e) => Refresh();

    void OnProjectChanged(object? sender, EventArgs e)
    {
        _selectedKeys.Clear();
        Refresh();
    }

    void NotifyStateShape()
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(ShowBoard));
    }

    // ---- commands ----

    [RelayCommand]
    void ToggleFilter(BoardFilter filter)
    {
        filter.IsActive = !filter.IsActive;
        Refresh();
    }

    [RelayCommand]
    void ToggleBlocked() => ShowBlockedOnly = !ShowBlockedOnly;

    [RelayCommand]
    void ClearFilters()
    {
        foreach (var f in Filters) f.IsActive = false;
        if (ShowBlockedOnly) ShowBlockedOnly = false; // refreshes
        else Refresh();
    }

    [RelayCommand]
    Task OpenIssueAsync(string key) => _navigator.NavigateAsync(Routes.IssueDetail, key);

    [RelayCommand]
    Task GoToBacklogAsync() => _navigator.NavigateAsync(Routes.Backlog);

    /// <summary>Runs an entry of a card's overflow menu.</summary>
    [RelayCommand]
    Task RunCardActionAsync(BoardCardAction action) =>
        action.MoveTo is { } to ? MoveCardAsync(action.Card, to) : OpenIssueAsync(action.Card.Key);

    /// <summary>Moves one card; its target column shows a loading overlay while the change is saved.</summary>
    public Task MoveCardAsync(BoardCard card, IssueStatus to) =>
        card.Status == to ? Task.CompletedTask : MoveAsync([card.Issue], to);

    [RelayCommand(CanExecute = nameof(HasSelection))]
    async Task MoveSelectedAsync(IssueStatus to)
    {
        var list = Selected.Where(c => c.Status != to).Select(c => c.Issue).ToList();
        if (list.Count == 0)
        {
            _feedback.Toast(FeedbackKind.Info, $"Already in {to.Text()}");
            return;
        }
        if (await MoveAsync(list, to))
        {
            _selectedKeys.Clear();
            Refresh();
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    void ClearSelection()
    {
        _selectedKeys.Clear();
        Refresh();
    }

    async Task<bool> MoveAsync(IReadOnlyList<Issue> issues, IssueStatus to)
    {
        var column = Columns.First(c => c.Status == to);
        column.BusyText = issues.Count == 1 ? $"Moving {issues[0].Key} to {to.Text()}" : $"Moving {issues.Count} issues to {to.Text()}";
        column.IsBusy = true;
        try
        {
            await _issues.SetStatusAsync(issues, to);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _feedback.Toast(FeedbackKind.Error, issues.Count == 1 ? $"Couldn't move {issues[0].Key}" : "Couldn't move the issues", ex.Message);
            return false;
        }
        finally
        {
            column.IsBusy = false;
        }
        _feedback.Toast(FeedbackKind.Success, issues.Count == 1 ? $"{issues[0].Key} moved to {to.Text()}" : $"{issues.Count} issues moved to {to.Text()}");
        return true;
    }

    // ---- query ----

    /// <summary>The filters in effect, including the blocked toggle.</summary>
    public IReadOnlySet<BoardFilterKind> ActiveFilters()
    {
        var set = Filters.Where(f => f.IsActive).Select(f => f.Kind).ToHashSet();
        if (ShowBlockedOnly) set.Add(BoardFilterKind.Blocked);
        return set;
    }

    void OnCardSelectionChanged(BoardCard card)
    {
        if (_refreshing) return;
        if (card.IsSelected) _selectedKeys.Add(card.Key);
        else _selectedKeys.Remove(card.Key);
        Selected = Columns.SelectMany(c => c.Cards).Where(c => c.IsSelected).ToList();
    }

    void Refresh()
    {
        if (!_loaded) return;
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
        var project = _workspace.CurrentProject;
        var now = _clock.Now;
        ProjectName = project.Name;
        var sprint = _sprints.Active(project.Id);
        HasActiveSprint = sprint is not null;
        NotifyStateShape();

        var filters = ActiveFilters();
        HasActiveFilters = filters.Count > 0;

        if (sprint is null)
        {
            SprintName = "";
            SprintGoal = "";
            SprintText = "";
            HeaderText = $"{project.Name} has no sprint running.";
            EmptyText = _sprints.ForProject(project.Id).Any(s => s.State == SprintState.Planned)
                ? $"{project.Name} has a planned sprint. Start it from Backlog and sprints to see its work here."
                : $"{project.Name} doesn't use sprints yet. Plan one from Backlog and sprints to see its work here.";
            foreach (var column in Columns)
            {
                column.Cards = [];
                column.Count = 0;
                column.ShownCount = 0;
            }
            _selectedKeys.Clear();
            Selected = [];
            BlockedCount = 0;
            TotalCount = 0;
            ShownCount = 0;
            SummaryText = "";
            return;
        }

        SprintName = sprint.Name;
        SprintGoal = sprint.Goal;
        SprintText = $"{DateText(sprint.Start)} – {DateText(sprint.End)} · {DaysLeftText(sprint.End, now)}";
        HeaderText = $"{project.Name} · {sprint.Name}: {sprint.Goal}";

        var sprintIssues = BoardRules.SprintIssues(_issues.All, sprint.Id).ToList();
        BlockedCount = sprintIssues.Count(BoardRules.IsBlocked);
        _selectedKeys.IntersectWith(sprintIssues.Select(i => i.Key));

        var me = _people.Me.Id;
        var shown = 0;
        foreach (var column in Columns)
        {
            var all = sprintIssues.Where(i => i.Status == column.Status).ToList();
            var visible = BoardRules.CardOrder(all.Where(i => BoardRules.Matches(i, filters, me))).ToList();
            column.Count = all.Count;
            column.ShownCount = visible.Count;
            column.Cards = visible
                .Select(i => new BoardCard(IssueRow.From(i, _people, _projects, now), IsSelectMode, _selectedKeys.Contains(i.Key), OnCardSelectionChanged))
                .ToList();
            shown += visible.Count;
        }

        TotalCount = sprintIssues.Count;
        ShownCount = shown;
        SummaryText = HasActiveFilters
            ? $"Showing {shown} of {Plural(sprintIssues.Count, "issue")}"
            : Plural(sprintIssues.Count, "issue");
        // Selection only covers visible cards: filtering a card out deselects it.
        Selected = Columns.SelectMany(c => c.Cards).Where(c => c.IsSelected).ToList();
        _selectedKeys.IntersectWith(Selected.Select(c => c.Key));
    }

    static string DateText(DateTime d) => d.ToString("MMM d", CultureInfo.InvariantCulture);

    static string DaysLeftText(DateTime end, DateTime now)
    {
        var days = (end.Date - now.Date).Days;
        return days switch
        {
            < 0 => "ended",
            0 => "ends today",
            1 => "1 day left",
            _ => $"{days} days left",
        };
    }

    static string Plural(int n, string noun) => n == 1 ? $"1 {noun}" : $"{n} {noun}s";
}

/// <summary>An entry in the bulk move menu.</summary>
public sealed record BoardBulkTarget(IssueStatus Status, string Text);
