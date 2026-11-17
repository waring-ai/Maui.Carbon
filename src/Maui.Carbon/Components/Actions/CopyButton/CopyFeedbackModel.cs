namespace Maui.Carbon;

/// <summary>
/// The feedback state of a copy button: "copied" from activation until the timeout passes. A second activation inside
/// the window restarts it; an expiry of an older activation is ignored.
/// </summary>
internal sealed class CopyFeedbackModel
{
    int _generation;

    /// <summary>True while the feedback ("Copied!") is showing.</summary>
    public bool IsCopied { get; private set; }

    /// <summary>Starts (or restarts) the feedback and returns the token the matching <see cref="Expire"/> must present.</summary>
    public int Start()
    {
        IsCopied = true;
        return ++_generation;
    }

    /// <summary>Ends the feedback if <paramref name="token"/> belongs to the latest <see cref="Start"/>. Returns whether it ended.</summary>
    public bool Expire(int token)
    {
        if (token != _generation || !IsCopied) return false;
        IsCopied = false;
        return true;
    }

    /// <summary>Ends the feedback at once (the control was disabled or unloaded).</summary>
    public void Reset()
    {
        _generation++;
        IsCopied = false;
    }

    /// <summary>The tooltip text: the feedback while copied, otherwise the description. Empty values fall back to the defaults.</summary>
    public static string TooltipText(bool isCopied, string? description, string? feedback) =>
        isCopied
            ? string.IsNullOrEmpty(feedback) ? "Copied!" : feedback
            : string.IsNullOrEmpty(description) ? "Copy to clipboard" : description;
}
