namespace Maui.Carbon;

/// <summary>The gutter mode of a <see cref="CdsGrid"/>.</summary>
public enum CdsGridMode
{
    /// <summary>The default: a 32 gutter (16 each side of a column) for most layouts and text.</summary>
    Wide,
    /// <summary>No gutter before each column, so content hangs 16 into the gutter (type next to imagery).</summary>
    Narrow,
    /// <summary>A 1 gutter for tiles and data that read as one unit.</summary>
    Condensed,
}

/// <summary>Where a <see cref="CdsGrid"/> sits when it is wider than its 1584 maximum.</summary>
public enum CdsGridAlign
{
    /// <summary>Centered (default).</summary>
    Center,
    /// <summary>Pinned to the start edge.</summary>
    Start,
    /// <summary>Pinned to the end edge.</summary>
    End,
}
