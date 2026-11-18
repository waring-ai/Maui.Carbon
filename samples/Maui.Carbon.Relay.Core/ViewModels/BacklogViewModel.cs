using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// Backlog and sprints for the current project: the running sprint (complete it), the planned sprint (start it), the
/// backlog grouped by epic, and the history of completed sprints, three per page. Refreshes when the project, the
/// issues or the sprints change.
/// </summary>
public sealed partial class BacklogViewModel : PageViewModel
{
    /// <summary>Completed sprints per history page.</summary>
    public const int HistoryPageSize = 3;

    readonly IIssueService _issues;
    readonly ISprintService _sprints;
    readonly IWorkspace _workspace;
    readonly INavigator _navigator;
    readonly IUserFeedback _feedback;
    readonly IClock _clock;
    IReadOnlyList<SprintHistoryRow> _history = [];
    bool _loaded;

    public BacklogViewModel(
        IIssueService issues,
        ISprintService sprints,
        IWorkspace workspace,
        INavigator navigator,
        IUserFeedback feedback,
        IClock clock)
    {
        _issues = issues;
        _sprints = sprints;
        _workspace = workspace;
        _navigator = navigator;
        _feedback = feedback;
        _clock = clock;
        Title = "Backlog and sprints";
    }

    [ObservableProperty]
    public partial string ProjectName { get; set; } = "";

    [ObservableProperty]
    public partial string HeaderText { get; set; } = "";

    public bool IsReady => !IsLoading && _loaded;

    // ---- active sprint ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActiveSprint))]
    [NotifyCanExecuteChangedFor(nameof(CompleteSprintCommand))]
    public partial BacklogSprint? ActiveSprint { get; set; }

