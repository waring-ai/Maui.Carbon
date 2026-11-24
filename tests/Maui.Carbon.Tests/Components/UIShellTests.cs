namespace Maui.Carbon.Tests.Components;

public class UIShellModelTests
{
    [Theory]
    [InlineData(CdsBreakpoint.Sm, false)]
    [InlineData(CdsBreakpoint.Md, false)]
    [InlineData(CdsBreakpoint.Lg, true)]
    [InlineData(CdsBreakpoint.Xlg, true)]
    [InlineData(CdsBreakpoint.Max, true)]
    public void Large_starts_at_the_lg_breakpoint(CdsBreakpoint breakpoint, bool large) =>
        Assert.Equal(large, UIShellModel.IsLarge(breakpoint));

    [Theory]
    [InlineData(CdsSideNavMode.Default, CdsBreakpoint.Lg, false, true, false, 256)]
    [InlineData(CdsSideNavMode.Default, CdsBreakpoint.Md, false, true, false, 0)]
    [InlineData(CdsSideNavMode.Default, CdsBreakpoint.Md, true, true, false, 256)]
    [InlineData(CdsSideNavMode.Default, CdsBreakpoint.Lg, false, false, false, 0)]
    [InlineData(CdsSideNavMode.Default, CdsBreakpoint.Lg, true, false, false, 256)]
    [InlineData(CdsSideNavMode.Rail, CdsBreakpoint.Lg, false, true, false, 48)]
    [InlineData(CdsSideNavMode.Rail, CdsBreakpoint.Sm, false, true, false, 48)]
    [InlineData(CdsSideNavMode.Rail, CdsBreakpoint.Lg, false, true, true, 256)]
    [InlineData(CdsSideNavMode.Rail, CdsBreakpoint.Lg, true, true, false, 256)]
    [InlineData(CdsSideNavMode.Fixed, CdsBreakpoint.Sm, false, true, false, 256)]
    public void Side_nav_width_by_mode_breakpoint_and_state(CdsSideNavMode mode, CdsBreakpoint bp, bool expanded, bool persistent, bool transient, double expected) =>
        Assert.Equal(expected, UIShellModel.SideNavTargetWidth(mode, bp, expanded, persistent, transient));

    [Fact]
    public void Body_offset_follows_the_side_nav_from_lg_and_a_rail_always_reserves_48()
    {
        Assert.Equal(256, UIShellModel.BodyOffset(CdsSideNavMode.Default, CdsBreakpoint.Lg, 256));
        Assert.Equal(128, UIShellModel.BodyOffset(CdsSideNavMode.Default, CdsBreakpoint.Xlg, 128));
        Assert.Equal(0, UIShellModel.BodyOffset(CdsSideNavMode.Default, CdsBreakpoint.Md, 256));
        Assert.Equal(48, UIShellModel.BodyOffset(CdsSideNavMode.Rail, CdsBreakpoint.Md, 256));
        Assert.Equal(48, UIShellModel.BodyOffset(CdsSideNavMode.Rail, CdsBreakpoint.Lg, 48));
        Assert.Equal(256, UIShellModel.BodyOffset(CdsSideNavMode.Fixed, CdsBreakpoint.Lg, 256));
    }

    [Fact]
    public void Scrim_shows_only_for_an_expanded_default_side_nav_below_lg()
    {
        Assert.True(UIShellModel.ShowScrim(CdsSideNavMode.Default, CdsBreakpoint.Md, true));
        Assert.False(UIShellModel.ShowScrim(CdsSideNavMode.Default, CdsBreakpoint.Md, false));
        Assert.False(UIShellModel.ShowScrim(CdsSideNavMode.Default, CdsBreakpoint.Lg, true));
        Assert.False(UIShellModel.ShowScrim(CdsSideNavMode.Rail, CdsBreakpoint.Sm, true));
        Assert.False(UIShellModel.ShowScrim(CdsSideNavMode.Fixed, CdsBreakpoint.Sm, true));
    }

