namespace Maui.Carbon.Tests.Components;

public class ProgressBarTests
{
    [Theory]
    [InlineData(0, 100, 0)]
    [InlineData(50, 100, 0.5)]
    [InlineData(100, 100, 1)]
    [InlineData(150, 100, 1)]
    [InlineData(-10, 100, 0)]
    [InlineData(5, 20, 0.25)]
    public void Active_fraction_is_value_over_max_clamped(double value, double max, double expected) =>
        Assert.Equal(expected, ProgressBarModel.Fraction(value, max, CdsProgressStatus.Active), 6);

    [Fact]
    public void Indeterminate_and_degenerate_values_are_empty()
    {
        Assert.Equal(0, ProgressBarModel.Fraction(null, 100, CdsProgressStatus.Active));
        Assert.Equal(0, ProgressBarModel.Fraction(10, 0, CdsProgressStatus.Active));
        Assert.Equal(0, ProgressBarModel.Fraction(double.NaN, 100, CdsProgressStatus.Active));
    }

    [Theory]
    [InlineData(CdsProgressStatus.Finished)]
    [InlineData(CdsProgressStatus.Error)]
    public void Finished_and_error_are_always_full(CdsProgressStatus status)
    {
        Assert.Equal(1, ProgressBarModel.Fraction(10, 100, status));
        Assert.Equal(1, ProgressBarModel.Fraction(null, 100, status));
    }

    [Fact]
    public void Only_an_active_bar_without_a_value_is_indeterminate()
    {
        Assert.True(ProgressBarModel.IsIndeterminate(null, CdsProgressStatus.Active));
        Assert.False(ProgressBarModel.IsIndeterminate(0, CdsProgressStatus.Active));
        Assert.False(ProgressBarModel.IsIndeterminate(null, CdsProgressStatus.Finished));
        Assert.False(ProgressBarModel.IsIndeterminate(null, CdsProgressStatus.Error));
    }

    [Fact]
    public void The_sweep_enters_from_the_left_leaves_on_the_right_and_pauses()
    {
        var (x0, w0, v0) = ProgressBarModel.IndeterminateBlock(0, 400);
        Assert.Equal(100, w0);
        Assert.Equal(-100, x0);
        Assert.True(v0);

        var (xMid, _, _) = ProgressBarModel.IndeterminateBlock(0.4, 400);
        Assert.Equal(-100 + 0.5 * 500, xMid, 6);

        var (xEnd, _, vEnd) = ProgressBarModel.IndeterminateBlock(0.8, 400);
        Assert.False(vEnd);
        Assert.Equal(400, xEnd);

        Assert.False(ProgressBarModel.IndeterminateBlock(0.95, 400).Visible);
        Assert.Equal(ProgressBarModel.IndeterminateBlock(0.25, 400), ProgressBarModel.IndeterminateBlock(1.25, 400));
    }

    [Fact]
    public void Description_carries_label_value_and_status()
    {
        Assert.Equal("Uploading usage-march.csv, 42 percent", ProgressBarModel.Describe("Uploading usage-march.csv", 42, 100, CdsProgressStatus.Active));
        Assert.Equal("Uploading, loading", ProgressBarModel.Describe("Uploading", null, 100, CdsProgressStatus.Active));
        Assert.Equal("Uploading, done", ProgressBarModel.Describe("Uploading", 100, 100, CdsProgressStatus.Finished));
        Assert.Equal("Uploading, error", ProgressBarModel.Describe("Uploading", 10, 100, CdsProgressStatus.Error));
        Assert.Equal("loading", ProgressBarModel.Describe(null, null, 100, CdsProgressStatus.Active));
    }

    [Fact]
    public void Status_maps_to_tokens()
    {
        var active = CdsProgressBarTokens.Resolve(CdsProgressStatus.Active);
        Assert.Equal(CdsColorKeys.Interactive, active.Fill);
        Assert.Null(active.Icon);
        Assert.Equal(CdsColorKeys.TextSecondary, active.HelperText);

        var finished = CdsProgressBarTokens.Resolve(CdsProgressStatus.Finished);
        Assert.Equal(CdsColorKeys.SupportSuccess, finished.Fill);
        Assert.Equal(CdsColorKeys.SupportSuccess, finished.Icon);

        var error = CdsProgressBarTokens.Resolve(CdsProgressStatus.Error);
        Assert.Equal(CdsColorKeys.SupportError, error.Fill);
        Assert.Equal(CdsColorKeys.SupportError, error.Icon);
        Assert.Equal(CdsColorKeys.TextError, error.HelperText);
    }

    [Fact]
    public void Track_height_follows_the_size()
    {
        Assert.Equal(8, CdsProgressBarTokens.TrackHeight(CdsProgressBarSize.Big));
        Assert.Equal(4, CdsProgressBarTokens.TrackHeight(CdsProgressBarSize.Small));
    }

    [Fact]
    public void Every_combination_can_be_set_without_a_platform()
    {
        var bar = new CdsProgressBar("Uploading usage-march.csv", 42) { HelperText = "42 of 100 MB" };
        foreach (var type in Enum.GetValues<CdsProgressBarType>())
        foreach (var size in Enum.GetValues<CdsProgressBarSize>())
        foreach (var status in Enum.GetValues<CdsProgressStatus>())
        foreach (var hide in new[] { false, true })
        {
            bar.Type = type;
            bar.Size = size;
            bar.Status = status;
            bar.HideLabel = hide;
        }
        bar.Value = null;
        bar.Max = 0;
        Assert.Equal("Uploading usage-march.csv", bar.Label);
    }

    [Fact]
    public void The_indeterminate_block_moves_by_transforms_only_so_it_never_waits_for_a_layout_pass()
    {
        var bar = new CdsProgressBar { Label = "Scanning registry" };
        Assert.True(bar.Sweep.IsVisible);
        bar.ApplySweep(0.4, 200);
        // A quarter of the track, drawn from its start edge, moved along the track; its layout size is the track's.
        Assert.Equal(-1, bar.Sweep.WidthRequest);
        Assert.Equal(0, bar.Sweep.AnchorX);
        Assert.Equal(0.25, bar.Sweep.ScaleX, 3);
        Assert.Equal(ProgressBarModel.IndeterminateBlock(0.4, 200).X, bar.Sweep.TranslationX, 3);
        Assert.True(bar.Sweep.IsVisible);
        bar.ApplySweep(0.9, 200);
        Assert.False(bar.Sweep.IsVisible);
    }
}
