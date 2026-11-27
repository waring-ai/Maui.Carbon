namespace Maui.Carbon;

/// <summary>The remove modal's confirmation rules (pure).</summary>
internal static class RemoveModalModel
{
    /// <summary>True when the typed text is exactly the resource name (case-sensitive, nothing trimmed).</summary>
    public static bool IsConfirmed(string? input, string? resourceName) =>
        string.Equals(input ?? string.Empty, resourceName ?? string.Empty, StringComparison.Ordinal);

    /// <summary>
    /// The danger button is disabled when the app says so (that wins) or, with text confirmation, until the typed text is
    /// the resource name.
    /// </summary>
    public static bool IsPrimaryDisabled(bool primaryButtonDisabled, bool textConfirmation, string? input, string? resourceName) =>
        primaryButtonDisabled || (textConfirmation && !IsConfirmed(input, resourceName));

    /// <summary>The input shows its invalid text once the person left it holding text that does not match.</summary>
    public static bool ShowsInvalid(bool textConfirmation, bool leftInput, string? input, string? resourceName) =>
        textConfirmation && leftInput && !string.IsNullOrEmpty(input) && !IsConfirmed(input, resourceName);
}
