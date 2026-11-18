namespace Maui.Carbon.Relay.Models;

/// <summary>A time-boxed iteration of one project.</summary>
public sealed class Sprint
{
    public required string Id { get; init; }

    public required string ProjectId { get; init; }

    public required string Name { get; init; }

    public string Goal { get; set; } = "";

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public SprintState State { get; set; }

    /// <summary>Story points committed at sprint start (completed sprints keep the number).</summary>
    public int CommittedPoints { get; set; }

    /// <summary>Story points done by the sprint's end (completed sprints only).</summary>
    public int CompletedPoints { get; set; }

    public override string ToString() => Name;
}