    public bool HasActiveSprint => ActiveSprint is not null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CompleteSprintCommand))]
    public partial bool IsCompleting { get; set; }

    // ---- planned sprint ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPlannedSprint))]
    [NotifyCanExecuteChangedFor(nameof(StartSprintCommand))]
    public partial BacklogSprint? PlannedSprint { get; set; }

    public bool HasPlannedSprint => PlannedSprint is not null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSprintCommand))]
    public partial bool IsStarting { get; set; }

    /// <summary>What starting the planned sprint does (the callout beside "Start sprint").</summary>
    [ObservableProperty]
    public partial string StartCalloutTitle { get; set; } = "";

    [ObservableProperty]
    public partial string StartCalloutText { get; set; } = "";

    /// <summary>Why the last start failed; shown in an inline error notification until dismissed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStartError))]
    public partial string? StartError { get; set; }

    public bool HasStartError => !string.IsNullOrEmpty(StartError);

    /// <summary>Shown when there is neither a running nor a planned sprint.</summary>
    [ObservableProperty]
    public partial string NoSprintText { get; set; } = "";

    public bool HasNoSprints => IsReady && !HasActiveSprint && !HasPlannedSprint;

    partial void OnActiveSprintChanged(BacklogSprint? value) => OnPropertyChanged(nameof(HasNoSprints));

    partial void OnPlannedSprintChanged(BacklogSprint? value) => OnPropertyChanged(nameof(HasNoSprints));

    // ---- backlog tree ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBacklog))]
    public partial IReadOnlyList<BacklogBranch> Tree { get; set; } = [];

    public bool HasBacklog => Tree.Count > 0;

    /// <summary>"14 issues · 52 points, grouped by epic".</summary>
    [ObservableProperty]
    public partial string BacklogText { get; set; } = "";

    // ---- history ----

    /// <summary>The completed sprints on the current history page.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<SprintHistoryRow> HistoryItems { get; set; } = [];

    /// <summary>The history page, 0-based (two-way with the pagination nav).</summary>
    [ObservableProperty]
    public partial int HistoryPage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHistoryPages))]
    public partial int HistoryPageCount { get; set; } = 1;

    public bool HasHistoryPages => HistoryPageCount > 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHistory))]
    public partial int HistoryCount { get; set; }

    public bool HasHistory => HistoryCount > 0;

    /// <summary>"4 completed sprints · 89% of committed points completed on average".</summary>
    [ObservableProperty]
    public partial string HistoryText { get; set; } = "";

    partial void OnHistoryPageChanged(int value) => ShowHistoryPage();

    // ---- lifecycle ----

    protected override async Task OnActivatedAsync(object? parameter, CancellationToken ct)
    {
        _issues.Changed += OnIssuesChanged;
        _sprints.Changed += OnSprintsChanged;
        _workspace.CurrentProjectChanged += OnProjectChanged;
        if (!_loaded)
        {
            IsLoading = true;
            NotifyReady();
            try
            {
                await _issues.ListAsync(ct);
                _loaded = true;
            }
            finally
            {
                IsLoading = false;
                NotifyReady();
            }
        }
        Refresh();
    }

    protected override void OnDeactivated()
    {
        _issues.Changed -= OnIssuesChanged;
        _sprints.Changed -= OnSprintsChanged;
        _workspace.CurrentProjectChanged -= OnProjectChanged;
    }

    void OnIssuesChanged(object? sender, IssuesChangedEventArgs e) => Refresh();

    void OnSprintsChanged(object? sender, EventArgs e) => Refresh();

    void OnProjectChanged(object? sender, EventArgs e)
    {
        StartError = null;
        HistoryPage = 0;
        Refresh();
    }

    void NotifyReady()
    {
        OnPropertyChanged(nameof(IsReady));
        OnPropertyChanged(nameof(HasNoSprints));
    }

    // ---- commands ----

    bool CanComplete => HasActiveSprint && !IsCompleting;

    [RelayCommand(CanExecute = nameof(CanComplete))]
    async Task CompleteSprintAsync()
    {
        if (ActiveSprint is not { } active) return;
        var sprint = active.Sprint;
        var next = PlannedSprint?.Sprint;
        IsCompleting = true;
        try
        {
            await _sprints.CompleteAsync(sprint);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _feedback.Toast(FeedbackKind.Error, $"Couldn't complete {sprint.Name}", ex.Message);
            return;
        }
        finally
        {
            IsCompleting = false;
        }
        var carried = active.OpenCount == 0
            ? "Every issue was done."
            : $"{Plural(active.OpenCount, "open issue")} moved to {next?.Name ?? "the backlog"}.";
        _feedback.Toast(FeedbackKind.Success, $"{sprint.Name} completed", $"{sprint.CompletedPoints} of {active.PointsTotal} points done. {carried}");
    }

    bool CanStart => HasPlannedSprint && !IsStarting;

    [RelayCommand(CanExecute = nameof(CanStart))]
    async Task StartSprintAsync()
    {
        if (PlannedSprint is not { } planned) return;
        var sprint = planned.Sprint;
        StartError = null;
        IsStarting = true;
        try
        {
            await _sprints.StartAsync(sprint);
        }
        catch (InvalidOperationException ex)
        {
            StartError = ex.Message;
            return;
        }
        finally
        {
            IsStarting = false;
        }
        _feedback.Toast(FeedbackKind.Success, $"{sprint.Name} started", $"{Plural(planned.IssueCount, "issue")} committed, ends {sprint.End.ToString("MMM d", CultureInfo.InvariantCulture)}.");
    }

    [RelayCommand]
    void DismissStartError() => StartError = null;

    [RelayCommand]
    Task OpenIssueAsync(string key) => _navigator.NavigateAsync(Routes.IssueDetail, key);

    [RelayCommand]
    Task OpenBoardAsync() => _navigator.NavigateAsync(Routes.Board);

    // ---- query ----

    void Refresh()
    {
        if (!_loaded) return;
        var project = _workspace.CurrentProject;
        var now = _clock.Now;
        var issues = _issues.All.Where(i => i.ProjectId == project.Id).ToList();
        var sprints = _sprints.ForProject(project.Id);
        ProjectName = project.Name;

        var active = sprints.FirstOrDefault(s => s.State == SprintState.Active);
        var planned = sprints.Where(s => s.State == SprintState.Planned).OrderBy(s => s.Start).FirstOrDefault();
        ActiveSprint = active is null ? null : Summarize(active, issues, now);
        PlannedSprint = planned is null ? null : Summarize(planned, issues, now);
        if (planned is null) StartError = null;

        if (planned is not null && PlannedSprint is { } p)
        {
            var end = now.Date.AddDays(14).ToString("MMM d", CultureInfo.InvariantCulture);
            StartCalloutTitle = $"Starting {planned.Name} commits its work";
            StartCalloutText = active is null
                ? $"{p.IssuesText} ({p.PointsText}) move onto the board and a two-week timebox starts today, ending {end}. Open work at the end rolls over to the next planned sprint."
                : $"{p.IssuesText} ({p.PointsText}) move onto the board and a two-week timebox starts. Only one sprint runs at a time, so complete {active.Name} first.";
        }
        NoSprintText = $"{project.Name} has no running or planned sprint. Issues stay in the backlog below until a sprint is planned.";
        HeaderText = active is null
            ? $"{project.Name}: plan sprints, start them and look back at how they went."
            : $"{project.Name}: {active.Name} is running. Plan the next one from the backlog.";

        var names = sprints.ToDictionary(s => s.Id, s => s.Name, StringComparer.Ordinal);
        Tree = BacklogTree.Build(issues, active?.Id, id => names.GetValueOrDefault(id));
        var count = Tree.Sum(b => b.Items.Count);
        BacklogText = count == 0
            ? "Nothing waiting: every open issue is in the running sprint."
            : $"{Plural(count, "open issue")} · {Plural(Tree.Sum(b => b.Points), "point")}, grouped by epic. Open an issue to edit it.";

        _history = sprints.Where(s => s.State == SprintState.Completed).OrderByDescending(s => s.End).Select(s => new SprintHistoryRow(s)).ToList();
        HistoryCount = _history.Count;
        HistoryPageCount = Math.Max(1, (int)Math.Ceiling(_history.Count / (double)HistoryPageSize));
        var average = _history.Count == 0 ? 0 : (int)Math.Round(_history.Average(h => h.Percent));
        HistoryText = _history.Count == 0
            ? "No sprint has been completed yet."
            : $"{Plural(_history.Count, "completed sprint")} · {average}% of committed points completed on average";
        if (HistoryPage >= HistoryPageCount) HistoryPage = HistoryPageCount - 1; // shows the page
        else ShowHistoryPage();
    }

    void ShowHistoryPage()
    {
        var page = Math.Clamp(HistoryPage, 0, HistoryPageCount - 1);
        HistoryItems = _history.Skip(page * HistoryPageSize).Take(HistoryPageSize).ToList();
    }

    static BacklogSprint Summarize(Sprint sprint, IReadOnlyList<Issue> projectIssues, DateTime now)
    {
        var work = projectIssues.Where(i => i.SprintId == sprint.Id && i.Type != IssueType.Epic && i.Status != IssueStatus.Cancelled).ToList();
        var done = work.Where(i => i.Status == IssueStatus.Done).ToList();
        var time = sprint.State switch
        {
            SprintState.Active => (sprint.End.Date - now.Date).Days switch
            {
                < 0 => "Past its end date",
                0 => "Ends today",
                1 => "1 day left",
                var d => $"{d} days left",
            },
            _ => "Not started",
        };
        return new BacklogSprint(
            sprint,
            work.Count,
            done.Count,
            work.Sum(i => i.StoryPoints ?? 0),
            done.Sum(i => i.StoryPoints ?? 0),
            BacklogSprint.Dates(sprint.Start, sprint.End),
            time);
    }

    static string Plural(int n, string noun) => n == 1 ? $"1 {noun}" : $"{n} {noun}s";
}
