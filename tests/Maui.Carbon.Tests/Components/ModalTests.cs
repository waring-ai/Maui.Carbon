namespace Maui.Carbon.Tests.Components;

public class ModalTests
{
    [Theory]
    [InlineData(CdsModalSize.Md, 500, 1.0)]
    [InlineData(CdsModalSize.Md, 700, 0.84)]
    [InlineData(CdsModalSize.Md, 1100, 0.60)]
    [InlineData(CdsModalSize.Md, 1400, 0.48)]
    [InlineData(CdsModalSize.Xs, 700, 0.48)]
    [InlineData(CdsModalSize.Xs, 1100, 0.32)]
    [InlineData(CdsModalSize.Xs, 1400, 0.24)]
    [InlineData(CdsModalSize.Sm, 700, 0.60)]
    [InlineData(CdsModalSize.Sm, 1100, 0.42)]
    [InlineData(CdsModalSize.Sm, 1400, 0.36)]
    [InlineData(CdsModalSize.Lg, 700, 0.96)]
    [InlineData(CdsModalSize.Lg, 1100, 0.84)]
    [InlineData(CdsModalSize.Lg, 1400, 0.72)]
    public void Width_fraction_follows_the_breakpoints(CdsModalSize size, double viewport, double expected) =>
        Assert.Equal(expected, CdsModalSizing.WidthFraction(size, viewport), 3);

    [Fact]
    public void Breakpoints_are_inclusive_at_the_lower_edge()
    {
        Assert.Equal(0.84, CdsModalSizing.WidthFraction(CdsModalSize.Md, 672), 3);
        Assert.Equal(0.60, CdsModalSizing.WidthFraction(CdsModalSize.Md, 1056), 3);
        Assert.Equal(0.48, CdsModalSizing.WidthFraction(CdsModalSize.Md, 1312), 3);
        Assert.Equal(1.0, CdsModalSizing.WidthFraction(CdsModalSize.Md, 671.9), 3);
    }

    [Theory]
    [InlineData(CdsModalSize.Md, 500, 1.0)]
    [InlineData(CdsModalSize.Md, 800, 0.90)]
    [InlineData(CdsModalSize.Md, 1200, 0.84)]
    [InlineData(CdsModalSize.Xs, 800, 0.90)]
    [InlineData(CdsModalSize.Xs, 1200, 0.48)]
    [InlineData(CdsModalSize.Sm, 1200, 0.72)]
    [InlineData(CdsModalSize.Lg, 1200, 0.96)]
    public void Max_height_fraction(CdsModalSize size, double viewportWidth, double expected) =>
        Assert.Equal(expected, CdsModalSizing.MaxHeightFraction(size, viewportWidth), 3);

    [Fact]
    public void Modal_dimensions_are_fractions_of_the_viewport()
    {
        Assert.Equal(660, CdsModalSizing.ModalWidth(CdsModalSize.Md, 1100), 3);
        Assert.Equal(588, CdsModalSizing.ModalMaxHeight(CdsModalSize.Md, 1100, 700), 3);
    }

    [Fact]
    public void Dialog_width_is_capped_at_48rem_except_lg()
    {
        Assert.Equal(768, CdsModalSizing.DialogWidth(CdsModalSize.Md, 1700), 3);
        Assert.Equal(0.36 * 1700, CdsModalSizing.DialogWidth(CdsModalSize.Sm, 1700), 3);
        Assert.Equal(0.72 * 1700, CdsModalSizing.DialogWidth(CdsModalSize.Lg, 1700), 3);
        Assert.Equal(468, CdsModalSizing.DialogWidth(CdsModalSize.Md, 500), 3);
        Assert.Equal(0, CdsModalSizing.DialogWidth(CdsModalSize.Md, 20), 3);
    }

    [Theory]
    [InlineData(CdsModalSize.Xs, 384)]
    [InlineData(CdsModalSize.Sm, 576)]
    [InlineData(CdsModalSize.Md, 768)]
    [InlineData(CdsModalSize.Lg, 1152)]
    public void Inline_dialog_widths(CdsModalSize size, double expected) =>
        Assert.Equal(expected, CdsModalSizing.DialogInlineWidth(size));

    [Fact]
    public void Dialog_max_height_steps_with_the_viewport()
    {
        Assert.Equal(500, CdsModalSizing.DialogMaxHeight(600, 500), 3);
        Assert.Equal(450, CdsModalSizing.DialogMaxHeight(800, 500), 3);
        Assert.Equal(420, CdsModalSizing.DialogMaxHeight(1200, 500), 3);
    }

    [Fact]
    public void Heading_wraps_at_about_eighty_percent()
    {
        Assert.Equal(0, CdsModalSizing.HeadingEndPadding(300));
        Assert.Equal(0.2 * (720 - 64) - 48, CdsModalSizing.HeadingEndPadding(720), 3);
        Assert.Equal(0, CdsModalSizing.HeadingEndPadding(0));
    }

