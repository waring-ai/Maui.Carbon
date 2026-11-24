namespace Maui.Carbon;

/// <summary>The sizes of a <see cref="CdsBreadcrumb"/>.</summary>
public enum CdsBreadcrumbSize
{
    /// <summary>12px (<c>label-01</c>) with 4 around the separator: page headers and dense areas.</summary>
    Sm,
    /// <summary>14px (<c>body-compact-01</c>) with 8 around the separator (default).</summary>
    Md,
}

/// <summary>Color and decoration of a breadcrumb item in one state.</summary>
/// <param name="Foreground">Text color.</param>
/// <param name="Underline">Whether the text is underlined.</param>
public readonly record struct CdsBreadcrumbVisuals(string Foreground, bool Underline);

/// <summary>Maps breadcrumb item state to Carbon tokens (from <c>packages/styles/scss/components/breadcrumb</c>).</summary>
public static class CdsBreadcrumbTokens
{
    /// <summary>
    /// Links: <c>link-primary</c>, <c>link-primary-hover</c> with underline on hover, <c>text-primary</c> while pressed (visited crumbs stay
    /// <c>link-primary</c>). The current page and plain text items are <c>text-primary</c> and never underline.
    /// </summary>
    public static CdsBreadcrumbVisuals Resolve(CdsInteractionState state, bool isLink)
    {
        if (!isLink) return new(CdsColorKeys.TextPrimary, false);
        var v = CdsLinkTokens.Resolve(state);
        return new(v.Foreground, v.Underline);
    }

    /// <summary>The type style: <c>body-compact-01</c> for md, <c>label-01</c> for sm.</summary>
    public static CdsTypeStyle TypeStyle(CdsBreadcrumbSize size) =>
        size == CdsBreadcrumbSize.Sm ? CdsTypeStyle.Label01 : CdsTypeStyle.BodyCompact01;

    /// <summary>The space on each side of the <c>/</c> separator: 8 for md, 4 for sm.</summary>
    public static double SeparatorGap(CdsBreadcrumbSize size) =>
        size == CdsBreadcrumbSize.Sm ? CdsSpacing.Spacing02 : CdsSpacing.Spacing03;
}

/// <summary>Pure logic for <see cref="CdsBreadcrumb"/>.</summary>
internal static class BreadcrumbModel
{
    /// <summary>The trailing levels a collapsed trail keeps (Carbon: the first level and the last two).</summary>
    public const int KeptTrailingLevels = 2;

    /// <summary>The fewest crumbs a collapsed trail shows: the first level, the overflow crumb and the last two levels.</summary>
    public const int MinVisibleItems = 1 + 1 + KeptTrailingLevels;

    /// <summary>
    /// <see cref="CdsBreadcrumb.MaxVisibleItems"/> as stored: 0 or less turns collapsing off (0), anything else is at least
    /// <see cref="MinVisibleItems"/> (a collapsed trail always shows four crumbs).
    /// </summary>
    public static int CoerceMaxVisible(int maxVisible) => maxVisible <= 0 ? 0 : Math.Max(MinVisibleItems, maxVisible);

    /// <summary>
    /// For a trail of <paramref name="count"/> items that may show at most <paramref name="maxVisible"/> crumbs (counting the overflow
    /// crumb), the middle run to collapse: keeps the first level and the last two, as Carbon does. Returns <c>(0, 0)</c> when
    /// nothing collapses (<paramref name="maxVisible"/> 0 or the trail fits; a limit below four counts as four).
    /// </summary>
    public static (int Start, int Count) Collapse(int count, int maxVisible)
    {
        maxVisible = CoerceMaxVisible(maxVisible);
        if (maxVisible == 0 || count <= maxVisible) return (0, 0);
        return (1, count - 1 - KeptTrailingLevels);
    }
}
