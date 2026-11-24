namespace Maui.Carbon;

/// <summary>The layout of a <see cref="CdsCodeSnippet"/>.</summary>
public enum CdsCodeSnippetType
{
    /// <summary>One line with a copy button; long code scrolls sideways under a fade. The default.</summary>
    Single,
    /// <summary>An inline code chip inside running text that copies on click.</summary>
    Inline,
    /// <summary>A multi-line block that can expand with Show more / Show less.</summary>
    Multi,
}

/// <summary>Pure logic of the code snippet: row math for the multi-line block.</summary>
public static class CdsCodeSnippetModel
{
    /// <summary>Height of one code row: <c>code-01</c> is 16px.</summary>
    public const double RowHeight = 16;

    /// <summary>Bottom padding of the code in a multi-line snippet (1.5rem).</summary>
    public const double CodeBottomPadding = 24;

    /// <summary>Splits code into lines (CRLF, CR and LF all end a line).</summary>
    public static string[] SplitLines(string? code) =>
        string.IsNullOrEmpty(code) ? [] : code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    /// <summary>The number of lines of code (an empty snippet has none).</summary>
    public static int LineCount(string? code) => SplitLines(code).Length;

    /// <summary>
    /// The height of the visible code area. Collapsed it is the content height kept between
    /// <paramref name="minRows"/> and <paramref name="maxRows"/> rows; expanded it is kept at least
    /// <paramref name="minExpandedRows"/> rows high and, when <paramref name="maxExpandedRows"/> is above zero, at most that
    /// many rows (0 = unlimited).
    /// </summary>
    public static double ViewportHeight(double contentHeight, bool expanded, int minRows, int maxRows, int minExpandedRows, int maxExpandedRows)
    {
        double lower, upper;
        if (expanded)
        {
            lower = minExpandedRows * RowHeight;
            upper = maxExpandedRows > 0 ? maxExpandedRows * RowHeight : double.PositiveInfinity;
        }
        else
        {
            lower = minRows * RowHeight;
            upper = maxRows * RowHeight;
        }
        if (upper < lower) upper = lower;
        return Math.Clamp(contentHeight, lower, upper);
    }

    /// <summary>True when Show more / Show less is offered: the code is taller than <paramref name="maxCollapsedRows"/> rows.</summary>
    public static bool ShowsExpandButton(double contentHeight, int maxCollapsedRows) =>
        contentHeight > maxCollapsedRows * RowHeight + 0.5;

    /// <summary>The text of the expand button.</summary>
    public static string ExpandText(bool expanded, string showMore, string showLess) => expanded ? showLess : showMore;

    /// <summary>Rotation of the expand chevron: 0 collapsed, 180 expanded.</summary>
    public static double ChevronRotation(bool expanded) => expanded ? 180 : 0;

    /// <summary>How far an arrow key scrolls the focused code area (40, like a browser's arrow-key step).</summary>
    public const double KeyScrollStep = 40;

    /// <summary>
    /// Where a key scrolls the focused code area, or <c>null</c> when the key does not scroll it. Up and Down (and Page Up /
    /// Page Down, by a viewport less one step) scroll code that overflows vertically, Left and Right code that overflows
    /// sideways; Home and End go to the top and bottom, or to the start and end of a single line. The result is clamped to
    /// the content, so at an edge the key is still taken (it does not scroll the page behind).
    /// </summary>
    public static (double X, double Y)? ScrollOffset(CdsKey key, double x, double y, double viewportWidth, double viewportHeight, double contentWidth, double contentHeight)
    {
        var maxX = Math.Max(0, contentWidth - viewportWidth);
        var maxY = Math.Max(0, contentHeight - viewportHeight);
        var vertical = maxY > 0.5;
        var horizontal = maxX > 0.5;
        var page = Math.Max(KeyScrollStep, viewportHeight - KeyScrollStep);
        (double X, double Y)? to = key switch
        {
            CdsKey.Up when vertical => (x, y - KeyScrollStep),
            CdsKey.Down when vertical => (x, y + KeyScrollStep),
            CdsKey.Left when horizontal => (x - KeyScrollStep, y),
            CdsKey.Right when horizontal => (x + KeyScrollStep, y),
            CdsKey.PageUp when vertical => (x, y - page),
            CdsKey.PageDown when vertical => (x, y + page),
            CdsKey.Home when vertical => (x, 0),
            CdsKey.End when vertical => (x, maxY),
            CdsKey.Home when horizontal => (0, y),
            CdsKey.End when horizontal => (maxX, y),
            _ => null,
        };
        return to is { } t ? (Math.Clamp(t.X, 0, maxX), Math.Clamp(t.Y, 0, maxY)) : null;
    }

    /// <summary>The text that is copied: <paramref name="copyText"/> when set, otherwise the code.</summary>
    public static string TextToCopy(string? code, string? copyText) => copyText ?? code ?? string.Empty;
}

/// <summary>Maps code snippet parts and states to Carbon tokens.</summary>
public static class CdsCodeSnippetTokens
{
    /// <summary>Fill of the snippet and of its buttons: <c>layer-01</c>; hover <c>layer-hover-01</c>, pressed <c>layer-active-01</c>. Disabled stays <c>layer-01</c>.</summary>
    public static string Fill(CdsInteractionState state)
    {
        if (state.HasFlag(CdsInteractionState.Disabled)) return CdsColorKeys.Layer01;
        if (state.HasFlag(CdsInteractionState.Pressed)) return CdsColorKeys.LayerActive01;
        if (state.HasFlag(CdsInteractionState.Hover)) return CdsColorKeys.LayerHover01;
        return CdsColorKeys.Layer01;
    }

    /// <summary>The fill of an inline snippet without a copy button: it never changes on hover.</summary>
    public static string PlainFill => CdsColorKeys.Layer01;

    /// <summary>Code and label color.</summary>
    public static string Text(bool disabled) => disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;

    /// <summary>Icon color.</summary>
    public static string Icon(bool disabled) => disabled ? CdsColorKeys.IconDisabled : CdsColorKeys.IconPrimary;

    /// <summary>The 1px border of a focused inline snippet (<c>focus</c>); none otherwise.</summary>
    public static string? InlineBorder(CdsInteractionState state) =>
        state.HasFlag(CdsInteractionState.FocusVisible) ? CdsColorKeys.Focus : null;
}
