namespace Maui.Carbon.Relay.Tests;

public class BacklogViewModelTests
{
    readonly TestWorld _w = new();

    BacklogViewModel Create() => new(_w.Issues, _w.Sprints, _w.Workspace, _w.Navigator, _w.Feedback, _w.Clock);

    string ProjectId => _w.Workspace.CurrentProject.Id;

    [Fact]
    public async Task Activation_shows_the_running_and_planned_sprints()
    {
        var vm = Create();
        await vm.ActivateAsync(null);

        Assert.False(vm.IsLoading);
        Assert.True(vm.IsReady);
        var active = _w.Sprints.Active(ProjectId)!;
        Assert.Equal(active.Name, vm.ActiveSprint?.Name);
        Assert.Equal("2 days left", vm.ActiveSprint!.TimeText);
        var work = _w.Issues.All.Where(i => i.SprintId == active.Id && i.Type != IssueType.Epic && i.Status != IssueStatus.Cancelled).ToList();
        Assert.Equal(work.Sum(i => i.StoryPoints ?? 0), vm.ActiveSprint.PointsTotal);
        Assert.Equal(work.Where(i => i.Status == IssueStatus.Done).Sum(i => i.StoryPoints ?? 0), vm.ActiveSprint.PointsDone);
        Assert.Equal($"{vm.ActiveSprint.PointsDone} of {vm.ActiveSprint.PointsTotal} points done", vm.ActiveSprint.ProgressText);

        Assert.True(vm.HasPlannedSprint);
        Assert.Equal(SprintState.Planned, vm.PlannedSprint!.Sprint.State);
        Assert.Contains($"complete {active.Name} first", vm.StartCalloutText, StringComparison.Ordinal);
        Assert.False(vm.HasNoSprints);
    }

    [Fact]
    public async Task Backlog_tree_groups_open_work_outside_the_running_sprint_by_epic()
    {
        var vm = Create();
        await vm.ActivateAsync(null);

        var active = _w.Sprints.Active(ProjectId)!;
        Assert.True(vm.HasBacklog);
        var leaves = vm.Tree.SelectMany(b => b.Items).ToList();
        var expected = _w.Issues.All.Where(i => i.ProjectId == ProjectId && i.Type != IssueType.Epic && i.IsOpen && i.SprintId != active.Id).Select(i => i.Key).Order();
        Assert.Equal(expected, leaves.Select(l => l.Key).Order());
        foreach (var branch in vm.Tree.Where(b => !b.IsUnassigned))
            Assert.All(branch.Items, l => Assert.Equal(branch.EpicKey, _w.Issues.Find(l.Key)!.ParentKey));
        Assert.DoesNotContain(vm.Tree.SkipLast(1), b => b.IsUnassigned);
        Assert.StartsWith($"{leaves.Count} open issues", vm.BacklogText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Opening_a_backlog_issue_navigates_to_its_detail()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var key = vm.Tree[0].Items[0].Key;
        await vm.OpenIssueCommand.ExecuteAsync(key);
        Assert.Equal((Routes.IssueDetail, (object?)key), _w.Navigator.History.Last());
    }

    [Fact]
    public async Task History_pages_completed_sprints_three_at_a_time_newest_first()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var completed = _w.Sprints.ForProject(ProjectId).Where(s => s.State == SprintState.Completed).OrderByDescending(s => s.End).ToList();

        Assert.Equal(completed.Count, vm.HistoryCount);
        Assert.Equal((int)Math.Ceiling(completed.Count / 3.0), vm.HistoryPageCount);
        Assert.True(vm.HasHistoryPages);
        Assert.Equal(completed.Take(3).Select(s => s.Name), vm.HistoryItems.Select(h => h.Name));

        vm.HistoryPage = 1;
        Assert.Equal(completed.Skip(3).Take(3).Select(s => s.Name), vm.HistoryItems.Select(h => h.Name));

        var row = vm.HistoryItems[0];
        Assert.Equal(row.Sprint.CommittedPoints, row.Committed);
        Assert.Equal(row.Sprint.CompletedPoints, row.Completed);
        Assert.Contains($"{row.Completed} of {row.Committed} committed points", row.CompletionText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Starting_while_a_sprint_runs_shows_the_error_inline()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var active = _w.Sprints.Active(ProjectId)!;

        await vm.StartSprintCommand.ExecuteAsync(null);

        Assert.True(vm.HasStartError);
        Assert.Contains(active.Name, vm.StartError, StringComparison.Ordinal);
        Assert.False(vm.IsStarting);
        Assert.Equal(SprintState.Planned, vm.PlannedSprint!.Sprint.State);
        Assert.Empty(_w.Feedback.Toasts);

        vm.DismissStartErrorCommand.Execute(null);
        Assert.False(vm.HasStartError);
    }

    [Fact]
    public async Task Completing_then_starting_moves_the_project_to_the_next_sprint()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var active = vm.ActiveSprint!;
        var planned = vm.PlannedSprint!;
        var historyBefore = vm.HistoryCount;

        await vm.CompleteSprintCommand.ExecuteAsync(null);

        Assert.False(vm.IsCompleting);
        Assert.False(vm.HasActiveSprint);
        Assert.Equal(SprintState.Completed, active.Sprint.State);
        Assert.Equal(historyBefore + 1, vm.HistoryCount);
        Assert.Equal(active.Name, vm.HistoryItems[0].Name);
        var toast = _w.Feedback.Toasts.Single();
        Assert.Equal((FeedbackKind.Success, $"{active.Name} completed"), (toast.Kind, toast.Title));
        Assert.Contains($"moved to {planned.Name}", toast.Subtitle, StringComparison.Ordinal);
        Assert.Contains("starts today", vm.StartCalloutText, StringComparison.Ordinal);

        await vm.StartSprintCommand.ExecuteAsync(null);

        Assert.False(vm.HasStartError);
        Assert.Equal(planned.Name, vm.ActiveSprint?.Name);
        Assert.Equal("14 days left", vm.ActiveSprint!.TimeText);
        Assert.False(vm.HasPlannedSprint);
        Assert.False(vm.StartSprintCommand.CanExecute(null));
        Assert.Contains(_w.Feedback.Toasts, t => t.Title == $"{planned.Name} started");
    }

    [Fact]
    public async Task Busy_flags_are_set_while_the_sprint_service_works()
    {
        _w.Latency.Scale = 0.02; // 24 ms per sprint call
        var vm = Create();
        await vm.ActivateAsync(null);

        var complete = vm.CompleteSprintCommand.ExecuteAsync(null);
        Assert.True(vm.IsCompleting);
        Assert.False(vm.CompleteSprintCommand.CanExecute(null));
        await complete;
        Assert.False(vm.IsCompleting);

        var start = vm.StartSprintCommand.ExecuteAsync(null);
        Assert.True(vm.IsStarting);
        await start;
        Assert.False(vm.IsStarting);
    }

    [Fact]
    public async Task Switching_project_refreshes_and_resets_the_history_page()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.HistoryPage = 1;

        _w.Workspace.SelectProject("hbr");
        Assert.Equal(0, vm.HistoryPage);
        Assert.Equal(_w.Sprints.Active("hbr")!.Name, vm.ActiveSprint?.Name);
        Assert.Equal(_w.Projects.Find("hbr")!.Name, vm.ProjectName);

        _w.Workspace.SelectProject("ins");
        Assert.False(vm.HasActiveSprint);
        Assert.False(vm.HasPlannedSprint);
        Assert.True(vm.HasNoSprints);
        Assert.False(vm.HasHistory);
        Assert.Equal(1, vm.HistoryPageCount);
    }
}
