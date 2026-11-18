using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// The create-issue wizard: Type → Details → Schedule → Review over an <see cref="IssueDraft"/>, validated per step by
/// <see cref="WizardValidation"/>. A field shows its message once it was touched (left, changed, or its step was
/// submitted with Next); errors block Next, warnings do not. Completed steps can be revisited. Cancelling a dirty draft
/// asks first.
/// </summary>
public sealed partial class CreateIssueViewModel : PageViewModel
{
    public const int TypeStep = 0;
    public const int DetailsStep = 1;
    public const int ScheduleStep = 2;
    public const int ReviewStep = 3;

    static readonly string[][] StepFields =
    [
        ["type", "project"],
        ["title", "description", "labels"],
        ["dates", "points", "reminder"],
        [],
    ];

    readonly IIssueService _issues;
    readonly IProjectService _projects;
    readonly IPeopleService _people;
    readonly INavigator _navigator;
    readonly IUserFeedback _feedback;
    readonly IClock _clock;
    readonly HashSet<string> _touched = [];

    public CreateIssueViewModel(
        IIssueService issues,
        IProjectService projects,
        IPeopleService people,
        IWorkspace workspace,
        INavigator navigator,
        IUserFeedback feedback,
        IClock clock)
    {
        _issues = issues;
        _projects = projects;
        _people = people;
        _navigator = navigator;
        _feedback = feedback;
        _clock = clock;
        Title = "Create issue";
        ProjectOptions = projects.All.Where(p => !p.IsArchived).Select(p => new ProjectOption(p.Id, p.Name)).ToList();
        AssigneeOptions = [new AssigneeOption(null, "Unassigned"), .. people.All.Select(p => new AssigneeOption(p.Id, p.Name))];
        AllLabels = issues.Labels;
        SelectedAssignee = AssigneeOptions[0];
        SelectedProject = ProjectOptions.FirstOrDefault(p => p.Id == workspace.CurrentProject.Id) ?? ProjectOptions.FirstOrDefault();
        ReminderTimeZone = TimeZones[0];
        UpdateStep();
        UpdateValidation();
    }

    /// <summary>The working copy (shared with <see cref="WizardValidation"/>; exposed for tests).</summary>
    public IssueDraft Draft { get; } = new();

    DateTime Today => _clock.Now.Date;

    // ---- steps ----

    public IReadOnlyList<string> StepLabels { get; } = ["Type", "Details", "Schedule", "Review"];

    [ObservableProperty]
    public partial int StepIndex { get; set; }

    [ObservableProperty]
    public partial bool IsTypeStep { get; set; }

    [ObservableProperty]
    public partial bool IsDetailsStep { get; set; }

    [ObservableProperty]
    public partial bool IsScheduleStep { get; set; }

    [ObservableProperty]
    public partial bool IsReviewStep { get; set; }

    [ObservableProperty]
    public partial bool CanGoBack { get; set; }

    /// <summary>"Next", or "Create issue" on the review step.</summary>
    [ObservableProperty]
    public partial string PrimaryActionText { get; set; } = "Next";

    [ObservableProperty]
    public partial string StepHeading { get; set; } = "";

    [ObservableProperty]
    public partial string StepDescription { get; set; } = "";

