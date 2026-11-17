namespace Maui.Carbon;

/// <summary>The sizes of a <see cref="CdsLink"/> (type sizes: sm 12/16, md 14/18, lg 16/22).</summary>
public enum CdsLinkSize
{
    /// <summary>12px / 16px (<c>label-01</c>).</summary>
    Sm,
    /// <summary>14px / 18px (<c>body-compact-01</c>), the default.</summary>
    Md,
    /// <summary>16px / 22px (<c>body-compact-02</c>).</summary>
    Lg,
}

/// <summary>Color key and decoration of a link in one state.</summary>
/// <param name="Foreground">Text and icon color.</param>
/// <param name="Underline">Whether the text is underlined.</param>
public readonly record struct CdsLinkVisuals(string Foreground, bool Underline);

/// <summary>Maps link state to Carbon tokens (from <c>packages/styles/scss/components/link</c>).</summary>
public static class CdsLinkTokens
{
    /// <summary>
    /// Resolves a link: <c>link-primary</c>; hover <c>link-primary-hover</c> with underline; pressed <c>text-primary</c>
    /// with underline; <c>link-visited</c> when visited (hover still <c>link-primary-hover</c>); disabled
    /// <c>text-disabled</c>. Inline links are always underlined. Inverse links use the <c>link-inverse</c> family.
    /// </summary>
    public static CdsLinkVisuals Resolve(CdsInteractionState state, bool isVisited = false, bool isInline = false, bool isInverse = false)
    {
        if (state.HasFlag(CdsInteractionState.Disabled)) return new(CdsColorKeys.TextDisabled, false);
        if (state.HasFlag(CdsInteractionState.Pressed))
            return new(isInverse ? CdsColorKeys.LinkInverseActive : CdsColorKeys.TextPrimary, true);
        if (state.HasFlag(CdsInteractionState.Hover))
            return new(isInverse ? CdsColorKeys.LinkInverseHover : CdsColorKeys.LinkPrimaryHover, true);
        if (isVisited) return new(isInverse ? CdsColorKeys.LinkInverseVisited : CdsColorKeys.LinkVisited, isInline);
        return new(isInverse ? CdsColorKeys.LinkInverse : CdsColorKeys.LinkPrimary, isInline);
    }

    /// <summary>The type style of a link size.</summary>
    public static CdsTypeStyle TypeStyle(CdsLinkSize size) => size switch
    {
        CdsLinkSize.Sm => CdsTypeStyle.Label01,
        CdsLinkSize.Lg => CdsTypeStyle.BodyCompact02,
        _ => CdsTypeStyle.BodyCompact01,
    };

    /// <summary>The icon size next to the text (always 16).</summary>
    public const double IconSize = 16;

    /// <summary>The gap between text and icon (8).</summary>
    public const double IconGap = CdsSpacing.Spacing03;
}
