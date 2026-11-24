namespace Maui.Carbon;

/// <summary>How a list-box field (<see cref="CdsDropdown"/>, <see cref="CdsMultiSelect"/>) is drawn.</summary>
public enum CdsListBoxType
{
    /// <summary>A filled field with the label above (forms, modals, side panels).</summary>
    Default,

    /// <summary>No fill and no underline, the label to the start of the field (toolbars and filter bars).</summary>
    Inline,
}

/// <summary>Which way the open list grows from the field.</summary>
public enum CdsListBoxDirection
{
    /// <summary>Below the field (default). It flips up by itself when there is no room.</summary>
    Bottom,

    /// <summary>Above the field.</summary>
    Top,
}

/// <summary>Where selected rows sit in an open <see cref="CdsMultiSelect"/> list (Carbon's <c>selectionFeedback</c>).</summary>
public enum CdsMultiSelectFeedback
{
    /// <summary>Rows that were selected when the list opened are listed first; toggling does not move rows (default).</summary>
    TopAfterReopen,

    /// <summary>Selected rows move to the top as they are chosen.</summary>
    Top,

    /// <summary>Rows keep the order of the items.</summary>
    Fixed,
}

/// <summary>Color keys of a list row. <c>null</c> background means transparent (the list surface shows through).</summary>
/// <param name="Background">Row fill.</param>
/// <param name="Text">Label color.</param>
public readonly record struct CdsListBoxRowVisuals(string? Background, string Text);

/// <summary>Color keys of the selection-count tag inside a <see cref="CdsMultiSelect"/>.</summary>
/// <param name="Background">Pill fill.</param>
/// <param name="Text">Count color.</param>
/// <param name="Icon">Clear (x) icon color.</param>
/// <param name="CloseHover">Fill of the x button while the pointer is over it; <c>null</c> when it has none.</param>
public readonly record struct CdsListBoxTagVisuals(string Background, string Text, string Icon, string? CloseHover);

/// <summary>
/// Maps list-box states to Carbon tokens (from <c>packages/styles/scss/components/list-box</c> and <c>multiselect</c>) and
/// holds its measurements. Dropdown, combo box and multi-select share them.
/// </summary>
public static class CdsListBoxTokens
{
    /// <summary>The list shows about this many rows before it scrolls (a half row hints that there is more).</summary>
    public const double VisibleRows = 5.5;

    /// <summary>Fluid list boxes: the open list is at most this tall (13.75rem), whatever the row height.</summary>
    public const double FluidMaxListHeight = 220;

    /// <summary>Fluid list boxes: row height with <see cref="CdsListBox.IsCondensed"/> (2.5rem; 4rem otherwise).</summary>
    public const double FluidCondensedRowHeight = 40;

    /// <summary>Fluid list boxes: the 64px rows keep their text, check mark and checkbox in this top band (2.5rem).</summary>
    public const double FluidRowContentHeight = 40;

    /// <summary>Size of the chevron, clear and tag-close hit boxes: 1.5rem.</summary>
    public const double ButtonSize = 24;

    /// <summary>Horizontal margin of the row text inside the list: 1rem (0.5rem for inline fields).</summary>
    public static double RowInset(bool inline) => inline ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05;

    /// <summary>Clamps a size into the list-box range sm..lg (the xs size is left out, as in Carbon's list box).</summary>
    public static CdsSize Coerce(CdsSize size) => size switch
    {
        CdsSize.Xs => CdsSize.Sm,
        > CdsSize.Lg => CdsSize.Lg,
        _ => size,
    };

    /// <summary>Row height of a list size: equal to the field height (sm 32, md 40, lg 48).</summary>
    public static double RowHeight(CdsSize size) => Coerce(size).Height();

    /// <summary>The list's maximum height: 5.5 rows (sm 176, md 220, lg 264).</summary>
    public static double MaxListHeight(CdsSize size) => RowHeight(size) * VisibleRows;

    /// <summary>Resolves a row's colors.</summary>
    /// <param name="selected">The row is the chosen item (single select) or a checked row (multi-select).</param>
    /// <param name="highlighted">The list's virtual focus is on the row.</param>
    /// <param name="hover">The pointer is over the row.</param>
    /// <param name="pressed">The row is being pressed.</param>
    /// <param name="disabled">The row (or the whole control) is disabled.</param>
    /// <param name="multi">Multi-select rows show a checkbox instead of a check mark, and selection does not fill them.</param>
    public static CdsListBoxRowVisuals ResolveRow(bool selected, bool highlighted, bool hover, bool pressed, bool disabled, bool multi)
    {
        if (disabled) return new(null, CdsColorKeys.TextDisabled);

        string? background;
        if (pressed) background = CdsColorKeys.LayerSelected01;
        else if (selected && !multi) background = hover ? CdsColorKeys.LayerSelectedHover01 : CdsColorKeys.LayerSelected01;
        else if (hover) background = CdsColorKeys.LayerHover01;
        else background = null;

        var text = multi || selected || highlighted || hover ? CdsColorKeys.TextPrimary : CdsColorKeys.TextSecondary;
        return new(background, text);
    }

    /// <summary>Color of the field's label or value text.</summary>
    public static string FieldText(bool disabled) => disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;

    /// <summary>Color of the chevron and clear icons: dimmed when disabled or read only.</summary>
    public static string FieldIcon(bool disabled, bool readOnly) =>
        disabled || readOnly ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;

    /// <summary>
    /// Fill of the field box for the inline type (<c>null</c> transparent): only the hover shows a <c>layer-hover-01</c> fill.
    /// </summary>
    public static string? InlineFill(bool hover, bool disabled, bool readOnly, bool open) =>
        hover && !disabled && !readOnly && !open ? CdsColorKeys.LayerHover01 : null;

    /// <summary>
    /// Color of the 1px rule under a filled field: <c>border-strong-01</c>, <c>border-subtle-01</c> while the list is open or
    /// the field is read only, none when disabled or inline.
    /// </summary>
    public static string? FieldRule(bool disabled, bool readOnly, bool open, bool flat)
    {
        if (disabled || flat) return null;
        return readOnly || open ? CdsColorKeys.BorderSubtle01 : CdsColorKeys.BorderStrong01;
    }

    /// <summary>Resolves the selection-count tag: a high-contrast filter tag (<c>background-inverse</c>, <c>text-inverse</c>).</summary>
    public static CdsListBoxTagVisuals ResolveTag(bool disabled) => disabled
        ? new(CdsColorKeys.Layer01, CdsColorKeys.TextDisabled, CdsColorKeys.IconDisabled, null)
        : new(CdsColorKeys.BackgroundInverse, CdsColorKeys.TextInverse, CdsColorKeys.IconInverse, CdsColorKeys.ButtonSecondaryHover);
}
