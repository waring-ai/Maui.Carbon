namespace Maui.Carbon;

/// <summary>The focusable items of an actionable notification.</summary>
internal enum NotificationFocusItem
{
    /// <summary>None (focus is outside, or there is nothing to focus).</summary>
    None,
    /// <summary>The action button.</summary>
    Action,
    /// <summary>The close button.</summary>
    Close,
}

/// <summary>Where Tab goes inside an actionable notification with the alert-dialog role (pure, tested).</summary>
internal static class NotificationFocusModel
{
    /// <summary>
    /// The item a focus sentinel sends focus to. From inside (Tab from the last item, Shift+Tab from the first) focus wraps
    /// to the other item, or stays on the only one. From outside it enters at the nearest item: the action from the start
    /// sentinel, the close button from the end sentinel.
    /// </summary>
    public static NotificationFocusItem SentinelTarget(bool startSentinel, NotificationFocusItem cameFrom, bool hasAction, bool hasClose)
    {
        bool Has(NotificationFocusItem item) => item switch
        {
            NotificationFocusItem.Action => hasAction,
            NotificationFocusItem.Close => hasClose,
            _ => false,
        };

        if (!hasAction && !hasClose) return NotificationFocusItem.None;
        if (cameFrom != NotificationFocusItem.None)
        {
            var other = cameFrom == NotificationFocusItem.Action ? NotificationFocusItem.Close : NotificationFocusItem.Action;
            return Has(other) ? other : cameFrom;
        }
        var preferred = startSentinel ? NotificationFocusItem.Action : NotificationFocusItem.Close;
        return Has(preferred) ? preferred : preferred == NotificationFocusItem.Action ? NotificationFocusItem.Close : NotificationFocusItem.Action;
    }
}
