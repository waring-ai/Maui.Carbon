using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>Maps Relay's domain enums (Core knows nothing of Carbon) to Carbon tag colors, indicators and icons.</summary>
public static class CarbonMap
{
    public static CdsTagType Tag(IssueStatus status) => status switch
    {
        IssueStatus.Backlog => CdsTagType.Gray,
        IssueStatus.Todo => CdsTagType.CoolGray,
        IssueStatus.InProgress => CdsTagType.Blue,
        IssueStatus.InReview => CdsTagType.Purple,
        IssueStatus.Done => CdsTagType.Green,
        _ => CdsTagType.WarmGray,
    };

    public static CdsIconIndicatorKind Indicator(IssueStatus status) => status switch
    {
        IssueStatus.Backlog => CdsIconIndicatorKind.NotStarted,
        IssueStatus.Todo => CdsIconIndicatorKind.Pending,
        IssueStatus.InProgress => CdsIconIndicatorKind.InProgress,
        IssueStatus.InReview => CdsIconIndicatorKind.Incomplete,
        IssueStatus.Done => CdsIconIndicatorKind.Succeeded,
        _ => CdsIconIndicatorKind.Unknown,
    };

    public static CdsShapeIndicatorKind Shape(IssuePriority priority) => priority switch
    {
        IssuePriority.Critical => CdsShapeIndicatorKind.Critical,
        IssuePriority.High => CdsShapeIndicatorKind.High,
        IssuePriority.Medium => CdsShapeIndicatorKind.Medium,
        IssuePriority.Low => CdsShapeIndicatorKind.Low,
        _ => CdsShapeIndicatorKind.Stable,
    };

    public static CdsTagType Tag(IssueType type) => type switch
    {
        IssueType.Bug => CdsTagType.Red,
        IssueType.Story => CdsTagType.Teal,
        IssueType.Task => CdsTagType.Blue,
        _ => CdsTagType.Magenta,
    };

    public static CdsIconName Icon(IssueType type) => type switch
    {
        IssueType.Bug => CdsIconName.Bee,
        IssueType.Story => CdsIconName.Document,
        IssueType.Task => CdsIconName.CheckmarkOutline,
        _ => CdsIconName.Plan,
    };

    public static CdsNotificationKind Notification(FeedbackKind kind) => kind switch
    {
        FeedbackKind.Success => CdsNotificationKind.Success,
        FeedbackKind.Warning => CdsNotificationKind.Warning,
        FeedbackKind.Error => CdsNotificationKind.Error,
        _ => CdsNotificationKind.Info,
    };

    public static CdsIconIndicatorKind Indicator(FeedbackKind kind) => kind switch
    {
        FeedbackKind.Success => CdsIconIndicatorKind.Succeeded,
        FeedbackKind.Warning => CdsIconIndicatorKind.CautionMinor,
        FeedbackKind.Error => CdsIconIndicatorKind.Failed,
        _ => CdsIconIndicatorKind.Informative,
    };

    public static CdsIconName Icon(ProjectCategory category) => category switch
    {
        ProjectCategory.Mobile => CdsIconName.VirtualMachine,
        ProjectCategory.Platform => CdsIconName.CloudFoundry1,
        ProjectCategory.Web => CdsIconName.Launch,
        ProjectCategory.Design => CdsIconName.Catalog,
        _ => CdsIconName.Analytics,
    };

    public static CdsTagType Tag(ProjectCategory category) => category switch
    {
        ProjectCategory.Mobile => CdsTagType.Blue,
        ProjectCategory.Platform => CdsTagType.Purple,
        ProjectCategory.Web => CdsTagType.Cyan,
        ProjectCategory.Design => CdsTagType.Magenta,
        _ => CdsTagType.Teal,
    };

    public static CdsTagType Tag(TeamRole role) => role switch
    {
        TeamRole.Engineer => CdsTagType.Blue,
        TeamRole.Designer => CdsTagType.Magenta,
        TeamRole.ProductManager => CdsTagType.Purple,
        TeamRole.QualityEngineer => CdsTagType.Teal,
        _ => CdsTagType.CoolGray,
    };

    public static CdsThemeName? Theme(ThemeChoice choice) => choice switch
    {
        ThemeChoice.White => CdsThemeName.White,
        ThemeChoice.G10 => CdsThemeName.G10,
        ThemeChoice.G90 => CdsThemeName.G90,
        ThemeChoice.G100 => CdsThemeName.G100,
        _ => null,
    };
}
