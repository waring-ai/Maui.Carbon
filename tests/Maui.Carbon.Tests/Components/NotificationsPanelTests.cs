namespace Maui.Carbon.Tests.Components;

public class NotificationsPanelTests
{
    static readonly DateTimeOffset Now = new(2026, 11, 27, 15, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(5, "Now")]
    [InlineData(30, "30 seconds ago")]
    [InlineData(60, "1 minute ago")]
    [InlineData(600, "10 minutes ago")]
    [InlineData(3600, "1 hour ago")]
    [InlineData(5 * 3600, "5 hours ago")]
    [InlineData(3 * 86400, "3 days ago")]
    [InlineData(45 * 86400, "2 months ago")]
    [InlineData(800 * 86400, "2 years ago")]
    public void Time_ago_matches_the_default_formatter(int secondsAgo, string expected) =>
        Assert.Equal(expected, NotificationsPanelModel.TimeAgo(Now.AddSeconds(-secondsAgo), Now));

    [Fact]
    public void One_day_ago_says_yesterday_at()
    {
        Assert.StartsWith("Yesterday at ", NotificationsPanelModel.TimeAgo(Now.AddHours(-25), Now));
    }

    [Fact]
    public void Groups_today_yesterday_previous_newest_first()
    {
        DateTimeOffset[] times = [Now.AddHours(-1), Now.AddDays(-5), Now.AddMinutes(-5), Now.AddHours(-20), Now.AddDays(-1).AddHours(-2)];
        var groups = NotificationsPanelModel.Group(times, t => t, Now);
        Assert.Equal([NotificationsPanelGroup.Today, NotificationsPanelGroup.Yesterday, NotificationsPanelGroup.Previous], groups.Select(g => g.Group));
        Assert.Equal([Now.AddMinutes(-5), Now.AddHours(-1)], groups[0].Items);
        Assert.Equal(2, groups[1].Items.Count);
        Assert.Single(groups[2].Items);
    }

    [Fact]
    public void Long_descriptions_get_read_more()
    {
        Assert.False(NotificationsPanelModel.NeedsReadMore(new string('a', 88)));
        Assert.True(NotificationsPanelModel.NeedsReadMore(new string('a', 89)));
        Assert.False(NotificationsPanelModel.NeedsReadMore(null));
    }

    [Fact]
    public void Opens_in_the_corner_lists_rows_and_closes_on_escape()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        var items = new System.Collections.ObjectModel.ObservableCollection<CdsNotificationsPanelItem>
        {
            new() { Kind = CdsNotificationKind.Error, Title = "Deployment failed", Timestamp = DateTimeOffset.Now.AddMinutes(-3), IsUnread = true },
            new() { Kind = CdsNotificationKind.Success, Title = "Cluster created", Timestamp = DateTimeOffset.Now.AddDays(-3) },
        };
        var panel = new CdsNotificationsPanel { ItemsSource = items, HasDoNotDisturb = true, HasFooterActions = true };
        page.Add(panel);
        var outside = 0;
        panel.ClickOutside += (_, _) => outside++;
        CdsNotificationsPanelItem? dismissed = null;
        panel.NotificationDismissed += (_, i) => dismissed = i;
        panel.IsOpen = true;
        panel.Show();

        var overlay = Assert.Single(host.Overlays);
        Assert.Equal(CdsOverlayDock.TopEnd, overlay.Options.Dock);
        Assert.True(overlay.Options.LightDismiss);
        Assert.Equal(panel.DismissAllButton, overlay.Options.InitialFocus);
        // Two group labels and two rows.
        Assert.Equal(2, panel.Rows.OfType<CdsNotificationsPanel.NotificationRow>().Count());

        panel.Dismiss(items[0]);
        Assert.Same(items[0], dismissed);
        items.Add(new CdsNotificationsPanelItem { Title = "Backup done", Timestamp = DateTimeOffset.Now });
        Assert.Equal(3, panel.Rows.OfType<CdsNotificationsPanel.NotificationRow>().Count());

        Assert.True(host.RouteKey(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.False(panel.IsOpen);
        Assert.Equal(1, outside);
        Assert.Empty(host.Overlays);
    }
}
