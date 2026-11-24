namespace Maui.Carbon.Tests.Components;

public class NotificationTokensTests
{
    [Theory]
    [InlineData(CdsNotificationKind.Error, CdsColorKeys.SupportErrorInverse, CdsColorKeys.SupportError, CdsColorKeys.NotificationBackgroundError)]
    [InlineData(CdsNotificationKind.Success, CdsColorKeys.SupportSuccessInverse, CdsColorKeys.SupportSuccess, CdsColorKeys.NotificationBackgroundSuccess)]
    [InlineData(CdsNotificationKind.Warning, CdsColorKeys.SupportWarningInverse, CdsColorKeys.SupportWarning, CdsColorKeys.NotificationBackgroundWarning)]
    [InlineData(CdsNotificationKind.WarningAlt, CdsColorKeys.SupportWarningInverse, CdsColorKeys.SupportWarning, CdsColorKeys.NotificationBackgroundWarning)]
    [InlineData(CdsNotificationKind.Info, CdsColorKeys.SupportInfoInverse, CdsColorKeys.SupportInfo, CdsColorKeys.NotificationBackgroundInfo)]
    [InlineData(CdsNotificationKind.InfoSquare, CdsColorKeys.SupportInfoInverse, CdsColorKeys.SupportInfo, CdsColorKeys.NotificationBackgroundInfo)]
    public void Kind_maps_to_accent_and_background(CdsNotificationKind kind, string inverse, string low, string background)
    {
        var high = CdsNotificationTokens.Resolve(kind, lowContrast: false);
        Assert.Equal(CdsColorKeys.BackgroundInverse, high.Background);
        Assert.Equal(inverse, high.Accent);
        Assert.Null(high.Outline);
        Assert.Equal(CdsColorKeys.TextInverse, high.Text);
        Assert.Equal(CdsColorKeys.IconInverse, high.CloseIcon);
        Assert.Equal(CdsColorKeys.FocusInverse, high.Focus);

        var lowContrast = CdsNotificationTokens.Resolve(kind, lowContrast: true);
        Assert.Equal(background, lowContrast.Background);
        Assert.Equal(low, lowContrast.Accent);
        Assert.Equal(low, lowContrast.Outline);
        Assert.Equal(CdsColorKeys.TextPrimary, lowContrast.Text);
        Assert.Equal(CdsColorKeys.IconPrimary, lowContrast.CloseIcon);
        Assert.Equal(CdsColorKeys.Focus, lowContrast.Focus);
    }

    [Theory]
    [InlineData(CdsNotificationKind.Error, CdsIconName.ErrorFilled)]
    [InlineData(CdsNotificationKind.Info, CdsIconName.InformationFilled)]
    [InlineData(CdsNotificationKind.InfoSquare, CdsIconName.InformationSquareFilled)]
    [InlineData(CdsNotificationKind.Success, CdsIconName.CheckmarkFilled)]
    [InlineData(CdsNotificationKind.Warning, CdsIconName.WarningFilled)]
    [InlineData(CdsNotificationKind.WarningAlt, CdsIconName.WarningAltFilled)]
    public void Kind_maps_to_status_icon(CdsNotificationKind kind, CdsIconName icon)
    {
        Assert.Equal(icon, CdsNotificationTokens.Icon(kind));
        Assert.EndsWith("icon", CdsNotificationTokens.DefaultIconDescription(kind));
    }

    [Fact]
    public void Ghost_action_hover_differs_by_contrast()
    {
        var high = CdsNotificationTokens.ResolveAction(false, false, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.BackgroundInverseHover, high.Background);
        Assert.Equal(CdsColorKeys.LinkInverse, high.Text);

        var low = CdsNotificationTokens.ResolveAction(true, false, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.NotificationActionHover, low.Background);
        Assert.Equal(CdsColorKeys.LinkPrimary, low.Text);

        Assert.Null(CdsNotificationTokens.ResolveAction(true, false, CdsInteractionState.None).Background);
        Assert.Equal(CdsColorKeys.TextDisabled, CdsNotificationTokens.ResolveAction(true, false, CdsInteractionState.Disabled).Text);
    }

