namespace Maui.Carbon.Tests.Components;

public class TagSetTests
{
    static readonly double[] Widths = [60, 70, 80, 90]; // each includes the 8 gap

    [Fact]
    public void Everything_fits_in_a_wide_row() =>
        Assert.Equal(4, TagSetModel.FitCount(Widths, 400, 40, 0, false));

    [Fact]
    public void Tags_are_dropped_until_the_overflow_fits()
    {
        // 60 + 70 + 80 = 210 fit in 220, the 90 does not; 10 left is less than the 40 overflow, so drop the 80.
        Assert.Equal(2, TagSetModel.FitCount(Widths, 220, 40, 0, false));
        // 250: three fit, 40 left fits the overflow.
        Assert.Equal(3, TagSetModel.FitCount(Widths, 250, 40, 0, false));
    }

    [Fact]
    public void Nothing_fits_shows_only_the_overflow()
    {
        Assert.Equal(0, TagSetModel.FitCount(Widths, 50, 40, 0, false));
        Assert.Equal(0, TagSetModel.FitCount(Widths, 70, 40, 0, false));
    }

    [Fact]
    public void Max_visible_caps_the_fit()
    {
        Assert.Equal(2, TagSetModel.FitCount(Widths, 400, 40, 2, false));
        Assert.Equal(4, TagSetModel.FitCount(Widths, 400, 40, 9, false));
    }

    [Fact]
    public void Multiline_and_unbounded_show_everything_up_to_the_cap()
    {
        Assert.Equal(4, TagSetModel.FitCount(Widths, 100, 40, 0, true));
        Assert.Equal(3, TagSetModel.FitCount(Widths, 100, 40, 3, true));
        Assert.Equal(4, TagSetModel.FitCount(Widths, double.PositiveInfinity, 40, 0, false));
        Assert.Equal(0, TagSetModel.FitCount([], 100, 40, 0, false));
    }

    [Fact]
    public void Arrange_places_tags_in_a_row_with_the_gap_and_the_overflow_last()
    {
        var sizes = new List<Size> { new(52, 24), new(62, 24), new(72, 24) };
        var r = TagSetModel.Arrange(sizes, 2, new Size(32, 24), 300, CdsTagSetAlign.Start, false, 8);
        Assert.Equal(2, r.Tags.Length);
        Assert.Equal(new Rect(0, 0, 52, 24), r.Tags[0]);
        Assert.Equal(new Rect(60, 0, 62, 24), r.Tags[1]);
        Assert.Equal(new Rect(130, 0, 32, 24), r.Overflow);
        Assert.Equal(new Size(170, 24), r.ContentSize);
    }

    [Theory]
    [InlineData(CdsTagSetAlign.Start, 0)]
    [InlineData(CdsTagSetAlign.Center, 70)]
    [InlineData(CdsTagSetAlign.End, 140)]
    public void Arrange_aligns_the_row(CdsTagSetAlign align, double x)
    {
        var r = TagSetModel.Arrange([new Size(52, 24)], 1, null, 200, align, false, 8);
        Assert.Equal(x, r.Tags[0].X);
        Assert.Null(r.Overflow);
    }

    [Fact]
    public void Multiline_wraps_rows_the_gap_apart()
    {
        var sizes = Enumerable.Repeat(new Size(52, 24), 5).ToList();
        var r = TagSetModel.Arrange(sizes, 5, null, 130, CdsTagSetAlign.Start, true, 8);
        Assert.Equal(new Rect(0, 0, 52, 24), r.Tags[0]);
        Assert.Equal(new Rect(60, 0, 52, 24), r.Tags[1]);
        Assert.Equal(new Rect(0, 32, 52, 24), r.Tags[2]);
        Assert.Equal(new Rect(0, 64, 52, 24), r.Tags[4]);
        Assert.Equal(88, r.ContentSize.Height);
    }

