namespace Maui.Carbon;

/// <summary>The width of a <see cref="CdsSidePanel"/>.</summary>
public enum CdsSidePanelSize
{
    /// <summary>xs: 256. Narrow navigation or a short list.</summary>
    Xs,
    /// <summary>sm: 320.</summary>
    Sm,
    /// <summary>md: 480, the default. Most forms and details.</summary>
    Md,
    /// <summary>lg: 640.</summary>
    Lg,
    /// <summary>xl: 1040. Rich content such as a table; consider a tearsheet.</summary>
    Xl,
    /// <summary>2xl: 1280.</summary>
    Xxl,
}

/// <summary>The page edge a <see cref="CdsSidePanel"/> is attached to.</summary>
public enum CdsSidePanelPlacement
{
    /// <summary>The end edge (right in left-to-right layouts), the default.</summary>
    End,
    /// <summary>The start edge (left in left-to-right layouts).</summary>
    Start,
}
