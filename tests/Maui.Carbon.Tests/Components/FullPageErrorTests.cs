namespace Maui.Carbon.Tests.Components;

public class FullPageErrorModelTests
{
    [Fact]
    public void Below_md_the_drawing_goes_under_the_text()
    {
        var c = FullPageErrorModel.Columns(CdsBreakpoint.Sm, 400);
        Assert.True(c.Stacked);
        Assert.Equal(400, c.TextWidth);
        Assert.Equal(400, c.ArtWidth);
        Assert.Equal(0, c.ArtX);
        Assert.Equal(0, c.TextEndPadding);
    }

    [Fact]
    public void Md_gives_the_text_3_of_8_columns()
    {
        var c = FullPageErrorModel.Columns(CdsBreakpoint.Md, 800);
        Assert.False(c.Stacked);
        Assert.Equal(300, c.TextWidth);
        Assert.Equal(300, c.ArtX);
        Assert.Equal(500, c.ArtWidth);
        Assert.Equal(0, c.TextEndPadding);
    }

    [Theory]
    [InlineData(CdsBreakpoint.Lg)]
    [InlineData(CdsBreakpoint.Xlg)]
    [InlineData(CdsBreakpoint.Max)]
    public void Lg_and_up_give_the_text_6_of_16_columns_and_32_before_the_drawing(CdsBreakpoint breakpoint)
    {
        var c = FullPageErrorModel.Columns(breakpoint, 1600);
        Assert.Equal(600, c.TextWidth);
        Assert.Equal(1000, c.ArtWidth);
        Assert.Equal(32, c.TextEndPadding);
    }

    [Fact]
    public void Negative_widths_clamp_to_zero() => Assert.Equal(0, FullPageErrorModel.Columns(CdsBreakpoint.Lg, -10).ArtWidth);

    [Fact]
    public void Margins_and_spacing_match_the_stylesheet()
    {
        Assert.Equal(8, FullPageErrorModel.HorizontalMargin(CdsBreakpoint.Sm));
        Assert.Equal(24, FullPageErrorModel.HorizontalMargin(CdsBreakpoint.Md));
        Assert.Equal(24, FullPageErrorModel.HorizontalMargin(CdsBreakpoint.Max));
        Assert.Equal(32, FullPageErrorModel.VerticalPadding);
        Assert.Equal(48, FullPageErrorModel.TitleGap);
        Assert.Equal(32, FullPageErrorModel.DescriptionGap);
        Assert.Equal(new Thickness(24, 8, 24, 80), FullPageErrorModel.ArtPadding);
    }

    [Theory]
    [InlineData(CdsFullPageErrorKind.Forbidden, "FullPageError403")]
    [InlineData(CdsFullPageErrorKind.NotFound, "FullPageError404")]
    [InlineData(CdsFullPageErrorKind.Custom, "FullPageErrorCustom")]
    public void Kinds_pick_their_drawing(CdsFullPageErrorKind kind, string name) => Assert.Equal(name, FullPageErrorModel.Art(kind).Name);

    [Fact]
    public void The_heading_reads_the_label_then_the_title()
    {
        Assert.Equal("Error 403. Access denied", FullPageErrorModel.HeadingDescription("Error 403", "Access denied"));
        Assert.Equal("Access denied", FullPageErrorModel.HeadingDescription(null, "Access denied"));
        Assert.Equal("", FullPageErrorModel.HeadingDescription(" ", null));
    }
}

public class FullPageErrorTests
{
    [Fact]
    public void Defaults_to_the_custom_drawing()
    {
        var error = new CdsFullPageError();
        Assert.Equal(CdsFullPageErrorKind.Custom, error.Kind);
        Assert.Equal("FullPageErrorCustom", error.CurrentArt!.Name);
        Assert.False(error.LabelText.IsVisible);
    }

    [Fact]
    public void Every_property_can_be_set_without_a_platform()
    {
        var body = new CdsLink("Go to the dashboard");
        var error = new CdsFullPageError
        {
            Kind = CdsFullPageErrorKind.Forbidden,
            Label = "Error 403",
            Title = "Access denied",
            Description = "Contact your administrator to request access.",
            Body = body,
        };
        Assert.Equal("FullPageError403", error.CurrentArt!.Name);
        Assert.Equal(FullPageErrorModel.LabelArrow + "Error 403", error.LabelText.Text);
        Assert.Equal(CdsTextTone.Error, error.LabelText.Tone);
        Assert.Equal(CdsTypeStyle.FluidHeading05, error.TitleLabel.TypeStyle);
        Assert.Equal(SemanticHeadingLevel.Level1, SemanticProperties.GetHeadingLevel(error.TitleLabel));
        Assert.Equal("Error 403. Access denied", SemanticProperties.GetDescription(error.TitleLabel));
        Assert.False(AutomationProperties.GetIsInAccessibleTree(error.LabelText) ?? true);
        error.Kind = CdsFullPageErrorKind.NotFound;
        Assert.Equal("FullPageError404", error.CurrentArt!.Name);
        error.Body = null;
    }
}
