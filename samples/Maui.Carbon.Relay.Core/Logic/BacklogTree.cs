using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Logic;

/// <summary>A backlog issue as a leaf of the backlog tree.</summary>
public sealed record BacklogLeaf(string Key, string Title, IssueType Type, IssuePriority Priority, int? Points, string? SprintName)
{
    /// <summary>The node label: key, title and, when it is already planned, the sprint.</summary>
    public string Label => SprintName is null ? $"{Key}  {Title}" : $"{Key}  {Title} ({SprintName})";
}

/// <summary>An epic (or the "unassigned" group) with its backlog issues.</summary>
public sealed record BacklogBranch(string? EpicKey, string Title, IReadOnlyList<BacklogLeaf> Items)
{
    public bool IsUnassigned => EpicKey is null;

    public int Points => Items.Sum(i => i.Points ?? 0);

    /// <summary>The node label with its issue count ("ATL-121  Offline visit logging · 3 issues").</summary>
    public string Label
    {
        get
        {
            var count = Items.Count == 1 ? "1 issue" : $"{Items.Count} issues";
            return EpicKey is null ? $"{Title} · {count}" : $"{EpicKey}  {Title} · {count}";
        }
    }
}

/// <summary>Builds the backlog tree: epics with their open child issues, then the issues that belong to no epic.</summary>
public static class BacklogTree
{
    public const string UnassignedTitle = "Unassigned to an epic";

    /// <summary>
    /// Groups the open, non-epic issues of <paramref name="projectIssues"/> that are not in the active sprint under their
    /// epic (<see cref="Issue.ParentKey"/>). Epics without backlog work are left out; issues whose parent is missing or is
    /// not an epic go to the "unassigned" branch, which comes last. Items are ordered most urgent first.
    /// </summary>
    public static IReadOnlyList<BacklogBranch> Build(IEnumerable<Issue> projectIssues, string? activeSprintId, Func<string, string?>? sprintName = null)
    {
        var issues = projectIssues.ToList();
        var epics = issues.Where(i => i.Type == IssueType.Epic).OrderBy(i => i.Id).ToList();
        var epicKeys = epics.Select(e => e.Key).ToHashSet(StringComparer.Ordinal);
        var backlog = issues
            .Where(i => i.Type != IssueType.Epic && i.IsOpen && (activeSprintId is null || i.SprintId != activeSprintId))
            .OrderByDescending(i => i.Priority).ThenBy(i => i.Id)
            .ToList();

        BacklogLeaf Leaf(Issue i) => new(i.Key, i.Title, i.Type, i.Priority, i.StoryPoints, i.SprintId is { } s ? sprintName?.Invoke(s) : null);

        var branches = new List<BacklogBranch>();
        foreach (var epic in epics)
        {
            var children = backlog.Where(i => i.ParentKey == epic.Key).Select(Leaf).ToList();
            if (children.Count > 0) branches.Add(new BacklogBranch(epic.Key, epic.Title, children));
        }
        var orphans = backlog.Where(i => i.ParentKey is null || !epicKeys.Contains(i.ParentKey)).Select(Leaf).ToList();
        if (orphans.Count > 0) branches.Add(new BacklogBranch(null, UnassignedTitle, orphans));
        return branches;
    }
}
