namespace Maui.Carbon.Relay.Tests;

public class IssueDetailViewModelTests
{
    readonly TestWorld _w = new();

    IssueDetailViewModel Create() => new(_w.Issues, _w.Projects, _w.People, _w.Assistant, _w.Navigator, _w.Feedback, _w.Clock);

    Issue OpenBug() => _w.Issues.All.First(i => i.StackTrace is not null && i.IsOpen);

    async Task<(IssueDetailViewModel Vm, Issue Issue)> OpenAsync(Issue? issue = null)
    {
        issue ??= OpenBug();
        var vm = Create();
        await vm.ActivateAsync(issue.Key);
        return (vm, issue);
    }

    [Fact]
    public async Task Activation_loads_the_issue_into_header_tabs_and_fields()
    {
        var (vm, issue) = await OpenAsync();
        Assert.False(vm.IsLoading);
        Assert.True(vm.HasIssue);
        Assert.False(vm.IsNotFound);
        Assert.Equal(issue.Title, vm.Title);
        Assert.Equal(issue.Status, vm.SelectedStatus!.Status);
        Assert.Equal(issue.AssigneeId, vm.SelectedAssignee!.Id);
        Assert.Equal(issue.StoryPoints, (int?)vm.StoryPoints);
        Assert.True(vm.HasStackTrace);
        Assert.Equal(issue.ReproSteps, vm.ReproSteps);
        Assert.Equal(issue.Comments.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), vm.CommentCountText);
        Assert.Equal(issue.Activity.Count, vm.Activity.Count);
        Assert.True(vm.Activity[^1].IsLast);
        Assert.Empty(_w.Feedback.Toasts);
    }

    [Fact]
    public async Task Missing_key_shows_not_found()
    {
        var vm = Create();
        await vm.ActivateAsync("ATL-9999");
        Assert.True(vm.IsNotFound);
        Assert.False(vm.HasIssue);
        Assert.Equal("ATL-9999", vm.Key);
    }

    [Fact]
    public async Task Child_and_linked_issues_are_listed()
    {
        var epic = _w.Issues.All.First(i => i.Type == IssueType.Epic && _w.Issues.All.Any(c => c.ParentKey == i.Key));
        var (vm, _) = await OpenAsync(epic);
        Assert.Equal(_w.Issues.All.Count(c => c.ParentKey == epic.Key), vm.ChildIssues.Count);
        Assert.True(vm.HasChildIssues);

        var linked = _w.Issues.All.First(i => i.LinkedKeys.Count > 0);
        var (vm2, _) = await OpenAsync(linked);
        Assert.Equal(linked.LinkedKeys, vm2.LinkedIssues.Select(r => r.Key));
    }

    [Fact]
    public async Task Changing_status_saves_and_toasts()
    {
        var (vm, issue) = await OpenAsync();
        var target = issue.Status == IssueStatus.InReview ? IssueStatus.InProgress : IssueStatus.InReview;
        vm.SelectedStatus = vm.StatusOptions.First(o => o.Status == target);
        await vm.PendingSave;
        Assert.Equal(target, issue.Status);
        Assert.Equal(target.Text(), vm.StatusText);
        Assert.Contains(_w.Feedback.Toasts, t => t.Kind == FeedbackKind.Success && t.Title == $"{issue.Key} moved to {target.Text()}");
    }

    [Fact]
    public async Task Due_date_and_points_save_and_are_recorded_in_activity()
    {
        var (vm, issue) = await OpenAsync();
        var due = TestWorld.Now.Date.AddDays(10);
        vm.DueDate = due;
        await vm.PendingSave;
        vm.StoryPoints = 8;
        await vm.PendingSave;
        Assert.Equal(due, issue.DueDate);
        Assert.Equal(8, issue.StoryPoints);
        Assert.Equal(2, _w.Feedback.Toasts.Count);
        Assert.Contains(vm.Activity, a => a.Text.Contains("story points", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Refresh_from_a_change_elsewhere_does_not_save_again()
    {
        var (vm, issue) = await OpenAsync();
        var activity = issue.Activity.Count;
        await _w.Issues.AssignAsync([issue], "p05", TestContext.Current.CancellationToken);
        Assert.Equal("p05", vm.SelectedAssignee!.Id);
        Assert.Empty(_w.Feedback.Toasts);
        Assert.Equal(activity + 1, issue.Activity.Count);
    }

    [Fact]
    public async Task Resolve_closes_the_issue_with_the_resolution()
    {
        var (vm, issue) = await OpenAsync();
        Assert.True(vm.CanResolve);
        await vm.ResolveCommand.ExecuteAsync(IssueResolution.WontFix);
        Assert.Equal(IssueStatus.Cancelled, issue.Status);
        Assert.Equal(IssueResolution.WontFix, issue.Resolution);
        Assert.True(vm.IsResolved);
        Assert.Equal("Won't fix", vm.ResolutionText);
        Assert.False(vm.CanResolve);
        Assert.Contains(_w.Feedback.Toasts, t => t.Title == $"{issue.Key} resolved as Won't fix");
    }

    [Fact]
    public async Task Posting_a_comment_adds_it_and_clears_the_box()
    {
        var (vm, issue) = await OpenAsync();
        var before = vm.Comments.Count;
        Assert.False(vm.CanPostComment);
        vm.NewComment = "Reproduced on build 4.12 with airplane mode toggled.";
        Assert.True(vm.PostCommentCommand.CanExecute(null));
        await vm.PostCommentCommand.ExecuteAsync(null);
        Assert.Equal(before + 1, vm.Comments.Count);
        Assert.True(vm.Comments[^1].IsMine);
        Assert.Equal("", vm.NewComment);
        Assert.False(vm.IsPostingComment);
        Assert.Equal(before + 1, issue.Comments.Count);
    }

    [Fact]
    public async Task Too_long_comment_cannot_be_posted()
    {
        var (vm, _) = await OpenAsync();
        vm.NewComment = new string('x', IssueDetailViewModel.CommentMaxLength + 1);
        Assert.False(vm.PostCommentCommand.CanExecute(null));
    }

    [Fact]
    public async Task Applying_the_ai_suggestion_can_be_reverted()
    {
        var issue = _w.Issues.All.First(i => i.IsOpen && i.Component is not null && i.StackTrace is not null);
        var suggestion = await _w.Assistant.SuggestAssigneeAsync(issue, TestContext.Current.CancellationToken);
        Assert.NotNull(suggestion);
        // Make sure the suggestion differs from the current assignee.
        issue.AssigneeId = null;
        var (vm, _) = await OpenAsync(issue);
        Assert.True(vm.HasSuggestion);
        Assert.Equal(_w.People.NameOf(suggestion!.PersonId), vm.SuggestedName);

        await vm.ApplySuggestionCommand.ExecuteAsync(null);
        Assert.Equal(suggestion.PersonId, issue.AssigneeId);
        Assert.True(vm.IsAssigneeAiFilled);
        Assert.False(vm.HasSuggestion);

        await vm.RevertAssigneeCommand.ExecuteAsync(null);
        Assert.Null(issue.AssigneeId);
        Assert.False(vm.IsAssigneeAiFilled);
        Assert.True(vm.HasSuggestion);
        Assert.Equal("Unassigned", vm.SelectedAssignee!.Name);
    }

    [Fact]
    public async Task Picking_an_assignee_by_hand_clears_the_ai_state()
    {
        var issue = _w.Issues.All.First(i => i.IsOpen && i.StackTrace is not null);
        issue.AssigneeId = null;
        var (vm, _) = await OpenAsync(issue);
        await vm.ApplySuggestionCommand.ExecuteAsync(null);
        vm.SelectedAssignee = vm.AssigneeOptions.First(o => o.Id == "p07");
        await vm.PendingSave;
        Assert.False(vm.IsAssigneeAiFilled);
        Assert.Equal("p07", issue.AssigneeId);
    }

    [Fact]
    public async Task Watch_toggles_and_relabels_the_menu_item()
    {
        var (vm, issue) = await OpenAsync();
        var watching = vm.IsWatching;
        await vm.ToggleWatchCommand.ExecuteAsync(null);
        Assert.Equal(!watching, vm.IsWatching);
        Assert.Equal(vm.IsWatching ? "Stop watching" : "Watch", vm.WatchText);
        Assert.Equal(!watching, issue.WatcherIds.Contains(MockDataSeed.MeId));
    }

    [Fact]
    public async Task Move_to_project_uses_the_dialog_choice()
    {
        var (vm, issue) = await OpenAsync();
        vm.OpenMoveDialogCommand.Execute(null);
        Assert.True(vm.IsMoveDialogOpen);
        Assert.DoesNotContain(vm.MoveProjectOptions, p => p.Id == issue.ProjectId);
        vm.MoveTarget = vm.MoveProjectOptions.First(p => p.Id == "hbr");
        await vm.ConfirmMoveCommand.ExecuteAsync(null);
        Assert.False(vm.IsMoveDialogOpen);
        Assert.Equal("hbr", issue.ProjectId);
        Assert.Null(issue.SprintId);
        Assert.Equal("Harbor design system", vm.ProjectName);
    }

    [Fact]
    public async Task Delete_asks_then_deletes_and_goes_back()
    {
        await _w.Navigator.NavigateAsync(Routes.Issues);
        var issue = OpenBug();
        await _w.Navigator.NavigateAsync(Routes.IssueDetail, issue.Key);
        var (vm, _) = await OpenAsync(issue);
        vm.RequestDeleteCommand.Execute(null);
        Assert.True(vm.IsDeleteConfirmOpen);
        Assert.NotNull(_w.Issues.Find(issue.Key));

        await vm.ConfirmDeleteCommand.ExecuteAsync(null);
        Assert.Null(_w.Issues.Find(issue.Key));
        Assert.Equal(Routes.Issues, _w.Navigator.CurrentRoute);
        Assert.Contains(_w.Feedback.Toasts, t => t.Title == $"{issue.Key} deleted");
    }

    [Fact]
    public async Task Deleted_elsewhere_switches_to_not_found()
    {
        var (vm, issue) = await OpenAsync();
        await _w.Issues.DeleteAsync([issue], TestContext.Current.CancellationToken);
        Assert.True(vm.IsNotFound);
        Assert.False(vm.HasIssue);
    }

    [Fact]
    public async Task Attachments_are_added_and_removed()
    {
        var (vm, issue) = await OpenAsync();
        var count = vm.Attachments.Count;
        await vm.AddAttachmentAsync("crash-report.txt", 2048);
        Assert.Equal(count + 1, vm.Attachments.Count);
        Assert.Equal("crash-report.txt", vm.Attachments[0].Name);
        await vm.RemoveAttachmentCommand.ExecuteAsync("crash-report.txt");
        Assert.Equal(count, vm.Attachments.Count);
        Assert.DoesNotContain(issue.Attachments, a => a.Name == "crash-report.txt");
    }

    [Fact]
    public async Task Leaving_stops_following_changes()
    {
        var (vm, issue) = await OpenAsync();
        vm.Deactivate();
        await _w.Issues.SetStatusAsync([issue], IssueStatus.Done, TestContext.Current.CancellationToken);
        Assert.NotEqual(IssueStatus.Done, vm.SelectedStatus!.Status);
    }
}
