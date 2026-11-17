namespace Maui.Carbon;

/// <summary>Colors of one segment of a content switcher. <c>null</c> means none.</summary>
/// <param name="Background">Fill under the segment (hover; the <c>focus-inset</c> fill of a focused unselected segment).</param>
/// <param name="Indicator">Fill of the selection indicator (the high-contrast selected segment); resolved inside a layer for low contrast.</param>
/// <param name="IndicatorBorder">1px edge of the low-contrast selected segment.</param>
/// <param name="Foreground">Label color.</param>
/// <param name="Icon">Icon color.</param>
public readonly record struct CdsSwitchVisuals(string? Background, string? Indicator, string? IndicatorBorder, string Foreground, string Icon);

/// <summary>Maps content switcher state to Carbon tokens (from <c>packages/styles/scss/components/content-switcher</c>).</summary>
public static class CdsContentSwitcherTokens
{
    /// <summary>
    /// Resolves a segment. High contrast: the selected segment fills with <c>layer-selected-inverse</c> and uses
    /// <c>text-inverse</c>. Low contrast: the selected segment sits on the next layer with a <c>border-subtle-01</c> edge and
    /// <c>text-primary</c>. Unselected: <c>text-secondary</c>, <c>layer-hover-01</c> and <c>text-primary</c> on hover or press.
    /// Disabled: <c>text-disabled</c>; a disabled selected segment fills with <c>button-disabled</c> (high contrast).
    /// </summary>
    public static CdsSwitchVisuals Resolve(bool lowContrast, CdsInteractionState state, bool selected)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
        {
            return selected && !lowContrast
                ? new(null, CdsColorKeys.ButtonDisabled, null, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled)
                : new(null, null, null, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled);
        }
        var hover = state.HasFlag(CdsInteractionState.Hover) || state.HasFlag(CdsInteractionState.Pressed);
        var focus = state.HasFlag(CdsInteractionState.FocusVisible);
        var background = hover ? CdsColorKeys.LayerHover01 : focus && !selected ? CdsColorKeys.FocusInset : null;
        if (selected)
        {
            return lowContrast
                ? new(background, CdsColorKeys.Layer01, CdsColorKeys.BorderSubtle01, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary)
                : new(background, CdsColorKeys.LayerSelectedInverse, null, CdsColorKeys.TextInverse, CdsColorKeys.IconInverse);
        }
        return hover
            ? new(background, null, null, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary)
            : new(background, null, null, CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary);
    }

    /// <summary>
    /// The color of the 1px, 16 high divider at the start of a segment, or <c>null</c> for none. There is none before the
    /// first segment or next to a selected disabled one; <c>border-disabled</c> for a disabled segment; none when the
    /// segment or its predecessor is selected, hovered or focused; else <c>border-subtle-01</c>.
    /// </summary>
    public static string? Divider(bool isFirst, bool hiddenByNeighbour, bool disabled, bool previousSelectedAndDisabled)
    {
        if (isFirst || previousSelectedAndDisabled) return null;
        if (disabled) return CdsColorKeys.BorderDisabled;
        return hiddenByNeighbour ? null : CdsColorKeys.BorderSubtle01;
    }

    /// <summary>The container outline: <c>border-inverse</c>, <c>border-disabled</c> when any segment is disabled, none for low contrast.</summary>
    public static string? Outline(bool lowContrast, bool anyDisabled) =>
        lowContrast ? null : anyDisabled ? CdsColorKeys.BorderDisabled : CdsColorKeys.BorderInverse;

    /// <summary>The height of a switcher: sm 32, md 40, lg 48 (other sizes fall back to md).</summary>
    public static double Height(CdsSize size) => size switch
    {
        CdsSize.Sm => CdsSizing.Small,
        CdsSize.Lg => CdsSizing.Large,
        _ => CdsSizing.Medium,
    };

    /// <summary>The inline padding of a text segment: 16, or 14 at lg.</summary>
    public static double InlinePadding(CdsSize size) => size == CdsSize.Lg ? 14 : CdsSpacing.Spacing05;
}
