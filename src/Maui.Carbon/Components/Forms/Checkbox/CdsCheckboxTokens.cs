namespace Maui.Carbon;

/// <summary>Color keys of a checkbox in one state. <c>null</c> means transparent.</summary>
/// <param name="Border">1px box border.</param>
/// <param name="Fill">Box fill (checked and indeterminate).</param>
/// <param name="Mark">Check or dash color.</param>
/// <param name="Label">Label color.</param>
public readonly record struct CdsCheckboxVisuals(string Border, string? Fill, string Mark, string Label);

/// <summary>Maps a checkbox state to Carbon tokens (from <c>packages/styles/scss/components/checkbox</c>).</summary>
public static class CdsCheckboxTokens
{
    /// <summary>
    /// Resolves the colors. <paramref name="isChecked"/>: <c>true</c> checked, <c>false</c> unchecked, <c>null</c>
    /// indeterminate. Flags that matter: <see cref="CdsInteractionState.Disabled"/>, <see cref="CdsInteractionState.ReadOnly"/>
    /// and <see cref="CdsInteractionState.Invalid"/> (ignored while disabled or read-only).
    /// </summary>
    public static CdsCheckboxVisuals Resolve(CdsInteractionState state, bool? isChecked)
    {
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var readOnly = state.HasFlag(CdsInteractionState.ReadOnly);
        var invalid = state.HasFlag(CdsInteractionState.Invalid) && !disabled && !readOnly;
        var filled = isChecked != false;

        if (disabled)
            return new(CdsColorKeys.IconDisabled, filled ? CdsColorKeys.IconDisabled : null, CdsColorKeys.IconInverse, CdsColorKeys.TextDisabled);
        if (readOnly)
            return new(CdsColorKeys.IconDisabled, null, CdsColorKeys.IconPrimary, CdsColorKeys.TextPrimary);
        return new(invalid ? CdsColorKeys.SupportError : CdsColorKeys.IconPrimary,
            filled ? CdsColorKeys.IconPrimary : null, CdsColorKeys.IconInverse, CdsColorKeys.TextPrimary);
    }
}
