namespace Maui.Carbon.Relay.Tests;

public class TeamViewModelTests
{
    readonly TestWorld _w = new();

    TeamViewModel Create() => new(_w.People, _w.Issues, _w.Navigator);

    [Fact]
    public async Task Activation_lists_everyone_and_selects_the_signed_in_user()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        Assert.Equal(_w.People.All.Count, vm.Rows.Count);
        Assert.Equal("12 people", vm.CountText);
        Assert.Equal(MockDataSeed.MeId, vm.Selected?.Id);
    }

    [Fact]
    public async Task Rows_count_each_persons_open_issues()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        foreach (var row in vm.Rows)
            Assert.Equal(_w.Issues.All.Count(i => i.IsOpen && i.AssigneeId == row.Id), row.OpenIssues);
    }

    [Fact]
    public async Task Search_matches_role_location_and_skills()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.SearchText = "designer";
        Assert.Equal(["Amara Nwosu", "Sofia Alvarez"], vm.Rows.Select(r => r.Name));
        vm.SearchText = "tokyo";
        Assert.Equal(["Hana Sato"], vm.Rows.Select(r => r.Name));
        vm.SearchText = "kotlin";
        Assert.Equal(["Lucas Moreau"], vm.Rows.Select(r => r.Name));
    }

    [Fact]
    public async Task Selection_follows_the_search_and_survives_when_visible()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.Select("p05");
        Assert.Equal("Sofia Alvarez", vm.Selected?.Name);
        vm.SearchText = "design";
        Assert.Equal("Sofia Alvarez", vm.Selected?.Name);
        vm.SearchText = "berlin";
        Assert.Equal("Jonas Weber", vm.Selected?.Name);
        vm.SearchText = "nobody here";
        Assert.Null(vm.Selected);
        Assert.False(vm.HasSelection);
        Assert.True(vm.IsEmpty);
    }

    [Fact]
    public async Task Detail_lists_the_selected_persons_open_issues_most_urgent_first()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var person = vm.Rows.OrderByDescending(r => r.OpenIssues).First();
        vm.Select(person.Id);
        Assert.Equal(person.OpenIssues, vm.SelectedIssueTotal);
        Assert.Equal(Math.Min(person.OpenIssues, TeamViewModel.MaxIssueLinks), vm.SelectedIssues.Count);
        Assert.Equal(person.OpenIssues > TeamViewModel.MaxIssueLinks, vm.HasMoreIssues);
        var priorities = vm.SelectedIssues.Select(i => (int)i.Priority).ToList();
        Assert.Equal(priorities.OrderDescending(), priorities);
    }

    [Fact]
    public async Task Issue_changes_refresh_the_counts()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var before = vm.Selected!.OpenIssues;
        var issue = _w.Issues.All.First(i => i.IsOpen && i.AssigneeId != MockDataSeed.MeId);
        await _w.Issues.AssignAsync([issue], MockDataSeed.MeId, TestContext.Current.CancellationToken);
        Assert.Equal(before + 1, vm.Selected!.OpenIssues);
        Assert.Equal(before + 1, vm.SelectedIssueTotal);
    }

    [Fact]
    public async Task Opening_an_issue_link_navigates_to_it()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var link = vm.SelectedIssues.First();
        await vm.OpenIssueCommand.ExecuteAsync(link);
        Assert.Equal((Routes.IssueDetail, (object?)link.Key), _w.Navigator.History[^1]);
    }

    [Fact]
    public async Task Leaving_stops_listening_for_issue_changes()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.Deactivate();
        var before = vm.Selected!.OpenIssues;
        var issue = _w.Issues.All.First(i => i.IsOpen && i.AssigneeId != MockDataSeed.MeId);
        await _w.Issues.AssignAsync([issue], MockDataSeed.MeId, TestContext.Current.CancellationToken);
        Assert.Equal(before, vm.Selected!.OpenIssues);
    }
}
