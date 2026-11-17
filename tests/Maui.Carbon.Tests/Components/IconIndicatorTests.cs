using Microsoft.Maui.Graphics;

namespace Maui.Carbon.Tests.Components;

public class IconIndicatorTests
{
    [Theory]
    [InlineData(CdsIconIndicatorKind.Failed, CdsColorKeys.StatusRed)]
    [InlineData(CdsIconIndicatorKind.CautionMajor, CdsColorKeys.StatusOrange)]
    [InlineData(CdsIconIndicatorKind.CautionMinor, CdsColorKeys.StatusYellow)]
    [InlineData(CdsIconIndicatorKind.Undefined, CdsColorKeys.StatusPurple)]
    [InlineData(CdsIconIndicatorKind.Succeeded, CdsColorKeys.StatusGreen)]
    [InlineData(CdsIconIndicatorKind.Normal, CdsColorKeys.StatusBlue)]
    [InlineData(CdsIconIndicatorKind.InProgress, CdsColorKeys.StatusBlue)]
    [InlineData(CdsIconIndicatorKind.Incomplete, CdsColorKeys.StatusBlue)]
    [InlineData(CdsIconIndicatorKind.NotStarted, CdsColorKeys.StatusGray)]
    [InlineData(CdsIconIndicatorKind.Pending, CdsColorKeys.StatusGray)]
    [InlineData(CdsIconIndicatorKind.Unknown, CdsColorKeys.StatusGray)]
    [InlineData(CdsIconIndicatorKind.Informative, CdsColorKeys.StatusBlue)]
    public void Kind_maps_to_its_status_token(CdsIconIndicatorKind kind, string key) =>
        Assert.Equal(key, CdsIconIndicatorTokens.Color(kind));

    [Fact]
    public void Every_kind_has_a_distinct_enough_shape()
    {
        // Color is never the only cue: kinds that share a color must differ in shape.
        var byColor = Enum.GetValues<CdsIconIndicatorKind>()
            .GroupBy(CdsIconIndicatorTokens.Color)
            .Where(g => g.Count() > 1);
        foreach (var group in byColor)
        {
            var shapes = group.Select(CdsIconIndicatorTokens.Shape).ToList();
            Assert.Equal(shapes.Count, shapes.Distinct().Count());
        }
    }

    [Theory]
    [InlineData(16, CdsTypeStyle.BodyCompact01)]
    [InlineData(20, CdsTypeStyle.BodyCompact02)]
    public void Label_style_follows_the_glyph_size(double size, CdsTypeStyle style) =>
        Assert.Equal(style, CdsIconIndicatorTokens.LabelStyle(size));

    [Fact]
    public void Size_snaps_to_16_or_20()
    {
        var indicator = new CdsIconIndicator(CdsIconIndicatorKind.Failed, "Failed") { Size = 18 };
        Assert.Equal(16, indicator.Size);
        indicator.Size = 20;
        Assert.Equal(20, indicator.Size);
        indicator.Size = 40;
        Assert.Equal(20, indicator.Size);
    }

    [Fact]
    public void Every_kind_can_be_set_without_a_platform()
    {
        var indicator = new CdsIconIndicator { Label = "Deploying (3 of 5 pods)" };
        foreach (var kind in Enum.GetValues<CdsIconIndicatorKind>()) indicator.Kind = kind;
    }

    [Fact]
    public void Thick_polyline_outlines_a_path_as_one_polygon()
    {
        var polygon = CdsStatusDrawing.ThickPolyline([new PointF(0, 0), new PointF(10, 0)], 2);
        Assert.Equal(4, polygon.Count);
        Assert.Equal(1, polygon.Max(p => p.Y), 3);
        Assert.Equal(-1, polygon.Min(p => p.Y), 3);
    }
}
