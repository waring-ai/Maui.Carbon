namespace Maui.Carbon.Tests.Components;

public class SidePanelTests
{
    [Fact]
    public void Defaults_follow_ibm_products()
    {
        var panel = new CdsSidePanel();
        Assert.False(panel.IsOpen);
        Assert.Equal(CdsSidePanelSize.Md, panel.Size);
        Assert.Equal(CdsSidePanelPlacement.End, panel.Placement);
        Assert.True(panel.AnimateTitle);
        Assert.False(panel.IncludeOverlay);
        Assert.Equal("Close", panel.CloseButtonLabel);
        Assert.Equal("Back", panel.BackButtonLabel);
        Assert.Equal(0, panel.CurrentStep);
    }

    [Theory]
    [InlineData(CdsSidePanelSize.Xs, 1440, 256)]
    [InlineData(CdsSidePanelSize.Md, 1440, 480)]
    [InlineData(CdsSidePanelSize.Xxl, 1440, 1080)]
    [InlineData(CdsSidePanelSize.Lg, 700, 525)]
    [InlineData(CdsSidePanelSize.Md, 400, 400)]
    [InlineData(CdsSidePanelSize.Xs, 200, 200)]
    public void Width_is_clamped_to_the_viewport(CdsSidePanelSize size, double viewport, double expected) =>
        Assert.Equal(expected, SidePanelModel.Width(size, viewport));

    [Fact]
    public void Header_measurements_follow_the_styles()
    {
        Assert.Equal(CdsSize.Md, SidePanelModel.HeaderButtonSize(CdsSidePanelSize.Lg, hasActions: true));
        Assert.Equal(CdsSize.Sm, SidePanelModel.HeaderButtonSize(CdsSidePanelSize.Lg, hasActions: false));
        Assert.Equal(CdsSize.Sm, SidePanelModel.HeaderButtonSize(CdsSidePanelSize.Md, hasActions: true));
        Assert.Equal(32, SidePanelModel.TitleEndPadding(false, false));
        Assert.Equal(64, SidePanelModel.TitleEndPadding(true, false));
        Assert.Equal(80, SidePanelModel.TitleEndPadding(true, true));
        Assert.Equal(16, SidePanelModel.HeaderTopPadding(false, CdsSize.Sm));
        Assert.Equal(32, SidePanelModel.HeaderTopPadding(true, CdsSize.Sm));
        Assert.Equal(40, SidePanelModel.HeaderTopPadding(true, CdsSize.Md));
        Assert.Equal(8, SidePanelModel.TitleBottomPadding(true));
        Assert.Equal(8, SidePanelModel.BodyTopPadding(hasToolbar: true, animateTitle: true));
        Assert.Equal(16, SidePanelModel.BodyTopPadding(hasToolbar: false, animateTitle: false));
        Assert.Equal(CdsActionSetSize.Sm, SidePanelModel.ActionSetSize(CdsSidePanelSize.Xs));
    }

    [Fact]
    public void Title_progress_is_clamped()
    {
        Assert.Equal(0, SidePanelModel.Progress(-10, 40));
        Assert.Equal(0.5, SidePanelModel.Progress(20, 40));
        Assert.Equal(1, SidePanelModel.Progress(400, 40));
        Assert.Equal(0, SidePanelModel.Progress(10, 0));
        Assert.False(SidePanelModel.CanAnimateTitle(true, true, true, scrollRange: 10, distance: 40));
        Assert.True(SidePanelModel.CanAnimateTitle(true, true, true, scrollRange: 400, distance: 40));
        Assert.False(SidePanelModel.CanAnimateTitle(true, true, false, 400, 40));
    }

