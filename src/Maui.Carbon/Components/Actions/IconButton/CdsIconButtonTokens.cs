namespace Maui.Carbon;

/// <summary>The emphasis of a <see cref="CdsIconButton"/>. Danger kinds do not exist: destructive actions need a visible label.</summary>
public enum CdsIconButtonKind
{
    /// <summary><c>button-primary</c> fill.</summary>
    Primary,
    /// <summary><c>button-secondary</c> fill.</summary>
    Secondary,
    /// <summary>1px <c>button-tertiary</c> outline.</summary>
    Tertiary,
    /// <summary>Transparent with an <c>icon-primary</c> glyph: the usual choice in toolbars.</summary>
    Ghost,
}

/// <summary>Maps an icon button kind and state to Carbon tokens. It reuses the <see cref="CdsButtonTokens"/> of the matching button.</summary>
public static class CdsIconButtonTokens
{
    /// <summary>The button kind an icon button kind renders as.</summary>
    public static CdsButtonKind ToButtonKind(CdsIconButtonKind kind) => kind switch
    {
        CdsIconButtonKind.Secondary => CdsButtonKind.Secondary,
        CdsIconButtonKind.Tertiary => CdsButtonKind.Tertiary,
        CdsIconButtonKind.Ghost => CdsButtonKind.Ghost,
        _ => CdsButtonKind.Primary,
    };

    /// <summary>
    /// Resolves the colors of an icon button. When <see cref="CdsInteractionState.Selected"/> is set (and the button is
    /// enabled) the fill is <c>background-selected</c> (<c>background-selected-hover</c> on hover or press) and the glyph
    /// <c>icon-primary</c>, whatever the kind.
    /// </summary>
    public static CdsButtonVisuals Resolve(CdsIconButtonKind kind, CdsInteractionState state)
    {
        if (state.HasFlag(CdsInteractionState.Selected) && !state.HasFlag(CdsInteractionState.Disabled))
        {
            var raised = state.HasFlag(CdsInteractionState.Hover) || state.HasFlag(CdsInteractionState.Pressed);
            return new(raised ? CdsColorKeys.BackgroundSelectedHover : CdsColorKeys.BackgroundSelected, null,
                CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
        }
        return CdsButtonTokens.Resolve(ToButtonKind(kind), state);
    }

    /// <summary>The focus ring of a kind: the triple ring for filled and outlined buttons, an inset ring for ghosts and selected buttons.</summary>
    public static CdsFocusRingStyle FocusRing(CdsIconButtonKind kind) => CdsButtonTokens.FocusRing(ToButtonKind(kind));

    /// <summary>The square size of an icon button: xs 24, sm 32, md 40, lg 48. Larger sizes clamp to lg.</summary>
    public static double Side(CdsSize size) => (size > CdsSize.Lg ? CdsSize.Lg : size).Height();

    /// <summary>
    /// The text of the badge for a count: <c>null</c> when there is no badge, an empty string for a dot (count 0),
    /// otherwise the number. Negative counts mean no badge.
    /// </summary>
    public static string? BadgeText(int? count) => count switch
    {
        null => null,
        < 0 => null,
        0 => string.Empty,
        var n => n.ToString(),
    };
}
