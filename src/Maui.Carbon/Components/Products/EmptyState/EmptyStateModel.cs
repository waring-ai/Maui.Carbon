namespace Maui.Carbon;

/// <summary>The grid cells of an empty state's illustration and text for a position (pure, tested).</summary>
internal readonly record struct EmptyStatePlacement(bool IsRow, int IllustrationIndex, int ContentIndex);

/// <summary>Empty state layout rules (pure, tested).</summary>
internal static class EmptyStateModel
{
    /// <summary>
    /// The flex direction of a position: top is a column (illustration first), bottom a reversed column (text first),
    /// left a row (illustration first) and right a reversed row (text first). The index is the row in a column and the
    /// column in a row.
    /// </summary>
    internal static EmptyStatePlacement Place(CdsEmptyStateIllustrationPosition position) => position switch
    {
        CdsEmptyStateIllustrationPosition.Bottom => new(false, 1, 0),
        CdsEmptyStateIllustrationPosition.Left => new(true, 0, 1),
        CdsEmptyStateIllustrationPosition.Right => new(true, 1, 0),
        _ => new(false, 0, 1),
    };

    /// <summary>Whether the action button shows: it needs text (Carbon also needs an <c>onClick</c>).</summary>
    internal static bool ShowsAction(string? text) => !string.IsNullOrEmpty(text);

    /// <summary>Whether the link shows: it needs text (Carbon also needs an <c>href</c>; here a command or handler will do).</summary>
    internal static bool ShowsLink(string? text) => !string.IsNullOrEmpty(text);
}
