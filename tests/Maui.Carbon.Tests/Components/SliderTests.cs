namespace Maui.Carbon.Tests.Components;

public class SliderTests
{
    [Theory]
    [InlineData(5, 0, 10, 5)]
    [InlineData(-3, 0, 10, 0)]
    [InlineData(99, 0, 10, 10)]
    [InlineData(double.NaN, 2, 10, 2)]
    [InlineData(5, 10, 0, 10)] // max below min yields min
    public void Clamp(double value, double min, double max, double expected) => Assert.Equal(expected, SliderModel.Clamp(value, min, max));

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0.5, 1)]
    [InlineData(0.25, 2)]
    [InlineData(0.001, 3)]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(double.NaN, 0)]
    public void Decimals(double step, int expected) => Assert.Equal(expected, SliderModel.Decimals(step));

    [Theory]
    [InlineData(7.4, 0, 100, 1, 7)]
    [InlineData(7.5, 0, 100, 1, 8)]
    [InlineData(34, 0, 100, 10, 30)]
    [InlineData(36, 0, 100, 10, 40)]
    [InlineData(37, 20, 2000, 10, 40)]   // counted from min: 20, 30, 40
    [InlineData(-5, 0, 100, 10, 0)]
    [InlineData(500, 0, 100, 10, 100)]
    [InlineData(98, 0, 99, 10, 99)]       // max not on a step: never past max
    [InlineData(0.3, 0, 1, 0.1, 0.3)]     // no float noise
    [InlineData(0.62, 0, 1, 0.25, 0.5)]
    [InlineData(12.3, 0, 100, 0, 12.3)]   // a zero step only clamps
    public void Snap(double value, double min, double max, double step, double expected) =>
        Assert.Equal(expected, SliderModel.Snap(value, min, max, step));

    [Theory]
    [InlineData(0, 0, 100, 0)]
    [InlineData(50, 0, 100, 0.5)]
    [InlineData(100, 0, 100, 1)]
    [InlineData(150, 0, 100, 1)]
    [InlineData(-10, 0, 100, 0)]
    [InlineData(30, 20, 40, 0.5)]
    [InlineData(5, 5, 5, 0)]
    public void ToFraction(double value, double min, double max, double expected) =>
        Assert.Equal(expected, SliderModel.ToFraction(value, min, max), 10);

    [Theory]
    [InlineData(0, 0, 100, 1, 0)]
    [InlineData(0.5, 0, 100, 1, 50)]
    [InlineData(1, 0, 100, 1, 100)]
    [InlineData(1.7, 0, 100, 1, 100)]
    [InlineData(-0.2, 0, 100, 1, 0)]
    [InlineData(0.333, 0, 100, 10, 30)]
    [InlineData(0.5, 20, 2000, 10, 1010)]
    [InlineData(double.NaN, 0, 100, 1, 0)]
    public void FromFraction(double fraction, double min, double max, double step, double expected) =>
        Assert.Equal(expected, SliderModel.FromFraction(fraction, min, max, step));

    [Fact]
    public void Arrow_keys_move_by_one_step()
    {
        Assert.Equal(51, SliderModel.KeyboardValue(CdsKey.Right, false, 50, 0, 100, 1, 4));
        Assert.Equal(51, SliderModel.KeyboardValue(CdsKey.Up, false, 50, 0, 100, 1, 4));
        Assert.Equal(49, SliderModel.KeyboardValue(CdsKey.Left, false, 50, 0, 100, 1, 4));
        Assert.Equal(49, SliderModel.KeyboardValue(CdsKey.Down, false, 50, 0, 100, 1, 4));
    }

    [Fact]
    public void Shift_arrow_moves_by_step_times_multiplier()
    {
        Assert.Equal(54, SliderModel.KeyboardValue(CdsKey.Right, true, 50, 0, 100, 1, 4));
        Assert.Equal(30, SliderModel.KeyboardValue(CdsKey.Left, true, 50, 0, 100, 10, 2));
    }

    [Fact]
    public void Keys_clamp_at_the_ends_and_Home_End_jump()
    {
        Assert.Equal(100, SliderModel.KeyboardValue(CdsKey.Right, false, 100, 0, 100, 1, 4));
        Assert.Equal(0, SliderModel.KeyboardValue(CdsKey.Left, true, 2, 0, 100, 1, 4));
        Assert.Equal(0, SliderModel.KeyboardValue(CdsKey.Home, false, 50, 0, 100, 1, 4));
        Assert.Equal(100, SliderModel.KeyboardValue(CdsKey.End, false, 50, 0, 100, 1, 4));
    }

    [Fact]
    public void Other_keys_are_not_slider_keys() => Assert.Null(SliderModel.KeyboardValue(CdsKey.Tab, false, 50, 0, 100, 1, 4));

    [Fact]
    public void Keyboard_steps_respect_a_fractional_step()
    {
        Assert.Equal(0.6, SliderModel.KeyboardValue(CdsKey.Right, false, 0.5, 0, 1, 0.1, 4));
        Assert.Equal(0.9, SliderModel.KeyboardValue(CdsKey.Right, true, 0.5, 0, 1, 0.1, 4));
    }

    [Fact]
    public void A_multiplier_below_one_is_treated_as_one() =>
        Assert.Equal(51, SliderModel.KeyboardValue(CdsKey.Right, true, 50, 0, 100, 1, 0));

    [Fact]
    public void OrderRange_keeps_lower_below_upper()
    {
        Assert.Equal((10d, 20d), SliderModel.OrderRange(10, 20, SliderHandle.Lower));
        Assert.Equal((20d, 20d), SliderModel.OrderRange(30, 20, SliderHandle.Lower));
        Assert.Equal((20d, 20d), SliderModel.OrderRange(20, 5, SliderHandle.Upper));
        Assert.Equal((20d, 20d), SliderModel.OrderRange(20, 20, SliderHandle.Upper));
    }

    [Theory]
    [InlineData(10, 20, 80, false)]
    [InlineData(90, 20, 80, true)]
    [InlineData(40, 20, 80, false)]
    [InlineData(60, 20, 80, true)]
    [InlineData(50, 20, 80, false)] // equidistant inside: lower
    [InlineData(70, 50, 50, true)] // handles together: tap past them moves upper
    [InlineData(30, 50, 50, false)]
    public void NearestHandle(double value, double lower, double upper, bool expectUpper) =>
        Assert.Equal(expectUpper ? SliderHandle.Upper : SliderHandle.Lower, SliderModel.NearestHandle(value, lower, upper));

    [Theory]
    [InlineData(-1, 0, 10, true)]
    [InlineData(11, 0, 10, true)]
    [InlineData(0, 0, 10, false)]
    [InlineData(10, 0, 10, false)]
    public void IsOutOfRange(double value, double min, double max, bool expected) =>
        Assert.Equal(expected, SliderModel.IsOutOfRange(value, min, max));

    [Theory]
    [InlineData("42", true, 42)]
    [InlineData(" 7.5 ", true, 7.5)]
    [InlineData("-3", true, -3)]
    [InlineData("", false, 0)]
    [InlineData("  ", false, 0)]
    [InlineData(null, false, 0)]
    [InlineData("abc", false, 0)]
    [InlineData("NaN", false, 0)]
    [InlineData("Infinity", false, 0)]
    public void TryParse(string? text, bool ok, double expected)
    {
        Assert.Equal(ok, SliderModel.TryParse(text, out var value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData(120, 1, "120")]
    [InlineData(0.5, 0.25, "0.5")]
    [InlineData(0.25, 0.25, "0.25")]
    [InlineData(3, 0.5, "3")]
    [InlineData(2.34567, 1, "2")]
    public void Format(double value, double step, string expected) => Assert.Equal(expected, SliderModel.Format(value, step));

    [Fact]
    public void Single_slider_tokens()
    {
        var rest = CdsSliderTokens.Resolve(CdsInteractionState.None, CdsSliderThumbState.Normal, false);
        Assert.Equal(CdsColorKeys.BorderSubtle01, rest.Track);
        Assert.Equal(CdsColorKeys.LayerSelectedInverse, rest.Fill);
        Assert.Equal(CdsColorKeys.LayerSelectedInverse, rest.Thumb);
        Assert.Null(rest.Ring);
        Assert.Null(rest.Gap);
    }

    [Fact]
    public void Focused_thumb_is_interactive_with_a_gap_ring_and_the_fill_turns_border_interactive()
    {
        var v = CdsSliderTokens.Resolve(CdsInteractionState.None, CdsSliderThumbState.Focus, false);
        Assert.Equal(CdsColorKeys.Interactive, v.Thumb);
        Assert.Equal(CdsColorKeys.Layer01, v.Gap);
        Assert.Equal(CdsColorKeys.BorderInteractive, v.Fill);
    }

    [Fact]
    public void Pressed_thumb_has_an_interactive_inner_ring()
    {
        var v = CdsSliderTokens.Resolve(CdsInteractionState.None, CdsSliderThumbState.Pressed, false);
        Assert.Equal(CdsColorKeys.Interactive, v.Ring);
        Assert.Equal(CdsColorKeys.LayerSelectedInverse, v.Thumb);
    }

    [Fact]
    public void Range_handles_change_color_on_hover_and_focus()
    {
        Assert.Equal(CdsColorKeys.TextSecondary, CdsSliderTokens.Resolve(CdsInteractionState.None, CdsSliderThumbState.Hover, true).Thumb);
        Assert.Equal(CdsColorKeys.Interactive, CdsSliderTokens.Resolve(CdsInteractionState.None, CdsSliderThumbState.Focus, true).Thumb);
        Assert.Equal(CdsColorKeys.LayerSelectedInverse, CdsSliderTokens.Resolve(CdsInteractionState.None, CdsSliderThumbState.Normal, true).Thumb);
    }

    [Theory]
    [InlineData(CdsSliderThumbState.Normal)]
    [InlineData(CdsSliderThumbState.Focus)]
    [InlineData(CdsSliderThumbState.Hover)]
    [InlineData(CdsSliderThumbState.Pressed)]
    public void Disabled_paints_everything_border_disabled(CdsSliderThumbState thumb)
    {
        var v = CdsSliderTokens.Resolve(CdsInteractionState.Disabled, thumb, false);
        Assert.Equal(CdsColorKeys.BorderDisabled, v.Track);
        Assert.Equal(CdsColorKeys.BorderDisabled, v.Fill);
        Assert.Equal(CdsColorKeys.BorderDisabled, v.Thumb);
        Assert.Equal(CdsColorKeys.TextDisabled, v.RangeLabel);
        Assert.Null(v.Ring);
        Assert.Null(v.Gap);
    }

    [Fact]
    public void Slider_defaults_and_clamping()
    {
        var slider = new CdsSlider { Label = "Size (GB)", Min = 20, Max = 2000, Step = 10, MinLabel = "GB", MaxLabel = "GB" };
        Assert.Equal(20, slider.Value); // clamped up to the new minimum
        slider.Value = 5000;
        Assert.Equal(2000, slider.Value);
        slider.Max = 500;
        Assert.Equal(500, slider.Value); // follows a lowered maximum
        Assert.Equal(4, slider.StepMultiplier);
    }

    [Fact]
    public void Slider_raises_ValueChanged_and_can_be_a_range()
    {
        var slider = new CdsSlider { IsRange = true, Min = 0, Max = 50, InputLabel = "Minimum nodes", InputLabelUpper = "Maximum nodes" };
        slider.Value = 3;
        slider.UpperValue = 24;
        CdsSliderChangedEventArgs? seen = null;
        slider.ValueChanged += (_, e) => seen = e;
        slider.UpperValue = 30;
        Assert.Equal(3, seen!.Value);
        Assert.Equal(30, seen.UpperValue);
    }

    [Fact]
    public void Slider_arrow_keys_need_a_focused_thumb()
    {
        var slider = new CdsSlider { Value = 10 };
        Assert.True(slider.WantsNavigationKeys);
        Assert.False(slider.OnKeyDown(new CdsKeyEventArgs(CdsKey.Right)));
        Assert.Equal(10, slider.Value);
    }

    [Fact]
    public void Slider_can_be_constructed_and_every_property_set()
    {
        var slider = new CdsSlider
        {
            HideLabel = true,
            HideTextInput = true,
            ValueFormatter = v => $"{v} nodes",
            IsInvalid = true,
            InvalidText = "bad",
            IsWarning = true,
            WarnText = "careful",
            IsReadOnly = true,
            IsEnabled = false,
            StepMultiplier = 10,
        };
        slider.IsRange = true;
        slider.IsReadOnly = false;
        slider.HideTextInput = false;
        Assert.True(slider.IsRange);
    }
}
