using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>A comment on the issue screen, with the author resolved.</summary>
public sealed record CommentRow(string Id, string Author, string Initials, string Body, string WhenText, bool IsMine);

/// <summary>One entry of the issue's activity timeline.</summary>
public sealed record IssueActivityRow(string Actor, string Text, ActivityKind Kind, string WhenText, bool IsLast)
{
    /// <summary>"Maya Chen moved it to In review".</summary>
    public string Sentence => $"{Actor} {Text}";
}

/// <summary>A file already attached to the issue.</summary>
public sealed record AttachmentRow(string Name, string SizeText, string Uploader, string WhenText)
{
    public string Details => $"{SizeText} · {Uploader} · {WhenText}";
}

/// <summary>An entry of the status select (the text is sentence case).</summary>
public sealed record StatusOption(IssueStatus Status, string Text)
{
    public override string ToString() => Text;
}
