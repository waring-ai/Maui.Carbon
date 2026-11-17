namespace Maui.Carbon;

/// <summary>Where a radio button's label sits relative to its circle.</summary>
public enum CdsRadioLabelPosition
{
    /// <summary>Label to the right of the circle (default).</summary>
    Right,
    /// <summary>Label to the left of the circle.</summary>
    Left,
}

/// <summary>Color keys of a radio button in one state.</summary>
/// <param name="Ring">1px ring.</param>
/// <param name="Dot">The selected dot.</param>
/// <param name="Label">Label color.</param>
public readonly record struct CdsRadioVisuals(string Ring, string Dot, string Label);

/// <summary>Maps a radio button state to Carbon tokens (from <c>packages/styles/scss/components/radio-button</c>).</summary>
public static class CdsRadioTokens
{
    /// <summary>
    /// Resolves the colors. Flags that matter: <see cref="CdsInteractionState.Disabled"/>,
    /// <see cref="CdsInteractionState.ReadOnly"/> and <see cref="CdsInteractionState.Invalid"/> (ignored while disabled or read-only).
    /// </summary>
    public static CdsRadioVisuals Resolve(CdsInteractionState state)
    {
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var readOnly = state.HasFlag(CdsInteractionState.ReadOnly);
        var invalid = state.HasFlag(CdsInteractionState.Invalid) && !disabled && !readOnly;
        if (disabled) return new(CdsColorKeys.IconDisabled, CdsColorKeys.TextDisabled, CdsColorKeys.TextDisabled);
        if (readOnly) return new(CdsColorKeys.IconDisabled, CdsColorKeys.IconPrimary, CdsColorKeys.TextPrimary);
        return new(invalid ? CdsColorKeys.SupportError : CdsColorKeys.IconPrimary, CdsColorKeys.IconPrimary, CdsColorKeys.TextPrimary);
    }
}
