namespace Maui.Carbon.Tests.Components;

public class StackTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 2)]
    [InlineData(3, 8)]
    [InlineData(5, 16)]
    [InlineData(7, 32)]
    [InlineData(9, 48)]
    [InlineData(13, 160)]
    public void Gap_steps_map_to_the_spacing_scale(int step, double expected) => Assert.Equal(expected, CdsStack.GapForStep(step));

    [Fact]
    public void Steps_are_clamped() => Assert.Equal([0d, 160d], [CdsStack.GapForStep(-4), CdsStack.GapForStep(99)]);

    [Fact]
    public void Every_step_matches_its_spacing_token()
    {
        double[] tokens =
        [
            CdsSpacing.Spacing01, CdsSpacing.Spacing02, CdsSpacing.Spacing03, CdsSpacing.Spacing04, CdsSpacing.Spacing05,
            CdsSpacing.Spacing06, CdsSpacing.Spacing07, CdsSpacing.Spacing08, CdsSpacing.Spacing09, CdsSpacing.Spacing10,
            CdsSpacing.Spacing11, CdsSpacing.Spacing12, CdsSpacing.Spacing13,
        ];
        for (var i = 0; i < tokens.Length; i++) Assert.Equal(tokens[i], CdsStack.GapForStep(i + 1));
    }

    [Fact]
    public void Gap_sets_spacing_and_raw_gap_wins()
    {
        var stack = new CdsStack { Gap = 5 };
        Assert.Equal(16, stack.Spacing);
        stack.GapValue = 56;
        Assert.Equal(56, stack.Spacing);
        stack.GapValue = null;
        Assert.Equal(16, stack.Spacing);
        stack.Gap = 40;
        Assert.Equal(13, stack.Gap);
        Assert.Equal(160, stack.Spacing);
    }

    [Fact]
    public void Defaults_to_vertical_without_a_gap()
    {
        var stack = new CdsStack();
        Assert.Equal(StackOrientation.Vertical, stack.Orientation);
        Assert.Equal(0, stack.Spacing);
        stack.Orientation = StackOrientation.Horizontal;
    }

    [Fact]
    public void Fluid_gap_follows_the_page_width()
    {
        var stack = new CdsStack { Gap = 5, FluidGap = 3 };
        var scope = new ContentView { Content = stack };
        scope.Resources[CdsContextKeys.ViewportWidth] = 1000d;
        Assert.Equal(50, stack.Spacing, 6);
        scope.Resources[CdsContextKeys.ViewportWidth] = 1600d;
        Assert.Equal(80, stack.Spacing, 6);
        stack.FluidGap = 9;
        Assert.Equal(4, stack.FluidGap);
        Assert.Equal(160, stack.Spacing, 6);
        stack.FluidGap = 1;
        Assert.Equal(0, stack.Spacing);
    }

    [Fact]
    public void Raw_gap_beats_fluid_gap_and_clearing_it_restores_the_step()
    {
        var stack = new CdsStack { Gap = 5, FluidGap = 2 };
        var scope = new ContentView { Content = stack };
        scope.Resources[CdsContextKeys.ViewportWidth] = 1000d;
        stack.GapValue = 12;
        Assert.Equal(12, stack.Spacing);
        stack.GapValue = null;
        Assert.Equal(20, stack.Spacing, 6);
        stack.FluidGap = null;
        Assert.Equal(16, stack.Spacing);
    }

    [Fact]
    public void Fluid_gap_without_a_page_width_is_zero() => Assert.Equal(0, new CdsStack { FluidGap = 4 }.Spacing);
}
