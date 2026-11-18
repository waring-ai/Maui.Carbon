namespace Maui.Carbon.Relay.Tests;

public class BoardRulesTests
{
    static Issue Make(IssueType type = IssueType.Story, IssuePriority priority = IssuePriority.Medium, string? assignee = null, params string[] labels) => new()
    {
        Id = 1,
        Key = "ATL-1",
        ProjectId = "atl",
        Title = "Sample",
        ReporterId = "p02",
        Type = type,
        Priority = priority,
        AssigneeId = assignee,
        Labels = [.. labels],
    };

    [Theory]
    [InlineData(3, null, WipState.Unlimited)]
    [InlineData(0, 5, WipState.Within)]
    [InlineData(4, 5, WipState.Within)]
    [InlineData(5, 5, WipState.AtLimit)]
    [InlineData(6, 5, WipState.Over)]
    public void Wip_compares_count_with_limit(int count, int? limit, WipState expected) =>
        Assert.Equal(expected, BoardRules.Wip(count, limit));

    [Fact]
    public void Wip_text_names_the_state()
    {
        Assert.Equal("WIP limit 8", BoardRules.WipText(3, 8));
        Assert.Equal("At WIP limit: 8", BoardRules.WipText(8, 8));
        Assert.Equal("Over WIP limit: 9 of 8", BoardRules.WipText(9, 8));
        Assert.Equal("No WIP limit", BoardRules.WipText(9, null));
    }

    [Fact]
    public void No_filters_match_everything()
    {
        Assert.True(BoardRules.Matches(Make(), new HashSet<BoardFilterKind>(), "p01"));
    }

    [Fact]
    public void Filters_must_all_match()
    {
        var filters = new HashSet<BoardFilterKind> { BoardFilterKind.Bugs, BoardFilterKind.HighPriority };
        Assert.True(BoardRules.Matches(Make(IssueType.Bug, IssuePriority.Critical), filters, "p01"));
        Assert.False(BoardRules.Matches(Make(IssueType.Bug, IssuePriority.Medium), filters, "p01"));
        Assert.False(BoardRules.Matches(Make(IssueType.Story, IssuePriority.High), filters, "p01"));
    }

    [Fact]
    public void Mine_and_blocked_filters()
    {
        Assert.True(BoardRules.Matches(Make(assignee: "p01"), new HashSet<BoardFilterKind> { BoardFilterKind.Mine }, "p01"));
        Assert.False(BoardRules.Matches(Make(assignee: "p02"), new HashSet<BoardFilterKind> { BoardFilterKind.Mine }, "p01"));
        Assert.True(BoardRules.Matches(Make(labels: ["ios", "Blocked"]), new HashSet<BoardFilterKind> { BoardFilterKind.Blocked }, "p01"));
        Assert.False(BoardRules.Matches(Make(labels: ["ios"]), new HashSet<BoardFilterKind> { BoardFilterKind.Blocked }, "p01"));
    }
}