    [Fact]
    public void Menu_button_shows_below_lg_and_from_lg_only_when_collapsible()
    {
        Assert.True(UIShellModel.MenuButtonVisible(CdsBreakpoint.Md, false));
        Assert.False(UIShellModel.MenuButtonVisible(CdsBreakpoint.Lg, false));
        Assert.True(UIShellModel.MenuButtonVisible(CdsBreakpoint.Lg, true));
        Assert.False(UIShellModel.HeaderNavigationVisible(CdsBreakpoint.Md));
        Assert.True(UIShellModel.HeaderNavigationVisible(CdsBreakpoint.Lg));
    }

    [Fact]
    public void Collapsed_side_nav_is_unreachable() => Assert.False(UIShellModel.SideNavReachable(0));

    [Fact]
    public void Rail_and_expanded_side_nav_are_reachable()
    {
        Assert.True(UIShellModel.SideNavReachable(48));
        Assert.True(UIShellModel.SideNavReachable(256));
    }

    [Fact]
    public void Next_enabled_wraps_and_skips_disabled_rows()
    {
        bool[] enabled = [true, false, true, true];
        Assert.Equal(2, UIShellModel.NextEnabled(enabled, 0, +1));
        Assert.Equal(0, UIShellModel.NextEnabled(enabled, 3, +1));
        Assert.Equal(3, UIShellModel.NextEnabled(enabled, 0, -1));
        Assert.Equal(0, UIShellModel.NextEnabled(enabled, 2, -1));
    }

    [Fact]
    public void Next_enabled_from_nothing_starts_at_the_edge()
    {
        bool[] enabled = [false, true, true];
        Assert.Equal(1, UIShellModel.NextEnabled(enabled, -1, +1));
        Assert.Equal(2, UIShellModel.NextEnabled(enabled, -1, -1));
    }

    [Fact]
    public void Next_enabled_is_minus_one_when_nothing_is_enabled()
    {
        Assert.Equal(-1, UIShellModel.NextEnabled([false, false], 0, +1));
        Assert.Equal(-1, UIShellModel.NextEnabled([], -1, +1));
    }

    [Fact]
    public void Home_and_end_jump_to_the_first_and_last_enabled_row()
    {
        bool[] enabled = [false, true, true, false];
        Assert.Equal(1, UIShellModel.Move(enabled, 2, CdsKey.Home));
        Assert.Equal(2, UIShellModel.Move(enabled, 1, CdsKey.End));
        Assert.Equal(2, UIShellModel.Move(enabled, 1, CdsKey.Down));
        Assert.Equal(1, UIShellModel.Move(enabled, 2, CdsKey.Up));
        Assert.Equal(-1, UIShellModel.Move(enabled, 1, CdsKey.Left));
    }

    [Fact]
    public void Header_panel_outside_tap_defaults_to_switcher_only()
    {
        Assert.True(UIShellModel.ClosesOnOutsideTap(null, true));
        Assert.False(UIShellModel.ClosesOnOutsideTap(null, false));
        Assert.False(UIShellModel.ClosesOnOutsideTap(false, true));
        Assert.True(UIShellModel.ClosesOnOutsideTap(true, false));
    }
}

public class UIShellTokenTests
{
    [Fact]
    public void Shell_surfaces_follow_the_theme()
    {
        Assert.Equal(CdsColorKeys.Background, CdsUIShellTokens.Surface);
        Assert.Equal(CdsColorKeys.BorderSubtle00, CdsUIShellTokens.Divider);
    }

    [Fact]
    public void Header_action_icons_go_from_secondary_to_primary()
    {
        Assert.Equal(CdsColorKeys.IconSecondary, CdsUIShellTokens.HeaderAction(CdsInteractionState.None, false).Icon);
        var hover = CdsUIShellTokens.HeaderAction(CdsInteractionState.Hover, false);
        Assert.Equal(CdsColorKeys.BackgroundHover, hover.Background);
        Assert.Equal(CdsColorKeys.IconPrimary, hover.Icon);
        Assert.Equal(CdsColorKeys.BackgroundActive, CdsUIShellTokens.HeaderAction(CdsInteractionState.Pressed, false).Background);
    }

