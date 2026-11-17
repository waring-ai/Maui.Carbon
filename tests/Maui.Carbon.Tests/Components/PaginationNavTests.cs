namespace Maui.Carbon.Tests.Components;

public class PaginationNavTests
{
    static string Describe(IReadOnlyList<NavSlot> slots) =>
        string.Join(" ", slots.Select(s => s.Kind == NavSlotKind.Page ? $"{s.Start + 1}" : $"[{s.Start + 1}-{s.End + 1}]"));

    [Fact]
    public void Everything_shows_when_it_fits()
    {
        Assert.Equal("1 2 3 4 5", Describe(PaginationNavModel.Build(0, 5, 10)));
        Assert.Equal("1 2 3 4 5 6 7 8 9 10", Describe(PaginationNavModel.Build(4, 10, 10)));
        Assert.Equal("1", Describe(PaginationNavModel.Build(0, 1, 10)));
        Assert.Empty(PaginationNavModel.Build(0, 0, 10));
    }

    [Theory]
    [InlineData(0, "1 2 3 4 5 [6-13] 14")]
    [InlineData(1, "1 2 3 4 5 [6-13] 14")]
    [InlineData(4, "1 2 3 4 5 [6-13] 14")]
    [InlineData(5, "1 [2-4] 5 6 7 [8-13] 14")]
    [InlineData(6, "1 [2-5] 6 7 8 [9-13] 14")]
    [InlineData(8, "1 [2-7] 8 9 10 [11-13] 14")]
    [InlineData(9, "1 [2-9] 10 11 12 13 14")]
    [InlineData(13, "1 [2-9] 10 11 12 13 14")]
    public void A_seven_slot_strip_over_fourteen_pages(int page, string expected) =>
        Assert.Equal(expected, Describe(PaginationNavModel.Build(page, 14, 7)));

    [Fact]
    public void The_strip_never_exceeds_the_slots_shown()
    {
        for (var total = 1; total <= 40; total++)
        {
            for (var shown = 5; shown <= 10; shown++)
            {
                for (var page = 0; page < total; page++)
                {
                    var slots = PaginationNavModel.Build(page, total, shown);
                    Assert.True(slots.Count <= Math.Max(shown, total <= shown ? total : shown), $"{total}/{shown}/{page}: {Describe(slots)}");
                }
            }
        }
    }

    [Fact]
    public void The_first_and_last_page_always_show_and_the_strip_never_starts_or_ends_with_an_overflow()
    {
        for (var total = 2; total <= 40; total++)
        {
            for (var shown = 4; shown <= 10; shown++)
            {
                for (var page = 0; page < total; page++)
                {
                    var slots = PaginationNavModel.Build(page, total, shown);
                    var text = Describe(slots);
                    Assert.Equal(NavSlotKind.Page, slots[0].Kind);
                    Assert.Equal(NavSlotKind.Page, slots[^1].Kind);
                    Assert.Equal(0, slots[0].Start);
                    Assert.Equal(total - 1, slots[^1].Start);
                    Assert.Contains(slots, s => s.Kind == NavSlotKind.Page && s.Start == page);
                    Assert.True(text.Length > 0);
                }
            }
        }
    }

    [Fact]
    public void Every_page_is_either_a_button_or_inside_exactly_one_overflow_in_order()
    {
        for (var total = 1; total <= 30; total++)
        {
            for (var shown = 4; shown <= 9; shown++)
            {
                for (var page = 0; page < total; page++)
                {
                    var seen = new List<int>();
                    foreach (var slot in PaginationNavModel.Build(page, total, shown))
                    {
                        for (var p = slot.Start; p <= slot.End; p++) seen.Add(p);
                        if (slot.Kind == NavSlotKind.Overflow) Assert.True(slot.Count >= 2, "an overflow hides at least two pages");
                    }
                    Assert.Equal(Enumerable.Range(0, total), seen);
                }
            }
        }
    }

