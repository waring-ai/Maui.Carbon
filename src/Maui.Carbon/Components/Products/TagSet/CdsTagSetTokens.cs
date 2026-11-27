namespace Maui.Carbon;

/// <summary>Colors of an entry in the overflow popover of a <see cref="CdsTagSet"/>. <c>null</c> means no fill.</summary>
/// <param name="Background">The entry's fill (a pill for <see cref="CdsTagSetOverflowType.Tag"/>).</param>
/// <param name="Foreground">The entry's text.</param>
public readonly record struct CdsTagSetEntryVisuals(string? Background, string Foreground);

/// <summary>Measurements and tokens of <see cref="CdsTagSet"/>.</summary>
public static class CdsTagSetTokens
{
    /// <summary>Space after and below every tag: 8 (<c>spacing-03</c>).</summary>
    public const double Gap = CdsSpacing.Spacing03;

    /// <summary>Padding of the overflow popover: 16 (<c>spacing-05</c>).</summary>
    public const double PopoverPadding = CdsSpacing.Spacing05;

    /// <summary>Space above "View all tags": 8 (<c>spacing-03</c>).</summary>
    public const double ShowAllGap = CdsSpacing.Spacing03;

    /// <summary>Space between the modal title and its search: 16 (<c>spacing-05</c>).</summary>
    public const double ModalSearchGap = CdsSpacing.Spacing05;

    /// <summary>The widest popover text entry before it truncates (the tag maximum).</summary>
    public const double EntryMaxWidth = 208;

    /// <summary>
    /// Colors of a popover entry on the high-contrast surface: plain <c>text-inverse</c> text for
    /// <see cref="CdsTagSetOverflowType.Default"/>; a <c>background-inverse-hover</c> pill with <c>text-inverse</c> for
    /// <see cref="CdsTagSetOverflowType.Tag"/>.
    /// </summary>
    public static CdsTagSetEntryVisuals Entry(CdsTagSetOverflowType type) => type == CdsTagSetOverflowType.Tag
        ? new(CdsColorKeys.BackgroundInverseHover, CdsColorKeys.TextInverse)
        : new(null, CdsColorKeys.TextInverse);

    /// <summary>Space between popover entries: none for text lines, 8 between tags.</summary>
    public static double EntrySpacing(CdsTagSetOverflowType type) => type == CdsTagSetOverflowType.Tag ? Gap : 0;
}
