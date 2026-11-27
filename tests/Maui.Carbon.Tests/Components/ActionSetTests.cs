namespace Maui.Carbon.Tests.Components;

public class ActionSetTests
{
    static readonly CdsButtonKind P = CdsButtonKind.Primary, S = CdsButtonKind.Secondary, G = CdsButtonKind.Ghost,
        T = CdsButtonKind.Tertiary, D = CdsButtonKind.Danger;

    [Fact]
    public void Row_order_puts_ghost_first_and_primary_last()
    {
        CdsButtonKind[] kinds = [P, G, S, T];
        var order = ActionSetModel.Arrange(kinds, stacking: false);
        Assert.Equal([G, T, S, P], order.Select(i => kinds[i]));
    }

    [Fact]
    public void Stacked_order_puts_primary_on_top()
    {
        CdsButtonKind[] kinds = [S, P, D];
        var order = ActionSetModel.Arrange(kinds, stacking: true);
        Assert.Equal([P, D, S], order.Select(i => kinds[i]));
    }

    [Theory]
    [InlineData(CdsActionSetSize.Sm, 1, true)]
    [InlineData(CdsActionSetSize.Md, 2, false)]
    [InlineData(CdsActionSetSize.Md, 3, true)]
    [InlineData(CdsActionSetSize.Lg, 3, false)]
    [InlineData(CdsActionSetSize.Xl, 4, false)]
    public void Stacking_follows_size_and_count(CdsActionSetSize size, int count, bool stacks) =>
        Assert.Equal(stacks, ActionSetModel.WillStack(size, count));

    [Fact]
    public void Single_button_fills_medium_and_takes_half_of_large()
    {
        Assert.Equal([480], ActionSetModel.RowWidths([P], CdsActionSetSize.Md, 480));
        Assert.Equal([320], ActionSetModel.RowWidths([P], CdsActionSetSize.Lg, 640));
        Assert.Equal([640], ActionSetModel.RowWidths([G], CdsActionSetSize.Lg, 640));
    }

    [Fact]
    public void Two_buttons_split_medium_and_large_sets()
    {
        Assert.Equal([240, 240], ActionSetModel.RowWidths([S, P], CdsActionSetSize.Md, 480));
        Assert.Equal([320, 320], ActionSetModel.RowWidths([G, P], CdsActionSetSize.Lg, 640));
    }

    [Fact]
    public void Wide_sets_use_capped_quarters_and_ghost_takes_the_rest()
    {
        var widths = ActionSetModel.RowWidths([G, S, P], CdsActionSetSize.Xl, 1040);
        Assert.Equal(ActionSetModel.MaxQuarterWidth, widths[1]);
        Assert.Equal(ActionSetModel.MaxQuarterWidth, widths[2]);
        Assert.Equal(1040 - 2 * ActionSetModel.MaxQuarterWidth, widths[0]);
        Assert.Equal([160, 160, 160], ActionSetModel.RowWidths([T, S, P], CdsActionSetSize.Lg, 640));
    }

    [Fact]
    public void Problems_report_carbon_rules()
    {
        Assert.Empty(ActionSetModel.Problems([G, S, P], CdsActionSetSize.Lg));
        Assert.Contains(ActionSetModel.Problems([P, P], CdsActionSetSize.Lg), p => p.Contains("one primary"));
        Assert.Contains(ActionSetModel.Problems([G, G], CdsActionSetSize.Lg), p => p.Contains("one ghost"));
        Assert.Contains(ActionSetModel.Problems([G, P], CdsActionSetSize.Sm), p => p.Contains("mix a ghost"));
        Assert.Contains(ActionSetModel.Problems([S, S, S, P, T], CdsActionSetSize.Xl), p => p.Contains("four"));
    }

    [Fact]
    public void Control_sorts_and_sizes_its_buttons()
    {
        var primary = new CdsButton("Save", CdsButtonKind.Primary);
        var cancel = new CdsButton("Cancel", CdsButtonKind.Secondary);
        var set = new CdsActionSet { Size = CdsActionSetSize.Md, ButtonSize = CdsSize.Lg };
        set.Actions.Add(primary);
        set.Actions.Add(cancel);
        Assert.False(set.IsStacking);
        Assert.Equal([cancel, primary], set.Shown);
        Assert.All(set.Shown, b => Assert.Equal(CdsSize.Lg, b.Size));

        set.Size = CdsActionSetSize.Sm;
        Assert.True(set.IsStacking);
        Assert.Equal([primary, cancel], set.Shown);

        cancel.Kind = CdsButtonKind.Danger;
        set.Size = CdsActionSetSize.Lg;
        Assert.Equal([cancel, primary], set.Shown);
    }
}
