namespace Maui.Carbon.Tests.Theming;

public class TypographyTests
{
    // tokens.json: heading-05 1.25, heading-06/07 1.199, fluid displays 1.19. Plex's natural line height is 1.3, so these
    // need a multiple below 1 to render the token's line box (CF-08).
    [Theory]
    [InlineData(CdsTypeStyle.Heading05, 40)]
    [InlineData(CdsTypeStyle.Heading06, 50.36)]
    [InlineData(CdsTypeStyle.Heading07, 64.75)]
    [InlineData(CdsTypeStyle.FluidDisplay01, 49.98)]
    [InlineData(CdsTypeStyle.FluidHeading05, 40)]
    [InlineData(CdsTypeStyle.FluidQuotation02, 40)]
    public void Styles_tighter_than_plex_render_their_token_line_height(CdsTypeStyle style, double lineBox)
    {
        var spec = CdsTypeSpec.Of(style);
        Assert.InRange(spec.MauiLineHeight, 0.5, 0.999);
        var rendered = spec.FontSize * CdsTypeSpec.NaturalLineHeight * spec.MauiLineHeight;
        Assert.InRange(rendered, lineBox - 0.1, lineBox + 0.1);
    }

    [Theory]
    [InlineData(CdsTypeStyle.BodyCompact01)]
    [InlineData(CdsTypeStyle.Label01)]
    [InlineData(CdsTypeStyle.HeadingCompact01)]
    [InlineData(CdsTypeStyle.Code01)]
    [InlineData(CdsTypeStyle.FluidQuotation01)]
    public void Compact_styles_near_plex_natural_keep_the_natural_height(CdsTypeStyle style) =>
        Assert.Equal(-1, CdsTypeSpec.Of(style).MauiLineHeight);

    [Theory]
    [InlineData(CdsTypeStyle.Body01, 20)]
    [InlineData(CdsTypeStyle.Body02, 24)]
    [InlineData(CdsTypeStyle.Heading03, 28)]
    public void Paragraph_styles_get_their_extra_leading(CdsTypeStyle style, double lineBox)
    {
        var spec = CdsTypeSpec.Of(style);
        Assert.InRange(spec.FontSize * CdsTypeSpec.NaturalLineHeight * spec.MauiLineHeight, lineBox - 0.1, lineBox + 0.1);
    }

    [Fact]
    public void Fluid_styles_step_up_at_the_documented_breakpoints()
    {
        // design/carbon: display-04 reaches 144px at max; the tokens.json usage gives the other steps.
        Assert.Equal(42, CdsTypeSpec.Of(CdsTypeStyle.FluidDisplay04, CdsBreakpoint.Sm).FontSize);
        Assert.Equal(144, CdsTypeSpec.Of(CdsTypeStyle.FluidDisplay04, CdsBreakpoint.Max).FontSize);
        Assert.Equal([32, 36, 42, 48, 60], Sizes(CdsTypeStyle.FluidHeading05));
        Assert.Equal([32, 36, 42, 48, 60], Sizes(CdsTypeStyle.FluidHeading06));
        Assert.Equal([20, 20, 20, 20, 24], Sizes(CdsTypeStyle.FluidHeading03));
        Assert.Equal([28, 28, 28, 32, 32], Sizes(CdsTypeStyle.FluidHeading04));
        Assert.Equal([24, 24, 28, 28, 32], Sizes(CdsTypeStyle.FluidParagraph01));
        Assert.Equal([42, 42, 54, 60, 84], Sizes(CdsTypeStyle.FluidDisplay01));
        Assert.Equal([42, 54, 60, 76, 92], Sizes(CdsTypeStyle.FluidDisplay03));
        Assert.Equal([20, 20, 24, 28, 32], Sizes(CdsTypeStyle.FluidQuotation01));
        Assert.Equal(60, CdsTypeSpec.Of(CdsTypeStyle.FluidQuotation02, CdsBreakpoint.Max).FontSize);
    }

    [Fact]
    public void Fluid_steps_keep_the_font_and_add_the_display_tracking()
    {
        var max = CdsTypeSpec.Of(CdsTypeStyle.FluidDisplay03, CdsBreakpoint.Max);
        Assert.Equal(CdsFonts.SansLight, max.FontFamily);
        Assert.Equal("fluid-display-03", max.TokenName);
        Assert.Equal(-0.96, max.CharacterSpacing);
        Assert.Equal(0, CdsTypeSpec.Of(CdsTypeStyle.FluidDisplay03, CdsBreakpoint.Md).CharacterSpacing);
        Assert.Equal(CdsFonts.SansSemiBold, CdsTypeSpec.Of(CdsTypeStyle.FluidHeading06, CdsBreakpoint.Max).FontFamily);
    }

    [Fact]
    public void Non_fluid_styles_never_step()
    {
        Assert.False(CdsFluidType.IsFluid(CdsTypeStyle.Heading05));
        Assert.True(CdsFluidType.IsFluid(CdsTypeStyle.FluidDisplay04));
        foreach (var bp in Enum.GetValues<CdsBreakpoint>())
            Assert.Equal(CdsTypeSpec.Of(CdsTypeStyle.Heading07), CdsTypeSpec.Of(CdsTypeStyle.Heading07, bp));
    }

    [Fact]
    public void CdsText_follows_the_page_breakpoint_for_fluid_styles()
    {
        var text = new CdsText("Build on Carbon", CdsTypeStyle.FluidDisplay04);
        var page = new ContentView { Content = text };
        Assert.Equal(42, text.FontSize);

        page.Resources[CdsContextKeys.Breakpoint] = CdsBreakpoint.Lg;
        Assert.Equal(92, text.FontSize);
        Assert.Equal(-0.64, text.CharacterSpacing);
        Assert.Equal(CdsTypeSpec.Of(CdsTypeStyle.FluidDisplay04, CdsBreakpoint.Lg).MauiLineHeight, text.LineHeight);

        page.Resources[CdsContextKeys.Breakpoint] = CdsBreakpoint.Max;
        Assert.Equal(144, text.FontSize);

        text.TypeStyle = CdsTypeStyle.Heading05;
        Assert.Equal(32, text.FontSize);
        page.Resources[CdsContextKeys.Breakpoint] = CdsBreakpoint.Md;
        Assert.Equal(32, text.FontSize);
    }

    [Fact]
    public void Plain_labels_take_a_breakpoint_explicitly()
    {
        var label = new Label();
        CdsTypography.Apply(label, CdsTypeStyle.FluidHeading05, CdsBreakpoint.Xlg);
        Assert.Equal(48, label.FontSize);
        CdsTypography.Apply(label, CdsTypeStyle.FluidHeading05);
        Assert.Equal(32, label.FontSize);
    }

    static double[] Sizes(CdsTypeStyle style) =>
        Enum.GetValues<CdsBreakpoint>().Select(bp => CdsTypeSpec.Of(style, bp).FontSize).ToArray();
}
