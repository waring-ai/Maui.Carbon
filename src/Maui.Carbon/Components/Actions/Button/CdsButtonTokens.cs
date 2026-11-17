namespace Maui.Carbon;

/// <summary>Color keys of a button in one state. <c>null</c> means transparent.</summary>
/// <param name="Background">Fill.</param>
/// <param name="Border">1px border.</param>
/// <param name="Foreground">Label color.</param>
/// <param name="Icon">Icon color.</param>
public readonly record struct CdsButtonVisuals(string? Background, string? Border, string Foreground, string Icon);

/// <summary>Maps a button kind and state to Carbon tokens (from <c>packages/styles/scss/components/button</c>).</summary>
public static class CdsButtonTokens
{
    /// <summary>Resolves the colors for a kind in a state.</summary>
    public static CdsButtonVisuals Resolve(CdsButtonKind kind, CdsInteractionState state)
    {
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var pressed = state.HasFlag(CdsInteractionState.Pressed);
        var hover = state.HasFlag(CdsInteractionState.Hover);
        var focused = state.HasFlag(CdsInteractionState.FocusVisible);
        var selected = state.HasFlag(CdsInteractionState.Selected);

        if (disabled)
        {
            return kind switch
            {
                CdsButtonKind.Tertiary or CdsButtonKind.DangerTertiary =>
                    new(null, CdsColorKeys.ButtonDisabled, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled),
                CdsButtonKind.Ghost or CdsButtonKind.DangerGhost =>
                    new(null, null, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled),
                _ => new(CdsColorKeys.ButtonDisabled, CdsColorKeys.ButtonDisabled, CdsColorKeys.TextOnColorDisabled, CdsColorKeys.IconOnColorDisabled),
            };
        }

        switch (kind)
        {
            case CdsButtonKind.Primary:
                return Filled(pressed ? CdsColorKeys.ButtonPrimaryActive : hover ? CdsColorKeys.ButtonPrimaryHover : CdsColorKeys.ButtonPrimary);
            case CdsButtonKind.Secondary:
                return Filled(pressed ? CdsColorKeys.ButtonSecondaryActive : hover ? CdsColorKeys.ButtonSecondaryHover : CdsColorKeys.ButtonSecondary);
            case CdsButtonKind.Danger:
                return Filled(pressed ? CdsColorKeys.ButtonDangerActive : hover ? CdsColorKeys.ButtonDangerHover : CdsColorKeys.ButtonDangerPrimary);
            case CdsButtonKind.Tertiary:
                if (pressed) return new(CdsColorKeys.ButtonTertiaryActive, CdsColorKeys.ButtonTertiaryActive, CdsColorKeys.TextInverse, CdsColorKeys.IconInverse);
                if (hover) return new(CdsColorKeys.ButtonTertiaryHover, CdsColorKeys.ButtonTertiaryHover, CdsColorKeys.TextInverse, CdsColorKeys.IconInverse);
                if (focused) return new(CdsColorKeys.ButtonTertiary, CdsColorKeys.Focus, CdsColorKeys.TextInverse, CdsColorKeys.IconInverse);
                return new(null, CdsColorKeys.ButtonTertiary, CdsColorKeys.ButtonTertiary, CdsColorKeys.ButtonTertiary);
            case CdsButtonKind.Ghost:
                if (selected) return new(hover ? CdsColorKeys.BackgroundSelectedHover : CdsColorKeys.BackgroundSelected, null, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
                if (pressed) return new(CdsColorKeys.BackgroundActive, null, CdsColorKeys.LinkPrimaryHover, CdsColorKeys.IconPrimary);
                if (hover) return new(CdsColorKeys.BackgroundHover, null, CdsColorKeys.LinkPrimaryHover, CdsColorKeys.IconPrimary);
                return new(null, null, CdsColorKeys.LinkPrimary, CdsColorKeys.IconPrimary);
            case CdsButtonKind.DangerTertiary:
                if (pressed) return new(CdsColorKeys.ButtonDangerActive, CdsColorKeys.ButtonDangerActive, CdsColorKeys.TextOnColor, CdsColorKeys.IconOnColor);
                if (hover) return new(CdsColorKeys.ButtonDangerHover, CdsColorKeys.ButtonDangerHover, CdsColorKeys.TextOnColor, CdsColorKeys.IconOnColor);
                if (focused) return new(CdsColorKeys.ButtonDangerPrimary, CdsColorKeys.Focus, CdsColorKeys.TextOnColor, CdsColorKeys.IconOnColor);
                return new(null, CdsColorKeys.ButtonDangerSecondary, CdsColorKeys.ButtonDangerSecondary, CdsColorKeys.ButtonDangerSecondary);
            default: // DangerGhost
                if (pressed) return new(CdsColorKeys.ButtonDangerActive, null, CdsColorKeys.TextOnColor, CdsColorKeys.IconOnColor);
                if (hover) return new(CdsColorKeys.ButtonDangerHover, null, CdsColorKeys.TextOnColor, CdsColorKeys.IconOnColor);
                return new(null, null, CdsColorKeys.ButtonDangerSecondary, CdsColorKeys.ButtonDangerSecondary);
        }

        static CdsButtonVisuals Filled(string fill) => new(fill, fill, CdsColorKeys.TextOnColor, CdsColorKeys.IconOnColor);
    }

    /// <summary>The focus ring style for a kind: filled and outlined buttons get the triple ring, ghosts a plain inset ring.</summary>
    public static CdsFocusRingStyle FocusRing(CdsButtonKind kind) =>
        kind is CdsButtonKind.Ghost or CdsButtonKind.DangerGhost ? CdsFocusRingStyle.Inset : CdsFocusRingStyle.Button;
}
