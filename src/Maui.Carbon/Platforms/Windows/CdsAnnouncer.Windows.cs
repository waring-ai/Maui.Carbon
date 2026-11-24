using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;

namespace Maui.Carbon;

public static partial class CdsAnnouncer
{
    const string ActivityId = "Maui.Carbon.Announcement";

    // A UI Automation notification on the window content: Narrator speaks the latest polite one when it can, and an
    // assertive one at once, replacing any pending notification of this activity.
    static partial void AnnouncePlatform(string text, CdsAnnouncePriority priority, ref bool handled)
    {
        if (Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView is not Microsoft.UI.Xaml.Window window ||
            window.Content is not UIElement root)
            return;
        var peer = FrameworkElementAutomationPeer.FromElement(root) ?? FrameworkElementAutomationPeer.CreatePeerForElement(root);
        if (peer is null) return;
        peer.RaiseNotificationEvent(
            AutomationNotificationKind.Other,
            priority == CdsAnnouncePriority.Assertive ? AutomationNotificationProcessing.ImportantMostRecent : AutomationNotificationProcessing.MostRecent,
            text,
            ActivityId);
        handled = true;
    }
}
