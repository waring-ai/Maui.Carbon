namespace Maui.Carbon.Relay.Tests;

public class IssuesViewModelTests
{
    readonly TestWorld _w = new();

    IssuesViewModel Create() => new(_w.Issues, _w.People, _w.Projects, _w.Navigator, _w.Feedback, _w.Clock);

    [Fact]
    public async Task Activation_loads_the_first_page()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        Assert.False(vm.IsLoading);
        Assert.Equal(_w.Issues.All.Count, vm.TotalCount);
        Assert.Equal(vm.PageSize, vm.Rows.Count);
    }

    [Fact]
    public async Task Scope_parameter_selects_my_issues()
    {
        var vm = Create();
        await vm.ActivateAsync(IssueScope.AssignedToMe);
        Assert.All(vm.Rows, r => Assert.Equal("Maya Chen", r.AssigneeName));
    }

    [Fact]
    public async Task Filters_show_as_chips_and_chips_remove_their_filter()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.ToggleStatus(IssueStatus.Done, true);
        vm.OpenOnly = true;
        Assert.Equal(0, vm.TotalCount);
        Assert.True(vm.IsEmpty);
        Assert.Equal(2, vm.AppliedFilters.Count);
        vm.AppliedFilters.First(c => c.Text.StartsWith("Open", StringComparison.Ordinal)).Remove();
        Assert.True(vm.TotalCount > 0);
        vm.ClearFiltersCommand.Execute(null);
        Assert.False(vm.HasAppliedFilters);
    }

    [Fact]
    public async Task Delete_asks_first_then_can_be_undone()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var count = vm.TotalCount;
        vm.Selected = vm.Rows.Take(2).ToList();
        vm.RequestDeleteSelectedCommand.Execute(null);
        Assert.True(vm.IsDeleteConfirmOpen);
        Assert.Equal(count, vm.TotalCount);

        await vm.ConfirmDeleteCommand.ExecuteAsync(null);
        Assert.False(vm.IsDeleteConfirmOpen);
        Assert.Equal(count - 2, vm.TotalCount);
        Assert.True(vm.HasUndo);
        Assert.Empty(vm.Selected);

        await vm.UndoDeleteCommand.ExecuteAsync(null);
        Assert.Equal(count, vm.TotalCount);
        Assert.Contains(_w.Feedback.Toasts, t => t.Title.Contains("restored", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Bulk_move_changes_status_and_toasts()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var picked = vm.Rows.Where(r => r.Status != IssueStatus.Done).Take(3).ToList();
        vm.Selected = picked;
        await vm.MoveSelectedCommand.ExecuteAsync(IssueStatus.Done);
        Assert.All(picked, r => Assert.Equal(IssueStatus.Done, r.Issue.Status));
        Assert.Single(_w.Feedback.Toasts);
    }

    [Fact]
    public async Task Opening_a_row_navigates_to_its_detail()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        await vm.OpenIssueCommand.ExecuteAsync(vm.Rows[0]);
        Assert.Equal((Routes.IssueDetail, (object?)vm.Rows[0].Key), _w.Navigator.History[^1]);
    }

    [Fact]
    public async Task Sorting_and_paging_follow_the_table()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.ApplySort("key", descending: false);
        Assert.Equal(_w.Issues.All.Min(i => i.Id), vm.Rows[0].Issue.Id);
        vm.ChangePage(2, 10);
        Assert.Equal(10, vm.Rows.Count);
        Assert.Equal(2, vm.Page);
    }
}
