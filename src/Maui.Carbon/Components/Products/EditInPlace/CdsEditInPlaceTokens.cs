namespace Maui.Carbon;

/// <summary>Maps the states of a <see cref="CdsEditInPlace"/> to Carbon tokens. <c>null</c> means transparent or none.</summary>
public static class CdsEditInPlaceTokens
{
    /// <summary>The fill: transparent at rest, <c>field-01</c> on hover and while editing; transparent while disabled.</summary>
    public static string? Background(CdsInteractionState state, bool editing)
    {
        if (state.HasFlag(CdsInteractionState.Disabled)) return null;
        return editing || state.HasFlag(CdsInteractionState.Hover) ? CdsColorKeys.Field01 : null;
    }

    /// <summary>
    /// The 2px outline: <c>focus</c> while the input has focus, else <c>support-error</c> when invalid and not editing
    /// (Carbon drops the error border while editing, so the two never double up).
    /// </summary>
    public static string? Outline(bool inputFocused, bool editing, bool invalid) =>
        inputFocused ? CdsColorKeys.Focus : invalid && !editing ? CdsColorKeys.SupportError : null;

    /// <summary>The value: <c>text-primary</c>, <c>text-disabled</c> while disabled.</summary>
    public static string Text(bool disabled) => disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;

    /// <summary>The placeholder: <c>text-placeholder</c>, <c>text-disabled</c> while disabled.</summary>
    public static string Placeholder(bool disabled) => disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder;

    /// <summary>The error icon in the toolbar and the error message under the field: <c>support-error</c>.</summary>
    public static string Invalid => CdsColorKeys.SupportError;
}
