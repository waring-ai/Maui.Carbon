namespace Maui.Carbon;

/// <summary>How a <see cref="CdsSideNav"/> sits in the shell.</summary>
public enum CdsSideNavMode
{
    /// <summary>256 wide from the lg breakpoint up (persistent), 0 wide below it until expanded, with a scrim.</summary>
    Default,
    /// <summary>A 48 icon rail that expands to 256 on hover, focus or <see cref="CdsSideNav.IsExpanded"/>.</summary>
    Rail,
    /// <summary>Always 256 wide, at every breakpoint.</summary>
    Fixed,
}

/// <summary>Pure shell geometry and keyboard logic (widths, offsets, visibility by breakpoint, list movement).</summary>
internal static class UIShellModel
{
    public const double HeaderHeight = 48;
    public const double SideNavWidth = 256;
    public const double RailWidth = 48;
    public const double HeaderPanelWidth = 256;
    public const double HeaderMenuWidth = 200;
    public const double HeaderMenuItemHeight = 48;
    public const double SideNavRowHeight = 32;
    public const double SideNavLargeRowHeight = 48;

    /// <summary>True from the lg breakpoint (1056) up, where the side nav is persistent and header links show.</summary>
    public static bool IsLarge(CdsBreakpoint breakpoint) => breakpoint >= CdsBreakpoint.Lg;

    /// <summary>
    /// The width a side nav animates to. <paramref name="userCollapsed"/> is a persistent side nav at lg or wider that the user
    /// collapsed with a collapsible header menu button (it stays 0 wide until expanded again).
    /// </summary>
    public static double SideNavTargetWidth(CdsSideNavMode mode, CdsBreakpoint breakpoint, bool isExpanded, bool isPersistent, bool transientExpanded, bool userCollapsed = false)
    {
        switch (mode)
        {
            case CdsSideNavMode.Fixed:
                return SideNavWidth;
            case CdsSideNavMode.Rail:
                return isExpanded || transientExpanded ? SideNavWidth : RailWidth;
            default:
                if (isExpanded) return SideNavWidth;
                return isPersistent && IsLarge(breakpoint) && !userCollapsed ? SideNavWidth : 0;
        }
    }

    /// <summary>
    /// Whether a side nav of this target width shows expanded (256, also while a rail is expanded by hover or focus, or a
    /// persistent side nav sits open at lg): what the footer toggle and the header menu button report.
    /// </summary>
    public static bool SideNavOpen(double targetWidth) => targetWidth > RailWidth + 0.5;

    /// <summary>
    /// Whether the side-nav footer toggle can change anything: a rail (48 to 256 and back), or a default side nav that is not
    /// held open by persistence at lg. A fixed side nav, and a persistent one at lg, never collapse from the footer (the
    /// collapsed nav would hide the footer itself and leave no way back but the header menu button).
    /// </summary>
    public static bool SideNavFooterToggles(CdsSideNavMode mode, CdsBreakpoint breakpoint, bool isPersistent) => mode switch
    {
        CdsSideNavMode.Rail => true,
        CdsSideNavMode.Fixed => false,
        _ => !(isPersistent && IsLarge(breakpoint)),
    };

    /// <summary>
    /// How far the page body is pushed from the start edge for a side nav that is currently <paramref name="currentWidth"/> wide.
    /// A rail always reserves 48 (hover and focus expansion floats over the body); the default side nav pushes the body
    /// only from lg up and floats over it below.
    /// </summary>
    public static double BodyOffset(CdsSideNavMode mode, CdsBreakpoint breakpoint, double currentWidth) => mode switch
    {
        CdsSideNavMode.Rail => RailWidth,
        CdsSideNavMode.Fixed => currentWidth,
        _ => IsLarge(breakpoint) ? currentWidth : 0,
    };

    /// <summary>Whether the full-screen scrim shows behind the side nav (below lg, default side nav, expanded).</summary>
    public static bool ShowScrim(CdsSideNavMode mode, CdsBreakpoint breakpoint, bool isExpanded) =>
        mode == CdsSideNavMode.Default && !IsLarge(breakpoint) && isExpanded;

    /// <summary>The hamburger shows below lg, and from lg up only when collapsible.</summary>
    public static bool MenuButtonVisible(CdsBreakpoint breakpoint, bool isCollapsible) => !IsLarge(breakpoint) || isCollapsible;

    /// <summary>Header navigation shows from lg up; below it the links move into <c>HeaderSideNavItems</c>.</summary>
    public static bool HeaderNavigationVisible(CdsBreakpoint breakpoint) => IsLarge(breakpoint);

    /// <summary>The side nav links are reachable (focusable) when the side nav is not collapsed to nothing.</summary>
    public static bool SideNavReachable(double currentWidth) => currentWidth > 0.5;

    /// <summary>
    /// The next enabled index in <paramref name="enabled"/> starting after <paramref name="current"/> in the direction
    /// (+1 or -1), wrapping at the ends. Returns -1 when nothing is enabled. A <paramref name="current"/> of -1 starts at the edge.
    /// </summary>
    public static int NextEnabled(IReadOnlyList<bool> enabled, int current, int direction)
    {
        var n = enabled.Count;
        if (n == 0) return -1;
        var step = direction >= 0 ? 1 : -1;
        var i = current;
        if (i < 0 || i >= n) i = step > 0 ? -1 : n;
        for (var tries = 0; tries < n; tries++)
        {
            i = ((i + step) % n + n) % n;
            if (enabled[i]) return i;
        }
        return -1;
    }

    /// <summary>The first enabled index, or -1.</summary>
    public static int FirstEnabled(IReadOnlyList<bool> enabled)
    {
        for (var i = 0; i < enabled.Count; i++) if (enabled[i]) return i;
        return -1;
    }

    /// <summary>The last enabled index, or -1.</summary>
    public static int LastEnabled(IReadOnlyList<bool> enabled)
    {
        for (var i = enabled.Count - 1; i >= 0; i--) if (enabled[i]) return i;
        return -1;
    }

    /// <summary>Index of a keyboard move in a menu: ArrowUp/ArrowDown wrap, Home/End jump. Returns the new index or -1 for no change.</summary>
    public static int Move(IReadOnlyList<bool> enabled, int current, CdsKey key) => key switch
    {
        CdsKey.Down => NextEnabled(enabled, current, +1),
        CdsKey.Up => NextEnabled(enabled, current, -1),
        CdsKey.Home => FirstEnabled(enabled),
        CdsKey.End => LastEnabled(enabled),
        _ => -1,
    };

    /// <summary>
    /// Whether an open header panel closes after focus left one of its elements: blur closing is on, nothing inside the panel
    /// has focus, focus is not on the action that opened it (<see cref="CdsHeaderPanel.ReturnFocusTo"/>, which toggles the
    /// panel itself) and focus went somewhere (focus that merely left the window keeps the panel open).
    /// </summary>
    public static bool HeaderPanelClosesOnBlur(bool closesOnBlur, bool isExpanded, bool focusInside, bool focusOnOpener, bool focusElsewhere) =>
        closesOnBlur && isExpanded && !focusInside && !focusOnOpener && focusElsewhere;

    /// <summary>Whether a header panel closes on a tap outside it: explicit setting, else only when it holds a switcher.</summary>
    public static bool ClosesOnOutsideTap(bool? explicitValue, bool holdsSwitcher) => explicitValue ?? holdsSwitcher;
}
