using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>Mappings used by the reports screen: workload level → status indicator.</summary>
public static class ReportsConverters
{
    public static CdsIconIndicatorKind Indicator(WorkloadLevel level) => level switch
    {
        WorkloadLevel.Heavy => CdsIconIndicatorKind.CautionMajor,
        WorkloadLevel.Light => CdsIconIndicatorKind.Informative,
        WorkloadLevel.Unassigned => CdsIconIndicatorKind.Undefined,
        _ => CdsIconIndicatorKind.Normal,
    };
}