    [Fact]
    public void Active_header_action_fills_layer_01_with_subtle_side_borders()
    {
        var active = CdsUIShellTokens.HeaderAction(CdsInteractionState.None, true);
        Assert.Equal(CdsColorKeys.Layer01, active.Background);
        Assert.Equal(CdsColorKeys.BorderSubtle00, active.SideBorder);
        Assert.Equal(CdsColorKeys.IconPrimary, active.Icon);
        Assert.Null(CdsUIShellTokens.HeaderAction(CdsInteractionState.None, false).SideBorder);
    }

    [Fact]
    public void Header_action_focus_is_a_focus_border()
    {
        Assert.Equal(CdsColorKeys.Focus, CdsUIShellTokens.HeaderActionBorder(CdsInteractionState.FocusVisible));
        Assert.Null(CdsUIShellTokens.HeaderActionBorder(CdsInteractionState.Hover));
    }

    [Fact]
    public void Header_link_is_secondary_and_the_current_one_is_primary_with_an_interactive_underline()
    {
        var rest = CdsUIShellTokens.HeaderMenuItem(CdsInteractionState.None, false);
        Assert.Equal(CdsColorKeys.TextSecondary, rest.Foreground);
        Assert.Null(rest.Bar);
        var current = CdsUIShellTokens.HeaderMenuItem(CdsInteractionState.None, true);
        Assert.Equal(CdsColorKeys.TextPrimary, current.Foreground);
        Assert.Equal(CdsColorKeys.BorderInteractive, current.Bar);
        Assert.Equal(CdsColorKeys.BackgroundHover, CdsUIShellTokens.HeaderMenuItem(CdsInteractionState.Hover, false).Background);
        Assert.Equal(CdsColorKeys.BackgroundActive, CdsUIShellTokens.HeaderMenuItem(CdsInteractionState.Pressed, false).Background);
    }

