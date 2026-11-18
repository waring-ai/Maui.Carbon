using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Logic;

/// <summary>The create-issue wizard's working copy, validated per step by <see cref="WizardValidation"/>.</summary>
public sealed class IssueDraft
{
    public IssueType? Type { get; set; }

    public string? ProjectId { get; set; }

    public string Title { get; set; } = "";

    public string Description { get; set; } = "";

    public string? Component { get; set; }

    public List<string> Labels { get; set; } = [];

    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    public string? AssigneeId { get; set; }

    public int StoryPoints { get; set; } = 3;

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public TimeSpan? ReminderTime { get; set; }

    public bool Watch { get; set; } = true;

    public bool NotifyAssignee { get; set; } = true;

    public bool NotifyWatchers { get; set; }

    /// <summary>True when the user has entered anything worth confirming before discarding.</summary>
    public bool IsDirty =>
        Type is not null || !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Description) || Labels.Count > 0 ||
        DueDate is not null;
}

/// <summary>A validation message for one field: an error blocks the step, a warning does not.</summary>
public sealed record FieldMessage(string Field, string Text, bool IsError);

/// <summary>Per-step validation of the create-issue wizard. Steps: 0 type, 1 details, 2 schedule, 3 review.</summary>
public static class WizardValidation
{
    public const int TitleMaxLength = 80;

    public static IReadOnlyList<FieldMessage> Validate(IssueDraft draft, int step, DateTime today)
    {
        var messages = new List<FieldMessage>();
        if (step == 0 || step == 3)
        {
            if (draft.Type is null) messages.Add(new("type", "Choose an issue type.", true));
            if (draft.ProjectId is null) messages.Add(new("project", "Choose a project.", true));
        }
        if (step == 1 || step == 3)
        {
            var title = draft.Title.Trim();
            if (title.Length == 0) messages.Add(new("title", "Enter a summary.", true));
            else if (title.Length < 8) messages.Add(new("title", "Use at least 8 characters so the issue can be found later.", true));
            else if (title.Length > TitleMaxLength) messages.Add(new("title", $"Keep the summary to {TitleMaxLength} characters.", true));
            if (draft.Type == IssueType.Bug && string.IsNullOrWhiteSpace(draft.Description))
                messages.Add(new("description", "Bugs without steps to reproduce take longer to fix.", false));
            if (draft.Labels.Count > 5) messages.Add(new("labels", "Use at most 5 labels.", true));
        }
        if (step == 2 || step == 3)
        {
            if (draft.StartDate is { } start && draft.DueDate is { } due && due < start)
                messages.Add(new("dates", "The due date is before the start date.", true));
            if (draft.DueDate is { } d && d.Date < today.Date)
                messages.Add(new("dates", "The due date is in the past.", false));
            if (draft.StoryPoints > 13)
                messages.Add(new("points", "Issues over 13 points are usually worth splitting.", false));
            if (draft.ReminderTime is not null && draft.DueDate is null)
                messages.Add(new("reminder", "Set a due date for the reminder.", true));
        }
        return messages;
    }

    public static bool CanAdvance(IssueDraft draft, int step, DateTime today) =>
        Validate(draft, step, today).All(m => !m.IsError);
}
