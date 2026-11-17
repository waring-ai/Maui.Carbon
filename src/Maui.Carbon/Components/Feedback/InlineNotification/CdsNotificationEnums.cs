namespace Maui.Carbon;

/// <summary>The status a notification reports. The kind picks the icon and the accent colors.</summary>
public enum CdsNotificationKind
{
    /// <summary>A failure (<c>error--filled</c>, <c>support-error</c>).</summary>
    Error,
    /// <summary>Neutral information (<c>information--filled</c>, <c>support-info</c>). The default for inline notifications and callouts.</summary>
    Info,
    /// <summary>Neutral information with a square glyph (<c>information--square--filled</c>).</summary>
    InfoSquare,
    /// <summary>A completed action (<c>checkmark--filled</c>, <c>support-success</c>).</summary>
    Success,
    /// <summary>A risk (<c>warning--filled</c>, <c>support-warning</c>).</summary>
    Warning,
    /// <summary>A risk with the alternate glyph (<c>warning--alt--filled</c>).</summary>
    WarningAlt,
}

/// <summary>How a notification is announced to screen readers when it appears.</summary>
public enum CdsNotificationRole
{
    /// <summary>Polite live region: announced without interrupting (the default for inline and toast notifications).</summary>
    Status,
    /// <summary>Assertive live region: interrupts. Only for urgent errors.</summary>
    Alert,
    /// <summary>A log: appended messages, announced politely.</summary>
    Log,
    /// <summary>Alert dialog (the default for actionable notifications): announced, and focus moves to the action button when <c>HasFocus</c> is set.</summary>
    AlertDialog,
    /// <summary>Not announced.</summary>
    None,
}

/// <summary>Event data of <c>CloseRequested</c>. Set <see cref="Cancel"/> to keep the notification open.</summary>
public sealed class CdsNotificationCloseEventArgs : EventArgs
{
    /// <summary>True to keep the notification open (Carbon's <c>onClose</c> returning <c>false</c>).</summary>
    public bool Cancel { get; set; }
}