    [Fact]
    public void Header_menu_rows_use_layer_tokens()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsUIShellTokens.HeaderMenuRow(CdsInteractionState.None, false, false).Background);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsUIShellTokens.HeaderMenuRow(CdsInteractionState.Hover, false, false).Background);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsUIShellTokens.HeaderMenuRow(CdsInteractionState.None, false, true).Background);
        Assert.Equal(CdsColorKeys.LayerActive01, CdsUIShellTokens.HeaderMenuRow(CdsInteractionState.Pressed, false, false).Background);
        var current = CdsUIShellTokens.HeaderMenuRow(CdsInteractionState.None, true, false);
        Assert.Equal(CdsColorKeys.LayerSelected01, current.Background);
        Assert.Equal(CdsColorKeys.BorderInteractive, current.Bar);
        Assert.Equal(CdsColorKeys.LayerSelectedHover01, CdsUIShellTokens.HeaderMenuRow(CdsInteractionState.Hover, true, false).Background);
    }

    [Fact]
    public void Open_header_menu_trigger_fills_layer_01_and_closed_shows_the_current_underline()
    {
        var open = CdsUIShellTokens.HeaderMenuTrigger(CdsInteractionState.None, true, true);
        Assert.Equal(CdsColorKeys.Layer01, open.Background);
        Assert.Null(open.Bar);
        var closed = CdsUIShellTokens.HeaderMenuTrigger(CdsInteractionState.None, false, true);
        Assert.Equal(CdsColorKeys.BorderInteractive, closed.Bar);
        Assert.Null(CdsUIShellTokens.HeaderMenuTrigger(CdsInteractionState.None, false, false).Bar);
    }

    [Fact]
    public void Side_nav_current_row_is_background_selected_with_a_bar()
    {
        var active = CdsUIShellTokens.SideNavRow(CdsInteractionState.None, true);
        Assert.Equal(CdsColorKeys.BackgroundSelected, active.Background);
        Assert.Equal(CdsColorKeys.BorderInteractive, active.Bar);
        Assert.Equal(CdsColorKeys.TextPrimary, active.Foreground);
        var rest = CdsUIShellTokens.SideNavRow(CdsInteractionState.None, false);
        Assert.Null(rest.Background);
        Assert.Equal(CdsColorKeys.TextSecondary, rest.Foreground);
        Assert.Equal(CdsColorKeys.BackgroundHover, CdsUIShellTokens.SideNavRow(CdsInteractionState.Hover, false).Background);
        Assert.Equal(CdsColorKeys.TextPrimary, CdsUIShellTokens.SideNavRow(CdsInteractionState.Hover, false).Foreground);
    }

    [Fact]
    public void Side_nav_footer_toggle_hovers_with_background_hover()
    {
        Assert.Equal(CdsColorKeys.BackgroundHover, CdsUIShellTokens.SideNavToggle(CdsInteractionState.Hover).Background);
        Assert.Equal(CdsColorKeys.IconPrimary, CdsUIShellTokens.SideNavToggle(CdsInteractionState.Hover).Icon);
        Assert.Equal(CdsColorKeys.IconSecondary, CdsUIShellTokens.SideNavToggle(CdsInteractionState.None).Icon);
    }

    [Fact]
    public void Switcher_items_use_layer_01_states()
    {
        Assert.Equal(CdsColorKeys.TextSecondary, CdsUIShellTokens.SwitcherItem(CdsInteractionState.None, false).Foreground);
        var hover = CdsUIShellTokens.SwitcherItem(CdsInteractionState.Hover, false);
        Assert.Equal(CdsColorKeys.LayerHover01, hover.Background);
        Assert.Equal(CdsColorKeys.TextPrimary, hover.Foreground);
        Assert.Equal(CdsColorKeys.LayerActive01, CdsUIShellTokens.SwitcherItem(CdsInteractionState.Pressed, false).Background);
        var selected = CdsUIShellTokens.SwitcherItem(CdsInteractionState.None, true);
        Assert.Equal(CdsColorKeys.LayerSelected01, selected.Background);
        Assert.Equal(CdsColorKeys.TextPrimary, selected.Foreground);
    }

    [Fact]
    public void Disabled_shell_items_are_disabled_text_and_icons()
    {
        Assert.Equal(CdsColorKeys.TextDisabled, CdsUIShellTokens.HeaderAction(CdsInteractionState.Disabled, false).Foreground);
        Assert.Equal(CdsColorKeys.IconDisabled, CdsUIShellTokens.SideNavRow(CdsInteractionState.Disabled, false).Icon);
        Assert.Equal(CdsColorKeys.TextDisabled, CdsUIShellTokens.SwitcherItem(CdsInteractionState.Disabled, true).Foreground);
    }
}

public class UIShellControlTests
{
    [Fact]
    public void Header_parts_can_be_created_and_configured()
    {
        var header = new CdsHeader { Label = "Platform" };
        header.Items.Add(new CdsSkipToContent { Text = "Skip to content" });
        header.Items.Add(new CdsHeaderMenuButton { IsCollapsible = true, Label = "Menu" });
        header.Items.Add(new CdsHeaderName("Platform", "IBM"));
        var nav = new CdsHeaderNavigation { Label = "Platform" };
        nav.Items.Add(new CdsHeaderMenuItem("Catalog", isActive: true));
        var menu = new CdsHeaderMenu { Label = "Manage", MenuLabel = "Manage" };
        menu.Items.Add(new CdsHeaderMenuItem("Users"));
        menu.Items.Add(new CdsHeaderMenuItem("Access groups", isActive: true));
        nav.Items.Add(menu);
        header.Items.Add(nav);
        var bar = new CdsHeaderGlobalBar();
        bar.Add(new CdsHeaderGlobalAction(CdsIconName.Search, "Search"));
        bar.Add(new CdsHeaderGlobalAction(CdsIconName.Switcher, "Switcher") { IsActive = true });
        header.Items.Add(bar);

        Assert.True(menu.HasCurrentChild);
        Assert.Equal(2, header.Items.Count(i => i is CdsHeaderName or CdsHeaderNavigation));
    }

