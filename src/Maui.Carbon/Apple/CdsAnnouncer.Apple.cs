using Foundation;
using UIKit;

namespace Maui.Carbon;

public static partial class CdsAnnouncer
{
    static partial void AnnouncePlatform(string text, CdsAnnouncePriority priority, ref bool handled)
    {
        // Queued (polite) announcements wait for current speech; the others interrupt it.
        var attributes = new NSMutableDictionary
        {
            [UIView.SpeechAttributeQueueAnnouncement] = NSNumber.FromBoolean(priority == CdsAnnouncePriority.Polite),
        };
        if (priority == CdsAnnouncePriority.Assertive && (OperatingSystem.IsIOSVersionAtLeast(17) || OperatingSystem.IsMacCatalystVersionAtLeast(17)) &&
            UIAccessibilityPriority.High.GetConstant() is { } high)
            attributes[UIView.SpeechAttributeAnnouncementPriority] = high;
        using var message = new NSAttributedString(text, attributes);
        UIAccessibility.PostNotification(UIAccessibilityPostNotification.Announcement, message);
        handled = true;
    }
}
