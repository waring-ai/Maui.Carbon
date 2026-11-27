namespace Maui.Carbon.Tests.Components;

public class CreateModalTests
{
    [Fact]
    public void Constructs_and_takes_every_property()
    {
        var modal = new CdsCreateModal
        {
            Title = "Create partition",
            Subtitle = "Your new partition will be added to topic-1",
            Description = "Partitions are ordered, immutable sequences of messages.",
            Body = new VerticalStackLayout { new CdsTextInput { Label = "Partition name" } },
            PrimaryButtonText = "Create",
            SecondaryButtonText = "Cancel",
            PrimaryButtonDisabled = true,
            RequestSubmitCommand = new Command(() => { }),
        };
        modal.IsInline = true;
        Assert.Equal("Create partition", modal.Header.Title);
        Assert.Equal("Your new partition will be added to topic-1", modal.Header.Subtitle);
        Assert.False(modal.Header.ShowsCloseButton);
        Assert.True(modal.Footer!.PrimaryButtonDisabled);
    }

    [Fact]
    public void Disabled_submit_raises_nothing()
    {
        var modal = new CdsCreateModal { PrimaryButtonText = "Create", SecondaryButtonText = "Cancel", PrimaryButtonDisabled = true };
        var submitted = 0;
        modal.RequestSubmit += (_, _) => submitted++;
        var primary = (CdsModalButton)modal.Footer!.Buttons.Last();
        ((IButton)primary.HitTarget).Clicked();
        Assert.Equal(0, submitted);
        modal.PrimaryButtonDisabled = false;
        primary = (CdsModalButton)modal.Footer.Buttons.Last();
        ((IButton)primary.HitTarget).Clicked();
        Assert.Equal(1, submitted);
    }

    [Fact]
    public void The_form_gets_focus_and_a_tap_outside_never_closes()
    {
        var input = new CdsTextInput { Label = "Partition name" };
        var modal = new CdsCreateModal { Title = "Create partition", Body = input, PrimaryButtonText = "Create", SecondaryButtonText = "Cancel" };
        Assert.True(modal.Modal.PreventCloseOnClickOutside);
        Assert.Same(input, modal.BodyPart.FirstFocusable());
    }

    [Theory]
    [InlineData(nameof(CdsProductModalHeaderStyle.Standard), 400, 48 + 0.2 * (400 - 64) - 48)]
    [InlineData(nameof(CdsProductModalHeaderStyle.Standard), 300, 48)]
    [InlineData(nameof(CdsProductModalHeaderStyle.Standard), 720, 48 + 0.2 * (720 - 64) - 48)]
    [InlineData(nameof(CdsProductModalHeaderStyle.About), 500, 100)]
    [InlineData(nameof(CdsProductModalHeaderStyle.Create), 400, 80 + 0.2 * (400 - 16 - 80) - 48)]
    [InlineData(nameof(CdsProductModalHeaderStyle.Create), 200, 40)]
    [InlineData(nameof(CdsProductModalHeaderStyle.Create), 600, 120 + 0.2 * (600 - 16 - 120) - 48)]
    [InlineData(nameof(CdsProductModalHeaderStyle.Import), 400, 64 + 0.2 * (400 - 16 - 64) - 48)]
    [InlineData(nameof(CdsProductModalHeaderStyle.Import), 60, 0)]
    [InlineData(nameof(CdsProductModalHeaderStyle.Import), 800, 144 + 0.2 * (800 - 16 - 144) - 48)]
    public void Header_end_inset_follows_each_modal(string style, double width, double expected) =>
        Assert.Equal(expected, ProductModalModel.HeaderEndInset(Enum.Parse<CdsProductModalHeaderStyle>(style), width), 3);

    [Fact]
    public void Header_end_inset_is_zero_without_a_width()
    {
        Assert.Equal(0, ProductModalModel.HeaderEndInset(CdsProductModalHeaderStyle.Standard, 0));
        Assert.Equal(0, ProductModalModel.HeaderEndInset(CdsProductModalHeaderStyle.About, double.PositiveInfinity));
        Assert.Equal(0, ProductModalModel.HeaderEndInset(CdsProductModalHeaderStyle.Create, double.NaN));
    }

    [Fact]
    public void Paragraph_insets_never_go_negative()
    {
        Assert.Equal(80, ProductModalModel.TwentyPercent(400), 3);
        Assert.Equal(0, ProductModalModel.TwentyPercent(double.PositiveInfinity));
        Assert.Equal(64, ProductModalModel.TwentyPercentLessSpacing05(400), 3);
        Assert.Equal(0, ProductModalModel.TwentyPercentLessSpacing05(50));
        Assert.Equal(0, ProductModalModel.TwentyPercentLessSpacing05(-1));
    }

    [Fact]
    public void End_inset_layout_measures_its_child_narrower()
    {
        var label = new Label { Text = "Partitions are ordered" };
        var box = new CdsEndInset(label, w => 0.2 * w);
        Assert.Equal(80, box.InsetFor(400), 3);
        Assert.Equal(0, box.InsetFor(-10));
        Assert.Equal(10, new CdsEndInset(label, _ => 50).InsetFor(10));
    }

    [Fact]
    public void Tokens_map_the_product_modal_parts()
    {
        Assert.Equal(CdsColorKeys.ButtonPrimary, CdsProductModalTokens.SuccessIcon);
        Assert.Equal(CdsColorKeys.ButtonDangerPrimary, CdsProductModalTokens.ErrorIcon);
        Assert.Equal(CdsColorKeys.IconPrimary, CdsProductModalTokens.InformationIcon);
        Assert.Equal(CdsColorKeys.LayerAccent01, CdsProductModalTokens.CreateHeaderRule);
        Assert.Equal(CdsColorKeys.BorderStrong01, CdsProductModalTokens.AboutLinkDivider);
        Assert.Equal(CdsColorKeys.Layer01, CdsProductModalTokens.AboutFooterFill);
        Assert.Equal(CdsColorKeys.ButtonDangerPrimary, CdsProductModalTokens.MessageIcon(true, true));
        Assert.Equal(CdsColorKeys.ButtonPrimary, CdsProductModalTokens.MessageIcon(true, false));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsProductModalTokens.MessageIcon(false, false));
    }

    [Fact]
    public void Headers_keep_the_composed_modal_close_wiring()
    {
        foreach (var style in Enum.GetValues<CdsProductModalHeaderStyle>())
        {
            var header = new CdsProductModalHeader(style) { Title = "Create partition", Label = "Topic" };
            var clicks = 0;
            header.CloseButtonClicked += (_, _) => clicks++;
            ((IButton)header.CloseTarget).Clicked();
            Assert.Equal(1, clicks);
            Assert.Equal(style is CdsProductModalHeaderStyle.Standard or CdsProductModalHeaderStyle.About, header.ShowsCloseButton);
        }
        var about = new CdsProductModalHeader(CdsProductModalHeaderStyle.About) { Title = "IBM Product name" };
        Assert.Equal("IBM Product name", about.LabelText.Text);
        Assert.Equal(CdsTypeStyle.Heading04, about.LabelText.TypeStyle);
        Assert.False(about.TitleText.IsVisible);
    }
}
