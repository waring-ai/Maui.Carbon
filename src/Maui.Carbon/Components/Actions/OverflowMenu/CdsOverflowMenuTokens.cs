namespace Maui.Carbon;

/// <summary>Which way an overflow menu opens.</summary>
public enum CdsOverflowMenuDirection
{
    /// <summary>Below the trigger (default).</summary>
    Bottom,
    /// <summary>Above the trigger.</summary>
    Top,
}

/// <summary>Maps an overflow menu trigger's state to Carbon tokens (from <c>packages/styles/scss/components/overflow-menu</c>).</summary>
public static class CdsOverflowMenuTokens
{
    /// <summary>
    /// Resolves the trigger colors: transparent with an <c>icon-primary</c> glyph; <c>background-hover</c> on hover;
    /// while open <c>layer-01</c> (so it reads as part of the menu); <c>icon-disabled</c> when disabled.
    /// </summary>
    public static CdsButtonVisuals Resolve(CdsInteractionState state)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(null, null, CdsColorKeys.IconDisabled, CdsColorKeys.IconDisabled);
        if (state.HasFlag(CdsInteractionState.Open))
            return new(CdsColorKeys.Layer01, null, CdsColorKeys.IconPrimary, CdsColorKeys.IconPrimary);
        if (state.HasFlag(CdsInteractionState.Hover))
            return new(CdsColorKeys.BackgroundHover, null, CdsColorKeys.IconPrimary, CdsColorKeys.IconPrimary);
        return new(null, null, CdsColorKeys.IconPrimary, CdsColorKeys.IconPrimary);
    }

    /// <summary>The square side of the trigger: xs 24, sm 32, md 40 (default), lg 48. Larger sizes clamp to lg.</summary>
    public static double Side(CdsSize size) => (size > CdsSize.Lg ? CdsSize.Lg : size).Height();

    /// <summary>The menu placement: start-aligned, or end-aligned when <paramref name="flipped"/>; below or above per <paramref name="direction"/>.</summary>
    public static CdsPlacement Placement(CdsOverflowMenuDirection direction, bool flipped) => (direction, flipped) switch
    {
        (CdsOverflowMenuDirection.Top, true) => CdsPlacement.TopEnd,
        (CdsOverflowMenuDirection.Top, false) => CdsPlacement.TopStart,
        (_, true) => CdsPlacement.BottomEnd,
        _ => CdsPlacement.BottomStart,
    };
}