    [Fact]
    public void Tertiary_action_uses_inverse_tokens_on_high_contrast()
    {
        var rest = CdsNotificationTokens.ResolveAction(false, true, CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.NotificationActionTertiaryInverse, rest.Border);
        Assert.Equal(CdsColorKeys.NotificationActionTertiaryInverse, rest.Text);
        Assert.Null(rest.Background);

        var hover = CdsNotificationTokens.ResolveAction(false, true, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.NotificationActionTertiaryInverseHover, hover.Background);
        Assert.Equal(CdsColorKeys.NotificationActionTertiaryInverseText, hover.Text);

        var pressed = CdsNotificationTokens.ResolveAction(false, true, CdsInteractionState.Hover | CdsInteractionState.Pressed);
        Assert.Equal(CdsColorKeys.NotificationActionTertiaryInverseActive, pressed.Background);

        var disabled = CdsNotificationTokens.ResolveAction(false, true, CdsInteractionState.Disabled);
        Assert.Equal(CdsColorKeys.NotificationActionTertiaryInverseTextOnColorDisabled, disabled.Text);
    }

    [Fact]
    public void Tertiary_action_uses_the_tertiary_button_tokens_on_low_contrast()
    {
        var rest = CdsNotificationTokens.ResolveAction(true, true, CdsInteractionState.None);
        Assert.Equal(CdsColorKeys.ButtonTertiary, rest.Border);
        var hover = CdsNotificationTokens.ResolveAction(true, true, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.ButtonTertiaryHover, hover.Background);
        Assert.Equal(CdsColorKeys.TextInverse, hover.Text);
    }
}

public class InlineNotificationTests
{
    [Fact]
    public void Defaults_follow_the_readme()
    {
        var n = new CdsInlineNotification();
        Assert.Equal(CdsNotificationKind.Info, n.Kind);
        Assert.False(n.IsLowContrast);
        Assert.False(n.HideCloseButton);
        Assert.Equal(CdsNotificationRole.Status, n.Role);
        Assert.Equal("Close notification", n.CloseButtonDescription);
        Assert.True(n.CloseOnEscape);
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var n = new CdsInlineNotification(CdsNotificationKind.Error, "Deployment failed", "Image checkout-api:2.4 was not found.")
        {
            IsLowContrast = true,
            HideCloseButton = true,
            StatusIconDescription = "Failure",
            CloseButtonDescription = "Dismiss",
            Role = CdsNotificationRole.Alert,
            Body = new Label { Text = "Open the registry" },
        };
        foreach (var kind in Enum.GetValues<CdsNotificationKind>()) n.Kind = kind;
        n.Title = null;
        n.Subtitle = null;
        n.Body = null;
    }

    [Fact]
    public void Close_raises_events_and_hides()
    {
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Saved");
        var requested = 0;
        var closed = 0;
        n.CloseRequested += (_, _) => requested++;
        n.Closed += (_, _) => closed++;

        Assert.True(n.Close());
        Assert.Equal(1, requested);
        Assert.Equal(1, closed);
        Assert.False(n.IsVisible);
    }

    [Fact]
    public void Close_can_be_canceled()
    {
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Saved");
        var closed = false;
        n.CloseRequested += (_, e) => e.Cancel = true;
        n.Closed += (_, _) => closed = true;

        Assert.False(n.Close());
        Assert.False(closed);
        Assert.True(n.IsVisible);
    }

