namespace Maui.Carbon;

/// <summary>The paging math of <see cref="CdsPagination"/> (pure, tested). Pages are 1-based.</summary>
internal static class PaginationModel
{
    /// <summary>Above this many pages the page select would be unwieldy, so a numeric entry replaces it.</summary>
    public const int MaxSelectPages = 100;

    /// <summary>The number of pages: at least 1.</summary>
    public static int PageCount(int totalItems, int pageSize)
    {
        if (pageSize <= 0 || totalItems <= 0) return 1;
        return (totalItems + pageSize - 1) / pageSize;
    }

    /// <summary>Clamps a page into 1..<paramref name="pageCount"/>.</summary>
    public static int ClampPage(int page, int pageCount) => Math.Clamp(page, 1, Math.Max(1, pageCount));

    /// <summary>The first and last item number on a page (1-based, inclusive); (0, 0) when there are no items.</summary>
    public static (int Min, int Max) Range(int page, int pageSize, int totalItems)
    {
        if (totalItems <= 0 || pageSize <= 0) return (0, 0);
        page = ClampPage(page, PageCount(totalItems, pageSize));
        var min = (page - 1) * pageSize + 1;
        return (min, Math.Min(page * pageSize, totalItems));
    }

    /// <summary>The first visible item's zero-based index on a page.</summary>
    public static int FirstIndex(int page, int pageSize) => Math.Max(0, (page - 1) * Math.Max(1, pageSize));

    /// <summary>Whether the previous button is enabled.</summary>
    public static bool CanGoBack(int page) => page > 1;

    /// <summary>
    /// Whether the next button is enabled: before the last page, or always when the page count is unknown unless
    /// <paramref name="isLastPage"/> says there are no more.
    /// </summary>
    public static bool CanGoForward(int page, int pageCount, bool pagesUnknown, bool isLastPage) =>
        pagesUnknown ? !isLastPage : page < pageCount;

    /// <summary>"1–10 of 103 items" (or "1–10 items" when the total is unknown).</summary>
    public static string RangeText(int page, int pageSize, int totalItems, bool pagesUnknown)
    {
        var (min, max) = Range(page, pageSize, totalItems);
        if (pagesUnknown) return $"{(page - 1) * pageSize + 1}–{page * pageSize} items";
        return $"{min}–{max} of {totalItems} items";
    }

    /// <summary>"of 11 pages" (or "of 1 page").</summary>
    public static string PageRangeText(int pageCount) => pageCount == 1 ? "of 1 page" : $"of {pageCount} pages";

    /// <summary>"page 3" (shown when the page count is unknown).</summary>
    public static string PageText(int page) => $"page {page}";

    /// <summary>Whether the page number is entered in a numeric field instead of picked from a list.</summary>
    public static bool UsesPageInput(int pageCount) => pageCount > MaxSelectPages;

    /// <summary>Normalizes the page sizes: positive, distinct, in the given order; falls back to 10, 20, 30, 40, 50.</summary>
    public static IReadOnlyList<int> NormalizeSizes(IEnumerable<int>? sizes)
    {
        var list = (sizes ?? []).Where(s => s > 0).Distinct().ToList();
        return list.Count > 0 ? list : [10, 20, 30, 40, 50];
    }

    /// <summary>Picks the page size to use: the requested one when it is in the list, else the first size.</summary>
    public static int ResolvePageSize(int requested, IReadOnlyList<int> sizes) =>
        sizes.Contains(requested) ? requested : sizes[0];

    /// <summary>Parses text typed into the numeric page field: a number clamped into range, or <c>null</c> when it is not a number.</summary>
    public static int? ParsePageInput(string? text, int pageCount) =>
        int.TryParse(text?.Trim(), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var n)
            ? ClampPage(n, pageCount)
            : null;
}
