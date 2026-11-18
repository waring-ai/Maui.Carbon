using Maui.Carbon.Relay.Logic;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// One group of the created-vs-resolved chart: the bucket, bar heights in device units (0 to
/// <see cref="ReportsViewModel.PlotHeight"/>) and the exact numbers for each bar's tooltip.
/// </summary>
public sealed record ChartBar(ReportBucket Bucket, double CreatedHeight, double ResolvedHeight, string PeriodText)
{
    public string Label => Bucket.Label;

    public int Created => Bucket.Created;

    public int Resolved => Bucket.Resolved;

    public string CreatedTip => $"{PeriodText}: {Created} created";

    public string ResolvedTip => $"{PeriodText}: {Resolved} resolved";

    /// <summary>The accessible description of the whole group.</summary>
    public string Description => $"{PeriodText}: {Created} created, {Resolved} resolved";
}

/// <summary>How loaded a person is compared with the team average.</summary>
public enum WorkloadLevel
{
    Light,
    Balanced,
    Heavy,

    /// <summary>The unassigned bucket: work nobody owns yet.</summary>
    Unassigned,
}

/// <summary>One row of the workload table.</summary>
public sealed record WorkloadRow(string? AssigneeId, string Name, string Role, int Open, int Points, string ShareText, WorkloadLevel Level)
{
    public string LevelText => Level switch
    {
        WorkloadLevel.Heavy => "Over capacity",
        WorkloadLevel.Light => "Has capacity",
        WorkloadLevel.Unassigned => "Needs an owner",
        _ => "Balanced",
    };
}
