namespace Maui.Carbon;

/// <summary>Empty state size (<c>size</c>): lg for page and panel areas, sm for small containers (cards, side panels).</summary>
public enum CdsEmptyStateSize
{
    /// <summary>64 illustration, <c>body-compact-02</c> title and <c>label-01</c> subtitle.</summary>
    Sm,
    /// <summary>80 illustration, <c>heading-03</c> title and <c>body-01</c> subtitle (the default).</summary>
    Lg,
}

/// <summary>Where the illustration sits relative to the text (<c>illustrationPosition</c>).</summary>
public enum CdsEmptyStateIllustrationPosition
{
    /// <summary>Above the text (the default).</summary>
    Top,
    /// <summary>After the text, at the end of the row.</summary>
    Right,
    /// <summary>Below the text.</summary>
    Bottom,
    /// <summary>Before the text, at the start of the row.</summary>
    Left,
}
