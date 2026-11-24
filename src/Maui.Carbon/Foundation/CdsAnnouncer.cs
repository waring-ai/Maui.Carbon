namespace Maui.Carbon;

/// <summary>How urgently a screen reader should speak an announcement (the web's <c>aria-live</c> politeness).</summary>
public enum CdsAnnouncePriority
{
    /// <summary>
    /// Queued after what the screen reader is saying (<c>aria-live="polite"</c>, <c>role="status"</c>): counts, progress,
    /// "Copied", sort and selection changes. The default.
    /// </summary>
    Polite,
    /// <summary>
    /// Interrupts the current speech (<c>aria-live="assertive"</c>, <c>role="alert"</c>): errors and alerts the user must
    /// hear now. Use sparingly.
    /// </summary>
    Assertive,
}

/// <summary>An announcement made through <see cref="CdsAnnouncer"/>.</summary>
/// <param name="Text">The text spoken.</param>
/// <param name="Priority">How urgently it was spoken.</param>
public readonly record struct CdsAnnouncement(string Text, CdsAnnouncePriority Priority);

/// <summary>
/// Speaks text through the platform screen reader, with a priority: on iOS and Mac Catalyst a UIAccessibility announcement
/// whose <c>UIAccessibilitySpeechAttributeQueueAnnouncement</c> is <c>true</c> for polite (queued) and <c>false</c> for
/// assertive (interrupting); on Android <c>announceForAccessibility</c>; on Windows a UI Automation notification
/// (<c>MostRecent</c> for polite, <c>ImportantMostRecent</c> for assertive). Elsewhere it falls back to
/// <c>SemanticScreenReader.Announce</c>, and platforms (and unit tests) without a screen reader are ignored.
/// Use it instead of <c>SemanticScreenReader.Announce</c>: <c>CdsAnnouncer.Announce("3 items selected");</c>,
/// <c>CdsAnnouncer.Announce(error, CdsAnnouncePriority.Assertive);</c>.
/// </summary>
public static partial class CdsAnnouncer
{
    static readonly AsyncLocal<List<CdsAnnouncement>?> s_capture = new();

    /// <summary>Speaks <paramref name="text"/> with <paramref name="priority"/>; does nothing for <c>null</c> or empty text.</summary>
    public static void Announce(string? text, CdsAnnouncePriority priority = CdsAnnouncePriority.Polite)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var announcement = new CdsAnnouncement(text, priority);
        s_capture.Value?.Add(announcement);
        try
        {
            var handled = false;
            AnnouncePlatform(text, priority, ref handled);
            if (!handled) SemanticScreenReader.Announce(text);
        }
        catch (Exception)
        {
            // No screen reader implementation on this platform (unit tests, a window not created yet).
        }
    }

    /// <summary>
    /// Test seam: records the announcements made on this logical call context (the test's thread and its awaits) until
    /// the returned scope is disposed. <c>using var heard = CdsAnnouncer.Capture(); …; Assert.Contains(…, heard.Announcements);</c>
    /// </summary>
    internal static CaptureScope Capture()
    {
        var scope = new CaptureScope(s_capture.Value);
        s_capture.Value = scope.Announcements;
        return scope;
    }

    /// <summary>A capture started by <see cref="Capture"/>.</summary>
    internal sealed class CaptureScope(List<CdsAnnouncement>? previous) : IDisposable
    {
        /// <summary>The announcements made since the capture started, oldest first.</summary>
        public List<CdsAnnouncement> Announcements { get; } = [];

        /// <summary>Stops capturing.</summary>
        public void Dispose() => s_capture.Value = previous;
    }

    /// <summary>Hands the announcement to the platform; sets <paramref name="handled"/> when it did.</summary>
    static partial void AnnouncePlatform(string text, CdsAnnouncePriority priority, ref bool handled);
}
