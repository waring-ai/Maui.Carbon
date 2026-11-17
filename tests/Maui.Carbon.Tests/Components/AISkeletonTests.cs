namespace Maui.Carbon.Tests.Components;

public class AISkeletonModelTests
{
    [Fact]
    public void Loop_is_1250_milliseconds() => Assert.Equal(1250u, AiShimmerModel.LoopMilliseconds);

    [Fact]
    public void Band_slides_from_fully_off_the_start_edge_to_fully_off_the_end_edge()
    {
        Assert.Equal(-1, AiShimmerModel.Translation(0), 6);
        Assert.Equal(1, AiShimmerModel.Translation(0.999999), 3);
        Assert.Equal(0, AiShimmerModel.Translation(0.5), 6);
    }

    [Fact]
    public void Translation_is_monotonic_and_eased()
    {
        var previous = double.NegativeInfinity;
        for (var i = 0; i <= 20; i++)
        {
            var t = AiShimmerModel.Translation(i / 20.0 * 0.999);
            Assert.True(t >= previous);
            previous = t;
        }
        // Ease-in-out: slower than linear at the start.
        Assert.True(AiShimmerModel.Translation(0.1) < -1 + 2 * 0.1);
    }

    [Fact]
    public void Phase_wraps_around_the_loop() =>
        Assert.Equal(AiShimmerModel.Translation(0.25), AiShimmerModel.Translation(1.25), 6);

    [Fact]
    public void Rest_phase_leaves_the_band_in_the_middle() =>
        Assert.Equal(0, AiShimmerModel.Translation(AiShimmerModel.RestPhase), 6);

    [Fact]
    public void Band_is_as_wide_as_text_and_twice_as_wide_for_placeholders_and_icons()
    {
        Assert.Equal(1, AiShimmerModel.BandWidthFactor(false));
        Assert.Equal(2, AiShimmerModel.BandWidthFactor(true));
    }

    [Fact]
    public void Radii_are_16_8_and_2()
    {
        Assert.Equal(16, AiShimmerModel.Radius(AiSkeletonShape.Text));
        Assert.Equal(8, AiShimmerModel.Radius(AiSkeletonShape.Placeholder));
        Assert.Equal(2, AiShimmerModel.Radius(AiSkeletonShape.Icon));
    }
}

public class AISkeletonControlTests
{
    [Fact]
    public void Text_skeleton_defaults_and_options()
    {
        var text = new CdsAISkeletonText();
        Assert.False(text.IsHeading);
        Assert.False(text.IsParagraph);
        Assert.Equal(3, text.LineCount);
        Assert.Equal(100, text.WidthPercent);
        text.IsParagraph = true;
        text.LineCount = 0;
        Assert.Equal(1, text.LineCount);
        text.WidthPercent = 150;
        Assert.Equal(100, text.WidthPercent);
        text.IsHeading = true;
        text.WidthPercent = 60;
        Assert.Equal(60, text.WidthPercent);
    }

    [Fact]
    public void Placeholder_is_100_by_100()
    {
        var placeholder = new CdsAISkeletonPlaceholder();
        Assert.Equal(100, placeholder.WidthRequest);
        Assert.Equal(100, placeholder.HeightRequest);
    }

    [Fact]
    public void Icon_is_16_and_resizes_and_clamps()
    {
        var icon = new CdsAISkeletonIcon();
        Assert.Equal(16, icon.WidthRequest);
        icon.Size = 24;
        Assert.Equal(24, icon.HeightRequest);
        icon.Size = -3;
        Assert.Equal(1, icon.Size);
    }
}
