namespace Maui.Carbon;

/// <summary>Measurements and rules of Carbon for IBM Products' side panel, kept pure so they are unit-tested.</summary>
internal static class SidePanelModel
{
    /// <summary>The narrowest a panel gets (<c>min-inline-size: 16rem</c>).</summary>
    public const double MinWidth = 256;

    /// <summary>Viewports at least this wide cap the panel at three quarters of their width (<c>md</c> breakpoint).</summary>
    public const double CapBreakpoint = 672;

    /// <summary>The width of each size (<c>$side-panel-sizes</c>).</summary>
    public static double SizeWidth(CdsSidePanelSize size) => size switch
    {
        CdsSidePanelSize.Xs => 256,
        CdsSidePanelSize.Sm => 320,
        CdsSidePanelSize.Lg => 640,
        CdsSidePanelSize.Xl => 1040,
        CdsSidePanelSize.Xxl => 1280,
        _ => 480,
    };

    /// <summary>
    /// The panel width in a viewport: the size's width, no wider than the viewport and, from the md breakpoint, than 75%
    /// of it, and never narrower than 256 (or the viewport, when that is narrower still).
    /// </summary>
    public static double Width(CdsSidePanelSize size, double viewport)
    {
        var width = SizeWidth(size);
        if (viewport <= 0) return width;
        width = Math.Min(width, viewport);
        if (viewport >= CapBreakpoint) width = Math.Min(width, viewport * 0.75);
        return Math.Min(Math.Max(width, MinWidth), viewport);
    }

    /// <summary>The action set width class of a panel size (an xs panel uses the small, stacked set).</summary>
    public static CdsActionSetSize ActionSetSize(CdsSidePanelSize size) => size switch
    {
        CdsSidePanelSize.Xs or CdsSidePanelSize.Sm => CdsActionSetSize.Sm,
        CdsSidePanelSize.Lg => CdsActionSetSize.Lg,
        CdsSidePanelSize.Xl => CdsActionSetSize.Xl,
        CdsSidePanelSize.Xxl => CdsActionSetSize.Xxl,
        _ => CdsActionSetSize.Md,
    };

    /// <summary>The close and back buttons: 40 (md) in large panels with actions, else 32 (sm).</summary>
    public static CdsSize HeaderButtonSize(CdsSidePanelSize size, bool hasActions) =>
        hasActions && size is CdsSidePanelSize.Lg or CdsSidePanelSize.Xl or CdsSidePanelSize.Xxl ? CdsSize.Md : CdsSize.Sm;

    /// <summary>The space kept at the end of the title for the corner buttons: 32, 64 with a toolbar or a decorator, 80 with both.</summary>
    public static double TitleEndPadding(bool hasToolbar, bool hasDecorator) =>
        hasToolbar && hasDecorator ? CdsSpacing.Spacing11 : hasToolbar || hasDecorator ? CdsSpacing.Spacing10 : CdsSpacing.Spacing07;

    /// <summary>The header's top padding: 16, or room for the back button (32 or 40) on a later step.</summary>
    public static double HeaderTopPadding(bool hasBack, CdsSize buttonSize) =>
        !hasBack ? CdsSpacing.Spacing05 : buttonSize == CdsSize.Md ? CdsSpacing.Spacing08 : CdsSpacing.Spacing07;

    /// <summary>The gap under the title: 16, or 8 when a subtitle follows.</summary>
    public static double TitleBottomPadding(bool hasSubtitle) => hasSubtitle ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05;

    /// <summary>The body's top padding: 8 under a toolbar, 16 when the title never animates, else 0 (the header's gap).</summary>
    public static double BodyTopPadding(bool hasToolbar, bool animateTitle) =>
        hasToolbar ? CdsSpacing.Spacing03 : animateTitle ? 0 : CdsSpacing.Spacing05;

    /// <summary>How far the title has collapsed (0 to 1) after scrolling the body <paramref name="scrollY"/> of <paramref name="distance"/>.</summary>
    public static double Progress(double scrollY, double distance) =>
        distance <= 0 ? 0 : Math.Clamp(scrollY / distance, 0, 1);

    /// <summary>
    /// The title collapses on scroll only when there is something to collapse (a label, subtitle or toolbar) and the body
    /// scrolls far enough to collapse it completely.
    /// </summary>
    public static bool CanAnimateTitle(bool animateTitle, bool hasTitle, bool hasCollapsible, double scrollRange, double distance) =>
        animateTitle && hasTitle && hasCollapsible && distance > 0 && scrollRange >= distance;
}
