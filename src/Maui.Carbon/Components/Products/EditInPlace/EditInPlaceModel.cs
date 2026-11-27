namespace Maui.Carbon;

/// <summary>What leaving an edit-in-place field (or pressing Enter in it) does.</summary>
internal enum EditInPlaceOutcome
{
    /// <summary>Keep the new value: it becomes the value the next cancel returns to.</summary>
    Save,
    /// <summary>Go back to the value the edit started from.</summary>
    Cancel,
    /// <summary>The app's own blur handler decides (Carbon's <c>onBlur</c>).</summary>
    Custom,
    /// <summary>Leave edit mode and change nothing.</summary>
    Leave,
}

/// <summary>
/// Pure rules of <see cref="CdsEditInPlace"/>, from <c>EditInPlace.tsx</c> and <c>_edit-in-place.scss</c>: the size scale
/// (the toolbar squares are one step larger than the input), when save and cancel are possible (a trimmed change, and no
/// error for save), what blur and Enter do, and the toolbar width.
/// </summary>
internal static class EditInPlaceModel
{
    /// <summary>The sizes the component has: sm (default), md and lg. Smaller sizes clamp to sm, larger to lg.</summary>
    public static CdsSize Clamp(CdsSize size) => size < CdsSize.Sm ? CdsSize.Sm : size > CdsSize.Lg ? CdsSize.Lg : size;

    /// <summary>The height of the text input (Carbon's <c>cds--text-input--sm|md|lg</c>): 32, 40 or 48.</summary>
    public static double InputHeight(CdsSize size) => Clamp(size).Height();

    /// <summary>
    /// The side of the square toolbar buttons and so the height of the component (<c>--size</c>): sm <c>spacing-08</c>
    /// (40), md <c>spacing-09</c> (48), lg <c>spacing-10</c> (64).
    /// </summary>
    public static double Side(CdsSize size) => Clamp(size) switch
    {
        CdsSize.Sm => CdsSpacing.Spacing08,
        CdsSize.Md => CdsSpacing.Spacing09,
        _ => CdsSpacing.Spacing10,
    };

    /// <summary>The room the error icon takes in the toolbar: 16 plus 8 on each side (<c>spacing-07</c>).</summary>
    public const double InvalidIconRoom = CdsSpacing.Spacing07;

    /// <summary>Whether the value differs from the one the edit started from, ignoring leading and trailing spaces.</summary>
    public static bool HasChanged(string? value, string? initial) => (value ?? "").Trim() != (initial ?? "").Trim();

    /// <summary>Save is possible when the value changed and is not invalid.</summary>
    public static bool CanSave(string? value, string? initial, bool invalid) => HasChanged(value, initial) && !invalid;

    /// <summary>Cancel is possible when the value changed.</summary>
    public static bool CanCancel(string? value, string? initial) => HasChanged(value, initial);

    /// <summary>
    /// The toolbar width: one square (edit, or edit-off when read only), two while editing (cancel and save), plus the
    /// error icon's room when invalid.
    /// </summary>
    public static double ToolbarWidth(CdsSize size, bool editing, bool invalid, bool readOnly) =>
        (editing && !readOnly ? 2 : 1) * Side(size) + (invalid ? InvalidIconRoom : 0);

    /// <summary>
    /// What focus leaving the component does: the app's handler when it has one, else save when the value can be saved,
    /// else cancel (which restores the value the edit started from).
    /// </summary>
    public static EditInPlaceOutcome Blur(bool customHandler, bool canSave) =>
        customHandler ? EditInPlaceOutcome.Custom : canSave ? EditInPlaceOutcome.Save : EditInPlaceOutcome.Cancel;

    /// <summary>Enter saves when the value can be saved (and the field stays in edit mode); otherwise it leaves edit mode.</summary>
    public static EditInPlaceOutcome Enter(bool canSave) => canSave ? EditInPlaceOutcome.Save : EditInPlaceOutcome.Leave;
}
