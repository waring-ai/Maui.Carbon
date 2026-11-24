using Microsoft.Maui.ApplicationModel;

namespace Maui.Carbon;

public static partial class CdsAnnouncer
{
    // Android has no politeness for announcements: TalkBack queues them after the current utterance either way.
    static partial void AnnouncePlatform(string text, CdsAnnouncePriority priority, ref bool handled)
    {
        if (Platform.CurrentActivity?.Window?.DecorView is not { } view) return;
#pragma warning disable CA1422 // Deprecated on API 36 in favour of live regions and pane titles; still delivered by TalkBack.
        view.AnnounceForAccessibility(text);
#pragma warning restore CA1422
        handled = true;
    }
}
