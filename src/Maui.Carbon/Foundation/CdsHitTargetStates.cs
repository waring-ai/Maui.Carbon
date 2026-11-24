namespace Maui.Carbon;

/// <summary>
/// The words the platform handlers of <see cref="CdsHitTarget"/> use for states that have no native slot on every platform
/// (Apple's accessibility value, Android's state description, Windows' item status). Pure, so it is unit-tested.
/// </summary>
internal static class CdsHitTargetStates
{
    public const string ReadOnlyWord = "read only";

    /// <summary>Roles whose <see cref="CdsHitTarget.IsChecked"/> is reported.</summary>
    public static bool IsCheckable(CdsAccessibilityRole role) =>
        role is CdsAccessibilityRole.Checkbox or CdsAccessibilityRole.Radio or CdsAccessibilityRole.Switch or CdsAccessibilityRole.ToggleButton;

    /// <summary>"checked", "not checked" or "mixed" ("on"/"off" for a switch); <c>null</c> for roles that are not checkable.</summary>
    public static string? CheckedWord(CdsAccessibilityRole role, bool? isChecked) => role switch
    {
        CdsAccessibilityRole.Switch => isChecked == true ? "on" : "off",
        CdsAccessibilityRole.Checkbox or CdsAccessibilityRole.Radio or CdsAccessibilityRole.ToggleButton =>
            isChecked switch { true => "checked", false => "not checked", null => "mixed" },
        _ => null,
    };

    /// <summary>"expanded" or "collapsed"; <c>null</c> when the target expands nothing.</summary>
    public static string? ExpandedWord(bool? isExpanded) => isExpanded switch
    {
        true => "expanded",
        false => "collapsed",
        null => null,
    };

    /// <summary>"current page", "current step"…; <c>null</c> for <see cref="CdsCurrentKind.None"/>.</summary>
    public static string? CurrentWord(CdsCurrentKind kind) => kind switch
    {
        CdsCurrentKind.Page => "current page",
        CdsCurrentKind.Step => "current step",
        CdsCurrentKind.Location => "current location",
        CdsCurrentKind.Date => "current date",
        CdsCurrentKind.Item => "current",
        _ => null,
    };

    /// <summary>
    /// The Apple accessibility value: the checked word, the expanded word (unless the OS reports expansion natively), the
    /// current word and "read only", joined with commas; <c>null</c> when there is nothing to say.
    /// </summary>
    public static string? AppleValue(CdsAccessibilityRole role, bool? isChecked, bool? isExpanded, bool isReadOnly, CdsCurrentKind current, bool nativeExpandedStatus) =>
        Join(CheckedWord(role, isChecked), nativeExpandedStatus ? null : ExpandedWord(isExpanded), CurrentWord(current), isReadOnly ? ReadOnlyWord : null);

    /// <summary>
    /// The Android state description. TalkBack already speaks checked and selected, so this is <c>null</c> unless there
    /// is more to say (mixed, expanded or collapsed, current, read only); a state description replaces TalkBack's own
    /// checked words, so for checkable roles it then starts with the checked word.
    /// </summary>
    public static string? AndroidStateDescription(CdsAccessibilityRole role, bool? isChecked, bool? isExpanded, bool isReadOnly, CdsCurrentKind current)
    {
        var mixed = isChecked is null && IsCheckable(role) && role != CdsAccessibilityRole.Switch;
        if (!mixed && isExpanded is null && !isReadOnly && current == CdsCurrentKind.None) return null;
        return Join(CheckedWord(role, isChecked), ExpandedWord(isExpanded), CurrentWord(current), isReadOnly ? ReadOnlyWord : null);
    }

    /// <summary>The role description TalkBack speaks for roles Android has no widget class for; <c>null</c> otherwise.</summary>
    public static string? AndroidRoleDescription(CdsAccessibilityRole role) => role switch
    {
        CdsAccessibilityRole.Link => "link",
        CdsAccessibilityRole.Tab => "tab",
        CdsAccessibilityRole.MenuItem => "menu item",
        CdsAccessibilityRole.Option => "option",
        _ => null,
    };

    /// <summary>The Android widget class reported for a role (TalkBack derives the spoken role from it).</summary>
    public static string AndroidClassName(CdsAccessibilityRole role) => role switch
    {
        CdsAccessibilityRole.Checkbox => "android.widget.CheckBox",
        CdsAccessibilityRole.Radio => "android.widget.RadioButton",
        CdsAccessibilityRole.Switch => "android.widget.Switch",
        CdsAccessibilityRole.ToggleButton => "android.widget.ToggleButton",
        CdsAccessibilityRole.Adjustable => "android.widget.SeekBar",
        CdsAccessibilityRole.ComboBox => "android.widget.Spinner",
        // No widget class: a plain view with a role description, so TalkBack does not call them buttons.
        CdsAccessibilityRole.Link or CdsAccessibilityRole.Tab or CdsAccessibilityRole.MenuItem or CdsAccessibilityRole.Option => "android.view.View",
        _ => "android.widget.Button",
    };

    static string? Join(params string?[] parts)
    {
        var words = parts.Where(p => !string.IsNullOrEmpty(p)).ToArray();
        return words.Length == 0 ? null : string.Join(", ", words);
    }
}