    [Fact]
    public void Header_menu_before_load_opens_on_demand_without_throwing()
    {
        var menu = new CdsHeaderMenu { Label = "Manage" };
        menu.Items.Add(new CdsHeaderMenuItem("Users"));
        menu.IsExpanded = true;
        menu.IsExpanded = false;
        Assert.False(menu.IsExpanded);
    }

    [Fact]
    public void Header_menu_dropdown_preview_moves_the_items_into_a_surface()
    {
        var menu = new CdsHeaderMenu { Label = "Manage" };
        var item = new CdsHeaderMenuItem("Users");
        menu.Items.Add(item);
        var preview = menu.CreateDropdownPreview(0);
        Assert.NotNull(preview);
        Assert.NotNull(item.Parent);
    }

    [Fact]
    public void Header_menu_item_keeps_its_properties()
    {
        var clicked = 0;
        var item = new CdsHeaderMenuItem { Text = "Billing", IsActive = true, Href = "https://example.com" };
        item.Clicked += (_, _) => clicked++;
        Assert.Equal("Billing", item.Text);
        Assert.True(item.IsActive);
        Assert.Equal(0, clicked);
    }

    [Fact]
    public void Side_nav_defaults_to_persistent_256_at_lg()
    {
        var nav = new CdsSideNav { Body = new CdsSideNavItems() };
        Assert.Equal(CdsSideNavMode.Default, nav.Mode);
        Assert.Equal(256, nav.CurrentWidth);
        Assert.False(nav.IsExpanded);
        nav.IsPersistent = false;
        Assert.Equal(0, nav.CurrentWidth);
        nav.IsExpanded = true;
        Assert.Equal(256, nav.CurrentWidth);
    }

    [Fact]
    public void Side_nav_rail_is_48_until_expanded()
    {
        var nav = new CdsSideNav { IsRail = true };
        Assert.Equal(CdsSideNavMode.Rail, nav.Mode);
        Assert.Equal(48, nav.CurrentWidth);
        Assert.Equal(48, nav.BodyOffset);
        nav.IsExpanded = true;
        Assert.Equal(256, nav.CurrentWidth);
        Assert.Equal(48, nav.BodyOffset);
    }

    [Fact]
    public void Fixed_side_nav_is_always_256()
    {
        var nav = new CdsSideNav { IsFixed = true, IsRail = true };
        Assert.Equal(CdsSideNavMode.Fixed, nav.Mode);
        Assert.Equal(256, nav.CurrentWidth);
    }

    [Fact]
    public void Shell_forces_a_breakpoint_for_its_parts()
    {
        var nav = new CdsSideNav { Body = new CdsSideNavItems() };
        var shell = new CdsUIShell { Breakpoint = CdsBreakpoint.Md, SideNav = nav };
        Assert.Equal(CdsBreakpoint.Md, shell.EffectiveBreakpoint);
        Assert.Equal(0, nav.CurrentWidth);
        Assert.False(nav.ShowsScrim);
        nav.IsExpanded = true;
        Assert.Equal(256, nav.CurrentWidth);
        Assert.True(nav.ShowsScrim);
        Assert.Equal(0, nav.BodyOffset);
        shell.Breakpoint = CdsBreakpoint.Lg;
        Assert.Equal(256, nav.BodyOffset);
        Assert.False(nav.ShowsScrim);
    }

