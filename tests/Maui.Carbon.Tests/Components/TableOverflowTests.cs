namespace Maui.Carbon.Tests.Components;

public class TableOverflowTests
{
    [Theory]
    [InlineData(600, 800, 800)]   // columns need more than the table: content scrolls sideways
    [InlineData(1000, 800, 1000)] // room to spare: content fills the table so star columns share it
    [InlineData(0, 800, 800)]     // not measured yet: the columns' minimum
    [InlineData(0, 0, -1)]        // nothing known: natural size
    public void Content_width_is_the_table_width_or_the_column_minimum(double viewport, double minimum, double expected) =>
        Assert.Equal(expected, CdsTable.ContentWidth(viewport, minimum));

    [Fact]
    public void A_table_with_fixed_columns_sizes_its_content_to_at_least_their_sum()
    {
        var table = new CdsTable
        {
            Head = new CdsTableHead
            {
                HeaderList =
                {
                    new CdsTableHeader { Text = "Name", Width = new GridLength(300) },
                    new CdsTableHeader { Text = "Region", Width = new GridLength(250) },
                    new CdsTableHeader { Text = "Notes", Width = GridLength.Star },
                },
            },
        };
        var scroll = table.GetVisualTreeDescendants().OfType<ScrollView>().First(s => s.Orientation == ScrollOrientation.Horizontal);
        Assert.Equal(300 + 250 + CdsTableTokens.MinimumFlexibleColumnWidth, ((View)scroll.Content!).WidthRequest);
    }

    static GridLength Px(double v) => new(v);
    static GridLength Stars(double v) => new(v, GridUnitType.Star);

    [Fact]
    public void Fixed_columns_share_the_spare_width_in_proportion_so_the_table_fills_its_container()
    {
        var columns = CdsTable.FillColumns([Px(200), Px(120), Px(90)], [false, false, false]);
        Assert.Equal([Stars(200), Stars(120), Stars(90)], columns);
    }

    [Fact]
    public void Flush_tool_columns_keep_their_width_when_the_content_columns_stretch()
    {
        // Selection checkbox, two data columns, the row menu.
        var columns = CdsTable.FillColumns([Px(48), Px(200), Px(90), Px(48)], [true, false, false, true]);
        Assert.Equal([Px(48), Stars(200), Stars(90), Px(48)], columns);
    }

    [Fact]
    public void A_star_column_already_takes_the_spare_width()
    {
        var columns = CdsTable.FillColumns([Px(200), GridLength.Star, Px(90)], [false, false, false]);
        Assert.Equal([Px(200), GridLength.Star, Px(90)], columns);
    }

    [Fact]
    public void Without_a_star_column_the_last_auto_column_takes_the_spare_width()
    {
        var columns = CdsTable.FillColumns([Px(200), GridLength.Auto, GridLength.Auto, Px(48)], [false, false, false, true]);
        Assert.Equal([Px(200), GridLength.Auto, GridLength.Star, Px(48)], columns);
    }

    [Fact]
    public void A_table_of_fixed_columns_lays_them_out_as_proportional_stars_with_their_sum_as_minimum()
    {
        var table = new CdsTable
        {
            Head = new CdsTableHead
            {
                HeaderList =
                {
                    new CdsTableHeader { Text = "Name", Width = new GridLength(200) },
                    new CdsTableHeader { Text = "Nodes", Width = new GridLength(90) },
                },
            },
        };
        var scroll = table.GetVisualTreeDescendants().OfType<ScrollView>().First(s => s.Orientation == ScrollOrientation.Horizontal);
        var grid = (Grid)scroll.Content!;
        Assert.Equal(290, grid.WidthRequest);
        Assert.Equal([Stars(200), Stars(90)], grid.ColumnDefinitions.Select(c => c.Width));
    }

    [Fact]
    public void A_sticky_header_sits_on_the_table_surface()
    {
        var table = new CdsTable
        {
            StickyHeader = true,
            Head = new CdsTableHead { HeaderList = { new CdsTableHeader { Text = "Name", Width = new GridLength(200) } } },
        };
        var root = new ContentView { Content = table, Resources = CdsPalettes.CreateThemeDictionary(CdsThemeName.White) };
        var scroll = table.GetVisualTreeDescendants().OfType<ScrollView>().First(s => s.Orientation == ScrollOrientation.Horizontal);
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.White, CdsTableTokens.Background), ((VisualElement)scroll.Content!).BackgroundColor);
        GC.KeepAlive(root);
    }
}
