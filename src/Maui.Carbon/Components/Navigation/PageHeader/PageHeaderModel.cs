namespace Maui.Carbon;

/// <summary>Pure layout rules of the page header.</summary>
internal static class PageHeaderModel
{
    /// <summary>Width of the title and body text columns (640).</summary>
    public const double TextMaxWidth = 640;

    /// <summary>The gap between the tab strip and the tags (64).</summary>
    public const double TabTagGap = 64;

    /// <summary>The height of the breadcrumb bar (40).</summary>
    public const double BreadcrumbBarHeight = 40;

    /// <summary>
    /// The inset of the content from the edge: the grid's outer margin plus its 16 column padding, so the title lines up
    /// with the first column of the 16-column grid.
    /// </summary>
    public static double Inset(CdsBreakpoint breakpoint) => breakpoint.Margin() + CdsSpacing.Spacing05;

    /// <summary>The title is clamped to two lines, or one when contextual actions share the row.</summary>
    public static int TitleMaxLines(bool hasContextualActions) => hasContextualActions ? 1 : 2;

    /// <summary>Below 672 (the sm breakpoint) the page actions stack under the title and left-align.</summary>
    public static bool StacksActions(CdsBreakpoint breakpoint) => breakpoint == CdsBreakpoint.Sm;

    /// <summary>The hero image box: 2 x 1 from lg up, 3 x 2 below.</summary>
    public static double HeroRatio(CdsBreakpoint breakpoint) => breakpoint >= CdsBreakpoint.Lg ? 2.0 : 1.5;

    /// <summary>The tab bar pulls 16 to the start so the tab labels line up with the title.</summary>
    public const double TabBarPull = -CdsSpacing.Spacing05;

    /// <summary>End padding of the breadcrumb bar: the inset, or none when its actions are flush with the edge.</summary>
    public static double BarEndInset(CdsBreakpoint breakpoint, bool flush) => flush ? 0 : Inset(breakpoint);
}
