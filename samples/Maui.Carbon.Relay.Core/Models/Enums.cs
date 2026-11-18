namespace Maui.Carbon.Relay.Models;

/// <summary>Where an issue is in the workflow. The board shows one column per status except <see cref="Cancelled"/>.</summary>
public enum IssueStatus
{
    Backlog,
    Todo,
    InProgress,
    InReview,
    Done,
    Cancelled,
}

/// <summary>How urgent an issue is.</summary>
public enum IssuePriority
{
    Lowest,
    Low,
    Medium,
    High,
    Critical,
}

/// <summary>The kind of work an issue tracks.</summary>
public enum IssueType
{
    Bug,
    Story,
    Task,
    Epic,
}

/// <summary>Why a closed issue was closed.</summary>
public enum IssueResolution
{
    None,
    Fixed,
    WontFix,
    Duplicate,
    CannotReproduce,
}

/// <summary>A person's role on the team.</summary>
public enum TeamRole
{
    Engineer,
    Designer,
    ProductManager,
    QualityEngineer,
    EngineeringManager,
}

/// <summary>Which issues the issue list shows before any filter.</summary>
public enum IssueScope
{
    All,
    AssignedToMe,
    Watching,
}

/// <summary>The lifecycle of a sprint.</summary>
public enum SprintState
{
    Planned,
    Active,
    Completed,
}

/// <summary>The tone of a toast, notification or feed item; mapped to Carbon notification kinds by the app.</summary>
public enum FeedbackKind
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>What a project is: decides its cover art and icon.</summary>
public enum ProjectCategory
{
    Mobile,
    Platform,
    Web,
    Design,
    Data,
}

/// <summary>The themes the user can pick; <see cref="System"/> follows the OS light/dark setting.</summary>
public enum ThemeChoice
{
    White,
    G10,
    G90,
    G100,
    System,
}

/// <summary>The range a report covers.</summary>
public enum ReportRange
{
    Week,
    Month,
    Quarter,
}

/// <summary>What an activity entry records.</summary>
public enum ActivityKind
{
    Created,
    StatusChanged,
    Assigned,
    Commented,
    Attached,
    Edited,
    Resolved,
}
