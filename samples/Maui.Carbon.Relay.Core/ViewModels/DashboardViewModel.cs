using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// The landing screen after sign-in, scoped to the workspace's current project: a greeting, KPI tiles that lead into
/// the issue list, sprint progress with velocity, a "sprint ends soon" notice, the signed-in user's top open issues,
/// the project's recent activity and an AI summary of the active sprint. Refreshes when the project or any issue
/// changes.
/// </summary>
public sealed partial class DashboardViewModel : PageViewModel
{
    /// <summary>How many of my issues "My work" lists.</summary>
    public const int MyWorkCount = 5;

    /// <summary>How many activity entries "Recent activity" lists.</summary>
    public const int ActivityCount = 6;

    /// <summary>The sprint-ends-soon notice shows when the active sprint ends within this many days.</summary>
    public const int EndingSoonDays = 3;

    readonly IIssueService _issues;
    readonly IPeopleService _people;
    readonly IProjectService _projects;
    readonly ISprintService _sprints;
    readonly IWorkspace _workspace;
    readonly IAssistantService _assistant;
    readonly INavigator _navigator;
    readonly IClock _clock;
    CancellationTokenSource? _summaryCts;
    string? _summaryProjectId;
    string? _noticeDismissedFor;
    bool _loaded;

    public DashboardViewModel(
        IIssueService issues,
        IPeopleService people,
        IProjectService projects,
        ISprintService sprints,
        IWorkspace workspace,
        IAssistantService assistant,
        INavigator navigator,
        IClock clock)
    {
        _issues = issues;
        _people = people;
        _projects = projects;
        _sprints = sprints;
        _workspace = workspace;
        _assistant = assistant;
        _navigator = navigator;
        _clock = clock;
        Title = Greet(clock.Now, people.Me);
    }

    // ---- header ----

    [ObservableProperty]
    public partial string ProjectName { get; set; } = "";

    /// <summary>"Atlas mobile · Sprint 23", or the project alone when no sprint is running.</summary>
    [ObservableProperty]
    public partial string ContextText { get; set; } = "";

    [ObservableProperty]
    public partial string SprintGoalText { get; set; } = "";

    // ---- KPI tiles ----

    [ObservableProperty]
    public partial int MyOpenCount { get; set; }

    [ObservableProperty]
    public partial string MyOpenHelper { get; set; } = "";

    [ObservableProperty]
    public partial int DueThisWeekCount { get; set; }

    [ObservableProperty]
    public partial string DueThisWeekHelper { get; set; } = "";

    [ObservableProperty]
    public partial int InReviewCount { get; set; }

    [ObservableProperty]
    public partial string InReviewHelper { get; set; } = "";

    [ObservableProperty]
    public partial int PointsDone { get; set; }

    [ObservableProperty]
    public partial int PointsTotal { get; set; }

    [ObservableProperty]
    public partial string PointsText { get; set; } = "";

    [ObservableProperty]
    public partial string PointsHelper { get; set; } = "";

    // ---- sprint ----

    [ObservableProperty]
    public partial bool HasActiveSprint { get; set; }

    [ObservableProperty]
    public partial string SprintName { get; set; } = "";

    /// <summary>Done points as a percentage 0..100 (the progress bar value).</summary>
    [ObservableProperty]
    public partial double SprintPercent { get; set; }

    [ObservableProperty]
    public partial string SprintProgressHelper { get; set; } = "";

    [ObservableProperty]
    public partial int DaysLeft { get; set; }

    [ObservableProperty]
    public partial string DaysLeftText { get; set; } = "";

    [ObservableProperty]
    public partial string SprintDatesText { get; set; } = "";

    /// <summary>Open issues in the active sprint ("12").</summary>
    [ObservableProperty]
    public partial string OpenInSprintText { get; set; } = "";

    /// <summary>"of 13 issues".</summary>
    [ObservableProperty]
    public partial string OpenInSprintHelper { get; set; } = "";

    /// <summary>"Ends Nov 20".</summary>
    [ObservableProperty]
    public partial string SprintEndText { get; set; } = "";

    /// <summary>Mean completed points of the project's completed sprints (<see cref="ReportMath.Velocity"/>).</summary>
    [ObservableProperty]
    public partial double Velocity { get; set; }

    /// <summary>"28 points", or a dash before any sprint has finished.</summary>
    [ObservableProperty]
    public partial string VelocityText { get; set; } = "";

    /// <summary>"Per sprint, last 4 sprints".</summary>
    [ObservableProperty]
    public partial string VelocityHelper { get; set; } = "";

    public string VelocityDefinition =>
        "The average story points the team completed per sprint over its finished sprints. Use it to judge how much work fits in the next sprint.";

    [ObservableProperty]
    public partial string NoSprintText { get; set; } = "";

    // ---- sprint ends soon ----

    [ObservableProperty]
    public partial bool IsSprintEndingSoon { get; set; }

    [ObservableProperty]
    public partial string SprintEndingTitle { get; set; } = "";

    [ObservableProperty]
    public partial string SprintEndingSubtitle { get; set; } = "";

    // ---- lists ----

