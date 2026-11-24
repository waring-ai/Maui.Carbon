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

        widths = ButtonSetModel.FluidWidths(600, [false, false, false], out offset);
        Assert.Equal([176, 176, 176], widths);
        Assert.Equal(72, offset);
    }

    [Theory]
    [InlineData(640, false, false)]
    [InlineData(704, false, false)]
    [InlineData(400, true, false)]
    [InlineData(500, false, true)]
    public void Two_fluid_buttons_split_the_set_50_50_up_to_44rem(double width, bool firstGhost, bool secondGhost)
    {
        var widths = ButtonSetModel.FluidWidths(width, [firstGhost, secondGhost], out var offset);
        Assert.Equal([width / 2, width / 2], widths);
        Assert.Equal(0, offset);
    }

    [Fact]
    public void Two_fluid_buttons_above_44rem_go_back_to_a_quarter_each()
    {
        var widths = ButtonSetModel.FluidWidths(720, [false, false], out var offset);
        Assert.Equal([180, 180], widths);
        Assert.Equal(360, offset);
    }

    [Fact]
    public void One_fluid_button_fills_the_set_up_to_33rem()
    {
        Assert.Equal([400], ButtonSetModel.FluidWidths(400, [false], out var offset));
        Assert.Equal(0, offset);
        Assert.Equal([528], ButtonSetModel.FluidWidths(528, [false], out offset));
        Assert.Equal([176], ButtonSetModel.FluidWidths(600, [false], out offset));
        Assert.Equal(424, offset);
    }

    [Fact]
    public void Plain_sets_fill_the_space_up_to_12_25rem_per_button_instead_of_fitting_their_labels()
    {
        var set = new CdsButtonSet();
        set.Items.Add(new CdsButton("Cancel", CdsButtonKind.Secondary));
        set.Items.Add(new CdsButton("Create cluster", CdsButtonKind.Primary));
        var (layout, box) = set.LayoutParts;
        Assert.IsType<CdsEqualWidthRow>(layout);
        Assert.NotNull(box);
        Assert.Equal(392, box!.MaxContentWidth);
        Assert.Equal(196, ((CdsEqualWidthRow)layout).MaxCellWidth);

        // The box gives the row all the width it may have, so each cell arranges at 196 rather than its label width.
        Assert.Equal(392, MaxWidthModel.ContentWidth(1000, natural: 300, max: 392));
        Assert.Equal(320, MaxWidthModel.ContentWidth(320, natural: 300, max: 392));
        Assert.Equal(196, EqualWidthModel.ArrangedCell(392, 2, 196));

        set.IsStacked = true;
        (layout, box) = set.LayoutParts;
        Assert.IsType<Grid>(layout);
        Assert.Equal(196, box!.MaxContentWidth);

        set.IsFluid = true;
        (layout, box) = set.LayoutParts;
        Assert.IsType<Grid>(layout);
        Assert.Null(box);

        set.IsFluid = false;
        set.IsStacked = false;
        set.Items.Add(new CdsButton("Skip", CdsButtonKind.Ghost) { IsExpressive = true });
        (layout, box) = set.LayoutParts;
        Assert.IsType<CdsEqualWidthRow>(layout);
        Assert.Equal(960, box!.MaxContentWidth);
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
