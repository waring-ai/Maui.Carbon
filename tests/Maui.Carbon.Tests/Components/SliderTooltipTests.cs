namespace Maui.Carbon.Tests.Components;

/// <summary>The value tooltip over a slider thumb when HideTextInput is set (Carbon's ThumbWrapper hasTooltip).</summary>
[Collection(FocusTrackerCollection.Name)]
public class SliderTooltipTests
{
    static SliderTooltipStep? Set(SliderTooltipModel m, SliderTooltipSource s, bool on, bool open) => m.Set(s, on, open);

    [Fact]
    public void Hover_opens_after_100_ms_and_closes_300_ms_after_leaving()
    {
        var m = new SliderTooltipModel();
        Assert.Equal(new SliderTooltipStep(true, 100), Set(m, SliderTooltipSource.Hover, true, open: false));
        Assert.Equal(new SliderTooltipStep(false, 300), Set(m, SliderTooltipSource.Hover, false, open: true));
    }

    [Fact]
    public void Keyboard_focus_and_dragging_open_at_once_and_blur_closes_at_once()
    {
        var m = new SliderTooltipModel();
        Assert.Equal(new SliderTooltipStep(true, 0), Set(m, SliderTooltipSource.Focus, true, open: false));
        Assert.Equal(new SliderTooltipStep(false, 0), Set(m, SliderTooltipSource.Focus, false, open: true));
        Assert.Equal(new SliderTooltipStep(true, 0), Set(m, SliderTooltipSource.Drag, true, open: false));
    }

    [Fact]
    public void Dragging_keeps_it_open_when_the_pointer_leaves_the_thumb()
    {
        var m = new SliderTooltipModel();
        Set(m, SliderTooltipSource.Hover, true, open: false);
        Set(m, SliderTooltipSource.Drag, true, open: true);
        Assert.Null(Set(m, SliderTooltipSource.Hover, false, open: true)); // still dragging
        Assert.True(m.Wanted);
        Assert.Equal(new SliderTooltipStep(false, 300), Set(m, SliderTooltipSource.Drag, false, open: true));
    }

    [Fact]
    public void Ending_a_drag_over_the_thumb_leaves_it_open()
    {
        var m = new SliderTooltipModel();
        Set(m, SliderTooltipSource.Hover, true, open: false);
        Set(m, SliderTooltipSource.Drag, true, open: true);
        Assert.Null(Set(m, SliderTooltipSource.Drag, false, open: true));
        Assert.True(m.Hover);
    }

    [Fact]
    public void Blur_while_hovering_keeps_it_open_and_reset_forgets_everything()
    {
        var m = new SliderTooltipModel();
        Set(m, SliderTooltipSource.Hover, true, open: false);
        Set(m, SliderTooltipSource.Focus, true, open: true);
        Assert.Null(Set(m, SliderTooltipSource.Focus, false, open: true));
        m.Reset();
        Assert.False(m.Wanted);
    }

    static (CdsSlider Slider, CdsOverlayHost Host) Shown(CdsSlider slider)
    {
        var host = ActionTestHost.Show(slider);
        return (slider, host);
    }

    static string TextOf(CdsOverlay overlay) =>
        Assert.IsType<CdsText>(Assert.IsType<CdsPopoverSurface>(overlay.Content).Box.Content).Text;

    [Fact]
    public void Keyboard_focus_shows_the_formatted_value_above_the_thumb_and_follows_it()
    {
        var (slider, host) = Shown(new CdsSlider { Label = "Size", Min = 0, Max = 100, Value = 40, HideTextInput = true, ValueFormatter = v => v + " GB" });
        slider.LowerThumb.ReportFocus(true, fromKeyboard: true);

        var overlay = Assert.Single(host.Overlays);
        Assert.Equal(CdsPlacement.Top, overlay.Options.Placement);
        Assert.True(overlay.Options.InputTransparent);
        Assert.False(overlay.Options.LightDismiss);
        Assert.Equal("40 GB", TextOf(overlay));

        slider.Value = 65;
        Assert.Equal("65 GB", TextOf(overlay));
        Assert.Same(overlay, Assert.Single(host.Overlays)); // moved, not reopened

        slider.LowerThumb.ReportFocus(false, fromKeyboard: true);
        Assert.Empty(host.Overlays);
    }