    [Fact]
    public void A_hidden_range_of_one_page_shows_that_page()
    {
        // Four slots over six pages, on page 2: 1 [2-?]... never a one-page overflow.
        var slots = PaginationNavModel.Build(2, 6, 4);
        Assert.DoesNotContain(slots, s => s.Kind == NavSlotKind.Overflow && s.Count < 2);
    }

    [Theory]
    [InlineData(CdsSize.Md, 10, 5)]
    [InlineData(CdsSize.Md, 2, 4)]
    [InlineData(CdsSize.Sm, 10, 7)]
    [InlineData(CdsSize.Sm, 5, 5)]
    [InlineData(CdsSize.Lg, 10, 10)]
    [InlineData(CdsSize.Lg, 2, 4)]
    public void Items_shown_is_limited_by_size(CdsSize size, int requested, int expected) =>
        Assert.Equal(expected, PaginationNavModel.CoerceItemsShown(size, requested));

    [Theory]
    [InlineData(0, 5, false, 1)]
    [InlineData(3, 5, false, 4)]
    [InlineData(4, 5, false, -1)]
    [InlineData(4, 5, true, 0)]
    [InlineData(0, 0, true, -1)]
    public void Next_steps_forward_and_wraps_only_when_looping(int page, int total, bool loop, int expected) =>
        Assert.Equal(expected, PaginationNavModel.Next(page, total, loop));

    [Theory]
    [InlineData(2, 5, false, 1)]
    [InlineData(0, 5, false, -1)]
    [InlineData(0, 5, true, 4)]
    [InlineData(0, 0, true, -1)]
    public void Previous_steps_back_and_wraps_only_when_looping(int page, int total, bool loop, int expected) =>
        Assert.Equal(expected, PaginationNavModel.Previous(page, total, loop));

    [Theory]
    [InlineData(-3, 5, 0)]
    [InlineData(2, 5, 2)]
    [InlineData(9, 5, 4)]
    [InlineData(3, 0, 0)]
    public void Pages_clamp(int page, int total, int expected) => Assert.Equal(expected, PaginationNavModel.ClampPage(page, total));

    [Fact]
    public void The_announcement_is_one_based()
    {
        Assert.Equal("Page 3 of 14", PaginationNavModel.Announcement(2, 14));
    }

    // ----- tokens -----

    [Fact]
    public void Tokens_follow_the_readme()
    {
        Assert.Equal(CdsColorKeys.BackgroundHover, CdsPaginationNavTokens.Hover);
        Assert.Equal(CdsColorKeys.BorderInteractive, CdsPaginationNavTokens.ActiveBar);
        Assert.Equal(CdsColorKeys.TextPrimary, CdsPaginationNavTokens.Text(false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsPaginationNavTokens.Text(true));
        Assert.Equal(16, CdsPaginationNavTokens.ActiveBarWidth);
        Assert.Equal(32, CdsPaginationNavTokens.Height(CdsSize.Sm));
        Assert.Equal(40, CdsPaginationNavTokens.Height(CdsSize.Md));
        Assert.Equal(48, CdsPaginationNavTokens.Height(CdsSize.Lg));
    }

    // ----- the control -----

    [Fact]
    public void Defaults_and_every_property_without_a_platform()
    {
        var nav = new CdsPaginationNav { TotalItems = 14, ItemsShown = 7 };
        Assert.Equal(0, nav.Page);
        Assert.Equal(CdsSize.Lg, nav.Size);
        Assert.Equal("pagination", nav.Label);
        nav.Page = 6;
        nav.Loop = true;
        nav.IsOverflowDisabled = true;
        nav.Label = "Search results pages";
        nav.Size = CdsSize.Sm;
        Assert.Equal(6, nav.Page);
        Assert.Equal("1 [2-5] 6 7 8 [9-13] 14".Replace("6 7 8", "6 7 8"), Describe(nav.Slots));
    }

    [Fact]
    public void The_page_is_clamped_when_the_total_shrinks()
    {
        var nav = new CdsPaginationNav { TotalItems = 14, Page = 12 };
        nav.TotalItems = 5;
        Assert.Equal(4, nav.Page);
        nav.Page = 99;
        Assert.Equal(4, nav.Page);
    }
}
