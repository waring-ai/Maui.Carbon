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
        Assert.Equal(608, n.MaxContentWidth);
        Assert.Equal(-1, n.WidthRequest);
        n.IsInline = false;
        Assert.Equal(288, n.WidthRequest);
        Assert.Equal(288, n.MaximumWidthRequest);
        Assert.True(double.IsPositiveInfinity(n.MaxContentWidth));
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

public class ActionableNotificationLayoutTests
{
    static readonly Size Icon = new(20, 34);
    static readonly Size Text = new(300, 48);
    static readonly Size Action = new(110, 48);

    [Fact]
    public void Single_row_keeps_the_action_inside_the_box_before_the_close_slot()
    {
        // 608 box, 3px edge, padding 13 start and 48 end: the inner width ends where the close button's slot begins.
        var inner = new Rect(13, 0, 605 - 13 - 48, 0);
        var (icon, text, action) = ActionableLayoutModel.Arrange(inner, Icon, Text, Action, hasAction: true, singleRow: true);
        Assert.Equal(inner.Right, action.Right);
        Assert.True(text.Right + ActionableLayoutModel.ActionGap <= action.X + 0.001);
        Assert.Equal(13 + 20 + 16, text.X);
        Assert.Equal(new Rect(13, 0, 20, 34), icon);
        Assert.Equal(48, ActionableLayoutModel.Height(Icon, Text, Action, true, singleRow: true));
    }

    [Fact]
    public void Stacked_puts_the_action_on_its_own_row_and_leaves_no_band_on_one_row()
    {
        var inner = new Rect(13, 0, 240, 0);
        var (_, text, action) = ActionableLayoutModel.Arrange(inner, Icon, Text, Action, hasAction: true, singleRow: false);
        Assert.Equal(240 - 20 - 16, text.Width);
        Assert.Equal(48, action.Y);
        Assert.Equal(13, action.X);
        Assert.Equal(96, ActionableLayoutModel.Height(Icon, Text, Action, true, singleRow: false));
        Assert.Equal(48, ActionableLayoutModel.Height(Icon, Text, Action, false, singleRow: false));
    }

    [Fact]
    public void Text_width_leaves_room_for_the_action_only_on_one_row()
    {
        Assert.Equal(500 - 20 - 16 - 13 - 110, ActionableLayoutModel.TextWidth(500, 20, 110, hasAction: true, singleRow: true));
        Assert.Equal(500 - 20 - 16, ActionableLayoutModel.TextWidth(500, 20, 110, hasAction: true, singleRow: false));
        Assert.Equal(500 - 20 - 16, ActionableLayoutModel.TextWidth(500, 20, 110, hasAction: false, singleRow: true));
        Assert.Equal(0, ActionableLayoutModel.TextWidth(100, 20, 110, hasAction: true, singleRow: true));
        Assert.True(double.IsPositiveInfinity(ActionableLayoutModel.TextWidth(double.PositiveInfinity, 20, 110, true, true)));
    }

    [Theory]
    [InlineData(1200, 400, true)]
    [InlineData(672, 400, true)]
    [InlineData(671, 671, false)]
    [InlineData(double.NaN, 608, true)]
    [InlineData(double.NaN, 500, false)]
    [InlineData(double.NaN, double.PositiveInfinity, true)]
    public void One_row_from_the_md_breakpoint(double viewport, double width, bool single) =>
        Assert.Equal(single, NotificationWidthModel.IsSingleRow(viewport, width));

    static CdsActionableNotification Measured(double viewport, bool inline)
    {
        var n = new CdsActionableNotification(CdsNotificationKind.Warning, "Payment method expires soon", "The card ending 4242 expires on 30 Nov 2026.", "Update card")
        {
            IsInline = inline,
        };
        var scope = new ContentView { Content = n };
        scope.Resources[CdsContextKeys.ViewportWidth] = viewport;
        // Headless views have no handler, so measure the layout's own manager.
        ((Microsoft.Maui.ILayout)n.ContentLayout).CrossPlatformMeasure(1000, double.PositiveInfinity);
        return n;
    }

    [Fact]
    public void The_row_decision_follows_the_viewport_when_measured()
    {
        Assert.True(Measured(1200, inline: true).IsSingleRow);
        Assert.False(Measured(600, inline: true).IsSingleRow);
        Assert.False(Measured(1200, inline: false).IsSingleRow);
    }

