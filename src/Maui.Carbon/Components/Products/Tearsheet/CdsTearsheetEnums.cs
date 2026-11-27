namespace Maui.Carbon;

/// <summary>Where the influencer column of a <see cref="CdsTearsheet"/> sits.</summary>
public enum CdsTearsheetInfluencerPosition
{
    /// <summary>Before the main content (left in left-to-right layouts), the default: steps, a table of contents.</summary>
    Start,
    /// <summary>After the main content, inside it (right in left-to-right layouts): a preview or a summary.</summary>
    End,
}

/// <summary>The width of the influencer column of a <see cref="CdsTearsheet"/>.</summary>
public enum CdsTearsheetInfluencerWidth
{
    /// <summary>256 wide, the default.</summary>
    Narrow,
    /// <summary>320 wide.</summary>
    Wide,
}
