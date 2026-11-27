namespace Maui.Carbon;

/// <summary>The look of a <see cref="CdsTabs"/> row.</summary>
public enum CdsTabsType
{
    /// <summary>Line tabs: 40 high on a 2px rule with a 2px <c>border-interactive</c> underline under the selected tab (default).</summary>
    Line,
    /// <summary>Contained tabs: 48 high <c>layer-accent-01</c> blocks, the selected one in <c>layer-01</c> with a 2px top edge.</summary>
    Contained,
}

/// <summary>Where the tabs of a <see cref="CdsTabs"/> sit.</summary>
public enum CdsTabsOrientation
{
    /// <summary>A row of tabs above the panel (default).</summary>
    Horizontal,
    /// <summary>
    /// Carbon's vertical tabs: a column of 64 high tabs at the start, a quarter of the width, with the panel beside it. Use them for
    /// many sections or long labels, on medium and larger screens.
    /// </summary>
    Vertical,
}

/// <summary>How arrow keys behave in <see cref="CdsTabs"/> and <see cref="CdsContentSwitcher"/>.</summary>
public enum CdsActivation
{
    /// <summary>Arrow keys move focus and select (default).</summary>
    Automatic,
    /// <summary>Arrow keys only move focus; Enter or Space select (use when a panel is slow to load).</summary>
    Manual,
}

/// <summary>Colors of a tab in one state. <c>null</c> means transparent / none.</summary>
/// <param name="Background">Fill.</param>
/// <param name="Foreground">Label color.</param>
/// <param name="SecondaryLabel">Secondary label color.</param>
/// <param name="Icon">Icon color.</param>
/// <param name="BottomRule">2px rule under a line tab.</param>
/// <param name="TopRule">2px edge on top of the selected contained tab.</param>
/// <param name="CloseIcon">Color of the close icon of a dismissible tab.</param>
/// <param name="IsSemiBold">The selected label is semibold.</param>
public readonly record struct CdsTabVisuals(
    string? Background, string Foreground, string SecondaryLabel, string Icon, string? BottomRule, string? TopRule, string CloseIcon, bool IsSemiBold);

/// <summary>Colors of a vertical tab in one state. <c>null</c> means none.</summary>
/// <param name="Background">Fill.</param>
/// <param name="Foreground">Label and icon color.</param>
/// <param name="StartEdge">The 3px edge at the start of the tab (<c>border-interactive</c> when selected).</param>
/// <param name="EndRule">The 1px rule at the end of the tab, between it and the panel; none on the selected tab.</param>
/// <param name="BottomRule">The 1px rule under the tab.</param>
/// <param name="IsSemiBold">The selected label is semibold.</param>
public readonly record struct CdsVerticalTabVisuals(
    string Background, string Foreground, string StartEdge, string? EndRule, string BottomRule, bool IsSemiBold);

/// <summary>Maps tab type and state to Carbon tokens (from <c>packages/styles/scss/components/tabs</c>).</summary>
public static class CdsTabsTokens
{
    /// <summary>Resolves a tab. Labeled tabs draw their icon in the text color; icon-only tabs use the icon tokens.</summary>
    public static CdsTabVisuals Resolve(CdsTabsType type, CdsInteractionState state, bool selected, bool iconOnly = false)
    {
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var hover = state.HasFlag(CdsInteractionState.Hover);
        var pressed = state.HasFlag(CdsInteractionState.Pressed);

        if (type == CdsTabsType.Contained)
        {
            if (disabled)
                return new(CdsColorKeys.ButtonDisabled, CdsColorKeys.TextOnColorDisabled, CdsColorKeys.TextOnColorDisabled,
                    iconOnly ? CdsColorKeys.IconDisabled : CdsColorKeys.TextOnColorDisabled, null, null, CdsColorKeys.IconDisabled, false);
            if (selected)
                return new(CdsColorKeys.Layer01, CdsColorKeys.TextPrimary, CdsColorKeys.TextSecondary,
                    iconOnly ? CdsColorKeys.IconPrimary : CdsColorKeys.TextPrimary, null, CdsColorKeys.BorderInteractive, CdsColorKeys.IconPrimary, true);
            if (pressed)
                return new(CdsColorKeys.LayerAccentActive01, CdsColorKeys.TextPrimary, CdsColorKeys.TextSecondary,
                    iconOnly ? CdsColorKeys.IconSecondary : CdsColorKeys.TextPrimary, null, null, CdsColorKeys.IconSecondary, false);
            if (hover)
                return new(CdsColorKeys.LayerAccentHover01, CdsColorKeys.TextPrimary, CdsColorKeys.TextSecondary,
                    iconOnly ? CdsColorKeys.IconSecondary : CdsColorKeys.TextPrimary, null, null, CdsColorKeys.IconSecondary, false);
            return new(CdsColorKeys.LayerAccent01, CdsColorKeys.TextSecondary, CdsColorKeys.TextSecondary,
                iconOnly ? CdsColorKeys.IconSecondary : CdsColorKeys.TextSecondary, null, null, CdsColorKeys.IconSecondary, false);
        }

        if (disabled)
            return new(null, CdsColorKeys.TextDisabled, CdsColorKeys.TextDisabled,
                iconOnly ? CdsColorKeys.IconDisabled : CdsColorKeys.TextDisabled, CdsColorKeys.BorderDisabled, null, CdsColorKeys.IconDisabled, false);
        if (selected)
            return new(null, CdsColorKeys.TextPrimary, CdsColorKeys.TextSecondary,
                iconOnly ? CdsColorKeys.IconPrimary : CdsColorKeys.TextPrimary, CdsColorKeys.BorderInteractive, null, CdsColorKeys.IconPrimary, true);
        if (hover || pressed)
            return new(null, CdsColorKeys.TextPrimary, CdsColorKeys.TextSecondary,
                iconOnly ? CdsColorKeys.IconSecondary : CdsColorKeys.TextPrimary, CdsColorKeys.BorderStrong01, null, CdsColorKeys.IconSecondary, false);
        return new(null, CdsColorKeys.TextSecondary, CdsColorKeys.TextSecondary,
            iconOnly ? CdsColorKeys.IconSecondary : CdsColorKeys.TextSecondary, CdsColorKeys.BorderSubtle00, null, CdsColorKeys.IconSecondary, false);
    }