    [Fact]
    public void Shorter_items_are_centered_in_their_row()
    {
        var r = TagSetModel.Arrange([new Size(52, 32), new Size(40, 24)], 2, null, 300, CdsTagSetAlign.Start, false, 8);
        Assert.Equal(4, r.Tags[1].Y);
        Assert.Equal(32, r.ContentSize.Height);
    }

    [Fact]
    public void Arrange_of_nothing_is_empty()
    {
        var r = TagSetModel.Arrange([], 0, null, 300, CdsTagSetAlign.Start, false, 8);
        Assert.Empty(r.Tags);
        Assert.Equal(Size.Zero, r.ContentSize);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(3, 3, false)]
    [InlineData(10, 10, false)]
    [InlineData(11, 10, true)]
    [InlineData(25, 10, true)]
    public void Popover_lists_ten_and_offers_view_all_past_that(int hidden, int listed, bool showsAll)
    {
        var (l, s) = TagSetModel.PopoverList(hidden);
        Assert.Equal(listed, l);
        Assert.Equal(showsAll, s);
    }

    [Theory]
    [InlineData("Twelve", "dozen", "doz", true)]
    [InlineData("Twelve", "dozen", "TWE", true)]
    [InlineData("Twelve", null, "one", false)]
    [InlineData("Twelve", null, "", true)]
    [InlineData("Twelve", null, null, true)]
    [InlineData(null, null, "x", false)]
    public void Search_matches_text_or_search_text(string? text, string? search, string? query, bool expected) =>
        Assert.Equal(expected, TagSetModel.Matches(text, search, query));

    [Fact]
    public void Overflow_text_is_plus_n() => Assert.Equal("+7", TagSetModel.OverflowText(7));

    [Fact]
    public void Popover_entries_use_inverse_tokens()
    {
        Assert.Equal(new CdsTagSetEntryVisuals(null, CdsColorKeys.TextInverse), CdsTagSetTokens.Entry(CdsTagSetOverflowType.Default));
        Assert.Equal(new CdsTagSetEntryVisuals(CdsColorKeys.BackgroundInverseHover, CdsColorKeys.TextInverse), CdsTagSetTokens.Entry(CdsTagSetOverflowType.Tag));
        Assert.Equal(0, CdsTagSetTokens.EntrySpacing(CdsTagSetOverflowType.Default));
        Assert.Equal(8, CdsTagSetTokens.EntrySpacing(CdsTagSetOverflowType.Tag));
        Assert.Equal(8, CdsTagSetTokens.Gap);
        Assert.Equal(16, CdsTagSetTokens.PopoverPadding);
    }

    [Fact]
    public void Defaults_follow_the_readme()
    {
        var set = new CdsTagSet();
        Assert.Equal(CdsTagSetAlign.Start, set.Align);
        Assert.Equal(0, set.MaxVisible);
        Assert.False(set.IsMultiline);
        Assert.Equal(CdsTagSize.Md, set.Size);
        Assert.Equal(CdsPlacement.Bottom, set.OverflowPlacement);
        Assert.Equal(CdsTagSetOverflowType.Default, set.OverflowType);
        Assert.Equal("View all tags", set.ShowAllTagsLabel);
        Assert.Equal("All tags", set.AllTagsModalTitle);
        Assert.Equal("Search all tags", set.AllTagsModalSearchLabel);
        Assert.Equal("Search all tags", set.AllTagsModalSearchPlaceholderText);
        Assert.Equal("List of all tags", set.AllTagsModalAriaLabel);
        Assert.Equal("Close", set.AllTagsModalCloseLabel);
        Assert.Equal("Clear search input", set.AllTagsModalSearchClearLabel);
        Assert.Empty(set.OverflowItems);
    }

    [Fact]
    public void Max_visible_moves_the_rest_into_the_overflow()
    {
        var set = new CdsTagSet();
        var changes = 0;
        set.OverflowItemsChanged += (_, _) => changes++;
        for (var i = 1; i <= 5; i++) set.Items.Add(new CdsTagSetItem($"Tag {i}", CdsTagType.Blue));
        Assert.Equal(5, set.VisibleCount);
        set.MaxVisible = 2;
        Assert.Equal(2, set.VisibleCount);
        Assert.Equal(["Tag 3", "Tag 4", "Tag 5"], set.OverflowItems.Select(t => t.Text));
        Assert.True(changes > 0);
        set.Items.RemoveAt(4);
        Assert.Equal(2, set.OverflowItems.Count);
        set.Items.Clear();
        Assert.Empty(set.OverflowItems);
    }

