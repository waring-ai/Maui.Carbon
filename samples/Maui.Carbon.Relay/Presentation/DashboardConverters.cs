using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>Converters used by the dashboard: activity kind → icon.</summary>
public static class DashboardConverters
{
    public static CdsIconName Icon(ActivityKind kind) => kind switch
    {
        ActivityKind.Created => CdsIconName.DocumentAdd,
        ActivityKind.StatusChanged => CdsIconName.ArrowRight,
        ActivityKind.Assigned => CdsIconName.User,
        ActivityKind.Commented => CdsIconName.Edit,
        ActivityKind.Attached => CdsIconName.Attachment,
        ActivityKind.Resolved => CdsIconName.CheckmarkOutline,
        _ => CdsIconName.SettingsAdjust,
    };

    public static IValueConverter ActivityIcon { get; } = new FuncConverter<ActivityKind, CdsIconName?>(k => Icon(k));
}
