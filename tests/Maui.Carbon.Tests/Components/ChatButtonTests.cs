namespace Maui.Carbon.Tests.Components;

public class ChatButtonTokenTests
{
    [Theory]
    [InlineData(CdsSize.Sm, 16)]
    [InlineData(CdsSize.Md, 20)]
    [InlineData(CdsSize.Lg, 24)]
    [InlineData(CdsSize.Xl, 24)]
    [InlineData(CdsSize.Xs, 24)]
    public void Pill_radius_by_size(CdsSize size, double radius) => Assert.Equal(radius, CdsChatButtonTokens.Radius(size));

    [Theory]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xxl, 48)]
    public void Height_falls_back_to_lg(CdsSize size, double height) => Assert.Equal(height, CdsChatButtonTokens.Height(size));

    [Fact]
    public void Quick_action_is_always_a_small_ghost()
    {
        Assert.Equal((CdsChatButtonKind.Ghost, CdsSize.Sm), CdsChatButtonTokens.Effective(CdsChatButtonKind.Primary, CdsSize.Lg, true));
        Assert.Equal((CdsChatButtonKind.Danger, CdsSize.Md), CdsChatButtonTokens.Effective(CdsChatButtonKind.Danger, CdsSize.Md, false));
        Assert.Equal((CdsChatButtonKind.Primary, CdsSize.Lg), CdsChatButtonTokens.Effective(CdsChatButtonKind.Primary, CdsSize.Xl, false));
    }

    [Fact]
    public void Label_end_padding_is_15_without_an_icon()
    {
        Assert.Equal(15, CdsChatButtonTokens.LabelEndPadding(false, false));
        Assert.Equal(63, CdsChatButtonTokens.LabelEndPadding(true, false));
        Assert.Equal(15, CdsChatButtonTokens.LabelEndPadding(true, true));
    }

    [Theory]
    [InlineData(CdsChatButtonKind.Primary, CdsButtonKind.Primary)]
    [InlineData(CdsChatButtonKind.Secondary, CdsButtonKind.Secondary)]
    [InlineData(CdsChatButtonKind.Tertiary, CdsButtonKind.Tertiary)]
    [InlineData(CdsChatButtonKind.Ghost, CdsButtonKind.Ghost)]
    [InlineData(CdsChatButtonKind.Danger, CdsButtonKind.Danger)]
    public void Regular_kinds_use_button_colors(CdsChatButtonKind kind, CdsButtonKind button)
    {
        Assert.Equal(CdsButtonTokens.Resolve(button, CdsInteractionState.Hover), CdsChatButtonTokens.Resolve(kind, CdsInteractionState.Hover, false, false));
        Assert.Equal(CdsButtonTokens.Resolve(button, CdsInteractionState.Disabled), CdsChatButtonTokens.Resolve(kind, CdsInteractionState.Disabled, false, false));
    }

    [Fact]
    public void Quick_action_is_a_link_primary_outline()
    {
        var rest = CdsChatButtonTokens.Resolve(CdsChatButtonKind.Ghost, CdsInteractionState.None, true, false);
        Assert.Null(rest.Background);
        Assert.Equal(CdsColorKeys.LinkPrimary, rest.Border);
        Assert.Equal(CdsColorKeys.LinkPrimary, rest.Foreground);
    }

    [Fact]
    public void Quick_action_hover_and_pressed_drop_the_border_and_fill()
    {
        var hover = CdsChatButtonTokens.Resolve(CdsChatButtonKind.Ghost, CdsInteractionState.Hover, true, false);
        Assert.Null(hover.Border);
        Assert.Equal(CdsColorKeys.BackgroundHover, hover.Background);
        Assert.Equal(CdsColorKeys.LinkPrimaryHover, hover.Foreground);
        var pressed = CdsChatButtonTokens.Resolve(CdsChatButtonKind.Ghost, CdsInteractionState.Hover | CdsInteractionState.Pressed, true, false);
        Assert.Null(pressed.Border);
        Assert.Equal(CdsColorKeys.BackgroundActive, pressed.Background);
        Assert.Equal(CdsColorKeys.LinkPrimaryHover, pressed.Foreground);
    }

    [Fact]
    public void Quick_action_focus_is_a_focus_border()
    {
        var focus = CdsChatButtonTokens.Resolve(CdsChatButtonKind.Ghost, CdsInteractionState.FocusVisible, true, false);
        Assert.Equal(CdsColorKeys.Focus, focus.Border);
    }

    [Fact]
    public void Quick_action_disabled_uses_button_disabled_for_border_and_text()
    {
        var disabled = CdsChatButtonTokens.Resolve(CdsChatButtonKind.Ghost, CdsInteractionState.Disabled, true, false);
        Assert.Equal(CdsColorKeys.ButtonDisabled, disabled.Border);
        Assert.Equal(CdsColorKeys.ButtonDisabled, disabled.Foreground);
        Assert.Null(disabled.Background);
    }

    [Theory]
    [InlineData(CdsInteractionState.None)]
    [InlineData(CdsInteractionState.Hover)]
    [InlineData(CdsInteractionState.Hover | CdsInteractionState.Pressed)]
    [InlineData(CdsInteractionState.Disabled)]
    public void Selected_quick_action_has_no_border_and_a_selected_fill(CdsInteractionState state)
    {
        var selected = CdsChatButtonTokens.Resolve(CdsChatButtonKind.Ghost, state, true, true);
        Assert.Null(selected.Border);
        Assert.Equal(CdsColorKeys.BackgroundSelected, selected.Background);
        Assert.Equal(CdsColorKeys.TextSecondary, selected.Foreground);
    }

    [Fact]
    public void Focus_ring_is_inset_for_quick_actions_and_follows_the_kind_otherwise()
    {
        Assert.Equal(CdsFocusRingStyle.Inset, CdsChatButtonTokens.FocusRing(CdsChatButtonKind.Primary, true));
        Assert.Equal(CdsFocusRingStyle.Button, CdsChatButtonTokens.FocusRing(CdsChatButtonKind.Primary, false));
        Assert.Equal(CdsFocusRingStyle.Inset, CdsChatButtonTokens.FocusRing(CdsChatButtonKind.Ghost, false));
    }
}

