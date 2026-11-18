using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// One issue: header with resolve and overflow actions, tabs for details, comments, activity and attachments, and a side
/// column of fields that save as soon as they change. The parameter is the issue key. The screen follows
/// <see cref="IIssueService.Changed"/> for its key, so edits made elsewhere show up at once.
/// </summary>
public sealed partial class IssueDetailViewModel : PageViewModel
{
    /// <summary>The longest comment the text area accepts (its counter shows the limit).</summary>
    public const int CommentMaxLength = 1000;

    readonly IIssueService _issues;
    readonly IProjectService _projects;
    readonly IPeopleService _people;
    readonly IAssistantService _assistant;
    readonly INavigator _navigator;
    readonly IUserFeedback _feedback;
    readonly IClock _clock;

    /// <summary>True while fields are filled from the issue, so the change handlers do not save them back.</summary>
    bool _syncing;

    /// <summary>The assignee before the AI suggestion was applied (restored by revert).</summary>
    string? _assigneeBeforeAi;

    public IssueDetailViewModel(
        IIssueService issues,
        IProjectService projects,
        IPeopleService people,
        IAssistantService assistant,
        INavigator navigator,
        IUserFeedback feedback,
        IClock clock)
    {
        _issues = issues;
        _projects = projects;
        _people = people;
        _assistant = assistant;
        _navigator = navigator;
        _feedback = feedback;
        _clock = clock;
        Title = "Issue";
        StatusOptions = Enum.GetValues<IssueStatus>().Select(s => new StatusOption(s, s.Text())).ToList();
        AssigneeOptions = [new AssigneeOption(null, "Unassigned"), .. people.All.Select(p => new AssigneeOption(p.Id, p.Name))];
    }

    // ---- identity and state ----

    /// <summary>The key the screen was opened with (also shown when the issue is missing).</summary>
    [ObservableProperty]
    public partial string Key { get; set; } = "";

    [ObservableProperty]
    public partial Issue? Issue { get; set; }

    /// <summary>Loaded and found: the content shows.</summary>
    [ObservableProperty]
    public partial bool HasIssue { get; set; }

    /// <summary>Loaded and missing (wrong key, or deleted): the empty state shows.</summary>
    [ObservableProperty]
    public partial bool IsNotFound { get; set; }

    /// <summary>The finished-save task of the last field edit (tests await it; the app ignores it).</summary>
    public Task PendingSave { get; private set; } = Task.CompletedTask;

    // ---- header ----

    [ObservableProperty]
    public partial IssueType Type { get; set; }

    [ObservableProperty]
    public partial string TypeText { get; set; } = "";

    [ObservableProperty]
    public partial IssueStatus Status { get; set; }

    [ObservableProperty]
    public partial string StatusText { get; set; } = "";

    [ObservableProperty]
    public partial IssuePriority Priority { get; set; }

    [ObservableProperty]
    public partial string PriorityText { get; set; } = "";

    [ObservableProperty]
    public partial string ProjectName { get; set; } = "";

    /// <summary>"Atlas mobile · Sync component · customer-reported", under the title.</summary>
    [ObservableProperty]
    public partial string Subtitle { get; set; } = "";

    /// <summary>Closed issues show their resolution ("Won't fix") as a tag.</summary>
    [ObservableProperty]
    public partial bool IsResolved { get; set; }

    [ObservableProperty]
    public partial string ResolutionText { get; set; } = "";

    /// <summary>Open issues can be resolved.</summary>
    [ObservableProperty]
    public partial bool CanResolve { get; set; }

    [ObservableProperty]
    public partial bool IsWatching { get; set; }

    [ObservableProperty]
    public partial string WatchText { get; set; } = "Watch";

    // ---- details tab ----

    [ObservableProperty]
    public partial int SelectedTabIndex { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; } = "";

    [ObservableProperty]
    public partial IReadOnlyList<string> ReproSteps { get; set; } = [];

    [ObservableProperty]
    public partial bool HasReproSteps { get; set; }

    [ObservableProperty]
    public partial string StackTrace { get; set; } = "";

    [ObservableProperty]
    public partial bool HasStackTrace { get; set; }

    [ObservableProperty]
    public partial string Branch { get; set; } = "";

