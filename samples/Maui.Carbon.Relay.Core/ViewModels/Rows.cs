using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// An issue prepared for display: names and texts resolved once, so table cells and lists bind to plain properties.
/// Built fresh on every refresh (the underlying <see cref="Issue"/> changes in place).
/// </summary>
public sealed record IssueRow(
    Issue Issue,
    string ProjectName,
    string AssigneeName,
    string AssigneeInitials,
    bool IsOverdue,
    string UpdatedText)
{
    public string Key => Issue.Key;

    public string Title => Issue.Title;

    public IssueType Type => Issue.Type;

    public IssueStatus Status => Issue.Status;

    public IssuePriority Priority => Issue.Priority;

    public string StatusText => Issue.Status.Text();

    public string PriorityText => Issue.Priority.Text();

    public string TypeText => Issue.Type.Text();

    public string PointsText => Issue.StoryPoints?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "—";

    public string DueText => Formatting.ShortDate(Issue.DueDate);

    public bool IsAssigned => Issue.AssigneeId is not null;

    public string LabelsText => Issue.Labels.Count == 0 ? "No labels" : string.Join(", ", Issue.Labels);

    /// <summary>The first paragraph of the description, for previews.</summary>
    public string Summary => Issue.Description.Split("\n\n", 2)[0];

    public static IssueRow From(Issue issue, IPeopleService people, IProjectService projects, DateTime now)
    {
        var assignee = people.Find(issue.AssigneeId);
        return new IssueRow(
            issue,
            projects.Find(issue.ProjectId)?.Name ?? issue.ProjectId,
            assignee?.Name ?? "Unassigned",
            assignee?.Initials ?? "–",
            issue.IsOverdue(now),
            Formatting.Relative(issue.Updated, now));
    }
}

/// <summary>
/// A notification prepared for the header panel. A snapshot (the read state is captured), so two rows are equal only when
/// they show the same thing and the panel can keep the views of rows that did not change.
/// </summary>
public sealed record NotificationRow(AppNotification Notification, string WhenText, bool IsUnread)
{
    public NotificationRow(AppNotification notification, string whenText) : this(notification, whenText, !notification.IsRead)
    {
    }

    public string Title => Notification.Title;

    public string Body => Notification.Body;

    public FeedbackKind Kind => Notification.Kind;
}

/// <summary>An applied filter shown as a dismissible tag; <see cref="Remove"/> clears just that filter.</summary>
public sealed record FilterChip(string Text, Action Remove);
