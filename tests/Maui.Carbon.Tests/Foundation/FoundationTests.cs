namespace Maui.Carbon.Tests.Foundation;

public class PlacementResolverTests
{
    static readonly Size Host = new(1000, 800);

    [Fact]
    public void Places_below_start_aligned()
    {
        var r = CdsPlacementResolver.Resolve(new Rect(100, 100, 80, 40), new Size(200, 120), Host, CdsPlacement.BottomStart, 4);
        Assert.Equal(CdsPlacement.BottomStart, r.Placement);
        Assert.Equal(new Rect(100, 144, 200, 120), r.Frame);
        Assert.Equal(40, r.CaretOffset);
    }

    [Fact]
    public void Flips_to_top_when_bottom_does_not_fit()
    {
        var r = CdsPlacementResolver.Resolve(new Rect(100, 700, 80, 40), new Size(200, 120), Host, CdsPlacement.BottomStart, 4);
        Assert.Equal(CdsPlacement.TopStart, r.Placement);
        Assert.Equal(700 - 4 - 120, r.Frame.Y);
    }

    [Fact]
    public void Does_not_flip_when_the_other_side_is_worse()
    {
        var r = CdsPlacementResolver.Resolve(new Rect(100, 20, 80, 700), new Size(200, 120), Host, CdsPlacement.Bottom, 0, autoFlip: true);
        Assert.Equal(CdsPlacement.Bottom, r.Placement);
    }

    [Fact]
    public void Clamps_into_the_host_horizontally()
    {
        var r = CdsPlacementResolver.Resolve(new Rect(950, 100, 40, 40), new Size(300, 100), Host, CdsPlacement.BottomStart);
        Assert.Equal(Host.Width - 300 - CdsPlacementResolver.EdgeMargin, r.Frame.X);
        Assert.Equal(970 - r.Frame.X, r.CaretOffset);
    }

    [Theory]
    [InlineData(CdsPlacement.Top, CdsPlacement.Bottom)]
    [InlineData(CdsPlacement.LeftBottom, CdsPlacement.RightBottom)]
    [InlineData(CdsPlacement.RightTop, CdsPlacement.LeftTop)]
    public void Flip_keeps_alignment(CdsPlacement from, CdsPlacement to) => Assert.Equal(to, CdsPlacementResolver.Flip(from));

    [Fact]
    public void Right_placement_centers_vertically()
    {
        var r = CdsPlacementResolver.Resolve(new Rect(100, 300, 40, 40), new Size(100, 60), Host, CdsPlacement.Right, 10);
        Assert.Equal(new Rect(150, 290, 100, 60), r.Frame);
        Assert.Equal(30, r.CaretOffset);
    }
}

public class ButtonTokenTests
{
    [Fact]
    public void Primary_states()
    {
        Assert.Equal(CdsColorKeys.ButtonPrimary, CdsButtonTokens.Resolve(CdsButtonKind.Primary, CdsInteractionState.None).Background);
        Assert.Equal(CdsColorKeys.ButtonPrimaryHover, CdsButtonTokens.Resolve(CdsButtonKind.Primary, CdsInteractionState.Hover).Background);
        Assert.Equal(CdsColorKeys.ButtonPrimaryActive, CdsButtonTokens.Resolve(CdsButtonKind.Primary, CdsInteractionState.Hover | CdsInteractionState.Pressed).Background);
        Assert.Equal(CdsColorKeys.TextOnColor, CdsButtonTokens.Resolve(CdsButtonKind.Primary, CdsInteractionState.None).Foreground);
    }

    [Theory]
    [InlineData(CdsButtonKind.Primary)]
    [InlineData(CdsButtonKind.Secondary)]
    [InlineData(CdsButtonKind.Danger)]
    public void Disabled_filled_buttons_use_button_disabled(CdsButtonKind kind)
    {
        var v = CdsButtonTokens.Resolve(kind, CdsInteractionState.Disabled | CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.ButtonDisabled, v.Background);
        Assert.Equal(CdsColorKeys.TextOnColorDisabled, v.Foreground);
    }

    [Fact]
    public void Disabled_ghost_is_transparent_with_text_disabled()
    {
        var v = CdsButtonTokens.Resolve(CdsButtonKind.Ghost, CdsInteractionState.Disabled);
        Assert.Null(v.Background);
        Assert.Equal(CdsColorKeys.TextDisabled, v.Foreground);
    }

    [Fact]
    public void Tertiary_inverts_on_hover()
    {
        var rest = CdsButtonTokens.Resolve(CdsButtonKind.Tertiary, CdsInteractionState.None);
        Assert.Null(rest.Background);
        Assert.Equal(CdsColorKeys.ButtonTertiary, rest.Border);
        var hover = CdsButtonTokens.Resolve(CdsButtonKind.Tertiary, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.ButtonTertiaryHover, hover.Background);
        Assert.Equal(CdsColorKeys.TextInverse, hover.Foreground);
    }