    [Fact]
    public void Items_are_parented_to_the_set()
    {
        var set = new CdsTagSet();
        var item = new CdsTagSetItem("prod-eu-de-1");
        set.Items.Add(item);
        Assert.Same(set, item.Parent);
        set.Items.Remove(item);
        Assert.Null(item.Parent);
    }

    [Fact]
    public void Every_property_can_be_set_without_a_platform()
    {
        var set = new CdsTagSet
        {
            Align = CdsTagSetAlign.End,
            MaxVisible = 1,
            IsMultiline = true,
            Size = CdsTagSize.Sm,
            OverflowPlacement = CdsPlacement.TopStart,
            OverflowType = CdsTagSetOverflowType.Tag,
            ShowAllTagsLabel = "See all",
            AllTagsModalTitle = "Labels",
            AllTagsModalSearchLabel = "Search labels",
            AllTagsModalSearchPlaceholderText = "Search labels",
            AllTagsModalAriaLabel = "All labels",
            AllTagsModalCloseLabel = "Close labels",
            AllTagsModalSearchClearLabel = "Clear",
            OverflowCommand = new Command(() => { }),
        };
        for (var i = 0; i < 14; i++)
            set.Items.Add(new CdsTagSetItem($"label-{i}", CdsTagType.Teal) { SearchText = "x", Icon = CdsIconName.Add, IsDismissible = i % 2 == 0, IsEnabled = i != 3 });
        set.IsOverflowOpen = true;
        set.IsOverflowOpen = false;
        set.IsAllTagsModalOpen = true;
        set.IsAllTagsModalOpen = false;
        set.Items[0].Text = "renamed";
        set.Items[0].IsDismissible = false;
        set.OverflowType = CdsTagSetOverflowType.Default;
        set.Size = CdsTagSize.Lg;
        set.IsEnabled = false;
    }

    [Fact]
    public void Dismissing_an_item_raises_its_event_and_command()
    {
        var item = new CdsTagSetItem("Region: us-east") { IsDismissible = true };
        object? parameter = null;
        var raised = false;
        item.Dismissed += (_, _) => raised = true;
        item.DismissCommand = new Command<object>(p => parameter = p);
        item.RaiseDismissed();
        Assert.True(raised);
        Assert.Same(item, parameter);
    }

    [Fact]
    public void Item_defaults()
    {
        var item = new CdsTagSetItem();
        Assert.Equal(CdsTagType.Gray, item.Type);
        Assert.True(item.IsEnabled);
        Assert.False(item.IsDismissible);
        Assert.Equal("Dismiss", item.DismissLabel);
    }

    [Fact]
    public void Overflow_event_args_carry_the_items_and_handled()
    {
        var items = new List<CdsTagSetItem> { new("a"), new("b") };
        var args = new CdsTagSetOverflowEventArgs(items) { Handled = true };
        Assert.Same(items, args.OverflowItems);
        Assert.True(args.Handled);
    }

    [Fact]
    public void A_loaded_set_can_open_its_popover_and_modal()
    {
        var set = new CdsTagSet { MaxVisible = 1 };
        for (var i = 0; i < 12; i++) set.Items.Add(new CdsTagSetItem($"Tag {i}"));
        var host = ActionTestHost.Show(set);
        set.IsOverflowOpen = true;
        Assert.True(set.IsOverflowOpen);
        Assert.Equal(11, set.OverflowItems.Count);
        set.IsOverflowOpen = false;
        set.IsAllTagsModalOpen = true;
        set.IsAllTagsModalOpen = false;
        Assert.NotNull(host);
    }
}
