namespace Maui.Carbon;

/// <summary>Color keys of a shell element in one state. <c>null</c> means transparent.</summary>
/// <param name="Background">Fill.</param>
/// <param name="Foreground">Text color.</param>
/// <param name="Icon">Icon color.</param>
/// <param name="Bar">The 3px current-item bar (<c>border-interactive</c>), or <c>null</c>.</param>
/// <param name="SideBorder">1px side borders of an active header action (<c>border-subtle-00</c>), or <c>null</c>.</param>
public readonly record struct CdsShellVisuals(string? Background, string Foreground, string Icon, string? Bar = null, string? SideBorder = null);

/// <summary>
/// Maps shell elements and states to Carbon tokens (from <c>packages/styles/scss/components/ui-shell</c>). The shell follows
/// the theme: there are no inverse shell tokens in v11.99.
/// </summary>
public static class CdsUIShellTokens
{
    /// <summary>Surface of the header and side nav: <c>background</c>.</summary>
    public const string Surface = CdsColorKeys.Background;

    /// <summary>Header bottom border and dividers: <c>border-subtle-00</c>.</summary>
    public const string Divider = CdsColorKeys.BorderSubtle00;

    /// <summary>A header action or the menu button: icon <c>icon-secondary</c> to <c>icon-primary</c>; active fills <c>layer-01</c> with side borders.</summary>
    public static CdsShellVisuals HeaderAction(CdsInteractionState state, bool isActive)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(null, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled);
        var pressed = state.HasFlag(CdsInteractionState.Pressed);
        var hover = state.HasFlag(CdsInteractionState.Hover);
        if (isActive)
        {
            var fill = pressed ? CdsColorKeys.BackgroundActive : CdsColorKeys.Layer01;
            return new(fill, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary, null, CdsColorKeys.BorderSubtle00);
        }
        if (pressed) return new(CdsColorKeys.BackgroundActive, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
        if (hover) return new(CdsColorKeys.BackgroundHover, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
        return new(null, CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary);
    }

    /// <summary>The 1px border of a header action: <c>focus</c> while focused, otherwise transparent (<c>null</c>).</summary>
    public static string? HeaderActionBorder(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.FocusVisible) ? CdsColorKeys.Focus : null;

    /// <summary>
    /// A link in the header bar: <c>text-secondary</c> on <c>background</c>; hover <c>background-hover</c>, pressed
    /// <c>background-active</c> (both <c>text-primary</c>); the current link is <c>text-primary</c> with a 3px <c>border-interactive</c> underline.
    /// </summary>
    public static CdsShellVisuals HeaderMenuItem(CdsInteractionState state, bool isCurrent)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(CdsColorKeys.Background, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled);
        var bar = isCurrent ? CdsColorKeys.BorderInteractive : null;
        if (state.HasFlag(CdsInteractionState.Pressed)) return new(CdsColorKeys.BackgroundActive, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary, bar);
        if (state.HasFlag(CdsInteractionState.Hover)) return new(CdsColorKeys.BackgroundHover, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary, bar);
        return new(CdsColorKeys.Background, isCurrent ? CdsColorKeys.TextPrimary : CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary, bar);
    }

    /// <summary>
    /// A row in an open header menu (<c>layer-01</c> list): hover <c>layer-hover-01</c>, pressed <c>layer-active-01</c>, current
    /// <c>layer-selected-01</c> (<c>layer-selected-hover-01</c> on hover) with a 3px <c>border-interactive</c> bar on the start edge.
    /// </summary>
    public static CdsShellVisuals HeaderMenuRow(CdsInteractionState state, bool isCurrent, bool isHighlighted)
    {
        var hover = state.HasFlag(CdsInteractionState.Hover) || isHighlighted;
        var bar = isCurrent ? CdsColorKeys.BorderInteractive : null;
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(CdsColorKeys.Layer01, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled);
        if (state.HasFlag(CdsInteractionState.Pressed)) return new(CdsColorKeys.LayerActive01, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary, bar);
        if (isCurrent) return new(hover ? CdsColorKeys.LayerSelectedHover01 : CdsColorKeys.LayerSelected01, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary, bar);
        if (hover) return new(CdsColorKeys.LayerHover01, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
        return new(CdsColorKeys.Layer01, CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary);
    }

    /// <summary>
    /// The trigger of a header menu: like a header link, but while open it fills with <c>layer-01</c> (flowing into the list);
    /// closed, it shows the current underline when a child is current.
    /// </summary>
    public static CdsShellVisuals HeaderMenuTrigger(CdsInteractionState state, bool isOpen, bool hasCurrentChild)
    {
        if (isOpen) return new(CdsColorKeys.Layer01, CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary);
        return HeaderMenuItem(state, hasCurrentChild);
    }

    /// <summary>
    /// A side nav row: <c>text-secondary</c>, hover <c>background-hover</c> + <c>text-primary</c>; active (current page, or a
    /// collapsed group holding it) <c>background-selected</c> with a 3px <c>border-interactive</c> start bar and <c>text-primary</c>.
    /// </summary>
    public static CdsShellVisuals SideNavRow(CdsInteractionState state, bool isActive)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(null, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled);
        var hover = state.HasFlag(CdsInteractionState.Hover);
        var pressed = state.HasFlag(CdsInteractionState.Pressed);
        if (isActive)
            return new(pressed ? CdsColorKeys.BackgroundActive : hover ? CdsColorKeys.BackgroundSelectedHover : CdsColorKeys.BackgroundSelected,
                CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary, CdsColorKeys.BorderInteractive);
        if (pressed) return new(CdsColorKeys.BackgroundActive, CdsColorKeys.TextPrimary, CdsColorKeys.IconSecondary);
        if (hover) return new(CdsColorKeys.BackgroundHover, CdsColorKeys.TextPrimary, CdsColorKeys.IconSecondary);
        return new(null, CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary);
    }

    /// <summary>The side nav footer toggle: <c>background-hover</c> on hover, icon <c>icon-secondary</c> going to <c>icon-primary</c>.</summary>
    public static CdsShellVisuals SideNavToggle(CdsInteractionState state)
    {
        if (state.HasFlag(CdsInteractionState.Pressed)) return new(CdsColorKeys.BackgroundActive, CdsColorKeys.TextSecondary, CdsColorKeys.IconPrimary);
        if (state.HasFlag(CdsInteractionState.Hover)) return new(CdsColorKeys.BackgroundHover, CdsColorKeys.TextSecondary, CdsColorKeys.IconPrimary);
        return new(null, CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary);
    }

    /// <summary>
    /// A switcher row: <c>text-secondary</c>; hover <c>layer-hover-01</c> + <c>text-primary</c>, pressed <c>layer-active-01</c>,
    /// selected <c>layer-selected-01</c> + <c>text-primary</c>.
    /// </summary>
    public static CdsShellVisuals SwitcherItem(CdsInteractionState state, bool isSelected)
    {
        if (state.HasFlag(CdsInteractionState.Disabled)) return new(null, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled);
        if (state.HasFlag(CdsInteractionState.Pressed)) return new(CdsColorKeys.LayerActive01, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
        if (isSelected) return new(CdsColorKeys.LayerSelected01, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
        if (state.HasFlag(CdsInteractionState.Hover)) return new(CdsColorKeys.LayerHover01, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
        return new(null, CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary);
    }

    /// <summary>The skip link in its focused state: <c>background</c> fill, <c>text-secondary</c>, a 4px <c>focus</c> border.</summary>
    public static CdsShellVisuals SkipToContent() => new(CdsColorKeys.Background, CdsColorKeys.TextSecondary, CdsColorKeys.IconPrimary);
}
