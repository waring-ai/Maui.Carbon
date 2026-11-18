namespace Maui.Carbon.Relay.Tests;

public class DashboardViewModelTests
{
    readonly TestWorld _w = new();

    DashboardViewModel Create() =>
        new(_w.Issues, _w.People, _w.Projects, _w.Sprints, _w.Workspace, _w.Assistant, _w.Navigator, _w.Clock);

    async Task<DashboardViewModel> Activated()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        return vm;
    }

    List<Issue> MyOpenIn(string projectId) =>
        _w.Issues.All.Where(i => i.ProjectId == projectId && i.IsOpen && i.AssigneeId == MockDataSeed.MeId).ToList();

    [Fact]
    public async Task Activation_greets_the_user_and_scopes_to_the_current_project()
    {
        var vm = await Activated();
        Assert.False(vm.IsLoading);
        Assert.Equal("Good morning, Maya", vm.Title);
        Assert.Equal("Atlas mobile", vm.ProjectName);
        Assert.Equal("Atlas mobile · Sprint 23", vm.ContextText);
        Assert.StartsWith("Sprint goal: ", vm.SprintGoalText, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(9, "Good morning, Maya")]
    [InlineData(14, "Good afternoon, Maya")]
    [InlineData(21, "Good evening, Maya")]
    public void Greeting_follows_the_time_of_day(int hour, string expected) =>
        Assert.Equal(expected, DashboardViewModel.Greet(TestWorld.Now.Date.AddHours(hour), _w.People.Me));

    [Fact]
    public async Task Kpis_count_my_open_due_and_in_review_issues_in_the_project()
    {
        var vm = await Activated();
        var mine = MyOpenIn("atl");
        Assert.Equal(mine.Count, vm.MyOpenCount);
        Assert.True(vm.MyOpenCount > 0);
        Assert.Equal(mine.Count(i => i.DueDate is { } d && d.Date <= TestWorld.Now.Date.AddDays(6)), vm.DueThisWeekCount);
        Assert.Equal(_w.Issues.All.Count(i => i.ProjectId == "atl" && i.Status == IssueStatus.InReview), vm.InReviewCount);
        Assert.Equal($"{vm.PointsDone} / {vm.PointsTotal}", vm.PointsText);
        Assert.True(vm.PointsAreForSprint);
    }

    [Fact]
    public async Task Kpi_tiles_open_the_issue_list_in_their_scope()
    {
        var vm = await Activated();
        await vm.OpenIssuesCommand.ExecuteAsync(IssueScope.AssignedToMe);
        Assert.Equal((Routes.Issues, (object?)IssueScope.AssignedToMe), _w.Navigator.History[^1]);
        await vm.OpenIssuesCommand.ExecuteAsync(IssueScope.All);
        Assert.Equal((Routes.Issues, (object?)IssueScope.All), _w.Navigator.History[^1]);
    }

    [Fact]
    public async Task Sprint_progress_days_left_and_velocity_come_from_the_active_sprint()
    {
        var vm = await Activated();
        var sprint = _w.Sprints.Active("atl")!;
        var sprintIssues = _w.Issues.All.Where(i => i.SprintId == sprint.Id).ToList();
        Assert.True(vm.HasActiveSprint);
        Assert.Equal("Sprint 23", vm.SprintName);
        Assert.Equal(2, vm.DaysLeft);
        Assert.Equal("2 days left", vm.DaysLeftText);
        Assert.Equal(Math.Round(ReportMath.SprintProgress(sprintIssues) * 100, 1), vm.SprintPercent);
        var velocity = ReportMath.Velocity(_w.Sprints.ForProject("atl"));
        Assert.Equal(velocity, vm.Velocity);
        Assert.Equal($"{velocity:0} points", vm.VelocityText);
        Assert.Equal("Per sprint, last 4 sprints", vm.VelocityHelper);
        Assert.Equal(sprintIssues.Count(i => i.IsOpen).ToString(System.Globalization.CultureInfo.InvariantCulture), vm.OpenInSprintText);
        Assert.Equal($"of {sprintIssues.Count} issues", vm.OpenInSprintHelper);
    }

    [Fact]
    public async Task Sprint_ending_soon_shows_a_notice_that_leads_to_the_backlog()
    {
        var vm = await Activated();
        Assert.True(vm.IsSprintEndingSoon);
        Assert.Equal("Sprint 23 ends in 2 days", vm.SprintEndingTitle);
        Assert.EndsWith("still open.", vm.SprintEndingSubtitle, StringComparison.Ordinal);

        await vm.ReviewSprintCommand.ExecuteAsync(null);
        Assert.Equal(Routes.Backlog, _w.Navigator.CurrentRoute);
    }

    [Fact]
    public async Task A_dismissed_notice_stays_hidden_after_a_refresh()
    {
        var vm = await Activated();
        vm.DismissSprintNoticeCommand.Execute(null);
        Assert.False(vm.IsSprintEndingSoon);

        var issue = _w.Issues.All.First(i => i.ProjectId == "atl" && i.IsOpen && i.AssigneeId != MockDataSeed.MeId);
        await _w.Issues.AssignAsync([issue], MockDataSeed.MeId, TestContext.Current.CancellationToken);
        Assert.False(vm.IsSprintEndingSoon);
    }

    [Fact]
    public async Task No_notice_when_the_sprint_has_more_than_three_days_left()
    {
        _w.Sprints.Active("atl")!.End = TestWorld.Now.Date.AddDays(6);
        var vm = await Activated();
        Assert.Equal(6, vm.DaysLeft);
        Assert.False(vm.IsSprintEndingSoon);
    }

    [Fact]
    public async Task My_work_lists_my_top_open_issues_by_priority()
    {
        var vm = await Activated();
        Assert.True(vm.HasMyWork);
        Assert.InRange(vm.MyWork.Count, 1, DashboardViewModel.MyWorkCount);
        Assert.All(vm.MyWork, r =>
        {
            Assert.Equal("Maya Chen", r.AssigneeName);
            Assert.True(r.Issue.IsOpen);
            Assert.Equal("atl", r.Issue.ProjectId);
        });
        var priorities = vm.MyWork.Select(r => r.Priority).ToList();
        Assert.Equal(priorities.OrderByDescending(p => p), priorities);
        Assert.Equal(MyOpenIn("atl").Max(i => i.Priority), vm.MyWork[0].Priority);
    }

    [Fact]
    public async Task Selecting_my_work_or_activity_opens_the_issue()
    {
        var vm = await Activated();
        await vm.OpenIssueCommand.ExecuteAsync(vm.MyWork[0].Key);
        Assert.Equal((Routes.IssueDetail, (object?)vm.MyWork[0].Key), _w.Navigator.History[^1]);
    }

    [Fact]
    public async Task Recent_activity_is_newest_first_and_names_the_issue()
    {
        var vm = await Activated();
        Assert.Equal(DashboardViewModel.ActivityCount, vm.Activity.Count);
        var times = vm.Activity.Select(a => a.Event.When).ToList();
        Assert.Equal(times.OrderByDescending(t => t), times);
        Assert.All(vm.Activity, a =>
        {
            Assert.StartsWith(a.ActorName, a.Text, StringComparison.Ordinal);
            Assert.Contains(a.IssueKey, a.Text, StringComparison.Ordinal);
            Assert.DoesNotContain(" it ", a.Text, StringComparison.Ordinal);
            Assert.StartsWith("ATL-", a.IssueKey, StringComparison.Ordinal);
        });
    }

    [Theory]
    [InlineData(ActivityKind.Created, "created the issue", "Maya Chen created ATL-1")]
    [InlineData(ActivityKind.Commented, "commented", "Maya Chen commented on ATL-1")]
    [InlineData(ActivityKind.StatusChanged, "moved it to In review", "Maya Chen moved ATL-1 to In review")]
    [InlineData(ActivityKind.Edited, "changed the due date", "Maya Chen changed the due date on ATL-1")]
    public void Activity_text_reads_on_its_own(ActivityKind kind, string stored, string expected)
    {
        var row = ActivityRow.From(new ActivityEvent("a", "ATL-1", MockDataSeed.MeId, kind, stored, TestWorld.Now.AddHours(-3)), _w.People, TestWorld.Now);
        Assert.Equal(expected, row.Text);
        Assert.Equal("3 h ago", row.WhenText);
    }

    [Fact]
    public async Task The_ai_summary_is_generated_on_activation_and_can_be_regenerated()
    {
        var vm = await Activated();
        Assert.False(vm.IsSummarizing);
        Assert.True(vm.HasSummary);
        Assert.Contains("Sprint 23", vm.SummaryText, StringComparison.Ordinal);
        Assert.NotEmpty(vm.SummaryHighlights);
        Assert.True(vm.RegenerateSummaryCommand.CanExecute(null));

        await vm.RegenerateSummaryCommand.ExecuteAsync(null);
        Assert.True(vm.HasSummary);
        Assert.False(vm.IsSummarizing);
    }

    [Fact]
    public async Task Switching_project_refreshes_everything_and_resummarizes()
    {
        var vm = await Activated();
        _w.Workspace.SelectProject("bil");
        Assert.Equal("Billing platform", vm.ProjectName);
        Assert.Equal("Billing platform · Sprint 35", vm.ContextText);
        Assert.All(vm.Activity, a => Assert.StartsWith("BIL-", a.IssueKey, StringComparison.Ordinal));
        Assert.Contains("Sprint 35", vm.SummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_project_without_a_sprint_shows_no_sprint_numbers()
    {
        var vm = await Activated();
        _w.Workspace.SelectProject("ins");
        Assert.False(vm.HasActiveSprint);
        Assert.False(vm.IsSprintEndingSoon);
        Assert.False(vm.PointsAreForSprint);
        Assert.Equal("—", vm.PointsText);
        Assert.Equal("Insights dashboards", vm.ContextText);
        Assert.Contains("no active sprint", vm.NoSprintText, StringComparison.Ordinal);
        Assert.Contains("no active sprint", vm.SummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Issue_changes_refresh_the_numbers()
    {
        var vm = await Activated();
        var before = vm.MyOpenCount;
        var issue = _w.Issues.All.First(i => i.ProjectId == "atl" && i.IsOpen && i.AssigneeId != MockDataSeed.MeId);
        await _w.Issues.AssignAsync([issue], MockDataSeed.MeId, TestContext.Current.CancellationToken);
        Assert.Equal(before + 1, vm.MyOpenCount);
        Assert.Contains(vm.Activity, a => a.IssueKey == issue.Key && a.Kind == ActivityKind.Assigned);
    }

    [Fact]
    public async Task Leaving_the_screen_stops_listening()
    {
        var vm = await Activated();
        vm.Deactivate();
        var before = vm.MyOpenCount;
        var issue = _w.Issues.All.First(i => i.ProjectId == "atl" && i.IsOpen && i.AssigneeId != MockDataSeed.MeId);
        await _w.Issues.AssignAsync([issue], MockDataSeed.MeId, TestContext.Current.CancellationToken);
        _w.Workspace.SelectProject("bil");
        Assert.Equal(before, vm.MyOpenCount);
        Assert.Equal("Atlas mobile", vm.ProjectName);
    }

    [Fact]
    public async Task Create_issue_opens_the_wizard()
    {
        var vm = await Activated();
        await vm.CreateIssueCommand.ExecuteAsync(null);
        Assert.Equal(Routes.CreateIssue, _w.Navigator.CurrentRoute);
    }
}
