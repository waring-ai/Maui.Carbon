namespace Maui.Carbon.Tests.Components;

public class IconButtonTests
{
    [Theory]
    [InlineData(CdsIconButtonKind.Primary, CdsButtonKind.Primary)]
    [InlineData(CdsIconButtonKind.Secondary, CdsButtonKind.Secondary)]
    [InlineData(CdsIconButtonKind.Tertiary, CdsButtonKind.Tertiary)]
    [InlineData(CdsIconButtonKind.Ghost, CdsButtonKind.Ghost)]
    public void Kinds_render_as_the_matching_button(CdsIconButtonKind kind, CdsButtonKind button)
    {
        Assert.Equal(button, CdsIconButtonTokens.ToButtonKind(kind));
        Assert.Equal(CdsButtonTokens.Resolve(button, CdsInteractionState.Hover), CdsIconButtonTokens.Resolve(kind, CdsInteractionState.Hover));
    }

    [Fact]
    public void Primary_uses_button_primary_and_ghost_icon_primary()
    {
        Assert.Equal(CdsColorKeys.ButtonPrimary, CdsIconButtonTokens.Resolve(CdsIconButtonKind.Primary, CdsInteractionState.None).Background);
        var ghost = CdsIconButtonTokens.Resolve(CdsIconButtonKind.Ghost, CdsInteractionState.None);
        Assert.Null(ghost.Background);
        Assert.Equal(CdsColorKeys.IconPrimary, ghost.Icon);
    }

    [Theory]
    [InlineData(CdsIconButtonKind.Primary)]
    [InlineData(CdsIconButtonKind.Secondary)]
    [InlineData(CdsIconButtonKind.Tertiary)]
    [InlineData(CdsIconButtonKind.Ghost)]
    public void Selected_fills_with_background_selected_whatever_the_kind(CdsIconButtonKind kind)
    {
        var rest = CdsIconButtonTokens.Resolve(kind, CdsInteractionState.Selected);
        Assert.Equal(CdsColorKeys.BackgroundSelected, rest.Background);
        Assert.Equal(CdsColorKeys.IconPrimary, rest.Icon);
        Assert.Equal(CdsColorKeys.BackgroundSelectedHover, CdsIconButtonTokens.Resolve(kind, CdsInteractionState.Selected | CdsInteractionState.Hover).Background);
        Assert.Equal(CdsColorKeys.BackgroundSelectedHover, CdsIconButtonTokens.Resolve(kind, CdsInteractionState.Selected | CdsInteractionState.Pressed).Background);
    }

    [Fact]
    public void Disabled_wins_over_selected()
    {
        var v = CdsIconButtonTokens.Resolve(CdsIconButtonKind.Ghost, CdsInteractionState.Selected | CdsInteractionState.Disabled);
        Assert.Equal(CdsColorKeys.IconDisabled, v.Icon);
        Assert.Null(v.Background);
    }

    [Fact]
    public void Ghost_gets_the_inset_ring_and_others_the_button_ring()
    {
        Assert.Equal(CdsFocusRingStyle.Inset, CdsIconButtonTokens.FocusRing(CdsIconButtonKind.Ghost));
        Assert.Equal(CdsFocusRingStyle.Button, CdsIconButtonTokens.FocusRing(CdsIconButtonKind.Primary));
    }

    [Theory]
    [InlineData(CdsSize.Xs, 24)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    public void Side_follows_size(CdsSize size, double side) => Assert.Equal(side, CdsIconButtonTokens.Side(size));

    [Theory]
    [InlineData(null, null)]
    [InlineData(-3, null)]
    [InlineData(0, "")]
    [InlineData(7, "7")]
    public void Badge_text(int? count, string? text) => Assert.Equal(text, CdsIconButtonTokens.BadgeText(count));

    [Fact]
    public void Defaults_and_property_setting_do_not_throw()
    {
        var button = new CdsIconButton(CdsIconName.Download, "Download logs", CdsIconButtonKind.Ghost);
        Assert.Equal(CdsSize.Lg, button.Size);
        Assert.Equal(CdsPlacement.Top, button.TooltipPlacement);
        Assert.True(button.IsHighContrast);
        Assert.Equal(100, button.EnterDelay);
        Assert.Equal(300, button.LeaveDelay);
        Assert.Null(button.IsSelected);
        button.Size = CdsSize.Sm;
        button.IsSelected = false;
        button.BadgeCount = 3;
        button.BadgeCount = 0;
        button.TooltipPlacement = CdsPlacement.Bottom;
        button.IsHighContrast = false;
        button.HasDropShadow = true;
        button.IsEnabled = false;
        button.IsEnabled = true;
        Assert.False(button.IsTooltipOpen);
    }

    [Fact]
    public void A_command_that_cannot_execute_disables_the_button()
    {
        var button = new CdsIconButton { Command = new Command(() => { }, () => false) };
        Assert.False(button.IsEnabled);
    }
}
