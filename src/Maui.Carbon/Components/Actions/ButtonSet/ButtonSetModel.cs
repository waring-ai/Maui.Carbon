namespace Maui.Carbon;

/// <summary>Pure layout and ordering rules of <see cref="CdsButtonSet"/> (from <c>.cds--btn-set</c> in Carbon's button styles).</summary>
internal static class ButtonSetModel
{
    /// <summary>Maximum width of one button: 12.25rem.</summary>
    public const double MaxButtonWidth = 196;

    /// <summary>Maximum width of one expressive button: 20rem.</summary>
    public const double MaxExpressiveButtonWidth = 320;

    /// <summary>Fluid sets: each button wants 25% of the set, between 11rem and 14.5rem.</summary>
    public const double FluidMinButtonWidth = 176;

    /// <summary>Fluid sets: maximum width of a non-ghost button (14.5rem).</summary>
    public const double FluidMaxButtonWidth = 232;

    /// <summary>The widest a plain set of <paramref name="count"/> buttons can be (each at most <see cref="MaxButtonWidth"/>).</summary>
    public static double MaxSetWidth(int count, bool stacked, bool expressive)
    {
        var button = expressive ? MaxExpressiveButtonWidth : MaxButtonWidth;
        return stacked ? button : button * Math.Max(1, count);
    }

    /// <summary>
    /// The order of a fluid set: ghost, danger ghost, tertiary, secondary, danger, primary, whatever the child order.
    /// Equal kinds keep their order. Returns indices into <paramref name="kinds"/>.
    /// </summary>
    public static IReadOnlyList<int> FluidOrder(IReadOnlyList<CdsButtonKind> kinds) =>
        Enumerable.Range(0, kinds.Count).OrderBy(i => Rank(kinds[i])).ThenBy(i => i).ToArray();

    /// <summary>The fluid sort rank of a kind (lower comes first).</summary>
    public static int Rank(CdsButtonKind kind) => kind switch
    {
        CdsButtonKind.Ghost => 0,
        CdsButtonKind.DangerGhost => 1,
        CdsButtonKind.Tertiary => 2,
        CdsButtonKind.Secondary => 3,
        CdsButtonKind.Danger => 4,
        _ => 5,
    };

    /// <summary>
    /// A fluid set stacks (primary first) when its width is at most 11rem per button for up to four buttons
    /// (11, 22, 33 and 44rem).
    /// </summary>
    public static bool ShouldStack(double width, int count) =>
        count > 0 && width > 0 && width <= FluidMinButtonWidth * Math.Min(count, 4);

    /// <summary>Fluid sets of two buttons split 50/50 up to this width (44rem).</summary>
    public const double FluidHalfSplitMaxWidth = 704;

    /// <summary>A fluid set of one button fills the set up to this width (33rem).</summary>
    public const double FluidFullWidthMaxWidth = 528;

    /// <summary>
    /// Widths of the buttons of a horizontal fluid set, in display order: non-ghost buttons take 25% of the set clamped to
    /// 11..14.5rem, ghost buttons share what is left; the group is right-aligned (the first button starts after
    /// the returned <paramref name="offset"/>). Carbon's container rules override that in narrow sets: two buttons in a
    /// set of at most 44rem take 50% each, and a single button in a set of at most 33rem takes all of it, with no maximum.
    /// </summary>
    public static double[] FluidWidths(double width, IReadOnlyList<bool> isGhost, out double offset)
    {
        var count = isGhost.Count;
        var widths = new double[count];
        if ((count == 2 && width <= FluidHalfSplitMaxWidth) || (count == 1 && width <= FluidFullWidthMaxWidth))
        {
            Array.Fill(widths, Math.Max(0, width) / count);
            offset = 0;
            return widths;
        }
        var fixedWidth = Math.Clamp(width * 0.25, FluidMinButtonWidth, FluidMaxButtonWidth);
        var ghosts = isGhost.Count(g => g);
        var remaining = Math.Max(0, width - fixedWidth * (count - ghosts));
        for (var i = 0; i < count; i++)
            widths[i] = isGhost[i] ? (ghosts > 0 ? remaining / ghosts : 0) : fixedWidth;
        offset = Math.Max(0, width - widths.Sum());
        return widths;
    }

    /// <summary>
    /// Whether the separator before button <paramref name="index"/> is drawn: never before the first button, and not next to a
    /// focused button (so the focus ring stays clean).
    /// </summary>
    public static bool ShowSeparator(int index, bool previousFocused, bool currentFocused) =>
        index > 0 && !previousFocused && !currentFocused;
}

/// <summary>Color keys of <see cref="CdsButtonSet"/> separators.</summary>
public static class CdsButtonSetTokens
{
    /// <summary>
    /// The separator color on the leading edge of a button: <c>button-separator</c>; for a disabled button
    /// <c>icon-on-color-disabled</c> (side by side) or <c>layer-selected-disabled</c> (stacked).
    /// </summary>
    public static string Separator(bool isStacked, bool isDisabled) =>
        !isDisabled ? CdsColorKeys.ButtonSeparator
        : isStacked ? CdsColorKeys.LayerSelectedDisabled
        : CdsColorKeys.IconOnColorDisabled;
}
