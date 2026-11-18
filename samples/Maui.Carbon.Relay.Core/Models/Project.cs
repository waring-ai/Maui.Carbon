namespace Maui.Carbon.Relay.Models;

/// <summary>A project groups issues under one key prefix ("ATL-142").</summary>
public sealed class Project
{
    public required string Id { get; init; }

    /// <summary>The issue key prefix, two to five capital letters.</summary>
    public required string Key { get; init; }

    public required string Name { get; set; }

    public string Description { get; set; } = "";

    public required string LeadId { get; set; }

    public ProjectCategory Category { get; set; }

    public DateTime Created { get; init; }

    public DateTime? TargetDate { get; set; }

    public bool IsStarred { get; set; }

    public bool IsArchived { get; set; }

    /// <summary>The components issues in this project can be filed against.</summary>
    public List<string> Components { get; init; } = [];

    public override string ToString() => Name;
}
