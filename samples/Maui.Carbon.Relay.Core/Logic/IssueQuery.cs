using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Logic;

/// <summary>The issue list's filters. Empty sets mean "any".</summary>
public sealed record IssueFilter
{
    public IssueScope Scope { get; init; } = IssueScope.All;

    public string? ProjectId { get; init; }

    public string? SearchText { get; init; }

    public IReadOnlySet<IssueStatus> Statuses { get; init; } = new HashSet<IssueStatus>();

    public IReadOnlySet<IssuePriority> Priorities { get; init; } = new HashSet<IssuePriority>();

    public IReadOnlySet<IssueType> Types { get; init; } = new HashSet<IssueType>();

    public IReadOnlySet<string> Labels { get; init; } = new HashSet<string>();

    /// <summary>Only issues assigned to this person; <see cref="Unassigned"/> for issues with no assignee.</summary>
    public string? AssigneeId { get; init; }

    /// <summary>Hide done and cancelled issues.</summary>
    public bool OpenOnly { get; init; }

    public const string Unassigned = "__unassigned";

    /// <summary>True when any filter beyond scope and search narrows the list (drives the "Clear filters" action).</summary>
    public bool HasFilters =>
        Statuses.Count > 0 || Priorities.Count > 0 || Types.Count > 0 || Labels.Count > 0 || AssigneeId is not null || OpenOnly;
}

/// <summary>The sort the issue table asked for: a column key and a direction.</summary>
public readonly record struct IssueSort(string? Key, bool Descending)
{
    public static IssueSort Default => new("updated", true);
}

/// <summary>One page of a filtered, sorted issue list.</summary>
public sealed record IssuePage(IReadOnlyList<Issue> Items, int TotalCount, int Page, int PageSize)
{
    public int PageCount => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

/// <summary>Pure filtering, searching, sorting and paging of issues (unit-tested; no services).</summary>
public static class IssueQuery
{
    /// <summary>Applies <paramref name="filter"/> for the signed-in user <paramref name="meId"/>.</summary>
    public static IEnumerable<Issue> Filter(IEnumerable<Issue> issues, IssueFilter filter, string meId)
    {
        var rows = issues;
        rows = filter.Scope switch
        {
            IssueScope.AssignedToMe => rows.Where(i => i.AssigneeId == meId),
            IssueScope.Watching => rows.Where(i => i.WatcherIds.Contains(meId)),
            _ => rows,
        };
        if (filter.ProjectId is { } project) rows = rows.Where(i => i.ProjectId == project);
        if (filter.Statuses.Count > 0) rows = rows.Where(i => filter.Statuses.Contains(i.Status));
        if (filter.Priorities.Count > 0) rows = rows.Where(i => filter.Priorities.Contains(i.Priority));
        if (filter.Types.Count > 0) rows = rows.Where(i => filter.Types.Contains(i.Type));
        if (filter.Labels.Count > 0) rows = rows.Where(i => i.Labels.Any(filter.Labels.Contains));
        if (filter.OpenOnly) rows = rows.Where(i => i.IsOpen);
        if (filter.AssigneeId is { } assignee)
            rows = assignee == IssueFilter.Unassigned ? rows.Where(i => i.AssigneeId is null) : rows.Where(i => i.AssigneeId == assignee);
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var q = filter.SearchText.Trim();
            rows = rows.Where(i =>
                i.Key.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                i.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                i.Labels.Any(l => l.Contains(q, StringComparison.OrdinalIgnoreCase)));
        }
        return rows;
    }

    /// <summary>Sorts by a table column key. Unknown keys fall back to most recently updated first.</summary>
    public static IEnumerable<Issue> Sort(IEnumerable<Issue> issues, IssueSort sort, Func<string?, string> personName)
    {
        IOrderedEnumerable<Issue> ordered = sort.Key switch
        {
            "key" => Order(issues, i => i.Id, sort.Descending),
            "title" => Order(issues, i => i.Title, sort.Descending, StringComparer.OrdinalIgnoreCase),
            "type" => Order(issues, i => i.Type, sort.Descending),
            "status" => Order(issues, i => i.Status, sort.Descending),
            // Ascending priority reads "most urgent first", as people expect.
            "priority" => Order(issues, i => i.Priority, !sort.Descending),
            "assignee" => Order(issues, i => i.AssigneeId is null ? "￿" : personName(i.AssigneeId), sort.Descending, StringComparer.OrdinalIgnoreCase),
            "points" => Order(issues, i => i.StoryPoints ?? -1, sort.Descending),
            "due" => Order(issues, i => i.DueDate ?? DateTime.MaxValue, sort.Descending),
            "created" => Order(issues, i => i.Created, sort.Descending),
            _ => Order(issues, i => i.Updated, sort.Key is null || sort.Descending),
        };
        return ordered.ThenByDescending(i => i.Id);
    }

    /// <summary>Clamps <paramref name="page"/> (1-based) into range and returns that slice.</summary>
    public static IssuePage Page(IReadOnlyList<Issue> issues, int page, int pageSize)
    {
        pageSize = Math.Max(1, pageSize);
        var pageCount = Math.Max(1, (int)Math.Ceiling(issues.Count / (double)pageSize));
        page = Math.Clamp(page, 1, pageCount);
        var items = issues.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new IssuePage(items, issues.Count, page, pageSize);
    }

    static IOrderedEnumerable<Issue> Order<T>(IEnumerable<Issue> rows, Func<Issue, T> key, bool descending, IComparer<T>? comparer = null) =>
        descending ? rows.OrderByDescending(key, comparer) : rows.OrderBy(key, comparer);
}
