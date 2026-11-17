namespace Maui.Carbon.Tests.Components;

public class ToastNotificationTests
{
    [Fact]
    public void Defaults_follow_the_readme()
    {
        var t = new CdsToastNotification();
        Assert.Equal(CdsNotificationKind.Error, t.Kind);
        Assert.Equal(0, t.Timeout);
        Assert.Equal(CdsNotificationRole.Status, t.Role);
        Assert.Equal(288, CdsToastNotification.ToastWidth);
        Assert.Equal(288, t.WidthRequest);
    }

    [Fact]
    public void Timeout_is_never_negative()
    {
        var t = new CdsToastNotification { Timeout = -5 };
        Assert.Equal(0, t.Timeout);
        t.Timeout = 5000;
        Assert.Equal(5000, t.Timeout);
    }

    [Fact]
    public void Create_copies_the_options()
    {
        var requested = 0;
        var toast = CdsToast.Create(new CdsToastOptions
        {
            Kind = CdsNotificationKind.Success,
            Title = "Deployment complete",
            Subtitle = "checkout-api v2.4.1 is live in prod-us-east.",
            Caption = "2 minutes ago",
            Timeout = 5000,
            IsLowContrast = false,
            HideCloseButton = true,
            Role = CdsNotificationRole.Alert,
            StatusIconDescription = "Success",
            CloseButtonDescription = "Dismiss toast",
            CloseRequested = _ => requested++,
        });

        Assert.Equal(CdsNotificationKind.Success, toast.Kind);
        Assert.Equal("Deployment complete", toast.Title);
        Assert.Equal("checkout-api v2.4.1 is live in prod-us-east.", toast.Subtitle);
        Assert.Equal("2 minutes ago", toast.Caption);
        Assert.Equal(5000, toast.Timeout);
        Assert.False(toast.IsLowContrast);
        Assert.True(toast.HideCloseButton);
        Assert.Equal(CdsNotificationRole.Alert, toast.Role);
        Assert.Equal("Dismiss toast", toast.CloseButtonDescription);

        toast.Close();
        Assert.Equal(1, requested);
    }

    [Fact]
    public void Options_default_to_a_low_contrast_polite_info_toast()
    {
        var options = new CdsToastOptions();
        Assert.Equal(CdsNotificationKind.Info, options.Kind);
        Assert.True(options.IsLowContrast);
        Assert.Equal(0, options.Timeout);
        Assert.Equal(CdsNotificationRole.Status, options.Role);
    }

    [Fact]
    public void Show_without_a_page_host_throws_a_clear_error()
    {
        Assert.Throws<InvalidOperationException>(() => CdsToast.Show(new Label(), new CdsToastOptions { Title = "Saved" }));
    }
}
