namespace Maui.Carbon.Tests.Components;

public class ToggleTests
{
    [Fact]
    public void Off_track_is_toggle_off_and_on_track_is_success()
    {
        Assert.Equal(CdsColorKeys.ToggleOff, CdsToggleTokens.Resolve(CdsInteractionState.None, false).Track);
        Assert.Equal(CdsColorKeys.SupportSuccess, CdsToggleTokens.Resolve(CdsInteractionState.None, true).Track);
    }

    [Fact]
    public void Knob_is_icon_on_color_and_check_is_success_green()
    {
        var v = CdsToggleTokens.Resolve(CdsInteractionState.None, true);
        Assert.Equal(CdsColorKeys.IconOnColor, v.Knob);
        Assert.Equal(CdsColorKeys.SupportSuccess, v.Check);
        Assert.Equal(CdsColorKeys.TextPrimary, v.StateText);
        Assert.Equal(CdsColorKeys.TextSecondary, v.LabelText);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Disabled_uses_button_disabled_and_text_on_color_disabled(bool toggled)
    {
        var v = CdsToggleTokens.Resolve(CdsInteractionState.Disabled, toggled);
        Assert.Equal(CdsColorKeys.ButtonDisabled, v.Track);
        Assert.Equal(CdsColorKeys.TextOnColorDisabled, v.Knob);
        Assert.Equal(CdsColorKeys.TextDisabled, v.StateText);
        Assert.Equal(CdsColorKeys.TextDisabled, v.LabelText);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Read_only_is_an_outlined_track_with_a_primary_knob_and_no_check(bool toggled)
    {
        var v = CdsToggleTokens.Resolve(CdsInteractionState.ReadOnly, toggled);
        Assert.Null(v.Track);
        Assert.Equal(CdsColorKeys.IconDisabled, v.TrackBorder);
        Assert.Equal(CdsColorKeys.TextPrimary, v.Knob);
        Assert.Null(v.Check);
    }

    [Fact]
    public void Disabled_wins_over_read_only()
    {
        var v = CdsToggleTokens.Resolve(CdsInteractionState.Disabled | CdsInteractionState.ReadOnly, true);
        Assert.Equal(CdsColorKeys.ButtonDisabled, v.Track);
    }

    [Fact]
    public void Defaults_are_off_on_and_medium()
    {
        var toggle = new CdsToggle();
        Assert.Equal("Off", toggle.LabelA);
        Assert.Equal("On", toggle.LabelB);
        Assert.Equal(CdsToggleSize.Md, toggle.Size);
        Assert.False(toggle.IsToggled);
    }

    [Fact]
    public void Toggled_event_reports_the_new_value_and_every_property_can_be_set()
    {
        var toggle = new CdsToggle { LabelText = "Auto-scaling", LabelA = "Disabled", LabelB = "Enabled", Size = CdsToggleSize.Sm, IsReadOnly = true, HideLabel = true };
        bool? seen = null;
        toggle.Toggled += (_, e) => seen = e.Value;
        toggle.IsToggled = true;
        Assert.True(seen);
        toggle.IsEnabled = false;
        toggle.IsToggled = false;
        Assert.False(seen);
    }
}
