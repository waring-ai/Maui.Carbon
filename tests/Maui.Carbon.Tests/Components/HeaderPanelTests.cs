namespace Maui.Carbon.Tests.Components;

public class HeaderPanelTests
{
    [Fact]
    public void Panel_is_closed_and_zero_wide_by_default()
    {
        var panel = new CdsHeaderPanel();
        Assert.False(panel.IsExpanded);
        Assert.Equal(0, panel.CurrentWidth);
    }

    [Fact]
    public void Expanding_sets_the_256_width_and_closing_returns_to_zero()
    {
        var panel = new CdsHeaderPanel { IsExpanded = true };
        Assert.Equal(256, panel.CurrentWidth);
        panel.IsExpanded = false;
        Assert.Equal(0, panel.CurrentWidth);
    }

    [Fact]
    public void Outside_tap_closes_only_a_switcher_panel_by_default()
    {
        var withSwitcher = new CdsHeaderPanel { Body = new CdsSwitcher() };
        Assert.True(withSwitcher.ClosesOnOutsideTapEffective);
        var custom = new CdsHeaderPanel { Body = new Label { Text = "Notifications" } };
        Assert.False(custom.ClosesOnOutsideTapEffective);
        custom.ClosesOnOutsideTap = true;
        Assert.True(custom.ClosesOnOutsideTapEffective);
        withSwitcher.ClosesOnOutsideTap = false;
        Assert.False(withSwitcher.ClosesOnOutsideTapEffective);
    }

    [Fact]
    public void Switcher_items_follow_the_panel_state()
    {
        var item = new CdsSwitcherItem("Storage");
        var switcher = new CdsSwitcher { Items = { item } };
        var panel = new CdsHeaderPanel { Body = switcher };
        Assert.False(switcher.IsExpanded);
        panel.IsExpanded = true;
        Assert.True(switcher.IsExpanded);
        panel.IsExpanded = false;
        Assert.False(switcher.IsExpanded);
    }

    [Fact]
    public void Escape_closes_an_open_panel_and_returns_focus_target()
    {
        var panel = new CdsHeaderPanel { IsExpanded = true };
        Assert.True(panel.WantsEscapeKey);
        var handled = panel.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape));
        Assert.True(handled);
        Assert.False(panel.IsExpanded);
        Assert.False(panel.WantsEscapeKey);
    }

    [Fact]
    public void Escape_is_ignored_when_closing_on_escape_is_off()
    {
        var panel = new CdsHeaderPanel { IsExpanded = true, ClosesOnEscape = false };
        Assert.False(panel.WantsEscapeKey);
        Assert.False(panel.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.True(panel.IsExpanded);
    }

    [Fact]
    public void Other_keys_are_not_handled()
    {
        var panel = new CdsHeaderPanel { IsExpanded = true };
        Assert.False(panel.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
    }

    [Fact]
    public void Panel_geometry_matches_the_spec()
    {
        Assert.Equal(256, UIShellModel.HeaderPanelWidth);
        Assert.Equal(48, UIShellModel.HeaderHeight);
    }

    [Fact]
    public void Shell_tracks_the_panel_for_outside_taps()
    {
        var panel = new CdsHeaderPanel { Body = new CdsSwitcher() };
        var shell = new CdsUIShell { HeaderPanel = panel };
        panel.IsExpanded = true;
        Assert.Same(panel, shell.HeaderPanel);
        panel.IsExpanded = false;
    }
}

/// <summary>Uses the process-wide focus tracker, so it runs on its own.</summary>
[Collection(FocusTrackerCollection.Name)]
public class HeaderPanelFocusTests
{
    static CdsHitTarget Hit(CdsControl control) => (CdsHitTarget)control.AccessibilityTarget!;

    static (CdsHeaderPanel Panel, CdsSwitcherItem Inside) Open(CdsHeaderGlobalAction? opener = null)
    {
        var inside = new CdsSwitcherItem("Storage");
        var panel = new CdsHeaderPanel { Body = new CdsSwitcher { Items = { inside, new CdsSwitcherItem("Compute") } }, ReturnFocusTo = opener, IsExpanded = true };
        return (panel, inside);
    }

    [Fact]
    public void Blur_closes_only_when_focus_moved_somewhere_outside_the_panel_and_its_action()
    {
        Assert.True(UIShellModel.HeaderPanelClosesOnBlur(true, true, focusInside: false, focusOnOpener: false, focusElsewhere: true));
        Assert.False(UIShellModel.HeaderPanelClosesOnBlur(true, true, focusInside: true, focusOnOpener: false, focusElsewhere: true));
        Assert.False(UIShellModel.HeaderPanelClosesOnBlur(true, true, focusInside: false, focusOnOpener: true, focusElsewhere: true));
        Assert.False(UIShellModel.HeaderPanelClosesOnBlur(true, true, focusInside: false, focusOnOpener: false, focusElsewhere: false));
        Assert.False(UIShellModel.HeaderPanelClosesOnBlur(false, true, focusInside: false, focusOnOpener: false, focusElsewhere: true));
        Assert.False(UIShellModel.HeaderPanelClosesOnBlur(true, false, focusInside: false, focusOnOpener: false, focusElsewhere: true));
    }

    [Fact]
    public void Tabbing_out_of_the_panel_closes_it()
    {
        var (panel, inside) = Open();
        var outside = new CdsButton { Text = "Deploy" };
        Assert.True(panel.ClosesOnBlur);
        Hit(inside).ReportFocus(true, fromKeyboard: true);
        Hit(outside).ReportFocus(true, fromKeyboard: true);
        Hit(inside).ReportFocus(false, fromKeyboard: true);
        Assert.False(panel.IsExpanded);
        Hit(outside).ReportFocus(false, fromKeyboard: true);
    }

    [Fact]
    public void Moving_focus_inside_the_panel_or_back_to_its_action_keeps_it_open()
    {
        var action = new CdsHeaderGlobalAction(CdsIconName.Switcher, "Switch product") { ControlsPanel = true, IsActive = true };
        var (panel, inside) = Open(action);
        var next = (CdsSwitcherItem)((CdsSwitcher)panel.Body!).Items[1];
        Hit(inside).ReportFocus(true, fromKeyboard: true);
        Hit(next).ReportFocus(true, fromKeyboard: true);
        Hit(inside).ReportFocus(false, fromKeyboard: true);
        Assert.True(panel.IsExpanded);
        Hit(action).ReportFocus(true, fromKeyboard: true);
        Hit(next).ReportFocus(false, fromKeyboard: true);
        Assert.True(panel.IsExpanded);
        Hit(action).ReportFocus(false, fromKeyboard: true);
    }

    [Fact]
    public void Blur_closing_can_be_turned_off()
    {
        var (panel, inside) = Open();
        panel.ClosesOnBlur = false;
        var outside = new CdsButton { Text = "Deploy" };
        Hit(inside).ReportFocus(true, fromKeyboard: true);
        Hit(outside).ReportFocus(true, fromKeyboard: true);
        Hit(inside).ReportFocus(false, fromKeyboard: true);
        Assert.True(panel.IsExpanded);
        Hit(outside).ReportFocus(false, fromKeyboard: true);
    }

    [Fact]
    public void Focus_that_leaves_for_nowhere_keeps_the_panel_open()
    {
        var (panel, inside) = Open();
        Hit(inside).ReportFocus(true, fromKeyboard: true);
        Hit(inside).ReportFocus(false, fromKeyboard: true);
        Assert.True(panel.IsExpanded);
    }
}
