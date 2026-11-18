using System.Globalization;
using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>A running or planned sprint with its work summed up, for the sprint tiles.</summary>
public sealed record BacklogSprint(
    Sprint Sprint,
    int IssueCount,
    int DoneCount,
    int PointsTotal,
    int PointsDone,
    string DatesText,
    string TimeText)
{
    public string Name => Sprint.Name;

    public string Goal => string.IsNullOrWhiteSpace(Sprint.Goal) ? "No goal set" : Sprint.Goal;

    public int OpenCount => IssueCount - DoneCount;

    /// <summary>The progress bar maximum (at least 1, so an empty sprint draws an empty bar).</summary>
    public double ProgressMax => Math.Max(1, PointsTotal);

    public double ProgressValue => PointsDone;

    public string ProgressText => $"{PointsDone} of {PointsTotal} points done";

    public string IssuesText => IssueCount == 1 ? "1 issue" : $"{IssueCount} issues";

    public string DoneText => $"{DoneCount} of {IssuesText} done";

    public string PointsText => PointsTotal == 1 ? "1 point" : $"{PointsTotal} points";

    /// <summary>"Sprint 23 · Nov 6 – Nov 20, 2026".</summary>
    public string HeadingText => $"{Name} · {DatesText}";

    public static string Dates(DateTime start, DateTime end) =>
        $"{start.ToString("MMM d", CultureInfo.InvariantCulture)} – {end.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)}";
}

/// <summary>A completed sprint in the history list: committed against completed points.</summary>
public sealed record SprintHistoryRow(Sprint Sprint)
{
    public string Name => Sprint.Name;

    public string Goal => Sprint.Goal;

    public string DatesText => BacklogSprint.Dates(Sprint.Start, Sprint.End);

    public int Committed => Sprint.CommittedPoints;

    public int Completed => Sprint.CompletedPoints;

    public double ProgressMax => Math.Max(1, Committed);

    public double ProgressValue => Math.Min(Completed, Math.Max(1, Committed));

    /// <summary>Completed as a whole percentage of committed.</summary>
    public int Percent => Committed == 0 ? 0 : (int)Math.Round(100.0 * Completed / Committed);

    public string CompletionText => $"{Completed} of {Committed} committed points completed ({Percent}%)";

    /// <summary>All committed work was finished.</summary>
    public bool MetCommitment => Completed >= Committed;

    public string OutcomeText => MetCommitment ? "Commitment met" : Committed - Completed == 1 ? "1 point carried over" : $"{Committed - Completed} points carried over";
}
