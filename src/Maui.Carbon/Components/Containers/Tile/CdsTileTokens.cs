namespace Maui.Carbon;

/// <summary>The tile variants, which differ in which states and tokens they use.</summary>
public enum CdsTileKind
{
    /// <summary>A plain, static tile.</summary>
    Static,
    /// <summary>A tile that is one link or button.</summary>
    Clickable,
    /// <summary>A multi-select tile with a checkbox glyph.</summary>
    Selectable,
    /// <summary>A single-select tile with a check mark when chosen.</summary>
    Radio,
    /// <summary>An expandable tile whose whole surface toggles it.</summary>
    Expandable,
    /// <summary>An expandable tile with interactive content: only the chevron toggles it.</summary>
    ExpandableInteractive,
}

/// <summary>Color keys of a tile in one state. <c>null</c> means transparent.</summary>
/// <param name="Background">Tile fill.</param>
/// <param name="Border">1px border (selected selectable and radio tiles).</param>
/// <param name="Icon">Glyph color: trailing arrow, check glyph or chevron.</param>
/// <param name="ChevronBackground">Fill behind the chevron button (interactive expandable tiles, on hover).</param>
public readonly record struct CdsTileVisuals(string Background, string? Border, string Icon, string? ChevronBackground);

/// <summary>
/// Color keys of an AI tile (a tile whose decorator is an <see cref="CdsAILabel"/>) in one state: the aura that rises from the bottom
/// edge, the surface it fades into, and the border.
/// </summary>
/// <param name="AuraKey">The translucent aura at the bottom edge: <c>ai-aura-start</c>, or <c>ai-aura-hover-start</c> while the fill is the hover fill.</param>
/// <param name="SurfaceKey">The surface the aura fades into: <c>layer-01</c>, or <c>ai-aura-hover-background</c> for the hover fill.</param>
/// <param name="Border">A solid border key (<c>border-inverse</c>, selected tiles), or <c>null</c> for the 1px gradient border from <c>ai-border-start</c> to <c>ai-border-end</c>.</param>
public readonly record struct CdsTileAiVisuals(string AuraKey, string SurfaceKey, string? Border);

/// <summary>Maps a tile kind and state to Carbon tokens (from <c>packages/styles/scss/components/tile</c>).</summary>
public static class CdsTileTokens
{
    /// <summary>
    /// The text and icon tokens a disabled tile re-maps for its content, and what they become: every text color is
    /// <c>text-disabled</c> and every icon <c>icon-disabled</c>.
    /// </summary>
    public static IReadOnlyList<(string Key, string DisabledKey)> DisabledContentKeys { get; } =
    [
        (CdsColorKeys.TextPrimary, CdsColorKeys.TextDisabled),
        (CdsColorKeys.TextSecondary, CdsColorKeys.TextDisabled),
        (CdsColorKeys.TextHelper, CdsColorKeys.TextDisabled),
        (CdsColorKeys.TextPlaceholder, CdsColorKeys.TextDisabled),
        (CdsColorKeys.LinkPrimary, CdsColorKeys.TextDisabled),
        (CdsColorKeys.IconPrimary, CdsColorKeys.IconDisabled),
        (CdsColorKeys.IconSecondary, CdsColorKeys.IconDisabled),
        (CdsColorKeys.IconInteractive, CdsColorKeys.IconDisabled),
    ];