    /// <summary>The tab height: line 40, contained 48, 64 for a contained tab with a secondary label, 48 for large icon-only tabs.</summary>
    public static double Height(CdsTabsType type, bool iconOnly, CdsSize iconSize, bool hasSecondaryLabel)
    {
        if (iconOnly) return iconSize == CdsSize.Lg ? CdsSizing.Large : CdsSizing.Medium;
        if (type == CdsTabsType.Contained) return hasSecondaryLabel ? CdsSizing.XLarge : CdsSizing.Large;
        return CdsSizing.Medium;
    }

    /// <summary>
    /// Resolves a vertical tab (<c>.cds--tabs--vertical</c>): <c>layer-01</c> with 1px <c>border-subtle-01</c> rules under and after
    /// it and a 3px <c>border-subtle-01</c> start edge; on hover <c>layer-hover-01</c> with a <c>border-strong-01</c> edge; selected, a
    /// <c>border-interactive</c> edge, no end rule and a semibold <c>text-primary</c> label; disabled, <c>text-disabled</c>.
    /// </summary>
    public static CdsVerticalTabVisuals ResolveVertical(CdsInteractionState state, bool selected)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(CdsColorKeys.Layer01, CdsColorKeys.TextDisabled, CdsColorKeys.BorderSubtle01, CdsColorKeys.BorderSubtle01, CdsColorKeys.BorderSubtle01, false);
        if (selected)
            return new(CdsColorKeys.Layer01, CdsColorKeys.TextPrimary, CdsColorKeys.BorderInteractive, null, CdsColorKeys.BorderSubtle01, true);
        if (state.HasFlag(CdsInteractionState.Hover) || state.HasFlag(CdsInteractionState.Pressed))
            return new(CdsColorKeys.LayerHover01, CdsColorKeys.TextPrimary, CdsColorKeys.BorderStrong01, CdsColorKeys.BorderSubtle01, CdsColorKeys.BorderSubtle01, false);
        return new(CdsColorKeys.Layer01, CdsColorKeys.TextSecondary, CdsColorKeys.BorderSubtle01, CdsColorKeys.BorderSubtle01, CdsColorKeys.BorderSubtle01, false);
    }

    /// <summary>Height of a vertical tab: 4rem (Carbon's default xl layout size). Its label wraps to two lines.</summary>
    public const double VerticalHeight = 64;

    /// <summary>Width of the start edge of a vertical tab: 3px.</summary>
    public const double VerticalEdgeWidth = 3;

    /// <summary>The share of the width the vertical tab column takes: a quarter (4 of 16 grid columns, 2 of 8 at md).</summary>
    public const double VerticalListFraction = 0.25;

    /// <summary>The smallest width of a tab with a label (80).</summary>
    public const double MinWidth = 80;

    /// <summary>The widest a tab with a label gets before the label truncates (240).</summary>
    public const double MaxWidth = 240;
}