    /// <summary>My top open issues in the project: highest priority first, then soonest due.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMyWork))]
    public partial IReadOnlyList<IssueRow> MyWork { get; set; } = [];

    public bool HasMyWork => MyWork.Count > 0;

    [ObservableProperty]
    public partial IReadOnlyList<ActivityRow> Activity { get; set; } = [];

    // ---- AI sprint summary ----

    /// <summary>True while the assistant writes the sprint summary (the tile shows AI skeletons).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegenerateSummaryCommand))]
    public partial bool IsSummarizing { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSummary))]
    public partial string SummaryText { get; set; } = "";

    public bool HasSummary => SummaryText.Length > 0;

    [ObservableProperty]
    public partial IReadOnlyList<string> SummaryHighlights { get; set; } = [];

    /// <summary>Shown under the summary: when it was generated.</summary>
    [ObservableProperty]
    public partial string SummaryCaption { get; set; } = "";

    // ---- lifecycle ----

    protected override async Task OnActivatedAsync(object? parameter, CancellationToken ct)
    {
        _issues.Changed += OnIssuesChanged;
        _sprints.Changed += OnSprintsChanged;
        _workspace.CurrentProjectChanged += OnProjectChanged;
        Title = Greet(_clock.Now, _people.Me);
        if (!_loaded)
        {
            IsLoading = true;
            await _issues.ListAsync(ct);
            _loaded = true;
            IsLoading = false;
        }
        Refresh();
        if (_summaryProjectId != _workspace.CurrentProject.Id || !HasSummary) await SummarizeAsync();
    }

    protected override void OnDeactivated()
    {
        _issues.Changed -= OnIssuesChanged;
        _sprints.Changed -= OnSprintsChanged;
        _workspace.CurrentProjectChanged -= OnProjectChanged;
        _summaryCts?.Cancel();
    }

    void OnIssuesChanged(object? sender, IssuesChangedEventArgs e) => Refresh();

    void OnSprintsChanged(object? sender, EventArgs e) => Refresh();

    async void OnProjectChanged(object? sender, EventArgs e)
    {
        Refresh();
        try
        {
            await SummarizeAsync();
        }
        catch (OperationCanceledException)
        {
            // Left the screen or switched again before the summary finished.
        }
    }

    // ---- commands ----

    /// <summary>KPI tiles: open the issue list in the given scope.</summary>
    [RelayCommand]
    Task OpenIssues(IssueScope scope) => _navigator.NavigateAsync(Routes.Issues, scope);

    [RelayCommand]
    Task OpenIssue(string key) => _navigator.NavigateAsync(Routes.IssueDetail, key);

    [RelayCommand]
    Task ReviewSprint() => _navigator.NavigateAsync(Routes.Backlog);

    [RelayCommand]
    Task CreateIssue() => _navigator.NavigateAsync(Routes.CreateIssue);

    [RelayCommand]
    void DismissSprintNotice()
    {
        _noticeDismissedFor = _sprints.Active(_workspace.CurrentProject.Id)?.Id;
        IsSprintEndingSoon = false;
    }

    bool CanRegenerate => !IsSummarizing;

    [RelayCommand(CanExecute = nameof(CanRegenerate))]
    Task RegenerateSummary() => SummarizeAsync();

    // ---- summary ----

    async Task SummarizeAsync()
    {
        _summaryCts?.Cancel();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(Lifetime);
        _summaryCts = cts;
        var projectId = _workspace.CurrentProject.Id;
        IsSummarizing = true;
        try
        {
            var result = await _assistant.SummarizeSprintAsync(projectId, cts.Token);
            if (cts.IsCancellationRequested) return;
            SummaryText = result.Text;
            SummaryHighlights = result.Highlights;
            SummaryCaption = $"Generated {_clock.Now.ToString("h:mm tt", CultureInfo.InvariantCulture)} from {result.IssueKeys.Count switch { 0 => "the sprint's issues", _ => $"{result.IssueKeys.Count} flagged issues and the sprint's progress" }}";
            _summaryProjectId = projectId;
        }
        finally
        {
            if (ReferenceEquals(_summaryCts, cts)) IsSummarizing = false;
        }
    }

    // ---- derived numbers ----

    void Refresh()
    {
        if (!_loaded) return;
        var now = _clock.Now;
        var today = now.Date;
        var project = _workspace.CurrentProject;
        var me = _people.Me.Id;
        var inProject = _issues.All.Where(i => i.ProjectId == project.Id).ToList();
        var mine = inProject.Where(i => i.IsOpen && i.AssigneeId == me).ToList();

        ProjectName = project.Name;

        MyOpenCount = mine.Count;
        var highOrAbove = mine.Count(i => i.Priority >= IssuePriority.High);
        MyOpenHelper = highOrAbove == 0 ? $"In {project.Name}" : $"{highOrAbove} high priority or above";

        var dueSoon = mine.Where(i => i.DueDate is { } d && d.Date <= today.AddDays(6)).ToList();
        var overdue = dueSoon.Count(i => i.IsOverdue(now));
        DueThisWeekCount = dueSoon.Count;
        DueThisWeekHelper = overdue > 0 ? $"{overdue} overdue" : "Next 7 days";

        var inReview = inProject.Where(i => i.Status == IssueStatus.InReview).ToList();
        InReviewCount = inReview.Count;
        var reviewMine = inReview.Count(i => i.AssigneeId == me);
        InReviewHelper = reviewMine == 0 ? "Across the team" : $"{reviewMine} of them yours";

        var sprint = _sprints.Active(project.Id);
        HasActiveSprint = sprint is not null;
        Velocity = ReportMath.Velocity(_sprints.ForProject(project.Id));
        var finished = _sprints.ForProject(project.Id).Count(s => s.State == SprintState.Completed);
        VelocityText = Velocity <= 0 ? "—" : $"{Velocity:0} points";
        VelocityHelper = finished switch { 0 => "No finished sprints yet", 1 => "Per sprint, last sprint", _ => $"Per sprint, last {finished} sprints" };
        if (sprint is null)
        {
            ContextText = project.Name;
            SprintGoalText = "No sprint is running. Plan the next one from the backlog.";
            SprintName = "";
            PointsDone = PointsTotal = 0;
            PointsText = "—";
            PointsHelper = "No active sprint";
            SprintPercent = 0;
            DaysLeft = 0;
            DaysLeftText = SprintDatesText = OpenInSprintText = OpenInSprintHelper = SprintEndText = SprintProgressHelper = "";
            NoSprintText = $"{project.Name} has no active sprint. Start one from the backlog to track progress here.";
            IsSprintEndingSoon = false;
        }
        else
        {
            var sprintIssues = inProject.Where(i => i.SprintId == sprint.Id).ToList();
            var open = sprintIssues.Count(i => i.IsOpen);
            PointsTotal = sprintIssues.Sum(i => i.StoryPoints ?? 0);
            PointsDone = sprintIssues.Where(i => i.Status == IssueStatus.Done).Sum(i => i.StoryPoints ?? 0);
            var fraction = ReportMath.SprintProgress(sprintIssues);
            SprintPercent = Math.Round(fraction * 100, 1);
            PointsText = $"{PointsDone} / {PointsTotal}";
            PointsHelper = $"{fraction:P0} of {sprint.Name} points";
            SprintName = sprint.Name;
            ContextText = $"{project.Name} · {sprint.Name}";
            SprintGoalText = string.IsNullOrWhiteSpace(sprint.Goal) ? $"{sprint.Name} has no goal yet." : $"Sprint goal: {sprint.Goal}";
            DaysLeft = Math.Max(0, (sprint.End.Date - today).Days);
            DaysLeftText = DaysLeft switch { 0 => "Ends today", 1 => "1 day left", _ => $"{DaysLeft} days left" };
            SprintDatesText = $"{sprint.Start.ToString("MMM d", CultureInfo.InvariantCulture)} – {sprint.End.ToString("MMM d", CultureInfo.InvariantCulture)}";
            SprintProgressHelper = $"{PointsDone} of {PointsTotal} points done";
            OpenInSprintText = open.ToString(CultureInfo.InvariantCulture);
            OpenInSprintHelper = sprintIssues.Count == 1 ? "of 1 issue" : $"of {sprintIssues.Count} issues";
            SprintEndText = $"Ends {sprint.End.ToString("MMM d", CultureInfo.InvariantCulture)}";
            NoSprintText = "";
            IsSprintEndingSoon = DaysLeft <= EndingSoonDays && open > 0 && _noticeDismissedFor != sprint.Id;
            SprintEndingTitle = DaysLeft switch
            {
                0 => $"{sprint.Name} ends today",
                1 => $"{sprint.Name} ends tomorrow",
                _ => $"{sprint.Name} ends in {DaysLeft} days",
            };
            SprintEndingSubtitle = $"{(open == 1 ? "1 issue is" : $"{open} issues are")} still open.";
        }
        PointsAreForSprint = sprint is not null;

        MyWork = mine
            .OrderByDescending(i => i.Priority)
            .ThenBy(i => i.DueDate ?? DateTime.MaxValue)
            .ThenBy(i => i.Key, StringComparer.Ordinal)
            .Take(MyWorkCount)
            .Select(i => IssueRow.From(i, _people, _projects, now))
            .ToList();

        var activity = inProject
            .SelectMany(i => i.Activity)
            .OrderByDescending(e => e.When)
            .ThenBy(e => e.Id, StringComparer.Ordinal)
            .Take(ActivityCount)
            .Select(e => ActivityRow.From(e, _people, now))
            .ToList();
        // Rows are values: an unchanged stream keeps its list, so the view does not rebuild it on every visit.
        if (!Activity.SequenceEqual(activity)) Activity = activity;
    }

    /// <summary>True when the points tile shows a running sprint (false shows "No active sprint").</summary>
    [ObservableProperty]
    public partial bool PointsAreForSprint { get; set; }

    /// <summary>"Good morning, Maya" / afternoon / evening by the hour.</summary>
    public static string Greet(DateTime now, Person me)
    {
        var part = now.Hour switch { < 12 => "morning", < 18 => "afternoon", _ => "evening" };
        return $"Good {part}, {me.Name.Split(' ')[0]}";
    }
}
