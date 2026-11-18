using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>Converters used by the issue and create-issue screens (the shared map has none for these).</summary>
public static class IssueDetailConverters
{
    /// <summary>Priority as a tag color; the tag always carries the priority word too.</summary>
    public static CdsTagType PriorityTagType(IssuePriority priority) => priority switch
    {
        IssuePriority.Critical => CdsTagType.Red,
        IssuePriority.High => CdsTagType.Magenta,
        IssuePriority.Medium => CdsTagType.Purple,
        IssuePriority.Low => CdsTagType.CoolGray,
        _ => CdsTagType.Gray,
    };

    /// <summary>The icon on an activity timeline entry.</summary>
    public static CdsIconName ActivityIconName(ActivityKind kind) => kind switch
    {
        ActivityKind.Created => CdsIconName.Add,
        ActivityKind.StatusChanged => CdsIconName.Renew,
        ActivityKind.Assigned => CdsIconName.User,
        ActivityKind.Commented => CdsIconName.Edit,
        ActivityKind.Attached => CdsIconName.Attachment,
        ActivityKind.Resolved => CdsIconName.CheckmarkOutline,
        _ => CdsIconName.Edit,
    };

    public static IValueConverter PriorityTag { get; } = new FuncConverter<IssuePriority, CdsTagType>(PriorityTagType);

    public static IValueConverter StatusIcon { get; } = new FuncConverter<IssueStatus, CdsIconName?>(s => s switch
    {
        IssueStatus.Done => CdsIconName.CheckmarkOutline,
        IssueStatus.Cancelled => CdsIconName.Close,
        IssueStatus.InProgress or IssueStatus.InReview => CdsIconName.Renew,
        _ => CdsIconName.Time,
    });

    public static IValueConverter ActivityIcon { get; } = new FuncConverter<ActivityKind, CdsIconName?>(k => ActivityIconName(k));
}
