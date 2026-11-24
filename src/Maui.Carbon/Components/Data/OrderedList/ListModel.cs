namespace Maui.Carbon;

/// <summary>Markers, widths and indents of Carbon lists (from <c>packages/styles/scss/components/list</c>).</summary>
public static class CdsListMarkers
{
    /// <summary>The en dash that marks a top-level unordered item.</summary>
    public const string EnDash = "–";

    /// <summary>The small square that marks a nested unordered item.</summary>
    public const string Square = "▪";

    /// <summary>Space between a nested list and its parent's text start (2rem).</summary>
    public const double NestedIndent = CdsSpacing.Spacing07;

    /// <summary>Start padding of nested items (0.25rem).</summary>
    public const double NestedItemPadding = CdsSpacing.Spacing02;

    /// <summary>
    /// The marker of an item: ordered top-level lists number (<c>1.</c>, <c>2.</c>), nested ordered lists use lower-case
    /// letters (<c>a.</c>, <c>b.</c>… <c>aa.</c> after <c>z.</c>); unordered lists use an en dash, nested ones a small square.
    /// </summary>
    /// <param name="ordered">Ordered list.</param>
    /// <param name="nested">Inside another list.</param>
    /// <param name="index">Zero-based position of the item.</param>
    public static string Marker(bool ordered, bool nested, int index)
    {
        if (!ordered) return nested ? Square : EnDash;
        return nested ? LowerLatin(index) + "." : (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture) + ".";
    }

    /// <summary>Bijective base-26 lower-case letters: 0 is a, 25 is z, 26 is aa.</summary>
    public static string LowerLatin(int index)
    {
        index = Math.Max(0, index);
        var chars = new Stack<char>();
        var n = index + 1;
        while (n > 0)
        {
            n--;
            chars.Push((char)('a' + n % 26));
            n /= 26;
        }
        return new string(chars.ToArray());
    }

    /// <summary>The width of the marker column: ordered 24 (marker 1.5rem before the text), unordered 16 (nested: 12).</summary>
    public static double MarkerWidth(bool ordered, bool nested) =>
        ordered ? CdsSpacing.Spacing06 : nested ? CdsSpacing.Spacing04 : CdsSpacing.Spacing05;

    /// <summary>
    /// The left margin of a list. A top-level ordered list hangs its 24 marker column before its box (Carbon places the
    /// number 1.5rem before the item, outside the box), so its text starts at the list's edge. A top-level unordered list
    /// starts at 0: Carbon's 1rem start margin is its marker column. A nested list's text starts 32 plus 4 of item padding
    /// after its parent's text, and its marker hangs before that.
    /// </summary>
    public static double LeftMargin(bool ordered, bool nested) =>
        nested ? NestedIndent + NestedItemPadding - MarkerWidth(ordered, nested) : ordered ? -MarkerWidth(ordered, nested) : 0;

    /// <summary>
    /// Whether the marker is aligned to the end of its column, just before the text: the native-style markers of nested
    /// ordered lists (<c>lower-latin</c>) and of <see cref="CdsOrderedList.IsNative"/> lists (<c>decimal</c>). Carbon's
    /// counters and bullets start at the column's start.
    /// </summary>
    public static bool IsMarkerEndAligned(bool ordered, bool nested, bool native) => ordered && (nested || native);

    /// <summary>The type style: <c>body-01</c> (14/20) or <c>body-02</c> (16/24) when expressive.</summary>
    public static CdsTypeStyle TypeStyle(bool expressive) => expressive ? CdsTypeStyle.Body02 : CdsTypeStyle.Body01;
}
