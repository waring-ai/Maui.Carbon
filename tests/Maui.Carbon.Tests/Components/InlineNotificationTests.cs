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
