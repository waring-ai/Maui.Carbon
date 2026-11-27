namespace Maui.Carbon.Tests.Components;

// Sets the process-wide CdsMotion.ReducedMotionOverride: runs apart from the parallel tests.
[Collection(FocusTrackerCollection.Name)]
public class CascadeTests
{
    [Theory]
    [InlineData(0, 60)]
    [InlineData(1, 120)]
    [InlineData(6, 420)]
    [InlineData(7, 480)]
    [InlineData(8, 480)]
    [InlineData(30, 480)]
    [InlineData(-1, 60)]
    public void Each_item_starts_60_ms_after_the_one_before_up_to_the_eighth(int index, int delay) =>
        Assert.Equal(delay, CascadeModel.Delay(index));

    [Fact]
    public void Entrance_uses_carbon_motion_and_3rem()
    {
        Assert.Equal(CdsDuration.Moderate02, CascadeModel.Duration);
        Assert.Equal(CdsSpacing.Spacing09, CascadeModel.Offset);
        Assert.Equal(0, CascadeModel.TotalDuration(0));
        Assert.Equal(60 + 240, CascadeModel.TotalDuration(1));
        Assert.Equal(480 + 240, CascadeModel.TotalDuration(12));
    }

    [Theory]
    [InlineData(0, 0, 48)]
    [InlineData(0.5, 0.5, 24)]
    [InlineData(1, 1, 0)]
    [InlineData(-1, 0, 48)]
    [InlineData(2, 1, 0)]
    public void Progress_maps_to_opacity_and_offset(double progress, double opacity, double offset)
    {
        var (o, y) = CascadeModel.At(progress);
        Assert.Equal(opacity, o, 6);
        Assert.Equal(offset, y, 6);
    }

    [Fact]
    public void Items_go_to_a_stack_or_a_grid()
    {
        var cascade = new CdsCascade();
        var a = new BoxView();
        var b = new BoxView();
        cascade.Items.Add(a);
        cascade.Items.Add(b);
        Assert.IsType<VerticalStackLayout>(cascade.Content);
        Assert.Same(cascade.Content, a.Parent);
        Assert.Null(cascade.Grid);

        cascade.IsGrid = true;
        Assert.IsType<CdsGrid>(cascade.Content);
        Assert.Same(cascade.Grid, a.Parent);
        Assert.Equal(2, cascade.Grid!.Count);

        cascade.Items.Remove(a);
        Assert.Null(a.Parent);
        Assert.Equal(1, a.Opacity);
        cascade.IsGrid = false;
        Assert.IsType<VerticalStackLayout>(cascade.Content);
        Assert.Same(cascade.Content, b.Parent);
    }

    [Fact]
    public void Items_start_hidden_below_their_place_until_the_entrance()
    {
        CdsMotion.ReducedMotionOverride = false;
        try
        {
            var cascade = new CdsCascade();
            var tile = new BoxView();
            cascade.Items.Add(tile);
            Assert.Equal(0, tile.Opacity);
            Assert.Equal(CascadeModel.Offset, tile.TranslationY);
            Assert.False(cascade.HasPlayed);
        }
        finally
        {
            CdsMotion.ReducedMotionOverride = null;
        }
    }

    [Fact]
    public void Reduced_motion_shows_items_at_once()
    {
        CdsMotion.ReducedMotionOverride = true;
        try
        {
            var cascade = new CdsCascade();
            var tile = new BoxView();
            cascade.Items.Add(tile);
            Assert.Equal(1, tile.Opacity);
            Assert.Equal(0, tile.TranslationY);
            cascade.Replay();
            Assert.True(cascade.HasPlayed);
            Assert.Equal(1, tile.Opacity);
        }
        finally
        {
            CdsMotion.ReducedMotionOverride = null;
        }
    }

    [Fact]
    public void Content_cannot_be_set_and_properties_do_not_throw()
    {
        var cascade = new CdsCascade { Spacing = CdsSpacing.Spacing05 };
        Assert.Equal(CdsSpacing.Spacing05, ((VerticalStackLayout)cascade.Content).Spacing);
        Assert.Throws<InvalidOperationException>(() => cascade.Content = new Label());
        cascade.Items.Clear();
        cascade.Replay();
    }
}
