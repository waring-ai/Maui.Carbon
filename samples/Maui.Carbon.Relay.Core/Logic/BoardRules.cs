using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Logic;

/// <summary>A quick filter on the board. Active filters combine: a card shows only when it matches all of them.</summary>
public enum BoardFilterKind
{
    /// <summary>Only bugs.</summary>
    Bugs,

    /// <summary>Only issues assigned to the signed-in user.</summary>
    Mine,

    /// <summary>Only high and critical priority.</summary>
    HighPriority,

    /// <summary>Only issues labelled <see cref="BoardRules.BlockedLabel"/>.</summary>
    Blocked,
}

/// <summary>Where a column stands against its work-in-progress limit.</summary>
public enum WipState
{
    /// <summary>The column has no limit.</summary>
    Unlimited,

    /// <summary>Below the limit.</summary>
    Within,

    /// <summary>Exactly at the limit: nothing more should be pulled in.</summary>
    AtLimit,

    /// <summary>Above the limit.</summary>
    Over,
}

/// <summary>Which issues the board shows, how its filters match and how a column compares with its WIP limit.</summary>
public static class BoardRules
{
    /// <summary>The label that marks an issue as blocked.</summary>
    public const string BlockedLabel = "blocked";

    /// <summary>The cards of a sprint: its issues in a board column (backlog and cancelled stay off), epics excluded.</summary>
    public static IEnumerable<Issue> SprintIssues(IEnumerable<Issue> all, string sprintId) =>
        all.Where(i => i.SprintId == sprintId && i.Type != IssueType.Epic && BoardColumns.Statuses.Contains(i.Status));

    public static bool IsBlocked(Issue issue) =>
        issue.Labels.Contains(BlockedLabel, StringComparer.OrdinalIgnoreCase);

    /// <summary>True when <paramref name="issue"/> matches every filter in <paramref name="filters"/> (none: everything matches).</summary>
    public static bool Matches(Issue issue, IReadOnlySet<BoardFilterKind> filters, string meId) =>
        filters.All(f => f switch
        {
            BoardFilterKind.Bugs => issue.Type == IssueType.Bug,
            BoardFilterKind.Mine => issue.AssigneeId == meId,
            BoardFilterKind.HighPriority => issue.Priority >= IssuePriority.High,
            BoardFilterKind.Blocked => IsBlocked(issue),
            _ => true,
        });

    /// <summary>The order of cards in a column: most urgent first, then oldest key first.</summary>
    public static IEnumerable<Issue> CardOrder(IEnumerable<Issue> issues) =>
        issues.OrderByDescending(i => i.Priority).ThenBy(i => i.Id);

    /// <summary>How <paramref name="count"/> cards compare with <paramref name="limit"/> (null: unlimited).</summary>
    public static WipState Wip(int count, int? limit) => limit switch
    {
        null => WipState.Unlimited,
        { } l when count > l => WipState.Over,
        { } l when count == l => WipState.AtLimit,
        _ => WipState.Within,
    };

    /// <summary>The WIP tag text; it names the state so it never relies on the tag color.</summary>
    public static string WipText(int count, int? limit) => Wip(count, limit) switch
    {
        WipState.Unlimited => "No WIP limit",
        WipState.Over => $"Over WIP limit: {count} of {limit}",
        WipState.AtLimit => $"At WIP limit: {limit}",
        _ => $"WIP limit {limit}",
    };
}
