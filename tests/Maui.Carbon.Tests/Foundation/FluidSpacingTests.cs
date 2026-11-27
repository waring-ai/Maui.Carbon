namespace Maui.Carbon.Tests.Foundation;

public class FluidSpacingTests
{
    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 0.02)]
    [InlineData(3, 0.05)]
    [InlineData(4, 0.1)]
    [InlineData(9, 0.1)]
    public void Steps_map_to_the_fluid_scale(int step, double fraction) => Assert.Equal(fraction, CdsFluidSpacing.Fraction(step));

    [Fact]
    public void Steps_resolve_against_the_page_width()
    {
        Assert.Equal(60, CdsFluidSpacing.Resolve(3, 1200), 6);
        Assert.Equal(0, CdsFluidSpacing.Resolve(3, double.NaN));
        Assert.Equal(0, CdsFluidSpacing.Resolve(4, 0));
        Assert.Equal(new Thickness(50, 0, 50, 0), CdsFluidSpacing.Resolve(new Thickness(3, 0), 1000));
    }

    [Fact]
    public void Attached_padding_and_margin_follow_the_page_width()
    {
        var border = new Border();
        var label = new Label();
        var stack = new VerticalStackLayout { border, label };
        var scope = new ContentView { Content = stack };
        scope.Resources[CdsContextKeys.ViewportWidth] = 1000d;

        CdsFluidSpacing.SetPadding(stack, new Thickness(2, 0));
        CdsFluidSpacing.SetPadding(border, new Thickness(4));
        CdsFluidSpacing.SetMargin(label, new Thickness(0, 3, 0, 0));
        CdsFluidSpacing.SetPadding(scope, new Thickness(0, 2));
        Assert.Equal(new Thickness(20, 0, 20, 0), stack.Padding);
        Assert.Equal(new Thickness(100), border.Padding);
        Assert.Equal(new Thickness(0, 50, 0, 0), label.Margin);
        Assert.Equal(new Thickness(0, 20, 0, 20), scope.Padding);
        Assert.Equal(new Thickness(2, 0), CdsFluidSpacing.GetPadding(stack));

        scope.Resources[CdsContextKeys.ViewportWidth] = 500d;
        Assert.Equal(new Thickness(10, 0, 10, 0), stack.Padding);
        Assert.Equal(new Thickness(50), border.Padding);
        Assert.Equal(new Thickness(0, 25, 0, 0), label.Margin);
    }

    [Fact]
    public void Without_a_page_width_fluid_space_is_zero()
    {
        var stack = new VerticalStackLayout { Padding = new Thickness(8) };
        CdsFluidSpacing.SetPadding(stack, new Thickness(3));
        Assert.Equal(Thickness.Zero, stack.Padding);
    }
}
