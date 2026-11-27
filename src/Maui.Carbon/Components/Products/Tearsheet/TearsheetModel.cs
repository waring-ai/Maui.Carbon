namespace Maui.Carbon;

/// <summary>Measurements and stacking rules of Carbon for IBM Products' tearsheets, kept pure so they are unit-tested.</summary>
internal static class TearsheetModel
{
    /// <summary>The gap above a tearsheet: <c>spacing-11 + spacing-03 + spacing-01</c> (90).</summary>
    public const double TopGap = CdsSpacing.Spacing11 + CdsSpacing.Spacing03 + CdsSpacing.Spacing01;

    /// <summary>At most three tearsheets are shown stacked; a fourth is not shown.</summary>
    public const int MaxDepth = 3;

    /// <summary>The influencer column: 256 or 320, plus its 1px border.</summary>
    public static double InfluencerWidth(CdsTearsheetInfluencerWidth width) => width == CdsTearsheetInfluencerWidth.Wide ? 321 : 257;

    /// <summary>A wide tearsheet fills narrow viewports and leaves 64 on each side from the md breakpoint.</summary>
    public static double WideWidth(double viewport) =>
        viewport < CdsModalSizing.BreakpointMd ? viewport : Math.Max(0, viewport - 2 * CdsSpacing.Spacing10);

    /// <summary>A narrow tearsheet is as wide as a small modal (full width, then 60%, 42% and 36% of the viewport).</summary>
    public static double NarrowWidth(double viewport) => CdsModalSizing.ModalWidth(CdsModalSize.Sm, viewport);

    /// <summary>
    /// The height: the whole viewport below the md breakpoint, else the viewport less the top gap. A tearsheet with others
    /// stacked on it is 16 taller per level (when all stacked tearsheets are the same size), so its top shows behind them.
    /// </summary>
    public static double Height(double viewportWidth, double viewportHeight, int levelsAbove, bool sameSize)
    {
        if (viewportWidth < CdsModalSizing.BreakpointMd) return Math.Max(0, viewportHeight);
        var extra = sameSize ? CdsSpacing.Spacing05 * Math.Min(levelsAbove, MaxDepth - 1) : 0;
        return Math.Max(0, viewportHeight - TopGap + extra);
    }

    /// <summary>A tearsheet with one tearsheet over it shrinks by 32, with two by 64 (only when all are the same size).</summary>
    public static double StackScale(double width, int levelsAbove, bool sameSize)
    {
        if (!sameSize || width <= 0 || levelsAbove <= 0) return 1;
        return (width - (levelsAbove == 1 ? 32 : 64)) / width;
    }

    /// <summary>A tearsheet without actions is passive: it has a close button and closes on a tap outside.</summary>
    public static bool IsPassive(int actionCount) => actionCount == 0;

    /// <summary>The close button shows when asked for, else only on passive tearsheets.</summary>
    public static bool HasCloseIcon(bool? hasCloseIcon, int actionCount) => hasCloseIcon ?? IsPassive(actionCount);
}
