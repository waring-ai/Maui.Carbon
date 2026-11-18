using System.Globalization;
using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>A workspace member prepared for the team table and detail panel.</summary>
public sealed record TeamMemberRow(Person Person, int OpenIssues)
{
    public string Id => Person.Id;

    public string Name => Person.Name;

    public string Initials => Person.Initials;

    public TeamRole Role => Person.Role;

    public string RoleText => Person.RoleText;

    public string JobTitle => Person.Title;

    public string Location => Person.Location;

    public string Email => Person.Email;

    public string MailTo => $"mailto:{Person.Email}";

    public IReadOnlyList<string> Skills => Person.Skills;

    public string OpenIssuesText => OpenIssues.ToString(CultureInfo.InvariantCulture);

    /// <summary>The avatar tooltip: name and where they work from.</summary>
    public string AvatarTooltip => $"{Name} · {Location}";
}

/// <summary>An open issue assigned to the selected person (a link in the detail panel).</summary>
public sealed record TeamIssueLink(string Key, string Title, IssueStatus Status, IssuePriority Priority)
{
    public string Text => $"{Key} {Title}";

    public string StatusText => Status.Text();
}
