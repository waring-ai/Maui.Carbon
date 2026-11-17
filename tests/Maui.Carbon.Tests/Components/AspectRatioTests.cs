namespace Maui.Carbon.Tests.Components;

public class AspectRatioTests
{
    [Theory]
    [InlineData(CdsRatio.Ratio1x1, 1d)]
    [InlineData(CdsRatio.Ratio2x1, 2d)]
    [InlineData(CdsRatio.Ratio3x2, 1.5)]
    [InlineData(CdsRatio.Ratio2x3, 2d / 3)]
    [InlineData(CdsRatio.Ratio4x3, 4d / 3)]
    [InlineData(CdsRatio.Ratio16x9, 16d / 9)]
    [InlineData(CdsRatio.Ratio9x16, 9d / 16)]
    [InlineData(CdsRatio.Ratio1x2, 0.5)]
    [InlineData(CdsRatio.Ratio3x4, 0.75)]
    public void Named_ratios(CdsRatio ratio, double expected) => Assert.Equal(expected, CdsAspectRatioModel.Value(ratio), 10);

    [Fact]
    public void Height_follows_width()
    {
        Assert.Equal(225, CdsAspectRatioModel.HeightFor(400, CdsAspectRatioModel.Value(CdsRatio.Ratio16x9)), 10);
        Assert.Equal(300, CdsAspectRatioModel.HeightFor(400, CdsAspectRatioModel.Value(CdsRatio.Ratio4x3)), 10);
        Assert.Equal(400, CdsAspectRatioModel.HeightFor(400, 1));
        Assert.Equal(0, CdsAspectRatioModel.HeightFor(400, 0));
    }

    [Fact]
    public void Custom_ratio_overrides_the_named_one()
    {
        var box = new CdsAspectRatio { Ratio = CdsRatio.Ratio2x1 };
        Assert.Equal(2, box.EffectiveRatio);
        box.CustomRatio = 2.39;
        Assert.Equal(2.39, box.EffectiveRatio);
        box.CustomRatio = 0;
        Assert.Equal(2, box.EffectiveRatio);
        Assert.True(box.IsClippedToBounds);
    }

    [Fact]
    public void Measures_height_from_the_offered_width()
    {
        var box = new CdsAspectRatio { Ratio = CdsRatio.Ratio16x9 };
        box.Add(new BoxView());
        var size = ((ICrossPlatformLayout)box).CrossPlatformMeasure(320, double.PositiveInfinity);
        Assert.Equal(320, size.Width);
        Assert.Equal(180, size.Height, 6);
    }
}