    /// <summary>
    /// Resolves an AI tile's fill and border (Carbon's <c>_tile.scss</c> AI selectors). Every AI tile has the default aura over
    /// <c>layer-01</c> and the gradient border. Clickable, selectable and radio tiles swap the fill for the hover fill
    /// (<c>ai-aura-hover-start</c> over <c>ai-aura-hover-background</c>) on hover and press, selected selectable and radio tiles
    /// get a <c>border-inverse</c> border (kept while hovered), and expandable and static tiles keep the default fill in every
    /// state. Carbon does not restyle a disabled AI tile (its AI selectors outrank the disabled ones), so disabled keeps the
    /// default fill and the selected border; it has no hover.
    /// </summary>
    public static CdsTileAiVisuals ResolveAi(CdsTileKind kind, CdsInteractionState state)
    {
        var selectable = kind is CdsTileKind.Selectable or CdsTileKind.Radio;
        var border = selectable && state.HasFlag(CdsInteractionState.Selected) ? CdsColorKeys.BorderInverse : null;
        var active = state.HasFlag(CdsInteractionState.Hover) || state.HasFlag(CdsInteractionState.Pressed);
        var hoverFill = active && !state.HasFlag(CdsInteractionState.Disabled)
            && kind is CdsTileKind.Clickable or CdsTileKind.Selectable or CdsTileKind.Radio;
        return hoverFill
            ? new(CdsAiTokens.AuraHoverStart, CdsAiTokens.AuraHoverBackground, border)
            : new(CdsAiTokens.Aura, CdsColorKeys.Layer01, border);
    }

    /// <summary>
    /// The end padding of a selectable or radio tile's body: 48 for the glyph, and with a decorator 72 (4.5rem, the design snapshot's
    /// rule) so text stays clear of the AI label and the glyph beside it.
    /// </summary>
    internal static double SelectableEndPadding(bool hasDecorator) =>
        hasDecorator ? CdsSpacing.Spacing09 + CdsSpacing.Spacing06 : CdsSpacing.Spacing09;

    /// <summary>
    /// Resolves the colors. Flags that matter: <see cref="CdsInteractionState.Disabled"/>, <see cref="CdsInteractionState.Hover"/>,
    /// <see cref="CdsInteractionState.Pressed"/> and <see cref="CdsInteractionState.Selected"/> (selectable and radio tiles).
    /// Only <c>-01</c> tokens are used: a <see cref="CdsLayer"/> steps them up.
    /// </summary>
    public static CdsTileVisuals Resolve(CdsTileKind kind, CdsInteractionState state)
    {
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var pressed = state.HasFlag(CdsInteractionState.Pressed);
        var hover = state.HasFlag(CdsInteractionState.Hover);
        var selected = state.HasFlag(CdsInteractionState.Selected);
        var selectable = kind is CdsTileKind.Selectable or CdsTileKind.Radio;

        if (disabled)
            return new(CdsColorKeys.Layer01, selectable && selected ? CdsColorKeys.BorderDisabled : null, CdsColorKeys.IconDisabled, null);

        switch (kind)
        {
            case CdsTileKind.Clickable:
                return new(pressed ? CdsColorKeys.LayerActive01 : hover ? CdsColorKeys.LayerHover01 : CdsColorKeys.Layer01,
                    null, CdsColorKeys.IconInteractive, null);
            case CdsTileKind.Selectable:
                return new(hover || pressed ? CdsColorKeys.LayerHover01 : CdsColorKeys.Layer01,
                    selected ? CdsColorKeys.LayerSelectedInverse : null,
                    selected ? CdsColorKeys.IconPrimary : CdsColorKeys.IconSecondary, null);
            case CdsTileKind.Radio:
                return new(hover || pressed ? CdsColorKeys.LayerHover01 : CdsColorKeys.Layer01,
                    selected ? CdsColorKeys.LayerSelectedInverse : null, CdsColorKeys.IconPrimary, null);
            case CdsTileKind.Expandable:
                return new(hover || pressed ? CdsColorKeys.LayerHover01 : CdsColorKeys.Layer01, null, CdsColorKeys.IconPrimary, null);
            case CdsTileKind.ExpandableInteractive:
                // No hover on the tile itself; the chevron button hovers on its own.
                return new(CdsColorKeys.Layer01, null, CdsColorKeys.IconPrimary, hover ? CdsColorKeys.LayerHover01 : null);
            default:
                return new(CdsColorKeys.Layer01, null, CdsColorKeys.IconPrimary, null);
        }
    }
}
