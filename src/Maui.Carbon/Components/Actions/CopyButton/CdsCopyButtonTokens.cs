namespace Maui.Carbon;

/// <summary>Maps a copy button's state to Carbon tokens (from <c>packages/styles/scss/components/copy-button</c>).</summary>
public static class CdsCopyButtonTokens
{
    /// <summary>
    /// Resolves the colors: <c>layer-01</c> fill (<c>layer-hover-01</c> on hover, <c>layer-active-01</c> pressed), an
    /// <c>icon-primary</c> glyph (<c>icon-disabled</c> when disabled) and no border.
    /// </summary>
    public static CdsButtonVisuals Resolve(CdsInteractionState state)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(CdsColorKeys.Layer01, null, CdsColorKeys.IconDisabled, CdsColorKeys.IconDisabled);
        var fill = state.HasFlag(CdsInteractionState.Pressed) ? CdsColorKeys.LayerActive01
            : state.HasFlag(CdsInteractionState.Hover) ? CdsColorKeys.LayerHover01
            : CdsColorKeys.Layer01;
        return new(fill, null, CdsColorKeys.IconPrimary, CdsColorKeys.IconPrimary);
    }

    /// <summary>The square side: sm 32, md 40 (default), lg 48. Xs clamps to sm and larger sizes to lg.</summary>
    public static double Side(CdsSize size) => Coerce(size).Height();

    /// <summary>Clamps a size into the copy button range sm..lg.</summary>
    public static CdsSize Coerce(CdsSize size) => size < CdsSize.Sm ? CdsSize.Sm : size > CdsSize.Lg ? CdsSize.Lg : size;
}