    [Fact]
    public void Menu_button_mirrors_the_shell_side_nav_and_toggles_it()
    {
        var nav = new CdsSideNav { Body = new CdsSideNavItems() };
        var button = new CdsHeaderMenuButton();
        var header = new CdsHeader();
        header.Items.Add(button);
        var shell = new CdsUIShell { Breakpoint = CdsBreakpoint.Md, Header = header, SideNav = nav };
        Assert.True(button.IsShown);
        Assert.False(button.IsActive);
        nav.IsExpanded = true;
        Assert.True(button.IsActive);
        shell.Breakpoint = CdsBreakpoint.Xlg;
        Assert.False(button.IsShown);
        button.IsCollapsible = true;
        Assert.True(button.IsShown);
    }

    [Fact]
    public void Side_nav_menu_expands_and_reports_a_current_child()
    {
        var menu = new CdsSideNavMenu { Text = "Infrastructure", Icon = CdsIconName.Folder };
        var current = new CdsSideNavMenuItem("Buckets", isActive: true);
        menu.Items.Add(new CdsSideNavMenuItem("Clusters"));
        menu.Items.Add(current);
        Assert.True(menu.HasActiveChild);
        Assert.False(menu.IsExpanded);
        menu.IsExpanded = true;
        Assert.True(menu.IsExpanded);
        current.IsActive = false;
        Assert.False(menu.HasActiveChild);
    }

    [Fact]
    public void Side_nav_header_footer_and_rows_can_be_created()
    {
        var nav = new CdsSideNav
        {
            Header = new CdsSideNavHeader { Text = "Platform", Icon = CdsIconName.Catalog },
            Footer = new CdsSideNavFooter { Label = "Toggle" },
            Body = new CdsSideNavItems
            {
                new CdsHeaderSideNavItems { HasDivider = true, Items = { new CdsHeaderMenuItem("Catalog") } },
                new CdsSideNavLink("Dashboard", CdsIconName.Dashboard, isActive: true) { IsLarge = true },
                new CdsSideNavDivider(),
            },
        };
        Assert.NotNull(nav.Header);
        Assert.NotNull(nav.Footer);
    }

    [Fact]
    public void Switcher_items_are_tabbable_only_while_expanded()
    {
        var item = new CdsSwitcherItem("Platform", isSelected: true);
        var switcher = new CdsSwitcher { Label = "Switch product", IsExpanded = false };
        switcher.Items.Add(item);
        switcher.Items.Add(new CdsSwitcherDivider());
        switcher.Items.Add(new CdsSwitcherItem("Storage"));
        switcher.IsExpanded = true;
        Assert.True(item.IsActive);
        Assert.Equal(3, switcher.Items.Count);
    }

    [Fact]
    public void The_shell_tells_its_parts_about_side_nav_state_but_not_about_every_width_change()
    {
        var nav = new CdsSideNav { Body = new CdsSideNavItems() };
        var shell = new CdsUIShell { SideNav = nav };
        var raised = 0;
        shell.ShellChanged += (_, _) => raised++;
        // The width animates frame by frame; parts that re-evaluated on each frame restarted the side nav's own animation.
        nav.IsPersistent = false;
        Assert.Equal(0, nav.CurrentWidth);
        Assert.Equal(1, raised); // the side nav closed: the menu button mirrors that
        nav.IsExpanded = true;
        Assert.Equal(2, raised);
        nav.IsExpanded = true;
        Assert.Equal(2, raised);
    }
}

public class UIShellConformanceTests
{
    static CdsHitTarget Hit(CdsControl control) => (CdsHitTarget)control.AccessibilityTarget!;

