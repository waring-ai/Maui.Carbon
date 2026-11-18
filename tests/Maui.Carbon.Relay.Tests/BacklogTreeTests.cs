namespace Maui.Carbon.Relay.Tests;

public class BacklogTreeTests
{
    static int _id;

    static Issue Make(string key, IssueType type = IssueType.Story, string? parent = null, IssueStatus status = IssueStatus.Backlog, string? sprint = null, IssuePriority priority = IssuePriority.Medium, int? points = 3) => new()
    {
        Id = ++_id,
        Key = key,
        ProjectId = "atl",
        Title = $"Title of {key}",
        ReporterId = "p02",
        Type = type,
        ParentKey = parent,
        Status = status,
        SprintId = sprint,
        Priority = priority,
        StoryPoints = type == IssueType.Epic ? null : points,
    };

    [Fact]
    public void Groups_backlog_issues_under_their_epic_with_unassigned_last()
    {
        var issues = new[]
        {
            Make("ATL-1", IssueType.Epic, status: IssueStatus.Done),
            Make("ATL-2", IssueType.Epic, status: IssueStatus.InProgress),
            Make("ATL-3", IssueType.Epic),
            Make("ATL-10", parent: "ATL-1", priority: IssuePriority.Low),
            Make("ATL-11", parent: "ATL-1", priority: IssuePriority.Critical),
            Make("ATL-12", parent: "ATL-2", type: IssueType.Task),
            Make("ATL-13"),
            Make("ATL-14", parent: "ATL-99"),
            Make("ATL-15", IssueType.Bug, parent: "ATL-13"),
        };

        var tree = BacklogTree.Build(issues, activeSprintId: null);

        Assert.Equal(["ATL-1", "ATL-2", null], tree.Select(b => b.EpicKey));
        Assert.Equal(["ATL-11", "ATL-10"], tree[0].Items.Select(i => i.Key)); // most urgent first
        Assert.Equal(BacklogTree.UnassignedTitle, tree[^1].Title);
        Assert.True(tree[^1].IsUnassigned);
        // No parent, a missing parent and a non-epic parent all count as unassigned.
        Assert.Equal(["ATL-13", "ATL-14", "ATL-15"], tree[^1].Items.Select(i => i.Key).Order());
        Assert.Equal(6, tree[0].Points);
        Assert.Equal("ATL-1  Title of ATL-1 · 2 issues", tree[0].Label);
        Assert.Equal("Unassigned to an epic · 3 issues", tree[^1].Label);
    }

    [Fact]
    public void Leaves_out_epics_closed_work_and_the_active_sprint()
    {
        var issues = new[]
        {
            Make("ATL-1", IssueType.Epic),
            Make("ATL-10", parent: "ATL-1", status: IssueStatus.Done),
            Make("ATL-11", parent: "ATL-1", status: IssueStatus.Cancelled),
            Make("ATL-12", parent: "ATL-1", status: IssueStatus.InProgress, sprint: "s-active"),
            Make("ATL-13", status: IssueStatus.Todo, sprint: "s-next"),
        };

        var tree = BacklogTree.Build(issues, "s-active", id => id == "s-next" ? "Sprint 24" : null);

        var branch = Assert.Single(tree);
        Assert.True(branch.IsUnassigned);
        var leaf = Assert.Single(branch.Items);
        Assert.Equal("ATL-13", leaf.Key);
        Assert.Equal("ATL-13  Title of ATL-13 (Sprint 24)", leaf.Label);
    }

    [Fact]
    public void Empty_backlog_builds_no_branches()
    {
        Assert.Empty(BacklogTree.Build([Make("ATL-1", IssueType.Epic)], null));
    }
}
