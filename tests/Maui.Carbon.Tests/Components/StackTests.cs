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
}
