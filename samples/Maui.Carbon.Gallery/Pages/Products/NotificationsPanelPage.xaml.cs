using System.Collections.ObjectModel;
using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Notifications panel", GalleryCategory.Products, Order = 30, Spec = "NotificationsPanel",
    Summary = "The history of system notifications, opened from a header action and grouped by day.")]
public partial class NotificationsPanelPage : GalleryPage
{
    readonly ObservableCollection<CdsNotificationsPanelItem> _items;
    int _added;

    public NotificationsPanelPage()
    {
        InitializeComponent();
        var now = DateTimeOffset.Now;
        _items =
        [
            new() { Kind = CdsNotificationKind.Error, Title = "Deployment failed", IsUnread = true, Timestamp = now.AddMinutes(-4),
                    Description = "checkout-api could not roll out version 2.4.1 to prod-eu-de-1 because two pods failed their readiness probe.",
                    LinkText = "View logs", LinkUrl = "https://example.com/logs" },
            new() { Kind = CdsNotificationKind.Warning, Title = "Quota nearly reached", IsUnread = true, Timestamp = now.AddHours(-2),
                    Description = "9 of 10 clusters used." },
            new() { Kind = CdsNotificationKind.Success, Title = "Cluster created", Timestamp = now.AddDays(-1).AddHours(-1),
                    Description = "prod-us-east-1 is ready." },
            new() { Kind = CdsNotificationKind.Info, Title = "Scheduled maintenance", Timestamp = now.AddDays(-6),
                    Description = "Control plane patches start on December 2 at 02:00 UTC. Workloads keep running." },
        ];
        Panel.ItemsSource = _items;
        Bell.BadgeCount = _items.Count(i => i.IsUnread);
    }

    void OnBellClicked(object? sender, EventArgs e) => Panel.IsOpen = !Panel.IsOpen;

    void OnEmptyClicked(object? sender, EventArgs e) => EmptyPanel.IsOpen = true;

    void OnAddClicked(object? sender, EventArgs e)
    {
        _added++;
        _items.Add(new CdsNotificationsPanelItem
        {
            Kind = CdsNotificationKind.Success, Title = $"Backup {_added} completed", IsUnread = true, Timestamp = DateTimeOffset.Now,
        });
        Bell.BadgeCount = _items.Count(i => i.IsUnread);
    }

    void OnNotificationClicked(object? sender, CdsNotificationsPanelItem item)
    {
        item.IsUnread = false;
        Bell.BadgeCount = _items.Count(i => i.IsUnread);
        Status.Text = $"Opened \"{item.Title}\"";
    }

    void OnDismissed(object? sender, CdsNotificationsPanelItem item)
    {
        _items.Remove(item);
        Bell.BadgeCount = _items.Count(i => i.IsUnread);
    }

    void OnDismissAll(object? sender, EventArgs e)
    {
        _items.Clear();
        Bell.BadgeCount = null;
    }

    void OnViewAll(object? sender, EventArgs e) => Status.Text = "View all";

    void OnSettings(object? sender, EventArgs e) => Status.Text = "Notification settings";

    void OnDoNotDisturb(object? sender, EventArgs e) => Status.Text = Panel.IsDoNotDisturb ? "Do not disturb on" : "Do not disturb off";
}
