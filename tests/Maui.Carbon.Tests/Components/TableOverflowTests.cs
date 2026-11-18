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
}
