namespace Maui.Carbon;

/// <summary>Pure logic of <see cref="CdsTagSet"/>: how many tags fit, where they go, the popover list and the modal search.</summary>
internal static class TagSetModel
{
    /// <summary>The number of hidden tags over which the popover stops and offers "View all tags".</summary>
    public const int AllTagsModalSearchThreshold = 10;

    /// <summary>
    /// How many tags show, as Carbon for IBM Products decides: tags (widths include the gap after each) are taken while
    /// they fit <paramref name="available"/>; if some are left out, tags are dropped from the end until the overflow tag
    /// fits too; the result is capped at <paramref name="maxVisible"/> (0 = no cap). Multiline shows every tag up to the cap.
    /// </summary>
    public static int FitCount(IReadOnlyList<double> widths, double available, double overflowWidth, int maxVisible, bool multiline)
    {
        var total = widths.Count;
        var cap = maxVisible > 0 ? Math.Min(maxVisible, total) : total;
        if (multiline || double.IsInfinity(available) || double.IsNaN(available)) return cap;

        var space = available;
        var fit = 0;
        while (fit < total && space >= widths[fit])
        {
            space -= widths[fit];
            fit++;
        }
        if (fit < total)
        {
            while (fit > 0 && space < overflowWidth)
            {
                fit--;
                space += widths[fit];
            }
        }
        return fit < 1 ? 0 : Math.Min(fit, cap);
    }

    /// <summary>
    /// Frames of the shown tags (the first <paramref name="count"/> of <paramref name="sizes"/>) and of the overflow tag
    /// (last, when <paramref name="overflow"/> is not null), for a width of <paramref name="available"/>. Each item is
    /// followed by <paramref name="gap"/>; multiline rows are <paramref name="gap"/> apart. Each row is aligned.
    /// </summary>
    public static TagSetLayoutResult Arrange(IReadOnlyList<Size> sizes, int count, Size? overflow, double available, CdsTagSetAlign align, bool multiline, double gap)
    {
        var items = new List<Size>(count + 1);
        for (var i = 0; i < count && i < sizes.Count; i++) items.Add(sizes[i]);
        if (overflow is { } o) items.Add(o);

        var frames = new Rect[items.Count];
        var rows = new List<(int Start, int End, double Width, double Height)>();
        var limit = double.IsInfinity(available) || double.IsNaN(available) ? double.PositiveInfinity : available;
        double x = 0, rowHeight = 0;
        var rowStart = 0;
        for (var i = 0; i < items.Count; i++)
        {
            var size = items[i];
            if (multiline && x > 0 && x + size.Width + gap > limit)
            {
                rows.Add((rowStart, i, x, rowHeight));
                rowStart = i;
                x = 0;
                rowHeight = 0;
            }
            frames[i] = new Rect(x, 0, size.Width, size.Height);
            x += size.Width + gap;
            rowHeight = Math.Max(rowHeight, size.Height);
        }
        if (items.Count > 0) rows.Add((rowStart, items.Count, x, rowHeight));

        double y = 0, contentWidth = 0;
        foreach (var row in rows)
        {
            contentWidth = Math.Max(contentWidth, row.Width);
            var offset = double.IsInfinity(limit) ? 0 : align switch
            {
                CdsTagSetAlign.Center => Math.Max(0, (limit - row.Width) / 2),
                CdsTagSetAlign.End => Math.Max(0, limit - row.Width),
                _ => 0,
            };
            for (var i = row.Start; i < row.End; i++)
            {
                var f = frames[i];
                frames[i] = new Rect(f.X + offset, y + (row.Height - f.Height) / 2, f.Width, f.Height);
            }
            y += row.Height + gap;
        }
        var height = rows.Count == 0 ? 0 : y - gap;
        var shown = overflow is null ? frames : frames[..^1];
        Rect? overflowFrame = overflow is null ? null : frames[^1];
        return new TagSetLayoutResult(shown, overflowFrame, new Size(contentWidth, height));
    }

    /// <summary>How many hidden tags the popover lists, and whether it offers "View all tags".</summary>
    public static (int Listed, bool ShowsAll) PopoverList(int hidden) =>
        hidden > AllTagsModalSearchThreshold ? (AllTagsModalSearchThreshold, true) : (Math.Max(0, hidden), false);

    /// <summary>Whether a tag matches the modal search: its text or search text contains the query, ignoring case.</summary>
    public static bool Matches(string? text, string? searchText, string? query)
    {
        if (string.IsNullOrEmpty(query)) return true;
        return (text?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false)
            || (searchText?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }

    /// <summary>The overflow tag's text: "+N".</summary>
    public static string OverflowText(int hidden) => $"+{hidden}";
}

/// <summary>Frames from <see cref="TagSetModel.Arrange"/>; <see cref="ContentSize"/> includes the gap after the last item of the widest row.</summary>
internal readonly record struct TagSetLayoutResult(Rect[] Tags, Rect? Overflow, Size ContentSize);
