namespace Maui.Carbon.Tests.Components;

/// <summary>Width math behind the wave 1 layout fixes (equal-width rows, max-width boxes, labels, tiles).</summary>
public class Wave1LayoutFixTests
{
    [Fact]
    public void Equal_cells_are_as_wide_as_the_widest_content()
    {
        Assert.Equal(90, EqualWidthModel.DesiredCell([40, 90, 62], 196));
        Assert.Equal(0, EqualWidthModel.DesiredCell([], 196));
    }

    [Fact]
    public void Equal_cells_never_exceed_the_maximum()
    {
        Assert.Equal(196, EqualWidthModel.DesiredCell([40, 300], 196));
        Assert.Equal(196, EqualWidthModel.ArrangedCell(1000, 3, 196));
    }

    [Fact]
    public void Equal_cells_shrink_to_a_finite_constraint_and_share_arranged_width()
    {
        Assert.Equal(100, EqualWidthModel.MeasuredCell([150, 90], 3, 300, 196));
        Assert.Equal(150, EqualWidthModel.MeasuredCell([150, 90], 3, double.PositiveInfinity, 196));
        Assert.Equal(80, EqualWidthModel.ArrangedCell(240, 3, double.PositiveInfinity));
        Assert.Equal(0, EqualWidthModel.ArrangedCell(240, 0, 196));
    }

    [Fact]
    public void Max_width_box_fills_up_to_the_maximum()
    {
        Assert.Equal(608, MaxWidthModel.ContentWidth(1200, 100, 608));
        Assert.Equal(400, MaxWidthModel.ContentWidth(400, 100, 608));
        Assert.Equal(100, MaxWidthModel.ContentWidth(double.PositiveInfinity, 100, 608));
        Assert.Equal(608, MaxWidthModel.ContentWidth(double.PositiveInfinity, 900, 608));
    }

    [Fact]
    public void Horizontal_progress_labels_are_capped_at_88_unless_spaced_equally()
    {
        Assert.Equal(88, ProgressIndicatorModel.LabelMaxWidth(vertical: false));
        Assert.True(double.IsPositiveInfinity(ProgressIndicatorModel.LabelMaxWidth(vertical: false, spaceEqually: true)));
        Assert.Equal(160, ProgressIndicatorModel.LabelMaxWidth(vertical: true));
        Assert.Equal(160, ProgressIndicatorModel.LabelMaxWidth(vertical: true, spaceEqually: true));
    }

    [Fact]
    public void Labelled_chevron_reserves_its_row_under_expanded_content()
    {
        Assert.Equal(48, ExpandableTileModel.ExpandedBottomPadding(chevronHasLabel: true));
        Assert.Equal(16, ExpandableTileModel.ExpandedBottomPadding(chevronHasLabel: false));
    }

    [Fact]
    public void File_rows_are_at_most_20rem()
    {
        Assert.Equal(320, FileUploaderModel.MaxRowWidth);
    }
}
