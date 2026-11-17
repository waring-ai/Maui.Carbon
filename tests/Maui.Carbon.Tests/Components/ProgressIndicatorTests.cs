namespace Maui.Carbon.Tests.Components;

public class ProgressIndicatorTests
{
    [Theory]
    [InlineData(0, 2, CdsProgressStepState.Complete)]
    [InlineData(1, 2, CdsProgressStepState.Complete)]
    [InlineData(2, 2, CdsProgressStepState.Current)]
    [InlineData(3, 2, CdsProgressStepState.Incomplete)]
    [InlineData(0, -1, CdsProgressStepState.Incomplete)]
    [InlineData(4, 9, CdsProgressStepState.Complete)]
    public void Position_decides_complete_current_incomplete(int index, int current, CdsProgressStepState expected) =>
        Assert.Equal(expected, ProgressIndicatorModel.Resolve(index, current, false, false, false));

    [Fact]
    public void Complete_forces_a_later_step_complete_but_not_the_current_one()
    {
        Assert.Equal(CdsProgressStepState.Complete, ProgressIndicatorModel.Resolve(4, 1, false, false, true));
        Assert.Equal(CdsProgressStepState.Current, ProgressIndicatorModel.Resolve(1, 1, false, false, true));
    }

    [Fact]
    public void Disabled_wins_over_invalid_which_wins_over_position()
    {
        Assert.Equal(CdsProgressStepState.Invalid, ProgressIndicatorModel.Resolve(0, 2, true, false, false));
        Assert.Equal(CdsProgressStepState.Invalid, ProgressIndicatorModel.Resolve(2, 2, true, false, false));
        Assert.Equal(CdsProgressStepState.Disabled, ProgressIndicatorModel.Resolve(3, 2, true, true, false));
        Assert.Equal(CdsProgressStepState.Disabled, ProgressIndicatorModel.Resolve(0, 2, false, true, false));
    }

    [Fact]
    public void The_connector_is_drawn_up_to_and_including_the_current_step()
    {
        Assert.True(ProgressIndicatorModel.LineReached(0, 2, false));
        Assert.True(ProgressIndicatorModel.LineReached(2, 2, false));
        Assert.False(ProgressIndicatorModel.LineReached(3, 2, false));
        Assert.True(ProgressIndicatorModel.LineReached(3, 2, true));
    }

    [Fact]
    public void Clickable_steps_exclude_the_current_and_disabled_ones()
    {
        Assert.True(ProgressIndicatorModel.IsClickable(true, 0, 2, false));
        Assert.True(ProgressIndicatorModel.IsClickable(true, 3, 2, false));
        Assert.False(ProgressIndicatorModel.IsClickable(true, 2, 2, false));
        Assert.False(ProgressIndicatorModel.IsClickable(true, 0, 2, true));
        Assert.False(ProgressIndicatorModel.IsClickable(false, 0, 2, false));
    }

    [Theory]
    [InlineData(CdsProgressStepState.Complete, CdsIconName.CheckmarkOutline, CdsColorKeys.Interactive, CdsColorKeys.TextPrimary)]
    [InlineData(CdsProgressStepState.Current, CdsIconName.Incomplete, CdsColorKeys.Interactive, CdsColorKeys.TextPrimary)]
    [InlineData(CdsProgressStepState.Incomplete, CdsIconName.CircleDash, CdsColorKeys.IconPrimary, CdsColorKeys.TextPrimary)]
    [InlineData(CdsProgressStepState.Invalid, CdsIconName.Warning, CdsColorKeys.SupportError, CdsColorKeys.TextPrimary)]
    [InlineData(CdsProgressStepState.Disabled, CdsIconName.CircleDash, CdsColorKeys.IconDisabled, CdsColorKeys.TextDisabled)]
    public void State_maps_to_icon_and_colors(CdsProgressStepState state, CdsIconName icon, string iconColor, string labelColor)
    {
        var v = CdsProgressStepTokens.Resolve(state);
        Assert.Equal(icon, v.Icon);
        Assert.Equal(iconColor, v.IconColor);
        Assert.Equal(labelColor, v.LabelColor);
    }

    [Fact]
    public void Line_colors_and_status_words()
    {
        Assert.Equal(CdsColorKeys.Interactive, CdsProgressStepTokens.LineColor(true));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsProgressStepTokens.LineColor(false));
        Assert.Equal("Complete", CdsProgressStepTokens.StatusWord(CdsProgressStepState.Complete));
        Assert.Equal("Invalid", CdsProgressStepTokens.StatusWord(CdsProgressStepState.Invalid));
    }

    static CdsProgressIndicator Sample() => new()
    {
        Steps =
        {
            new CdsProgressStep("Choose plan"),
            new CdsProgressStep("Configure nodes"),
            new CdsProgressStep("Networking") { IsInvalid = true, SecondaryLabel = "Fix the subnet" },
            new CdsProgressStep("Add-ons", "Optional"),
            new CdsProgressStep("Review") { IsDisabled = true },
        },
    };

    [Fact]
    public void Steps_and_layouts_can_be_built_without_a_platform()
    {
        var indicator = Sample();
        indicator.CurrentIndex = 1;
        indicator.IsVertical = true;
        indicator.IsClickable = true;
        indicator.IsVertical = false;
        indicator.SpaceEqually = true;
        indicator.Steps.Add(new CdsProgressStep("Done"));
        indicator.Steps.RemoveAt(0);
        indicator.Steps[0].Label = "Renamed";
        indicator.CurrentIndex = 99;
        indicator.CurrentIndex = -1;
        Assert.Equal(5, indicator.Steps.Count);
    }

    [Fact]
    public void CurrentIndex_changes_raise_an_event_with_old_and_new_values()
    {
        var indicator = Sample();
        ValueChangedEventArgs? args = null;
        indicator.CurrentIndexChanged += (_, e) => args = e;
        indicator.CurrentIndex = 3;
        Assert.NotNull(args);
        Assert.Equal(0, args!.OldValue);
        Assert.Equal(3, args.NewValue);
    }
}
