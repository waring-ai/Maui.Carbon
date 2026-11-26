namespace Maui.Carbon.Tests.Components;

public class PaginationTests
{
    [Theory]
    [InlineData(0, 10, 1)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(103, 10, 11)]
    [InlineData(100, 25, 4)]
    [InlineData(50, 0, 1)]
    public void Page_count_rounds_up_and_is_at_least_one(int total, int size, int pages) =>
        Assert.Equal(pages, PaginationModel.PageCount(total, size));

    [Theory]
    [InlineData(0, 11, 1)]
    [InlineData(-5, 11, 1)]
    [InlineData(5, 11, 5)]
    [InlineData(12, 11, 11)]
    [InlineData(3, 0, 1)]
    public void Pages_clamp_into_range(int page, int count, int expected) => Assert.Equal(expected, PaginationModel.ClampPage(page, count));

    [Theory]
    [InlineData(1, 10, 103, 1, 10)]
    [InlineData(2, 10, 103, 11, 20)]
    [InlineData(11, 10, 103, 101, 103)]
    [InlineData(99, 10, 103, 101, 103)] // clamped to the last page
    [InlineData(1, 10, 0, 0, 0)]
    [InlineData(1, 10, 4, 1, 4)]
    public void Range_is_the_first_and_last_item_of_the_page(int page, int size, int total, int min, int max) =>
        Assert.Equal((min, max), PaginationModel.Range(page, size, total));

    [Theory]
    [InlineData(1, 10, 103, "1–10 of 103 items")]
    [InlineData(11, 10, 103, "101–103 of 103 items")]
    [InlineData(1, 10, 0, "0–0 of 0 items")]
    public void Range_text_uses_an_en_dash(int page, int size, int total, string text) =>
        Assert.Equal(text, PaginationModel.RangeText(page, size, total, false));

    [Fact]
    public void Unknown_totals_show_the_item_span_without_a_total()
    {
        Assert.Equal("21–30 items", PaginationModel.RangeText(3, 10, 0, true));
        Assert.Equal("page 3", PaginationModel.PageText(3));
    }

    [Fact]
    public void Page_range_text_pluralizes()
    {
        Assert.Equal("of 11 pages", PaginationModel.PageRangeText(11));
        Assert.Equal("of 1 page", PaginationModel.PageRangeText(1));
    }

    [Fact]
    public void Changing_the_page_size_returns_to_page_1()
    {
        var pagination = new CdsPagination { TotalItems = 103, PageSize = 10, Page = 5 };
        CdsPaginationChangedEventArgs? change = null;
        pagination.PageChanged += (_, e) => change = e;
        pagination.ChangePageSize(20);
        Assert.Equal(1, pagination.Page);
        Assert.Equal(20, pagination.PageSize);
        Assert.Equal(1, change!.Page);
        Assert.Equal(20, change.PageSize);
    }

    [Fact]
    public void Back_is_enabled_after_the_first_page_and_forward_before_the_last()
    {
        Assert.False(PaginationModel.CanGoBack(1));
        Assert.True(PaginationModel.CanGoBack(2));
        Assert.True(PaginationModel.CanGoForward(1, 3, false, false));
        Assert.False(PaginationModel.CanGoForward(3, 3, false, false));
    }

    [Fact]
    public void With_unknown_pages_forward_stays_enabled_until_it_is_the_last_page()
    {
        Assert.True(PaginationModel.CanGoForward(7, 1, true, false));
        Assert.False(PaginationModel.CanGoForward(7, 1, true, true));
    }

    [Fact]
    public void Page_sizes_are_normalized_with_a_default()
    {
        Assert.Equal([10, 20, 30, 40, 50], PaginationModel.NormalizeSizes(null));
        Assert.Equal([10, 20, 30, 40, 50], PaginationModel.NormalizeSizes([0, -3]));
        Assert.Equal([25, 50], PaginationModel.NormalizeSizes([25, 25, 0, 50]));
    }

    [Fact]
    public void The_page_size_resolves_to_a_listed_size()
    {
        Assert.Equal(20, PaginationModel.ResolvePageSize(20, [10, 20]));
        Assert.Equal(10, PaginationModel.ResolvePageSize(15, [10, 20]));
    }

    [Theory]
    [InlineData("7", 20, 7)]
    [InlineData(" 7 ", 20, 7)]
    [InlineData("0", 20, 1)]
    [InlineData("99", 20, 20)]
    [InlineData("abc", 20, null)]
    [InlineData("", 20, null)]
    [InlineData("-3", 20, null)]
    public void Typed_pages_parse_and_clamp(string text, int count, int? expected) =>
        Assert.Equal(expected, PaginationModel.ParsePageInput(text, count));

    [Fact]
    public void Many_pages_switch_to_a_numeric_field()
    {
        Assert.False(PaginationModel.UsesPageInput(100));
        Assert.True(PaginationModel.UsesPageInput(101));
    }

    // ----- tokens -----

