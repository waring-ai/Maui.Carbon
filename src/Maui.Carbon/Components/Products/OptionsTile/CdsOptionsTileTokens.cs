namespace Maui.Carbon;

/// <summary>The validation state an options tile shows in its summary line.</summary>
public enum CdsOptionsTileStatus
{
    /// <summary>No state: the summary in <c>text-secondary</c>.</summary>
    None,
    /// <summary>Invalid: <c>WarningFilled</c> and the invalid text in <c>support-error</c>.</summary>
    Invalid,
    /// <summary>Warning: <c>WarningAltFilled</c> in <c>support-warning</c>, the warning text in <c>text-primary</c>.</summary>
    Warning,
    /// <summary>Locked: a lock glyph and the text in <c>text-secondary</c>; the toggle is disabled.</summary>
    Locked,
}

/// <summary>Color keys and glyph of an options tile's summary line. <c>Icon</c> is <c>null</c> without a validation state.</summary>
/// <param name="Text">The summary text color.</param>
/// <param name="Icon">The status glyph, if any.</param>
/// <param name="IconColor">The status glyph color.</param>
public readonly record struct CdsOptionsTileSummaryVisuals(string Text, CdsIconName? Icon, string IconColor);

/// <summary>Maps an options tile's state to tokens (from <c>ibm-products-styles/src/components/OptionsTile/_options-tile.scss</c>).</summary>
public static class CdsOptionsTileTokens
{
    /// <summary>
    /// The lock glyph. Carbon uses <c>locked</c>, which the bundled icon set lacks; <see cref="CdsIconName.EditOff"/> (not
    /// editable) is the closest.
    /// </summary>
    public const CdsIconName LockedIcon = CdsIconName.EditOff;

    /// <summary>The tile surface (<c>layer</c>) and its bottom rule (<c>border-subtle</c>).</summary>
    public const string Background = CdsColorKeys.Layer01;

    /// <summary>The 1px rule under the tile.</summary>
    public const string BottomRule = CdsColorKeys.BorderSubtle01;

    /// <summary>The header fill: <c>background-hover</c> while the pointer is over the header (not over the toggle).</summary>
    public static string? HeaderBackground(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.Hover) && !state.HasFlag(CdsInteractionState.Disabled) ? CdsColorKeys.BackgroundHover : null;

    /// <summary>The title color (<c>text-primary</c>; <c>text-disabled</c> when disabled).</summary>
    public static string Title(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.Disabled) ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;

    /// <summary>The chevron color (<c>icon-primary</c>; <c>icon-disabled</c> when disabled).</summary>
    public static string Chevron(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.Disabled) ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;

    /// <summary>The summary line in a status.</summary>
    public static CdsOptionsTileSummaryVisuals Summary(CdsOptionsTileStatus status, CdsInteractionState state)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(CdsColorKeys.TextDisabled, Glyph(status), CdsColorKeys.IconDisabled);
        return status switch
        {
            CdsOptionsTileStatus.Invalid => new(CdsColorKeys.SupportError, CdsIconName.WarningFilled, CdsColorKeys.SupportError),
            CdsOptionsTileStatus.Warning => new(CdsColorKeys.TextPrimary, CdsIconName.WarningAltFilled, CdsColorKeys.SupportWarning),
            CdsOptionsTileStatus.Locked => new(CdsColorKeys.TextSecondary, LockedIcon, CdsColorKeys.IconSecondary),
            _ => new(CdsColorKeys.TextSecondary, null, CdsColorKeys.IconSecondary),
        };
    }

    /// <summary>The glyph of a status, or <c>null</c> for none.</summary>
    public static CdsIconName? Glyph(CdsOptionsTileStatus status) => status switch
    {
        CdsOptionsTileStatus.Invalid => CdsIconName.WarningFilled,
        CdsOptionsTileStatus.Warning => CdsIconName.WarningAltFilled,
        CdsOptionsTileStatus.Locked => LockedIcon,
        _ => null,
    };
}
