namespace Maui.Carbon.Tests.Components;

public class SkeletonTextTests
{
    [Fact]
    public void A_single_line_uses_the_width()
    {
        Assert.Equal([1.0], SkeletonTextModel.LineFractions(false, 3, 100));
        Assert.Equal([0.55], SkeletonTextModel.LineFractions(false, 3, 55));
    }

    [Fact]
    public void A_paragraph_cycles_through_the_fixed_widths()
    {
        var lines = SkeletonTextModel.LineFractions(true, 7, 100);
        Assert.Equal([1.0, 0.85, 0.95, 0.75, 0.90, 1.0, 0.85], lines);
    }

    [Fact]
    public void A_width_other_than_100_applies_to_every_paragraph_line()
    {
        Assert.Equal([0.6, 0.6], SkeletonTextModel.LineFractions(true, 2, 60));
    }

    [Fact]
    public void A_paragraph_has_at_least_one_line_and_widths_are_clamped()
    {
        Assert.Single(SkeletonTextModel.LineFractions(true, 0, 100));
        Assert.Equal([1.0], SkeletonTextModel.LineFractions(false, 1, 250));
        Assert.Equal([0.0], SkeletonTextModel.LineFractions(false, 1, -4));
    }

    [Fact]
    public void Line_height_is_16_or_24_for_headings()
    {
        Assert.Equal(16, SkeletonTextModel.LineHeight(false));
        Assert.Equal(24, SkeletonTextModel.LineHeight(true));
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var text = new CdsSkeletonText { IsHeading = true, WidthPercent = 55 };
        text.IsHeading = false;
        text.IsParagraph = true;
        text.LineCount = 0;
        Assert.Equal(1, text.LineCount);
        text.LineCount = 5;
        text.WidthPercent = 400;
        Assert.Equal(100, text.WidthPercent);
    }
}

public class SkeletonPlaceholderTests
{
    [Fact]
    public void Defaults_to_100_by_100()
    {
        var placeholder = new CdsSkeletonPlaceholder();
        Assert.Equal(100, placeholder.WidthRequest);
        Assert.Equal(100, placeholder.HeightRequest);
        placeholder.WidthRequest = 240;
        placeholder.HeightRequest = 120;
    }
}

public class SkeletonIconTests
{
    [Fact]
    public void Defaults_to_16_and_follows_its_size()
    {
        var icon = new CdsSkeletonIcon();
        Assert.Equal(16, icon.WidthRequest);
        Assert.Equal(16, icon.HeightRequest);
        icon.Size = 24;
        Assert.Equal(24, icon.WidthRequest);
        Assert.Equal(24, icon.HeightRequest);
    }
}

public class ShimmerModelTests
{
    [Fact]
    public void The_band_grows_from_the_start_in_the_first_fifth()
    {
        var start = ShimmerModel.At(0);
        Assert.Equal(0, start.Scale);
        Assert.True(start.FromStart);
        Assert.Equal(0.3, start.Opacity, 6);

        var full = ShimmerModel.At(0.20);
        Assert.Equal(1, full.Scale, 6);
        Assert.Equal(1, full.Opacity, 6);
    }

    [Fact]
    public void The_band_leaves_towards_the_end_edge()
    {
        Assert.Equal(1, ShimmerModel.At(0.28).Scale, 6);
        Assert.False(ShimmerModel.At(0.28).FromStart);
        Assert.Equal(0, ShimmerModel.At(0.51).Scale, 6);
        Assert.Equal(0, ShimmerModel.At(0.55).Scale, 6);
    }

    [Fact]
    public void The_band_returns_from_the_end_then_leaves_towards_the_start()
    {
        Assert.Equal(1, ShimmerModel.At(0.82).Scale, 6);
        Assert.False(ShimmerModel.At(0.70).FromStart);
        Assert.True(ShimmerModel.At(0.90).FromStart);
        Assert.Equal(0, ShimmerModel.At(0.96).Scale, 6);
        Assert.Equal(0, ShimmerModel.At(0.99).Scale, 6);
    }

    [Fact]
    public void The_scale_stays_within_zero_and_one_and_the_loop_wraps()
    {
        for (var p = 0.0; p <= 1.0; p += 0.01)
        {
            var (scale, _, opacity) = ShimmerModel.At(p);
            Assert.InRange(scale, 0, 1);
            Assert.InRange(opacity, 0.3 - 1e-9, 1 + 1e-9);
        }
        Assert.Equal(ShimmerModel.At(0.3).Scale, ShimmerModel.At(1.3).Scale, 6);
        Assert.Equal(ShimmerModel.At(0.3).FromStart, ShimmerModel.At(1.3).FromStart);
        Assert.Equal(0.3, ShimmerModel.At(0.9999).Opacity, 2);
    }

    [Fact]
    public void Ease_is_clamped_and_monotonic()
    {
        Assert.Equal(0, ShimmerModel.Ease(-1));
        Assert.Equal(1, ShimmerModel.Ease(2));
        Assert.Equal(0.5, ShimmerModel.Ease(0.5), 6);
        Assert.True(ShimmerModel.Ease(0.3) < ShimmerModel.Ease(0.4));
    }
}
