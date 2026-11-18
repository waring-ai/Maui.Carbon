using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Logic;

/// <summary>One bar of a report chart.</summary>
public sealed record ReportBucket(string Label, DateTime Start, DateTime End, int Created, int Resolved)
{
    public int Net => Created - Resolved;
}

/// <summary>Throughput and workload numbers for the reports and dashboard (pure, unit-tested).</summary>
public static class ReportMath
{
    /// <summary>Buckets created and resolved issues ending at <paramref name="now"/>: days for a week, weeks for a month or quarter.</summary>
    public static IReadOnlyList<ReportBucket> Throughput(IEnumerable<Issue> issues, ReportRange range, DateTime now)
    {
        var (count, step, label) = range switch
        {
            ReportRange.Week => (7, TimeSpan.FromDays(1), (Func<DateTime, string>)(d => d.ToString("ddd", System.Globalization.CultureInfo.InvariantCulture))),
            ReportRange.Month => (5, TimeSpan.FromDays(7), d => d.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture)),
            _ => (13, TimeSpan.FromDays(7), d => d.ToString("MMM d", System.Globalization.CultureInfo.InvariantCulture)),
        };
        var list = issues.ToList();
        var end = now.Date.AddDays(1);
        var buckets = new List<ReportBucket>();
        for (var i = count - 1; i >= 0; i--)
        {
            var bEnd = end - step * i;
            var bStart = bEnd - step;
            var created = list.Count(x => x.Created >= bStart && x.Created < bEnd);
            var resolved = list.Count(x => !x.IsOpen && x.Updated >= bStart && x.Updated < bEnd);
            buckets.Add(new ReportBucket(label(bStart), bStart, bEnd, created, resolved));
        }
        return buckets;
    }

    /// <summary>Open issues and open points per assignee, busiest first.</summary>
    public static IReadOnlyList<(string? AssigneeId, int Open, int Points)> Workload(IEnumerable<Issue> issues) =>
        issues.Where(i => i.IsOpen)
            .GroupBy(i => i.AssigneeId)
            .Select(g => (g.Key, g.Count(), g.Sum(i => i.StoryPoints ?? 0)))
            .OrderByDescending(t => t.Item3)
            .ThenBy(t => t.Key)
            .ToList();

    /// <summary>Mean completed points of the given sprints (0 when none have completed).</summary>
    public static double Velocity(IEnumerable<Sprint> sprints)
    {
        var done = sprints.Where(s => s.State == SprintState.Completed).ToList();
        return done.Count == 0 ? 0 : done.Average(s => s.CompletedPoints);
    }

    /// <summary>Fraction 0..1 of a sprint's points that are done.</summary>
    public static double SprintProgress(IEnumerable<Issue> sprintIssues)
    {
        var list = sprintIssues.ToList();
        var total = list.Sum(i => i.StoryPoints ?? 0);
        return total == 0 ? 0 : list.Where(i => i.Status == IssueStatus.Done).Sum(i => i.StoryPoints ?? 0) / (double)total;
    }
}
