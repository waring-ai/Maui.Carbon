namespace Maui.Carbon.Tests.Components;

public class ComboButtonTests
{
    [Fact]
    public void Both_halves_use_the_primary_button_tokens()
    {
        foreach (var state in new[] { CdsInteractionState.None, CdsInteractionState.Hover, CdsInteractionState.Pressed, CdsInteractionState.Disabled })
            Assert.Equal(CdsButtonTokens.Resolve(CdsButtonKind.Primary, state), CdsComboButtonTokens.Resolve(state));
        Assert.Equal(CdsColorKeys.ButtonPrimary, CdsComboButtonTokens.Resolve(CdsInteractionState.None).Background);
        Assert.Equal(CdsColorKeys.TextOnColor, CdsComboButtonTokens.Resolve(CdsInteractionState.None).Foreground);
    }

    [Theory]
    [InlineData(CdsSize.Xs, 24)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    public void Side_is_the_height_and_the_chevron_width(CdsSize size, double side) => Assert.Equal(side, CdsComboButtonTokens.Side(size));

    [Theory]
    [InlineData(CdsSize.Sm, 127)]
    [InlineData(CdsSize.Md, 119)]
    [InlineData(CdsSize.Lg, 111)]
    [InlineData(CdsSize.Xs, 135)]
    public void The_primary_action_makes_both_halves_at_least_10rem(CdsSize size, double min)
    {
        Assert.Equal(min, CdsComboButtonTokens.PrimaryMinimumWidth(size));
        Assert.Equal(160, min + CdsComboButtonTokens.Gap + CdsComboButtonTokens.Side(size));
    }

    [Fact]
    public void The_primary_action_never_gets_narrower_than_111()
    {
        Assert.True(CdsComboButtonTokens.PrimaryMinimumWidth(CdsSize.Lg) >= CdsComboButtonTokens.PrimaryMinWidth);
        Assert.Equal(239, CdsComboButtonTokens.PrimaryMaxWidth);
    }

    [Fact]
    public void Defaults_and_property_setting_do_not_throw()
    {
        var button = new CdsComboButton { Label = "Deploy" };
        button.Items.Add(new CdsMenuItem("Deploy to staging"));
        button.Items.Add(new CdsMenuItemDivider());
        button.Items.Add(new CdsMenuItem("Roll back to v2.13.0") { Kind = CdsMenuItemKind.Danger });
        Assert.Equal(CdsSize.Lg, button.Size);
        Assert.Equal("Additional actions", button.TriggerDescription);
        Assert.Same(button.Items, button.Menu.Items);
        button.Size = CdsSize.Md;
        button.MenuAlignment = CdsMenuAlignment.BottomEnd;
        button.IsEnabled = false;
        button.IsEnabled = true;
        button.IsOpen = true;
        button.IsOpen = false;
    }

    [Fact]
    public void A_command_that_cannot_execute_disables_the_control()
    {
        var button = new CdsComboButton { Command = new Command(() => { }, () => false) };
        Assert.False(button.IsEnabled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Focus_leaving_either_half_closes_the_menu(int half)
    {
        var combo = new CdsComboButton { Label = "Deploy" };
        combo.Items.Add(new CdsMenuItem("Deploy to staging"));
        var host = ActionTestHost.Show(combo);
        var hit = ((Layout)combo.Content).Children.OfType<CdsActionFace>().ElementAt(half).Hit;
        hit.ReportFocus(true, fromKeyboard: true);
        combo.IsOpen = true;
        Assert.True(combo.Menu.IsOpen);
        hit.ReportFocus(false, fromKeyboard: true);
        Assert.False(combo.Menu.IsOpen);
        Assert.False(combo.IsOpen);
        Assert.Null(host.TopOverlay);
    }
}
