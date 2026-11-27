namespace Maui.Carbon.Tests.Components;

public class TearsheetTests
{
    [Fact]
    public void Model_follows_the_styles()
    {
        Assert.Equal(90, TearsheetModel.TopGap);
        Assert.Equal(600, TearsheetModel.WideWidth(600));
        Assert.Equal(1312, TearsheetModel.WideWidth(1440));
        Assert.Equal(900 - 90, TearsheetModel.Height(1440, 900, 0, true));
        Assert.Equal(900 - 90 + 32, TearsheetModel.Height(1440, 900, 2, true));
        Assert.Equal(900 - 90, TearsheetModel.Height(1440, 900, 2, false));
        Assert.Equal(900, TearsheetModel.Height(500, 900, 0, true));
        Assert.Equal(1, TearsheetModel.StackScale(1000, 0, true));
        Assert.Equal(0.968, TearsheetModel.StackScale(1000, 1, true), 3);
        Assert.Equal(0.936, TearsheetModel.StackScale(1000, 2, true), 3);
        Assert.Equal(1, TearsheetModel.StackScale(1000, 2, false));
        Assert.Equal(257, TearsheetModel.InfluencerWidth(CdsTearsheetInfluencerWidth.Narrow));
        Assert.Equal(321, TearsheetModel.InfluencerWidth(CdsTearsheetInfluencerWidth.Wide));
    }

    [Fact]
    public void Close_icon_defaults_to_passive_only()
    {
        Assert.True(TearsheetModel.HasCloseIcon(null, 0));
        Assert.False(TearsheetModel.HasCloseIcon(null, 2));
        Assert.True(TearsheetModel.HasCloseIcon(true, 2));
        var sheet = new CdsTearsheet();
        Assert.True(sheet.EffectiveHasCloseIcon);
        sheet.Actions.Add(new CdsButton("Create", CdsButtonKind.Primary));
        Assert.False(sheet.EffectiveHasCloseIcon);
    }

    static (CdsOverlayHost Host, VerticalStackLayout Page) Host()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        return (host, page);
    }

    [Fact]
    public void Opens_modal_from_the_bottom_with_wide_action_set()
    {
        var (host, page) = Host();
        var sheet = new CdsTearsheet { Title = "Create pipeline", Label = "Delivery", Description = "Build and deploy on every commit.", HeaderNavigation = new CdsTabs(), Influencer = new Label { Text = "Steps" } };
        sheet.Actions.Add(new CdsButton("Cancel", CdsButtonKind.Secondary));
        sheet.Actions.Add(new CdsButton("Create", CdsButtonKind.Primary));
        page.Add(sheet);
        sheet.IsOpen = true;
        sheet.Show();
        var overlay = Assert.Single(host.Overlays);
        Assert.True(overlay.Options.IsModal);
        Assert.Equal(CdsOverlayDock.Bottom, overlay.Options.Dock);
        Assert.Equal(CdsActionSetSize.Xxl, sheet.ActionSet.Size);
        Assert.Equal(CdsSize.Xxl, sheet.ActionSet.ButtonSize);
        Assert.False(sheet.CloseButton!.IsVisible);

        overlay.Options.ScrimTapped!();
        Assert.True(sheet.IsOpen);
        Assert.True(host.RouteKey(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.False(sheet.IsOpen);
        Assert.Empty(host.Overlays);
    }

    [Fact]
    public void Passive_narrow_tearsheet_closes_on_a_tap_outside()
    {
        var (host, page) = Host();
        var sheet = new CdsTearsheetNarrow { Title = "Release notes", Body = new Label { Text = "Version 2.4" } };
        page.Add(sheet);
        sheet.IsOpen = true;
        sheet.Show();
        Assert.True(sheet.CloseButton!.IsVisible);
        Assert.Equal(CdsActionSetSize.Lg, sheet.ActionSet.Size);
        Assert.Single(host.Overlays).Options.ScrimTapped!();
        Assert.False(sheet.IsOpen);
    }

    [Fact]
    public void Stacks_up_to_three()
    {
        var (host, page) = Host();
        var sheets = Enumerable.Range(0, 4).Select(i => new CdsTearsheet { Title = $"Sheet {i}" }).ToArray();
        foreach (var s in sheets)
        {
            page.Add(s);
            s.IsOpen = true;
            s.Show();
        }
        Assert.Equal(3, host.Overlays.Count);
        Assert.False(sheets[3].IsShown);
        Assert.Equal([2, 1, 0], sheets.Take(3).Select(s => s.LevelsAbove));
        sheets[2].IsOpen = false;
        Assert.Equal([1, 0], sheets.Take(2).Select(s => s.LevelsAbove));
        foreach (var s in sheets) s.IsOpen = false;
        Assert.Empty(host.Overlays);
    }
}
