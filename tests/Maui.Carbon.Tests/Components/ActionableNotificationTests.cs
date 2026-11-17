namespace Maui.Carbon.Tests.Components;

public class ActionableNotificationTests
{
    [Fact]
    public void Defaults_follow_the_readme()
    {
        var n = new CdsActionableNotification();
        Assert.Equal(CdsNotificationKind.Error, n.Kind);
        Assert.False(n.IsInline);
        Assert.True(n.HasFocus);
        Assert.Equal(CdsNotificationRole.AlertDialog, n.Role);
        Assert.Equal(288, n.WidthRequest);
    }

    [Fact]
    public void Inline_layout_is_fluid_and_toast_layout_is_288()
    {
        var n = new CdsActionableNotification(CdsNotificationKind.Warning, "Payment method expires soon", "The card ending 4242 expires on 30 Nov 2026.", "Update card")
        {
            IsLowContrast = true,
        };
        n.IsInline = true;
        Assert.Equal(608, n.MaximumWidthRequest);
        Assert.Equal(-1, n.WidthRequest);
        n.IsInline = false;
        Assert.Equal(288, n.WidthRequest);
        Assert.Equal(288, n.MaximumWidthRequest);
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var n = new CdsActionableNotification
        {
            Title = "Backup failed",
            Subtitle = "Retry in a few minutes.",
            Caption = "2 minutes ago",
            ActionButtonLabel = "View logs",
            HasFocus = false,
            Role = CdsNotificationRole.Status,
            HideCloseButton = true,
            ActionCommand = new Command(() => { }),
            ActionCommandParameter = 3,
        };
        foreach (var kind in Enum.GetValues<CdsNotificationKind>()) n.Kind = kind;
        n.ActionButtonLabel = null;
    }
}

public class CalloutTests
{
    [Fact]
    public void Defaults_to_low_contrast_info_without_a_close_button()
    {
        var c = new CdsCallout();
        Assert.True(c.IsLowContrast);
        Assert.Equal(CdsNotificationKind.Info, c.Kind);
        Assert.Equal(288, c.MinimumWidthRequest);
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var c = new CdsCallout(CdsNotificationKind.Warning, "3 persistent volumes are deleted too", "Snapshot volumes you want to keep first.")
        {
            ActionButtonLabel = "Snapshot",
            ActionCommand = new Command(() => { }),
            IsLowContrast = false,
            Body = new Label { Text = "How to snapshot volumes" },
        };
        foreach (var kind in Enum.GetValues<CdsNotificationKind>()) c.Kind = kind;
        c.Title = null;
    }
}
