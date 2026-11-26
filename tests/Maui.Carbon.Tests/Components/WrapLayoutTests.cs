using Maui.Carbon.Gallery.Infrastructure;
using Microsoft.Maui.Layouts;

namespace Maui.Carbon.Tests.Components;

public class WrapLayoutTests
{
    // A view with a fixed size and no platform handler; its desired size includes its margins, as MAUI's do.
    sealed class Fixed : View
    {
        protected override Size MeasureOverride(double widthConstraint, double heightConstraint) =>
            new(WidthRequest + Margin.HorizontalThickness, HeightRequest + Margin.VerticalThickness);
    }

    static Fixed Box(double w, double h, Thickness margin) => new() { WidthRequest = w, HeightRequest = h, Margin = margin };

    static void Layout(WrapLayout layout, double width)
    {
        var size = ((ICrossPlatformLayout)layout).CrossPlatformMeasure(width, double.PositiveInfinity);
        ((ICrossPlatformLayout)layout).CrossPlatformArrange(new Rect(0, 0, width, size.Height));
    }

    [Fact]
    public void Margins_count_once_and_rows_are_one_margin_apart()
    {
        var layout = new WrapLayout { AlignItems = FlexAlignItems.End };
        var boxes = new[] { Box(160, 160, new(0, 0, 24, 24)), Box(160, 80, new(0, 0, 24, 24)), Box(160, 90, new(0, 0, 24, 24)) };
        foreach (var b in boxes) layout.Add(b);
        var size = ((ICrossPlatformLayout)layout).CrossPlatformMeasure(400, double.PositiveInfinity);
        Assert.Equal(184 + 114, size.Height);
        Layout(layout, 400);
        Assert.Equal(new Rect(0, 0, 160, 160), boxes[0].Frame);
        Assert.Equal(new Rect(184, 80, 160, 80), boxes[1].Frame);
        Assert.Equal(new Rect(0, 184, 160, 90), boxes[2].Frame);
    }

    [Fact]
    public void Justify_and_align_follow_flex_layout()
    {
        var layout = new WrapLayout { JustifyContent = FlexJustify.Center, AlignItems = FlexAlignItems.Center };
        var a = Box(100, 40, new(0, 0, 0, 0));
        var b = Box(100, 20, new(0, 0, 0, 0));
        layout.Add(a); layout.Add(b);
        Layout(layout, 400);
        Assert.Equal(new Rect(100, 0, 100, 40), a.Frame);
        Assert.Equal(new Rect(200, 10, 100, 20), b.Frame);
    }
}
