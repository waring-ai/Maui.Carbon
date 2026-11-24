namespace Maui.Carbon.Tests.Components;

public class OverflowMenuTests
{
    [Fact]
    public void The_trigger_is_transparent_with_an_icon_primary_glyph()
    {
        var v = CdsOverflowMenuTokens.Resolve(CdsInteractionState.None);
        Assert.Null(v.Background);
        Assert.Equal(CdsColorKeys.IconPrimary, v.Icon);
    }

    [Fact]
    public void Hover_is_background_hover_and_open_is_layer_01()
    {
        Assert.Equal(CdsColorKeys.BackgroundHover, CdsOverflowMenuTokens.Resolve(CdsInteractionState.Hover).Background);
        Assert.Equal(CdsColorKeys.Layer01, CdsOverflowMenuTokens.Resolve(CdsInteractionState.Open).Background);
        Assert.Equal(CdsColorKeys.Layer01, CdsOverflowMenuTokens.Resolve(CdsInteractionState.Open | CdsInteractionState.Hover).Background);
    }

    [Fact]
    public void Disabled_uses_icon_disabled_and_no_fill()
    {
        var v = CdsOverflowMenuTokens.Resolve(CdsInteractionState.Disabled | CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.IconDisabled, v.Icon);
        Assert.Null(v.Background);
    }

    [Theory]
    [InlineData(CdsSize.Xs, 24)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xxl, 48)]
    public void Side_follows_size(CdsSize size, double side) => Assert.Equal(side, CdsOverflowMenuTokens.Side(size));

    [Theory]
    [InlineData(CdsOverflowMenuDirection.Bottom, false, CdsPlacement.BottomStart)]
    [InlineData(CdsOverflowMenuDirection.Bottom, true, CdsPlacement.BottomEnd)]
    [InlineData(CdsOverflowMenuDirection.Top, false, CdsPlacement.TopStart)]
    [InlineData(CdsOverflowMenuDirection.Top, true, CdsPlacement.TopEnd)]
    public void Placement_follows_direction_and_flipped(CdsOverflowMenuDirection direction, bool flipped, CdsPlacement placement) =>
        Assert.Equal(placement, CdsOverflowMenuTokens.Placement(direction, flipped));

    [Fact]
    public void Defaults_and_items_work_without_a_platform()
    {
        var menu = new CdsOverflowMenu { IconDescription = "Actions for ci-deploy", IsFlipped = true };
        menu.Items.Add(new CdsMenuItem("Rename"));
        menu.Items.Add(new CdsMenuItemDivider());
        menu.Items.Add(new CdsMenuItem("Delete API key") { Kind = CdsMenuItemKind.Danger });
        Assert.Equal(CdsSize.Md, menu.Size);
        Assert.Equal(CdsIconName.OverflowMenuVertical, menu.Icon);
        Assert.Equal(3, menu.Items.Count);
        Assert.Same(menu.Items, menu.Menu.Items);
        menu.Icon = CdsIconName.OverflowMenuHorizontal;
        menu.Size = CdsSize.Sm;
        menu.Direction = CdsOverflowMenuDirection.Top;
        menu.IsEnabled = false;
        Assert.False(menu.IsOpen);
    }

    [Fact]
    public void Setting_IsOpen_before_loading_defers_instead_of_throwing()
    {
        var menu = new CdsOverflowMenu();
        menu.Items.Add(new CdsMenuItem("Rename"));
        menu.IsOpen = true;
        menu.IsOpen = false;
    }

    [Fact]
    public void Focus_leaving_the_trigger_closes_the_menu()
    {
        var menu = new CdsOverflowMenu { IconDescription = "Actions for ci-deploy" };
        menu.Items.Add(new CdsMenuItem("Edit"));
        menu.Items.Add(new CdsMenuItem("Delete") { Kind = CdsMenuItemKind.Danger });
        var host = ActionTestHost.Show(menu);
        var hit = ((CdsActionFace)menu.Content).Hit;
        hit.ReportFocus(true, fromKeyboard: true);
        menu.IsOpen = true;
        Assert.True(menu.Menu.IsOpen);
        hit.ReportFocus(false, fromKeyboard: true);
        Assert.False(menu.Menu.IsOpen);
        Assert.False(menu.IsOpen);
        Assert.Null(host.TopOverlay);
    }
}
