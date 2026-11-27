namespace Maui.Carbon.Gallery.Pages.Forms.Data;

/// <summary>A worker flavor with its price: sample data for rich dropdown rows.</summary>
public sealed record GFlavor(string Name, string Price);

/// <summary>A person: sample data for the combo box.</summary>
public sealed record GPerson(string Name, string Email);

/// <summary>Sample data for the list-box and date picker gallery pages.</summary>
public static class GListData
{
    public static readonly string[] Regions =
    [
        "Dallas (us-south)", "Washington DC (us-east)", "Frankfurt (eu-de)", "London (eu-gb)", "Tokyo (jp-tok)",
    ];

    /// <summary>Tokyo is not available in the sample.</summary>
    public static readonly Func<object?, bool> RegionDisabled = static o => o is string s && s.StartsWith("Tokyo", StringComparison.Ordinal);

    public static readonly string[] Sorts = ["Name", "Date created", "Last deployed", "Monthly cost"];

    public static readonly string[] Versions = ["1.31.2 (default)", "1.30.6", "1.29.10", "1.28.15 (end of support)"];

    public static readonly GFlavor[] Flavors =
    [
        new("bx2.4x16", "$0.19/hr"),
        new("bx2.8x32", "$0.38/hr"),
        new("mx2.8x64", "$0.51/hr"),
        new("gx3.16x80 (1 GPU)", "$2.40/hr"),
    ];

    public static readonly Func<object?, string> FlavorToString = static o => (o as GFlavor)?.Name ?? string.Empty;

    public static readonly string[] Environments = ["Production", "Staging", "Development"];

    public static readonly string[] Zones =
    [
        "us-south-1", "us-south-2", "us-south-3", "us-east-1", "us-east-2", "us-east-3", "eu-de-1", "eu-de-2", "eu-de-3",
        "eu-gb-1", "eu-gb-2", "eu-gb-3", "jp-tok-1", "jp-tok-2", "jp-tok-3",
    ];

    public static readonly string[] Images =
    [
        "Ubuntu 24.04 LTS minimal", "Ubuntu 22.04 LTS", "Debian 12 bookworm", "Red Hat Enterprise Linux 8", "Red Hat Enterprise Linux 9",
        "Windows Server 2022", "CentOS Stream 9", "Rocky Linux 9",
    ];

    public static readonly GPerson[] People =
    [
        new("Amara Okafor", "amara.okafor@example.com"),
        new("Ben Hartmann", "ben.hartmann@example.com"),
        new("Chen Wei", "chen.wei@example.com"),
        new("Elena Petrova", "elena.petrova@example.com"),
        new("Eric Lindqvist", "eric.lindqvist@example.com"),
        new("Farah Haddad", "farah.haddad@example.com"),
    ];

    public static readonly Func<object?, string> PersonToString = static o => o is GPerson p ? $"{p.Name} ({p.Email})" : string.Empty;

    public static readonly string[] Events =
    [
        "Cluster created", "Cluster deleted", "Node pool resized", "Deployment failed", "Certificate expiring", "Quota reached",
    ];

    public static readonly string[] Channels = ["Email", "Slack", "PagerDuty", "Webhook"];

    public static readonly string[] Roles = ["Viewer", "Operator", "Editor", "Administrator", "Billing manager"];

    public static readonly string[] Statuses = ["Succeeded", "In progress", "Failed"];

    /// <summary>Roles for the "select all" examples; the owner role cannot be chosen or cleared by the parent row.</summary>
    public static readonly string[] AccessRoles = ["Viewer", "Operator", "Editor", "Administrator", "Owner"];

    public static readonly string[] SelectedAccessRoles = ["Viewer", "Editor"];

    public static readonly string[] SelectedEnabledRoles = ["Viewer", "Operator", "Editor", "Administrator"];

    public static readonly Func<object?, bool> IsOwnerRole = static o => o as string == "Owner";

    /// <summary>Preselected items for the multi-select (arrays are fixed-size, so each control copies them on change).</summary>
    public static readonly string[] SelectedEvents = ["Deployment failed", "Certificate expiring"];

    public static readonly string[] SelectedChannel = ["Email"];

    public static readonly string[] SelectedChannels = ["Slack", "PagerDuty"];

    public static readonly string[] SelectedRole = ["Operator"];

    public static readonly string[] SelectedRoles = ["Viewer", "Billing manager"];

    public static readonly string[] SelectedStatus = ["Failed"];

    public static readonly string[] SelectedWebhook = ["Webhook"];

    public static readonly string[] Formats = ["CSV", "JSON", "Parquet"];

    /// <summary>A day a few days ahead, for the single picker's selected date.</summary>
    public static DateTime SnapshotDate => DayOfThisMonth(DateTime.Today.Day + 3);

    /// <summary>The earliest choosable date of the single example: two days ago.</summary>
    public static DateTime EarliestDate => DayOfThisMonth(DateTime.Today.Day - 2);

    public static DateTime RangeStart => DayOfThisMonth(6);

    public static DateTime RangeEnd => DayOfThisMonth(19);

    public static DateTime Today => DateTime.Today;

    /// <summary>A date in February of next year, for the ISO-format example.</summary>
    public static DateTime CertificateExpiry => new(DateTime.Today.Year + 1, 2, 14);

    /// <summary>Weekends are unavailable in the disabled-days example.</summary>
    public static readonly Func<DateTime, bool> IsWeekend = static d => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>The day of this month, clamped to 28 so every month has it.</summary>
    public static DateTime DayOfThisMonth(int day)
    {
        var now = DateTime.Today;
        return new DateTime(now.Year, now.Month, Math.Clamp(day, 1, 28));
    }
}
