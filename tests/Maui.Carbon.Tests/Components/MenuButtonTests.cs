namespace Maui.Carbon.Tests.Components;

public class MenuButtonTests
{
    [Theory]
    [InlineData(CdsMenuButtonKind.Primary, CdsButtonKind.Primary)]
    [InlineData(CdsMenuButtonKind.Secondary, CdsButtonKind.Secondary)]
    [InlineData(CdsMenuButtonKind.Tertiary, CdsButtonKind.Tertiary)]
    [InlineData(CdsMenuButtonKind.Ghost, CdsButtonKind.Ghost)]
    public void Kinds_use_the_matching_button_tokens(CdsMenuButtonKind kind, CdsButtonKind button)
    {
        Assert.Equal(button, CdsMenuButtonTokens.ToButtonKind(kind));
        foreach (var state in new[] { CdsInteractionState.None, CdsInteractionState.Hover, CdsInteractionState.Pressed, CdsInteractionState.Disabled })
            Assert.Equal(CdsButtonTokens.Resolve(button, state), CdsMenuButtonTokens.Resolve(kind, state));
    }

    [Fact]
    public void Primary_is_button_primary_tertiary_is_outlined_and_ghost_uses_link_primary()
    {
        Assert.Equal(CdsColorKeys.ButtonPrimary, CdsMenuButtonTokens.Resolve(CdsMenuButtonKind.Primary, CdsInteractionState.None).Background);
        var tertiary = CdsMenuButtonTokens.Resolve(CdsMenuButtonKind.Tertiary, CdsInteractionState.None);
        Assert.Null(tertiary.Background);
        Assert.Equal(CdsColorKeys.ButtonTertiary, tertiary.Border);
        Assert.Equal(CdsColorKeys.LinkPrimary, CdsMenuButtonTokens.Resolve(CdsMenuButtonKind.Ghost, CdsInteractionState.None).Foreground);
    }

    [Fact]
    public void Only_non_ghost_triggers_are_12rem_wide_at_least()
    {
        Assert.Equal(192, CdsMenuButtonTokens.MinimumWidth(CdsMenuButtonKind.Primary));
        Assert.Equal(192, CdsMenuButtonTokens.MinimumWidth(CdsMenuButtonKind.Tertiary));
        Assert.Equal(0, CdsMenuButtonTokens.MinimumWidth(CdsMenuButtonKind.Ghost));
    }

    [Theory]
    [InlineData(CdsSize.Xs, 24)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    public void Height_follows_size(CdsSize size, double height) => Assert.Equal(height, CdsMenuButtonTokens.Height(size));

    [Fact]
    public void Defaults_and_property_setting_do_not_throw()
    {
        var button = new CdsMenuButton { Label = "Actions" };
        button.Items.Add(new CdsMenuItem("Restart cluster"));
        button.Items.Add(new CdsMenuItemDivider());
        button.Items.Add(new CdsMenuItem("Delete cluster") { Kind = CdsMenuItemKind.Danger });
        Assert.Equal(CdsMenuButtonKind.Primary, button.Kind);
        Assert.Equal(CdsSize.Lg, button.Size);
        Assert.Equal(CdsMenuAlignment.Bottom, button.MenuAlignment);
        Assert.Same(button.Items, button.Menu.Items);
        button.Kind = CdsMenuButtonKind.Ghost;
        button.Kind = CdsMenuButtonKind.Tertiary;
        button.Size = CdsSize.Sm;
        button.MenuAlignment = CdsMenuAlignment.TopEnd;
        button.HasMenuBorder = true;
        button.IsEnabled = false;
        button.IsOpen = true;
        button.IsOpen = false;
    }
}
