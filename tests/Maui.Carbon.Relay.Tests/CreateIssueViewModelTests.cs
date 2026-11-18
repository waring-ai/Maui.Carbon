namespace Maui.Carbon.Relay.Tests;

public class CreateIssueViewModelTests
{
    readonly TestWorld _w = new();

    CreateIssueViewModel Create() => new(_w.Issues, _w.Projects, _w.People, _w.Workspace, _w.Navigator, _w.Feedback, _w.Clock);

    static void FillDetails(CreateIssueViewModel vm)
    {
        vm.Summary = "Offline queue drops edits after a forced app update";
        vm.Description = "1. Queue edits offline\n2. Update the app\n3. Reconnect";
    }

    [Fact]
    public void Starts_on_the_type_step_in_the_current_project()
    {
        var vm = Create();
        Assert.Equal(CreateIssueViewModel.TypeStep, vm.StepIndex);
        Assert.True(vm.IsTypeStep);
        Assert.False(vm.CanGoBack);
        Assert.Equal(_w.Workspace.CurrentProject.Id, vm.SelectedProject!.Id);
        Assert.Equal(_w.Workspace.CurrentProject.Components, vm.ComponentOptions);
        Assert.Equal("Next", vm.PrimaryActionText);
        Assert.False(vm.HasTypeError);
    }

    [Fact]
    public async Task Next_without_a_type_is_blocked_and_shows_the_error()
    {
        var vm = Create();
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.TypeStep, vm.StepIndex);
        Assert.True(vm.HasTypeError);
        Assert.Equal("Choose an issue type.", vm.TypeErrorText);
        Assert.True(vm.InvalidSteps[0]);

