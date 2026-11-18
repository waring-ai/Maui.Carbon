using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>One entry of the dashboard's recent activity stream: who did what to which issue, and when.</summary>
public sealed record ActivityRow(ActivityEvent Event, string ActorName, string Text, string WhenText)
{
    public string IssueKey => Event.IssueKey;

    public ActivityKind Kind => Event.Kind;

    /// <summary>
    /// Builds the row. The stored text is written from the issue's point of view ("moved it to In review"), so the
    /// issue key is put in place of "it" to read on its own ("Hana Sato moved ATL-142 to In review").
    /// </summary>
    public static ActivityRow From(ActivityEvent e, IPeopleService people, DateTime now)
    {
        var actor = people.NameOf(e.ActorId);
        return new ActivityRow(e, actor, $"{actor} {Describe(e)}", Formatting.Relative(e.When, now));
    }

    static string Describe(ActivityEvent e)
    {
        var text = e.Text.Trim();
        if (text == "created the issue") return $"created {e.IssueKey}";
        if (text == "commented") return $"commented on {e.IssueKey}";
        var it = text.IndexOf(" it ", StringComparison.Ordinal);
        if (it >= 0) return $"{text[..it]} {e.IssueKey} {text[(it + 4)..]}";
        if (text.EndsWith(" it", StringComparison.Ordinal)) return $"{text[..^3]} {e.IssueKey}";
        return $"{text} on {e.IssueKey}";
    }
}
