namespace Maui.Carbon;

/// <summary>Where a <see cref="CdsTagSet"/> places its row of tags in its width.</summary>
public enum CdsTagSetAlign
{
    /// <summary>At the start (the default).</summary>
    Start,
    /// <summary>Centered.</summary>
    Center,
    /// <summary>At the end.</summary>
    End,
}

/// <summary>How the overflow popover of a <see cref="CdsTagSet"/> lists the hidden tags.</summary>
public enum CdsTagSetOverflowType
{
    /// <summary>As plain text lines (the default).</summary>
    Default,
    /// <summary>As tags on <c>background-inverse-hover</c>.</summary>
    Tag,
}