    [Theory]
    [InlineData(500, 288)]
    [InlineData(672, 608)]
    [InlineData(1056, 736)]
    [InlineData(1584, 832)]
    public void Inline_width_follows_the_viewport(double viewport, double cap)
    {
        Assert.Equal(cap, NotificationWidthModel.Inline(viewport));
        Assert.Equal(cap, Measured(viewport, inline: true).MaxContentWidth);
        var callout = new CdsCallout();
        var scope = new ContentView { Content = callout };
        scope.Resources[CdsContextKeys.ViewportWidth] = viewport;
        Assert.Equal(cap, callout.MaxContentWidth);
    }

    [Fact]
    public void Without_a_page_host_the_medium_width_applies()
    {
        Assert.Equal(608, NotificationWidthModel.Inline(double.NaN));
        Assert.Equal(608, new CdsCallout().MaxContentWidth);
        Assert.Equal(288, new CdsActionableNotification { IsInline = false }.WidthRequest);
    }

    [Fact]
    public void A_viewport_change_updates_the_cap()
    {
        var callout = new CdsCallout();
        var scope = new ContentView { Content = callout };
        scope.Resources[CdsContextKeys.ViewportWidth] = 1600d;
        Assert.Equal(832, callout.MaxContentWidth);
        scope.Resources[CdsContextKeys.ViewportWidth] = 700d;
        Assert.Equal(608, callout.MaxContentWidth);
    }
}

public class ActionableNotificationFocusTests
{
    [Theory]
    // Tab from inside wraps to the other item, or stays on the only one.
    [InlineData(false, "Close", true, true, "Action")]
    [InlineData(true, "Action", true, true, "Close")]
    [InlineData(false, "Action", true, false, "Action")]
    [InlineData(true, "Close", false, true, "Close")]
    // From outside it enters at the nearest item.
    [InlineData(true, "None", true, true, "Action")]
    [InlineData(false, "None", true, true, "Close")]
    [InlineData(true, "None", false, true, "Close")]
    [InlineData(false, "None", true, false, "Action")]
    [InlineData(true, "None", false, false, "None")]
    public void Sentinels_wrap_focus_between_the_action_and_the_close_button(
        bool start, string from, bool hasAction, bool hasClose, string expected) =>
        Assert.Equal(
            Enum.Parse<NotificationFocusItem>(expected),
            NotificationFocusModel.SentinelTarget(start, Enum.Parse<NotificationFocusItem>(from), hasAction, hasClose));

    [Fact]
    public void Only_the_alert_dialog_role_wraps_and_takes_focus()
    {
        var n = new CdsActionableNotification(CdsNotificationKind.Error, "Backup failed", "Retry in a few minutes.", "View logs");
        Assert.True(n.WrapsFocus);
        Assert.True(n.TakesFocusWhenShown);
        Assert.True(n.StartSentinel.Parent is VisualElement { IsVisible: true });

        n.HasFocus = false;
        Assert.False(n.TakesFocusWhenShown);
        Assert.True(n.WrapsFocus);

        n.Role = CdsNotificationRole.Status;
        Assert.False(n.WrapsFocus);
        Assert.False(n.EndSentinel.Parent is VisualElement { IsVisible: true });

        var noAction = new CdsActionableNotification { Title = "Backup failed" };
        Assert.False(noAction.TakesFocusWhenShown);
        noAction.HideCloseButton = true;
        Assert.False(noAction.WrapsFocus);
    }

    [Fact]
    public void A_sentinel_reached_from_the_action_sends_focus_to_the_close_button()
    {
        var n = new CdsActionableNotification(CdsNotificationKind.Error, "Backup failed", null, "View logs");
        Assert.Same(n.ActionButton, n.SentinelTarget(start: true));
        Assert.Same(n.CloseButton, n.SentinelTarget(start: false));

        var hit = (CdsHitTarget)n.ActionButton.AccessibilityTarget!;
        hit.ReportFocus(true, fromKeyboard: true);
        hit.ReportFocus(false, fromKeyboard: true);
        Assert.Same(n.CloseButton, n.SentinelTarget(start: true));
    }
}
