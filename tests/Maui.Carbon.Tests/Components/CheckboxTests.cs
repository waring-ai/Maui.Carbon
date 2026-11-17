namespace Maui.Carbon.Tests.Components;

public class CheckboxTests
{
    const CdsInteractionState None = CdsInteractionState.None;

    [Fact]
    public void Unchecked_has_icon_primary_border_and_no_fill()
    {
        var v = CdsCheckboxTokens.Resolve(None, false);
        Assert.Equal(CdsColorKeys.IconPrimary, v.Border);
        Assert.Null(v.Fill);
        Assert.Equal(CdsColorKeys.TextPrimary, v.Label);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(null)]
    public void Checked_and_indeterminate_fill_with_icon_primary_and_inverse_mark(bool? isChecked)
    {
        var v = CdsCheckboxTokens.Resolve(None, isChecked);
        Assert.Equal(CdsColorKeys.IconPrimary, v.Fill);
        Assert.Equal(CdsColorKeys.IconInverse, v.Mark);
    }

    [Fact]
    public void Disabled_uses_disabled_tokens_and_fills_when_checked()
    {
        var off = CdsCheckboxTokens.Resolve(CdsInteractionState.Disabled, false);
        var on = CdsCheckboxTokens.Resolve(CdsInteractionState.Disabled, true);
        Assert.Equal(CdsColorKeys.IconDisabled, off.Border);
        Assert.Null(off.Fill);
        Assert.Equal(CdsColorKeys.IconDisabled, on.Fill);
        Assert.Equal(CdsColorKeys.TextDisabled, on.Label);
    }

    [Fact]
    public void Invalid_turns_the_border_error_but_keeps_the_fill()
    {
        var v = CdsCheckboxTokens.Resolve(CdsInteractionState.Invalid, true);
        Assert.Equal(CdsColorKeys.SupportError, v.Border);
        Assert.Equal(CdsColorKeys.IconPrimary, v.Fill);
    }

    [Fact]
    public void Read_only_outlines_the_box_with_a_primary_mark()
    {
        var v = CdsCheckboxTokens.Resolve(CdsInteractionState.ReadOnly, true);
        Assert.Equal(CdsColorKeys.IconDisabled, v.Border);
        Assert.Null(v.Fill);
        Assert.Equal(CdsColorKeys.IconPrimary, v.Mark);
    }

    [Theory]
    [InlineData(CdsInteractionState.Invalid | CdsInteractionState.Disabled)]
    [InlineData(CdsInteractionState.Invalid | CdsInteractionState.ReadOnly)]
    public void Invalid_is_ignored_while_disabled_or_read_only(CdsInteractionState state)
    {
        Assert.Equal(CdsColorKeys.IconDisabled, CdsCheckboxTokens.Resolve(state, false).Border);
    }

    [Fact]
    public void Message_invalid_beats_warning_beats_helper()
    {
        var all = CdsInteractionState.Invalid | CdsInteractionState.Warn;
        var invalid = CdsFormMessageResolver.Resolve(all, "help", "bad", "careful");
        Assert.Equal(CdsFormMessageKind.Invalid, invalid.Kind);
        Assert.Equal("bad", invalid.Text);
        Assert.Equal(CdsColorKeys.TextError, invalid.TextColor);
        Assert.Equal(CdsIconName.WarningFilled, invalid.Icon);
        Assert.Equal(CdsColorKeys.SupportError, invalid.IconColor);

        var warn = CdsFormMessageResolver.Resolve(CdsInteractionState.Warn, "help", "bad", "careful");
        Assert.Equal(CdsFormMessageKind.Warn, warn.Kind);
        Assert.Equal(CdsColorKeys.TextPrimary, warn.TextColor);
        Assert.Equal(CdsIconName.WarningAltFilled, warn.Icon);
        Assert.Equal(CdsColorKeys.SupportWarning, warn.IconColor);

        var helper = CdsFormMessageResolver.Resolve(None, "help", "bad", "careful");
        Assert.Equal(CdsFormMessageKind.Helper, helper.Kind);
        Assert.Equal(CdsColorKeys.TextHelper, helper.TextColor);
        Assert.Null(helper.Icon);
    }

    [Fact]
    public void Message_falls_back_to_helper_when_the_error_text_is_empty()
    {
        var v = CdsFormMessageResolver.Resolve(CdsInteractionState.Invalid, "help", null, null);
        Assert.Equal(CdsFormMessageKind.Helper, v.Kind);
    }

    [Fact]
    public void Message_ignores_invalid_while_disabled_and_dims_the_helper()
    {
        var v = CdsFormMessageResolver.Resolve(CdsInteractionState.Invalid | CdsInteractionState.Disabled, "help", "bad", null);
        Assert.Equal(CdsFormMessageKind.Helper, v.Kind);
        Assert.Equal(CdsColorKeys.TextDisabled, v.TextColor);
    }

    [Theory]
    [InlineData(CdsInteractionState.Disabled)]
    [InlineData(CdsInteractionState.Invalid)]
    [InlineData(CdsInteractionState.Warn)]
    public void Message_can_hide_the_helper_while_blocked(CdsInteractionState state)
    {
        var v = CdsFormMessageResolver.Resolve(state, "help", null, null, hideHelperWhenBlocked: true);
        Assert.Equal(CdsFormMessageKind.None, v.Kind);
        Assert.Null(v.Text);
    }

    [Fact]
    public void Message_is_empty_without_text()
    {
        Assert.Equal(CdsFormMessageKind.None, CdsFormMessageResolver.Resolve(None, null, null, null).Kind);
    }

    [Fact]
    public void Checkbox_can_be_constructed_and_every_property_set()
    {
        var box = new CdsCheckbox("Deployment finished")
        {
            HideLabel = true,
            IsIndeterminate = true,
            HelperText = "help",
            IsInvalid = true,
            InvalidText = "bad",
            IsWarning = true,
            WarnText = "careful",
            IsReadOnly = true,
            IsEnabled = false,
        };
        box.IsChecked = true;
        Assert.True(box.IsChecked);
        Assert.Equal("Deployment finished", box.Label);
    }

    [Fact]
    public void Checkbox_raises_CheckedChanged_with_the_new_value()
    {
        var box = new CdsCheckbox("Email");
        bool? seen = null;
        box.CheckedChanged += (_, e) => seen = e.Value;
        box.IsChecked = true;
        Assert.True(seen);
        box.IsChecked = false;
        Assert.False(seen);
    }

    [Fact]
    public void Group_collects_items_and_builds_in_both_orientations()
    {
        var group = new CdsCheckboxGroup
        {
            Legend = "Notify me by",
            HelperText = "You can change this later",
            IsInvalid = true,
            InvalidText = "Select at least one channel",
            IsWarning = true,
            WarnText = "careful",
            IsReadOnly = true,
        };
        group.Items.Add(new CdsCheckbox("Email"));
        group.Items.Add(new CdsCheckbox("Slack"));
        group.Orientation = CdsChoiceOrientation.Horizontal;
        group.Items.RemoveAt(0);
        group.Orientation = CdsChoiceOrientation.Vertical;
        group.IsEnabled = false;
        Assert.Single(group.Items);
    }
}