    [Fact]
    public void Side_nav_open_state_and_footer_toggling_by_mode()
    {
        Assert.Equal(0, UIShellModel.SideNavTargetWidth(CdsSideNavMode.Default, CdsBreakpoint.Lg, false, true, false, userCollapsed: true));
        Assert.Equal(256, UIShellModel.SideNavTargetWidth(CdsSideNavMode.Default, CdsBreakpoint.Lg, true, true, false, userCollapsed: true));
        Assert.True(UIShellModel.SideNavOpen(256));
        Assert.False(UIShellModel.SideNavOpen(48));
        Assert.False(UIShellModel.SideNavOpen(0));
        Assert.True(UIShellModel.SideNavFooterToggles(CdsSideNavMode.Rail, CdsBreakpoint.Lg, true));
        Assert.False(UIShellModel.SideNavFooterToggles(CdsSideNavMode.Fixed, CdsBreakpoint.Sm, true));
        Assert.False(UIShellModel.SideNavFooterToggles(CdsSideNavMode.Default, CdsBreakpoint.Lg, true));
        Assert.True(UIShellModel.SideNavFooterToggles(CdsSideNavMode.Default, CdsBreakpoint.Md, true));
        Assert.True(UIShellModel.SideNavFooterToggles(CdsSideNavMode.Default, CdsBreakpoint.Lg, false));
    }

    [Fact]
    public void Footer_of_a_persistent_side_nav_at_lg_says_expanded_and_hides_its_toggle()
    {
        var footer = new CdsSideNavFooter();
        var nav = new CdsSideNav { Footer = footer, Body = new CdsSideNavItems() };
        Assert.False(nav.IsExpanded);
        Assert.True(nav.IsOpen);
        Assert.True(footer.ShowsExpanded);
        Assert.Equal(true, Hit(footer).IsExpanded);
        Assert.Equal("Collapse side navigation", SemanticProperties.GetDescription(Hit(footer)));
        Assert.False(((View)footer.AccessibilityTarget!.Parent!).IsVisible);
    }

    [Fact]
    public void Rail_footer_follows_hover_or_focus_expansion_and_collapses_it()
    {
        var footer = new CdsSideNavFooter();
        var nav = new CdsSideNav { IsRail = true, Footer = footer, Body = new CdsSideNavItems() };
        Assert.False(footer.ShowsExpanded);
        nav.ItemFocusChanged(true); // focus inside expands a rail
        Assert.Equal(256, nav.CurrentWidth);
        Assert.True(footer.ShowsExpanded);
        footer.Activate();
        Assert.Equal(48, nav.CurrentWidth);
        Assert.False(footer.ShowsExpanded);
        footer.Activate();
        Assert.True(nav.IsExpanded);
        Assert.Equal(256, nav.CurrentWidth);
        Assert.True(footer.ShowsExpanded);
    }

    [Fact]
    public void Collapsible_menu_button_reports_and_collapses_a_persistent_side_nav_at_lg()
    {
        var nav = new CdsSideNav { Body = new CdsSideNavItems() };
        var button = new CdsHeaderMenuButton { IsCollapsible = true };
        var header = new CdsHeader();
        header.Items.Add(button);
        _ = new CdsUIShell { Breakpoint = CdsBreakpoint.Lg, Header = header, SideNav = nav };
        Assert.Equal(256, nav.CurrentWidth);
        Assert.True(button.IsActive);
        Assert.Equal("Close menu", SemanticProperties.GetDescription(Hit(button)));
        button.Activate();
        Assert.Equal(0, nav.CurrentWidth);
        Assert.False(button.IsActive);
        Assert.False(nav.IsExpanded);
        button.Activate();
        Assert.Equal(256, nav.CurrentWidth);
        Assert.True(button.IsActive);
        button.Activate();
        Assert.Equal(0, nav.CurrentWidth);
    }

    [Fact]
    public void Menu_button_below_lg_toggles_the_side_nav()
    {
        var nav = new CdsSideNav { Body = new CdsSideNavItems() };
        var button = new CdsHeaderMenuButton();
        var header = new CdsHeader();
        header.Items.Add(button);
        _ = new CdsUIShell { Breakpoint = CdsBreakpoint.Md, Header = header, SideNav = nav };
        Assert.False(button.IsActive);
        button.Activate();
        Assert.True(nav.IsExpanded);
        Assert.True(button.IsActive);
        button.Activate();
        Assert.False(nav.IsExpanded);
        Assert.False(button.IsActive);
    }