        vm.SelectedType = IssueType.Bug;
        Assert.False(vm.HasTypeError);
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.DetailsStep, vm.StepIndex);
    }

    [Fact]
    public async Task Summary_validates_on_blur_and_blocks_next()
    {
        var vm = Create();
        vm.SelectedType = IssueType.Bug;
        await vm.NextCommand.ExecuteAsync(null);

        vm.Summary = "Crash";
        Assert.False(vm.IsSummaryInvalid); // not touched yet
        vm.TouchField("title");
        Assert.True(vm.IsSummaryInvalid);
        Assert.StartsWith("Use at least 8 characters", vm.SummaryInvalidText, StringComparison.Ordinal);

        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.DetailsStep, vm.StepIndex);
        Assert.True(vm.IsDescriptionWarning); // bug without description: warning, not blocking

        vm.Summary = "Crash when the offline queue is full";
        Assert.False(vm.IsSummaryInvalid);
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.ScheduleStep, vm.StepIndex);
    }

    [Fact]
    public async Task Too_many_labels_block_the_details_step()
    {
        var vm = Create();
        vm.SelectedType = IssueType.Task;
        await vm.NextCommand.ExecuteAsync(null);
        FillDetails(vm);
        vm.SetLabels(_w.Issues.Labels.Take(6));
        Assert.True(vm.IsLabelsInvalid);
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.DetailsStep, vm.StepIndex);
        vm.SetLabels(_w.Issues.Labels.Take(2));
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.ScheduleStep, vm.StepIndex);
    }

    [Fact]
    public async Task Schedule_warns_over_13_points_and_blocks_reversed_dates()
    {
        var vm = Create();
        vm.SelectedType = IssueType.Story;
        await vm.NextCommand.ExecuteAsync(null);
        FillDetails(vm);
        await vm.NextCommand.ExecuteAsync(null);

        vm.StoryPoints = 21;
        Assert.True(vm.IsPointsWarning);
        vm.StartDate = TestWorld.Now.Date.AddDays(5);
        vm.DueDate = TestWorld.Now.Date.AddDays(2);
        Assert.True(vm.IsDatesInvalid);
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.ScheduleStep, vm.StepIndex);

        vm.DueDate = TestWorld.Now.Date.AddDays(12);
        Assert.False(vm.IsDatesInvalid);
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.ReviewStep, vm.StepIndex);
        Assert.Equal("Create issue", vm.PrimaryActionText);
    }

    [Fact]
    public async Task Reminder_needs_a_due_date()
    {
        var vm = Create();
        vm.SelectedType = IssueType.Task;
        await vm.NextCommand.ExecuteAsync(null);
        FillDetails(vm);
        await vm.NextCommand.ExecuteAsync(null);
        vm.ReminderTime = new TimeSpan(9, 0, 0);
        Assert.True(vm.IsReminderInvalid);
        vm.DueDate = TestWorld.Now.Date.AddDays(3);
        Assert.False(vm.IsReminderInvalid);
    }

    [Fact]
    public async Task Only_completed_steps_can_be_opened_and_back_steps_back()
    {
        var vm = Create();
        Assert.False(vm.GoToStep(CreateIssueViewModel.DetailsStep));
        vm.SelectedType = IssueType.Task;
        await vm.NextCommand.ExecuteAsync(null);
        FillDetails(vm);
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.ScheduleStep, vm.StepIndex);
        Assert.False(vm.GoToStep(CreateIssueViewModel.ReviewStep));
        Assert.True(vm.GoToStep(CreateIssueViewModel.TypeStep));
        Assert.True(vm.IsTypeStep);
        await vm.NextCommand.ExecuteAsync(null);
        vm.BackCommand.Execute(null);
        Assert.Equal(CreateIssueViewModel.TypeStep, vm.StepIndex);
    }

    [Fact]
    public async Task Review_lists_each_section_with_its_edit_step()
    {
        var vm = Create();
        vm.SelectedType = IssueType.Bug;
        await vm.NextCommand.ExecuteAsync(null);
        FillDetails(vm);
        vm.ComponentText = "Background refresh";
        vm.Priority = IssuePriority.High;
        await vm.NextCommand.ExecuteAsync(null);
        await vm.NextCommand.ExecuteAsync(null);

        Assert.Equal(4, vm.ReviewRows.Count);
        Assert.Equal("Bug in Atlas mobile", vm.ReviewRows[0].Details);
        Assert.Contains("Component: Background refresh", vm.ReviewRows[1].Details, StringComparison.Ordinal);
        Assert.Contains("Priority: High", vm.ReviewRows[1].Details, StringComparison.Ordinal);

        vm.EditSectionCommand.Execute(vm.ReviewRows[1]);
        Assert.Equal(CreateIssueViewModel.DetailsStep, vm.StepIndex);
    }

    [Fact]
    public async Task Create_saves_the_draft_toasts_and_opens_the_new_issue()
    {
        var count = _w.Issues.All.Count;
        var vm = Create();
        vm.SelectedType = IssueType.Bug;
        vm.SelectedProject = vm.ProjectOptions.First(p => p.Id == "bil");
        await vm.NextCommand.ExecuteAsync(null);
        FillDetails(vm);
        vm.ComponentText = "Invoices";
        vm.SetLabels(["customer-reported"]);
        vm.SelectedAssignee = vm.AssigneeOptions.First(a => a.Id == "p06");
        await vm.NextCommand.ExecuteAsync(null);
        vm.StoryPoints = 5;
        vm.DueDate = TestWorld.Now.Date.AddDays(7);
        vm.Watch = false;
        await vm.NextCommand.ExecuteAsync(null);
        await vm.NextCommand.ExecuteAsync(null);

        Assert.Equal(count + 1, _w.Issues.All.Count);
        var created = _w.Issues.All[^1];
        Assert.StartsWith("BIL-", created.Key, StringComparison.Ordinal);
        Assert.Equal(IssueType.Bug, created.Type);
        Assert.Equal("Invoices", created.Component);
        Assert.Equal("p06", created.AssigneeId);
        Assert.Equal(5, created.StoryPoints);
        Assert.DoesNotContain(MockDataSeed.MeId, created.WatcherIds);
        Assert.Contains(_w.Feedback.Toasts, t => t.Kind == FeedbackKind.Success && t.Title == $"{created.Key} created");
        Assert.Equal((Routes.IssueDetail, (object?)created.Key), _w.Navigator.History[^1]);
        Assert.False(vm.IsCreating);
    }

    [Fact]
    public async Task Create_jumps_to_the_first_invalid_step()
    {
        var vm = Create();
        vm.SelectedType = IssueType.Task;
        await vm.NextCommand.ExecuteAsync(null);
        FillDetails(vm);
        await vm.NextCommand.ExecuteAsync(null);
        await vm.NextCommand.ExecuteAsync(null);
        Assert.True(vm.IsReviewStep);
        vm.Summary = "";
        var count = _w.Issues.All.Count;
        await vm.NextCommand.ExecuteAsync(null);
        Assert.Equal(CreateIssueViewModel.DetailsStep, vm.StepIndex);
        Assert.True(vm.IsSummaryInvalid);
        Assert.Equal(count, _w.Issues.All.Count);
    }

    [Fact]
    public async Task Cancel_with_a_clean_draft_leaves_at_once()
    {
        var vm = Create();
        await vm.CancelCommand.ExecuteAsync(null);
        Assert.False(vm.IsDiscardConfirmOpen);
        Assert.Equal(Routes.Issues, _w.Navigator.CurrentRoute);
    }

    [Fact]
    public async Task Cancel_with_a_dirty_draft_asks_first()
    {
        await _w.Navigator.NavigateAsync(Routes.Issues);
        await _w.Navigator.NavigateAsync(Routes.CreateIssue);
        var vm = Create();
        vm.SelectedType = IssueType.Story;
        await vm.CancelCommand.ExecuteAsync(null);
        Assert.True(vm.IsDiscardConfirmOpen);
        Assert.Equal(Routes.CreateIssue, _w.Navigator.CurrentRoute);

        vm.KeepEditingCommand.Execute(null);
        Assert.False(vm.IsDiscardConfirmOpen);

        await vm.CancelCommand.ExecuteAsync(null);
        await vm.ConfirmDiscardCommand.ExecuteAsync(null);
        Assert.Equal(Routes.Issues, _w.Navigator.CurrentRoute);
    }

    [Fact]
    public void Epics_turn_the_estimate_off()
    {
        var vm = Create();
        vm.SelectedType = IssueType.Epic;
        Assert.False(vm.ArePointsEnabled);
        vm.StoryPoints = 20;
        Assert.False(vm.IsPointsWarning);
    }
}