    [Fact]
    public void Escape_closes_when_enabled()
    {
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Saved");
        Assert.False(n.OnKeyDown(new CdsKeyEventArgs(CdsKey.Enter)));
        Assert.True(n.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.False(n.IsVisible);

        var kept = new CdsInlineNotification { CloseOnEscape = false };
        Assert.False(kept.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.True(kept.IsVisible);
    }
}

/// <summary>Puts views on a page in a window, so they are loaded (Loaded fires without a platform).</summary>
internal static class TestWindow
{
    public static Window Show(params View[] views)
    {
        var stack = new VerticalStackLayout();
        foreach (var view in views) stack.Children.Add(view);
        return new Window(new ContentPage { Content = stack });
    }
}

public class NotificationAnnouncementTests
{
    [Fact]
    public void A_notification_shown_when_loaded_is_announced_once_politely()
    {
        using var heard = CdsAnnouncer.Capture();
        var n = new CdsInlineNotification(CdsNotificationKind.Error, "Deployment failed", "Image checkout-api:2.4 was not found.");
        _ = TestWindow.Show(n);
        Assert.True(n.IsLoaded);
        Assert.Equal([new CdsAnnouncement("Deployment failed. Image checkout-api:2.4 was not found", CdsAnnouncePriority.Polite)], heard.Announcements);
    }

    [Fact]
    public void A_hidden_notification_is_not_announced_at_load_but_when_it_is_shown()
    {
        using var heard = CdsAnnouncer.Capture();
        var n = new CdsInlineNotification(CdsNotificationKind.Error, "Pre-flight failed") { IsVisible = false };
        _ = TestWindow.Show(n);
        Assert.Empty(heard.Announcements);

        n.IsVisible = true;
        Assert.Equal("Pre-flight failed", Assert.Single(heard.Announcements).Text);

        n.IsVisible = false;
        n.IsVisible = true;
        Assert.Equal(2, heard.Announcements.Count);
    }

    [Fact]
    public void Showing_a_hidden_ancestor_announces_the_notification()
    {
        using var heard = CdsAnnouncer.Capture();
        var n = new CdsInlineNotification(CdsNotificationKind.Warning, "Quota nearly reached");
        var section = new ContentView { Content = new VerticalStackLayout { Children = { n } }, IsVisible = false };
        _ = TestWindow.Show(section);
        Assert.Empty(heard.Announcements);

        section.IsVisible = true;
        Assert.Equal("Quota nearly reached", Assert.Single(heard.Announcements).Text);
    }

    [Fact]
    public void Moving_into_a_hidden_container_is_tracked()
    {
        using var heard = CdsAnnouncer.Capture();
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Saved");
        var first = new VerticalStackLayout { Children = { n } };
        var hidden = new VerticalStackLayout { IsVisible = false };
        _ = TestWindow.Show(first, hidden);
        heard.Announcements.Clear();

        first.Children.Remove(n);
        hidden.Children.Add(n);
        Assert.Empty(heard.Announcements);
        hidden.IsVisible = true;
        Assert.Equal("Saved", Assert.Single(heard.Announcements).Text);
    }

    [Fact]
    public void Text_and_kind_changes_are_announced_only_while_shown()
    {
        using var heard = CdsAnnouncer.Capture();
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Checking") { IsVisible = false };
        _ = TestWindow.Show(n);
        n.Title = "Outcome";
        n.Subtitle = "Teardown failed.";
        Assert.Empty(heard.Announcements);

        n.IsVisible = true;
        heard.Announcements.Clear();
        n.Subtitle = "Deploy failed.";
        Assert.Equal("Outcome. Deploy failed", Assert.Single(heard.Announcements).Text);
        n.Kind = CdsNotificationKind.Error;
        Assert.Equal(2, heard.Announcements.Count);
    }

    [Fact]
    public void Nothing_is_announced_before_the_notification_is_loaded()
    {
        using var heard = CdsAnnouncer.Capture();
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Saved") { IsVisible = false };
        n.IsVisible = true;
        n.Title = "Changed";
        Assert.Empty(heard.Announcements);
    }

    [Theory]
    [InlineData(CdsNotificationRole.Status, CdsAnnouncePriority.Polite)]
    [InlineData(CdsNotificationRole.Log, CdsAnnouncePriority.Polite)]
    [InlineData(CdsNotificationRole.Alert, CdsAnnouncePriority.Assertive)]
    [InlineData(CdsNotificationRole.AlertDialog, CdsAnnouncePriority.Assertive)]
    public void The_role_picks_the_priority(CdsNotificationRole role, CdsAnnouncePriority priority)
    {
        Assert.Equal(priority, CdsNotificationBase.AnnouncementPriority(role));
        using var heard = CdsAnnouncer.Capture();
        _ = TestWindow.Show(new CdsInlineNotification(CdsNotificationKind.Error, "Backup failed") { Role = role });
        Assert.Equal(priority, Assert.Single(heard.Announcements).Priority);
    }

    [Fact]
    public void Role_none_and_callouts_are_not_announced()
    {
        Assert.Null(CdsNotificationBase.AnnouncementPriority(CdsNotificationRole.None));
        using var heard = CdsAnnouncer.Capture();
        _ = TestWindow.Show(
            new CdsInlineNotification(CdsNotificationKind.Info, "Quiet") { Role = CdsNotificationRole.None },
            new CdsCallout(CdsNotificationKind.Warning, "3 persistent volumes are deleted too", "Snapshot them first."));
        Assert.Empty(heard.Announcements);
    }

    [Fact]
    public void A_closed_notification_is_announced_again_when_shown_again()
    {
        using var heard = CdsAnnouncer.Capture();
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Saved");
        _ = TestWindow.Show(n);
        n.Close();
        n.Title = "Saved again";
        Assert.Single(heard.Announcements);
        n.IsVisible = true;
        Assert.Equal("Saved again", heard.Announcements[^1].Text);
    }
}

public class NotificationEscapeTests
{
    [Fact]
    public void A_dismissible_notification_asks_for_Escape()
    {
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Saved");
        Assert.True(n.WantsEscapeKey);
        n.CloseOnEscape = false;
        Assert.False(n.WantsEscapeKey);
        n.CloseOnEscape = true;
        n.HideCloseButton = true;
        Assert.False(n.WantsEscapeKey);
        n.HideCloseButton = false;
        n.IsVisible = false;
        Assert.False(n.WantsEscapeKey);
        Assert.True(new CdsToastNotification().WantsEscapeKey);
        Assert.True(new CdsActionableNotification().WantsEscapeKey);
    }

    [Fact]
    public void Escape_does_not_close_a_notification_without_a_close_button()
    {
        var n = new CdsInlineNotification(CdsNotificationKind.Error, "Read this") { HideCloseButton = true };
        Assert.False(n.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.True(n.IsVisible);
    }
}

[Collection(FocusTrackerCollection.Name)]
public class NotificationFocusAfterCloseTests
{
    static CdsHitTarget Hit(CdsControl control) => (CdsHitTarget)control.AccessibilityTarget!;

    [Fact]
    public void Focus_moves_to_the_next_focusable_element_after_the_notification()
    {
        var before = new CdsButton { Text = "Back" };
        var n = new CdsInlineNotification(CdsNotificationKind.Error, "Deployment failed");
        var after = new CdsButton { Text = "Retry" };
        _ = TestWindow.Show(before, new Label { Text = "Not focusable" }, n, new Label(), after);
        Assert.Same(after, n.FindFocusAfterClose());

        after.IsEnabled = false;
        Assert.Same(before, n.FindFocusAfterClose());
        before.IsVisible = false;
        Assert.Null(n.FindFocusAfterClose());
    }

    [Fact]
    public void Focus_after_close_and_the_toast_fallback_come_first_when_they_can_take_focus()
    {
        var chosen = new CdsButton { Text = "Open logs" };
        var fallback = new CdsButton { Text = "Deploy" };
        var after = new CdsButton { Text = "Retry" };
        var n = new CdsInlineNotification(CdsNotificationKind.Error, "Deployment failed") { FocusAfterClose = chosen };
        _ = TestWindow.Show(chosen, fallback, n, after);
        Assert.Same(chosen, n.FindFocusAfterClose());

        chosen.IsVisible = false;
        Assert.Same(after, n.FindFocusAfterClose());
        n.FocusFallback = fallback;
        Assert.Same(fallback, n.FindFocusAfterClose());
    }

    [Fact]
    public void Focus_inside_the_notification_is_detected()
    {
        var n = new CdsInlineNotification(CdsNotificationKind.Info, "Saved");
        _ = TestWindow.Show(n);
        Assert.False(n.HoldsFocus);
        var hit = Hit(n.CloseButton);
        hit.ReportFocus(true, fromKeyboard: true);
        try
        {
            Assert.True(n.HoldsFocus);
            Assert.True(n.Close());
            Assert.False(n.IsVisible);
        }
        finally
        {
            hit.ReportFocus(false, fromKeyboard: true);
        }
        Assert.False(n.HoldsFocus);
    }
}
