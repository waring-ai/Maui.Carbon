namespace Maui.Carbon.Tests.Components;

public class ButtonSetTests
{
    [Fact]
    public void Plain_sets_are_as_wide_as_their_buttons_allow()
    {
        Assert.Equal(392, ButtonSetModel.MaxSetWidth(2, stacked: false, expressive: false));
        Assert.Equal(588, ButtonSetModel.MaxSetWidth(3, stacked: false, expressive: false));
        Assert.Equal(196, ButtonSetModel.MaxSetWidth(3, stacked: true, expressive: false));
        Assert.Equal(640, ButtonSetModel.MaxSetWidth(2, stacked: false, expressive: true));
        Assert.Equal(196, ButtonSetModel.MaxSetWidth(0, stacked: false, expressive: false));
    }

    [Fact]
    public void Fluid_order_is_ghost_danger_ghost_tertiary_secondary_danger_primary()
    {
        CdsButtonKind[] kinds =
        [
            CdsButtonKind.Primary, CdsButtonKind.Danger, CdsButtonKind.Secondary, CdsButtonKind.Tertiary,
            CdsButtonKind.DangerGhost, CdsButtonKind.Ghost,
        ];
        var order = ButtonSetModel.FluidOrder(kinds);
        Assert.Equal(
            [CdsButtonKind.Ghost, CdsButtonKind.DangerGhost, CdsButtonKind.Tertiary, CdsButtonKind.Secondary, CdsButtonKind.Danger, CdsButtonKind.Primary],
            order.Select(i => kinds[i]));
    }

    [Fact]
    public void Fluid_order_is_stable_for_equal_kinds()
    {
        CdsButtonKind[] kinds = [CdsButtonKind.Secondary, CdsButtonKind.Secondary, CdsButtonKind.Primary];
        Assert.Equal([0, 1, 2], ButtonSetModel.FluidOrder(kinds));
    }

    [Theory]
    [InlineData(176, 1, true)]
    [InlineData(177, 1, false)]
    [InlineData(352, 2, true)]
    [InlineData(353, 2, false)]
    [InlineData(528, 3, true)]
    [InlineData(529, 3, false)]
    [InlineData(704, 4, true)]
    [InlineData(705, 4, false)]
    [InlineData(704, 5, true)]
    [InlineData(0, 2, false)]
    [InlineData(100, 0, false)]
    public void Fluid_sets_stack_at_11rem_per_button(double width, int count, bool stack) =>
        Assert.Equal(stack, ButtonSetModel.ShouldStack(width, count));

    [Fact]
    public void Fluid_buttons_take_a_quarter_of_the_set_between_11_and_14_5_rem_and_right_align()
    {
        var widths = ButtonSetModel.FluidWidths(1000, [false, false], out var offset);
        Assert.Equal([232, 232], widths);
        Assert.Equal(536, offset);

        widths = ButtonSetModel.FluidWidths(800, [false, false], out offset);
        Assert.Equal([200, 200], widths);
        Assert.Equal(400, offset);

        widths = ButtonSetModel.FluidWidths(600, [false, false], out offset);
        Assert.Equal([176, 176], widths);
        Assert.Equal(248, offset);
    }

    [Fact]
    public void Fluid_ghost_buttons_fill_the_remaining_width()
    {
        var widths = ButtonSetModel.FluidWidths(1000, [true, false, false], out var offset);
        Assert.Equal([536, 232, 232], widths);
        Assert.Equal(0, offset);
    }

    [Theory]
    [InlineData(0, false, false, false)]
    [InlineData(1, false, false, true)]
    [InlineData(1, true, false, false)]
    [InlineData(1, false, true, false)]
    [InlineData(2, false, false, true)]
    public void Separators_hide_before_the_first_button_and_next_to_focus(int index, bool previousFocused, bool currentFocused, bool shown) =>
        Assert.Equal(shown, ButtonSetModel.ShowSeparator(index, previousFocused, currentFocused));

    [Fact]
    public void Separator_colors()
    {
        Assert.Equal(CdsColorKeys.ButtonSeparator, CdsButtonSetTokens.Separator(false, false));
        Assert.Equal(CdsColorKeys.ButtonSeparator, CdsButtonSetTokens.Separator(true, false));
        Assert.Equal(CdsColorKeys.IconOnColorDisabled, CdsButtonSetTokens.Separator(false, true));
        Assert.Equal(CdsColorKeys.LayerSelectedDisabled, CdsButtonSetTokens.Separator(true, true));
    }

    [Fact]
    public void A_set_collects_buttons_and_toggling_modes_does_not_throw()
    {
        var set = new CdsButtonSet { Label = "Form actions" };
        set.Items.Add(new CdsButton("Cancel", CdsButtonKind.Secondary));
        set.Items.Add(new CdsButton("Create", CdsButtonKind.Primary) { IsEnabled = false });
        set.Items.Add(new CdsButton("Skip", CdsButtonKind.Ghost));
        Assert.Equal(3, set.Items.Count);
        set.IsStacked = true;
        set.IsFluid = true;
        set.IsStacked = false;
        set.Items.RemoveAt(0);
        Assert.Equal(2, set.Items.Count);
    }
}
