using System.Globalization;
using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>A one-way converter from a lambda (XAML uses the static instances on <see cref="Converters"/>).</summary>
public sealed class FuncConverter<TIn, TOut>(Func<TIn, TOut> convert) : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TIn v ? convert(v) : default;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Converters for XAML: <c>Converter="{x:Static p:Converters.StatusTag}"</c>.</summary>
public static class Converters
{
    public static IValueConverter StatusTag { get; } = new FuncConverter<IssueStatus, CdsTagType>(CarbonMap.Tag);

    public static IValueConverter StatusIndicator { get; } = new FuncConverter<IssueStatus, CdsIconIndicatorKind>(CarbonMap.Indicator);

    public static IValueConverter PriorityShape { get; } = new FuncConverter<IssuePriority, CdsShapeIndicatorKind>(CarbonMap.Shape);

    public static IValueConverter TypeTag { get; } = new FuncConverter<IssueType, CdsTagType>(CarbonMap.Tag);

    public static IValueConverter TypeIcon { get; } = new FuncConverter<IssueType, CdsIconName?>(t => CarbonMap.Icon(t));

    public static IValueConverter FeedbackNotification { get; } = new FuncConverter<FeedbackKind, CdsNotificationKind>(CarbonMap.Notification);

    public static IValueConverter FeedbackIndicator { get; } = new FuncConverter<FeedbackKind, CdsIconIndicatorKind>(CarbonMap.Indicator);

    public static IValueConverter CategoryIcon { get; } = new FuncConverter<ProjectCategory, CdsIconName?>(c => CarbonMap.Icon(c));

    public static IValueConverter CategoryTag { get; } = new FuncConverter<ProjectCategory, CdsTagType>(CarbonMap.Tag);

    public static IValueConverter RoleTag { get; } = new FuncConverter<TeamRole, CdsTagType>(CarbonMap.Tag);

    public static IValueConverter Not { get; } = new FuncConverter<bool, bool>(b => !b);

    /// <summary>True when an int is above zero (counts → visibility).</summary>
    public static IValueConverter Positive { get; } = new FuncConverter<int, bool>(n => n > 0);

    /// <summary>True when a string has text.</summary>
    public static IValueConverter HasText { get; } = new FuncConverter<string, bool>(s => !string.IsNullOrWhiteSpace(s));

    /// <summary>Busy → an active inline loading indicator.</summary>
    public static IValueConverter BusyLoading { get; } = new FuncConverter<bool, CdsInlineLoadingStatus>(b => b ? CdsInlineLoadingStatus.Active : CdsInlineLoadingStatus.Inactive);

    /// <summary>An int count as a nullable badge count (0 hides the badge).</summary>
    public static IValueConverter BadgeCount { get; } = new FuncConverter<int, int?>(n => n > 0 ? n : null);
}
