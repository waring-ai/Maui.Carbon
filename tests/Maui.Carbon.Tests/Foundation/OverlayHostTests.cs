namespace Maui.Carbon.Tests.Foundation;

/// <summary>
/// The page overlay host's key routing, toast stacking, focus return and motion (design-conformance review CO-01, CO-07,
/// CO-04, CO-13). It sets the process-wide focus tracker, so it runs in the non-parallel focus collection.
/// </summary>
[Collection(FocusTrackerCollection.Name)]
public class OverlayHostTests
{
    static (CdsOverlayHost Host, VerticalStackLayout Page) NewPage()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        return (host, page);
    }

    static CdsModal NewModal(VerticalStackLayout page)
    {
        var modal = new CdsModal { Heading = "Delete bucket", PrimaryButtonText = "Delete", SecondaryButtonText = "Cancel" };
        page.Add(modal);
        return modal;
    }

    static readonly CdsKeyEventArgs Escape = new(CdsKey.Escape, false);

    [Fact]
    public void Toasts_stack_newest_on_top()
    {
        var (host, _) = NewPage();
        var first = CdsToast.Show(host, new CdsToastOptions { Title = "Deployment failed" });
        var second = CdsToast.Show(host, new CdsToastOptions { Title = "Rollback failed" });
        var third = CdsToast.Show(host, new CdsToastOptions { Title = "Backup complete" });

        var column = Assert.IsAssignableFrom<Layout>(first.Container.Parent);
        Assert.Equal(0, column.IndexOf(third.Container));
        Assert.Equal(1, column.IndexOf(second.Container));
        Assert.Equal(2, column.IndexOf(first.Container));

        second.Close();
        Assert.Equal(0, column.IndexOf(third.Container));
        Assert.Equal(1, column.IndexOf(first.Container));
    }

    [Fact]
    public void A_toast_that_opens_over_a_modal_does_not_take_Escape_from_it()
    {
        var (host, page) = NewPage();
        var modal = NewModal(page);
        modal.IsOpen = true;
        modal.Show();
        var modalOverlay = Assert.Single(host.Overlays);

        var toast = CdsToast.Show(host, new CdsToastOptions { Title = "Sync failed", Kind = CdsNotificationKind.Error });
        Assert.Same(toast, host.TopOverlay);
        Assert.Same(modalOverlay, host.KeyOverlay);
        Assert.False(toast.TakesKeys);

        Assert.True(host.RouteKey(Escape));
        Assert.False(modal.IsOpen);
        Assert.False(modalOverlay.IsOpen);
        Assert.Same(toast, Assert.Single(host.Overlays));
    }

    [Fact]
    public void A_toast_that_opens_over_a_menu_leaves_it_the_arrows_and_Enter()
    {
        var (host, page) = NewPage();
        var trigger = new CdsButton { Text = "Options" };
        page.Add(trigger);
        var keys = new List<CdsKey>();
        var menu = CdsOverlayService.Open(new ContentView(), new CdsOverlayOptions
        {
            Anchor = trigger,
            Keys = CdsOverlayKeys.Navigation | CdsOverlayKeys.Activation,
            KeyHandler = e =>
            {
                keys.Add(e.Key);
                return true;
            },
        });
        CdsToast.Show(host, new CdsToastOptions { Title = "Saved", Kind = CdsNotificationKind.Success });

        Assert.Same(menu, host.KeyOverlay);
        var wanted = host.WantedKeys;
        Assert.True(wanted.Escape);
        Assert.True(wanted.Vertical);
        Assert.True(wanted.Horizontal);
        Assert.True(wanted.Activation);
        Assert.True(CdsOverlayHostHandler.Route(host, CdsKey.Down, false));
        Assert.True(CdsOverlayHostHandler.Route(host, CdsKey.Enter, false));
        Assert.Equal([CdsKey.Down, CdsKey.Enter], keys);
    }

    [Fact]
    public void A_toast_alone_still_has_Escape_captured_but_takes_no_other_keys()
    {
        var (host, _) = NewPage();
        Assert.False(host.WantedKeys.Escape);
        CdsToast.Show(host, new CdsToastOptions { Title = "Saved" });
        Assert.Null(host.KeyOverlay);
        var wanted = host.WantedKeys;
        Assert.True(wanted.Escape);
        Assert.False(wanted.Activation);
    }

    [Fact]
    public void A_modal_without_a_launcher_returns_focus_to_the_control_focused_before_it_opened()
    {
        var (host, page) = NewPage();
        var deploy = new CdsButton { Text = "Deploy" };
        page.Add(deploy);
        var modal = NewModal(page);
        CdsFocusTracker.SetFocused(deploy);
        try
        {
            modal.IsOpen = true;
            modal.Show();
            var overlay = Assert.Single(host.Overlays);
            Assert.Same(deploy, overlay.ReturnFocusTarget);

            // Gone from the page by the time the modal closes: no stale target.
            page.Remove(deploy);
            Assert.Null(overlay.ReturnFocusTarget);
            page.Add(deploy);
            deploy.IsVisible = false;
            Assert.Null(overlay.ReturnFocusTarget);
            deploy.IsVisible = true;
            modal.IsOpen = false;
            Assert.Empty(host.Overlays);

            // A launcher wins over the remembered control.
            var launcher = new CdsButton { Text = "Delete" };
            page.Add(launcher);
            modal.LauncherElement = launcher;
            modal.IsOpen = true;
            modal.Show();
            Assert.Same(launcher, Assert.Single(host.Overlays).ReturnFocusTarget);
            modal.IsOpen = false;
        }
        finally
        {
            CdsFocusTracker.ClearFocused(deploy);
        }
    }

    [Fact]
    public void Dialog_and_composed_modal_return_focus_to_the_control_focused_before_opening()
    {
        var (host, page) = NewPage();
        var search = new CdsButton { Text = "Find" };
        page.Add(search);
        var dialog = new CdsDialog { Title = "Find and replace", IsModal = true };
        var composed = new CdsComposedModal();
        composed.Parts.Add(new CdsModalHeader { Title = "Create API key" });
        composed.Parts.Add(new CdsModalFooter { PrimaryButtonText = "Create", SecondaryButtonText = "Cancel" });
        page.Add(dialog);
        page.Add(composed);
        CdsFocusTracker.SetFocused(search);
        try
        {
            dialog.IsOpen = true;
            dialog.Show();
            Assert.Same(search, Assert.Single(host.Overlays).ReturnFocusTarget);
            dialog.IsOpen = false;

            composed.IsOpen = true;
            composed.Show();
            Assert.Same(search, Assert.Single(host.Overlays).ReturnFocusTarget);
            composed.IsOpen = false;
            Assert.Empty(host.Overlays);
        }
        finally
        {
            CdsFocusTracker.ClearFocused(search);
        }
    }

    [Fact]
    public void Toasts_and_tooltips_do_not_move_focus_back()
    {
        var (host, page) = NewPage();
        var field = new CdsButton { Text = "Name" };
        page.Add(field);
        CdsFocusTracker.SetFocused(field);
        try
        {
            var toast = CdsToast.Show(host, new CdsToastOptions { Title = "Saved" });
            Assert.Null(toast.ReturnFocusTarget);
        }
        finally
        {
            CdsFocusTracker.ClearFocused(field);
        }
    }

    [Fact]
    public void Modal_and_dialog_motion_is_expressive()
    {
        Assert.Same(CdsEasing.EntranceExpressive, CdsOverlayHost.EntranceEasing);
        Assert.Same(CdsEasing.ExitExpressive, CdsOverlayHost.ExitEasing);
        // .cds--modal-container enters from translate3d(0, -24px, 0) and leaves the same way.
        Assert.Equal(-24, CdsOverlayHost.SlideDistance);
    }
}
