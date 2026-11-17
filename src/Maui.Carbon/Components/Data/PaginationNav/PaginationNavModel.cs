namespace Maui.Carbon;

/// <summary>What one slot of the page strip shows.</summary>
internal enum NavSlotKind
{
    /// <summary>A numbered page button.</summary>
    Page,
    /// <summary>An overflow select ("...") listing the hidden pages <see cref="NavSlot.Start"/> to <see cref="NavSlot.End"/>.</summary>
    Overflow,
}

/// <summary>One slot of the page strip. Pages are 0-based.</summary>
/// <param name="Kind">Page button or overflow select.</param>
/// <param name="Start">The page (for a button) or the first hidden page (for an overflow).</param>
/// <param name="End">The last hidden page of an overflow; equal to <paramref name="Start"/> for a page button.</param>
internal readonly record struct NavSlot(NavSlotKind Kind, int Start, int End)
{
    /// <summary>A page button.</summary>
    public static NavSlot Page(int page) => new(NavSlotKind.Page, page, page);

    /// <summary>An overflow over a hidden range, inclusive.</summary>
    public static NavSlot Overflow(int start, int end) => new(NavSlotKind.Overflow, start, end);

    /// <summary>The number of pages an overflow hides.</summary>
    public int Count => End - Start + 1;
}

/// <summary>The windowing and stepping logic of <see cref="CdsPaginationNav"/> (pure, tested). Pages are 0-based.</summary>
internal static class PaginationNavModel
{
    /// <summary>The fewest slots the strip uses.</summary>
    public const int MinItemsShown = 4;

    /// <summary>Clamps the requested slot count to what a size allows: md 4 to 5, sm 4 to 7, lg at least 4.</summary>
    public static int CoerceItemsShown(CdsSize size, int itemsShown) => size switch
    {
        CdsSize.Sm => Math.Clamp(itemsShown, MinItemsShown, 7),
        CdsSize.Md => Math.Clamp(itemsShown, MinItemsShown, 5),
        _ => Math.Max(itemsShown, MinItemsShown),
    };

    /// <summary>Clamps a page into 0..<paramref name="totalPages"/> - 1.</summary>
    public static int ClampPage(int page, int totalPages) => Math.Clamp(page, 0, Math.Max(0, totalPages - 1));

    /// <summary>
    /// Builds the strip for the current page. The first and last page always show; everything that does not fit collapses
    /// into overflow slots, each listing a hidden range of at least two pages (a hidden range of one shows that page
    /// instead). The strip never starts or ends with an overflow, and the current page is always a button.
    /// </summary>
    public static IReadOnlyList<NavSlot> Build(int page, int totalPages, int itemsShown)
    {
        var slots = new List<NavSlot>();
        if (totalPages <= 0) return slots;
        itemsShown = Math.Max(itemsShown, MinItemsShown);
        page = ClampPage(page, totalPages);

        if (totalPages <= itemsShown)
        {
            for (var i = 0; i < totalPages; i++) slots.Add(NavSlot.Page(i));
            return slots;
        }

        var last = totalPages - 1;
        int windowStart, windowEnd; // the visible middle pages, inclusive
        if (page <= itemsShown - 3)
        {
            // Near the start: first pages, one overflow, last.
            windowStart = 1;
            windowEnd = itemsShown - 3;
        }
        else if (page >= totalPages - itemsShown + 2)
        {
            // Near the end: first, one overflow, last pages.
            windowStart = totalPages - itemsShown + 2;
            windowEnd = last - 1;
        }
        else
        {
            // In the middle: first, overflow, a window around the page, overflow, last.
            var width = Math.Max(1, itemsShown - 4);
            windowStart = page - (width - 1) / 2;
            windowEnd = windowStart + width - 1;
        }

        slots.Add(NavSlot.Page(0));
        AddHidden(slots, 1, windowStart - 1);
        for (var i = windowStart; i <= windowEnd; i++) slots.Add(NavSlot.Page(i));
        AddHidden(slots, windowEnd + 1, last - 1);
        slots.Add(NavSlot.Page(last));
        return slots;
    }

    static void AddHidden(List<NavSlot> slots, int start, int end)
    {
        if (end < start) return;
        if (end == start) slots.Add(NavSlot.Page(start));
        else slots.Add(NavSlot.Overflow(start, end));
    }

    /// <summary>The page after <paramref name="page"/>, or -1 when at the end without looping.</summary>
    public static int Next(int page, int totalPages, bool loop)
    {
        if (totalPages <= 0) return -1;
        if (page < totalPages - 1) return page + 1;
        return loop ? 0 : -1;
    }

    /// <summary>The page before <paramref name="page"/>, or -1 when at the start without looping.</summary>
    public static int Previous(int page, int totalPages, bool loop)
    {
        if (totalPages <= 0) return -1;
        if (page > 0) return page - 1;
        return loop ? totalPages - 1 : -1;
    }

    /// <summary>The announcement after a page change: "Page 3 of 14".</summary>
    public static string Announcement(int page, int totalPages) => $"Page {page + 1} of {totalPages}";
}
