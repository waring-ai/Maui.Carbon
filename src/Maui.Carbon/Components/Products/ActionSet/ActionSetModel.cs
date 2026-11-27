namespace Maui.Carbon;

/// <summary>
/// The rules of Carbon for IBM Products' action set (<c>ActionSet</c>): which buttons it may hold, the order they show
/// in, when they stack and how wide each one is. Pure, so it is unit-tested without a platform.
/// </summary>
internal static class ActionSetModel
{
    /// <summary>The widest a button gets in a quarter-width column (<c>max-inline-size: to-rem(232px)</c>).</summary>
    public const double MaxQuarterWidth = 232;

    /// <summary>Ghost buttons fill the space the others leave; they never get a fixed column.</summary>
    public static bool IsGhost(CdsButtonKind kind) => kind is CdsButtonKind.Ghost or CdsButtonKind.DangerGhost;

    /// <summary>
    /// Sort key, start to end of a row: ghost, danger ghost, tertiary, secondary (and danger tertiary), danger,
    /// primary. A stacked set reverses it, so the primary action is on top.
    /// </summary>
    public static int Order(CdsButtonKind kind) => kind switch
    {
        CdsButtonKind.Ghost => 1,
        CdsButtonKind.DangerGhost => 2,
        CdsButtonKind.Tertiary => 3,
        CdsButtonKind.Danger => 5,
        CdsButtonKind.Primary => 6,
        _ => 4,
    };

    /// <summary>Small sets always stack; medium ones once they hold more than two buttons.</summary>
    public static bool WillStack(CdsActionSetSize size, int count) =>
        size == CdsActionSetSize.Sm || (size == CdsActionSetSize.Md && count > 2);

    /// <summary>The indices of <paramref name="kinds"/> in display order (a stable sort by <see cref="Order"/>).</summary>
    public static int[] Arrange(IReadOnlyList<CdsButtonKind> kinds, bool stacking)
    {
        var indices = Enumerable.Range(0, kinds.Count).ToArray();
        var sign = stacking ? -1 : 1;
        return [.. indices.OrderBy(i => sign * Order(kinds[i])).ThenBy(i => i)];
    }

    /// <summary>
    /// The widths of the buttons of a row, in display order, for a set <paramref name="width"/> wide. A single button
    /// fills a medium set and takes the end half of a large one; two buttons share a medium or large set; three or more,
    /// and every button of an extra large set, take a quarter each (at most 232). A ghost button takes the space left
    /// over. Space nobody takes stays empty at the start, so the buttons sit at the end edge.
    /// </summary>
    public static double[] RowWidths(IReadOnlyList<CdsButtonKind> ordered, CdsActionSetSize size, double width)
    {
        var count = ordered.Count;
        var widths = new double[count];
        if (count == 0 || width <= 0) return widths;
        var wide = size is CdsActionSetSize.Xl or CdsActionSetSize.Xxl;
        double Fixed() =>
            wide || count >= 3 ? Math.Min(width / 4, MaxQuarterWidth)
            : count == 2 ? width / 2
            : size == CdsActionSetSize.Lg ? width / 2
            : width;

        var ghosts = 0;
        double taken = 0;
        for (var i = 0; i < count; i++)
        {
            // Two buttons in a medium or large set always split it in halves, ghost or not.
            if (IsGhost(ordered[i]) && (wide || count != 2))
            {
                ghosts++;
                continue;
            }
            widths[i] = Fixed();
            taken += widths[i];
        }
        if (ghosts > 0)
        {
            var share = Math.Max(0, width - taken) / ghosts;
            for (var i = 0; i < count; i++)
                if (IsGhost(ordered[i]) && (wide || count != 2)) widths[i] = share;
        }
        return widths;
    }

    /// <summary>
    /// What is wrong with a set of buttons by Carbon's rules (an empty list when nothing is): at most four buttons,
    /// three when stacked, one primary, one ghost, and no ghost next to others in a stacked set.
    /// </summary>
    public static IReadOnlyList<string> Problems(IReadOnlyList<CdsButtonKind> kinds, CdsActionSetSize size)
    {
        var problems = new List<string>();
        if (kinds.Count == 0) return problems;
        var stacking = WillStack(size, kinds.Count);
        var ghosts = kinds.Count(IsGhost);
        if (stacking && kinds.Count > 3) problems.Add("a stacked action set holds at most three buttons");
        if (kinds.Count > 4) problems.Add("an action set holds at most four buttons");
        if (kinds.Count(k => k == CdsButtonKind.Primary) > 1) problems.Add("an action set has at most one primary button");
        if (ghosts > 1) problems.Add("an action set has at most one ghost button");
        if (stacking && kinds.Count > 1 && ghosts > 0) problems.Add("a stacked action set cannot mix a ghost button with others");
        return problems;
    }
}
