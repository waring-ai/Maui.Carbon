using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// One board column. The four columns live as long as the view model, so a column's busy state survives the card
/// refresh that a move triggers; only <see cref="Cards"/> and the counts change.
/// </summary>
public sealed partial class BoardColumn : ObservableObject
{
    public BoardColumn(IssueStatus status)
    {
        Status = status;
        Title = status.Text();
        WipLimit = BoardColumns.WipLimit(status);
    }

    public IssueStatus Status { get; }

    public string Title { get; }

    public int? WipLimit { get; }

    public bool HasWipLimit => WipLimit is not null;

    /// <summary>The cards shown (after the board filters).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial IReadOnlyList<BoardCard> Cards { get; set; } = [];

    /// <summary>Every card in the column, filtered or not (the WIP limit counts all of them).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Wip), nameof(IsOverLimit), nameof(WipText), nameof(CountText))]
    public partial int Count { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText))]
    public partial int ShownCount { get; set; }

    /// <summary>True while a move into this column is being saved (the column shows a loading overlay).</summary>
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    /// <summary>What the column's spinner announces ("Moving ATL-142 to In review").</summary>
    [ObservableProperty]
    public partial string BusyText { get; set; } = "Saving";

    public WipState Wip => BoardRules.Wip(Count, WipLimit);

    public bool IsOverLimit => Wip == WipState.Over;

    public string WipText => BoardRules.WipText(Count, WipLimit);

    /// <summary>"6", or "2 of 6" while filters hide some cards.</summary>
    public string CountText => ShownCount == Count ? $"{Count}" : $"{ShownCount} of {Count}";

    public bool IsEmpty => Cards.Count == 0;

    public string EmptyText => Count == 0 ? $"Nothing {Title.ToLowerInvariant()}" : "No cards match the filters";
}

/// <summary>A card on the board. <see cref="IsSelected"/> is two-way bound to the selectable tile in select mode.</summary>
public sealed partial class BoardCard : ObservableObject
{
    readonly Action<BoardCard>? _selectionChanged;

    public BoardCard(IssueRow row, bool isSelectMode, bool isSelected, Action<BoardCard>? selectionChanged)
    {
        Row = row;
        IsSelectMode = isSelectMode;
        IsSelected = isSelected;
        _selectionChanged = selectionChanged;
        Actions =
        [
            .. BoardColumns.MoveTargets(row.Status).Select(s => new BoardCardAction(this, $"Move to {s.Text()}", s)),
            new BoardCardAction(this, "Open issue", null),
        ];
    }

    public IssueRow Row { get; }

    public Issue Issue => Row.Issue;

    public string Key => Row.Key;

    public string Title => Row.Title;

    public IssueType Type => Row.Type;

    public string TypeText => Row.TypeText;

    public IssueStatus Status => Row.Status;

    public IssuePriority Priority => Row.Priority;

    public string PriorityText => Row.PriorityText;

    public string AssigneeName => Row.AssigneeName;

    public string AssigneeInitials => Row.AssigneeInitials;

    public bool IsAssigned => Row.IsAssigned;

    public string PointsText => Issue.StoryPoints is { } p ? (p == 1 ? "1 point" : $"{p} points") : "Not estimated";

    public bool IsBlocked => BoardRules.IsBlocked(Issue);

    /// <summary>Up to two labels as tags, then "+N" for the rest.</summary>
    public IReadOnlyList<string> Labels =>
        Issue.Labels.Count > 2 ? [.. Issue.Labels.Take(2), $"+{Issue.Labels.Count - 2}"] : Issue.Labels.ToList();

    public bool HasLabels => Issue.Labels.Count > 0;

    /// <summary>The card's overflow menu: "Move to …" for every other column, then "Open issue".</summary>
    public IReadOnlyList<BoardCardAction> Actions { get; }

    public string MenuDescription => $"Actions for {Key}";

    public bool IsSelectMode { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    partial void OnIsSelectedChanged(bool value) => _selectionChanged?.Invoke(this);
}

/// <summary>An entry in a card's overflow menu: a move (<see cref="MoveTo"/> set) or "Open issue".</summary>
public sealed record BoardCardAction(BoardCard Card, string Text, IssueStatus? MoveTo);

/// <summary>A quick filter tag above the board.</summary>
public sealed partial class BoardFilter(BoardFilterKind kind, string text) : ObservableObject
{
    public BoardFilterKind Kind { get; } = kind;

    public string Text { get; } = text;

    [ObservableProperty]
    public partial bool IsActive { get; set; }
}
