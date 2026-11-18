namespace Maui.Carbon.Relay.Models;

/// <summary>Sentence-case display text for the domain enums, shared by every screen and by exports.</summary>
public static class Formatting
{
    public static string Text(this IssueStatus status) => status switch
    {
        IssueStatus.Backlog => "Backlog",
        IssueStatus.Todo => "To do",
        IssueStatus.InProgress => "In progress",
        IssueStatus.InReview => "In review",
        IssueStatus.Done => "Done",
        IssueStatus.Cancelled => "Cancelled",
        _ => status.ToString(),
    };

    public static string Text(this IssuePriority priority) => priority.ToString();

    public static string Text(this IssueType type) => type.ToString();

    public static string Text(this IssueResolution resolution) => resolution switch
    {
        IssueResolution.None => "Unresolved",
        IssueResolution.WontFix => "Won't fix",
        IssueResolution.CannotReproduce => "Cannot reproduce",
        _ => resolution.ToString(),
    };

    public static string Text(this ProjectCategory category) => category.ToString();

    /// <summary>"just now", "5 min ago", "3 h ago", "2 days ago", else the date.</summary>
    public static string Relative(DateTime when, DateTime now)
    {
        var span = now - when;
        if (span < TimeSpan.FromMinutes(1)) return "just now";
        if (span < TimeSpan.FromHours(1)) return $"{(int)span.TotalMinutes} min ago";
        if (span < TimeSpan.FromDays(1)) return $"{(int)span.TotalHours} h ago";
        if (span < TimeSpan.FromDays(7)) return (int)span.TotalDays == 1 ? "yesterday" : $"{(int)span.TotalDays} days ago";
        return when.ToString("MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>The short date used in tables ("Nov 18, 2026").</summary>
    public static string ShortDate(DateTime? when) =>
        when?.ToString("MMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "—";
}