    static (CdsOverlayHost Host, VerticalStackLayout Page) Host()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        return (host, page);
    }

    [Fact]
    public void Opens_docked_at_the_end_and_closes_from_the_close_button()
    {
        var (host, page) = Host();
        var panel = new CdsSidePanel { Title = "Edit cluster", Label = "prod-eu-de-1", Body = new CdsTextInput { Label = "Name" } };
        panel.Actions.Add(new CdsButton("Save", CdsButtonKind.Primary));
        panel.Actions.Add(new CdsButton("Cancel", CdsButtonKind.Secondary));
        page.Add(panel);
        panel.IsOpen = true;
        panel.Show();

        Assert.True(panel.IsShown);
        var overlay = Assert.Single(host.Overlays);
        Assert.Equal(CdsOverlayDock.End, overlay.Options.Dock);
        Assert.False(overlay.Options.IsModal);
        Assert.Equal(LayoutOptions.End, overlay.Container.HorizontalOptions);
        Assert.Equal(CdsActionSetSize.Md, panel.ActionSet.Size);
        Assert.Equal("Save", panel.ActionSet.Shown[^1].Text);

        var closing = 0;
        panel.RequestClose += (_, e) => { closing++; e.Cancel = closing == 1; };
        Assert.False(panel.TryClose());
        Assert.True(panel.IsOpen);
        Assert.True(panel.TryClose());
        Assert.False(panel.IsOpen);
        Assert.False(panel.IsShown);
        Assert.Empty(host.Overlays);
    }

    [Fact]
    public void Overlay_variant_is_modal_and_the_scrim_closes_it()
    {
        var (host, page) = Host();
        var panel = new CdsSidePanel { Title = "Filters", IncludeOverlay = true, Placement = CdsSidePanelPlacement.Start };
        page.Add(panel);
        var closed = 0;
        panel.Closed += (_, _) => closed++;
        panel.IsOpen = true;
        panel.Show();
        var overlay = Assert.Single(host.Overlays);
        Assert.True(overlay.Options.IsModal);
        Assert.Equal(LayoutOptions.Start, overlay.Container.HorizontalOptions);

        panel.PreventCloseOnClickOutside = true;
        overlay.Options.ScrimTapped!();
        Assert.True(panel.IsOpen);
        panel.PreventCloseOnClickOutside = false;
        overlay.Options.ScrimTapped!();
        Assert.False(panel.IsOpen);
        Assert.Equal(1, closed);
    }

    [Fact]
    public void Escape_closes_a_slide_over_but_not_a_slide_in()
    {
        var (host, page) = Host();
        var content = new ContentView();
        page.Add(content);
        var over = new CdsSidePanel { Title = "Details" };
        page.Add(over);
        over.IsOpen = true;
        over.Show();
        Assert.True(host.RouteKey(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.False(over.IsOpen);

        var slide = new CdsSidePanel { Title = "Details", SlideIn = true, PageContent = content, Size = CdsSidePanelSize.Sm };
        page.Add(slide);
        slide.IsOpen = true;
        slide.Show();
        Assert.Null(Assert.Single(host.Overlays).Options.KeyHandler);
        Assert.False(host.RouteKey(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.True(slide.IsOpen);
        Assert.Equal(320, content.Margin.Right);
        slide.IsOpen = false;
        Assert.Equal(0, content.Margin.Right);
    }

    [Fact]
    public void Steps_show_a_back_button_that_raises_navigation_back()
    {
        var (_, page) = Host();
        var panel = new CdsSidePanel { Title = "Create rule" };
        page.Add(panel);
        panel.IsOpen = true;
        panel.Show();
        Assert.False(panel.BackButton!.IsVisible);
        panel.CurrentStep = 1;
        Assert.True(panel.BackButton.IsVisible);
        Assert.Equal("Back", panel.BackButton.Label);
        panel.HideCloseButton = true;
        Assert.False(panel.CloseButton!.IsVisible);
        panel.IsOpen = false;
    }

    [Fact]
    public void Popups_draw_above_modals_and_panels()
    {
        var (host, page) = Host();
        var anchor = new CdsButton("Options");
        page.Add(anchor);
        var modal = CdsOverlayService.Open(new ContentView(), new CdsOverlayOptions { IsModal = true }, page);
        var docked = CdsOverlayService.Open(new ContentView(), new CdsOverlayOptions { Dock = CdsOverlayDock.End }, page);
        var popup = CdsOverlayService.Open(new ContentView(), new CdsOverlayOptions { Anchor = anchor }, page);
        int Layer(CdsOverlay o) => host.Children.IndexOf((IView)o.Container.Parent!);
        Assert.True(Layer(popup) > Layer(modal));
        Assert.True(Layer(modal) > Layer(docked));
        host.CloseAll();
    }
}
