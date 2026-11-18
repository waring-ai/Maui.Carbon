namespace Maui.Carbon.Gallery.Pages.Patterns.Data;

/// <summary>A storage bucket row of the notifications pattern page.</summary>
public sealed record K2Bucket(string Name, string Location, string Size);

/// <summary>A deployment row of the overflow content pattern page.</summary>
public sealed record K2Deployment(string Name, string Description, string Updated);

/// <summary>Sample data and helpers for the K2 pattern pages (notifications, confirmation, overflow content, login).</summary>
public static class K2Data
{
    /// <summary>The buckets table of the notifications page.</summary>
    public static IReadOnlyList<K2Bucket> Buckets { get; } =
    [
        new("web-assets-eu", "Frankfurt", "1.2 GB"),
        new("backups-prod", "Dallas", "412 GB"),
        new("logs-archive", "London", "86 GB"),
        new("ci-cache", "Frankfurt", "9.4 GB"),
        new("audit-logs-eu", "Frankfurt", "3.1 GB"),
        new("ml-datasets", "Dallas", "2.7 TB"),
    ];

    /// <summary>The deployments table of the overflow content page.</summary>
    public static IReadOnlyList<K2Deployment> Deployments { get; } =
    [
        new("checkout-api-canary-eu-frankfurt", "Canary release of checkout-api 4.3.0 receiving 5% of storefront traffic in Frankfurt", "2 minutes ago"),
        new("search-indexer", "Rebuilds the product search index from the catalog database every night at 02:00", "1 hour ago"),
        new("payments-webhook-relay-v2", "Relays payment provider webhooks to the internal event bus with retries", "Yesterday"),
    ];

    /// <summary>Recent cluster events, loaded a few at a time.</summary>
    public static IReadOnlyList<string> Events { get; } =
    [
        "Node kube-pool1-c3 drained for maintenance",
        "Autoscaler added node kube-pool2-a4",
        "Ingress certificate renewed",
        "checkout-api 4.3.0 deployed",
        "Node kube-pool1-a1 rebooted",
        "Worker pool pool3 resized to 6 nodes",
        "search-indexer job completed",
        "Backup snap-2026-11-17-0200 created",
        "Autoscaler removed node kube-pool2-b5",
        "Ingress rule /api/v2 added",
    ];

    /// <summary>Keeps the start and the end of <paramref name="text"/> and drops the middle (at least 3 characters).</summary>
    public static string MidTruncate(string text, int head, int tail)
        => text.Length <= head + tail + 3 ? text : string.Concat(text.AsSpan(0, head), "…", text.AsSpan(text.Length - tail));

    /// <summary>
    /// Calls <paramref name="onBlur"/> when the text input loses focus. <see cref="CdsTextInput"/> has no blur event, so this
    /// finds its native entry in the visual tree.
    /// </summary>
    public static void OnBlur(CdsTextInput input, Action onBlur)
    {
        var entry = input.GetVisualTreeDescendants().OfType<Entry>().FirstOrDefault();
        if (entry is not null) entry.Unfocused += (_, _) => onBlur();
        else input.Loaded += (_, _) => OnBlur(input, onBlur);
    }
}