    [Fact]
    public void Skip_to_content_defaults_to_the_first_focusable_element_of_the_shell_body()
    {
        var hidden = new CdsButton { Text = "Hidden", IsVisible = false };
        var disabled = new CdsLink { Text = "Disabled", IsEnabled = false };
        var first = new CdsLink { Text = "Clusters" };
        var skip = new CdsSkipToContent();
        var header = new CdsHeader();
        header.Items.Add(skip);
        var shell = new CdsUIShell
        {
            Header = header,
            Body = new VerticalStackLayout { new CdsText("Overview"), hidden, disabled, new ContentView { Content = first } },
        };
        Assert.Same(first.AccessibilityTarget, skip.ResolveTarget());
        var entry = new Entry();
        skip.Target = new VerticalStackLayout { new Label { Text = "Name" }, entry };
        Assert.Same(entry, skip.ResolveTarget());
        skip.Target = entry;
        Assert.Same(entry, skip.ResolveTarget());
        shell.Body = null;
        skip.Target = null;
        Assert.Null(skip.ResolveTarget());
    }

    [Fact]
    public void Side_nav_links_and_menu_items_report_the_current_page()
    {
        var link = new CdsSideNavLink("Dashboard");
        var item = new CdsSideNavMenuItem("Buckets", isActive: true);
        Assert.Equal(CdsCurrentKind.None, Hit(link).CurrentKind);
        Assert.Equal(CdsCurrentKind.Page, Hit(item).CurrentKind);
        link.IsActive = true;
        item.IsActive = false;
        Assert.Equal(CdsCurrentKind.Page, Hit(link).CurrentKind);
        Assert.Equal(CdsCurrentKind.None, Hit(item).CurrentKind);
    }

    [Fact]
    public void Header_menu_items_and_switcher_items_report_the_current_page()
    {
        var link = new CdsHeaderMenuItem("Catalog", isActive: true);
        Assert.Equal(CdsCurrentKind.Page, Hit(link).CurrentKind);
        link.IsActive = false;
        Assert.Equal(CdsCurrentKind.None, Hit(link).CurrentKind);
        var product = new CdsSwitcherItem("Platform", isSelected: true);
        Assert.Equal(CdsCurrentKind.Page, Hit(product).CurrentKind);
    }

    [Fact]
    public void Panel_actions_report_collapsed_while_closed_and_plain_actions_report_nothing()
    {
        Assert.Equal(true, CdsHeaderGlobalAction.ExpandedState(controlsPanel: true, isActive: true));
        Assert.Equal(false, CdsHeaderGlobalAction.ExpandedState(controlsPanel: true, isActive: false));
        Assert.Null(CdsHeaderGlobalAction.ExpandedState(controlsPanel: false, isActive: false));
        var action = new CdsHeaderGlobalAction(CdsIconName.Switcher, "Switch environment") { ControlsPanel = true };
        Assert.Equal(false, Hit(action).IsExpanded);
        action.IsActive = true;
        Assert.Equal(true, Hit(action).IsExpanded);
        action.IsActive = false;
        Assert.Equal(false, Hit(action).IsExpanded);
        var plain = new CdsHeaderGlobalAction(CdsIconName.Search, "Search");
        Assert.Null(Hit(plain).IsExpanded);
    }

    [Fact]
    public void Header_menu_trigger_and_breadcrumb_overflow_report_collapsed_while_closed()
    {
        var menu = new CdsHeaderMenu { Label = "Manage" };
        Assert.Equal(false, Hit(menu).IsExpanded);
        var overflow = new CdsBreadcrumbItem();
        overflow.OverflowItems.Add(new CdsBreadcrumbItem("Projects"));
        Assert.Equal(false, Hit(overflow).IsExpanded);
        Assert.Equal(CdsAccessibilityRole.Button, Hit(overflow).Role);
    }
}