    [Fact]
    public void Text_is_the_default_format_and_never_carries_the_min_or_max_label()
    {
        var (slider, host) = Shown(new CdsSlider { Min = 0, Max = 1, Step = 0.05, Value = 0.25, MinLabel = "GB", MaxLabel = "GB", HideTextInput = true });
        slider.LowerThumb.ReportFocus(true, fromKeyboard: true);
        Assert.Equal("0.25", TextOf(Assert.Single(host.Overlays)));
    }

    [Fact]
    public void Focus_from_a_pointer_does_not_open_it()
    {
        var (slider, host) = Shown(new CdsSlider { HideTextInput = true });
        slider.LowerThumb.ReportFocus(true, fromKeyboard: false);
        Assert.Empty(host.Overlays);
    }

    [Fact]
    public void No_tooltip_with_the_number_box_read_only_or_disabled_and_an_open_one_closes()
    {
        var (withBox, hostA) = Shown(new CdsSlider { Value = 10 });
        withBox.LowerThumb.ReportFocus(true, fromKeyboard: true);
        Assert.Empty(hostA.Overlays);

        var (readOnly, hostB) = Shown(new CdsSlider { HideTextInput = true, IsReadOnly = true });
        readOnly.LowerThumb.ReportFocus(true, fromKeyboard: true);
        Assert.Empty(hostB.Overlays);

        var (slider, hostC) = Shown(new CdsSlider { HideTextInput = true });
        slider.LowerThumb.ReportFocus(true, fromKeyboard: true);
        Assert.Single(hostC.Overlays);
        slider.IsEnabled = false;
        Assert.Empty(hostC.Overlays);

        var (other, hostD) = Shown(new CdsSlider { HideTextInput = true });
        other.LowerThumb.ReportFocus(true, fromKeyboard: true);
        Assert.Single(hostD.Overlays);
        other.HideTextInput = false;
        Assert.Empty(hostD.Overlays);
    }

    [Fact]
    public void A_range_has_one_tooltip_per_thumb_with_its_own_value()
    {
        var (slider, host) = Shown(new CdsSlider { IsRange = true, Min = 0, Max = 500, Value = 80, UpperValue = 320, HideTextInput = true });
        slider.LowerThumb.ReportFocus(true, fromKeyboard: true);
        slider.UpperThumb.ReportFocus(true, fromKeyboard: true);
        Assert.Equal(2, host.Overlays.Count);
        Assert.Equal("80", slider.LowerTooltip!.Text);
        Assert.Equal("320", slider.UpperTooltip!.Text);

        slider.UpperValue = 400;
        Assert.Equal("80", slider.LowerTooltip.Text);
        Assert.Equal("400", slider.UpperTooltip.Text);

        slider.LowerThumb.ReportFocus(false, fromKeyboard: true);
        Assert.Single(host.Overlays);
        Assert.True(slider.UpperTooltip.IsOpen);
    }

    [Fact]
    public void Dragging_shows_it_and_the_content_stays_out_of_the_accessibility_tree()
    {
        var (slider, host) = Shown(new CdsSlider { HideTextInput = true, Value = 30 });
        slider.LowerThumb.ReportFocus(true, fromKeyboard: true);
        slider.LowerThumb.ReportFocus(false, fromKeyboard: true);
        Assert.Empty(host.Overlays);

        slider.LowerTooltip!.DragChanged(true);
        var overlay = Assert.Single(host.Overlays);
        Assert.True(AutomationProperties.GetExcludedWithChildren(overlay.Content));
        Assert.Equal("30", TextOf(overlay));
        Assert.Null(SemanticProperties.GetDescription(overlay.Content));
        slider.LowerTooltip.Dispose();
        Assert.Empty(host.Overlays);
    }

    [Fact]
    public void Surface_caret_size_sets_the_offset_for_the_gap()
    {
        var label = new CdsText("40");
        Assert.Equal(2, new CdsPopoverSurface(label).OffsetFor(8));
        var small = new CdsPopoverSurface(label, caretWidth: 8, caretHeight: 4);
        Assert.Equal(4, small.OffsetFor(8));
        Assert.Equal(0, small.OffsetFor(4));
        small.UpdateCaret(CdsPlacement.Top, 20);
        Assert.Equal(new Thickness(0, 0, 0, 4), small.Box.Margin);
        small.UpdateCaret(CdsPlacement.Bottom, 20);
        Assert.Equal(new Thickness(0, 4, 0, 0), small.Box.Margin);
    }
}
