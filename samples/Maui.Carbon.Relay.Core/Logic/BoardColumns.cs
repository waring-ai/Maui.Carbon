using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Logic;

/// <summary>The board's columns and which moves are allowed between them.</summary>
public static class BoardColumns
{
    /// <summary>Board columns in order (backlog and cancelled issues stay off the board).</summary>
    public static IReadOnlyList<IssueStatus> Statuses { get; } =
        [IssueStatus.Todo, IssueStatus.InProgress, IssueStatus.InReview, IssueStatus.Done];

    /// <summary>The statuses a card in <paramref name="from"/> can move to from its menu.</summary>
    public static IReadOnlyList<IssueStatus> MoveTargets(IssueStatus from) =>
        Statuses.Where(s => s != from).ToList();

    /// <summary>Work-in-progress limit per column; null when unlimited.</summary>
    public static int? WipLimit(IssueStatus status) => status switch
    {
        IssueStatus.InProgress => 8,
        IssueStatus.InReview => 5,
        _ => null,
    };
}