    /// <summary>Per step: has a touched field with an error (the progress indicator marks it invalid).</summary>
    [ObservableProperty]
    public partial IReadOnlyList<bool> InvalidSteps { get; set; } = [false, false, false, false];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand), nameof(BackCommand), nameof(CancelCommand))]
    public partial bool IsCreating { get; set; }

    // ---- step 1: type and project ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ArePointsEnabled))]
    public partial IssueType? SelectedType { get; set; }

    public IReadOnlyList<ProjectOption> ProjectOptions { get; }

    [ObservableProperty]
    public partial ProjectOption? SelectedProject { get; set; }

    [ObservableProperty]
    public partial string TypeErrorText { get; set; } = "";

    [ObservableProperty]
    public partial bool HasTypeError { get; set; }

    [ObservableProperty]
    public partial bool IsProjectInvalid { get; set; }

    [ObservableProperty]
    public partial string ProjectInvalidText { get; set; } = "";

    // ---- step 2: details ----

    public int SummaryMaxLength => WizardValidation.TitleMaxLength;

    [ObservableProperty]
    public partial string Summary { get; set; } = "";

    [ObservableProperty]
    public partial bool IsSummaryInvalid { get; set; }

    [ObservableProperty]
    public partial string SummaryInvalidText { get; set; } = "";

    [ObservableProperty]
    public partial string Description { get; set; } = "";

    [ObservableProperty]
    public partial bool IsDescriptionWarning { get; set; }

    [ObservableProperty]
    public partial string DescriptionWarnText { get; set; } = "";

    /// <summary>The selected project's components (the combo box also takes a new one).</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> ComponentOptions { get; set; } = [];

    /// <summary>The combo box text: a listed component or a new one.</summary>
    [ObservableProperty]
    public partial string ComponentText { get; set; } = "";

    public IReadOnlyList<string> AllLabels { get; }

    [ObservableProperty]
    public partial IReadOnlyList<string> SelectedLabels { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLabelsInvalid { get; set; }

    [ObservableProperty]
    public partial string LabelsInvalidText { get; set; } = "";

    public IReadOnlyList<IssuePriority> Priorities { get; } = Enum.GetValues<IssuePriority>().Reverse().ToList();

    [ObservableProperty]
    public partial IssuePriority Priority { get; set; } = IssuePriority.Medium;

    public IReadOnlyList<AssigneeOption> AssigneeOptions { get; }

    [ObservableProperty]
    public partial AssigneeOption? SelectedAssignee { get; set; }

    // ---- step 3: schedule ----

    public int MinPoints => 1;

    public int MaxPoints => 21;

    [ObservableProperty]
    public partial double StoryPoints { get; set; } = 3;

    /// <summary>Epics are sized by their children, so the estimate is off for them.</summary>
    public bool ArePointsEnabled => SelectedType != IssueType.Epic;

    [ObservableProperty]
    public partial bool IsPointsWarning { get; set; }

    [ObservableProperty]
    public partial string PointsWarnText { get; set; } = "";

    [ObservableProperty]
    public partial DateTime? StartDate { get; set; }

    [ObservableProperty]
    public partial DateTime? DueDate { get; set; }

    [ObservableProperty]
    public partial bool IsDatesInvalid { get; set; }

    [ObservableProperty]
    public partial string DatesInvalidText { get; set; } = "";

    [ObservableProperty]
    public partial bool IsDatesWarning { get; set; }

    [ObservableProperty]
    public partial string DatesWarnText { get; set; } = "";

    [ObservableProperty]
    public partial TimeSpan? ReminderTime { get; set; }

    public IReadOnlyList<string> TimeZones { get; } = ["ET", "PT", "UTC", "CET", "JST"];

    [ObservableProperty]
    public partial string? ReminderTimeZone { get; set; }

    [ObservableProperty]
    public partial bool IsReminderInvalid { get; set; }

    [ObservableProperty]
    public partial string ReminderInvalidText { get; set; } = "";

    [ObservableProperty]
    public partial bool NotifyAssignee { get; set; } = true;

    [ObservableProperty]
    public partial bool NotifyWatchers { get; set; }

    [ObservableProperty]
    public partial bool Watch { get; set; } = true;

    // ---- step 4: review, and the discard modal ----

    [ObservableProperty]
    public partial IReadOnlyList<ReviewRow> ReviewRows { get; set; } = [];

    [ObservableProperty]
    public partial bool IsDiscardConfirmOpen { get; set; }

    // ---- draft sync ----

    partial void OnSelectedTypeChanged(IssueType? value)
    {
        Draft.Type = value;
        Touch("type");
    }

    partial void OnSelectedProjectChanged(ProjectOption? value)
    {
        Draft.ProjectId = value?.Id;
        ComponentOptions = _projects.Find(value?.Id)?.Components ?? [];
        Touch("project");
    }

    partial void OnSummaryChanged(string value)
    {
        Draft.Title = value ?? "";
        UpdateValidation();
    }

    partial void OnDescriptionChanged(string value)
    {
        Draft.Description = value ?? "";
        UpdateValidation();
    }

    partial void OnComponentTextChanged(string value) => Draft.Component = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    partial void OnPriorityChanged(IssuePriority value) => Draft.Priority = value;

    partial void OnSelectedAssigneeChanged(AssigneeOption? value) => Draft.AssigneeId = value?.Id;

    partial void OnStoryPointsChanged(double value)
    {
        Draft.StoryPoints = (int)Math.Round(value);
        Touch("points");
    }

    partial void OnStartDateChanged(DateTime? value)
    {
        Draft.StartDate = value?.Date;
        Touch("dates");
    }

    partial void OnDueDateChanged(DateTime? value)
    {
        Draft.DueDate = value?.Date;
        Touch("dates");
        if (_touched.Contains("reminder")) UpdateValidation();
    }

    partial void OnReminderTimeChanged(TimeSpan? value)
    {
        Draft.ReminderTime = value;
        Touch("reminder");
    }

    partial void OnNotifyAssigneeChanged(bool value) => Draft.NotifyAssignee = value;

    partial void OnNotifyWatchersChanged(bool value) => Draft.NotifyWatchers = value;

    partial void OnWatchChanged(bool value) => Draft.Watch = value;

    partial void OnStepIndexChanged(int value) => UpdateStep();

    /// <summary>The labels multi-select changed (the view adapts its selection event).</summary>
    public void SetLabels(IEnumerable<string> labels)
    {
        var list = labels.ToList();
        if (list.SequenceEqual(SelectedLabels)) return;
        SelectedLabels = list;
        Draft.Labels = list.ToList();
        Touch("labels");
    }

    /// <summary>A field lost focus: from now on it shows its validation message.</summary>
    public void TouchField(string field) => Touch(field);

    void Touch(string field)
    {
        _touched.Add(field);
        UpdateValidation();
    }

    void UpdateStep()
    {
        IsTypeStep = StepIndex == TypeStep;
        IsDetailsStep = StepIndex == DetailsStep;
        IsScheduleStep = StepIndex == ScheduleStep;
        IsReviewStep = StepIndex == ReviewStep;
        CanGoBack = StepIndex > 0;
        PrimaryActionText = IsReviewStep ? "Create issue" : "Next";
        (StepHeading, StepDescription) = StepIndex switch
        {
            TypeStep => ("What kind of work is it?", "The type decides the fields that matter and where the issue shows on the board."),
            DetailsStep => ("Describe the issue", "A clear summary helps teammates find it later. Bugs need steps to reproduce."),
            ScheduleStep => ("Plan the work", "Estimate it, give it dates and choose who hears about it."),
            _ => ("Review and create", "Check the details. Use Edit to change a section."),
        };
        if (IsReviewStep) ReviewRows = BuildReview();
        BackCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Recomputes the field messages; only touched fields show theirs.</summary>
    void UpdateValidation()
    {
        var messages = WizardValidation.Validate(Draft, ReviewStep, Today);
        FieldMessage? Shown(string field, bool error) =>
            _touched.Contains(field) ? messages.FirstOrDefault(m => m.Field == field && m.IsError == error) : null;

        var type = Shown("type", true);
        HasTypeError = type is not null;
        TypeErrorText = type?.Text ?? "";

        var project = Shown("project", true);
        IsProjectInvalid = project is not null;
        ProjectInvalidText = project?.Text ?? "";

        var title = Shown("title", true);
        IsSummaryInvalid = title is not null;
        SummaryInvalidText = title?.Text ?? "";

        var description = Shown("description", false);
        IsDescriptionWarning = description is not null;
        DescriptionWarnText = description?.Text ?? "";

        var labels = Shown("labels", true);
        IsLabelsInvalid = labels is not null;
        LabelsInvalidText = labels?.Text ?? "";

        var datesError = Shown("dates", true);
        var datesWarn = Shown("dates", false);
        IsDatesInvalid = datesError is not null;
        DatesInvalidText = datesError?.Text ?? "";
        IsDatesWarning = datesError is null && datesWarn is not null;
        DatesWarnText = datesWarn?.Text ?? "";

        var points = Shown("points", false);
        IsPointsWarning = points is not null && ArePointsEnabled;
        PointsWarnText = points?.Text ?? "";

        var reminder = Shown("reminder", true);
        IsReminderInvalid = reminder is not null;
        ReminderInvalidText = reminder?.Text ?? "";

        InvalidSteps = Enumerable.Range(0, 4)
            .Select(step => StepFields[step].Any(f => _touched.Contains(f) && messages.Any(m => m.Field == f && m.IsError)))
            .ToList();
        if (IsReviewStep) ReviewRows = BuildReview();
    }

    // ---- navigation ----

    bool IsIdle => !IsCreating;

    bool CanStepBack => StepIndex > 0 && !IsCreating;

    /// <summary>Next: validates the step (touching its fields) and moves on; on the review step creates the issue.</summary>
    [RelayCommand(CanExecute = nameof(IsIdle))]
    async Task NextAsync()
    {
        if (IsReviewStep)
        {
            await CreateAsync();
            return;
        }
        foreach (var field in StepFields[StepIndex]) _touched.Add(field);
        UpdateValidation();
        if (!WizardValidation.CanAdvance(Draft, StepIndex, Today)) return;
        StepIndex++;
    }

    [RelayCommand(CanExecute = nameof(CanStepBack))]
    void Back()
    {
        if (StepIndex > 0) StepIndex--;
    }

    /// <summary>A click on a progress step or a review Edit link: only earlier (completed) steps can be opened.</summary>
    public bool GoToStep(int step)
    {
        if (IsCreating || step < 0 || step >= StepIndex) return false;
        StepIndex = step;
        return true;
    }

    [RelayCommand]
    void EditSection(ReviewRow row) => GoToStep(row.Step);

    [RelayCommand(CanExecute = nameof(IsIdle))]
    async Task CancelAsync()
    {
        if (Draft.IsDirty)
        {
            IsDiscardConfirmOpen = true;
            return;
        }
        await LeaveAsync();
    }

    [RelayCommand]
    async Task ConfirmDiscardAsync()
    {
        IsDiscardConfirmOpen = false;
        await LeaveAsync();
    }

    [RelayCommand]
    void KeepEditing() => IsDiscardConfirmOpen = false;

    Task LeaveAsync() => _navigator.CanGoBack ? _navigator.GoBackAsync() : _navigator.NavigateAsync(Routes.Issues);

    async Task CreateAsync()
    {
        foreach (var field in StepFields.SelectMany(f => f)) _touched.Add(field);
        UpdateValidation();
        for (var step = 0; step < ReviewStep; step++)
        {
            if (WizardValidation.CanAdvance(Draft, step, Today)) continue;
            StepIndex = step;
            return;
        }

        IsCreating = true;
        try
        {
            var issue = await _issues.CreateAsync(Draft);
            _feedback.Toast(FeedbackKind.Success, $"{issue.Key} created", issue.Title);
            // Replace the wizard with the new issue, so Back from it skips the finished form.
            if (_navigator.CanGoBack) await _navigator.GoBackAsync();
            await _navigator.NavigateAsync(Routes.IssueDetail, issue.Key);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _feedback.Toast(FeedbackKind.Error, "The issue was not created", ex.Message);
        }
        finally
        {
            IsCreating = false;
        }
    }

    protected override void OnDeactivated() => IsDiscardConfirmOpen = false;

    // ---- review ----

    IReadOnlyList<ReviewRow> BuildReview()
    {
        var project = _projects.Find(Draft.ProjectId)?.Name ?? "No project";
        var type = Draft.Type?.Text() ?? "No type";
        var summary = Draft.Title.Trim().Length == 0 ? "No summary" : Draft.Title.Trim();
        var details = string.Join("\n", new[]
        {
            summary,
            $"Component: {Draft.Component ?? "None"} · Priority: {Draft.Priority.Text()} · Assignee: {_people.NameOf(Draft.AssigneeId)}",
            Draft.Labels.Count == 0 ? "No labels" : $"Labels: {string.Join(", ", Draft.Labels)}",
        });

        var schedule = new List<string>();
        if (Draft.Type != IssueType.Epic) schedule.Add(Draft.StoryPoints == 1 ? "1 story point" : $"{Draft.StoryPoints} story points");
        schedule.Add(Draft.StartDate is null && Draft.DueDate is null
            ? "No dates"
            : $"{Formatting.ShortDate(Draft.StartDate)} to {Formatting.ShortDate(Draft.DueDate)}");
        if (Draft.ReminderTime is { } t)
            schedule.Add($"Reminder at {DateTime.Today.Add(t).ToString("h:mm tt", CultureInfo.InvariantCulture)} {ReminderTimeZone}");

        var notify = new List<string>();
        if (Draft.NotifyAssignee) notify.Add("Notify the assignee");
        if (Draft.NotifyWatchers) notify.Add("Notify project watchers");
        notify.Add(Draft.Watch ? "You watch the issue" : "You don't watch the issue");

        return
        [
            new ReviewRow("Type and project", $"{type} in {project}", TypeStep),
            new ReviewRow("Details", details, DetailsStep),
            new ReviewRow("Schedule", string.Join(" · ", schedule), ScheduleStep),
            new ReviewRow("Notifications", string.Join(" · ", notify), ScheduleStep),
        ];
    }
}