    [Fact]
    public void Paragraph_end_padding_never_goes_negative()
    {
        Assert.Equal(0, CdsModalSizing.ParagraphEndPadding(100));
        Assert.Equal(0.2 * (800 - 32) - 32, CdsModalSizing.ParagraphEndPadding(800), 3);
    }

    [Fact]
    public void Fade_shows_only_for_overflowing_or_forced_content()
    {
        Assert.False(CdsModalSizing.ShowsFade(200, 300, false));
        Assert.False(CdsModalSizing.ShowsFade(300, 300, false));
        Assert.True(CdsModalSizing.ShowsFade(301, 300, false));
        Assert.True(CdsModalSizing.ShowsFade(100, 300, true));
        Assert.False(CdsModalSizing.ShowsFade(500, 0, false));
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(2, 0, 1)]
    [InlineData(3, 1, 1)]
    public void Footer_columns_give_one_two_and_three_buttons_their_share(int count, double spacer, double button)
    {
        var (s, b) = CdsModalSizing.FooterColumns(count);
        Assert.Equal(spacer, s);
        Assert.Equal(button, b);
    }

    [Fact]
    public void Footer_button_shares_are_half_whole_and_a_quarter()
    {
        static double Share(int n)
        {
            var (s, b) = CdsModalSizing.FooterColumns(n);
            return b / (s + b * n);
        }
        Assert.Equal(0.5, Share(1), 3);
        Assert.Equal(0.5, Share(2), 3);
        Assert.Equal(0.25, Share(3), 3);
    }

    [Fact]
    public void Close_button_background_follows_state()
    {
        Assert.Null(CdsModalTokens.CloseButtonBackground(CdsInteractionState.None));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsModalTokens.CloseButtonBackground(CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.LayerActive01, CdsModalTokens.CloseButtonBackground(CdsInteractionState.Hover | CdsInteractionState.Pressed));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsModalTokens.CloseButtonIcon(CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsModalTokens.CloseButtonIcon(CdsInteractionState.Disabled));
    }

    [Fact]
    public void Danger_footer_uses_the_danger_button()
    {
        Assert.Equal(CdsButtonKind.Danger, CdsModalTokens.PrimaryKind(true));
        Assert.Equal(CdsButtonKind.Primary, CdsModalTokens.PrimaryKind(false));
        Assert.Equal(CdsColorKeys.Layer01, CdsModalTokens.ContainerFill);
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsModalTokens.ContainerBorder);
    }

    [Fact]
    public void Modal_can_be_created_and_configured()
    {
        var modal = new CdsModal
        {
            Heading = "Create API key",
            Label = "Access",
            Size = CdsModalSize.Sm,
            PrimaryButtonText = "Create key",
            SecondaryButtonText = "Cancel",
            TertiaryButtonText = "Save draft",
            IsDanger = true,
            IsPassive = false,
            PreventCloseOnClickOutside = true,
            PrimaryButtonDisabled = true,
            HasScrollingContent = true,
            HasForm = true,
            IsFullWidth = true,
            Body = new Label { Text = "Body" },
        };
        Assert.False(modal.IsOpen);
        Assert.Equal("Close", modal.CloseButtonLabel);
        Assert.Equal(CdsModalSize.Sm, modal.Size);
    }

    [Fact]
    public void Setting_IsOpen_before_the_modal_is_loaded_does_not_throw()
    {
        var modal = new CdsModal { Heading = "Delete bucket" };
        modal.IsOpen = true;
        Assert.True(modal.IsOpen);
        modal.IsOpen = false;
        Assert.False(modal.IsOpen);
    }

    [Fact]
    public void Header_body_and_footer_parts_can_be_created()
    {
        var header = new CdsModalHeader { Label = "Access", Title = "Create API key", IconDescription = "Dismiss" };
        var body = new CdsModalBody { HasForm = true, HasScrollingContent = true, IsFullWidth = true, Body = new Label() };
        var footer = new CdsModalFooter { PrimaryButtonText = "Create", SecondaryButtonText = "Cancel", TertiaryButtonText = "Draft", IsDanger = true, PrimaryButtonDisabled = true };
        Assert.Equal("Dismiss", header.IconDescription);
        Assert.True(footer.IsVisible);
        Assert.NotNull(footer.Content);
        footer.PrimaryButtonText = null;
        footer.SecondaryButtonText = null;
        footer.TertiaryButtonText = null;
        Assert.False(footer.IsVisible);
        Assert.True(body.HasForm);
    }

    [Fact]
    public void Composed_modal_collects_parts()
    {
        var modal = new CdsComposedModal { Size = CdsModalSize.Lg, IsDanger = true, PreventCloseOnClickOutside = true };
        modal.Parts.Add(new CdsModalHeader { Title = "Delete" });
        modal.Parts.Add(new CdsModalBody());
        modal.Parts.Add(new CdsModalFooter { PrimaryButtonText = "Delete" });
        Assert.Equal(3, modal.Parts.Count);
        modal.IsOpen = true;
        modal.IsOpen = false;
    }
}
