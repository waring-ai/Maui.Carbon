using System.Globalization;
using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>A project prepared for its card: lead, open issue count and target date resolved once per refresh.</summary>
public sealed record ProjectCard(Project Project, string LeadName, string LeadInitials, int OpenIssues)
{
    public string Id => Project.Id;

    public string Name => Project.Name;

    public string Key => Project.Key;

    public string Description => Project.Description;

    public ProjectCategory Category => Project.Category;

    public string CategoryText => Project.Category.Text();

    public bool IsStarred => Project.IsStarred;

    public bool IsArchived => Project.IsArchived;

    public string LeadText => $"Lead: {LeadName}";

    public string OpenIssuesText => OpenIssues == 1 ? "1 open issue" : $"{OpenIssues.ToString(CultureInfo.InvariantCulture)} open issues";

    public string TargetText => Project.TargetDate is { } date ? $"Target {Formatting.ShortDate(date)}" : "No target date";

    /// <summary>The star button's accessible name and tooltip.</summary>
    public string StarLabel => IsStarred ? $"Remove {Name} from starred" : $"Star {Name}";

    /// <summary>The tile's accessible name: the destination first.</summary>
    public string AccessibleName => $"Open {Name} ({Key}) dashboard";
}

/// <summary>A choice in the new project form's category select.</summary>
public sealed record CategoryOption(ProjectCategory Value, string Text)
{
    public override string ToString() => Text;
}
