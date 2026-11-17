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

/// <summary>Maps a tile kind and state to Carbon tokens (from <c>packages/styles/scss/components/tile</c>).</summary>
public static class CdsTileTokens
{
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
