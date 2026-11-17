namespace Maui.Carbon;

/// <summary>Color keys of a tag. <c>null</c> means transparent.</summary>
/// <param name="Background">Fill.</param>
/// <param name="Border">1px outline (outline tags only).</param>
/// <param name="Foreground">Label and icon color.</param>
/// <param name="Hover">Fill for operational tags and the close button on hover.</param>
public readonly record struct CdsTagVisuals(string? Background, string? Border, string Foreground, string Hover);

/// <summary>Maps tag types and states to Carbon tokens (<c>tag-background-*</c>, <c>tag-color-*</c>, <c>tag-hover-*</c>).</summary>
public static class CdsTagTokens
{
    /// <summary>Resolves the colors of a tag type; disabled tags use <c>layer-01</c> with <c>text-disabled</c>.</summary>
    public static CdsTagVisuals Resolve(CdsTagType type, bool disabled)
    {
        if (disabled) return new(CdsColorKeys.Layer01, null, CdsColorKeys.TextDisabled, CdsColorKeys.Layer01);
        return type switch
        {
            CdsTagType.Red => new(CdsColorKeys.TagBackgroundRed, null, CdsColorKeys.TagColorRed, CdsColorKeys.TagHoverRed),
            CdsTagType.Magenta => new(CdsColorKeys.TagBackgroundMagenta, null, CdsColorKeys.TagColorMagenta, CdsColorKeys.TagHoverMagenta),
            CdsTagType.Purple => new(CdsColorKeys.TagBackgroundPurple, null, CdsColorKeys.TagColorPurple, CdsColorKeys.TagHoverPurple),
            CdsTagType.Blue => new(CdsColorKeys.TagBackgroundBlue, null, CdsColorKeys.TagColorBlue, CdsColorKeys.TagHoverBlue),
            CdsTagType.Cyan => new(CdsColorKeys.TagBackgroundCyan, null, CdsColorKeys.TagColorCyan, CdsColorKeys.TagHoverCyan),
            CdsTagType.Teal => new(CdsColorKeys.TagBackgroundTeal, null, CdsColorKeys.TagColorTeal, CdsColorKeys.TagHoverTeal),
            CdsTagType.Green => new(CdsColorKeys.TagBackgroundGreen, null, CdsColorKeys.TagColorGreen, CdsColorKeys.TagHoverGreen),
            CdsTagType.CoolGray => new(CdsColorKeys.TagBackgroundCoolGray, null, CdsColorKeys.TagColorCoolGray, CdsColorKeys.TagHoverCoolGray),
            CdsTagType.WarmGray => new(CdsColorKeys.TagBackgroundWarmGray, null, CdsColorKeys.TagColorWarmGray, CdsColorKeys.TagHoverWarmGray),
            CdsTagType.HighContrast => new(CdsColorKeys.BackgroundInverse, null, CdsColorKeys.TextInverse, CdsColorKeys.BackgroundInverseHover),
            CdsTagType.Outline => new(null, CdsColorKeys.BackgroundInverse, CdsColorKeys.TextPrimary, CdsColorKeys.LayerHover01),
            _ => new(CdsColorKeys.TagBackgroundGray, null, CdsColorKeys.TagColorGray, CdsColorKeys.TagHoverGray),
        };
    }

    /// <summary>Height of a tag size.</summary>
    public static double Height(CdsTagSize size) => size switch
    {
        CdsTagSize.Sm => 18,
        CdsTagSize.Lg => 32,
        _ => 24,
    };
}
