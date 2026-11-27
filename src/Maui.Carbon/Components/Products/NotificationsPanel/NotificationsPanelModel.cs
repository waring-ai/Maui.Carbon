namespace Maui.Carbon;

/// <summary>The time groups of a notifications panel.</summary>
internal enum NotificationsPanelGroup
{
    Today,
    Yesterday,
    Previous,
}

/// <summary>Grouping, ordering and relative-time rules of Carbon for IBM Products' notifications panel (pure, unit-tested).</summary>
internal static class NotificationsPanelModel
{
    /// <summary>Descriptions longer than this are clamped to two lines with a Read more button.</summary>
    public const int TrimLength = 88;

    public static bool NeedsReadMore(string? description) => description is { Length: > TrimLength };

    /// <summary>The group of a timestamp: since local midnight today, since midnight yesterday, or before.</summary>
    public static NotificationsPanelGroup GroupOf(DateTimeOffset timestamp, DateTimeOffset now)
    {
        var startOfToday = new DateTimeOffset(now.Date, now.Offset);
        if (timestamp >= startOfToday) return NotificationsPanelGroup.Today;
        return timestamp >= startOfToday.AddDays(-1) ? NotificationsPanelGroup.Yesterday : NotificationsPanelGroup.Previous;
    }

    /// <summary>The items of each group, newest first. Empty groups are left out.</summary>
    public static IReadOnlyList<(NotificationsPanelGroup Group, IReadOnlyList<T> Items)> Group<T>(IEnumerable<T> items, Func<T, DateTimeOffset> time, DateTimeOffset now)
    {
        var groups = new List<(NotificationsPanelGroup, IReadOnlyList<T>)>();
        foreach (var g in new[] { NotificationsPanelGroup.Today, NotificationsPanelGroup.Yesterday, NotificationsPanelGroup.Previous })
        {
            var members = items.Where(i => GroupOf(time(i), now) == g).OrderByDescending(time).ToList();
            if (members.Count > 0) groups.Add((g, members));
        }
        return groups;
    }

    /// <summary>
    /// The relative time of Carbon's default English formatter (<c>timeAgo</c>): "Now" up to 10 seconds, then seconds,
    /// minutes, hours, "Yesterday at …" for one day, days, months (30 days) and years (365 days).
    /// </summary>
    public static string TimeAgo(DateTimeOffset then, DateTimeOffset now, IFormatProvider? culture = null)
    {
        var elapsed = now - then;
        static int R(double v) => (int)Math.Round(v, MidpointRounding.AwayFromZero);
        if (elapsed.TotalMinutes < 1)
        {
            var s = R(elapsed.TotalSeconds);
            return s > 10 ? $"{s} seconds ago" : "Now";
        }
        if (elapsed.TotalHours < 1)
        {
            var m = R(elapsed.TotalMinutes);
            return m > 1 ? $"{m} minutes ago" : $"{m} minute ago";
        }
        if (elapsed.TotalDays < 1)
        {
            var h = R(elapsed.TotalHours);
            return h > 1 ? $"{h} hours ago" : $"{h} hour ago";
        }
        if (elapsed.TotalDays < 30)
        {
            var d = R(elapsed.TotalDays);
            return d > 1 ? $"{d} days ago" : $"Yesterday at {then.ToLocalTime().ToString("t", culture)}";
        }
        if (elapsed.TotalDays < 365)
        {
            var mo = R(elapsed.TotalDays / 30);
            return mo > 1 ? $"{mo} months ago" : $"{mo} month ago";
        }
        var y = R(elapsed.TotalDays / 365);
        return y > 1 ? $"{y} years ago" : $"{y} year ago";
    }
}
