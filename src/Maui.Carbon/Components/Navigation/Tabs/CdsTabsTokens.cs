namespace Maui.Carbon;

/// <summary>The look of a <see cref="CdsTabs"/> row.</summary>
public enum CdsTabsType
{
    /// <summary>Line tabs: 40 high on a 2px rule with a 2px <c>border-interactive</c> underline under the selected tab (default).</summary>
    Line,
    /// <summary>Contained tabs: 48 high <c>layer-accent-01</c> blocks, the selected one in <c>layer-01</c> with a 2px top edge.</summary>
    Contained,
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

    /// <summary>The smallest width of a tab with a label (80).</summary>
    public const double MinWidth = 80;

    /// <summary>The widest a tab with a label gets before the label truncates (240).</summary>
    public const double MaxWidth = 240;
}