public class ChatButtonControlTests
{
    [Fact]
    public void Defaults_are_a_large_primary_button()
    {
        var button = new CdsChatButton();
        Assert.Equal(CdsChatButtonKind.Primary, button.Kind);
        Assert.Equal(CdsSize.Lg, button.Size);
        Assert.False(button.IsQuickAction);
        Assert.False(button.IsSelected);
    }

    [Fact]
    public void Unsupported_sizes_fall_back_to_lg()
    {
        var button = new CdsChatButton { Size = CdsSize.Xl };
        Assert.Equal(CdsSize.Lg, button.Size);
        button.Size = CdsSize.Sm;
        Assert.Equal(CdsSize.Sm, button.Size);
        button.Size = CdsSize.Xs;
        Assert.Equal(CdsSize.Lg, button.Size);
    }

    [Fact]
    public void Every_kind_size_and_icon_can_be_set()
    {
        foreach (var kind in Enum.GetValues<CdsChatButtonKind>())
            foreach (var size in new[] { CdsSize.Sm, CdsSize.Md, CdsSize.Lg })
            {
                var button = new CdsChatButton("Send", kind) { Size = size, Icon = CdsIconName.ArrowUp };
                button.Icon = null;
                button.IsEnabled = false;
            }
    }

    [Fact]
    public void Quick_action_can_be_selected_and_clicked_through_a_command()
    {
        var ran = 0;
        var button = new CdsChatButton("Show nodes not ready") { IsQuickAction = true, IsSelected = true, Command = new Command(() => ran++) };
        Assert.True(button.IsQuickAction);
        Assert.True(button.IsSelected);
        Assert.Equal(0, ran);
    }

    [Fact]
    public void Command_can_execute_drives_is_enabled()
    {
        var canRun = false;
        var command = new Command(() => { }, () => canRun);
        var button = new CdsChatButton("Send") { Command = command };
        Assert.False(button.IsEnabled);
        canRun = true;
        command.ChangeCanExecute();
        Assert.True(button.IsEnabled);
    }
}
