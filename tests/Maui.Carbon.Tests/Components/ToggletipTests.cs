namespace Maui.Carbon.Tests.Components;

public class ToggletipTests
{
    [Fact]
    public void Icon_is_secondary_until_hover_or_open()
    {
        Assert.Equal(CdsColorKeys.IconSecondary, CdsToggletipTokens.Icon(CdsInteractionState.None));
        Assert.Equal(CdsColorKeys.IconSecondary, CdsToggletipTokens.Icon(CdsInteractionState.FocusVisible));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsToggletipTokens.Icon(CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsToggletipTokens.Icon(CdsInteractionState.Open));
    }

    [Fact]
    public void Toggletip_can_be_created_and_toggled_before_loading()
    {
        var tip = new CdsToggletip
        {
            Label = "Show information about the upgrade window",
            Text = "Upgrades start only inside this window.",
            Placement = CdsPlacement.BottomStart,
            Icon = CdsIconName.Information,
            ToggletipContent = new CdsToggletipActions(),
        };
        Assert.False(tip.IsOpen);
        tip.IsOpen = true;
        tip.IsOpen = false;
        Assert.Equal("Show information about the upgrade window", tip.Label);
    }

    [Fact]
    public void Default_label_is_Show_information() => Assert.Equal("Show information", new CdsToggletip().Label);
}
