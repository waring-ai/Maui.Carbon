namespace Maui.Carbon.Relay.Models;

/// <summary>A member of the workspace.</summary>
public sealed record Person(
    string Id,
    string Name,
    string Email,
    TeamRole Role,
    string Title,
    string Location,
    IReadOnlyList<string> Skills)
{
    /// <summary>Two-letter initials for avatars ("Maya Chen" → "MC").</summary>
    public string Initials => string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => p[0]));

    /// <summary>The role in sentence case for tags and tables.</summary>
    public string RoleText => Role switch
    {
        TeamRole.Engineer => "Engineer",
        TeamRole.Designer => "Designer",
        TeamRole.ProductManager => "Product manager",
        TeamRole.QualityEngineer => "Quality engineer",
        TeamRole.EngineeringManager => "Engineering manager",
        _ => Role.ToString(),
    };

    public override string ToString() => Name;
}