    [ObservableProperty]
    public partial bool HasBranch { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<IssueRow> LinkedIssues { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<IssueRow> ChildIssues { get; set; } = [];

    [ObservableProperty]
    public partial string LinkedTitle { get; set; } = "Linked issues";

    [ObservableProperty]
    public partial string ChildTitle { get; set; } = "Child issues";

    [ObservableProperty]
    public partial bool HasLinkedIssues { get; set; }

    [ObservableProperty]
    public partial bool HasChildIssues { get; set; }

    // ---- comments, activity, attachments ----

    [ObservableProperty]
    public partial IReadOnlyList<CommentRow> Comments { get; set; } = [];

    [ObservableProperty]
    public partial bool HasComments { get; set; }

    /// <summary>The comment count for the tab's secondary label.</summary>
    [ObservableProperty]
    public partial string CommentCountText { get; set; } = "0";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPostComment))]
    [NotifyCanExecuteChangedFor(nameof(PostCommentCommand))]
    public partial string NewComment { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPostComment))]
    [NotifyCanExecuteChangedFor(nameof(PostCommentCommand))]
    public partial bool IsPostingComment { get; set; }

    public bool CanPostComment =>
        !IsPostingComment && Issue is not null && NewComment.Trim().Length > 0 && NewComment.Length <= CommentMaxLength;

    [ObservableProperty]
    public partial IReadOnlyList<IssueActivityRow> Activity { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<AttachmentRow> Attachments { get; set; } = [];

    [ObservableProperty]
    public partial bool HasAttachments { get; set; }

    [ObservableProperty]
    public partial string AttachmentCountText { get; set; } = "0";

    // ---- side column fields ----

    public IReadOnlyList<StatusOption> StatusOptions { get; }

    public IReadOnlyList<AssigneeOption> AssigneeOptions { get; }

    [ObservableProperty]
    public partial StatusOption? SelectedStatus { get; set; }

    [ObservableProperty]
    public partial AssigneeOption? SelectedAssignee { get; set; }

    [ObservableProperty]
    public partial DateTime? DueDate { get; set; }

    /// <summary>Story points as the number input's value (whole numbers; empty clears them).</summary>
    [ObservableProperty]
    public partial double? StoryPoints { get; set; }

    [ObservableProperty]
    public partial string ReporterName { get; set; } = "";

    [ObservableProperty]
    public partial string CreatedText { get; set; } = "";

    [ObservableProperty]
    public partial string UpdatedText { get; set; } = "";

    [ObservableProperty]
    public partial string WatchersText { get; set; } = "";

    // ---- AI assignee suggestion ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SuggestedName), nameof(SuggestionReason), nameof(SuggestionConfidenceText), nameof(ApplySuggestionText))]
    public partial AssigneeSuggestion? Suggestion { get; set; }

    [ObservableProperty]
    public partial bool IsSuggesting { get; set; }

    /// <summary>A suggestion exists that differs from the current assignee and is not applied yet.</summary>
    [ObservableProperty]
    public partial bool HasSuggestion { get; set; }

    /// <summary>The assignee came from the AI suggestion: the field shows the AI treatment and the label offers revert.</summary>
    [ObservableProperty]
    public partial bool IsAssigneeAiFilled { get; set; }

    /// <summary>The AI label shows while there is a suggestion to explain or an applied one to revert.</summary>
    [ObservableProperty]
    public partial bool ShowAssigneeAiLabel { get; set; }

    public string SuggestedName => Suggestion is null ? "" : _people.NameOf(Suggestion.PersonId);

    public string SuggestionReason => Suggestion?.Reason ?? "";

    public string SuggestionConfidenceText => Suggestion is null ? "" : $"{(int)Math.Round(Suggestion.Confidence * 100)}% confidence";

    public string ApplySuggestionText => Suggestion is null ? "" : $"Assign to {SuggestedName}";

    // ---- dialogs ----

    [ObservableProperty]
    public partial bool IsMoveDialogOpen { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<ProjectOption> MoveProjectOptions { get; set; } = [];

    [ObservableProperty]
    public partial ProjectOption? MoveTarget { get; set; }

    [ObservableProperty]
    public partial bool IsDeleteConfirmOpen { get; set; }

    [ObservableProperty]
    public partial string DeleteConfirmText { get; set; } = "";

    // ---- lifecycle ----

    protected override async Task OnActivatedAsync(object? parameter, CancellationToken ct)
    {
        _issues.Changed -= OnIssuesChanged;
        _issues.Changed += OnIssuesChanged;
        Key = parameter as string ?? "";
        IsNotFound = false;
        HasIssue = false;
        IsLoading = true;
        var issue = Key.Length == 0 ? null : await _issues.GetAsync(Key, ct);
        IsLoading = false;
        Issue = issue;
        if (issue is null)
        {
            ShowNotFound();
            return;
        }
        Refresh();
        HasIssue = true;
        await LoadSuggestionAsync(issue, ct);
    }

    protected override void OnDeactivated()
    {
        _issues.Changed -= OnIssuesChanged;
        IsMoveDialogOpen = false;
        IsDeleteConfirmOpen = false;
    }

    void OnIssuesChanged(object? sender, IssuesChangedEventArgs e)
    {
        if (Issue is not { } issue || !e.Keys.Contains(issue.Key, StringComparer.OrdinalIgnoreCase)) return;
        if (_issues.Find(issue.Key) is null)
        {
            ShowNotFound();
            return;
        }
        Refresh();
    }

    void ShowNotFound()
    {
        Issue = null;
        HasIssue = false;
        IsNotFound = true;
        Title = "Issue not found";
        OnPropertyChanged(nameof(CanPostComment));
    }

    async Task LoadSuggestionAsync(Issue issue, CancellationToken ct)
    {
        IsSuggesting = true;
        try
        {
            Suggestion = await _assistant.SuggestAssigneeAsync(issue, ct);
        }
        finally
        {
            IsSuggesting = false;
        }
        UpdateSuggestion();
    }

    void UpdateSuggestion()
    {
        HasSuggestion = Suggestion is { } s && Issue is { } i && i.IsOpen && !IsAssigneeAiFilled && i.AssigneeId != s.PersonId;
        ShowAssigneeAiLabel = HasSuggestion || IsAssigneeAiFilled;
    }

    partial void OnIsAssigneeAiFilledChanged(bool value) => UpdateSuggestion();

    /// <summary>Copies the issue into the bound properties (field change handlers stay quiet meanwhile).</summary>
    void Refresh()
    {
        if (Issue is not { } i) return;
        var now = _clock.Now;
        var project = _projects.Find(i.ProjectId);

        Title = i.Title;
        Type = i.Type;
        TypeText = i.Type.Text();
        Status = i.Status;
        StatusText = i.Status.Text();
        Priority = i.Priority;
        PriorityText = $"{i.Priority.Text()} priority";
        ProjectName = project?.Name ?? i.ProjectId;
        Subtitle = string.Join(" · ", new[] { ProjectName, i.Component is null ? null : $"{i.Component} component", i.Labels.Count == 0 ? null : string.Join(", ", i.Labels) }.Where(s => s is not null));
        IsResolved = !i.IsOpen && i.Resolution != IssueResolution.None;
        ResolutionText = i.Resolution.Text();
        CanResolve = i.IsOpen;
        IsWatching = i.WatcherIds.Contains(_people.Me.Id);
        WatchText = IsWatching ? "Stop watching" : "Watch";

        Description = i.Description.Length == 0 ? "No description yet." : i.Description;
        ReproSteps = i.ReproSteps.ToList();
        HasReproSteps = ReproSteps.Count > 0;
        StackTrace = i.StackTrace ?? "";
        HasStackTrace = i.StackTrace is not null;
        Branch = i.Branch ?? "";
        HasBranch = i.Branch is not null;

        LinkedIssues = i.LinkedKeys.Select(_issues.Find).OfType<Issue>().Select(x => IssueRow.From(x, _people, _projects, now)).ToList();
        ChildIssues = _issues.All.Where(x => string.Equals(x.ParentKey, i.Key, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Status).ThenBy(x => x.Id)
            .Select(x => IssueRow.From(x, _people, _projects, now)).ToList();
        HasLinkedIssues = LinkedIssues.Count > 0;
        HasChildIssues = ChildIssues.Count > 0;
        LinkedTitle = $"Linked issues ({LinkedIssues.Count})";
        ChildTitle = $"Child issues ({ChildIssues.Count})";

        Comments = i.Comments.OrderBy(c => c.Created).Select(c =>
        {
            var author = _people.Find(c.AuthorId);
            return new CommentRow(c.Id, author?.Name ?? "Former member", author?.Initials ?? "?", c.Body, Formatting.Relative(c.Created, now), c.AuthorId == _people.Me.Id);
        }).ToList();
        HasComments = Comments.Count > 0;
        CommentCountText = Comments.Count.ToString(CultureInfo.InvariantCulture);

        var events = i.Activity.OrderByDescending(a => a.When).ToList();
        Activity = events.Select((a, n) => new IssueActivityRow(_people.NameOf(a.ActorId), a.Text, a.Kind, Formatting.Relative(a.When, now), n == events.Count - 1)).ToList();

        Attachments = i.Attachments.OrderByDescending(a => a.Uploaded)
            .Select(a => new AttachmentRow(a.Name, a.SizeText, _people.NameOf(a.UploaderId), Formatting.Relative(a.Uploaded, now))).ToList();
        HasAttachments = Attachments.Count > 0;
        AttachmentCountText = Attachments.Count.ToString(CultureInfo.InvariantCulture);

        ReporterName = _people.NameOf(i.ReporterId);
        CreatedText = Formatting.ShortDate(i.Created);
        UpdatedText = Formatting.Relative(i.Updated, now);
        WatchersText = i.WatcherIds.Count == 1 ? "1 watcher" : $"{i.WatcherIds.Count} watchers";

        _syncing = true;
        try
        {
            SelectedStatus = StatusOptions.First(o => o.Status == i.Status);
            SelectedAssignee = AssigneeOptions.FirstOrDefault(o => o.Id == i.AssigneeId) ?? AssigneeOptions[0];
            DueDate = i.DueDate;
            StoryPoints = i.StoryPoints;
        }
        finally
        {
            _syncing = false;
        }
        UpdateSuggestion();
        OnPropertyChanged(nameof(CanPostComment));
        PostCommentCommand.NotifyCanExecuteChanged();
    }

    // ---- field edits: each saves at once and confirms with a toast ----

    partial void OnSelectedStatusChanged(StatusOption? value)
    {
        if (_syncing || value is null || Issue is not { } i || i.Status == value.Status) return;
        PendingSave = SaveAsync(async () =>
        {
            await _issues.SetStatusAsync([i], value.Status);
            _feedback.Toast(FeedbackKind.Success, $"{i.Key} moved to {value.Text}");
        });
    }

    partial void OnSelectedAssigneeChanged(AssigneeOption? value)
    {
        if (_syncing || value is null || Issue is not { } i || i.AssigneeId == value.Id) return;
        // A manual pick replaces the AI value, so there is nothing left to revert.
        IsAssigneeAiFilled = false;
        PendingSave = SaveAsync(async () =>
        {
            await _issues.AssignAsync([i], value.Id);
            _feedback.Toast(FeedbackKind.Success, value.Id is null ? $"{i.Key} is now unassigned" : $"{i.Key} assigned to {value.Name}");
        });
    }

    partial void OnDueDateChanged(DateTime? value)
    {
        var date = value?.Date;
        if (_syncing || Issue is not { } i || i.DueDate?.Date == date) return;
        PendingSave = SaveAsync(async () =>
        {
            await _issues.UpdateAsync(i, x => x.DueDate = date, date is null ? "removed the due date" : $"set the due date to {Formatting.ShortDate(date)}");
            _feedback.Toast(FeedbackKind.Success, date is null ? "Due date removed" : $"Due date set to {Formatting.ShortDate(date)}");
        });
    }

    partial void OnStoryPointsChanged(double? value)
    {
        int? points = value is null ? null : (int)Math.Round(Math.Max(0, value.Value));
        if (_syncing || Issue is not { } i || i.StoryPoints == points) return;
        PendingSave = SaveAsync(async () =>
        {
            await _issues.UpdateAsync(i, x => x.StoryPoints = points, points is null ? "cleared the story points" : $"changed the story points to {points}");
            _feedback.Toast(FeedbackKind.Success, points is null ? "Story points cleared" : $"Story points set to {points}");
        });
    }

    async Task SaveAsync(Func<Task> save)
    {
        try
        {
            await save();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _feedback.Toast(FeedbackKind.Error, "The change was not saved", ex.Message);
            Refresh();
        }
    }

    // ---- AI suggestion ----

    [RelayCommand]
    async Task ApplySuggestionAsync()
    {
        if (Issue is not { } i || Suggestion is not { } s || i.AssigneeId == s.PersonId) return;
        _assigneeBeforeAi = i.AssigneeId;
        await _issues.AssignAsync([i], s.PersonId);
        IsAssigneeAiFilled = true;
        _feedback.Toast(FeedbackKind.Success, $"{i.Key} assigned to {SuggestedName}", "Suggested by Relay AI. Revert from the AI label to undo.");
    }

    [RelayCommand]
    async Task RevertAssigneeAsync()
    {
        if (Issue is not { } i || !IsAssigneeAiFilled) return;
        IsAssigneeAiFilled = false;
        await _issues.AssignAsync([i], _assigneeBeforeAi);
        _feedback.Toast(FeedbackKind.Info, _assigneeBeforeAi is null ? $"{i.Key} is unassigned again" : $"Assignee restored to {_people.NameOf(_assigneeBeforeAi)}");
    }

    // ---- header actions ----

    /// <summary>The breadcrumb's "Issues" and the not-found state: back to where the user came from.</summary>
    [RelayCommand]
    async Task GoBackAsync()
    {
        if (_navigator.CanGoBack) await _navigator.GoBackAsync();
        else await _navigator.NavigateAsync(Routes.Issues);
    }

    [RelayCommand]
    Task OpenProjectsAsync() => _navigator.NavigateAsync(Routes.Projects);

    [RelayCommand]
    Task OpenIssueAsync(IssueRow row) => _navigator.NavigateAsync(Routes.IssueDetail, row.Key);

    /// <summary>Resolves the issue; <see cref="IssueResolution.Fixed"/> is the combo button's main action.</summary>
    [RelayCommand]
    async Task ResolveAsync(IssueResolution resolution)
    {
        if (Issue is not { } i || !i.IsOpen) return;
        await _issues.ResolveAsync(i, resolution);
        _feedback.Toast(FeedbackKind.Success, $"{i.Key} resolved as {resolution.Text()}",
            resolution == IssueResolution.Fixed ? "Moved to Done." : "Moved to Cancelled.");
    }

    [RelayCommand]
    async Task ToggleWatchAsync()
    {
        if (Issue is not { } i) return;
        await _issues.ToggleWatchAsync(i);
        var watching = i.WatcherIds.Contains(_people.Me.Id);
        _feedback.Toast(FeedbackKind.Info, watching ? $"Watching {i.Key}" : $"Stopped watching {i.Key}",
            watching ? "You'll be notified about comments and status changes." : null);
    }

    [RelayCommand]
    void OpenMoveDialog()
    {
        if (Issue is not { } i) return;
        MoveProjectOptions = _projects.All.Where(p => !p.IsArchived && p.Id != i.ProjectId).Select(p => new ProjectOption(p.Id, p.Name)).ToList();
        MoveTarget = MoveProjectOptions.FirstOrDefault();
        IsMoveDialogOpen = true;
    }

    [RelayCommand]
    async Task ConfirmMoveAsync()
    {
        IsMoveDialogOpen = false;
        if (Issue is not { } i || MoveTarget is not { Id: { } targetId } || _projects.Find(targetId) is not { } project) return;
        await _issues.UpdateAsync(i, x =>
        {
            x.ProjectId = project.Id;
            if (x.Component is not null && !project.Components.Contains(x.Component)) x.Component = null;
            x.SprintId = null;
        }, $"moved it to {project.Name}");
        _feedback.Toast(FeedbackKind.Success, $"{i.Key} moved to {project.Name}", "It left its sprint. The key stays the same, so links keep working.");
    }

    [RelayCommand]
    void CancelMove() => IsMoveDialogOpen = false;

    [RelayCommand]
    void RequestDelete()
    {
        if (Issue is not { } i) return;
        DeleteConfirmText = i.Comments.Count == 0
            ? $"{i.Key} \"{i.Title}\" will be deleted for everyone."
            : $"{i.Key} \"{i.Title}\" and its {i.Comments.Count} comments will be deleted for everyone.";
        IsDeleteConfirmOpen = true;
    }

    [RelayCommand]
    async Task ConfirmDeleteAsync()
    {
        IsDeleteConfirmOpen = false;
        if (Issue is not { } i) return;
        await _issues.DeleteAsync([i]);
        _feedback.Toast(FeedbackKind.Success, $"{i.Key} deleted");
        await GoBackAsync();
    }

    [RelayCommand]
    void CancelDelete() => IsDeleteConfirmOpen = false;

    // ---- comments and attachments ----

    [RelayCommand(CanExecute = nameof(CanPostComment))]
    async Task PostCommentAsync()
    {
        if (Issue is not { } i) return;
        IsPostingComment = true;
        try
        {
            await _issues.AddCommentAsync(i, NewComment);
            NewComment = "";
            _feedback.Toast(FeedbackKind.Success, "Comment posted", $"Watchers of {i.Key} were notified.");
        }
        finally
        {
            IsPostingComment = false;
        }
    }

    /// <summary>Uploads one file picked or dropped on the attachments tab.</summary>
    public async Task AddAttachmentAsync(string fileName, long sizeBytes)
    {
        if (Issue is not { } i) return;
        await _issues.AddAttachmentAsync(i, fileName, sizeBytes);
        _feedback.Toast(FeedbackKind.Success, $"{fileName} attached", $"Added to {i.Key}.");
    }

    [RelayCommand]
    async Task RemoveAttachmentAsync(string fileName)
    {
        if (Issue is not { } i) return;
        await _issues.RemoveAttachmentAsync(i, fileName);
        _feedback.Toast(FeedbackKind.Success, $"{fileName} removed");
    }
}
