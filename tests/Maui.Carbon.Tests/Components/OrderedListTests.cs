namespace Maui.Carbon.Tests.Components;

public class ListMarkerTests
{
    [Fact]
    public void Top_level_ordered_lists_number()
    {
        Assert.Equal("1.", CdsListMarkers.Marker(true, false, 0));
        Assert.Equal("12.", CdsListMarkers.Marker(true, false, 11));
    }

    [Fact]
    public void Nested_ordered_lists_use_lower_latin()
    {
        Assert.Equal("a.", CdsListMarkers.Marker(true, true, 0));
        Assert.Equal("c.", CdsListMarkers.Marker(true, true, 2));
    }

    [Theory]
    [InlineData(0, "a")]
    [InlineData(25, "z")]
    [InlineData(26, "aa")]
    [InlineData(27, "ab")]
    [InlineData(51, "az")]
    [InlineData(52, "ba")]
    [InlineData(701, "zz")]
    [InlineData(702, "aaa")]
    [InlineData(-5, "a")]
    public void Latin_letters_continue_past_z(int index, string expected) => Assert.Equal(expected, CdsListMarkers.LowerLatin(index));

    [Fact]
    public void Unordered_lists_use_an_en_dash_and_nested_ones_a_square()
    {
        Assert.Equal("–", CdsListMarkers.Marker(false, false, 4));
        Assert.Equal("▪", CdsListMarkers.Marker(false, true, 4));
    }

    [Fact]
    public void Marker_columns_and_indents()
    {
        Assert.Equal(24, CdsListMarkers.MarkerWidth(true, false));
        Assert.Equal(16, CdsListMarkers.MarkerWidth(false, false));
        Assert.Equal(12, CdsListMarkers.MarkerWidth(false, true));
        // A top-level ordered list hangs its marker 24 before its box (outside it, as in Carbon).
        Assert.Equal(-24, CdsListMarkers.LeftMargin(true, false));
        Assert.Equal(0, CdsListMarkers.LeftMargin(false, false));
        // Nested text starts 32 + 4 after the parent's text; the marker hangs before it.
        Assert.Equal(36, CdsListMarkers.LeftMargin(false, true) + CdsListMarkers.MarkerWidth(false, true));
        Assert.Equal(36, CdsListMarkers.LeftMargin(true, true) + CdsListMarkers.MarkerWidth(true, true));
    }

    [Fact]
    public void Type_style_follows_expressive()
    {
        Assert.Equal(CdsTypeStyle.Body01, CdsListMarkers.TypeStyle(false));
        Assert.Equal(CdsTypeStyle.Body02, CdsListMarkers.TypeStyle(true));
    }
}

public class ListControlTests
{
    [Fact]
    public void Nested_lists_know_they_are_nested()
    {
        var nested = new CdsOrderedList { Items = { new CdsListItem("List clusters"), new CdsListItem("Download the kubeconfig") } };
        var list = new CdsOrderedList
        {
            Items =
            {
                new CdsListItem("Install the CLI and log in"),
                new CdsListItem("Target your cluster") { NestedList = nested },
                new CdsListItem("Apply the deployment file"),
            },
        };
        Assert.False(list.IsNested);
        Assert.True(nested.IsNested);
        Assert.Equal("a.", nested.MarkerFor(0));
        Assert.Equal("2.", list.MarkerFor(1));
    }

    [Fact]
    public void Unordered_lists_construct_with_expressive_type()
    {
        var list = new CdsUnorderedList
        {
            IsExpressive = true,
            Items = { new CdsListItem("Dedicated hardware") { Body = new Label { Text = "Details" } }, new CdsListItem("24/7 support") },
        };
        Assert.Equal(2, list.Items.Count);
        Assert.Equal("–", list.MarkerFor(0));
    }

    [Fact]
    public void Native_and_nested_ordered_markers_end_before_the_text()
    {
        Assert.False(CdsListMarkers.IsMarkerEndAligned(true, false, false)); // Carbon counter at the column start
        Assert.True(CdsListMarkers.IsMarkerEndAligned(true, false, true));   // native decimal
        Assert.True(CdsListMarkers.IsMarkerEndAligned(true, true, false));   // nested lower-latin
        Assert.False(CdsListMarkers.IsMarkerEndAligned(false, false, true)); // bullets never
    }

    [Fact]
    public void Ordered_lists_can_use_native_markers()
    {
        var list = new CdsOrderedList { IsNative = true };
        list.Items.Add(new CdsListItem("Install the CLI"));
        Assert.True(list.IsNative);
        Assert.True(list.IsNativeMarkers);
        Assert.False(new CdsOrderedList().IsNativeMarkers);
        Assert.Equal("1.", list.MarkerFor(0));
    }
}
