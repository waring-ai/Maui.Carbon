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
    /// <summary>
    /// For a trail of <paramref name="count"/> items that may show at most <paramref name="maxVisible"/> crumbs (counting the overflow
    /// crumb), the middle run to collapse: keeps the first level and the last <c>maxVisible - 2</c> (at least one). Returns
    /// <c>(0, 0)</c> when nothing collapses (<paramref name="maxVisible"/> 0 or the trail fits).
    /// </summary>
    public static (int Start, int Count) Collapse(int count, int maxVisible)
    {
        if (maxVisible <= 0 || count <= maxVisible) return (0, 0);
        var tail = Math.Max(1, maxVisible - 2);
        var collapsed = count - 1 - tail;
        return collapsed < 1 ? (0, 0) : (1, collapsed);
    }
}
