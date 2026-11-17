namespace Maui.Carbon.Tests.Components;

public class DataTableSkeletonTests
{
    [Fact]
    public void Bars_title_and_toolbar_measurements_follow_the_readme()
    {
        Assert.Equal(64, CdsDataTableSkeletonTokens.BarWidth);
        Assert.Equal(16, CdsDataTableSkeletonTokens.BarHeight);
        Assert.Equal((120d, 24d), CdsDataTableSkeletonTokens.TitleBar);
        Assert.Equal((160d, 16d), CdsDataTableSkeletonTokens.DescriptionBar);
        Assert.Equal(150, CdsDataTableSkeletonTokens.ToolbarButtonWidth);
        Assert.Equal(48, CdsDataTableSkeletonTokens.RowHeight);
    }

    [Fact]
    public void Header_bars_invert_the_skeleton_tokens()
    {
        Assert.Equal((CdsColorKeys.SkeletonBackground, CdsColorKeys.SkeletonElement), CdsDataTableSkeletonTokens.BarColors(false));
        Assert.Equal((CdsColorKeys.SkeletonElement, CdsColorKeys.SkeletonBackground), CdsDataTableSkeletonTokens.BarColors(true));
    }

    [Fact]
    public void Rows_are_layer_01_and_zebra_alternates_layer_accent()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsDataTableSkeletonTokens.RowFill(1, false));
        Assert.Equal(CdsColorKeys.Layer01, CdsDataTableSkeletonTokens.RowFill(0, true));
        Assert.Equal(CdsColorKeys.LayerAccent01, CdsDataTableSkeletonTokens.RowFill(1, true));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsDataTableSkeletonTokens.RowRule(0, false));
        Assert.Equal(CdsColorKeys.LayerAccent01, CdsDataTableSkeletonTokens.RowRule(1, true));
    }

    [Fact]
    public void Defaults_and_every_property_without_a_platform()
    {
        var skeleton = new CdsDataTableSkeleton();
        Assert.Equal(5, skeleton.RowCount);
        Assert.Equal(5, skeleton.ColumnCount);
        Assert.True(skeleton.ShowHeader);
        Assert.True(skeleton.ShowToolbar);
        Assert.False(skeleton.UseZebraStyles);
        skeleton.RowCount = 4;
        skeleton.ColumnCount = 3;
        skeleton.ShowHeader = false;
        skeleton.ShowToolbar = false;
        skeleton.UseZebraStyles = true;
        skeleton.Label = "Clusters (loading)";
        skeleton.Headers.Add("Name");
        skeleton.Headers.Add("Status");
        Assert.Equal(2, skeleton.Headers.Count);
    }

    [Fact]
    public void Counts_are_coerced_to_sane_values()
    {
        var skeleton = new CdsDataTableSkeleton { RowCount = -3, ColumnCount = 0 };
        Assert.Equal(0, skeleton.RowCount);
        Assert.Equal(1, skeleton.ColumnCount);
    }
}
