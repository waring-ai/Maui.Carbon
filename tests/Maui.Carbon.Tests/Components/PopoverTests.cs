using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon.Tests.Components;

public class PopoverTests
{
    static readonly Size Host = new(1000, 800);

    static CdsPopover NewPopover(Action<CdsPopover>? configure = null)
    {
        var popover = new CdsPopover { Body = new CdsIconButton { Icon = CdsIconName.Settings, Label = "Settings", Kind = CdsIconButtonKind.Ghost } };
        configure?.Invoke(popover);
        return popover;
    }

    [Theory]
    [InlineData(CdsPlacement.Bottom, CdsPlacement.BottomStart)]
    [InlineData(CdsPlacement.Top, CdsPlacement.BottomStart)]
    [InlineData(CdsPlacement.TopStart, CdsPlacement.BottomStart)]
    [InlineData(CdsPlacement.TopEnd, CdsPlacement.BottomStart)]
    [InlineData(CdsPlacement.Right, CdsPlacement.BottomStart)]
    [InlineData(CdsPlacement.BottomStart, CdsPlacement.BottomStart)]
    [InlineData(CdsPlacement.BottomEnd, CdsPlacement.BottomEnd)]
    public void A_tab_tip_aligns_below_its_trigger_to_the_start_or_end(CdsPlacement requested, CdsPlacement used) =>
        Assert.Equal(used, CdsPopoverModel.TabTipPlacement(requested));

    [Fact]
    public void A_tab_tip_is_square_caretless_flush_and_reaches_the_window_edge()
    {
        var popover = NewPopover(p => { p.IsTabTip = true; p.HasCaret = true; });
        var (surface, options) = popover.Compose(new Label { Text = "Display" });

        Assert.IsType<Rectangle>(surface.Box.StrokeShape);
        surface.UpdateCaret(CdsPlacement.BottomStart, 20);
        Assert.All(surface.Children.OfType<Polygon>(), caret => Assert.False(caret.IsVisible));
        Assert.Equal(CdsPlacement.BottomStart, options.Placement);
        Assert.Equal(0, options.Offset);
        Assert.Equal(0, options.EdgeMargin);
    }

    [Fact]
    public void A_tab_tip_keeps_BottomEnd()
    {
        var popover = NewPopover(p => { p.IsTabTip = true; p.Placement = CdsPlacement.BottomEnd; });
        Assert.Equal(CdsPlacement.BottomEnd, popover.Compose(new Label()).Options.Placement);
    }

    [Fact]
    public void A_regular_popover_keeps_its_rounded_corners_caret_and_margins()
    {
        var popover = NewPopover(p => p.Placement = CdsPlacement.Top);
        var (surface, options) = popover.Compose(new Label());

        var shape = Assert.IsType<RoundRectangle>(surface.Box.StrokeShape);
        Assert.Equal(new CornerRadius(2), shape.CornerRadius);
        Assert.Equal(CdsPlacement.Top, options.Placement);
        Assert.Equal(10 - CdsPopoverSurface.CaretHeight, options.Offset);
        Assert.Equal(CdsPlacementResolver.EdgeMargin, options.EdgeMargin);
    }

    [Fact]
    public void Without_a_caret_the_content_sits_4_from_the_trigger()
    {
        Assert.Equal(4, CdsPopoverSurface.NoCaretGap);
        var popover = NewPopover(p => p.HasCaret = false);
        Assert.Equal(CdsSpacing.Spacing02, popover.Compose(new Label()).Options.Offset);
        Assert.Equal(CdsSpacing.Spacing02, new CdsPopoverSurface(new Label(), hasCaret: false).OffsetFor(13));
        Assert.Equal(7, new CdsPopoverSurface(new Label(), hasCaret: true).OffsetFor(13));
    }

    [Fact]
    public void A_bottom_start_popup_with_no_gap_is_flush_with_the_trigger()
    {
        var anchor = new Rect(100, 40, 40, 40);
        var r = CdsPlacementResolver.Resolve(anchor, new Size(240, 120), Host, CdsPlacement.BottomStart, 0, edgeMargin: 0);
        Assert.Equal(anchor.Left, r.Frame.Left);
        Assert.Equal(anchor.Bottom, r.Frame.Top);
    }

    [Fact]
    public void A_bottom_end_popup_lines_up_with_the_trigger_end_and_can_touch_the_window_edge()
    {
        var anchor = new Rect(960, 0, 40, 48);
        var margin = CdsPlacementResolver.Resolve(anchor, new Size(300, 100), Host, CdsPlacement.BottomEnd, 0);
        Assert.Equal(Host.Width - CdsPlacementResolver.EdgeMargin, margin.Frame.Right);

        var flush = CdsPlacementResolver.Resolve(anchor, new Size(300, 100), Host, CdsPlacement.BottomEnd, 0, edgeMargin: 0);
        Assert.Equal(anchor.Right, flush.Frame.Right);
        Assert.Equal(Host.Width, flush.Frame.Right);
        Assert.Equal(anchor.Bottom, flush.Frame.Top);
    }

    [Fact]
    public void A_tab_tip_flips_above_the_trigger_when_there_is_no_room_below()
    {
        var anchor = new Rect(100, 700, 40, 48);
        var r = CdsPlacementResolver.Resolve(anchor, new Size(240, 120), Host, CdsPlacement.BottomStart, 0, edgeMargin: 0);
        Assert.Equal(CdsPlacement.TopStart, r.Placement);
        Assert.Equal(anchor.Top, r.Frame.Bottom);
    }

    [Fact]
    public void The_trigger_of_an_open_tab_tip_is_drawn_as_a_tab()
    {
        Assert.Equal((CdsColorKeys.Layer01, true), CdsPopoverTokens.TabTipTrigger(true, true, false));
        Assert.Equal((CdsColorKeys.Layer01, false), CdsPopoverTokens.TabTipTrigger(true, true, true));
        Assert.Equal((null, false), CdsPopoverTokens.TabTipTrigger(true, false, false));
        Assert.Equal((null, false), CdsPopoverTokens.TabTipTrigger(false, true, false));
    }

    [Fact]
    public void Every_property_can_be_set_before_loading()
    {
        var popover = NewPopover(p =>
        {
            p.IsTabTip = true;
            p.HasCaret = false;
            p.HasBorder = true;
            p.IsHighContrast = true;
            p.HasDropShadow = false;
            p.Placement = CdsPlacement.BottomEnd;
            p.PopoverContent = new Label { Text = "Display" };
        });
        popover.IsOpen = true;
        popover.IsOpen = false;
        popover.IsTabTip = false;
        Assert.False(popover.IsTabTip);
    }
}
