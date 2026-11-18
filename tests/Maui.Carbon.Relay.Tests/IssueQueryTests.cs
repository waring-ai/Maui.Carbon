namespace Maui.Carbon.Relay.Tests;

public class IssueQueryTests
{
    readonly TestWorld _w = new();

    [Fact]
    public void Seed_is_deterministic()
    {
        var other = new TestWorld();
        Assert.Equal(_w.Db.Issues.Select(i => i.Key), other.Db.Issues.Select(i => i.Key));
        Assert.True(_w.Db.Issues.Count > 90);
        Assert.Equal(_w.Db.Issues.Count, _w.Db.Issues.Select(i => i.Key).Distinct().Count());
    }

    [Fact]
    public void Scope_assigned_to_me_only_returns_my_issues()
    {
        var mine = IssueQuery.Filter(_w.Issues.All, new IssueFilter { Scope = IssueScope.AssignedToMe }, MockDataSeed.MeId).ToList();
        Assert.NotEmpty(mine);
        Assert.All(mine, i => Assert.Equal(MockDataSeed.MeId, i.AssigneeId));
    }

    [Fact]
    public void Search_matches_key_title_and_labels_case_insensitively()
    {
        var issue = _w.Issues.All.First(i => i.Labels.Count > 0);
        Assert.Contains(issue, IssueQuery.Filter(_w.Issues.All, new IssueFilter { SearchText = issue.Key.ToLowerInvariant() }, MockDataSeed.MeId));
        Assert.Contains(issue, IssueQuery.Filter(_w.Issues.All, new IssueFilter { SearchText = issue.Labels[0].ToUpperInvariant() }, MockDataSeed.MeId));
    }

    [Fact]
    public void Unassigned_filter_returns_issues_without_assignee()
    {
        var rows = IssueQuery.Filter(_w.Issues.All, new IssueFilter { AssigneeId = IssueFilter.Unassigned }, MockDataSeed.MeId).ToList();
        Assert.All(rows, i => Assert.Null(i.AssigneeId));
    }

    [Fact]
    public void Filters_combine_with_and_and_sets_with_or()
    {
        var filter = new IssueFilter
        {
            Statuses = new HashSet<IssueStatus> { IssueStatus.Todo, IssueStatus.InProgress },
            Types = new HashSet<IssueType> { IssueType.Bug },
        };
        var rows = IssueQuery.Filter(_w.Issues.All, filter, MockDataSeed.MeId).ToList();
        Assert.All(rows, i => Assert.True(i.Type == IssueType.Bug && i.Status is IssueStatus.Todo or IssueStatus.InProgress));
        Assert.True(filter.HasFilters);
    }

    [Fact]
    public void Ascending_priority_sort_puts_most_urgent_first()
    {
        var rows = IssueQuery.Sort(_w.Issues.All, new IssueSort("priority", false), _w.People.NameOf).ToList();
        Assert.Equal(_w.Issues.All.Max(i => i.Priority), rows[0].Priority);
    }

    [Fact]
    public void Default_sort_is_most_recently_updated_first()
    {
        var rows = IssueQuery.Sort(_w.Issues.All, IssueSort.Default, _w.People.NameOf).ToList();
        Assert.True(rows.Zip(rows.Skip(1)).All(p => p.First.Updated >= p.Second.Updated));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(99, 4)]
    public void Page_clamps_into_range(int requested, int expected)
    {
        var page = IssueQuery.Page(_w.Issues.All.Take(95).ToList(), requested, 25);
        Assert.Equal(expected, page.Page);
        Assert.Equal(4, page.PageCount);
        Assert.Equal(expected == 4 ? 20 : 25, page.Items.Count);
    }

    [Fact]
    public void Empty_list_has_one_empty_page()
    {
        var page = IssueQuery.Page([], 3, 25);
        Assert.Equal(1, page.Page);
        Assert.Equal(1, page.PageCount);
        Assert.Empty(page.Items);
    }
}