    [Fact]
    public void The_bar_uses_layer_01_with_subtle_rules_and_hover()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsPaginationTokens.Background);
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsPaginationTokens.Rule);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsPaginationTokens.Hover);
        Assert.Equal(CdsColorKeys.TextSecondary, CdsPaginationTokens.RangeText);
        Assert.Equal(CdsColorKeys.TextPrimary, CdsPaginationTokens.Label(false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsPaginationTokens.Label(true));
    }

    [Theory]
    [InlineData(CdsSize.Xs, 32)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    public void Heights_are_32_40_and_48(CdsSize size, double height) => Assert.Equal(height, CdsPaginationTokens.Height(size));

    [Fact]
    public void Narrow_bars_are_compact_and_the_top_rule_drops_under_a_table()
    {
        Assert.True(CdsPaginationTokens.IsCompact(500));
        Assert.False(CdsPaginationTokens.IsCompact(672));
        Assert.False(CdsPaginationTokens.IsCompact(0)); // not laid out yet
        Assert.True(CdsPaginationTokens.ShowsTopRule(false));
        Assert.False(CdsPaginationTokens.ShowsTopRule(true));
    }

    [Fact]
    public void Disabled_and_normal_arrow_colors()
    {
        Assert.Equal(CdsColorKeys.IconPrimary, CdsPageButtonTokens.Icon(false));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsPageButtonTokens.Icon(true));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsPageButtonTokens.Fill(true, false, CdsColorKeys.LayerHover01));
        Assert.Null(CdsPageButtonTokens.Fill(true, true, CdsColorKeys.LayerHover01));
        Assert.Null(CdsPageButtonTokens.Fill(false, false, CdsColorKeys.LayerHover01));
    }

    [Fact]
    public void The_page_sizes_converter_parses_lists()
    {
        var converter = new CdsPageSizesConverter();
        var sizes = (List<int>)converter.ConvertFrom(null, null, "10, 20 ,50")!;
        Assert.Equal([10, 20, 50], sizes);
        Assert.Throws<FormatException>(() => converter.ConvertFrom(null, null, "ten"));
    }

    // ----- the control -----

    [Fact]
    public void Defaults_and_normalization()
    {
        var p = new CdsPagination { TotalItems = 103 };
        Assert.Equal(1, p.Page);
        Assert.Equal(10, p.PageSize);
        Assert.Equal(11, p.PageCount);
        Assert.Equal(CdsSize.Md, p.Size);
        Assert.Equal("Next page", p.ForwardText);
        Assert.Equal("Previous page", p.BackwardText);
        p.Page = 50;
        Assert.Equal(11, p.Page);
        p.Page = -4;
        Assert.Equal(1, p.Page);
    }

    [Fact]
    public void An_unlisted_page_size_becomes_the_first_listed_size_and_the_page_is_reclamped()
    {
        var p = new CdsPagination { TotalItems = 103, PageSizes = [20, 50], PageSize = 20, Page = 6 };
        Assert.Equal(6, p.Page);
        p.PageSize = 15;
        Assert.Equal(20, p.PageSize);
        p.TotalItems = 30;
        Assert.Equal(2, p.Page);
    }

    [Fact]
    public void The_first_item_index_slices_data()
    {
        var p = new CdsPagination { TotalItems = 103, Page = 3 };
        Assert.Equal(20, p.FirstItemIndex);
    }

    [Fact]
    public void Every_property_can_be_set_without_a_platform()
    {
        var p = new CdsPagination
        {
            TotalItems = 500,
            PageSizes = [10, 25],
            Size = CdsSize.Lg,
            IsPageInputDisabled = true,
            IsPageSizeInputDisabled = true,
            ItemsPerPageText = "Buckets per page:",
            ItemRangeText = (min, max, total) => $"{min}–{max} of {total} buckets",
            ItemText = (min, max) => $"{min}–{max} buckets",
            PageRangeText = (page, count) => $"of {count} pages",
            PageText = page => $"page {page}",
            BackwardText = "Back",
            ForwardText = "Forward",
            IsAttachedToTable = true,
        };
        p.IsPagesUnknown = true;
        p.IsLastPage = true;
        p.IsEnabled = false;
        p.TotalItems = 100000; // more than 100 pages: numeric entry
        Assert.Equal(CdsSize.Lg, p.Size);
    }

    [Fact]
    public void A_pagination_after_a_data_table_detects_it_is_attached()
    {
        var stack = new VerticalStackLayout();
        var table = new CdsDataTable();
        var pagination = new CdsPagination { TotalItems = 40 };
        stack.Add(table);
        stack.Add(pagination);
        pagination.RefreshAttachment();
        Assert.True(pagination.IsAttached);
        var lone = new CdsPagination { TotalItems = 40 };
        new VerticalStackLayout { lone };
        lone.RefreshAttachment();
        Assert.False(lone.IsAttached);
        lone.IsAttachedToTable = true;
        Assert.True(lone.IsAttached);
    }

    [Fact]
    public void A_compact_bar_caps_the_range_to_what_the_arrows_leave()
    {
        Assert.Equal(186 - 64 - 32, CdsPaginationTokens.CompactRangeWidth(true, 186, 32, 32));
        Assert.Equal(0, CdsPaginationTokens.CompactRangeWidth(true, 50, 32, 32));
        Assert.True(double.IsPositiveInfinity(CdsPaginationTokens.CompactRangeWidth(false, 800, 40, 32)));
    }
}