    [Fact]
    public void Selected_ghost_uses_background_selected()
    {
        Assert.Equal(CdsColorKeys.BackgroundSelected, CdsButtonTokens.Resolve(CdsButtonKind.Ghost, CdsInteractionState.Selected).Background);
        Assert.Equal(CdsColorKeys.BackgroundSelectedHover, CdsButtonTokens.Resolve(CdsButtonKind.Ghost, CdsInteractionState.Selected | CdsInteractionState.Hover).Background);
    }

    [Fact]
    public void Ghosts_get_an_inset_ring_others_the_button_ring()
    {
        Assert.Equal(CdsFocusRingStyle.Inset, CdsButtonTokens.FocusRing(CdsButtonKind.Ghost));
        Assert.Equal(CdsFocusRingStyle.Button, CdsButtonTokens.FocusRing(CdsButtonKind.Primary));
    }
}

public class TagTokenTests
{
    [Fact]
    public void Each_color_pairs_background_and_text()
    {
        var v = CdsTagTokens.Resolve(CdsTagType.Purple, disabled: false);
        Assert.Equal(CdsColorKeys.TagBackgroundPurple, v.Background);
        Assert.Equal(CdsColorKeys.TagColorPurple, v.Foreground);
        Assert.Equal(CdsColorKeys.TagHoverPurple, v.Hover);
    }

    [Fact]
    public void Disabled_and_outline()
    {
        Assert.Equal(CdsColorKeys.TextDisabled, CdsTagTokens.Resolve(CdsTagType.Red, disabled: true).Foreground);
        var outline = CdsTagTokens.Resolve(CdsTagType.Outline, disabled: false);
        Assert.Null(outline.Background);
        Assert.Equal(CdsColorKeys.BackgroundInverse, outline.Border);
    }

    [Fact]
    public void Heights() => Assert.Equal([18d, 24d, 32d], [CdsTagTokens.Height(CdsTagSize.Sm), CdsTagTokens.Height(CdsTagSize.Md), CdsTagTokens.Height(CdsTagSize.Lg)]);
}

public class ControlTests
{
    [Fact]
    public void Controls_construct_without_a_platform()
    {
        _ = new CdsButton("Create cluster") { Icon = CdsIconName.Add, Size = CdsSize.Md };
        _ = new CdsButton { IsIconOnly = true, Icon = CdsIconName.Copy, IconDescription = "Copy", Kind = CdsButtonKind.Ghost, IsSelected = true };
        _ = new CdsTag("us-south", CdsTagType.Blue) { Size = CdsTagSize.Lg, Icon = CdsIconName.AiGenerate };
        _ = new CdsTextInput { Label = "Name", IsInvalid = true, InvalidText = "Required", EnableCounter = true, MaxCount = 10, IsInline = true };
        _ = new CdsPopover { Body = new CdsButton("Filter"), PopoverContent = new Label() };
        _ = new CdsTooltip { Text = "Download", Body = new CdsButton { IsIconOnly = true, Icon = CdsIconName.Download } };
        _ = new CdsIcon(CdsIconName.WarningFilled, 20);
    }

    [Fact]
    public void Content_cannot_be_replaced()
    {
        var button = new CdsButton("Save");
        Assert.Throws<InvalidOperationException>(() => button.Content = new Label());
    }

    [Fact]
    public void Text_input_round_trips_text()
    {
        var input = new CdsTextInput { Text = "prod-eu-1" };
        Assert.Equal("prod-eu-1", input.Text);
    }

    [Fact]
    public void Command_can_execute_drives_is_enabled()
    {
        var canExecute = false;
        var command = new Command(() => { }, () => canExecute);
        var button = new CdsButton("Save") { Command = command };
        Assert.False(button.IsEnabled);
        canExecute = true;
        command.ChangeCanExecute();
        Assert.True(button.IsEnabled);
    }

    [Fact]
    public void Icon_glyph_selection_prefers_exact_artboard()
    {
        Assert.Equal(16, CdsIcons.GetGlyph(CdsIconName.ArrowRight, 16).ViewBox.Width);
        Assert.Equal(20, CdsIcons.GetGlyph(CdsIconName.ArrowRight, 20).ViewBox.Width);
        Assert.True(CdsIcons.TryGetName("error--filled", out var icon));
        Assert.Equal(CdsIconName.ErrorFilled, icon);
        Assert.Equal("overflow-menu--vertical", CdsIcons.GetStem(CdsIconName.OverflowMenuVertical));
    }
}
