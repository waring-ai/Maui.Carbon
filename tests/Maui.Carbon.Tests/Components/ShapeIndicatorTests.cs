namespace Maui.Carbon.Tests.Components;

public class ShapeIndicatorTests
{
    [Theory]
    [InlineData(CdsShapeIndicatorKind.Failed, CdsColorKeys.StatusRed, null)]
    [InlineData(CdsShapeIndicatorKind.Critical, CdsColorKeys.StatusRed, null)]
    [InlineData(CdsShapeIndicatorKind.High, CdsColorKeys.StatusRed, null)]
    [InlineData(CdsShapeIndicatorKind.Medium, CdsColorKeys.StatusOrange, CdsColorKeys.StatusOrangeOutline)]
    [InlineData(CdsShapeIndicatorKind.Low, CdsColorKeys.StatusYellow, CdsColorKeys.StatusYellowOutline)]
    [InlineData(CdsShapeIndicatorKind.Cautious, CdsColorKeys.StatusYellow, CdsColorKeys.StatusYellowOutline)]
    [InlineData(CdsShapeIndicatorKind.Undefined, CdsColorKeys.StatusPurple, null)]
    [InlineData(CdsShapeIndicatorKind.Stable, CdsColorKeys.StatusGreen, null)]
    [InlineData(CdsShapeIndicatorKind.Informative, CdsColorKeys.StatusBlue, null)]
    [InlineData(CdsShapeIndicatorKind.Incomplete, CdsColorKeys.StatusBlue, null)]
    [InlineData(CdsShapeIndicatorKind.Draft, CdsColorKeys.StatusGray, null)]
    public void Kind_maps_to_fill_and_outline(CdsShapeIndicatorKind kind, string fill, string? outline)
    {
        var v = CdsShapeIndicatorTokens.Resolve(kind);
        Assert.Equal(fill, v.Fill);
        Assert.Equal(outline, v.Outline);
    }

    [Fact]
    public void Severity_levels_have_different_shapes()
    {
        var shapes = new[] { CdsShapeIndicatorKind.Failed, CdsShapeIndicatorKind.Critical, CdsShapeIndicatorKind.High }
            .Select(CdsShapeIndicatorTokens.Shape).ToList();
        Assert.Equal(3, shapes.Distinct().Count());
    }

    [Theory]
    [InlineData(12, CdsTypeStyle.HelperText01)]
    [InlineData(14, CdsTypeStyle.BodyCompact01)]
    public void Label_style_follows_the_text_size(int size, CdsTypeStyle style) =>
        Assert.Equal(style, CdsShapeIndicatorTokens.LabelStyle(size));

    [Fact]
    public void Text_size_snaps_to_12_or_14()
    {
        var indicator = new CdsShapeIndicator(CdsShapeIndicatorKind.Critical, "Critical") { TextSize = 13 };
        Assert.Equal(12, indicator.TextSize);
        indicator.TextSize = 14;
        Assert.Equal(14, indicator.TextSize);
    }

    [Fact]
    public void Every_kind_can_be_set_without_a_platform()
    {
        var indicator = new CdsShapeIndicator { Label = "Stable" };
        foreach (var kind in Enum.GetValues<CdsShapeIndicatorKind>()) indicator.Kind = kind;
    }
}
